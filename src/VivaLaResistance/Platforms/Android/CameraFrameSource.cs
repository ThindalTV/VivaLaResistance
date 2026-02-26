#nullable enable

using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;
using Android.OS;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Platforms.Android;

/// <summary>
/// Android Camera2 implementation of <see cref="IFrameSource"/>.
/// Delivers BGRA8888 frames at approximately 10–15 FPS for ML inference.
/// Caller is responsible for requesting <c>android.permission.CAMERA</c> before calling <see cref="StartAsync"/>.
/// </summary>
public sealed class CameraFrameSource : IFrameSource, IDisposable
{
    // Resolution fed to the ImageReader — matches ONNX model default input.
    private const int CaptureWidth = 640;
    private const int CaptureHeight = 480;

    // Software frame-rate cap: skip frames that arrive faster than ~15 FPS.
    private const long FrameIntervalMs = 1000 / 15;

    private CameraDevice? _cameraDevice;
    private CameraCaptureSession? _captureSession;
    private ImageReader? _imageReader;
    private HandlerThread? _backgroundThread;
    private Handler? _backgroundHandler;
    private long _lastFrameMs;
    private bool _isRunning;
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<CameraFrame>? FrameAvailable;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No active Android activity or no back camera.</exception>
    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        StartBackgroundThread();

        var activity = global::Android.App.Application.Context
            ?? throw new InvalidOperationException("Android application context not available.");

        var cameraManager = (CameraManager?)activity.GetSystemService(Context.CameraService)
            ?? throw new InvalidOperationException("CameraManager not available.");

        var cameraId = FindBackCameraId(cameraManager);

        _imageReader = ImageReader.NewInstance(CaptureWidth, CaptureHeight, global::Android.Graphics.ImageFormatType.Yuv420888, 2)!;
        _imageReader.SetOnImageAvailableListener(new ImageAvailableListener(OnImageAvailable), _backgroundHandler);

        var openTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stateCallback = new CameraStateCallback(
            onOpened: device =>
            {
                _cameraDevice = device;
                openTcs.TrySetResult(true);
            },
            onDisconnected: device =>
            {
                device.Close();
                _cameraDevice = null;
                openTcs.TrySetResult(false);
            },
            onError: (device, error) =>
            {
                device.Close();
                _cameraDevice = null;
                openTcs.TrySetException(new InvalidOperationException($"Camera open error: {error}"));
            });

        cameraManager.OpenCamera(cameraId, stateCallback, _backgroundHandler);
        var opened = await openTcs.Task.ConfigureAwait(false);

        if (!opened || _cameraDevice is null)
            return;

