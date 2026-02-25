# VivaLaResistance — UX Design Guidelines

**Version:** 1.1  
**Author:** Hope (UX Designer)  
**Last Updated:** 2026-02-25

---

## 1. App Overview & Design Philosophy

### What This App Is
VivaLaResistance is a mobile utility app for electronics hobbyists, students, and professionals who need to quickly identify resistor values. The user points their phone camera at one or more resistors, and the app overlays the calculated value (e.g., "10kΩ ±5%") directly next to each detected component in real-time.

### Who Uses It
- Electronics hobbyists building circuits
- Students learning about resistors
- Professionals doing quick checks without multimeters
- Anyone who can never remember the color band codes

### Core UX Principles

| Principle | Description |
|-----------|-------------|
| **Speed** | The app should feel instant. Camera launches immediately, detection happens in real-time, overlays appear without delay. |
| **Legibility** | Overlay text must be readable in any lighting condition, over any background, at arm's length. |
| **Non-Intrusiveness** | The UI should stay out of the way. This is a camera app — every pixel of chrome is a pixel stolen from the user's view. |
| **Respect** | The monetization prompt must feel like a polite ask from a friend, not a guilt trip or dark pattern. |
| **Simplicity** | No settings. No menus. No learning curve. Point and read. |

---

## 2. Permissions

### Required Permissions

| Platform | Permission | Purpose |
|----------|------------|---------|
| **iOS** | `NSCameraUsageDescription` | Live camera feed for resistor detection |
| **Android** | `android.permission.CAMERA` | Live camera feed for resistor detection |

### Explicitly Excluded Permissions

The following permissions are **intentionally not requested** — the app operates on live camera only:

| Platform | Excluded Permission | Reason |
|----------|---------------------|--------|
| **iOS** | `NSPhotoLibraryUsageDescription` | No gallery access — live camera only |
| **iOS** | `NSMicrophoneUsageDescription` | Audio not required |
| **Android** | `READ_MEDIA_IMAGES` | No gallery access — live camera only |
| **Android** | `READ_EXTERNAL_STORAGE` | No gallery access — live camera only |
| **Android** | `RECORD_AUDIO` | Audio not required |

### Permission Request UX

The permission request should be minimal and direct:

| Element | Specification |
|---------|---------------|
| **iOS Usage Description** | "VivaLaResistance needs camera access to detect resistors." |
| **Android Rationale** | Same text if showing a rationale dialog |
| **Timing** | Request on first camera screen launch, not on app open |
| **Denied State** | Show centered message: "Camera access is required to use this app. Please enable in Settings." with a "Open Settings" button |

### Design Principle: No Gallery

There is **no "pick from gallery" button** anywhere in the app. The app:
- Never shows a gallery picker
- Never prompts to select existing photos
- Never suggests importing images
- Operates exclusively on the live camera feed

This simplifies the UX, reduces permission scope, and keeps the app focused on its core real-time use case.

---

## 3. Responsive Layout & Screen Adaptations

### Device Classes

The app must adapt to three primary device classes:

| Device Class | Screen Width | Aspect Ratio | Primary Use |
|--------------|--------------|--------------|-------------|
| **Standard Phone** | 360-420dp | ~16:9 to ~20:9 | Portrait only |
| **Samsung Galaxy Fold (Outer)** | ~360dp | ~24:9 (very narrow) | Portrait, cramped |
| **Samsung Galaxy Fold (Inner)** | ~720dp | ~4:3 (near-square) | Portrait or landscape |
| **Tablet** | 600dp+ | ~4:3 to ~16:10 | Landscape-friendly |

### Breakpoint Definitions

For Shuri's MAUI implementation, use these breakpoints:

| Breakpoint | Width Range | Layout Mode |
|------------|-------------|-------------|
| **Compact** | < 400dp | Minimal UI, compact badges |
| **Standard** | 400-599dp | Normal phone layout |
| **Expanded** | ≥ 600dp | Tablet/large screen layout |

**MAUI Implementation Note:** Use `DeviceDisplay.Current.MainDisplayInfo` to get screen dimensions. Subscribe to window `SizeChanged` events to detect Fold open/close transitions. The `Window.Width` and `Window.Height` properties provide current window size in device-independent units.

### Standard Phone Layout

