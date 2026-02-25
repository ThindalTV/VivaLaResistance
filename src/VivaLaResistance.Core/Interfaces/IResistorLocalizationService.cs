using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Core.Interfaces;

/// <summary>
/// Localizes resistor bodies in a camera frame using ONNX inference (YOLOv8-nano).
/// Input frames MUST be in BGRA8888 format.
/// </summary>
public interface IResistorLocalizationService
{
    /// <summary>
    /// Initializes the ONNX model. Must be called once before InferAsync.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Returns bounding boxes for all detected resistors in the frame.
    /// </summary>
    /// <param name="frameData">Raw BGRA8888 pixel data.</param>
    /// <param name="width">Frame width in pixels.</param>
    /// <param name="height">Frame height in pixels.</param>
    /// <returns>List of bounding boxes in normalized coordinates [0,1].</returns>
    Task<IReadOnlyList<ResistorBoundingBox>> InferAsync(byte[] frameData, int width, int height);

    bool IsInitialized { get; }
}
