# Overlay Badge Component Specification

**Version:** 1.0  
**Author:** Hope (UX Designer)  
**Issue:** #21  
**Last Updated:** 2025-01-15

---

## 1. Overview

The overlay badge is the core UI component that displays resistor values as AR-style labels anchored to detected resistors in the live camera view. This spec documents the complete visual design, behavior, and implementation requirements.

### Component Summary

| Property | Description |
|----------|-------------|
| **Purpose** | Display detected resistor value + tolerance as a floating badge |
| **Anchor** | Above detected resistor's bounding box, horizontally centered |
| **Content** | Two lines: formatted value (line 1), tolerance percent (line 2) |
| **Confidence Indicator** | Colored dot showing detection confidence |

---

## 2. Visual Design

### 2.1 Badge Shape

| Property | Value | Notes |
|----------|-------|-------|
| **Shape** | Rounded rectangle | |
| **Corner Radius** | 8dp | Consistent with design-guidelines.md |
| **Height** | Variable by breakpoint | See Section 4 |
| **Width** | Dynamic, clamped | See Section 2.3 |

### 2.2 Colors

| Element | Current Implementation | Design Guidelines Spec | Recommendation |
|---------|------------------------|------------------------|----------------|
| **Background** | `#CC1a1a2e` (dark blue-grey, 80% opacity) | `#000000` at 80% opacity | **Change to guidelines spec** — pure black provides better universal contrast |
| **Border** | `#66FFFFFF` (white, 40% opacity) | `#FFFFFF` at 30% opacity | Current is acceptable — 40% provides slightly better edge definition |
| **Value Text** | `#FFFFFF` (white) | `#FFFFFF` (white) | ✅ Matches |
| **Tolerance Text** | `#CCBBBBBB` (light grey, 80% opacity) | Not specified | ✅ Acceptable — creates clear hierarchy |

#### Recommended Badge Colors

| Element | Hex Value | Opacity | Usage |
|---------|-----------|---------|-------|
| **Background** | `#000000` | 80% (`#CC000000`) | Badge backing — universal contrast |
| **Border** | `#FFFFFF` | 40% (`#66FFFFFF`) | Optional edge definition |
| **Value Text** | `#FFFFFF` | 100% | Primary content |
| **Tolerance Text** | `#BBBBBB` | 80% (`#CCBBBBBB`) | Secondary content |

### 2.3 Badge Sizing

#### Width

| Property | Value | Notes |
|----------|-------|-------|
| **Calculation** | Width of resistor bounding box | In screen pixels |
| **Minimum Width** | Breakpoint-dependent | See Section 4 |
| **Maximum Width** | 160dp (all breakpoints) | Prevents excessive badge sprawl |

#### Height

Height is fixed per breakpoint to accommodate two text lines plus padding. Calculated as:
- `(value line height) + (tolerance line height) + (padding × 2)`

| Breakpoint | Badge Height | Calculation |
|------------|--------------|-------------|
| **Compact** (<400dp) | 38dp | 16 + 12 + 10 = 38 |
| **Standard** (400-599dp) | 42dp | 18 + 14 + 10 = 42 |
| **Expanded** (≥600dp) | 46dp | 20 + 16 + 10 = 46 |

### 2.4 Confidence Indicator Dot

| Property | Value |
|----------|-------|
| **Shape** | Circle |
| **Radius** | 5dp |
| **Position** | Top-right corner of badge, inset by 7dp from edges |
| **Purpose** | Visual indicator of detection confidence |

#### Confidence Colors

| Confidence Level | Color | Hex Value |
|------------------|-------|-----------|
| **High** (≥0.85) | Lime Green | `#32CD32` (LimeGreen) |
| **Medium** (0.65–0.84) | Yellow | `#FFFF00` (Yellow) |
| **Low** (<0.65) | Orange-Red | `#FF4500` (OrangeRed) |

> **Note:** Badges only appear when confidence ≥0.65 (show threshold). The "low" color is visible during hysteresis when confidence drops to 0.60-0.64 but hasn't crossed the hide threshold yet.

