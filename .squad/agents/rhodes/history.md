# Rhodes — History

## Project Context

**Project:** VivaLaResistance
**Role:** Lead
**User:** ThindalTV
**Stack:** C# .NET MAUI, iOS + Android only, SLNX solution format, ML/Vision for resistor detection
**Mission:** Camera app that detects resistors, calculates values from color bands, displays AR-style overlays next to each resistor. Multiple resistors per frame. 7-day free trial then dismissible support modal (WinRAR-style).

## Learnings

### 2026-02-25: Initial Solution Architecture Setup

**Created full solution structure under `/src`:**

- **Solution:** `src/VivaLaResistance.slnx` (SLNX format)
- **Main App:** `src/VivaLaResistance/` — MAUI app targeting net9.0-ios;net9.0-android only
- **Core Library:** `src/VivaLaResistance.Core/` — Pure .NET 9.0, no MAUI dependencies
- **Services Library:** `src/VivaLaResistance.Services/` — Service implementations
- **Tests:** `src/VivaLaResistance.Tests/` — xUnit tests (66 tests, all passing)

**Key Files:**
- `src/VivaLaResistance/MauiProgram.cs` — DI registration for all services
- `src/VivaLaResistance/MainPage.xaml` — Camera view placeholder with MVVM binding
- `src/VivaLaResistance/ViewModels/MainViewModel.cs` — Main ViewModel with trial and detection logic
- `src/VivaLaResistance.Core/Models/ColorBand.cs` — Enum for resistor colors
- `src/VivaLaResistance.Core/Models/ResistorReading.cs` — Domain model for detected resistors
- `src/VivaLaResistance.Core/Interfaces/` — Service interfaces (IResistorDetectionService, IResistorValueCalculatorService, ITrialService)
- `src/VivaLaResistance.Services/ResistorValueCalculatorService.cs` — Full implementation of color band → ohm calculation
- `src/VivaLaResistance.Services/TrialService.cs` — 7-day trial logic with IPreferencesWrapper abstraction

**Architectural Decisions:**
1. Services pattern: All compute logic in service classes, never in ViewModels
2. Core library has zero MAUI dependencies for testability
3. IPreferencesWrapper and IDateTimeProvider abstractions enable unit testing of TrialService
4. MVVM with dependency injection via MauiProgram.cs
5. Platform targets: iOS 15.0+ and Android 21.0+ only (no desktop)

**Test Coverage:**
- ResistorValueCalculatorServiceTests: 43 tests covering digit values, multipliers, tolerances, 4/5/6 band calculations, formatting
- TrialServiceTests: 23 tests covering fresh install, trial active/expired states, days remaining, support modal logic

### 2026-02-25: GitHub Issues Created for Project Tracking

**Created 34 comprehensive GitHub issues** in thindaltv/VivaLaResistance repository to track all work for building the app.

### 2026-02-25: GitHub Issue Categorization with Labels

**Created and applied 6 new category labels to all 34 open issues** in thindaltv/VivaLaResistance.

