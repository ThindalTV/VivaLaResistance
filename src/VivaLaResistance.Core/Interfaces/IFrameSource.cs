using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Core.Interfaces;

/// <summary>
/// Provides live camera frames for vision processing.
/// All frames are delivered in BGRA8888 format.
/// </summary>
public interface IFrameSource
{
    /// <summary>
    /// Starts the camera and begins emitting frames.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the camera and cleans up resources.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Raised when a new frame is available.
    /// Frame data is BGRA8888 format.
    /// </summary>
    event EventHandler<CameraFrame>? FrameAvailable;

    bool IsRunning { get; }
}
