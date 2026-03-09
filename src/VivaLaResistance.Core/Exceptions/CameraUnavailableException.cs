namespace VivaLaResistance.Core.Exceptions;

/// <summary>
/// Thrown when the camera device is unavailable or cannot be opened.
/// </summary>
public class CameraUnavailableException : Exception
{
    public CameraUnavailableException()
        : base("Camera is currently unavailable. Please ensure no other app is using the camera.")
    {
    }

    public CameraUnavailableException(string message)
        : base(message)
    {
    }

    public CameraUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
