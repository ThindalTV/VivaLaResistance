namespace VivaLaResistance.Core.Models;

/// <summary>
/// Represents a detected resistor in the camera frame with its calculated value and position.
/// Immutable record type for thread-safe detection pipeline output.
/// </summary>
/// <param name="Id">Unique identifier for this reading within a frame.</param>
/// <param name="ValueInOhms">The calculated resistance value in ohms.</param>
/// <param name="FormattedValue">Human-readable formatted value (e.g., "4.7kΩ", "1MΩ").</param>
/// <param name="TolerancePercent">Tolerance percentage (e.g., 5.0 for ±5%).</param>
/// <param name="BandCount">Number of color bands on this resistor (4, 5, or 6).</param>
/// <param name="ColorBands">The color bands detected on this resistor, in order from first to last.</param>
/// <param name="BoundingBox">Bounding box of the detected resistor in normalized coordinates.</param>
/// <param name="Confidence">Detection confidence score (0.0 to 1.0).</param>
/// <param name="Timestamp">Timestamp when this reading was captured.</param>
public record ResistorReading(
    Guid Id,
    double ValueInOhms,
    string FormattedValue,
    double TolerancePercent,
    int BandCount,
    IReadOnlyList<ColorBand> ColorBands,
    ResistorBoundingBox BoundingBox,
    double Confidence,
    DateTimeOffset Timestamp);
