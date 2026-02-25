namespace VivaLaResistance.Core.Models;

/// <summary>
/// Represents a detected resistor in the camera frame with its calculated value and position.
/// </summary>
public class ResistorReading
{
    /// <summary>
    /// Unique identifier for this reading within a frame.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The calculated resistance value in ohms.
    /// </summary>
    public double ValueInOhms { get; set; }

    /// <summary>
    /// Human-readable formatted value (e.g., "4.7kΩ", "1MΩ").
    /// </summary>
    public string FormattedValue { get; set; } = string.Empty;

    /// <summary>
    /// The color bands detected on this resistor, in order from first to last.
    /// </summary>
    public IReadOnlyList<ColorBand> ColorBands { get; set; } = [];

    /// <summary>
    /// Tolerance percentage (e.g., 5.0 for ±5%).
    /// </summary>
    public double TolerancePercent { get; set; }

    /// <summary>
    /// Bounding box of the detected resistor in normalized coordinates (0.0 to 1.0).
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new();

    /// <summary>
    /// Confidence score of the detection (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Timestamp when this reading was captured.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a bounding box in normalized coordinates (0.0 to 1.0).
/// </summary>
public class BoundingBox
{
    /// <summary>X coordinate of the top-left corner (0.0 to 1.0).</summary>
    public double X { get; set; }

    /// <summary>Y coordinate of the top-left corner (0.0 to 1.0).</summary>
    public double Y { get; set; }

    /// <summary>Width of the bounding box (0.0 to 1.0).</summary>
    public double Width { get; set; }

    /// <summary>Height of the bounding box (0.0 to 1.0).</summary>
    public double Height { get; set; }

    /// <summary>Center X coordinate.</summary>
    public double CenterX => X + (Width / 2);

    /// <summary>Center Y coordinate.</summary>
    public double CenterY => Y + (Height / 2);
}
