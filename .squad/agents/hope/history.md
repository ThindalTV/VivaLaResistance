# Hope — History

## Project Context
**Project:** VivaLaResistance — C# .NET MAUI mobile app (iOS & Android) that uses computer vision to detect resistors in a live camera view, calculates their values from color bands, and displays AR-style overlays next to each detected resistor.

**Tech Stack:** C# .NET MAUI, iOS & Android only (no desktop), ML/Vision for color-band detection, SLNX solution format.

**User:** ThindalTV

**Key design considerations:**
- Live camera feed is the primary surface — overlays must be non-intrusive and legible
- Support multiple resistors simultaneously in one frame
- Monetization: 7-day free trial, then a dismissible modal on each subsequent launch ("You like this. Would you like to support it?") — must feel respectful, not predatory
- Mobile-first, small screens, real-world lighting conditions

**Team:**
- Rhodes — Lead
- Shuri — Mobile Dev (MAUI/XAML implementation partner)
- Bruce — Vision/ML Dev (defines what detection data is available)
- Natasha — Tester
- Scribe — Session Logger
- Ralph — Work Monitor
- @copilot — Coding Agent

## Learnings

### 2026-02-25: Created UX Design Guidelines
**Document:** `docs/design-guidelines.md`

**Key Design Decisions:**
- **Dark-first color palette** — camera apps work better with dark UI chrome (#121212 primary, #4FC3F7 accent)
- **Semi-transparent badge overlays** — black at 80% opacity ensures readability over any real-world background (13.5:1 contrast ratio)
- **No persistent chrome** — 100% camera view, overlays only appear as AR badges anchored to detected resistors
- **Badge positioning** — anchored above resistor bounding box, flips to below when near screen top, stacks vertically when resistors overlap
- **Respectful monetization** — WinRAR-style dialog with warm tone, easy dismiss via "Maybe Later" or tap-outside, no guilt-tripping
- **Portrait-only** — landscape adds complexity with little value for this use case
- **No detection indicators** — app is always scanning, no spinners or "searching" text; absence of badges implies no resistors detected

**Design Philosophy:** Speed, legibility, non-intrusiveness, respect. The app should feel instant and stay out of the user's way.

### 2026-02-25: Responsive Design & Permission Scope Update
**Document:** `design/design-guidelines.md` (v1.1)

**Responsive Design Decisions:**
- **Three device classes:** Standard phones (360-420dp), Samsung Galaxy Fold (both screens), tablets (600dp+)
- **Three breakpoints:** Compact (<400dp), Standard (400-599dp), Expanded (≥600dp)
- **Fold outer screen:** Cramped layout — compact badges (value only, no type prefix), 14sp font, max 2 badges visible
- **Fold inner screen:** Rich layout — full badges with type prefix, 18sp font, up to 5 badges visible
- **Tablets:** Similar to Fold inner, up to 6+ badges, potential landscape support in future
- **Support dialog adapts:** Bottom sheet on phones (<600dp), centered modal on tablets (≥600dp)
- **Fold transitions:** Smooth scaling when user folds/unfolds, badges transition format over 200ms

**Permission Scope (Confirmed by ThindalTV):**
- **Camera only** — `NSCameraUsageDescription` (iOS), `CAMERA` (Android)
- **No gallery access** — explicitly excluded: `READ_MEDIA_IMAGES`, `READ_EXTERNAL_STORAGE`, `NSPhotoLibraryUsageDescription`
- **No microphone** — `RECORD_AUDIO` and `NSMicrophoneUsageDescription` explicitly excluded
- **No gallery picker UI** — no "pick from gallery" button anywhere in the app

**MAUI Implementation Notes for Shuri:**
- Use `DeviceDisplay.Current.MainDisplayInfo` for screen dimensions
- Subscribe to `Window.SizeChanged` for Fold transitions
- 400dp threshold triggers badge format change (compact ↔ full)

### 2026-07-18: Support Modal UX Spec (Issue #15)
**Document:** `design/support-modal-spec.md`

**Key Design Decisions:**
- **Trigger timing** — Show 500ms after first camera frame renders, not on app launch. Camera continues running behind modal.
- **Copy tone** — "Thanks for using Viva La Resistance! This app is made by one person. If it's saved you time, consider showing some support." Human, warm, no guilt.
- **Single-tap dismiss** — Button ("Got it, thanks"), scrim tap, Android back, or swipe-down all dismiss. No confirmation.
- **Responsive layout** — Bottom sheet on phones (<600dp), centered modal on tablets (≥600dp)
- **Visual style** — #1a1a2e surface, #0f3460 accent, 50% scrim so camera stays visible, 16dp corner radius
- **Accessibility** — All text exceeds WCAG AAA contrast, 48dp touch targets, screen reader announces dialog role

**Philosophy Applied:** The modal is a polite ask from a friend, not a toll booth. One tap and you're through. No "Support" button yet since no store page exists—future iteration will add it.

**Rejected Copy Options:**
- "Enjoying Viva La Resistance?" — Feels like fishing for "yes" before the ask
- "Support indie development" — Too corporate/generic
- "Help keep the lights on" — Guilt-trippy

### 2026-07-19: App Store Listing Materials (Issue #34)
**Documents:** `design/app-store/` directory

**Deliverables:**
- `description-ios.md` — Complete iOS App Store listing (title, subtitle, description, keywords, promotional text)
- `description-android.md` — Complete Google Play listing (short/full descriptions, feature graphic spec)
- `screenshots-spec.md` — Screenshot requirements for all device sizes with content and overlay specs
- `privacy-policy.md` — Full privacy policy for camera-only local-processing app
- `release-notes-v1.0.md` — v1.0 release notes for both platforms

**Messaging Strategy:**
- **Lead with pain point:** "Stop squinting at tiny color bands"
- **Three pillars:** Instant AR overlays, works 100% offline, multiple resistors at once
- **Privacy-first:** Emphasized no data collection, no internet required, camera-only permissions
- **Monetization transparency:** Described support dialog honestly as "fully functional, no feature lockout"

**Character Limit Compliance:**
- iOS title: 20/30 chars
- iOS subtitle: 26/30 chars
- iOS promotional text: 145/170 chars
- iOS description: 1,596/4,000 chars
- iOS keywords: exactly 100 chars (comma-separated, no spaces)
- Android short description: 80/80 chars (exactly at limit)
- Android full description: 2,238/4,000 chars

**Screenshot Strategy:**
- 6 screenshots per platform (hero → multiple → real-world → detail → offline → monetization)
- Overlay text style: white Inter Bold on 70% opacity black pills
- Positioned in lower third, avoiding AR badges

**Design Decisions:**
- **App name:** "Viva La Resistance" (20 chars, pun works internationally)
- **Categories:** iOS = Utilities (primary) + Education (secondary); Android = Tools + Education
- **Age rating:** 4+ (iOS), Everyone (Android) — camera-only, no objectionable content
- **Privacy policy format:** Q&A summary table at end for quick scanning

**Placeholders Noted:**
- Support URL → GitHub issues (recommend landing page before launch)
- Marketing URL → GitHub repo (recommend landing page)
- Contact email → needs real address before submission
- Feature graphic → needs design execution per provided spec

**Status:** ✅ PR #51 approved by Rhodes. Shipped to squad/34-app-store-listing branch (no .squad/ commits per directive).

### 2026-03-09: Sprint Wrap — Both Design Specs Shipped
**PRs:** #50 (icon spec), #51 (app store listing)
**Status:** ✅ Both complete, both on squad/* branches without .squad/ commits

All design deliverables merged and ready for implementation/submission. Next: Placeholder resolution, feature graphic creation, screenshot capture.
