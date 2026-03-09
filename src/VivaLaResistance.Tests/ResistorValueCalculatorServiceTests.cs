namespace VivaLaResistance.Tests;

using VivaLaResistance.Core.Models;
using VivaLaResistance.Services;
using Xunit;

/// <summary>
/// Unit tests for the ResistorValueCalculatorService.
/// </summary>
public class ResistorValueCalculatorServiceTests
{
    private readonly ResistorValueCalculatorService _calculator = new();

    #region GetDigitValue Tests

    [Theory]
    [InlineData(ColorBand.Black, 0)]
    [InlineData(ColorBand.Brown, 1)]
    [InlineData(ColorBand.Red, 2)]
    [InlineData(ColorBand.Orange, 3)]
    [InlineData(ColorBand.Yellow, 4)]
    [InlineData(ColorBand.Green, 5)]
    [InlineData(ColorBand.Blue, 6)]
    [InlineData(ColorBand.Violet, 7)]
    [InlineData(ColorBand.Grey, 8)]
    [InlineData(ColorBand.White, 9)]
    public void GetDigitValue_ValidDigitBand_ReturnsCorrectValue(ColorBand band, int expected)
    {
        var result = _calculator.GetDigitValue(band);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ColorBand.Gold)]
    [InlineData(ColorBand.Silver)]
    [InlineData(ColorBand.None)]
    public void GetDigitValue_InvalidDigitBand_ThrowsArgumentException(ColorBand band)
    {
        Assert.Throws<ArgumentException>(() => _calculator.GetDigitValue(band));
    }

    #endregion

    #region GetMultiplier Tests

    [Theory]
    [InlineData(ColorBand.Black, 1)]
    [InlineData(ColorBand.Brown, 10)]
    [InlineData(ColorBand.Red, 100)]
    [InlineData(ColorBand.Orange, 1_000)]
    [InlineData(ColorBand.Yellow, 10_000)]
    [InlineData(ColorBand.Green, 100_000)]
    [InlineData(ColorBand.Blue, 1_000_000)]
    [InlineData(ColorBand.Violet, 10_000_000)]
    [InlineData(ColorBand.Grey, 100_000_000)]
    [InlineData(ColorBand.White, 1_000_000_000)]
    [InlineData(ColorBand.Gold, 0.1)]
    [InlineData(ColorBand.Silver, 0.01)]
    public void GetMultiplier_ValidBand_ReturnsCorrectValue(ColorBand band, double expected)
    {
        var result = _calculator.GetMultiplier(band);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetMultiplier_InvalidBand_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.GetMultiplier(ColorBand.None));
    }

    #endregion

    #region GetTolerancePercent Tests

    [Theory]
    [InlineData(ColorBand.Brown, 1.0)]
    [InlineData(ColorBand.Red, 2.0)]
    [InlineData(ColorBand.Green, 0.5)]
    [InlineData(ColorBand.Blue, 0.25)]
    [InlineData(ColorBand.Violet, 0.1)]
    [InlineData(ColorBand.Grey, 0.05)]
    [InlineData(ColorBand.Gold, 5.0)]
    [InlineData(ColorBand.Silver, 10.0)]
    [InlineData(ColorBand.None, 20.0)]
    public void GetTolerancePercent_ValidBand_ReturnsCorrectValue(ColorBand band, double expected)
    {
        var result = _calculator.GetTolerancePercent(band);
        Assert.Equal(expected, result);
    }

    #endregion

    #region CalculateResistance Tests - 4 Band Resistors

    [Fact]
    public void CalculateResistance_4Band_470Ohm_ReturnsCorrectValue()
    {
        // Yellow, Violet, Brown, Gold = 47 * 10 = 470Ω
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Brown, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(470, result);
    }

    [Fact]
    public void CalculateResistance_4Band_10kOhm_ReturnsCorrectValue()
    {
        // Brown, Black, Orange, Gold = 10 * 1000 = 10,000Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Orange, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(10_000, result);
    }

    [Fact]
    public void CalculateResistance_4Band_4_7kOhm_ReturnsCorrectValue()
    {
        // Yellow, Violet, Red, Gold = 47 * 100 = 4,700Ω
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Red, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(4_700, result);
    }

    [Fact]
    public void CalculateResistance_4Band_1MOhm_ReturnsCorrectValue()
    {
        // Brown, Black, Green, Gold = 10 * 100,000 = 1,000,000Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Green, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(1_000_000, result);
    }

    #endregion

    #region CalculateResistance Tests - 5 Band Resistors

    [Fact]
    public void CalculateResistance_5Band_4_7kOhm_ReturnsCorrectValue()
    {
        // Yellow, Violet, Black, Brown, Brown = 470 * 10 = 4,700Ω
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Black, ColorBand.Brown, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(4_700, result);
    }

    [Fact]
    public void CalculateResistance_5Band_1kOhm_ReturnsCorrectValue()
    {
        // Brown, Black, Black, Brown, Brown = 100 * 10 = 1,000Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Brown, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(1_000, result);
    }

    [Fact]
    public void CalculateResistance_5Band_33_2kOhm_RedTolerance_ReturnsCorrectValue()
    {
        // Orange, Orange, Red, Red, Red = 332 * 100 = 33,200Ω (±2%)
        var bands = new[] { ColorBand.Orange, ColorBand.Orange, ColorBand.Red, ColorBand.Red, ColorBand.Red };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(33_200, result);
    }

    [Fact]
    public void CalculateResistance_5Band_4_75kOhm_GreenTolerance_ReturnsCorrectValue()
    {
        // Yellow, Violet, Green, Brown, Green = 475 * 10 = 4,750Ω (±0.5%)
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Green, ColorBand.Brown, ColorBand.Green };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(4_750, result);
    }

    [Fact]
    public void CalculateResistance_5Band_100Ohm_BlueTolerance_ReturnsCorrectValue()
    {
        // Brown, Black, Black, Black, Blue = 100 * 1 = 100Ω (±0.25%)
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Black, ColorBand.Blue };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateResistance_5Band_47kOhm_VioletTolerance_ReturnsCorrectValue()
    {
        // Yellow, Violet, Black, Red, Violet = 470 * 100 = 47,000Ω (±0.1%)
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Black, ColorBand.Red, ColorBand.Violet };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(47_000, result);
    }

    [Fact]
    public void CalculateResistance_5Band_WithGoldMultiplier_ReturnsSubOhmValue()
    {
        // Red, Red, Black, Gold, Brown = 220 * 0.1 = 22Ω
        var bands = new[] { ColorBand.Red, ColorBand.Red, ColorBand.Black, ColorBand.Gold, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(22, result);
    }

    [Fact]
    public void CalculateResistance_5Band_WithSilverMultiplier_ReturnsSubOhmValue()
    {
        // Brown, Black, Black, Silver, Brown = 100 * 0.01 = 1Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Silver, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(1, result);
    }

    #endregion

    #region CalculateResistance Tests - 6 Band Resistors

    [Fact]
    public void CalculateResistance_6Band_4_7kOhm_ReturnsCorrectValue()
    {
        // Yellow, Violet, Black, Brown, Brown, Brown = 470 * 10 = 4,700Ω (temp coeff band ignored)
        var bands = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Black, ColorBand.Brown, ColorBand.Brown, ColorBand.Brown };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(4_700, result);
    }

    [Fact]
    public void CalculateResistance_6Band_10MOhm_ReturnsCorrectValue()
    {
        // Brown, Black, Black, Green, Brown, Red = 100 * 100,000 = 10,000,000Ω (±1%, temp coeff Red)
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Green, ColorBand.Brown, ColorBand.Red };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(10_000_000, result);
    }

    [Fact]
    public void CalculateResistance_6Band_MatchesSameFiveBands()
    {
        // 6-band should yield same resistance as 5-band with identical first 5 bands
        var fiveBands = new[] { ColorBand.Orange, ColorBand.Orange, ColorBand.Red, ColorBand.Red, ColorBand.Red };
        var sixBands = new[] { ColorBand.Orange, ColorBand.Orange, ColorBand.Red, ColorBand.Red, ColorBand.Red, ColorBand.Brown };

        var fiveBandResult = _calculator.CalculateResistance(fiveBands);
        var sixBandResult = _calculator.CalculateResistance(sixBands);

        Assert.Equal(fiveBandResult, sixBandResult);
    }

    [Fact]
    public void CalculateResistance_6Band_WithDifferentTempCoeffBands_SameResistance()
    {
        // Temp coeff band (6th) should not affect the calculated resistance value
        var bandsBrownTempCoeff = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Black, ColorBand.Brown, ColorBand.Brown, ColorBand.Brown };
        var bandsRedTempCoeff   = new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Black, ColorBand.Brown, ColorBand.Brown, ColorBand.Red };

        Assert.Equal(
            _calculator.CalculateResistance(bandsBrownTempCoeff),
            _calculator.CalculateResistance(bandsRedTempCoeff));
    }

    #endregion

    #region CalculateResistance Tests - Edge Cases

    [Fact]
    public void CalculateResistance_TooFewBands_ThrowsArgumentException()
    {
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Brown };
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(bands));
    }

    [Fact]
    public void CalculateResistance_TooManyBands_ThrowsArgumentException()
    {
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Black, ColorBand.Brown, ColorBand.Brown, ColorBand.Brown, ColorBand.Brown };
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(bands));
    }

    [Fact]
    public void CalculateResistance_NullBands_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _calculator.CalculateResistance(null!));
    }

    [Fact]
    public void CalculateResistance_SubOhm_WithGoldMultiplier_ReturnsCorrectValue()
    {
        // Brown, Black, Gold, Gold = 10 * 0.1 = 1Ω
        var bands = new[] { ColorBand.Brown, ColorBand.Black, ColorBand.Gold, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(1, result);
    }

    [Fact]
    public void CalculateResistance_SubOhm_WithSilverMultiplier_ReturnsCorrectValue()
    {
        // Red, Red, Silver, Gold = 22 * 0.01 = 0.22Ω
        var bands = new[] { ColorBand.Red, ColorBand.Red, ColorBand.Silver, ColorBand.Gold };
        var result = _calculator.CalculateResistance(bands);
        Assert.Equal(0.22, result, precision: 10);
    }

    #endregion

    #region FormatResistance Tests

    [Theory]
    [InlineData(0.1, "0.1Ω")]
    [InlineData(0.47, "0.47Ω")]
    [InlineData(1, "1Ω")]
    [InlineData(10, "10Ω")]
    [InlineData(220, "220Ω")]
    [InlineData(470, "470Ω")]
    [InlineData(999, "999Ω")]
    [InlineData(1000, "1kΩ")]
    [InlineData(4700, "4.7kΩ")]
    [InlineData(10000, "10kΩ")]
    [InlineData(999000, "999kΩ")]
    [InlineData(1000000, "1MΩ")]
    [InlineData(4700000, "4.7MΩ")]
    [InlineData(999000000, "999MΩ")]
    [InlineData(1000000000, "1GΩ")]
    [InlineData(2200000000, "2.2GΩ")]
    public void FormatResistance_VariousValues_FormatsCorrectly(double value, string expected)
    {
        var result = _calculator.FormatResistance(value);
        Assert.Equal(expected, result);
    }

    #endregion
}
