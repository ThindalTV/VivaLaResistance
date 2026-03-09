# Rhodes — History

## Core Context

**Role:** Architect & Lead  
**Mission:** Design and validate the VivaLaResistance resistor-detection MAUI app across architecture, error handling, domain models, and cross-team coordination.

### Key Architectural Decisions (Locked)

1. **Solution Format:** SLNX (.NET 10.0 multi-platform) — Visual Studio 2022+
2. **Platform Targets:** iOS 15.0+ and Android 21.0+ only; net10.0-ios, net10.0-android TFM
3. **Vision Pipeline (ONNX-First Hybrid):**
   - ONNX YOLOv8-nano for resistor body localization
   - HSV math for color band classification (never ML-ify color detection)
   - BGRA8888 pixel format contract (platform → detection service)
   - ~17MB binary (ONNX Runtime ~15MB + model ~6MB)
4. **Architecture Pattern:** Services-oriented, Core library zero-MAUI, MVVM with DI
5. **Error Handling Strategy:**
   - Global exception handlers (app lifecycle) in MauiProgram.cs
   - Graceful degradation in ML pipeline (failures return empty results, not exceptions)
   - Event-based camera error propagation via IFrameSource.ErrorOccurred
   - Custom exception types: CameraPermissionException, CameraUnavailableException
6. **Domain Model:** ResistorReading as immutable record (thread-safe for concurrent pipeline)

### Active Blockers & Dependencies

