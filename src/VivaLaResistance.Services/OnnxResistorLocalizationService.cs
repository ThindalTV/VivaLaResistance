using Microsoft.ML.OnnxRuntime;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Services;

public class OnnxResistorLocalizationService : IResistorLocalizationService
{
    private InferenceSession? _session;

    public bool IsInitialized => _session is not null;

    public Task InitializeAsync()
    {
        // TODO: Load model from MauiAsset when trained model is available
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ResistorBoundingBox>> InferAsync(byte[] frameData, int width, int height)
    {
        // TODO: Run YOLOv8 inference + NMS post-processing
        return Task.FromResult<IReadOnlyList<ResistorBoundingBox>>(Array.Empty<ResistorBoundingBox>());
    }
}
