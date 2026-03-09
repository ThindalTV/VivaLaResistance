namespace VivaLaResistance.Tests;

using VivaLaResistance.Core.Models;
using VivaLaResistance.Services;
using Xunit;

/// <summary>
/// Edge case tests for ResistorValueCalculatorService covering fallback behaviour,
/// boundary formatting, invalid digit/multiplier bands, and high-value multipliers.
/// Complements ResistorValueCalculatorServiceTests.cs without duplicating its coverage.
/// </summary>
public class ResistorValueCalculatorEdgeCaseTests
{
    private readonly ResistorValueCalculatorService _calculator = new();

    #region GetTolerancePercent — fallback for unlisted bands

    // Black, Orange, Yellow, White are valid colour bands but are NOT in the Tolerances
    // dictionary, so the service must fall back to 20.0 (the ±20% default).
    [Theory]
    [InlineData(ColorBand.Black)]
    [InlineData(ColorBand.Orange)]
    [InlineData(ColorBand.Yellow)]
    [InlineData(ColorBand.White)]
    public void GetTolerancePercent_UnlistedBand_ReturnsTwentyPercent(ColorBand band)
    {
        var result = _calculator.GetTolerancePercent(band);
        Assert.Equal(20.0, result);
    }

    #endregion

    #region GetDigitValue — out-of-range enum casts

    [Fact]
    public void GetDigitValue_NegativeEnumCast_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.GetDigitValue((ColorBand)(-1)));
    }

    [Fact]
    public void GetDigitValue_LargeEnumCast_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.GetDigitValue((ColorBand)(100)));
    }

    #endregion

    #region GetMultiplier — out-of-range enum cast

    [Fact]
    public void GetMultiplier_NegativeEnumCast_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.GetMultiplier((ColorBand)(-1)));
    }

    #endregion

    #region CalculateResistance — invalid first digit bands

    [Theory]
    [InlineData(ColorBand.Gold)]
    [InlineData(ColorBand.Silver)]
    [InlineData(ColorBand.None)]
    public void CalculateResistance_InvalidFirstDigitBand_ThrowsArgumentException(ColorBand invalidBand)
    {
        // 4-band resistor where the first (digit) band is not a valid digit band.
        var bands = new[] { invalidBand, ColorBand.Black, ColorBand.Brown, ColorBand.Gold };
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(bands));
    }

    #endregion

    #region CalculateResistance — too-few bands (0–3)

    [Fact]
    public void CalculateResistance_EmptyArray_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(new ColorBand[0]));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateResistance_BelowMinimumBands_ThrowsArgumentException(int bandCount)
    {
        var bands = new ColorBand[bandCount];
        Array.Fill(bands, ColorBand.Brown);
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(bands));
    }

    #endregion

    #region CalculateResistance — high-value multiplier bands

    [Fact]
    public void CalculateResistance_VioletMultiplier_Returns1GOhm()
    {
        // Brown, Black, Black, Violet, Brown (5-band):
        //   digits = 100, multiplier = 10 MΩ → 1,000,000,000 Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Violet, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(1_000_000_000, result);
    }

    [Fact]
    public void CalculateResistance_GreyMultiplier_Returns10GOhm()
    {
        // Brown, Black, Black, Grey, Brown (5-band):
        //   digits = 100, multiplier = 100 MΩ → 10,000,000,000 Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Grey, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(10_000_000_000d, result);
    }

    [Fact]
    public void CalculateResistance_WhiteMultiplier_Returns100GOhm()
    {
        // Brown, Black, Black, White, Brown (5-band):
        //   digits = 100, multiplier = 1 GΩ → 100,000,000,000 Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.White, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(100_000_000_000d, result);
    }

    #endregion

    #region FormatResistance — zero, negative, and precision edge cases

    [Fact]
    public void FormatResistance_Zero_ReturnsZeroOhm()
    {
        Assert.Equal("0Ω", _calculator.FormatResistance(0));
    }

    [Fact]
    public void FormatResistance_NegativeValue_FormatsWithOhmUnit()
    {
        // Negative ohms are not physically meaningful but the formatter must not crash.
        Assert.Equal("-100Ω", _calculator.FormatResistance(-100));
    }

    [Theory]
    [InlineData(1234, "1.234kΩ")]
    [InlineData(10000, "10kΩ")]      // G4 must not emit "10.00kΩ"
    [InlineData(10_000_000_000d, "10GΩ")]
    [InlineData(100_000_000_000d, "100GΩ")]
    public void FormatResistance_VariousEdgeValues_FormatsCorrectly(double value, string expected)
    {
        Assert.Equal(expected, _calculator.FormatResistance(value));
    }

    #endregion
}