---

## 3. Typography

### 3.1 Font Specifications

| Element | Weight | Notes |
|---------|--------|-------|
| **Value Text** | Bold (700) | Maximum legibility over varied backgrounds |
| **Tolerance Text** | Regular (400) | Secondary information, clear hierarchy |
| **Font Family** | System Default | San Francisco (iOS), Roboto (Android) |

### 3.2 Font Sizes by Breakpoint

#### Current Implementation vs. Design Guidelines

| Element | Current (Fixed) | Guidelines: Compact | Guidelines: Standard | Guidelines: Expanded |
|---------|-----------------|---------------------|----------------------|----------------------|
| Value | 13sp | 14sp | 16sp | 18sp |
| Tolerance | 10sp | 10sp | 12sp | 14sp |

**Gap Identified:** Current implementation uses fixed 13sp/10sp regardless of screen size. This should be responsive.

#### Recommended Font Sizes

| Breakpoint | Value Font | Tolerance Font | Line Height |
|------------|------------|----------------|-------------|
| **Compact** (<400dp) | 14sp | 10sp | 1.2 |
| **Standard** (400-599dp) | 16sp | 12sp | 1.2 |
| **Expanded** (≥600dp) | 18sp | 14sp | 1.2 |

---

## 4. Responsive Breakpoints

The badge adapts to three device classes per design-guidelines.md Section 3.

### 4.1 Complete Badge Specifications by Breakpoint

| Property | Compact (<400dp) | Standard (400-599dp) | Expanded (≥600dp) |
|----------|------------------|----------------------|-------------------|
| **Content Format** | Value + tolerance only | Value + tolerance | Value + tolerance |
| **Value Font Size** | 14sp | 16sp | 18sp |
| **Tolerance Font Size** | 10sp | 12sp | 14sp |
| **Horizontal Padding** | 6dp | 8dp | 10dp |
| **Vertical Padding** | 4dp | 6dp | 6dp |
| **Badge Height** | 38dp | 42dp | 46dp |
| **Min Width** | 60dp | 80dp | 100dp |
| **Max Width** | 160dp | 160dp | 200dp |
| **Confidence Dot Radius** | 4dp | 5dp | 6dp |
| **Max Visible Badges** | 2 | 3 | 5-6 |
| **Gap Above Bounding Box** | 6dp | 8dp | 8dp |

### 4.2 Current Implementation Gaps

The existing `ResistorOverlayDrawable.cs` uses fixed values:

```csharp
// CURRENT (non-responsive):
private const float BadgePaddingH    = 10f;
private const float BadgePaddingV    = 6f;
private const float BadgeMinWidth    = 80f;
private const float BadgeMaxWidth    = 160f;
private const float BadgeHeight      = 46f;
private const float ValueFontSize    = 13f;
private const float ToleranceFontSize = 10f;
```

**Recommendation:** These constants should become methods that return values based on current screen width breakpoint.

---

## 5. Positioning & Layout

### 5.1 Anchor Position

| Property | Value | Notes |
|----------|-------|-------|
| **Horizontal Anchor** | Centered on bounding box | `centerX = boxLeft + boxWidth / 2` |
| **Vertical Anchor** | Above bounding box | Gap defined by breakpoint |
| **Gap Above Box** | 6–8dp | Per breakpoint (see Section 4.1) |

### 5.2 Edge Clamping

The badge must remain visible within the camera view.

| Edge | Current Behavior | Recommended Behavior |
|------|------------------|----------------------|
| **Top** | Clamp Y to minimum 4dp | ✅ Correct |
| **Bottom** | Not handled | Flip badge to below bounding box |
| **Left** | Not handled | Clamp X so badge doesn't exit left edge |
| **Right** | Not handled | Clamp X so badge doesn't exit right edge |

#### Recommended Clamping Logic

