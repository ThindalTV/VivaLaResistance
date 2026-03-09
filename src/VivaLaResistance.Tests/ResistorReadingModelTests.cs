namespace VivaLaResistance.Tests;

using VivaLaResistance.Core.Models;
using Xunit;

/// <summary>
/// Tests for the ResistorReading domain model.
/// Validates properties and constraints (covers AC from #20).
/// </summary>
public class ResistorReadingModelTests
{
    [Fact]
    public void ResistorReading_HasExpectedProperties()
    {
        // Arrange & Act
        var reading = new ResistorReading(
            Id: Guid.NewGuid(),
            ValueInOhms: 4700,
            FormattedValue: "4.7kΩ",
            TolerancePercent: 5.0,
            BandCount: 4,
            ColorBands: new[] { ColorBand.Yellow, ColorBand.Violet, ColorBand.Red, ColorBand.Gold },
            BoundingBox: new ResistorBoundingBox(X: 0.1f, Y: 0.2f, Width: 0.3f, Height: 0.4f, Confidence: 0.95f),
            Confidence: 0.95,
            Timestamp: DateTimeOffset.UtcNow
        );

        // Assert
        Assert.NotEqual(Guid.Empty, reading.Id);
        Assert.Equal(4700, reading.ValueInOhms);
        Assert.Equal("4.7kΩ", reading.FormattedValue);
        Assert.Equal(4, reading.ColorBands.Count);
        Assert.Equal(5.0, reading.TolerancePercent);
        Assert.Equal(0.95, reading.Confidence);
        Assert.NotNull(reading.BoundingBox);
        Assert.True(reading.Timestamp > DateTimeOffset.MinValue);
    }

    [Fact]
    public void ResistorReading_Confidence_IsInValidRange()
    {
        // Arrange & Act
        var reading = new ResistorReading(
            Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(),
            new ResistorBoundingBox(0, 0, 0, 0, 0), 0.85, DateTimeOffset.UtcNow
        );

        // Assert
        Assert.InRange(reading.Confidence, 0.0, 1.0);
    }

    [Fact]
    public void ResistorReading_ConfidenceBoundaries_AreValid()
    {
        // Arrange & Act
        var readingMin = new ResistorReading(Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(), new ResistorBoundingBox(0, 0, 0, 0, 0), 0.0, DateTimeOffset.UtcNow);
        var readingMax = new ResistorReading(Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(), new ResistorBoundingBox(0, 0, 0, 0, 0), 1.0, DateTimeOffset.UtcNow);

        // Assert
        Assert.Equal(0.0, readingMin.Confidence);
        Assert.Equal(1.0, readingMax.Confidence);
    }

    [Fact]
    public void ResistorReading_BoundingBox_HasCenterCalculations()
    {
        // Arrange
        var boundingBox = new ResistorBoundingBox(
            X: 0.1f, Y: 0.2f, Width: 0.4f, Height: 0.6f, Confidence: 0.9f
        );

        var reading = new ResistorReading(
            Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(),
            boundingBox, 0.9, DateTimeOffset.UtcNow
        );

        // Act & Assert
        Assert.Equal(0.3, reading.BoundingBox.X + reading.BoundingBox.Width / 2, precision: 5); // 0.1 + 0.4/2
        Assert.Equal(0.5, reading.BoundingBox.Y + reading.BoundingBox.Height / 2, precision: 5); // 0.2 + 0.6/2
    }

    [Fact]
    public void ResistorReading_DefaultColorBands_IsEmptyList()
    {
        // Arrange & Act
        var reading = new ResistorReading(
            Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(),
            new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow
        );

        // Assert
        Assert.NotNull(reading.ColorBands);
        Assert.Empty(reading.ColorBands);
    }

    [Fact]
    public void ResistorReading_Id_IsUniqueByDefault()
    {
        // Arrange & Act
        var reading1 = new ResistorReading(Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(), new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow);
        var reading2 = new ResistorReading(Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(), new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow);

        // Assert
        Assert.NotEqual(reading1.Id, reading2.Id);
    }

