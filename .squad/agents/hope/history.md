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
