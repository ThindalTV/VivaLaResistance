namespace VivaLaResistance.Core.Exceptions;

/// <summary>
/// Thrown when camera permissions are denied or revoked.
/// </summary>
public class CameraPermissionException : Exception
{
    public CameraPermissionException()
        : base("Camera permission is required to detect resistors. Please grant camera access in your device settings.")
    {
    }

    public CameraPermissionException(string message)
        : base(message)
    {
    }

    public CameraPermissionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
