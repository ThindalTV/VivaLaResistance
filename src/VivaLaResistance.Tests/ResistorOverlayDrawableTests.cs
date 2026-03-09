using VivaLaResistance.Controls;
using VivaLaResistance.Core.Models;
using Xunit;

namespace VivaLaResistance.Tests;

/// <summary>
/// Tests for ResistorOverlayDrawable hysteresis logic.
/// Tests the confidence threshold state machine:
///  - Appear threshold: ≥ 0.65
///  - Disappear threshold: &lt; 0.60
///  - Hysteresis zone: 0.60–0.64 (badge persists once shown)
/// </summary>
public class ResistorOverlayDrawableTests
{
    private ResistorOverlayDrawable CreateDrawable()
    {
        return new ResistorOverlayDrawable();
    }

    private ResistorReading CreateReading(Guid id, double confidence, string formattedValue = "100Ω")
    {
        return new ResistorReading(
            id,
            ValueInOhms: 100.0,
            FormattedValue: formattedValue,
            TolerancePercent: 5.0,
            BandCount: 4,
            ColorBands: Array.Empty<ColorBand>(),
            BoundingBox: new ResistorBoundingBox(0.1f, 0.1f, 0.2f, 0.2f, (float)confidence),
            Confidence: confidence,
            Timestamp: DateTimeOffset.UtcNow);
    }

    [Fact]
    public void UpdateReadings_ConfidenceAtShowThreshold_BadgeAppears()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        var reading = CreateReading(id, 0.65); // exactly at show threshold

        // Act
        drawable.UpdateReadings(new[] { reading });

