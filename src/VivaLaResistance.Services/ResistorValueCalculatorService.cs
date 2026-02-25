namespace VivaLaResistance.Services;

using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;

/// <summary>
/// Service for calculating resistor values from color bands.
/// Supports 4, 5, and 6 band resistors.
/// </summary>
public class ResistorValueCalculatorService : IResistorValueCalculatorService
{
    private static readonly Dictionary<ColorBand, double> Multipliers = new()
    {
        { ColorBand.Black, 1 },
        { ColorBand.Brown, 10 },
        { ColorBand.Red, 100 },
        { ColorBand.Orange, 1_000 },
        { ColorBand.Yellow, 10_000 },
        { ColorBand.Green, 100_000 },
        { ColorBand.Blue, 1_000_000 },
        { ColorBand.Violet, 10_000_000 },
        { ColorBand.Grey, 100_000_000 },
        { ColorBand.White, 1_000_000_000 },
        { ColorBand.Gold, 0.1 },
        { ColorBand.Silver, 0.01 }
    };

    private static readonly Dictionary<ColorBand, double> Tolerances = new()
    {
        { ColorBand.Brown, 1.0 },
        { ColorBand.Red, 2.0 },
        { ColorBand.Green, 0.5 },
        { ColorBand.Blue, 0.25 },
        { ColorBand.Violet, 0.1 },
        { ColorBand.Grey, 0.05 },
        { ColorBand.Gold, 5.0 },
        { ColorBand.Silver, 10.0 },
        { ColorBand.None, 20.0 }
    };

    /// <inheritdoc />
    public double CalculateResistance(IReadOnlyList<ColorBand> bands)
    {
        if (bands == null || bands.Count < 4 || bands.Count > 6)
        {
            throw new ArgumentException("Resistors must have 4, 5, or 6 color bands.", nameof(bands));
        }

        // 4-band: digit1, digit2, multiplier, tolerance
        // 5-band: digit1, digit2, digit3, multiplier, tolerance
        // 6-band: digit1, digit2, digit3, multiplier, tolerance, temperature coefficient

        int digitCount = bands.Count >= 5 ? 3 : 2;
        int multiplierIndex = digitCount;

        // Calculate base value from digit bands
        double baseValue = 0;
        for (int i = 0; i < digitCount; i++)
        {
            int digit = GetDigitValue(bands[i]);
            baseValue = (baseValue * 10) + digit;
        }

        // Apply multiplier
        double multiplier = GetMultiplier(bands[multiplierIndex]);
        return baseValue * multiplier;
    }

    /// <inheritdoc />
    public double GetTolerancePercent(ColorBand toleranceBand)
    {
        return Tolerances.TryGetValue(toleranceBand, out var tolerance) ? tolerance : 20.0;
    }

    /// <inheritdoc />
    public string FormatResistance(double valueInOhms)
    {
        return valueInOhms switch
        {
            >= 1_000_000_000 => $"{valueInOhms / 1_000_000_000:G4}GΩ",
            >= 1_000_000 => $"{valueInOhms / 1_000_000:G4}MΩ",
            >= 1_000 => $"{valueInOhms / 1_000:G4}kΩ",
            _ => $"{valueInOhms:G4}Ω"
        };
    }

    /// <inheritdoc />
    public double GetMultiplier(ColorBand multiplierBand)
    {
        if (Multipliers.TryGetValue(multiplierBand, out var multiplier))
        {
            return multiplier;
        }

        throw new ArgumentException($"Color band '{multiplierBand}' is not a valid multiplier band.", nameof(multiplierBand));
    }

    /// <inheritdoc />
    public int GetDigitValue(ColorBand digitBand)
    {
        int value = (int)digitBand;
        if (value >= 0 && value <= 9)
        {
            return value;
        }

        throw new ArgumentException($"Color band '{digitBand}' is not a valid digit band (must be Black through White).", nameof(digitBand));
    }
}
