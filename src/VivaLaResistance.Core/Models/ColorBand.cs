namespace VivaLaResistance.Core.Models;

/// <summary>
/// Represents the color bands found on a resistor.
/// Standard resistors have 4, 5, or 6 bands representing digit values, multiplier, and tolerance.
/// </summary>
public enum ColorBand
{
    /// <summary>Black - Value: 0, Multiplier: 1Ω</summary>
    Black = 0,

    /// <summary>Brown - Value: 1, Multiplier: 10Ω, Tolerance: ±1%</summary>
    Brown = 1,

    /// <summary>Red - Value: 2, Multiplier: 100Ω, Tolerance: ±2%</summary>
    Red = 2,

    /// <summary>Orange - Value: 3, Multiplier: 1kΩ</summary>
    Orange = 3,

    /// <summary>Yellow - Value: 4, Multiplier: 10kΩ</summary>
    Yellow = 4,

    /// <summary>Green - Value: 5, Multiplier: 100kΩ, Tolerance: ±0.5%</summary>
    Green = 5,

    /// <summary>Blue - Value: 6, Multiplier: 1MΩ, Tolerance: ±0.25%</summary>
    Blue = 6,

    /// <summary>Violet - Value: 7, Multiplier: 10MΩ, Tolerance: ±0.1%</summary>
    Violet = 7,

    /// <summary>Grey - Value: 8, Multiplier: 100MΩ, Tolerance: ±0.05%</summary>
    Grey = 8,

    /// <summary>White - Value: 9, Multiplier: 1GΩ</summary>
    White = 9,

    /// <summary>Gold - Multiplier: 0.1Ω, Tolerance: ±5%</summary>
    Gold = 10,

    /// <summary>Silver - Multiplier: 0.01Ω, Tolerance: ±10%</summary>
    Silver = 11,

    /// <summary>None - Tolerance: ±20%</summary>
    None = 12
}
