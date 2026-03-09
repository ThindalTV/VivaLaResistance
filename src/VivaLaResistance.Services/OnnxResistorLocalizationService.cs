using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Services;

public class OnnxResistorLocalizationService : IResistorLocalizationService
{
    private readonly ILogger<OnnxResistorLocalizationService> _logger;
    private InferenceSession? _session;

    public OnnxResistorLocalizationService(ILogger<OnnxResistorLocalizationService> logger)
    {
        _logger = logger;
    }

    public bool IsInitialized => _session is not null;

    public Task InitializeAsync()
    {
        try
        {
            // TODO: Load model from MauiAsset when trained model is available
            // If model file not found, log error but don't throw - graceful degradation
            _logger.LogInformation("ONNX model initialization skipped - model not yet available");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ONNX model - inference will return empty results");
            return Task.CompletedTask;
        }
    }

    public Task<IReadOnlyList<ResistorBoundingBox>> InferAsync(byte[] frameData, int width, int height)
    {
        if (_session is null)
        {
            // Model not loaded - graceful degradation
            return Task.FromResult<IReadOnlyList<ResistorBoundingBox>>(Array.Empty<ResistorBoundingBox>());
        }

        try
        {
            // TODO: Run YOLOv8 inference + NMS post-processing
            // When implemented, wrap InferenceSession.Run() in try-catch
            return Task.FromResult<IReadOnlyList<ResistorBoundingBox>>(Array.Empty<ResistorBoundingBox>());
        }
        catch (OnnxRuntimeException ex)
        {
            _logger.LogWarning(ex, "ONNX inference failed for frame {Width}x{Height} - returning empty results", width, height);
            return Task.FromResult<IReadOnlyList<ResistorBoundingBox>>(Array.Empty<ResistorBoundingBox>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during ONNX inference - returning empty results");
            return Task.FromResult<IReadOnlyList<ResistorBoundingBox>>(Array.Empty<ResistorBoundingBox>());
        }
    }
}
