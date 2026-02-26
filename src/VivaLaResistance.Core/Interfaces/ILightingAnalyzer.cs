namespace VivaLaResistance.Core.Interfaces;

public enum LightingQuality
{
    Good,
    TooDark,
    TooBright,
    Unknown
}

/// <summary>
/// Analyzes a camera frame to assess lighting quality for color band detection.
/// </summary>
public interface ILightingAnalyzer
{
    /// <summary>
    /// Assesses lighting quality from a BGRA8888 frame.
    /// </summary>
    LightingQuality Analyze(byte[] frameData, int width, int height);
}
