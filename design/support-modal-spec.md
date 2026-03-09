# Support Modal UX Specification

**Version:** 1.0  
**Author:** Hope (UX Designer)  
**Created:** 2026-07-18  
**For Issue:** #15 — Dismissible support modal after trial expiration

---

## 1. Overview

The support modal is a friendly, dismissible prompt shown after the 7-day trial expires. This is **not a paywall**—the user can always dismiss and use the full app. The modal appears once per app launch as a polite reminder that the app is indie-made and would appreciate support.

**Design Philosophy:** WinRAR-style honesty. The modal says "hey, you've been using this—consider supporting it" and gets out of the way with a single tap.

---

## 2. Trigger & Timing

| Aspect | Specification |
|--------|---------------|
| **Condition** | `TrialService.ShouldShowSupportModal()` returns `true` |
| **Frequency** | Once per app launch (not per resume from background) |
| **Timing** | Show **after camera preview initializes** and first frame renders |
| **Delay** | 500ms after first camera frame to avoid jarring the user |
| **Animation** | Fade in over 200ms (modal surface + scrim together) |

### Why Wait for Camera Init?

Showing the modal before the camera loads creates a "wall" feeling. By waiting until the camera is running, the user sees the app is functional first. The camera continues running behind the modal's scrim, reinforcing "this works, we're just asking politely."

---

## 3. Visual Layout

### Responsive Presentation

| Screen Width | Presentation |
|--------------|--------------|
| **< 600dp** (phones) | **Bottom sheet** — slides up from bottom |
| **≥ 600dp** (tablets/Fold inner) | **Centered modal** — vertically centered |

### Bottom Sheet Layout (Phones)

```
┌─────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Camera feed visible through scrim
│ ░░░░░░░ (50% scrim) ░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
├─────────────────────────────┤  ← Rounded top corners (16dp radius)
│                             │
│           ⏄ ⏄ ⏄            │  ← Resistor illustration (see below)
│          ───┬┬┬───          │
│                             │
│   Thanks for using          │  ← Headline (18sp, bold)
│   Viva La Resistance!       │
│                             │
│   This app is made by one   │  ← Body (14sp, regular)
│   person. If it's saved     │
│   you time, consider        │
│   showing some support.     │
│                             │
│   ┌─────────────────────┐   │
│   │    Got it, thanks   │   │  ← Primary button (48dp height)
│   └─────────────────────┘   │
│                             │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Safe area padding (bottom)
└─────────────────────────────┘
```

### Centered Modal Layout (Tablets)

```
┌─────────────────────────────────────────────────┐
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░ (50% scrim) ░░░░░░░░░░░░░░░░░░░ │
│ ░░░░░░░░┌─────────────────────────┐░░░░░░░░░░░░ │  ← 16dp radius all corners
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│        ⏄ ⏄ ⏄           │░░░░░░░░░░░░ │
│ ░░░░░░░░│       ───┬┬┬───         │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│   Thanks for using      │░░░░░░░░░░░░ │
│ ░░░░░░░░│   Viva La Resistance!   │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│   This app is made by   │░░░░░░░░░░░░ │
│ ░░░░░░░░│   one person. If it's   │░░░░░░░░░░░░ │
│ ░░░░░░░░│   saved you time,       │░░░░░░░░░░░░ │
│ ░░░░░░░░│   consider showing      │░░░░░░░░░░░░ │
│ ░░░░░░░░│   some support.         │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░│  ┌───────────────────┐  │░░░░░░░░░░░░ │
│ ░░░░░░░░│  │   Got it, thanks  │  │░░░░░░░░░░░░ │
│ ░░░░░░░░│  └───────────────────┘  │░░░░░░░░░░░░ │
│ ░░░░░░░░│                         │░░░░░░░░░░░░ │
│ ░░░░░░░░└─────────────────────────┘░░░░░░░░░░░░ │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
└─────────────────────────────────────────────────┘
          max 360dp modal width
```

