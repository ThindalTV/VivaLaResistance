namespace VivaLaResistance.Core.Models;

/// <summary>
/// Bounding box for a detected resistor body, in normalized coordinates [0,1].
/// </summary>
/// <param name="X">Left edge, normalized.</param>
/// <param name="Y">Top edge, normalized.</param>
/// <param name="Width">Box width, normalized.</param>
/// <param name="Height">Box height, normalized.</param>
/// <param name="Confidence">Detection confidence score [0,1].</param>
public record ResistorBoundingBox(float X, float Y, float Width, float Height, float Confidence);