**Labels Created:**
- `infrastructure` (#0075ca) — Build, CI, project setup, configuration (7 issues)
- `vision` (#e4e669) — ML/computer vision, resistor detection, camera (10 issues)
- `mobile` (#d73a4a) — MAUI, UI, platform-specific mobile concerns (10 issues)
- `monetization` (#a2eeef) — Trial logic, modals, payment flows (3 issues)
- `ux` (#cfd3d7) — Design, overlays, user experience (4 issues)
- `testing` (#0e8a16) — Tests, quality, edge cases (4 issues)

**Label Distribution Across 34 Issues:**
- Infrastructure only: #1, #2, #3, #4, #20, #28, #29 (7 issues)
- Infrastructure + Vision: #5 (1 issue)
- Vision only: #6, #7, #8, #9, #10, #11, #19, #27, #31 (9 issues)
- Mobile only: #12, #14, #16, #17, #32, #33, #34 (7 issues)
- Mobile + Monetization: #15 (1 issue)
- Mobile + UX: #13, #30 (2 issues)
- Monetization only: #18 (1 issue)
- UX only: #21 (1 issue)
- UX + Monetization: #22 (1 issue)
- Testing only: #23, #24, #25, #26 (4 issues)

All 34 open issues are now labeled for easy filtering and navigation by team members.

**Issue Breakdown by Area:**
- Infrastructure/Architecture (Issues #1-5): CI pipeline, permissions, DI wiring, ML framework selection
- ML/Vision (Issues #6-11): Camera capture, model selection, detection service, value calculation, multi-resistor support
- UI/MAUI (Issues #12-17): Full-screen camera, AR overlays, responsive layout, support modal, Fold support, permission flow
- Core/Services (Issues #18-20): TrialService, color band lookup, ResistorReading model
- Design (Issues #21-22): Badge design spec, support modal design
- Testing (Issues #23-26): Unit tests for calculator and trial services, integration test plan
- Additional (Issues #27-34): Performance optimization, documentation, error handling, icons, memory management, accessibility, battery optimization, app store prep

**Key Architectural Decisions Captured:**
- ONNX Runtime vs TensorFlow Lite evaluation required (Issue #5)
- On-device ML constraint emphasized across detection issues
- Responsive layout breakpoints defined for phone/Fold/tablet (Issue #14)
- 7-day trial + dismissible modal pattern formalized (Issues #15, #18)
- Multi-resistor simultaneous detection as core requirement (Issue #10)
- Performance targets: <100ms inference, 10-15 FPS (Issues #27, #6)

**Major Epic Issues:**
- #5: ML framework selection (Rhodes + Bruce collaboration required)
- #8: ResistorDetectionService implementation (core ML pipeline)
- #13: AR overlay layer (core UX feature)
- #14: Responsive layout (Samsung Fold support critical)
- #26: Integration testing plan (end-to-end validation)

### 2025-07-18: Vision/ML Library Decision (Issue #5)

**Reviewed and approved Bruce's vision-library-comparison.md document.**

**Decision: Two-phase approach approved.**
- **Phase 1:** SkiaSharp + custom HSV color analysis (classical CV, no ML model needed)
- **Phase 2:** ONNX Runtime + lightweight ONNX model (YOLOv8-nano) for robust localization under varied lighting

**Key architectural insights:**
- Services project stays `net10.0` for Phase 1 — SkiaSharp supports plain .NET TFM
- Phase 2 ONNX Runtime native lib resolution needs validation through MAUI head project before committing
- Color classification should NEVER be ML-ified — HSV math is sufficient and more predictable
- Camera frame format must be contractually defined (BGRA8888 recommended) before implementation
- Phase 1 requires lighting UX guidance as mitigation for classical CV's lighting sensitivity

**Rejected options and why:**
- ML.NET: not mobile-capable
- TFLite .NET: no official .NET 10 packages, community bindings stale
- CoreML/ML Kit: platform-only, dual implementation doubles maintenance
- Emgu CV: binary size (~70MB) unacceptable for mobile app

**Key file:** `design/vision-library-comparison.md`
**Decision file:** `.squad/decisions/inbox/rhodes-vision-library-decision.md`

### 2026-07-18: Revised Vision Pipeline — ONNX-First Hybrid (Supersedes Phase 1/Phase 2 Split)

**Reviewed Bruce's ONNX model research and revised the architecture decision.**

**Previous decision (2025-07-18):** Two-phase — Phase 1 SkiaSharp+HSV localization+color, Phase 2 ONNX localization.
**New decision:** Single unified pipeline — ONNX (YOLOv8-nano) for localization + HSV for color classification from day one. Phases collapsed.

**Why the change:**
- Bruce found a 4,422-image labeled resistor dataset (isha-74mjj/yolov5-u3oks) on Roboflow
- YOLOv8 → ONNX export is proven (one command), C# integration straightforward
- Building a full classical CV localization pipeline would take comparable effort and be thrown away
- Color classification stays HSV math — "never ML-ify color reading" ruling unchanged

**Key rulings:**
1. **Option B chosen:** ONNX localization + HSV color bands (hybrid). Not pure ONNX (Option A) — color classification stays deterministic. Not two-phase (Option C) — the data availability changes the calculus.
2. **License gate is hard:** Bruce must verify isha-74mjj dataset license on Roboflow BEFORE any training. CC-BY 4.0 is acceptable with attribution.
3. **Hope must spike ONNX Runtime native lib resolution** through MAUI head project before Bruce starts integration.
4. **Camera frame format contract: BGRA8888** — formalized, was previously a condition.
5. **640px default, 320px fallback** for ONNX input resolution based on real-device profiling.
6. **Lighting UX still mandatory** — HSV is lighting-sensitive regardless of ONNX localization.
7. **Binary size: ~17MB** from start (ONNX Runtime ~15MB + model ~6MB) vs. previous ~2MB Phase 1.
8. **No interface changes needed** — IResistorDetectionService already accommodates model loading.

**New dependencies (both required from start):**
- `SkiaSharp` v3.* (color classification)
- `Microsoft.ML.OnnxRuntime` v1.* (localization)

**Risk flags:**
- Dataset domain mismatch (benchtop vs. handheld) — mitigate with augmentation + 100-200 handheld photos
- If ONNX underperforms, failure is graceful (no detection, not wrong detection)
- If dataset license is incompatible, fallback is self-collected dataset (+1 week)

**Decision file:** `.squad/decisions/inbox/rhodes-onnx-first-decision.md`
