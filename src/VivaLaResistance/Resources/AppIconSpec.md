# VivaLaResistance — App Icon & Splash Screen Design Spec

**Version:** 1.0  
**Author:** Hope (UX Designer)  
**Date:** 2026-07-18  
**For:** Shuri (MAUI Implementation)  
**Related Issue:** #30

---

## 1. Design Philosophy

The app icon and splash screen must:
- **Communicate purpose instantly** — electronics, precision, technical utility
- **Feel approachable** — not cold/industrial; welcoming to hobbyists and students
- **Scale gracefully** — recognizable from 1024×1024 down to 20×20
- **Align with app palette** — dark-first theme with electric blue accent per design guidelines

---

## 2. App Icon Concept

### 2.1 Visual Description

**Primary Element: Stylized Resistor**

The icon features a simplified, stylized resistor viewed at a 3/4 angle, rendered in a modern flat design:

```
    ═══╡▮▮▮▮╞═══
        ↑
   Color bands (4 visible)
```

**Key Visual Elements:**
1. **Resistor body** — Rounded rectangle (pill shape), cream/tan color (#F5E6D3) reminiscent of classic carbon film resistors
2. **Color bands** — Four distinct bands representing a memorable value (see 2.2)
3. **Lead wires** — Simplified lines extending from each end, electric blue (#4FC3F7) to tie into the accent palette
4. **Background** — Dark near-black (#121212) matching the app's primary dark color

**Design Rationale:**
- The resistor is THE core object this app identifies — it's the perfect icon subject
- Color bands are the visual puzzle the app solves — showing them creates instant recognition for the target audience
- The cream/tan body color is universally recognized as "resistor" by anyone who's touched electronics
- Electric blue leads create visual interest and connect to the app's accent color

### 2.2 Color Band Values

For the icon, use bands representing **4.7kΩ (4700Ω)** — a common, memorable value:

| Band | Color | Hex | Meaning |
|------|-------|-----|---------|
| Band 1 | Yellow | `#FFD700` | 4 |
| Band 2 | Violet | `#8B00FF` | 7 |
| Band 3 | Red | `#FF0000` | ×100 |
| Band 4 | Gold | `#FFD700` | ±5% tolerance |

**Why 4.7kΩ?**
- Yellow, violet, red, gold is visually distinctive and colorful
- 4.7kΩ is one of the most common resistor values — users will recognize it
- The colors provide good contrast against the cream body

### 2.3 Icon Construction (SVG Reference)

```
┌─────────────────────────────────┐
│                                 │
│         ████████████████        │  ← Background: #121212
│         █              █        │
│   ══════█   ▮ ▮ ▮ ▮   █══════   │  ← Blue leads (#4FC3F7)
│         █              █        │     extend from center
│         ████████████████        │
│                                 │
│                                 │
└─────────────────────────────────┘

Legend:
████  = Resistor body (#F5E6D3, rounded corners)
▮ ▮ ▮ ▮ = Color bands (Yellow, Violet, Red, Gold)
══════ = Lead wires (#4FC3F7)
```

**Safe Zone:** Keep the resistor centered with 12.5% padding on all sides for platform icon masking (especially Android adaptive icons).

### 2.4 Scalability Considerations

| Size Range | Adaptation |
|------------|------------|
| **Large (512-1024px)** | Full detail — all 4 color bands clearly visible, rounded corners on body |
| **Medium (128-512px)** | Same design, no changes needed |
| **Small (48-128px)** | Bands may start to merge visually — acceptable |
| **Tiny (20-48px)** | Color bands become abstract stripes — still recognizable as "resistor with stripes" |

**Critical:** The icon must remain recognizable at 29×29px (iOS Settings icon). At this size, the shape (horizontal pill with colored stripes) carries the recognition, not the individual band colors.

### 2.5 iOS Considerations

- **No internal corner rounding** — iOS applies its own rounded rect mask
- **Design to the edge** — the system will clip; don't add your own padding beyond the safe zone
- **No transparency** — iOS icons must be opaque
- **Single layer** — iOS does not use adaptive icon layers

### 2.6 Android Adaptive Icon Considerations

Android adaptive icons require two separate layers that the system composites:

**Foreground Layer (`appiconfg.svg`):**
- Contains the resistor graphic only (body + bands + leads)
- Must be designed for a 108×108dp canvas with 72×72dp safe zone centered
- The outer 18dp on each side may be clipped by various device masks
- Transparent background

**Background Layer (`appicon.svg`):**
- Solid color fill: `#121212` (Primary Dark)
- Or subtle radial gradient: center `#1a1a2e` → edge `#121212` for depth

**Why not a pattern/texture background?**
The resistor is already detailed — a busy background would compete. Solid dark maintains focus and matches app identity.

---

## 3. Required Icon Sizes

### 3.1 MAUI Single-Project Approach

MAUI generates all required sizes from a single SVG source. You provide:
- `appicon.svg` — Background layer (or solid fill for iOS base)
- `appiconfg.svg` — Foreground layer (the resistor graphic)

MAUI's build process generates all platform-specific sizes automatically.

### 3.2 iOS Icon Sizes (Reference)

MAUI generates these from your SVG. Listed for QA verification:

| Size | Scale | Usage | Filename (generated) |
|------|-------|-------|---------------------|
| 20×20 | 1x | iPad Notifications | Icon-20.png |
| 40×40 | 2x | iPad Notifications | Icon-20@2x.png |
| 60×60 | 3x | iPad Notifications | Icon-20@3x.png |
| 29×29 | 1x | iPad Settings | Icon-29.png |
| 58×58 | 2x | iPhone Settings | Icon-29@2x.png |
| 87×87 | 3x | iPhone Settings | Icon-29@3x.png |
| 40×40 | 1x | iPad Spotlight | Icon-40.png |
| 80×80 | 2x | iPhone Spotlight | Icon-40@2x.png |
| 120×120 | 3x | iPhone Spotlight | Icon-40@3x.png |
| 60×60 | 1x | — | Icon-60.png |
| 120×120 | 2x | iPhone Home | Icon-60@2x.png |
| 180×180 | 3x | iPhone Home | Icon-60@3x.png |
| 76×76 | 1x | iPad Home | Icon-76.png |
| 152×152 | 2x | iPad Home | Icon-76@2x.png |
| 167×167 | 2x | iPad Pro Home | Icon-83.5@2x.png |
| 1024×1024 | 1x | App Store | Icon-1024.png |

### 3.3 Android Icon Sizes (Reference)

MAUI generates these from your SVG. Listed for QA verification:

**Adaptive Icon (API 26+):**

| Density | Size | Foreground | Background |
|---------|------|------------|------------|
| mdpi | 108×108 | ic_launcher_foreground.png | ic_launcher_background.png |
| hdpi | 162×162 | ic_launcher_foreground.png | ic_launcher_background.png |
| xhdpi | 216×216 | ic_launcher_foreground.png | ic_launcher_background.png |
| xxhdpi | 324×324 | ic_launcher_foreground.png | ic_launcher_background.png |
| xxxhdpi | 432×432 | ic_launcher_foreground.png | ic_launcher_background.png |

**Legacy Icon (pre-API 26) & Round Icon:**

| Density | Size | Legacy | Round |
|---------|------|--------|-------|
| mdpi | 48×48 | ic_launcher.png | ic_launcher_round.png |
| hdpi | 72×72 | ic_launcher.png | ic_launcher_round.png |
| xhdpi | 96×96 | ic_launcher.png | ic_launcher_round.png |
| xxhdpi | 144×144 | ic_launcher.png | ic_launcher_round.png |
| xxxhdpi | 192×192 | ic_launcher.png | ic_launcher_round.png |

**Play Store:**
| Size | Usage |
|------|-------|
| 512×512 | Play Store listing |

---

## 4. Splash Screen Design

### 4.1 Design Philosophy

The splash screen should:
- **Load fast** — MAUI `MauiSplashScreen` is a simple color + centered image
- **Feel cohesive** — match the app's dark-first palette
- **Be minimal** — no taglines, no loading bars, just brand identity
- **Transition smoothly** — the dark background matches the camera view's chrome

### 4.2 Visual Description

```
┌─────────────────────────────────┐
│                                 │
│                                 │
│                                 │
│                                 │
│         ═══╡▮▮▮▮╞═══            │  ← Same resistor icon
│                                 │     (centered, 128dp base)
│                                 │
│                                 │
│                                 │
│                                 │
└─────────────────────────────────┘
     Background: #121212
```

**Elements:**
- **Background:** `#121212` (Primary Dark)
- **Icon:** The same resistor graphic from the app icon, centered
- **Size:** 128×128dp base size (MAUI scales appropriately)
- **No tagline:** The app name appears in the icon label on home screen; no need to repeat it on splash

### 4.3 Why No Tagline?

1. **Speed perception** — text takes time to read; if the splash is fast (as it should be), users won't read it anyway
2. **Localization** — taglines need translation; the icon is universal
3. **Simplicity** — per design principles, "no learning curve"

### 4.4 Color Specification

| Element | Hex | Notes |
|---------|-----|-------|
| Splash Background | `#121212` | Matches app's Primary Dark |
| Icon graphic | (embedded in SVG) | Resistor with colored bands |

---

## 5. MAUI Project Configuration

### 5.1 File Structure

```
src/VivaLaResistance/Resources/
├── AppIcon/
│   ├── appicon.svg          ← Background layer (solid #121212)
│   └── appiconfg.svg         ← Foreground layer (resistor graphic)
├── Splash/
│   └── splash.svg            ← Centered resistor on transparent bg
└── Images/
    └── (other app images)
```

### 5.2 Required SVG Files

**`Resources/AppIcon/appicon.svg`** — Background Layer
```xml
<?xml version="1.0" encoding="UTF-8"?>
<svg viewBox="0 0 456 456" xmlns="http://www.w3.org/2000/svg">
  <rect x="0" y="0" width="456" height="456" fill="#121212"/>
</svg>
```

**`Resources/AppIcon/appiconfg.svg`** — Foreground Layer

This SVG should contain the resistor graphic:
- Canvas: 456×456 (matches MAUI default)
- Resistor body: centered horizontally, ~60% of canvas width
- Safe zone: keep content within center 72% (328×328) for Android adaptive masking
- Elements: body pill shape (#F5E6D3), four color bands, lead wires (#4FC3F7)

**`Resources/Splash/splash.svg`** — Splash Icon

Same resistor graphic as foreground, on transparent background:
- Canvas: 456×456
- Resistor centered
- No background fill (MAUI applies background color separately)

### 5.3 .csproj Configuration

Update `VivaLaResistance.csproj`:

```xml
<ItemGroup>
  <!-- App Icon -->
  <MauiIcon Include="Resources\AppIcon\appicon.svg" 
            ForegroundFile="Resources\AppIcon\appiconfg.svg" 
            Color="#121212" />

  <!-- Splash Screen -->
  <MauiSplashScreen Include="Resources\Splash\splash.svg" 
                    Color="#121212" 
                    BaseSize="128,128" />
</ItemGroup>
```

**Configuration Notes:**
- `Color="#121212"` — replaces the placeholder purple (#512BD4) with our Primary Dark
- `BaseSize="128,128"` — splash icon renders at 128dp, centered (MAUI scales for density)
- `ForegroundFile` — enables Android adaptive icon with separate foreground layer

### 5.4 Platform-Specific Overrides

**None required.** MAUI's single-project approach handles all platform variations automatically.

If platform-specific tweaks are needed later (e.g., different tint for iOS), use:

```xml
<!-- Example: iOS-specific override (not currently needed) -->
<MauiIcon Condition="$(TargetFramework.Contains('-ios'))" 
          Include="Resources\AppIcon\appicon-ios.svg" />
```

---

## 6. SVG Implementation Guide for Shuri

### 6.1 Foreground SVG Structure

```xml
<?xml version="1.0" encoding="UTF-8"?>
<svg viewBox="0 0 456 456" xmlns="http://www.w3.org/2000/svg">
  <!-- Lead wires (behind body) -->
  <line x1="48" y1="228" x2="138" y2="228" 
        stroke="#4FC3F7" stroke-width="12" stroke-linecap="round"/>
  <line x1="318" y1="228" x2="408" y2="228" 
        stroke="#4FC3F7" stroke-width="12" stroke-linecap="round"/>
  
  <!-- Resistor body -->
  <rect x="128" y="178" width="200" height="100" rx="20" ry="20" 
        fill="#F5E6D3"/>
  
  <!-- Color bands -->
  <rect x="158" y="178" width="24" height="100" fill="#FFD700"/>  <!-- Yellow (4) -->
  <rect x="198" y="178" width="24" height="100" fill="#8B00FF"/>  <!-- Violet (7) -->
  <rect x="238" y="178" width="24" height="100" fill="#FF0000"/>  <!-- Red (×100) -->
  <rect x="288" y="178" width="24" height="100" fill="#D4AF37"/>  <!-- Gold (±5%) -->
</svg>
```

**Coordinate Notes:**
- Canvas: 456×456 (MAUI default)
- Body: 200×100, centered at (228, 228)
- Bands: 24px wide each, evenly spaced
- Leads: extend 90px from body edge
- Safe zone for Android: content within ~64-392 (328px inner area)

### 6.2 Visual Refinements (Optional)

For added polish, Shuri may consider:
- **Subtle gradient on body:** lighter at top (#FAF0E6) to darker at bottom (#EED9C4) for depth
- **Band shadows:** 2px darker line on right edge of each band
- **Lead wire terminals:** small circles at ends

These are enhancements, not requirements. The flat design works well at all sizes.

---

## 7. Dark Mode Considerations

### 7.1 App Icon

**No dark mode variant needed.**

The icon already uses a dark background (#121212). It will appear correctly in both light and dark system themes:
- On light backgrounds: dark icon provides excellent contrast
- On dark backgrounds: the colored resistor and blue leads stand out

### 7.2 Splash Screen

**No dark mode variant needed.**

The splash uses the same dark background as the app chrome. Users transitioning from splash to camera view will experience a seamless dark → dark transition.

---

## 8. Accessibility Checklist

| Check | Status | Notes |
|-------|--------|-------|
| Icon recognizable at 20×20 | ✓ | Pill shape + colored stripes visible |
| No text in icon | ✓ | Avoids readability issues at small sizes |
| Color not sole identifier | ✓ | Shape (resistor) conveys meaning even in grayscale |
| Sufficient contrast | ✓ | Cream body on dark bg: >10:1 contrast |

---

## 9. QA Verification Checklist

After implementation, verify:

- [ ] Icon displays correctly in iOS Simulator (Settings, Home, Spotlight)
- [ ] Icon displays correctly in Android Emulator (Home, App Drawer)
- [ ] Android adaptive icon shape masks correctly (circle, squircle, rounded square)
- [ ] Splash screen background matches app chrome (#121212)
- [ ] Splash icon is centered and appropriately sized
- [ ] No visible artifacts at any icon size
- [ ] App Store / Play Store screenshots show correct icon

---

## 10. Implementation Summary for Shuri

| Task | File | Action |
|------|------|--------|
| 1 | `Resources/AppIcon/appicon.svg` | Replace with solid `#121212` background |
| 2 | `Resources/AppIcon/appiconfg.svg` | Replace with resistor graphic (per Section 6.1) |
| 3 | `Resources/Splash/splash.svg` | Replace with resistor graphic on transparent bg |
| 4 | `VivaLaResistance.csproj` | Update `MauiIcon` and `MauiSplashScreen` Color to `#121212` |
| 5 | Build & Test | Verify on iOS Simulator + Android Emulator |

---

## Appendix A: Color Reference

| Name | Hex | Usage |
|------|-----|-------|
| Primary Dark | `#121212` | Icon bg, splash bg |
| Electric Blue | `#4FC3F7` | Lead wires, accent |
| Resistor Body | `#F5E6D3` | Cream/tan body |
| Band Yellow | `#FFD700` | First digit (4) |
| Band Violet | `#8B00FF` | Second digit (7) |
| Band Red | `#FF0000` | Multiplier (×100) |
| Band Gold | `#D4AF37` | Tolerance (±5%) |

---

## Appendix B: Asset Checklist

Files to create:
- [ ] `Resources/AppIcon/appicon.svg` — dark background
- [ ] `Resources/AppIcon/appiconfg.svg` — resistor foreground
- [ ] `Resources/Splash/splash.svg` — splash icon

Files to update:
- [ ] `VivaLaResistance.csproj` — change Color values to `#121212`

---

*End of Spec*