This is the baseline layout documented in Section 6. No modifications needed for standard phones.

```
┌────────────────────────┐
│ ░░░ Status bar ░░░░░░░ │  ← System status bar
│                        │
│   [LIVE CAMERA FEED]   │
│                        │
│      ┌──────────────┐  │
│      │ 5-band·10kΩ  │  │  ← Full badge, standard size
│      └──────────────┘  │
│      ┌──────────────┐  │
│      │ [resistor]   │  │
│      └──────────────┘  │
│                        │
│                        │
│                        │
│ ░░░ Home indicator ░░░ │  ← iOS home indicator
└────────────────────────┘
         ~380dp
```

### Samsung Galaxy Fold — Outer Screen (Compact Mode)

The Fold's outer cover screen is extremely narrow (~360dp × ~800dp, ~24:9). This requires a cramped, minimal UI:

**Adaptations:**
- Badges use **compact format**: value only, no type prefix
- Badge font size reduced to **14sp** (from 16sp)
- Badge horizontal padding reduced to **6dp** (from 8dp)
- Maximum **2 badges visible** at once (others queued)
- Badges anchor to left/right edges if centered badge would overflow

```
┌──────────────────┐
│ ░░ Status bar ░░ │  ← System status bar
│                  │
│  [CAMERA FEED]   │
│                  │
│   ┌──────────┐   │
│   │ 10kΩ ±1% │   │  ← Compact badge (no type)
│   └──────────┘   │
│   ┌──────────┐   │
│   │[resistor]│   │
│   └──────────┘   │
│                  │
│   ┌──────────┐   │
│   │ 470Ω ±5% │   │  ← Max 2 badges visible
│   └──────────┘   │
│   ┌──────────┐   │
│   │[resistor]│   │
│   └──────────┘   │
│                  │
│ ░░ Home ind. ░░░ │
└──────────────────┘
       ~360dp
```

### Samsung Galaxy Fold — Inner Screen (Expanded Mode)

The Fold's inner screen unfolds to ~720dp width with a near-square aspect ratio. This provides ample space:

**Adaptations:**
- Use **full badge format** with type prefix
- Badge font size can increase to **18sp** for better readability
- Up to **5 badges** visible simultaneously
- Badges have more horizontal breathing room
- Camera view fills the full screen (may letterbox slightly depending on camera aspect)

```
┌─────────────────────────────────────────────────┐
│ ░░░░░░░░░░░░░░░ Status bar ░░░░░░░░░░░░░░░░░░░░ │
│                                                 │
│                                                 │
│              [LIVE CAMERA FEED]                 │
│                                                 │
│                                                 │
│         ┌─────────────────────┐                 │
│         │ 5-band · 10kΩ ±1%   │                 │  ← Full badge, larger text
│         └─────────────────────┘                 │
│         ┌─────────────────────┐                 │
│         │     [resistor A]    │                 │
│         └─────────────────────┘                 │
│                                                 │
│    ┌─────────────────────┐  ┌─────────────────────┐
│    │ 4-band · 470Ω ±5%   │  │ 5-band · 2.2kΩ ±1%  │
│    └─────────────────────┘  └─────────────────────┘
│    ┌─────────────────────┐  ┌─────────────────────┐
│    │   [resistor B]      │  │    [resistor C]     │
│    └─────────────────────┘  └─────────────────────┘
│                                                 │
│ ░░░░░░░░░░░░░░░ Home indicator ░░░░░░░░░░░░░░░░ │
└─────────────────────────────────────────────────┘
                      ~720dp
```

### Tablet Layout (Expanded Mode)

Tablets (600dp+ width) get similar treatment to the Fold's inner screen, with additional considerations:

**Adaptations:**
- **Full badge format** with type prefix
- Badge font size **18sp**
- Up to **6+ badges** visible simultaneously
- Camera view may be **letterboxed** to maintain aspect ratio on very wide screens
- Consider **landscape support** on tablets (optional, may be implemented later)
- More generous margins (24dp from screen edges)