        await CreateCaptureSessionAsync().ConfigureAwait(false);
        _isRunning = true;
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        _isRunning = false;
        try
        {
            _captureSession?.Close();
            _cameraDevice?.Close();
            _imageReader?.Close();
        }
        finally
        {
            _captureSession = null;
            _cameraDevice = null;
            _imageReader = null;
            StopBackgroundThread();
        }
        return Task.CompletedTask;
    }

    private async Task CreateCaptureSessionAsync()
    {
        if (_cameraDevice is null || _imageReader is null)
            return;

        var surfaces = new List<global::Android.Views.Surface> { _imageReader.Surface! };
        var sessionTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sessionCallback = new CaptureSessionCallback(
            onConfigured: session =>
            {
                _captureSession = session;
                StartRepeatingCapture();
                sessionTcs.TrySetResult(true);
            },
            onConfigureFailed: _ =>
                sessionTcs.TrySetException(new InvalidOperationException("CaptureSession configuration failed.")));

#pragma warning disable CA1422 // CreateCaptureSession overload deprecated in API 30; simpler API acceptable for API 21+ support
        _cameraDevice.CreateCaptureSession(surfaces, sessionCallback, _backgroundHandler);
#pragma warning restore CA1422

        await sessionTcs.Task.ConfigureAwait(false);
    }

    private void StartRepeatingCapture()
    {
        if (_captureSession is null || _cameraDevice is null || _imageReader is null)
            return;

        var builder = _cameraDevice.CreateCaptureRequest(CameraTemplate.Preview)!;
        builder.AddTarget(_imageReader.Surface!);
        _captureSession.SetRepeatingRequest(builder.Build()!, null, _backgroundHandler);
    }

    private void OnImageAvailable(ImageReader reader)
    {
        // Software throttle — discard frames arriving faster than FrameIntervalMs.
        var nowMs = Java.Lang.JavaSystem.CurrentTimeMillis();
        if (nowMs - _lastFrameMs < FrameIntervalMs)
        {
            using var skipped = reader.AcquireLatestImage();
            skipped?.Close();
            return;
        }
        _lastFrameMs = nowMs;

        using var image = reader.AcquireLatestImage();
        if (image is null)
            return;

        try
        {
            var bgra = ConvertYuvToBgra(image);
            var frame = new CameraFrame(bgra, image.Width, image.Height, DateTime.UtcNow);
            FrameAvailable?.Invoke(this, frame);
        }
        finally
        {
            image.Close();
        }
    }

    private static string FindBackCameraId(CameraManager manager)
    {
        foreach (var id in manager.GetCameraIdList() ?? [])
        {
            var characteristics = manager.GetCameraCharacteristics(id)!;
            var facingObj = characteristics.Get(CameraCharacteristics.LensFacing);
            if (facingObj is null) continue;
            var facing = (int)facingObj;
            if (facing == (int)LensFacing.Back)
                return id;
        }
        throw new InvalidOperationException("No back-facing camera found on this device.");
    }

    /// <summary>
    /// Converts a YUV_420_888 <see cref="global::Android.Media.Image"/> to a packed BGRA8888 byte array using BT.601 coefficients.
    /// </summary>
    private static byte[] ConvertYuvToBgra(global::Android.Media.Image image)
    {
        int width = image.Width;
        int height = image.Height;

        var planes = image.GetPlanes()!;
        var yPlane = planes[0];
        var uPlane = planes[1]; // Cb
        var vPlane = planes[2]; // Cr

        int yRowStride = yPlane.RowStride;
        int yPixelStride = yPlane.PixelStride;
        int uRowStride = uPlane.RowStride;
        int uPixelStride = uPlane.PixelStride;
        int vRowStride = vPlane.RowStride;
        int vPixelStride = vPlane.PixelStride;

        var yBuf = yPlane.Buffer!;
        var uBuf = uPlane.Buffer!;
        var vBuf = vPlane.Buffer!;

        var yBytes = new byte[yBuf.Remaining()];
        var uBytes = new byte[uBuf.Remaining()];
        var vBytes = new byte[vBuf.Remaining()];
        yBuf.Get(yBytes);
        uBuf.Get(uBytes);
        vBuf.Get(vBytes);

        var bgra = new byte[width * height * 4];

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int yIdx = row * yRowStride + col * yPixelStride;
                int uvRow = row / 2;
                int uvCol = col / 2;
                int uIdx = uvRow * uRowStride + uvCol * uPixelStride;
                int vIdx = uvRow * vRowStride + uvCol * vPixelStride;

                int y = yBytes[yIdx] & 0xFF;
                int u = uBytes[uIdx] & 0xFF;
                int v = vBytes[vIdx] & 0xFF;

                // BT.601 full-range YCbCr → RGB
                int r = (int)Math.Clamp(y + 1.402 * (v - 128), 0, 255);
                int g = (int)Math.Clamp(y - 0.344136 * (u - 128) - 0.714136 * (v - 128), 0, 255);
                int b = (int)Math.Clamp(y + 1.772 * (u - 128), 0, 255);

                int bgraIdx = (row * width + col) * 4;
                bgra[bgraIdx] = (byte)b;
                bgra[bgraIdx + 1] = (byte)g;
                bgra[bgraIdx + 2] = (byte)r;
                bgra[bgraIdx + 3] = 0xFF;
            }
        }

        return bgra;
    }

    private void StartBackgroundThread()
    {
        _backgroundThread = new HandlerThread("VLR-CameraBackground");
        _backgroundThread.Start();
        _backgroundHandler = new Handler(_backgroundThread.Looper!);
    }

    private void StopBackgroundThread()
    {
        _backgroundThread?.QuitSafely();
        try { _backgroundThread?.Join(1000); } catch (Java.Lang.InterruptedException) { /* ignore */ }
        _backgroundThread = null;
        _backgroundHandler = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        StopAsync().GetAwaiter().GetResult();
    }

    // ── Inner helper types ────────────────────────────────────────────────────

    private sealed class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        private readonly Action<ImageReader> _callback;

        public ImageAvailableListener(Action<ImageReader> callback) => _callback = callback;

        public void OnImageAvailable(ImageReader? reader)
        {
            if (reader is not null)
                _callback(reader);
        }
    }

    private sealed class CameraStateCallback : CameraDevice.StateCallback
    {
        private readonly Action<CameraDevice> _onOpened;
        private readonly Action<CameraDevice> _onDisconnected;
        private readonly Action<CameraDevice, CameraError> _onError;

        public CameraStateCallback(
            Action<CameraDevice> onOpened,
            Action<CameraDevice> onDisconnected,
            Action<CameraDevice, CameraError> onError)
        {
            _onOpened = onOpened;
            _onDisconnected = onDisconnected;
            _onError = onError;
        }

        public override void OnOpened(CameraDevice camera) => _onOpened(camera);
        public override void OnDisconnected(CameraDevice camera) => _onDisconnected(camera);
        public override void OnError(CameraDevice camera, [global::Android.Runtime.GeneratedEnum] CameraError error) => _onError(camera, error);
    }

    private sealed class CaptureSessionCallback : CameraCaptureSession.StateCallback
    {
        private readonly Action<CameraCaptureSession> _onConfigured;
        private readonly Action<CameraCaptureSession> _onConfigureFailed;

        public CaptureSessionCallback(
            Action<CameraCaptureSession> onConfigured,
            Action<CameraCaptureSession> onConfigureFailed)
        {
            _onConfigured = onConfigured;
            _onConfigureFailed = onConfigureFailed;
        }

        public override void OnConfigured(CameraCaptureSession session) => _onConfigured(session);
        public override void OnConfigureFailed(CameraCaptureSession session) => _onConfigureFailed(session);
    }
}
