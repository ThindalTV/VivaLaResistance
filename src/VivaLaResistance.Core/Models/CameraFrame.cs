namespace VivaLaResistance.Core.Models;

/// <summary>
/// A single captured camera frame in BGRA8888 format.
/// </summary>
/// <param name="Data">Raw pixel data, BGRA8888.</param>
/// <param name="Width">Frame width in pixels.</param>
/// <param name="Height">Frame height in pixels.</param>
/// <param name="Timestamp">Capture time (UTC).</param>
public record CameraFrame(byte[] Data, int Width, int Height, DateTime Timestamp);
