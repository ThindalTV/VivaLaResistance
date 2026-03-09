using AVFoundation;
using CoreMedia;
using CoreVideo;
using Foundation;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using VivaLaResistance.Core.Exceptions;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Platforms.iOS;

/// <summary>
/// iOS implementation of IFrameSource using AVFoundation.
/// Delivers frames in BGRA8888 format.
/// </summary>
public class CameraFrameSource : IFrameSource, IDisposable
{
    private readonly ILogger<CameraFrameSource> _logger;
    private AVCaptureSession? _captureSession;
    private AVCaptureVideoDataOutput? _videoOutput;
    private SampleBufferDelegate? _sampleBufferDelegate;
    private bool _isRunning;

    public event EventHandler<CameraFrame>? FrameAvailable;
    public event EventHandler<Exception>? ErrorOccurred;

    public bool IsRunning => _isRunning;

    public CameraFrameSource(ILogger<CameraFrameSource> logger)
    {
        _logger = logger;
    }

    public Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Starting camera capture");

            // Check camera authorization
            var authStatus = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
            if (authStatus == AVAuthorizationStatus.Denied || authStatus == AVAuthorizationStatus.Restricted)
            {
                throw new CameraPermissionException("Camera access denied. Please enable camera permissions in Settings.");
            }

            if (authStatus == AVAuthorizationStatus.NotDetermined)
            {
                throw new CameraPermissionException("Camera permissions not yet requested. Please grant camera access.");
            }

            // Get camera device
            var videoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (videoDevice == null)
            {
                throw new CameraUnavailableException("No camera device found on this device.");
            }

            // TODO: Create AVCaptureSession with BGRA8888 output
            // 1. Create capture device input
            // 2. Create video output with kCVPixelFormatType_32BGRA
            // 3. Set up session and start running

            _isRunning = true;
            _logger.LogInformation("Camera capture started successfully");
            return Task.CompletedTask;
        }
        catch (CameraPermissionException)
        {
            _logger.LogError("Camera permission denied");
            _isRunning = false;
            throw;
        }
        catch (CameraUnavailableException)
        {
            _logger.LogError("Camera device unavailable");
            _isRunning = false;
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start camera capture");
            _isRunning = false;
            ErrorOccurred?.Invoke(this, ex);
            throw;
        }
    }

    public Task StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping camera capture");

            if (_captureSession?.Running == true)
            {
                _captureSession.StopRunning();
            }

            _captureSession?.Dispose();
            _captureSession = null;

            _videoOutput?.Dispose();
            _videoOutput = null;

            _isRunning = false;
            _logger.LogInformation("Camera capture stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error occurred while stopping camera - cleaning up");
            _isRunning = false;
        }

        return Task.CompletedTask;
    }

    private void ProcessSampleBuffer(CMSampleBuffer sampleBuffer)
    {
        try
        {
            using var imageBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer;
            if (imageBuffer == null)
                return;

            // TODO: Extract BGRA8888 pixel data
            // TODO: Create CameraFrame and raise FrameAvailable event
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing camera frame - frame dropped");
            ErrorOccurred?.Invoke(this, ex);
        }
    }

    private class SampleBufferDelegate : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private readonly Action<CMSampleBuffer> _onSampleBuffer;

        public SampleBufferDelegate(Action<CMSampleBuffer> onSampleBuffer)
        {
            _onSampleBuffer = onSampleBuffer;
        }

        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            _onSampleBuffer(sampleBuffer);
            sampleBuffer?.Dispose();
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}
