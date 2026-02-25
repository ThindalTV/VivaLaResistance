# Decision: UX Design Guidelines

**Date:** 2026-02-25  
**Author:** Hope (UX Designer)  
**Status:** Proposed

## Summary

Established comprehensive UX design guidelines for VivaLaResistance covering color palette, typography, overlay component design, support dialog UX, and accessibility requirements.

## Key Decisions

### 1. Color Palette: Dark-First
- Primary background: `#121212` (near black)
- Primary accent: `#4FC3F7` (electric blue)
- Overlay badges: `#000000` at 80% opacity with white text
- **Rationale:** Camera apps work better with dark UI; reduces glare and improves contrast with live camera feed

### 2. Overlay Badge Design
- Semi-transparent dark background ensures readability over any real-world background
- Badge anchors above resistor bounding box (flips to below when near screen top)
- Content format: `{type} · {value} {tolerance}` (e.g., "5-band · 10kΩ ±1%")
- **Rationale:** High contrast (13.5:1) ensures WCAG AAA compliance; above-anchor positioning avoids occluding the resistor itself

### 3. No Persistent Chrome
- Camera view fills 100% of screen
- No toolbars, no bottom bars, no floating buttons
- Overlays appear only as AR badges anchored to detected resistors
- **Rationale:** Every pixel of chrome is a pixel stolen from the user's view; the app should stay invisible

### 4. Support Dialog Tone: Respectful WinRAR-Style
- Message: "If this app has been useful, consider supporting its development."
- Two actions: "Support" (primary) and "Maybe Later" (secondary)
- Dismiss by: tapping "Maybe Later", tapping outside dialog, or hardware back
- **Rationale:** Users tolerate awareness-ware that respects their time; guilt-tripping creates resentment

### 5. Portrait-Only Orientation
- No landscape support
- **Rationale:** Landscape adds overlay positioning complexity with little value; resistors are typically viewed in portrait

### 6. No Active Detection Indicators
- No spinners, no "searching" text, no loading states visible to user
- Absence of overlay badges implies no resistors detected
- **Rationale:** The app should feel instant; detection state is self-evident

## Document Location

Full guidelines: `docs/design-guidelines.md`

## Assumptions Requiring Validation

- ML output includes bounding box coordinates, resistor type, value, tolerance, and confidence score
- Low confidence threshold is configurable (assumed default: 0.7)
- Detection updates at near-frame-rate (at least 10fps)

## Dependencies

- Shuri: Implementation in XAML
- Bruce: ML output format validation
