# App Store Screenshots Specification

This document specifies the screenshots required for iOS App Store and Google Play Store listings, including device sizes, content, and overlay text.

---

## iOS App Store Requirements

### Required Device Sizes

| Display Size | Resolution | Device Examples |
|-------------|------------|-----------------|
| 6.9" | 1320 × 2868 px | iPhone 16 Pro Max |
| 6.5" | 1284 × 2778 px | iPhone 15 Plus, 14 Plus, 13 Pro Max |
| 5.5" | 1242 × 2208 px | iPhone 8 Plus (legacy) |
| 12.9" iPad Pro | 2048 × 2732 px | iPad Pro 12.9" (6th gen) |

**Note:** App Store Connect allows using larger screenshots to auto-scale for smaller devices. Prioritize 6.9" and 12.9" iPad as primary captures.

### Screenshot Order & Content

| # | Screen State | Overlay Headline | Overlay Subtext | Notes |
|---|-------------|------------------|-----------------|-------|
| 1 | **Hero shot** — Camera view with single resistor detected, AR badge showing "4.7kΩ ±5%" | "Point. See. Know." | "Instant resistor identification" | Use a clean, well-lit resistor on a white/neutral background. Badge should be prominent and readable. |
| 2 | **Multiple resistors** — Camera view with 3-4 resistors detected simultaneously, each with its own AR badge | "Multiple Resistors" | "Detect several at once" | Show variety: different values (100Ω, 10kΩ, 470kΩ). Badges should not overlap. |
| 3 | **Real-world context** — Resistor detected on a populated PCB or breadboard | "Works Anywhere" | "From bench to breadboard" | Demonstrates practical use case. Background should be realistic but not cluttered. |
| 4 | **Confidence indicator** — Close-up showing badge with value + tolerance + optional confidence | "Accurate Results" | "Value, tolerance, and confidence" | Highlight the detail provided. If confidence indicator not in v1.0, use just value + tolerance. |
| 5 | **Offline indicator** — Camera view working with airplane mode visible (or "No internet required" badge) | "100% Offline" | "No internet. No cloud. No waiting." | Emphasize privacy/offline capability. Could show device status bar with airplane mode. |
| 6 | **Support modal** — The friendly support dialog appearing over the (dimmed) camera view | "Made by One Person" | "Fully functional. Forever." | Shows the non-intrusive monetization. Dialog should be clearly dismissible. |

### iOS Screenshot Guidelines

- **Orientation:** Portrait only (per design decision)
- **Status bar:** Include or use a clean device frame — Apple permits both
- **Safe areas:** Keep key content away from notch/Dynamic Island area
- **Text overlays:** Use app's typography (Inter or SF Pro), white text on semi-transparent dark pills for readability over camera content
- **Localization consideration:** If localizing later, use separate text layers in design files

---

## Google Play Store Requirements

### Required Device Sizes

| Device Type | Aspect Ratio | Resolution | Notes |
|-------------|--------------|------------|-------|
| Phone | 16:9 or taller | 1080 × 1920 px minimum | Most common; prioritize this |
| 7" Tablet | 16:10 | 1200 × 1920 px | Optional but recommended |
| 10" Tablet | 16:10 | 1600 × 2560 px | Optional but recommended |

**Note:** Google Play requires at least 2 screenshots; maximum 8. Phone screenshots can be reused for tablets if aspect ratio is similar.

### Screenshot Order & Content

