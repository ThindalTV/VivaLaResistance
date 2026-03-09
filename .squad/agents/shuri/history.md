# Shuri — Mobile Developer History

## Learnings

### Issue #30 — App Icons & Splash Screen Design (2026-07-18)

- MAUI uses a two-layer adaptive icon approach: `appicon.svg` (background) + `appiconfg.svg` (foreground). The `.csproj` `<MauiIcon>` entry wires them together with `ForegroundFile=` attribute.
- The default MAUI template uses `#512BD4` (purple) as the icon/splash color. For this project, all color references must be `#121212` (Primary Dark) per Hope's design spec.
- `<MauiSplashScreen>` with `Color="#121212"` and `BaseSize="128,128"` is the correct configuration — no platform-specific overrides needed.
- SVG canvas for MAUI icons should be `456×456` (MAUI default). Safe zone for Android adaptive icons is within the center 72% (~328px inner area).
- When building the solution with `-p:TargetFramework=net10.0-android`, the non-MAUI projects (Core, Services, Tests) will emit NETSDK1005 errors because they don't target `net10.0-android`. Build the MAUI project directly (`src/VivaLaResistance/VivaLaResistance.csproj`) to verify the MAUI build clean.
- Splash SVG must have NO background fill — MAUI applies the `Color` attribute as the background separately.

### Issue #30 — App Icons & Splash Implementation (2026-03-09)

**Status:** ✅ Complete, build passes (0 errors)

**Changes Made:**
- Updated `appicon.svg` per Hope's spec coordinates (4.7kΩ resistor with yellow-violet-red-gold bands)
- Updated `appiconfg.svg` foreground layer with electric blue lead wires (#4FC3F7)
- Updated `splash.svg` with no background fill (MAUI applies #121212 color separately)
- Modified `VivaLaResistance.csproj`:
  - Set `<MauiIcon Color="#121212">` (Primary Dark, replacing default #512BD4)
  - Set `<MauiSplashScreen Color="#121212">`

**Build Verification:** MAUI project builds clean. Non-MAUI projects emit expected NETSDK1005 errors (by design).

**PR Status:** Shipped to squad/30-app-icons-splash (no .squad/ commits per directive). Ready for platform testing by Natasha.