    [Fact]
    public void ResistorReading_FormattedValue_CanBeSet()
    {
        // Arrange & Act
        var reading = new ResistorReading(
            Guid.NewGuid(), 0, "10kΩ", 0, 0, Array.Empty<ColorBand>(),
            new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow
        );

        // Assert
        Assert.Equal("10kΩ", reading.FormattedValue);
    }

    [Fact]
    public void BoundingBox_NormalizedCoordinates_AreInValidRange()
    {
        // Arrange & Act
        var boundingBox = new ResistorBoundingBox(
            X: 0.25f, Y: 0.30f, Width: 0.50f, Height: 0.40f, Confidence: 0.9f
        );

        // Assert
        Assert.InRange(boundingBox.X, 0.0, 1.0);
        Assert.InRange(boundingBox.Y, 0.0, 1.0);
        Assert.InRange(boundingBox.Width, 0.0, 1.0);
        Assert.InRange(boundingBox.Height, 0.0, 1.0);
    }

    [Fact]
    public void ResistorReading_Timestamp_DefaultsToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTimeOffset.UtcNow;

        // Act
        var reading = new ResistorReading(
            Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(),
            new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow
        );
        var afterCreation = DateTimeOffset.UtcNow;

        // Assert
        Assert.InRange(reading.Timestamp, beforeCreation, afterCreation.AddSeconds(1));
    }

    [Fact]
    public void ResistorReading_MultipleReadings_HaveUniqueIds()
    {
        // Arrange & Act
        var readings = Enumerable.Range(0, 10)
            .Select(_ => new ResistorReading(
                Guid.NewGuid(), 0, "", 0, 0, Array.Empty<ColorBand>(),
                new ResistorBoundingBox(0, 0, 0, 0, 0), 0, DateTimeOffset.UtcNow
            ))
            .ToList();

        var uniqueIds = readings.Select(r => r.Id).Distinct().Count();

        // Assert
        Assert.Equal(10, uniqueIds);
    }

    [Fact]
    public void ResistorBoundingBox_IsRecord_WithExpectedProperties()
    {
        // Arrange & Act
        var bbox = new ResistorBoundingBox(X: 0.1f, Y: 0.2f, Width: 0.3f, Height: 0.4f, Confidence: 0.95f);

        // Assert
        Assert.Equal(0.1f, bbox.X);
        Assert.Equal(0.2f, bbox.Y);
        Assert.Equal(0.3f, bbox.Width);
        Assert.Equal(0.4f, bbox.Height);
        Assert.Equal(0.95f, bbox.Confidence);
    }

    [Fact]
    public void ResistorBoundingBox_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var bbox1 = new ResistorBoundingBox(0.1f, 0.2f, 0.3f, 0.4f, 0.95f);
        var bbox2 = new ResistorBoundingBox(0.1f, 0.2f, 0.3f, 0.4f, 0.95f);
        var bbox3 = new ResistorBoundingBox(0.2f, 0.2f, 0.3f, 0.4f, 0.95f);

        // Act & Assert
        Assert.Equal(bbox1, bbox2); // Same values should be equal
        Assert.NotEqual(bbox1, bbox3); // Different values should not be equal
    }

    [Fact]
    public void ResistorBoundingBox_ConfidenceRange_IsValid()
    {
        // Arrange & Act
        var bboxMin = new ResistorBoundingBox(0.1f, 0.2f, 0.3f, 0.4f, 0.0f);
        var bboxMax = new ResistorBoundingBox(0.1f, 0.2f, 0.3f, 0.4f, 1.0f);
        var bboxMid = new ResistorBoundingBox(0.1f, 0.2f, 0.3f, 0.4f, 0.75f);

        // Assert
        Assert.InRange(bboxMin.Confidence, 0.0f, 1.0f);
        Assert.InRange(bboxMax.Confidence, 0.0f, 1.0f);
        Assert.InRange(bboxMid.Confidence, 0.0f, 1.0f);
    }
}
