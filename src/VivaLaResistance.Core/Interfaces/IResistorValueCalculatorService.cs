namespace VivaLaResistance.Core.Interfaces;

using VivaLaResistance.Core.Models;

/// <summary>
/// Service for calculating resistor values from color bands.
/// </summary>
public interface IResistorValueCalculatorService
{
    /// <summary>
    /// Calculates the resistance value in ohms from the given color bands.
    /// </summary>
    /// <param name="bands">The color bands in order (supports 4, 5, or 6 band resistors).</param>
    /// <returns>The calculated resistance value in ohms.</returns>
    /// <exception cref="ArgumentException">Thrown when the band count is invalid.</exception>
    double CalculateResistance(IReadOnlyList<ColorBand> bands);

    /// <summary>
    /// Gets the tolerance percentage for the given tolerance band color.
    /// </summary>
    /// <param name="toleranceBand">The color of the tolerance band.</param>
    /// <returns>Tolerance as a percentage (e.g., 5.0 for ±5%).</returns>
    double GetTolerancePercent(ColorBand toleranceBand);

    /// <summary>
    /// Formats a resistance value in ohms to a human-readable string.
    /// </summary>
    /// <param name="valueInOhms">The resistance value in ohms.</param>
    /// <returns>Formatted string (e.g., "4.7kΩ", "1MΩ", "220Ω").</returns>
    string FormatResistance(double valueInOhms);

    /// <summary>
    /// Gets the multiplier value for a given color band.
    /// </summary>
    /// <param name="multiplierBand">The color of the multiplier band.</param>
    /// <returns>The multiplier value.</returns>
    double GetMultiplier(ColorBand multiplierBand);

    /// <summary>
    /// Gets the digit value (0-9) for a given color band.
    /// </summary>
    /// <param name="digitBand">The color of the digit band.</param>
    /// <returns>The digit value.</returns>
    /// <exception cref="ArgumentException">Thrown when the band is not a valid digit band.</exception>
    int GetDigitValue(ColorBand digitBand);
}
