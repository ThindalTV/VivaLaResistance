# Rhodes — History

## Project Context

**Project:** VivaLaResistance
**Role:** Lead
**User:** ThindalTV
**Stack:** C# .NET MAUI, iOS + Android only, SLNX solution format, ML/Vision for resistor detection
**Mission:** Camera app that detects resistors, calculates values from color bands, displays AR-style overlays next to each resistor. Multiple resistors per frame. 7-day free trial then dismissible support modal (WinRAR-style).

## Learnings

### 2026-02-25: CI Pipeline Setup (Issue #1)

**Created `.github/workflows/ci.yml`** with three jobs:

- **test** (ubuntu-latest): Restores solution, runs xUnit tests against `VivaLaResistance.Tests` (net9.0 — no MAUI workload needed)
- **build-android** (ubuntu-latest): Installs MAUI workload, builds `net9.0-android` target in Release config
- **build-ios** (macos-latest): Installs MAUI workload, builds `net9.0-ios` target in Release config with `BuildIpa=false` to skip code signing requirement

**Key decisions:**
- iOS job must run on `macos-latest` (Apple toolchain requirement)
- Android and test jobs run on `ubuntu-latest` (cheaper + sufficient)
- NuGet cache keyed on `${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}` — OS-scoped to avoid cross-contamination
- iOS build uses `/p:BuildIpa=false` so CI doesn't fail on missing provisioning profile
- Tests target `net9.0` directly (no MAUI dependency), so test job is lean and fast
- Existing `squad-*.yml` and `sync-squad-labels.yml` workflows untouched

**README:** Added CI badge pointing at `ci.yml` workflow on ThindalTV/VivaLaResistance.

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

### 2026-02-25: PR #35 — CI Pipeline Review & Approval

Reviewed GitHub Actions workflow submission from Rhodes (Issue #1):
- `.github/workflows/ci.yml` with three jobs (test, build-android, build-ios)
- All 5 acceptance criteria verified as met
- Architecture sound: OS-specific runners, NuGet cache keyed per OS, iOS unsigned build appropriate
- README badge added correctly
- PR ready for merge