```
// Horizontal clamping
badgeX = max(safeAreaLeft + 4dp, badgeX)
badgeX = min(viewWidth - safeAreaRight - badgeWidth - 4dp, badgeX)

// Vertical clamping (flip if near top)
if (boxTop - badgeHeight - gap < safeAreaTop + 4dp):
    badgeY = boxBottom + gap  // flip to below
else:
    badgeY = boxTop - badgeHeight - gap  // normal: above

// Vertical clamping (ensure visible if near bottom)
if (badgeY + badgeHeight > viewHeight - safeAreaBottom - 4dp):
    badgeY = viewHeight - safeAreaBottom - badgeHeight - 4dp
```

### 5.3 Safe Area Considerations

| Platform | Safe Area | Badge Behavior |
|----------|-----------|----------------|
| **iOS Notch/Dynamic Island** | Top inset varies | Badge must not anchor behind notch |
| **iOS Home Indicator** | Bottom ~34dp | Badge must not anchor behind indicator |
| **Android Punch-hole** | Varies by device | Badge must not anchor behind cutout |
| **Android Nav Bar** | Bottom varies | Badge respects nav bar safe area |

---

## 6. Multiple Badge Handling

### 6.1 Overlap Detection

When multiple resistors are detected close together, their badges may overlap.

| Scenario | Behavior |
|----------|----------|
| **Badges would overlap** | Stack vertically with 4dp gap |
| **Stacking exits screen** | Anchor badges to left/right instead of center |
| **More than max visible** | Show only N most confident badges (N per breakpoint) |

### 6.2 Stacking Rules

```
Primary Strategy:
┌───────────────┐
│ 5-band · 10kΩ │
├───────────────┤  4dp gap
│ 4-band · 470Ω │
└───────────────┘
┌───────────────┐
│ [2 resistors] │
└───────────────┘

Secondary Strategy (if stack exits bounds):
┌─────────────┐           ┌─────────────┐
│ 5-band·10kΩ │           │ 4-band·470Ω │
└─────────────┘           └─────────────┘
        ┌─────────────────────────┐
        │    [2 resistors]        │
        └─────────────────────────┘
```

### 6.3 Badge Priority

When limiting visible badges, prioritize by:

1. **Highest confidence** — most reliable readings shown first
2. **Longest visible duration** — stable readings over transient ones
3. **Center of frame** — resistors near center take priority

### 6.4 Maximum Simultaneous Badges

| Breakpoint | Max Visible | Additional Resistors |
|------------|-------------|----------------------|
| Compact | 2 | Detected but not labeled |
| Standard | 3 | Detected but not labeled |
| Expanded | 5-6 | Detected but not labeled |

---

## 7. Content & Text

### 7.1 Badge Content Layout

```
┌──────────────────────────────────────┐
│ 10kΩ ±5%                          🟢 │
│                                      │
└──────────────────────────────────────┘
  ↑                                  ↑
  Line 1: Formatted value            Confidence dot
  Line 2: Tolerance (implicit in ±5%)

Current implementation (two lines):
┌──────────────────────────────────────┐
│ 10kΩ                              🟢 │
│ ±5%                                  │
└──────────────────────────────────────┘
```

**Recommendation:** The current two-line format is acceptable. Consider single-line format (`10kΩ ±5%`) for compact breakpoint to reduce badge height.

### 7.2 Text Truncation

| Scenario | Behavior |
|----------|----------|
| **Value exceeds badge width** | Truncate with ellipsis `…` |
| **Very long formatted value** | Values like "2.2MΩ" fit within max width |
| **Compact breakpoint** | Omit type prefix (e.g., show "10kΩ" not "5-band · 10kΩ") |

### 7.3 Content Format by Breakpoint

| Breakpoint | Line 1 Format | Line 2 Format | Example |
|------------|---------------|---------------|---------|
| **Compact** | `{value}` | `±{tolerance}%` | "10kΩ" / "±5%" |
| **Standard** | `{value}` | `±{tolerance}%` | "10kΩ" / "±5%" |
| **Expanded** | `{type} · {value}` | `±{tolerance}%` | "5-band · 10kΩ" / "±5%" |

