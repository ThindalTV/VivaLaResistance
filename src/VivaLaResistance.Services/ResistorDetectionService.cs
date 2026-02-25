namespace VivaLaResistance.Services;

using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

/// <summary>
/// Stub implementation of the resistor detection service.
/// The actual ML/vision implementation will be added later.
/// </summary>
public class ResistorDetectionService : IResistorDetectionService
{
    private bool _isInitialized;

    /// <inheritdoc />
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Initialize ML model here (e.g., load ONNX model, TensorFlow Lite, etc.)
        _isInitialized = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ResistorReading>> DetectResistorsAsync(
        byte[] imageData,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Detection service must be initialized before use. Call InitializeAsync first.");
        }

        // TODO: Implement actual ML/vision detection
        // This will be implemented by the ML/vision specialist (Bruce)
        throw new NotImplementedException("ML/vision detection not yet implemented.");
    }
}
