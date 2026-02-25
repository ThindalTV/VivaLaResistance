using VivaLaResistance.Core.Interfaces;

namespace VivaLaResistance.Services;

/// <summary>
/// Analyzes average luminance of a sampled region to determine lighting quality.
/// </summary>
public class SkiaSharpLightingAnalyzer : ILightingAnalyzer
{
    private const float DarkThreshold = 0.2f;
    private const float BrightThreshold = 0.85f;

    public LightingQuality Analyze(byte[] frameData, int width, int height)
    {
        if (frameData is null || frameData.Length == 0) return LightingQuality.Unknown;

        // Sample center 1/4 of the frame (most likely to contain the resistor)
        int startX = width / 4, endX = 3 * width / 4;
        int startY = height / 4, endY = 3 * height / 4;

        double totalLuminance = 0;
        int sampleCount = 0;
        int stride = width * 4; // BGRA8888

        for (int y = startY; y < endY; y += 4) // sample every 4th row
        {
            for (int x = startX; x < endX; x += 4) // sample every 4th col
            {
                int idx = y * stride + x * 4;
                if (idx + 2 >= frameData.Length) continue;

                byte b = frameData[idx];
                byte g = frameData[idx + 1];
                byte r = frameData[idx + 2];

                // Perceived luminance (BT.601)
                totalLuminance += (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
                sampleCount++;
            }
        }

        if (sampleCount == 0) return LightingQuality.Unknown;

        float avgLuminance = (float)(totalLuminance / sampleCount);
        return avgLuminance < DarkThreshold ? LightingQuality.TooDark
             : avgLuminance > BrightThreshold ? LightingQuality.TooBright
             : LightingQuality.Good;
    }
}