> **Note:** The current implementation shows `FormattedValue` on line 1, which should include the value with formatting. The type prefix can be conditionally included based on breakpoint.

---

## 8. Accessibility

### 8.1 Contrast Ratios

| Element Pair | Ratio | WCAG Level | Status |
|--------------|-------|------------|--------|
| Value text (#FFFFFF) on badge (#000000 @ 80%) | 13.5:1 | AAA | ✅ Exceeds |
| Tolerance text (#BBBBBB) on badge (#000000 @ 80%) | 9.2:1 | AAA | ✅ Exceeds |
| Border (#FFFFFF @ 40%) on camera background | Variable | — | Edge definition only |

### 8.2 Minimum Readable Size

| Breakpoint | Value Font | At 60cm Viewing Distance | Legibility |
|------------|------------|-------------------------|------------|
| Compact | 14sp | ~2.3mm | Acceptable |
| Standard | 16sp | ~2.7mm | Good |
| Expanded | 18sp | ~3.0mm | Excellent |

> **Note:** 16sp is the minimum recommended for arm's-length reading per design-guidelines.md. Compact breakpoint uses 14sp as a necessary compromise on cramped screens.

### 8.3 Screen Reader Behavior

| Event | VoiceOver/TalkBack Announcement |
|-------|--------------------------------|
| **New resistor detected** | "Detected: {type} resistor, {value} ohms, plus or minus {tolerance} percent" |
| **Resistor value changes** | Re-announce only if value differs significantly |
| **Resistor exits frame** | Silent (default) or "Resistor lost" (optional, default off) |

### 8.4 Dynamic Type Support

| Setting | Badge Behavior |
|---------|----------------|
| **Up to 200% scale** | Badge scales proportionally |
| **Beyond 200%** | Truncate text with ellipsis, cap badge size |

---

## 9. Animation

### 9.1 Badge Appear

| Property | Value |
|----------|-------|
| **Trigger** | Confidence crosses show threshold (≥0.65) |
| **Animation** | Fade in |
| **Duration** | 150ms |
| **Easing** | Ease-out |

### 9.2 Badge Update

| Property | Value |
|----------|-------|
| **Trigger** | Value text changes |
| **Animation** | Cross-fade text |
| **Duration** | 100ms |
| **Easing** | Linear |

### 9.3 Badge Disappear

| Property | Value |
|----------|-------|
| **Trigger** | Confidence crosses hide threshold (<0.60) or resistor exits |
| **Animation** | Fade out |
| **Duration** | 100ms |
| **Easing** | Linear |

### 9.4 Badge Reposition

| Property | Value |
|----------|-------|
| **Trigger** | Resistor moves within frame |
| **Animation** | Smooth translate |
| **Duration** | 100ms |
| **Easing** | Ease-in-out |

> **Current Implementation:** No animations. Badges appear/disappear instantly. Animation support is optional for v1.0 but recommended for polish.

---

## 10. Hysteresis Behavior

The badge uses hysteresis to prevent flickering when confidence hovers near threshold.

### 10.1 Thresholds

| Threshold | Value | Behavior |
|-----------|-------|----------|
| **Show** | ≥0.65 | Badge appears when confidence rises to or above |
| **Hide** | <0.60 | Badge disappears when confidence drops below |
| **Hysteresis Zone** | 0.60–0.64 | Badge persists if already shown |

### 10.2 State Diagram

```
                  conf ≥ 0.65
    ┌───────┐ ───────────────► ┌─────────┐
    │ Hidden│                  │ Visible │
    └───────┘ ◄─────────────── └─────────┘
                  conf < 0.60
```

---

## 11. Edge Cases

### 11.1 Very Long Value Strings

| Scenario | Example | Behavior |
|----------|---------|----------|
| High-value resistor | "2.2MΩ" | Fits within max width |
| 6-band detailed | "6-band · 4.7kΩ" | Truncate to "6-band · 4…" if needed |
| Extreme precision | "±0.05%" | Tolerance line accommodates |

### 11.2 Badge Near Screen Edges

| Edge | Behavior |
|------|----------|
| **Top edge** | Flip badge to below bounding box |
| **Bottom edge** | Clamp Y position, don't extend into nav bar |
| **Left edge** | Shift badge right to stay visible |
| **Right edge** | Shift badge left to stay visible |
| **Corner** | Apply both horizontal and vertical adjustments |

### 11.3 Overlapping Resistors

| Scenario | Behavior |
|----------|----------|
| **2 resistors, badges overlap** | Stack vertically with 4dp gap |
| **3+ resistors in tight cluster** | Stack up to max visible, hide lowest confidence |
| **Stack would exit screen** | Anchor to sides instead of center |

### 11.4 Rapid Detection Changes

| Scenario | Behavior |
|----------|----------|
| **Resistor detected/lost rapidly** | Hysteresis prevents flicker |
| **Value oscillates** | Show most recent stable value |
| **Camera panning quickly** | Badges track smoothly with 100ms ease |

### 11.5 Low Confidence State

| Confidence | Visual Indicator |
|------------|------------------|
| **≥0.85** | Green dot — high confidence |
| **0.65–0.84** | Yellow dot — medium confidence |
| **0.60–0.64** | Orange-red dot — low confidence (hysteresis zone) |
| **<0.60** | Badge hidden |

> **Design Guidelines Note:** Section 7 mentions dashed border + "?" suffix for low confidence. Current implementation uses confidence dot instead. **Recommend: Keep confidence dot** — it's more elegant and less visually noisy than dashed borders.

---

## 12. Implementation Gap Analysis

### 12.1 Current vs. Recommended

| Aspect | Current Implementation | Recommended | Priority |
|--------|------------------------|-------------|----------|
| **Background color** | `#CC1a1a2e` (dark blue-grey) | `#CC000000` (pure black) | Medium |
| **Responsive font sizes** | Fixed 13sp/10sp | Breakpoint-dependent | High |
| **Responsive padding** | Fixed 10dp/6dp | Breakpoint-dependent | High |
| **Responsive min/max width** | Fixed 80dp/160dp | Breakpoint-dependent | High |
| **Edge clamping (horizontal)** | Not implemented | Clamp to safe area | Medium |
| **Edge flipping (vertical)** | Partial (top clamp only) | Flip to below when near top | Medium |
| **Max visible badges** | Unlimited | Breakpoint-dependent limit | Low |
| **Animations** | None | Fade/translate | Low |
| **Overlap handling** | None | Vertical stacking | Low |

### 12.2 Recommended Implementation Order

1. **High Priority (v1.0):**
   - Responsive font sizes and padding
   - Responsive min/max badge width
   - Background color update to pure black

2. **Medium Priority (v1.1):**
   - Horizontal edge clamping
   - Vertical flip behavior
   - Safe area respect on iOS/Android

3. **Low Priority (Future):**
   - Animations
   - Overlap handling / stacking
   - Max visible badge limiting

---

## 13. Summary

The overlay badge component is the primary user interface element in VivaLaResistance. This spec ensures:

- **Legibility** over any camera background via high-contrast dark badge
- **Adaptability** to phone, fold, and tablet screen sizes via responsive breakpoints
- **Clarity** through consistent typography hierarchy
- **Reliability** through hysteresis preventing badge flicker
- **Accessibility** exceeding WCAG AAA contrast requirements

### Key Deliverables for Shuri

1. Update `ResistorOverlayDrawable.cs` to use responsive constants based on screen width
2. Change badge background from `#CC1a1a2e` to `#CC000000`
3. Implement horizontal edge clamping to keep badges on-screen
4. Implement vertical flip when resistor is near screen top

---

*This specification supersedes any conflicting values in design-guidelines.md Section 7. Design-guidelines.md should be updated to reference this document for overlay badge details.*
