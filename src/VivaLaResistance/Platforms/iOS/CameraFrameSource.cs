#nullable enable

using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using System.Runtime.InteropServices;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Platforms.iOS;

/// <summary>
/// iOS AVFoundation implementation of <see cref="IFrameSource"/>.
/// Delivers BGRA8888 frames at 15 FPS using <see cref="AVCaptureSession"/> with
/// <c>kCVPixelFormatType_32BGRA</c> — no per-frame pixel conversion required.
/// Caller is responsible for requesting camera usage permission (<c>NSCameraUsageDescription</c>)
/// before calling <see cref="StartAsync"/>.
/// </summary>
public sealed class CameraFrameSource : IFrameSource, IDisposable
{
    // Hardware frame rate: 15 FPS max, 10 FPS min.
    private const int MaxFps = 15;
    private const int MinFps = 10;

    private AVCaptureSession? _session;
    private AVCaptureVideoDataOutput? _videoOutput;
    private SampleBufferDelegate? _delegate;
    private DispatchQueue? _captureQueue;
    private bool _isRunning;
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<CameraFrame>? FrameAvailable;

    /// <inheritdoc />
    public bool IsRunning => _isRunning;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No back camera or session configuration failure.</exception>
    public Task StartAsync()
    {
        if (_isRunning)
            return Task.CompletedTask;

        _captureQueue = new DispatchQueue("com.vivalaresistance.camera", false);
        _session = new AVCaptureSession { SessionPreset = AVCaptureSession.PresetMedium };

        // Back camera input
        var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video)
            ?? throw new InvalidOperationException("No video capture device available on this device.");

        var input = new AVCaptureDeviceInput(device, out var inputError);
        if (inputError is not null)
            throw new InvalidOperationException($"Camera input error: {inputError.LocalizedDescription}");

        if (_session.CanAddInput(input))
            _session.AddInput(input);

        // Configure hardware frame rate (best-effort; ignore if unsupported format)
        if (device.LockForConfiguration(out _))
        {
            var targetFps = new CMTime(1, MaxFps);
            var minFps = new CMTime(1, MinFps);
            device.ActiveVideoMinFrameDuration = targetFps;
            device.ActiveVideoMaxFrameDuration = minFps;
            device.UnlockForConfiguration();
        }

        // Video output — request BGRA8888 directly; no YUV conversion needed
        _videoOutput = new AVCaptureVideoDataOutput
        {
            AlwaysDiscardsLateVideoFrames = true,
            WeakVideoSettings = NSDictionary.FromObjectAndKey(
                NSNumber.FromUInt32((uint)CVPixelFormatType.CV32BGRA),
                new NSString("PixelFormatType"))
        };

        _delegate = new SampleBufferDelegate(OnSampleBuffer);
        _videoOutput.SetSampleBufferDelegateQueue(_delegate, _captureQueue);

        if (_session.CanAddOutput(_videoOutput))
            _session.AddOutput(_videoOutput);

        _session.StartRunning();
        _isRunning = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        _isRunning = false;
        _session?.StopRunning();
        _session = null;
        _videoOutput = null;
        _delegate?.Dispose();
        _delegate = null;
        return Task.CompletedTask;
    }

    private void OnSampleBuffer(CMSampleBuffer sampleBuffer)
    {
        using var imageBuffer = sampleBuffer.GetImageBuffer();
        if (imageBuffer is not CVPixelBuffer pixelBuffer)
            return;

        var bgra = ExtractBgra(pixelBuffer);
        var frame = new CameraFrame(
            bgra,
            (int)pixelBuffer.Width,
            (int)pixelBuffer.Height,
            DateTime.UtcNow);

        FrameAvailable?.Invoke(this, frame);
    }

    /// <summary>
    /// Copies BGRA8888 pixel data from a locked <see cref="CVPixelBuffer"/> into a new byte array.
    /// Handles row padding (bytesPerRow ≥ width × 4).
    /// </summary>
    private static byte[] ExtractBgra(CVPixelBuffer pixelBuffer)
    {
        pixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
        try
        {
            int width = (int)pixelBuffer.Width;
            int height = (int)pixelBuffer.Height;
            int bytesPerRow = (int)pixelBuffer.BytesPerRow;
            nint baseAddress = pixelBuffer.BaseAddress;

            var result = new byte[width * height * 4];

            // Copy row-by-row to strip any row-stride padding.
            for (int row = 0; row < height; row++)
            {
                nint srcOffset = baseAddress + row * bytesPerRow;
                Marshal.Copy(srcOffset, result, row * width * 4, width * 4);
            }

            return result;
        }
        finally
        {
            pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        StopAsync().GetAwaiter().GetResult();
    }

    // ── Inner helper type ─────────────────────────────────────────────────────

    /// <summary>
    /// Bridges <see cref="AVCaptureVideoDataOutputSampleBufferDelegate"/> callbacks into a managed delegate.
    /// </summary>
    private sealed class SampleBufferDelegate : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private readonly Action<CMSampleBuffer> _onSampleBuffer;

        public SampleBufferDelegate(Action<CMSampleBuffer> onSampleBuffer) =>
            _onSampleBuffer = onSampleBuffer;

        public override void DidOutputSampleBuffer(
            AVCaptureOutput captureOutput,
            CMSampleBuffer sampleBuffer,
            AVCaptureConnection connection)
        {
            // sampleBuffer lifetime is managed by AVFoundation; process synchronously.
            _onSampleBuffer(sampleBuffer);
        }
    }
}