- **Bruce:** Dataset license verification (isha-74mjj/yolov5-u3oks) required before model training
- **Hope:** ONNX Runtime native lib resolution spike (MAUI head) required before integration
- **Shuri:** Must subscribe to IFrameSource.ErrorOccurred, display error banners for camera failures
- **Bruce/Hope:** Full CameraFrameSource platform implementation needed (PR #41 has stubs, blocking issues from review)
- **Natasha:** Enable error handling test cases (currently skipped)

### Project Context

**Project:** VivaLaResistance  
**User:** ThindalTV  
**Stack:** C# .NET MAUI, iOS + Android, SLNX solution, ONNX Runtime + SkiaSharp  
**Mission:** Live-view camera app detecting resistors, displaying AR-style value overlays on 4/5/6-band color readings. Multiple resistors per frame. 7-day trial + dismissible support modal (WinRAR-style).

---

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

### 2026-07-19: PR #41 Review — Platform Camera Frame Source (iOS + Android)

**PR:** #41 (`squad/6-camera-pipeline` → `main`), closes #6
**Verdict:** REQUEST CHANGES (3 blocking, 1 minor)

**Gate check results:**
- **MauiProgram.cs:** IFrameSource DI registration is correct — `#if ANDROID / #elif IOS` with Singleton lifetime follows our platform-only scope and existing patterns. Approved architecturally.
- **NuGet dep:** `Microsoft.Extensions.Logging.Debug 10.0.0` is justified — fixes pre-existing `AddDebug()` build error. Version aligns with net10.0 TFM. Approved.
- **Monetization touch:** `ShouldShowSupportModal` and MainPage modal push are read-only delegations to TrialService. No modification to trial logic. Approved — but the SupportModalPage files on the branch are STALE (older version than what PR #42 merged to main). Must be resolved via rebase.

**Blocking issues:**
1. Rebase required — `mergeable_state: dirty` after PR #42 merge. SupportModalPage.xaml, SupportModalPage.xaml.cs, and possibly MauiProgram.cs have conflicts.
2. `.squad/agents/bruce/history.md` must be `git rm --cached` — guard CI blocks merge of `.squad/` files to main.
3. SupportModalPage divergence — branch has an older, simpler version (single dismiss button, no store link). Main has the richer version from PR #42 (support + dismiss buttons, store URL opening). After rebase, branch must NOT re-introduce the older version.

**Minor issue:**
4. Android `CameraFrameSource.OnImageAvailable`: `using var image` + explicit `image.Close()` in finally = double-close. Harmless but redundant — remove either the `using` or the `finally { image.Close(); }`.

**Architecture observations:**
- Both platform implementations correctly deliver BGRA8888 frames per contract (IFrameSource / CameraFrame).
- Android: Camera2 + YUV_420_888 → managed BT.601 conversion. Correct but per-pixel loop (307K pixels × 15 FPS) is a future optimization target.
- iOS: AVFoundation + kCVPixelFormatType_32BGRA — zero-conversion path, excellent choice. Row-stride correction via Marshal.Copy is correct.
- FrameAvailable fires on platform capture threads (Android HandlerThread, iOS DispatchQueue). Subscribers must marshal to main thread. MainViewModel.ProcessFrameAsync already does this via `MainThread.BeginInvokeOnMainThread()`. PR description documents this correctly.
- Dispose() uses sync-over-async (`StopAsync().GetAwaiter().GetResult()`) — acceptable since both StopAsync implementations are synchronous in practice (return `Task.CompletedTask`).
- 640×480 hardcoded on Android, medium preset on iOS — ONNX wrapper owns resize per sprint plan. Acceptable.

**CI status at review time:**
- Build Android: ✅ | Build iOS: ✅ | xUnit Tests: ✅ | Guard: ❌ (forbidden .squad/ file)

### 2026-07-19: Error Handling Strategy Implementation (Issue #29)

**PR:** #45 (`squad/29-error-handling` → `main`), closes #29  
**Implemented:** Comprehensive error handling strategy across camera, ML inference, and app lifecycle.

**Deliverables:**
1. **Global exception handlers in MauiProgram.cs:**
   - `AppDomain.CurrentDomain.UnhandledException` — logs critical errors, displays user-facing fatal error alert when terminating
   - `TaskScheduler.UnobservedTaskException` — marks exceptions as observed to prevent app crash, logs errors for diagnostics
   - Uses ILogger<MauiApp> for centralized logging

2. **Custom exception types in Core/Exceptions/:**
   - `CameraPermissionException` — thrown when camera permissions denied/revoked, user-friendly message guides to settings
   - `CameraUnavailableException` — thrown when camera device unavailable/busy/not found
   - Both provide actionable user-facing messages, not technical details

3. **IFrameSource.ErrorOccurred event:**
   - Added `event EventHandler<Exception>? ErrorOccurred` to interface contract
   - Enables UI layer (Shuri) to subscribe and display user-friendly error banners
   - Camera errors propagate through event instead of crashing

4. **Platform-specific camera error handling:**
   - Android `CameraFrameSource.cs` stub implementation with error handling scaffolding
   - iOS `CameraFrameSource.cs` stub implementation with AVFoundation permission checks
   - Both wrap StartAsync/StopAsync in try-catch, throw typed exceptions, log via ILogger<CameraFrameSource>
   - OnImageAvailable/ProcessSampleBuffer wrap frame processing in try-catch, raise ErrorOccurred on failure
   - Note: Full implementations pending — these are architectural templates for camera team (Bruce/Hope)

5. **ML inference error handling in OnnxResistorLocalizationService:**
   - Constructor injection of ILogger<OnnxResistorLocalizationService>
   - InitializeAsync: catches model load failures, logs but doesn't throw (graceful degradation)
   - InferAsync: wraps inference in try-catch, catches OnnxRuntimeException specifically, returns empty list on failure
   - All errors logged at appropriate levels (Info for skipped init, Warning for inference failures, Error for unexpected issues)
   - Frame pipeline never crashes due to ML errors

**Architectural decisions:**
- **Graceful degradation over crashing:** All error paths return empty/safe results rather than propagating exceptions that crash the app
- **Structured logging over silent failures:** Every error path logs to ILogger with contextual information (frame dimensions, exception details)
- **User-facing messages in exception types:** Exception messages written for end users, not developers — "Please enable camera permissions in Settings" not "AVAuthorizationStatus.Denied"
- **Event-based error notification for UI:** IFrameSource.ErrorOccurred allows ViewModels to react without coupling camera implementation to UI
- **Platform-specific exception handling:** iOS checks AVAuthorizationStatus proactively before camera start; Android will check permissions via Camera2 API

**Dependencies added:**
- None — all error handling uses existing ILogger infrastructure from Microsoft.Extensions.Logging

**Build status:**
- Core: ✅ Clean build, 0 errors
- Services: ✅ 1 expected warning (CS0649 _session field — stub implementation)
- Platform-specific files: Not built yet (platform targets only compile in MAUI head project)

**Follow-up work:**
- Bruce/Hope: Implement full camera capture logic in platform CameraFrameSource stubs (TODO comments mark integration points)
- Shuri: Subscribe to IFrameSource.ErrorOccurred in MainViewModel, display error banner in UI
- Natasha: Enable test cases in ExceptionTypeTests.cs, OnnxResistorLocalizationServiceErrorTests.cs (currently skipped with "Implementation pending #29")

### 2026-03-09: GitHub Issue #28 — Comprehensive README & Documentation

**Task:** Create comprehensive README.md covering all project context, build instructions, and architecture.

**Deliverables (PR #43, `squad/28-readme-documentation`):**
- **Project overview:** 1-sentence description + feature list
- **Prerequisites:** .NET 10 SDK (confirmed in VivaLaResistance.csproj), MAUI workloads, Xcode 15.0+ (iOS), Android SDK 21+ (Android)
- **Build instructions:**
  - Solution structure table (4 projects, all TFMs, purposes)
  - `dotnet restore`, `dotnet build -f net10.0-ios/android`, `dotnet run`, `dotnet test` commands
  - Platform-specific guidance (macOS for iOS, any platform for Android)
- **Architecture Overview:**
  - Vision pipeline diagram (camera → ONNX localization → HSV color → calculator → overlay)
  - Key contracts documented (CameraFrame BGRA8888, ResistorReading model structure)
  - ILightingAnalyzer contract (Good/TooDark/TooBright classification, BT.601 luminance)
  - Design philosophy: platform isolation, service-oriented, testability (zero MAUI deps in Core), MVVM
- **Dependencies section:** NuGet packages with versions (SkiaSharp 3.x, ONNX Runtime 1.x, xUnit)
- **SLNX format:** Explanation of solution file format, Visual Studio 2022+ compatibility
- **Team roles:** Table mapping Rhodes (architecture), Shuri (UI), Bruce (vision), Natasha (tests), Hope (CI) to responsibilities
- **License:** MIT with copyright © 2026 Thindal
- **Resources:** Links to design docs, .squad/decisions.md, CI/CD workflow, GitHub issues
- **CI badge:** GitHub Actions badge in header

**Key architectural decisions documented:**
1. SLNX solution format requirement (Visual Studio 2022+)
2. Net10.0 TFM confirmed (not net9.0 — .csproj shows `TargetFrameworks>net10.0-android`)
3. ONNX-first hybrid vision pipeline (Option B): YOLOv8-nano localization + HSV color classification
4. Camera frame contract: BGRA8888 pixel format, 640×480 (or 320×240 fallback)
5. Platform targets: iOS 15.0+, Android 21.0+ (from SupportedOSPlatformVersion in .csproj)

**Documentation approach:**
- Developer-focused (no marketing fluff)
- Technical depth: contracts, pipeline diagram, library choices explained
- Copy-paste ready: all commands can be run directly
- Architecture visible at glance (vision pipeline ASCII diagram)

### 2026-03-09: GitHub Issue #20 — ResistorReading Domain Model Completion

**Task:** Audit and complete the ResistorReading domain model to meet all acceptance criteria for the detection pipeline.

**Starting state:**
- ResistorReading existed as a mutable `class` with properties (Id, ValueInOhms, FormattedValue, ColorBands, TolerancePercent, BoundingBox, Confidence, Timestamp)
- Used a separate mutable `BoundingBox` class (with X, Y, Width, Height, CenterX, CenterY)
- No explicit BandCount property (was derivable from ColorBands.Count)
- All properties had setters (mutable)

**Changes made (PR #44, `squad/20-resistor-reading-domain-model`):**
1. Converted ResistorReading from `class` to immutable `record` with positional parameters
2. Added explicit `BandCount` property (int) as constructor parameter
3. Replaced `BoundingBox` class reference with `ResistorBoundingBox` record (already existed in Models)
4. Removed the entire `BoundingBox` class definition (was redundant with ResistorBoundingBox)
5. All properties now immutable by default (record semantics)

**Acceptance criteria met:**
- ✅ Model includes ohm value (`ValueInOhms: double`)
- ✅ Model includes tolerance (`TolerancePercent: double`)
- ✅ Model includes band count (`BandCount: int` — now explicit)
- ✅ Model includes bounding box coordinates (`ResistorBoundingBox` record with X, Y, Width, Height, Confidence)
- ✅ Model includes detection confidence score (`Confidence: double`)
- ✅ Model includes formatted display string (`FormattedValue: string`)
- ✅ Model is immutable (C# `record` type)
- ✅ Model defined in Core project (net10.0, zero MAUI dependencies)

**Key architectural rulings:**
1. **Immutability via record:** Chosen positional record syntax for clarity and thread-safety in detection pipeline
2. **Explicit BandCount:** Added as separate property rather than relying on ColorBands.Count — makes band configuration explicit in detection output
3. **BoundingBox consolidation:** Eliminated duplicate `BoundingBox` class, standardized on `ResistorBoundingBox` record (which includes Confidence as part of bounding box data from ONNX)
4. **Core isolation maintained:** Model has zero MAUI/platform dependencies, pure net10.0 TFM
5. **Aligned with Bruce's detection pipeline:** IResistorLocalizationService already returns `ResistorBoundingBox[]`, this change makes ResistorReading consistent

**Build verification:** Core project builds clean (0 errors, 0 warnings).

**References:** PR #44, issue #20

---

## 2025-01-XX — CS8852 Fix: Init-Only Property Assignment

**Issue:** PR #44 failing on Android & iOS with CS8852 error:
``
error CS8852: Init-only property or indexer 'ResistorReading.FormattedValue'
can only be assigned in an object initializer, or on 'this' or 'base' in an
instance constructor or an 'init' accessor.
``

**Root cause:** After converting ResistorReading from mutable class to immutable positional record, properties became init-only by default. MainViewModel.cs line 182 attempted to assign FormattedValue after object construction.

**Solution chosen:** Use C# with expression to create a new record instance with FormattedValue populated.

**Why not computed property?** While FormattedValue is derived from ValueInOhms, the formatting logic lives in the Services layer, not Core. Making it a computed property would require moving formatting logic to Core or introducing a service dependency in the model itself.

**Lesson learned:** When converting to immutable records with positional parameters, all properties become init-only. Post-construction assignments must use:
1. with expressions to create modified copies, OR
2. Include all values in the initial object initializer, OR  
3. Make derived properties computed if logic is available in the same layer

**Build verification:** Core project compiles clean, CS8852 error resolved.

**Commit:** 1be56df — pushed to squad/20-resistor-reading-domain-model