| # | Screen State | Overlay Headline | Overlay Subtext | Notes |
|---|-------------|------------------|-----------------|-------|
| 1 | **Hero shot** — Single resistor with AR badge | "Instant Resistor ID" | "Point your camera, see the value" | Same concept as iOS #1. Adapt framing for Android device bezels. |
| 2 | **Multiple resistors** — 3-4 resistors detected | "Detect Multiple" | "Handle a handful at once" | Same concept as iOS #2. |
| 3 | **Real-world context** — PCB or breadboard | "Real-World Ready" | "Your bench, your lighting" | Same concept as iOS #3. |
| 4 | **Offline emphasis** — Camera working with no connection | "No Internet Needed" | "Works offline, everywhere" | Android users may be more sensitive to data usage. Emphasize this. |
| 5 | **Speed/simplicity** — App opening or mid-detection | "Fast & Focused" | "No setup. No accounts." | Show simplicity of UX. Could be a "just opened" state. |
| 6 | **Support modal** — Friendly dialog over camera | "Indie-Made, No Tricks" | "Support optional. Features? All yours." | Same concept as iOS #6. |

### Android Screenshot Guidelines

- **Orientation:** Portrait only
- **Device frames:** Optional; Google Play displays screenshots without frames in most contexts
- **Material You:** If app adopts dynamic theming later, consider showing Material You integration
- **Text overlays:** Use Roboto or app's Inter font. White text with dark pills/shadows for contrast.

---

## Tablet Screenshots (iOS & Android)

For iPad Pro 12.9" and 10" Android tablets, adapt the phone screenshots with these considerations:

| Adaptation | Guidance |
|------------|----------|
| **Layout** | Centered composition; more whitespace is acceptable on tablets |
| **Badge size** | Badges may render larger on tablets per design spec; ensure they're readable but not oversized in screenshots |
| **Text overlays** | Scale headline/subtext proportionally (larger absolute size) |
| **Content** | Same 6 screens as phone; re-capture at tablet resolution rather than scaling |

---

## Overlay Text Style Guide

### Typography
- **Headline:** Inter Bold (or SF Pro Bold for iOS), 48–64pt depending on device size
- **Subtext:** Inter Regular, 24–32pt, ~80% opacity white
- **Background pill:** #000000 at 70% opacity, 16dp corner radius, 24dp padding

### Placement
- **Headlines:** Lower third of screen (above home indicator area)
- **Subtext:** Directly below headline, 8dp gap
- **Avoid:** Placing text over the AR badges or detected resistors

### Color
- **Text:** #FFFFFF (white)
- **Accent (if highlighting a word):** #4FC3F7 (app's signature cyan)

---

## Capture Checklist

Before capturing, ensure:

- [ ] Test device has clean status bar (minimal icons, full battery, good signal or airplane mode as appropriate)
- [ ] App is in "fresh" state (no debug overlays, no development watermarks)
- [ ] Resistors used are clean, clearly colored, and represent a variety of values
- [ ] Lighting is good — matches the app's expected real-world use
- [ ] AR badges are positioned cleanly (not clipped by screen edges)
- [ ] Support modal screenshot uses post-trial state to trigger the dialog

---

## File Naming Convention

```
{platform}-{device}-{number}-{description}.png

Examples:
ios-6.9in-01-hero-single-resistor.png
ios-12.9in-ipad-03-pcb-context.png
android-phone-02-multiple-resistors.png
android-10in-tablet-05-fast-focused.png
```

---

## Assumptions & Notes

1. **Screenshot count** — 6 screenshots recommended for both platforms. This balances completeness with attention span. Apple allows up to 10; Google allows 8.

2. **No video for v1.0** — App Preview (iOS) and Promo Video (Android) not specified for initial launch. Consider adding post-launch once user flows are finalized.

3. **Localization** — Specs are for English (US). If localizing, screenshot text overlays need translation. Recommend maintaining layered design files (Figma/Sketch) with text as separate layers.

4. **Resistor props** — Recommend sourcing a resistor variety pack for photo-realistic captures: 100Ω, 1kΩ, 4.7kΩ, 10kΩ, 47kΩ, 100kΩ, 1MΩ. Mix 4-band and 5-band.

5. **Device frames** — Apple and Google both offer official device frames/mockup tools. Use these for marketing materials but not for App Store uploads (where raw screenshots are expected).

6. **Support modal timing** — To capture the support modal screenshot, advance the device date past the 7-day trial period or use a debug flag if available.
