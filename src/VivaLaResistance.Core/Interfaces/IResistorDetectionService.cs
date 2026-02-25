namespace VivaLaResistance.Core.Interfaces;

using VivaLaResistance.Core.Models;

/// <summary>
/// Service for detecting resistors in camera frames using ML/vision processing.
/// </summary>
public interface IResistorDetectionService
{
    /// <summary>
    /// Detects resistors in the provided image frame.
    /// </summary>
    /// <param name="imageData">Raw image data from the camera.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of detected resistor readings with their positions and color bands.</returns>
    Task<IReadOnlyList<ResistorReading>> DetectResistorsAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the ML model and any required resources.
    /// Should be called once at app startup.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates whether the detection service is ready to process frames.
    /// </summary>
    bool IsInitialized { get; }
}