        // Assert
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
        Assert.Contains(id, drawable._activeIds);
    }

    [Fact]
    public void UpdateReadings_ConfidenceAboveShowThreshold_BadgeAppears()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        var reading = CreateReading(id, 0.70); // above show threshold

        // Act
        drawable.UpdateReadings(new[] { reading });

        // Assert
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
    }

    [Fact]
    public void UpdateReadings_ConfidenceBelowShowThreshold_BadgeDoesNotAppear()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        var reading = CreateReading(id, 0.64); // just below show threshold

        // Act
        drawable.UpdateReadings(new[] { reading });

        // Assert
        Assert.Empty(drawable._visibleBadges);
    }

    [Fact]
    public void UpdateReadings_BadgeShownThenConfidenceInHysteresisZone_BadgePersists()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // First: show badge at 0.70
        var reading1 = CreateReading(id, 0.70);
        drawable.UpdateReadings(new[] { reading1 });

        // Act: confidence drops to 0.62 (in hysteresis zone 0.60–0.64)
        var reading2 = CreateReading(id, 0.62);
        drawable.UpdateReadings(new[] { reading2 });

        // Assert: badge persists
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
        Assert.Equal(0.62, drawable._visibleBadges[id].Confidence);
    }

    [Fact]
    public void UpdateReadings_BadgeShownThenConfidenceAtHideThreshold_BadgePersists()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // First: show badge at 0.70
        var reading1 = CreateReading(id, 0.70);
        drawable.UpdateReadings(new[] { reading1 });

        // Act: confidence exactly at hide threshold (0.60)
        var reading2 = CreateReading(id, 0.60);
        drawable.UpdateReadings(new[] { reading2 });

        // Assert: badge persists (≥ 0.60)
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
    }

    [Fact]
    public void UpdateReadings_BadgeShownThenConfidenceBelowHideThreshold_BadgeDisappears()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // First: show badge at 0.70
        var reading1 = CreateReading(id, 0.70);
        drawable.UpdateReadings(new[] { reading1 });

        // Act: confidence drops below hide threshold
        var reading2 = CreateReading(id, 0.59);
        drawable.UpdateReadings(new[] { reading2 });

        // Assert: badge disappears
        Assert.Empty(drawable._visibleBadges);
    }

    [Fact]
    public void UpdateReadings_BadgeDisappearsThenRises_RequiresShowThresholdToReappear()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // Show at 0.70
        drawable.UpdateReadings(new[] { CreateReading(id, 0.70) });
        
        // Drop to 0.55 (disappears)
        drawable.UpdateReadings(new[] { CreateReading(id, 0.55) });

        // Act: rise to 0.62 (in hysteresis zone but badge already hidden)
        drawable.UpdateReadings(new[] { CreateReading(id, 0.62) });

        // Assert: badge stays hidden (must reach show threshold again)
        Assert.Empty(drawable._visibleBadges);

        // Act: rise to show threshold
        drawable.UpdateReadings(new[] { CreateReading(id, 0.65) });

        // Assert: badge reappears
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
    }

    [Fact]
    public void UpdateReadings_MultipleReadings_TrackedIndependently()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        
        // Both start above show threshold
        var reading1 = CreateReading(id1, 0.70, "100Ω");
        var reading2 = CreateReading(id2, 0.75, "220Ω");
        drawable.UpdateReadings(new[] { reading1, reading2 });

        // Act: id1 drops into hysteresis, id2 drops below hide threshold
        var updated1 = CreateReading(id1, 0.62, "100Ω"); // persists
        var updated2 = CreateReading(id2, 0.58, "220Ω"); // disappears
        drawable.UpdateReadings(new[] { updated1, updated2 });

        // Assert: only id1 remains
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id1, drawable._visibleBadges.Keys);
        Assert.DoesNotContain(id2, drawable._visibleBadges.Keys);
    }

    [Fact]
    public void UpdateReadings_ReadingRemovedFromCollection_BadgeClears()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        
        // Show two badges
        drawable.UpdateReadings(new[]
        {
            CreateReading(id1, 0.70, "100Ω"),
            CreateReading(id2, 0.75, "220Ω")
        });

        // Act: only provide id1 in next update (id2 no longer detected)
        drawable.UpdateReadings(new[] { CreateReading(id1, 0.70, "100Ω") });

        // Assert: id2 badge removed immediately
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id1, drawable._visibleBadges.Keys);
        Assert.DoesNotContain(id2, drawable._visibleBadges.Keys);
        Assert.DoesNotContain(id2, drawable._activeIds);
    }

    [Fact]
    public void UpdateReadings_EmptyCollection_ClearsAllBadges()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // Show a badge
        drawable.UpdateReadings(new[] { CreateReading(id, 0.70) });
        Assert.Single(drawable._visibleBadges);

        // Act: send empty collection
        drawable.UpdateReadings(Array.Empty<ResistorReading>());

        // Assert: all badges cleared
        Assert.Empty(drawable._visibleBadges);
        Assert.Empty(drawable._activeIds);
    }

    [Fact]
    public void UpdateReadings_ConfidenceOscillatesInHysteresisZone_BadgeStable()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id = Guid.NewGuid();
        
        // Show badge
        drawable.UpdateReadings(new[] { CreateReading(id, 0.70) });

        // Act: oscillate in hysteresis zone multiple times
        drawable.UpdateReadings(new[] { CreateReading(id, 0.64) }); // persists
        drawable.UpdateReadings(new[] { CreateReading(id, 0.61) }); // persists
        drawable.UpdateReadings(new[] { CreateReading(id, 0.63) }); // persists
        drawable.UpdateReadings(new[] { CreateReading(id, 0.60) }); // persists

        // Assert: badge stable throughout
        Assert.Single(drawable._visibleBadges);
        Assert.Contains(id, drawable._visibleBadges.Keys);
    }

    [Fact]
    public void UpdateReadings_ThreeResistors_IndependentHysteresis()
    {
        // Arrange
        var drawable = CreateDrawable();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();
        
        // All start visible
        drawable.UpdateReadings(new[]
        {
            CreateReading(id1, 0.80, "100Ω"),
            CreateReading(id2, 0.75, "220Ω"),
            CreateReading(id3, 0.70, "470Ω")
        });

        // Act: id1 stays high, id2 enters hysteresis, id3 drops out
        drawable.UpdateReadings(new[]
        {
            CreateReading(id1, 0.78, "100Ω"),
            CreateReading(id2, 0.61, "220Ω"),
            CreateReading(id3, 0.58, "470Ω")
        });

        // Assert: id1 and id2 visible, id3 gone
        Assert.Equal(2, drawable._visibleBadges.Count);
        Assert.Contains(id1, drawable._visibleBadges.Keys);
        Assert.Contains(id2, drawable._visibleBadges.Keys);
        Assert.DoesNotContain(id3, drawable._visibleBadges.Keys);
    }
}