```
┌──────────────────────────────────────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░ Status bar ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│                                                              │
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│    ░░░░░░░░░░░░░░ [CAMERA FEED] ░░░░░░░░░░░░░░░░░░░░░░░░░    │  ← May letterbox
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│              ┌────────────────────┐                          │
│              │ 5-band · 10kΩ ±1%  │                          │  ← Large badges
│              └────────────────────┘                          │
│              ┌────────────────────┐                          │
│              │    [resistor A]    │                          │
│              └────────────────────┘                          │
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│    ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    │
│                                                              │
└──────────────────────────────────────────────────────────────┘
                            ~800dp+
```

### Support Dialog — Responsive Behavior

The Support Dialog adapts based on screen width:

| Screen Width | Dialog Presentation |
|--------------|---------------------|
| **< 600dp** (Compact/Standard) | **Bottom sheet** — slides up from bottom, full width, rounded top corners |
| **≥ 600dp** (Expanded) | **Centered modal** — 85% width (max 400dp), centered vertically |

#### Bottom Sheet Layout (Phones, Fold Outer)

```
┌─────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░ (dimmed) ░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
├─────────────────────────────┤  ← Rounded top corners (16dp)
│         ── handle ──        │  ← Drag handle (40dp × 4dp)
│                             │
│          [ICON]             │
│                             │
│     VivaLaResistance        │
│                             │
│   If this app has been      │
│   useful, consider          │
│   supporting it.            │
│                             │
│   ┌─────────────────────┐   │
│   │      Support        │   │
│   └─────────────────────┘   │
│                             │
│      Maybe Later            │
│                             │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Safe area padding
└─────────────────────────────┘
```

#### Centered Modal Layout (Tablets, Fold Inner)

```
┌─────────────────────────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░┌─────────────────────────┐░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│         [ICON]          │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│    VivaLaResistance     │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│   If this app has been  │░░░░░░░░░░░░ │
│ ░░░░░░░░│   useful, consider      │░░░░░░░░░░░░ │
│ ░░░░░░░░│   supporting it.        │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│  ┌───────────────────┐  │░░░░░░░░░░░░ │
│ ░░░░░░░░│  │      Support      │  │░░░░░░░░░░░░ │
│ ░░░░░░░░│  └───────────────────┘  │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│      Maybe Later        │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░└─────────────────────────┘░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
└─────────────────────────────────────────────────┘
                max 400dp wide
```

### Responsive Badge Specifications

| Property | Compact (<400dp) | Standard (400-599dp) | Expanded (≥600dp) |
|----------|------------------|----------------------|-------------------|
| **Format** | Value + tolerance only | Type · Value ± Tolerance | Type · Value ± Tolerance |
| **Font Size** | 14sp | 16sp | 18sp |
| **Padding H** | 6dp | 8dp | 10dp |
| **Padding V** | 4dp | 6dp | 8dp |
| **Max Visible** | 2 | 3 | 5-6 |
| **Min Width** | 60dp | 80dp | 100dp |

### Fold Transition Handling

When the user opens/closes the Samsung Galaxy Fold, the app must handle the transition gracefully:

| Transition | Behavior |
|------------|----------|
| **Outer → Inner** (unfolding) | Smoothly scale camera preview and badges to fill new space. Badges transition from compact to full format over 200ms. |
| **Inner → Outer** (folding) | Smoothly scale down. Truncate badges to compact format. If >2 resistors detected, show only the 2 most recently detected. |

**Implementation Note:** Subscribe to `Window.SizeChanged` and `DeviceDisplay.MainDisplayInfoChanged` events. When width crosses the 400dp threshold, trigger badge format change.

---

## 4. Color Palette

### Design Rationale
Camera apps work better with dark UI chrome — it reduces glare, improves contrast with the camera feed, and is less distracting in varied lighting conditions.

### Primary Colors

| Role | Color | Hex | Usage |
|------|-------|-----|-------|
| **Primary Dark** | Near Black | `#121212` | App chrome, dialog backgrounds |
| **Primary Accent** | Electric Blue | `#4FC3F7` | Primary buttons, interactive elements |
| **Secondary Accent** | Warm Amber | `#FFB74D` | Secondary highlights, warnings |
| **Success** | Soft Green | `#81C784` | Detection confirmed, positive states |
| **Error** | Soft Red | `#E57373` | Error states |

### Overlay Colors