### Illustration

Use a simple resistor doodle or emoji composition at the top. Options in order of preference:

1. **Custom SVG resistor icon** — simple line art, 48dp tall, app accent color (#4FC3F7)
2. **Unicode fallback** — `🔌` or `⚡` if custom icon unavailable for v1

The illustration adds warmth and reminds the user what the app does.

---

## 4. Tone & Copy

### Final Copy

| Element | Text |
|---------|------|
| **Headline** | Thanks for using Viva La Resistance! |
| **Body** | This app is made by one person. If it's saved you time, consider showing some support. |
| **Button** | Got it, thanks |

### Tone Guidelines

- **Friendly, not corporate** — "one person" feels human vs. "our team"
- **Grateful, not demanding** — "thanks for using" comes first
- **Soft ask, not guilt** — "consider" is an invitation, not an obligation
- **Brief** — 2 sentences max in body; respects user's time
- **Playful nod to app name** — "Resistance" is in the name, no need to force puns

### What We Avoid

❌ "Your trial has expired"  
❌ "Please support the developer"  
❌ "Without your support, this app can't continue"  
❌ "Don't you want to support indie devs?"  
❌ Any countdown or "X days overdue" language

---

## 5. Interaction Behavior

| Interaction | Behavior |
|-------------|----------|
| **Tap "Got it, thanks" button** | Dismiss modal immediately (no delay) |
| **Tap scrim (outside modal)** | Dismiss modal |
| **Android back button** | Dismiss modal |
| **Swipe down (bottom sheet)** | Dismiss modal (swipe-to-dismiss gesture) |
| **After dismiss** | Camera view is fully usable; modal won't reappear until next fresh launch |

### Animation on Dismiss

- Fade out over 150ms (faster than fade-in for snappier feel)
- Bottom sheet variant: slide down + fade simultaneously

### One Tap Philosophy

The user should never need more than one tap to get past this modal. Every interaction path leads to dismissal.

---

## 6. Visual Style

### Colors

| Element | Color | Notes |
|---------|-------|-------|
| **Scrim** | `#000000` at 50% opacity | Dims camera but keeps it visible |
| **Modal surface** | `#1a1a2e` | Slightly lighter than pure black; matches app dark theme |
| **Modal border** | `#0f3460` at 60% opacity | Subtle accent border, 1dp width |
| **Headline text** | `#FFFFFF` | Pure white |
| **Body text** | `#CCCCCC` | Slightly muted for hierarchy |
| **Button background** | `#0f3460` | Primary accent |
| **Button text** | `#FFFFFF` | Pure white |
| **Button pressed** | `#0a2540` | Darker state on press |

### Dimensions

| Property | Phone (< 600dp) | Tablet (≥ 600dp) |
|----------|-----------------|------------------|
| **Modal width** | 100% (full width) | 360dp max |
| **Corner radius** | 16dp (top only) | 16dp (all corners) |
| **Padding (horizontal)** | 24dp | 24dp |
| **Padding (vertical)** | 24dp top, safe area bottom | 24dp all |
| **Headline font** | 18sp, Bold | 20sp, Bold |
| **Body font** | 14sp, Regular | 15sp, Regular |
| **Button height** | 48dp | 48dp |
| **Button width** | 100% - 48dp margins | 100% - 48dp margins |
| **Button corner radius** | 8dp | 8dp |
| **Element spacing** | 16dp between elements | 20dp between elements |

---

## 7. Accessibility

### Touch Targets

| Element | Minimum Size | Notes |
|---------|--------------|-------|
| **"Got it, thanks" button** | 48dp × 48dp | Already 48dp tall; full width ensures >48dp wide |
| **Scrim tap area** | Full remaining screen | Easy to tap anywhere to dismiss |

### Semantic Labels

| Element | Accessibility Label | Role |
|---------|---------------------|------|
| **Modal container** | "Support reminder" | Dialog |
| **Headline** | (visible text) | Heading |
| **Body** | (visible text) | Text |
| **Button** | "Dismiss support reminder" | Button |
| **Scrim** | "Dismiss" | Button (for screen readers) |

### Screen Reader Announcement

When modal appears, announce: "Support reminder. Thanks for using Viva La Resistance. Press button to dismiss."

### Contrast Ratios

| Pair | Ratio | Standard Met |
|------|-------|--------------|
| Headline (#FFFFFF) on surface (#1a1a2e) | **14.5:1** | WCAG AAA ✓ |
| Body (#CCCCCC) on surface (#1a1a2e) | **9.8:1** | WCAG AAA ✓ |
| Button text (#FFFFFF) on button (#0f3460) | **10.2:1** | WCAG AAA ✓ |

### Motion & Reduced Motion

If user has reduced motion enabled:
- Skip fade-in/fade-out animations
- Modal appears/disappears instantly

---

## 8. Edge Cases

### Camera Still Scanning

**Scenario:** User is mid-scan with resistors detected when modal appears.

**Behavior:**
- Camera **continues running** behind the scrim
- Detection **continues** but overlays are hidden/dimmed
- When modal is dismissed, any detected resistors immediately show their overlays
- No "restart" or re-detection needed

### Rapid Dismissal

**Scenario:** User immediately taps dismiss before reading.

**Behavior:** That's fine. This is the expected behavior. No delay, no "are you sure?"

### App Backgrounded During Modal

**Scenario:** Modal is showing, user switches apps, returns later.

**Behavior:**
- Modal persists on return
- Camera reinitializes behind it
- User dismisses as normal

### First Launch After Trial Expires

**Scenario:** User's first app launch after the 7-day trial ends.

**Behavior:** Standard modal appearance—500ms delay after camera init, then fade in. No special messaging for "this is your first time seeing this."

### Orientation Change During Modal

**Scenario:** (Tablets only) User rotates device while modal is visible.

**Behavior:**
- Modal remains centered and re-layouts to new orientation
- Camera preview re-layouts behind it
- No dismissal, no re-animation

---

## 9. Future Considerations (Out of Scope for v1)

These are noted but **not implemented** in the initial version:

- **"Support" button** — Currently no store page exists. When available, add a secondary "Support" button that opens the store page. The primary button remains "Got it, thanks."
- **"Don't show again" option** — Intentionally omitted. The modal is polite enough that a permanent dismiss isn't needed.
- **Support status indicator** — If user has supported, don't show modal. Requires payment integration.

---

## 10. Implementation Notes for Shuri

### MAUI Components

- Use `ContentView` overlay or `Popup` (from MAUI Community Toolkit)
- Bottom sheet: Consider `BottomSheet` control from Community Toolkit
- Animate with `FadeTo()` and `TranslateTo()` for smooth entrance/exit

### Trigger Integration

```csharp
// Pseudocode
if (TrialService.ShouldShowSupportModal())
{
    await Task.Delay(500); // Wait after camera init
    await ShowSupportModal();
}
```

### Dismiss Handling

All dismiss paths should call the same method that:
1. Animates modal out
2. Removes from visual tree
3. Does NOT set any "don't show again" flag (shows again next launch)

### Safe Area

On phones, respect bottom safe area (home indicator on iPhone, nav bar on Android). Add appropriate padding so button isn't obscured.

---

## Appendix: Copy Alternatives Considered

| Option | Why Rejected |
|--------|--------------|
| "Enjoying Viva La Resistance?" | Feels like it's fishing for "yes" before the ask |
| "Support indie development" | Too corporate/generic |
| "Buy me a coffee" | Overused, and we're not using that platform |
| "Join the resistance!" | Cute but doesn't communicate the ask |
| "Help keep the lights on" | Guilt-trippy |

The final copy ("Thanks for using... one person... consider showing some support") tested best for warmth without pressure.
