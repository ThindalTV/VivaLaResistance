# Decisions

## 2026-03-09T17:34 — User Directive: .squad/ Commit Scope

**By:** ThindalTV (via Copilot)  
**Category:** Git workflow

**Decision:** `.squad/` must NOT be committed on feature branches. Scribe commits `.squad/` state to `main` (or a dedicated state branch) only, never to feature/work PRs.

**Rationale:** Feature PRs should contain only source code changes. `.squad/` team state in feature PRs pollutes diffs and review experience.

**Applies To:** All future PRs; retroactively cleaned PRs #46, #48, #49.

---

## 2026-03-09 — App Icon & Splash Screen Design Spec

**By:** Hope (UX Designer)  
**Issue:** #30  
**PR:** #50  
**Status:** ✅ Approved, implemented by Shuri

### Decisions Made

#### 1. Icon Subject: Stylized Resistor
Use a simplified resistor graphic as the app icon.

**Rationale:** The resistor is the core object the app identifies. Electronics users immediately recognize the shape and color band pattern.

#### 2. Color Band Value: 4.7kΩ
Show Yellow-Violet-Red-Gold bands (4.7kΩ ±5%).

**Rationale:** Visually distinctive colors with strong contrast. 4.7kΩ is one of the most common resistor values — target users will recognize it.

#### 3. Icon Background: #121212 (Primary Dark)
Use the app's Primary Dark color for icon background.

**Rationale:** Maintains brand consistency. Works on both light and dark home screens. Matches splash-to-app transition.

#### 4. Lead Wire Accent: #4FC3F7 (Electric Blue)
Use the app's accent color for resistor lead wires.

**Rationale:** Ties the icon to the app's visual identity. Adds color interest without competing with the color bands.

#### 5. No Splash Tagline
Splash screen shows only the centered icon with no text.

**Rationale:** Speed perception (fast splash = no time to read text), localization simplicity (icon is universal).

#### 6. MAUI Single-Source SVG
Use MAUI's SVG-based icon generation (`appicon.svg` + `appiconfg.svg`).

**Rationale:** MAUI automatically generates all required sizes for iOS and Android from a single source, reducing maintenance burden and ensuring consistency.

---

## 2026-07-18 — App Icon & Splash Color Standard

**By:** Shuri (Mobile Developer)  
**Issue:** #30  
**Status:** ✅ Implemented

**Decision:** Icon background and splash screen background color is `#121212` (Primary Dark), replacing the MAUI template default of `#512BD4`.

**Rationale:** Hope's `AppIconSpec.md` specifies `#121212` as the Primary Dark color throughout the design. Using `#512BD4` (MAUI purple) creates visual inconsistency between the icon, splash screen, and app chrome.

**Applies To:**
- `<MauiIcon Color="#121212">` in `VivaLaResistance.csproj`
- `<MauiSplashScreen Color="#121212">` in `VivaLaResistance.csproj`
- Any future icon variants or platform-specific overrides

**Note on Build Verification:**
When building the solution with `-p:TargetFramework=net10.0-android` on the full solution (`.slnx`), non-MAUI projects (Core, Services, Tests) will emit NETSDK1005 errors because they don't target `net10.0-android`. Build the MAUI `.csproj` directly to get a clean pass.

---

## 2026-07-19 — App Store Copy & Positioning

**By:** Hope (UX Designer)  
**Issue:** #34  
**PR:** #51  
**Status:** ✅ Approved by Rhodes

### Decisions Made

#### 1. Brand Messaging: "Point. See. Know."
Using "Point. See. Know." as the primary tagline/hero screenshot headline.

**Rationale:** Three words, action-oriented, immediately communicates the value proposition. Alternatives considered: "Instant Resistor ID" (more literal), "No More Guessing" (negative framing).

**Team Impact:** If adopted as official tagline, should appear consistently across marketing materials, splash screens, etc.

#### 2. Privacy-First Positioning
Leading with privacy/offline capability as a key differentiator, not just a feature.

**Rationale:** Most competitor apps require internet or have unclear data practices. "No data collection, no internet, no tracking" stance is rare and valuable. Privacy policy emphasizes this.

**Team Impact:** This is now a brand promise. Any future features requiring network access would need careful consideration.

#### 3. Monetization Disclosure Approach
Described the support dialog transparently as "fully functional from day one" with "occasional, easily-dismissed prompt" after 7 days.

**Rationale:** Honesty builds trust. Apple and Google both require accurate monetization disclosure. Framing positively ("no features locked, no ads, no pressure") differentiates from nagware/adware.

**Team Impact:** None — aligns with existing WinRAR-style monetization decision.

#### 4. Target Audience Expansion
Explicitly targeting engineering/physics students alongside hobbyists and technicians.

**Rationale:** Students represent a large, underserved audience. "Never misread a color code again" messaging resonates with anyone learning electronics.

**Team Impact:** May influence future feature prioritization (educational modes, tutorials) but no immediate impact.

### Outstanding Items for Resolution

1. **URLs:** Currently pointing to GitHub. Recommend creating a simple landing page before launch.
2. **Contact email:** Privacy policy has placeholder. Need real email before submission.
3. **Feature graphic:** Spec provided but asset needs creation (Shuri or external designer).
4. **Minimum Android version:** Stated Android 8.0 (API 26) — Shuri should confirm this matches build config.
5. **Ω symbol:** Verify range compatibility with target devices.

---

## Core Context

- **Project:** VivaLaResistance — C# .NET MAUI mobile app (iOS & Android) using computer vision to detect resistors and display AR-style overlays.
- **Team:** Rhodes (Lead), Shuri (Mobile Dev), Bruce (Vision/ML), Natasha (Tester), Scribe (Session Logger), Ralph (Work Monitor).
- **Design Philosophy:** Speed, legibility, non-intrusiveness, respect. Respectful monetization (WinRAR-style support dialog).
- **Scope:** Camera-only permissions, portrait-only, 100% offline processing, support for multiple resistors simultaneously.