| Role | Color | Hex | Usage |
|------|-------|-----|-------|
| **Badge Background** | Semi-transparent Dark | `#000000` at 80% opacity | Overlay badge backing |
| **Badge Text** | Pure White | `#FFFFFF` | Primary overlay text |
| **Badge Border** | Subtle Light | `#FFFFFF` at 30% opacity | Optional badge outline for extra contrast |

### Accessibility Notes
- Badge text (#FFFFFF) on badge background (#000000 at 80%) achieves **13.5:1 contrast ratio**, exceeding WCAG AAA (7:1)
- Primary accent (#4FC3F7) on dark background (#121212) achieves **8.2:1 contrast ratio**, exceeding WCAG AA (4.5:1)
- All interactive text must meet WCAG AA minimum (4.5:1 for normal text, 3:1 for large text)

---

## 5. Typography

### Font Choices

| Context | Font Family | Rationale |
|---------|-------------|-----------|
| **Overlay Text** | System Default Bold (San Francisco on iOS, Roboto on Android) | Maximum legibility, native rendering, no custom font loading |
| **Dialog Text** | System Default | Consistency with platform conventions |

### Type Scale

| Element | Size | Weight | Line Height |
|---------|------|--------|-------------|
| **Resistor Value** (main overlay) | 16sp / 16pt | Bold (700) | 1.2 |
| **Resistor Type** (secondary overlay) | 12sp / 12pt | Medium (500) | 1.2 |
| **Dialog Title** | 20sp / 20pt | Bold (700) | 1.3 |
| **Dialog Body** | 16sp / 16pt | Regular (400) | 1.5 |
| **Button Text** | 16sp / 16pt | SemiBold (600) | 1.0 |

### Overlay Readability
- Minimum 16sp for the primary value — must be readable at arm's length (approx. 60cm)
- Bold weight to cut through visual noise from real-world backgrounds
- No italics, no thin weights — legibility trumps aesthetics

---

## 6. Main Screen Layout

> **Note:** This section describes the **standard phone layout**. For responsive variants (Samsung Fold, tablets), see **Section 3: Responsive Layout & Screen Adaptations**.

### Layout Principles
- **100% camera view** — the camera feed fills the entire screen, edge to edge
- **No persistent chrome** — no toolbars, no bottom bars, no floating buttons
- **Overlays only** — UI elements appear only as AR badges anchored to detected resistors
- **Safe areas respected** — overlays should not render behind notches or system UI

### State Indicators

| State | Visual Indication |
|-------|-------------------|
| **Idle / Scanning** | No indication — the app is always scanning. No spinners, no "searching" text. |
| **Detected** | Badge appears next to resistor |
| **Low Confidence** | Badge appears with dashed border (see Component spec below) |
| **No Resistors** | Nothing shown — empty camera view. User understands implicitly. |

### ASCII Wireframe — Main Screen

```
┌─────────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Status bar (system)
│                                 │
│    [LIVE CAMERA FEED]           │
│                                 │
│        ┌───────────────┐        │
│        │ Resistor A    │        │
│        │ (detected)    │◄──┬────┼──── Overlay badge anchored
│        └───────────────┘   │    │     above bounding box
│                            │    │
│    ┌───────────────┐       │    │
│    │ Resistor B    │◄──────┘    │
│    │ (detected)    │            │
│    └───────────────┘            │
│                                 │
│                                 │
│                                 │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Home indicator (iOS)
└─────────────────────────────────┘
```

---

## 7. Resistor Value Overlay Component

> **Note:** Badge sizing and format adapt to screen width. See **Section 3: Responsive Badge Specifications** for breakpoint-specific values.

### Visual Specification

| Property | Value |
|----------|-------|
| **Shape** | Rounded rectangle (8dp corner radius) |
| **Background** | `#000000` at 80% opacity |
| **Border** | 1dp, `#FFFFFF` at 30% opacity (optional, for extra contrast) |
| **Drop Shadow** | None (keeps rendering simple, avoids visual noise) |
| **Padding** | 8dp horizontal, 6dp vertical |
| **Min Width** | 80dp (prevents badges from looking cramped) |

### Content Layout

```
┌────────────────────────────┐
│ 5-band · 10kΩ ±1%          │
└────────────────────────────┘
  ↑                ↑      ↑
  Type            Value  Tolerance
```

- **Format:** `{type} · {value} {tolerance}`
- **Examples:**
  - `4-band · 470Ω ±5%`
  - `5-band · 10kΩ ±1%`
  - `6-band · 2.2MΩ ±0.1%`
- **Typography:** Single line, system bold, 16sp for value portion

### Anchor Point
- Badge anchors **above** the detected resistor bounding box, **horizontally centered**
- 8dp gap between bottom of badge and top of bounding box
- If resistor is near top of screen, badge flips to **below** the bounding box

```
        ┌───────────────┐
        │ 4-band · 470Ω │  ← Badge
        └───────┬───────┘
                │ 8dp gap
        ┌───────▼───────┐
        │ [resistor]    │  ← Bounding box (not visible to user)
        └───────────────┘
```

### Overlap Handling
When multiple resistors are close together:

1. **Primary strategy:** Stack badges vertically with 4dp gap
2. **Secondary strategy:** If stacking causes badges to exit screen bounds, anchor badges to left/right sides instead of center
3. **Edge case:** If more than 3 resistors overlap in a small area, show maximum 3 badges; additional resistors are detected but not labeled until others leave frame

```
        ┌───────────────┐
        │ 5-band · 10kΩ │
        ├───────────────┤  4dp gap
        │ 4-band · 470Ω │
        └───────────────┘
        ┌───────────────┐
        │ [2 resistors] │
        └───────────────┘
```

### Animation

| Event | Animation |
|-------|-----------|
| **Badge appears** | Fade in over 150ms (ease-out) |
| **Badge updates** | Cross-fade value text over 100ms |
| **Badge disappears** | Fade out over 100ms |
| **Badge repositions** | Smooth translate over 100ms (ease-in-out) |

### Low Confidence State
When detection confidence is below threshold (see Assumptions):
- Badge border becomes **dashed** (2dp dash, 2dp gap)
- Badge text appends "?" → `4-band · 470Ω ±5%?`
- No other visual difference — keep it subtle

---

## 8. Support Dialog

> **Note:** The dialog presentation adapts to screen width: bottom sheet on phones (<600dp), centered modal on tablets (≥600dp). See **Section 3: Support Dialog — Responsive Behavior** for details.

### Design Philosophy
This dialog appears every time the app launches after the 7-day trial expires. It must:
- Feel respectful — a polite ask, not a demand
- Be easy to dismiss — no dark patterns, no hiding the "no thanks" option
- Be brief — user wants to get to the camera, not read paragraphs
- Not guilt-trip — no sad faces, no countdown timers, no "you're hurting developers"

### Tone Examples

**Good:**
> "You've been using VivaLaResistance — if it's useful, consider supporting it."

**Bad:**
> "You've used this app 47 times without paying. Don't you think that's unfair?"

### Visual Specification

| Property | Value |
|----------|-------|
| **Background** | `#121212` |
| **Corner Radius** | 16dp |
| **Width** | 85% of screen width, max 320dp |
| **Shadow** | Subtle elevation (8dp shadow, 20% opacity black) |
| **Backdrop** | Dim overlay (`#000000` at 50% opacity) over camera preview |

### Content Layout

| Element | Specification |
|---------|---------------|
| **App Icon** | 48x48dp, centered at top, 24dp from top edge |
| **Title** | "VivaLaResistance" — 20sp bold, centered, 16dp below icon |
| **Message** | "If this app has been useful, consider supporting its development." — 16sp regular, centered, 16dp below title, max 2 lines |
| **Primary Button** | "Support" — filled, Electric Blue (`#4FC3F7`), 48dp height, full width minus 24dp margins, 24dp below message |
| **Secondary Button** | "Maybe Later" — text only, white, 48dp touch target, 8dp below primary button, 24dp from bottom edge |

### ASCII Wireframe — Support Dialog

```
┌─────────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░ (dimmed camera) ░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░┌─────────────────────┐░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░│        [ICON]       │░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░│   VivaLaResistance  │░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░│  If this app has    │░░░░░ │
│ ░░░│  been useful,       │░░░░░ │
│ ░░░│  consider           │░░░░░ │
│ ░░░│  supporting it.     │░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░│ ┌─────────────────┐ │░░░░░ │
│ ░░░│ │    Support      │ │░░░░░ │
│ ░░░│ └─────────────────┘ │░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░│    Maybe Later      │░░░░░ │
│ ░░░│                     │░░░░░ │
│ ░░░└─────────────────────┘░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
└─────────────────────────────────┘
```

### Behavior

| Action | Result |
|--------|--------|
| Tap "Support" | Navigate to app store / payment flow (implementation TBD) |
| Tap "Maybe Later" | Dialog dismisses, camera screen shown |
| Tap outside dialog | Dialog dismisses, camera screen shown |
| Hardware back (Android) | Dialog dismisses, camera screen shown |
| Swipe down (iOS sheet gesture) | Not applicable — this is a modal, not a sheet |

---

## 9. Interaction States

### App Launch Flow

```
┌──────────────┐
│  App Launch  │
└──────┬───────┘
       │
       ▼
┌──────────────────┐     No      ┌────────────────┐
│ Trial Expired?   │────────────►│  Camera Screen │
└──────┬───────────┘             └────────────────┘
       │ Yes
       ▼
┌──────────────────┐
│  Support Dialog  │
└──────┬───────────┘
       │ (any dismiss)
       ▼
┌──────────────────┐
│  Camera Screen   │
└──────────────────┘
```

### Camera Screen States

| State | Description | Visual |
|-------|-------------|--------|
| **No Resistors** | Camera is live, no resistors detected | Empty camera view — no indicators |
| **Resistors Detected** | One or more resistors in frame | Overlay badges appear above each |
| **Low Confidence** | Resistor detected but confidence below threshold | Dashed border badge with "?" suffix |
| **Resistor Exits Frame** | Resistor moves out of camera view | Badge fades out over 100ms |
| **Resistor Re-enters** | Resistor returns to camera view | Badge fades in over 150ms |

### Overlay Lifecycle

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│  Not Detected ──► Detected ──► Tracking ──► Lost ──► Fade   │
│       │              │            │           │        Out   │
│       │              │            │           │              │
│       └──────────────┴────────────┴───────────┘              │
│       (resistor in frame)         (resistor exits frame)    │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## 10. Accessibility

### Touch Targets
- **Minimum touch target size:** 48x48dp (per Material Design / iOS HIG guidelines)
- All buttons in the Support Dialog meet this minimum
- Overlay badges are not interactive — no touch targets needed

### Color Contrast

| Element | Foreground | Background | Ratio | WCAG Level |
|---------|------------|------------|-------|------------|
| Overlay badge text | #FFFFFF | #000000 (80%) | 13.5:1 | AAA |
| Primary button text | #000000 | #4FC3F7 | 8.6:1 | AAA |
| Secondary button text | #FFFFFF | #121212 | 15.3:1 | AAA |
| Dialog body text | #FFFFFF | #121212 | 15.3:1 | AAA |

### VoiceOver / TalkBack Considerations

Since the primary surface is a live camera with AR overlays:

| Element | Accessibility Label |
|---------|---------------------|
| **Camera View** | "Camera view. Point at resistors to identify values." |
| **Overlay Badge** | Read aloud when new resistor detected: "Detected: 4-band resistor, 470 ohms, plus or minus 5 percent" |
| **Support Dialog** | Standard dialog accessibility — title, message, and buttons all labeled |
| **Support Button** | "Support. Button. Opens purchase options." |
| **Maybe Later Button** | "Maybe Later. Button. Dismisses dialog." |

### Screen Reader Behavior
- When a resistor is detected, announce the value once
- Do not continuously re-announce while the resistor remains in frame
- When resistor leaves frame, optionally announce "Resistor lost" (configurable, default off)

### Dynamic Type
- All text elements should scale with system Dynamic Type settings
- Overlay badges should scale up to 200% of base size
- Beyond 200%, truncate with ellipsis rather than overflow

---

## 11. Platform Notes

### iOS-Specific

| Consideration | Guidance |
|---------------|----------|
| **Safe Area (Notch)** | Do not position overlay badges behind the notch or Dynamic Island. If a resistor is detected near the top, badge flips to below. |
| **Home Indicator** | Camera view extends under the home indicator, but badges should not anchor behind it. |
| **Status Bar** | Hide status bar for full immersion, OR show light status bar (white icons) to maintain clock/battery visibility. Recommend: show status bar. |

### Android-Specific

| Consideration | Guidance |
|---------------|----------|
| **Navigation Bar** | Use edge-to-edge mode; camera extends behind nav bar. Overlay badges respect nav bar safe area. |
| **Camera Cutouts** | Handle punch-hole cameras same as notch — do not anchor badges behind cutouts. |
| **Back Gesture** | Support gesture navigation. In Support Dialog, back gesture dismisses dialog. On Camera Screen, back gesture exits app. |

### Cross-Platform

| Consideration | Guidance |
|---------------|----------|
| **Orientation** | Portrait primary for phones. Tablets (≥600dp) may support landscape in future iteration — see Section 3. |
| **Split Screen / Multitasking** | Not supported. If app enters split screen, camera may not function correctly — acceptable limitation. |
| **Samsung Fold** | App must handle fold/unfold transitions gracefully. See Section 3 for breakpoints and badge adaptations. |

---

## 12. Confirmed Constraints

The following constraints are **confirmed by ThindalTV** and are not assumptions:

### Device Support

| Constraint | Details |
|------------|---------|
| **Standard phones** | Primary target, portrait orientation |
| **Samsung Galaxy Fold** | Both outer (~360dp) and inner (~720dp) screens must work gracefully |
| **Tablets** | 600dp+ screens, may support landscape |
| **Responsive breakpoints** | <400dp (compact), 400-599dp (standard), ≥600dp (expanded) |

### Permissions Scope

| Constraint | Details |
|------------|---------|
| **Camera permission only** | Required for live resistor detection |
| **No gallery/media access** | App operates on live camera only — no gallery picker, no photo import |
| **No microphone** | Audio not required for any feature |

---

## 13. Assumptions

The following assumptions were made during design. These should be validated with Bruce (Vision/ML) and Shuri (Implementation):

### ML Output Format Assumptions

| Assumption | Notes |
|------------|-------|
| ML returns bounding box coordinates for each detected resistor | (x, y, width, height) in screen coordinates or normalized 0-1 |
| ML returns resistor type (4-band, 5-band, 6-band) | String enum |
| ML returns calculated value as a number | e.g., 10000 for 10kΩ |
| ML returns tolerance as a number | e.g., 5 for ±5% |
| ML returns confidence score per detection | 0.0 to 1.0 |
| Low confidence threshold is configurable | Assumed default: 0.7 |
| ML can detect multiple resistors simultaneously | Up to reasonable limit (assumed: 10+) |
| ML updates at frame rate (or near it) | At least 10fps detection updates |

### Platform Assumptions

| Assumption | Notes |
|------------|-------|
| Payment/support flow is handled externally | "Support" button navigates to App Store/Play Store or in-app purchase flow |
| Trial expiration is tracked locally | First launch date stored in app preferences |

### User Behavior Assumptions

| Assumption | Notes |
|------------|-------|
| Users hold phone at arm's length | Approximately 45-60cm viewing distance |
| Users understand resistors are physical components | No educational onboarding needed |
| Users can tolerate brief detection latency | Up to 500ms acceptable for initial detection |

---

## Appendix: Quick Reference

### Color Codes

```
Primary Dark:      #121212
Primary Accent:    #4FC3F7
Secondary Accent:  #FFB74D
Success:           #81C784
Error:             #E57373
Badge Background:  #000000 @ 80%
Badge Text:        #FFFFFF
```

### Type Scale

```
Overlay Value:     16sp Bold
Overlay Type:      12sp Medium  
Dialog Title:      20sp Bold
Dialog Body:       16sp Regular
Button Text:       16sp SemiBold
```

### Key Dimensions

```
Badge corner radius:    8dp
Badge padding:          8dp H, 6dp V (standard); 6dp H, 4dp V (compact)
Badge gap from resistor: 8dp
Min touch target:       48x48dp
Dialog corner radius:   16dp
Dialog width:           85% screen, max 320dp (phone) / max 400dp (tablet)
```

### Responsive Breakpoints

```
Compact:   < 400dp (Fold outer, small phones)
Standard:  400-599dp (most phones)
Expanded:  ≥ 600dp (Fold inner, tablets)
```

---

*This document is a living spec. As implementation reveals edge cases or constraints, update this document to reflect actual behavior.*
