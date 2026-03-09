using Microsoft.Maui.Graphics;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Controls;

/// <summary>
/// Renders AR-style value badges for detected resistors on a GraphicsView canvas.
/// Hysteresis: badges appear at confidence ≥ 0.65, disappear below 0.60 (per decisions.md).
/// All coordinates use the normalized BoundingBox from ResistorReading (0–1 range).
/// </summary>
public sealed class ResistorOverlayDrawable : IDrawable
{
    // ── Confidence thresholds (decisions.md — Bruce's confidence threshold) ──
    private const double ShowThreshold   = 0.65; // add badge when confidence first exceeds this
    private const double HideThreshold   = 0.60; // remove badge when confidence drops below this
    private const double GreenThreshold  = 0.85;
    private const double YellowThreshold = 0.65;

    // ── Badge layout constants ────────────────────────────────────────────────
    private const float BadgeCornerRadius = 8f;
    private const float BadgeGapAboveBox  = 6f;

    // Responsive metrics are computed per draw call via GetBreakpointMetrics().
    private readonly record struct BadgeMetrics(
        float ValueFontSize,
        float ToleranceFontSize,
        float BadgeHeight,
        float PaddingH,
        float PaddingV,
        float MinWidth,
        float MaxWidth,
        float DotRadius);

    // Breakpoint thresholds (dp): Compact < 400, Standard 400–599, Expanded ≥ 600.
    private static BadgeMetrics GetBreakpointMetrics(float viewW) => viewW switch
    {
        < 400f => new BadgeMetrics(14f, 10f, 38f, 6f,  4f,  60f, 160f, 4f),
        < 600f => new BadgeMetrics(16f, 12f, 42f, 8f,  6f,  80f, 160f, 5f),
        _      => new BadgeMetrics(18f, 14f, 46f, 10f, 6f, 100f, 200f, 6f),
    };

    // Visible badges keyed by ResistorReading.Id (Guid).
    // Separate sets track which IDs are "active" for hysteresis logic.
    internal readonly Dictionary<Guid, ResistorReading> _visibleBadges = new();
    internal readonly HashSet<Guid> _activeIds = new();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Merges a new set of detection results into the visible badge set,
    /// applying hysteresis: adds at ≥ 0.65, removes below 0.60.
    /// Must be called on the main thread.
    /// </summary>
    public void UpdateReadings(IEnumerable<ResistorReading> newReadings)
    {
        var incoming = newReadings.ToDictionary(r => r.Id);

        // Update or evict existing badges
        var toEvict = new List<Guid>();
        foreach (var id in _activeIds)
        {
            if (incoming.TryGetValue(id, out var updated))
            {
                if (updated.Confidence >= HideThreshold)
                    _visibleBadges[id] = updated;
                else
                    toEvict.Add(id);
            }
            else
            {
                // No longer detected — remove immediately
                toEvict.Add(id);
            }
        }
        foreach (var id in toEvict)
        {
            _visibleBadges.Remove(id);
            _activeIds.Remove(id);
        }

        // Admit new badges that cross the show threshold
        foreach (var (id, reading) in incoming)
        {
            if (!_activeIds.Contains(id) && reading.Confidence >= ShowThreshold)
            {
                _visibleBadges[id] = reading;
                _activeIds.Add(id);
            }
        }
    }

    // ── IDrawable ─────────────────────────────────────────────────────────────

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_visibleBadges.Count == 0)
            return;

        foreach (var reading in _visibleBadges.Values)
            DrawBadge(canvas, reading, dirtyRect.Width, dirtyRect.Height);
    }

    // ── Badge rendering ───────────────────────────────────────────────────────

    private static void DrawBadge(ICanvas canvas, ResistorReading reading, float viewW, float viewH)
    {
        var m   = GetBreakpointMetrics(viewW);
        var box = reading.BoundingBox;

        float boxLeft   = box.X      * viewW;
        float boxTop    = box.Y      * viewH;
        float boxBottom = (box.Y + box.Height) * viewH;
        float boxWidth  = box.Width  * viewW;

        // Badge width clamped to breakpoint-specific range, centered on the bounding box
        float badgeW = Math.Clamp(boxWidth, m.MinWidth, m.MaxWidth);
        float badgeH = m.BadgeHeight;

        float centerX = boxLeft + boxWidth / 2f;
        float badgeX  = centerX - badgeW / 2f;

        // Horizontal edge clamping: keep badge within the view
        badgeX = Math.Max(4f, badgeX);
        badgeX = Math.Min(viewW - badgeW - 4f, badgeX);

        // Vertical placement: above the box normally; flip below when near the top edge
        float badgeY;
        if (boxTop - badgeH - BadgeGapAboveBox < 4f)
            badgeY = Math.Min(boxBottom + BadgeGapAboveBox, viewH - badgeH - 4f);
        else
            badgeY = Math.Max(4f, boxTop - badgeH - BadgeGapAboveBox);

        canvas.SaveState();

        // Background: pure black, 80% opacity — best universal contrast (spec §2.2)
        canvas.FillColor = Color.FromArgb("#CC000000");
        canvas.FillRoundedRectangle(badgeX, badgeY, badgeW, badgeH, BadgeCornerRadius);

        // Thin border
        canvas.StrokeColor = Color.FromArgb("#66FFFFFF");
        canvas.StrokeSize  = 1f;
        canvas.DrawRoundedRectangle(badgeX, badgeY, badgeW, badgeH, BadgeCornerRadius);

        // Confidence dot — top-right corner inside the badge
        float dotX = badgeX + badgeW - m.DotRadius - 7f;
        float dotY = badgeY + m.DotRadius + 7f;
        canvas.FillColor = ConfidenceColor(reading.Confidence);
        canvas.FillCircle(dotX, dotY, m.DotRadius);

        // FormattedValue on the first line
        float textMaxW = badgeW - m.PaddingH * 2 - m.DotRadius * 2 - 6f;
        canvas.FontColor = Colors.White;
        canvas.FontSize  = m.ValueFontSize;
        canvas.DrawString(
            reading.FormattedValue,
            badgeX + m.PaddingH,
            badgeY + m.PaddingV,
            textMaxW,
            20f,
            HorizontalAlignment.Left,
            VerticalAlignment.Top);

        // TolerancePercent on the second line
        string toleranceLine = $"\u00b1{reading.TolerancePercent:0.#}%";
        canvas.FontColor = Color.FromArgb("#CCBBBBBB");
        canvas.FontSize  = m.ToleranceFontSize;
        canvas.DrawString(
            toleranceLine,
            badgeX + m.PaddingH,
            badgeY + m.PaddingV + 19f,
            badgeW - m.PaddingH * 2,
            16f,
            HorizontalAlignment.Left,
            VerticalAlignment.Top);

        canvas.RestoreState();
    }

    private static Color ConfidenceColor(double confidence) =>
        confidence >= GreenThreshold  ? Colors.LimeGreen :
        confidence >= YellowThreshold ? Colors.Yellow    :
                                        Colors.OrangeRed;
}
