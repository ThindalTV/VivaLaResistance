using Microsoft.Maui.Graphics;
using VivaLaResistance.Core.Models;

namespace VivaLaResistance.Controls;

/// <summary>
/// Renders AR-style value badges for detected resistors on a GraphicsView canvas.
/// Hysteresis: badges appear at confidence ≥ 0.65, disappear below 0.60 (per decisions.md).
/// All coordinates use the normalized BoundingBox from ResistorReading (0–1 range).
/// </summary>
internal sealed class ResistorOverlayDrawable : IDrawable
{
    // ── Confidence thresholds (decisions.md — Bruce's confidence threshold) ──
    private const double ShowThreshold   = 0.65; // add badge when confidence first exceeds this
    private const double HideThreshold   = 0.60; // remove badge when confidence drops below this
    private const double GreenThreshold  = 0.85;
    private const double YellowThreshold = 0.65;

    // ── Badge layout constants ────────────────────────────────────────────────
    private const float BadgePaddingH    = 10f;
    private const float BadgePaddingV    = 6f;
    private const float BadgeCornerRadius = 8f;
    private const float BadgeMinWidth    = 80f;
    private const float BadgeMaxWidth    = 160f;
    private const float BadgeHeight      = 46f;  // two text lines + padding
    private const float BadgeGapAboveBox = 6f;
    private const float DotRadius        = 5f;

    private const float ValueFontSize     = 13f;
    private const float ToleranceFontSize = 10f;

    // Visible badges keyed by ResistorReading.Id (Guid).
    // Separate sets track which IDs are "active" for hysteresis logic.
    private readonly Dictionary<string, ResistorReading> _visibleBadges = new();
    private readonly HashSet<string> _activeIds = new();

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
        var toEvict = new List<string>();
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
        var box = reading.BoundingBox;

        float boxLeft   = box.X      * viewW;
        float boxTop    = box.Y      * viewH;
        float boxWidth  = box.Width  * viewW;

        // Badge width clamped to reasonable range, centered on the bounding box
        float badgeW = Math.Clamp(boxWidth, BadgeMinWidth, BadgeMaxWidth);
        float badgeH = BadgeHeight;

        float centerX  = boxLeft + boxWidth / 2f;
        float badgeX   = centerX - badgeW / 2f;
        float badgeY   = Math.Max(4f, boxTop - badgeH - BadgeGapAboveBox);

        canvas.SaveState();

        // Background: dark semi-transparent rounded rectangle
        canvas.FillColor   = Color.FromArgb("#CC1a1a2e");
        canvas.FillRoundedRectangle(badgeX, badgeY, badgeW, badgeH, BadgeCornerRadius);

        // Thin border
        canvas.StrokeColor = Color.FromArgb("#66FFFFFF");
        canvas.StrokeSize  = 1f;
        canvas.DrawRoundedRectangle(badgeX, badgeY, badgeW, badgeH, BadgeCornerRadius);

        // Confidence dot — top-right corner inside the badge
        float dotX = badgeX + badgeW - DotRadius - 7f;
        float dotY = badgeY + DotRadius + 7f;
        canvas.FillColor = ConfidenceColor(reading.Confidence);
        canvas.FillCircle(dotX, dotY, DotRadius);

        // FormattedValue on the first line
        float textMaxW = badgeW - BadgePaddingH * 2 - DotRadius * 2 - 6f;
        canvas.FontColor = Colors.White;
        canvas.FontSize  = ValueFontSize;
        canvas.DrawString(
            reading.FormattedValue,
            badgeX + BadgePaddingH,
            badgeY + BadgePaddingV,
            textMaxW,
            20f,
            HorizontalAlignment.Left,
            VerticalAlignment.Top);

        // TolerancePercent on the second line
        string toleranceLine = $"\u00b1{reading.TolerancePercent:0.#}%";
        canvas.FontColor = Color.FromArgb("#CCBBBBBB");
        canvas.FontSize  = ToleranceFontSize;
        canvas.DrawString(
            toleranceLine,
            badgeX + BadgePaddingH,
            badgeY + BadgePaddingV + 19f,
            badgeW - BadgePaddingH * 2,
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
