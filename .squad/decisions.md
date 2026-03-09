# VivaLaResistance — Team Decisions

## Active Decisions

### 2026-02-25: Target platforms
**Decision:** iOS and Android only. No Windows, macOS, or other desktop targets.
**Rationale:** User directive — keep the app mobile-focused.

### 2026-02-25: Solution format
**Decision:** Use SLNX format for the solution file.
**Rationale:** User directive — must be openable in full Visual Studio.

### 2026-02-25: Monetization model
**Decision:** Crippleware / awareness-ware (WinRAR-style). App is fully functional for 7 days from first launch. After that, display a dismissible modal on each app start: "You like this. Would you like to support it?" — user can click away to enter the full application. No feature lockout.
**Rationale:** User directive.

### 2026-02-25: Multiple resistors
**Decision:** Support detecting and displaying values for multiple resistors simultaneously in a single camera view.
**Rationale:** Core feature requirement.

### 2026-02-25: Display style
**Decision:** AR-style overlay — display calculated resistor value as text/badge rendered directly next to each detected resistor in the live camera view.
**Rationale:** Core UX requirement.

### 2025-07-18: Vision/ML Library Selection (Phase 1 & 2)
**Decision:** Two-phase approach — SkiaSharp + custom HSV (Phase 1) + ONNX Runtime + lightweight model (Phase 2).
**Rationale:** De-risks project: Phase 1 ships faster with zero ML infrastructure; Phase 2 adds robustness when real-world lighting data reveals classical CV limitations. Color classification via HSV math sufficient; ML-ifying color detection is overkill.

**Phase 1 (SkiaSharp + HSV):**
- SkiaSharp v3.* for classical CV resistor detection and color band classification
- No ML model required; minimal binary impact (~2MB)
- Services project TFM: `net10.0` (SkiaSharp supports plain .NET)
- Requires lighting UX guidance (brightness indicator/"move to better light" prompt)
- Camera frame format contract: BGRA8888 recommended

**Phase 2 (ONNX Runtime):**
- ONNX Runtime v1.* + lightweight detection model (YOLOv8-nano preferred)
- Triggers after Phase 1 ships and real-world lighting data collected
- Color classification remains SkiaSharp/HSV — no ML-ification
- Prefer pre-trained Roboflow/HuggingFace model over training custom

**Rejected:** ML.NET (not mobile), TFLite .NET (no .NET 10 packages), CoreML/ML Kit (platform-only), Emgu CV (70MB binary unacceptable)

**Conditions:**
1. Camera frame format contractually defined (BGRA8888 recommended) before implementation
2. Services TFM stays `net10.0` for Phase 1
3. Phase 2: validate ONNX Runtime native lib resolution through MAUI head before committing
4. Phase 1 lighting UX mitigation mandatory — classical CV is lighting-sensitive

### 2026-02-25: Revised Vision Pipeline — ONNX-First Hybrid (Supersedes Phase 1/Phase 2 Split)
**Decision:** Collapse two-phase plan into single unified pipeline — ONNX (YOLOv8-nano) for resistor body localization + HSV math for color band classification. Both from day one.
**Rationale:** Bruce's research found a 4,422-image labeled dataset (isha-74mjj/yolov5-u3oks) and confirmed the YOLOv8→ONNX pipeline is clean. Building throwaway classical CV localization is waste when the ONNX path takes comparable effort. Color classification stays deterministic HSV — never ML-ify color reading.

**Pipeline:** Camera frame (BGRA8888) → ONNX model detects resistor bodies → crop → HSV classifies color bands → IResistorValueCalculatorService calculates value → ResistorReading[]

**Dependencies (both required from start):**
- `SkiaSharp` v3.* — pixel sampling, HSV conversion, color classification
- `Microsoft.ML.OnnxRuntime` v1.* — ONNX model inference for localization

**Hard gates:**
1. Bruce must verify Roboflow dataset license BEFORE training (CC-BY 4.0 acceptable with attribution)
2. Hope must spike ONNX Runtime native lib resolution through MAUI head project
3. Camera frame format contract: BGRA8888 (formalized)
4. Lighting UX guidance still mandatory (HSV is lighting-sensitive)
5. 640px ONNX input default; 320px fallback based on real-device profiling

**Binary size:** ~17MB (ONNX Runtime ~15MB + model ~6MB) — accepted tradeoff for localization robustness
**Interface changes:** None — IResistorDetectionService already accommodates this architecture

**Sprint ordering (revised):**
| Task | Owner | Blocker |
|---|---|---|
| Verify dataset license | Bruce | — |
| ONNX native lib spike | Hope | — |
| Train + export ONNX model | Bruce | License ✓ |
| Camera frame pipeline (BGRA8888) | Shuri | — |
| ONNX inference wrapper | Bruce | Spike ✓, Model ✓ |
| HSV color classifier | Bruce | Frame format ✓ |
| Compose full pipeline | Bruce | Wrapper + Classifier |
| AR overlay rendering | Shuri | Pipeline |
| Lighting UX guidance | Shuri | Frame format |

### 2026-07-18: ONNX Runtime NuGet Package — Spike Complete ✅
**Decision:** ONNX Runtime 1.20.1 validated and approved for production integration.
**Spike Result:** NuGet package resolves cleanly through MAUI head build. All non-platform targets (Core, Services, Tests) build with 0 errors. One expected warning (CS0649 on stub field) — no build blockers.
**Deliverables:**
- `IResistorLocalizationService` interface contract (BGRA8888 input, normalized bounding box output)
- `ResistorBoundingBox` value type
- `OnnxResistorLocalizationService` stub implementation (ready for real inference when trained `.onnx` model available)
- DI registration in `MauiProgram.cs` (pending Rhodes review before PR #39 merge)
**Pending:** Model training pipeline (isha-74mjj/yolov5-u3oks → YOLOv8n fine-tune → ONNX export, ~1.5 weeks)
**References:** PR #39, `squad/5-onnx-spike` branch

### 2026-07-18: Camera Pipeline Implementation — Shuri Phase Complete ✅
**Decision:** Camera frame delivery contract and lighting quality assessment finalized.
**Deliverables:**
- `IFrameSource` interface — async frame emission contract; pixel format BGRA8888 (per Rhodes mandate)
- `CameraFrame` record — `byte[] Data` (BGRA8888), `Width`, `Height`, `DateTime Timestamp`
- `ILightingAnalyzer` interface — lighting quality classification (Good / TooDark / TooBright / Unknown)
- `SkiaSharpLightingAnalyzer` — center-quad sampling, BT.601 luminance, threshold-based classification (dark < 0.2, bright > 0.85)
- `LightingIndicatorView` (MAUI) — bottom banner with visual warnings for TooDark / TooBright states
- Updated XML docs on `IResistorDetectionService` — explicit BGRA8888 contractual requirement
- DI registration in `MauiProgram.cs` (`ILightingAnalyzer` as singleton)
**Open Questions for Bruce (frame source implementation):**
1. Frame delivery timing — target frame budget (e.g., 100ms / 10fps cap) to prevent pipeline flooding?
2. Thread context — raise `FrameAvailable` on capture thread or marshal to main thread?
3. Buffer ownership — is `byte[]` safe to hold across async hops, or copy immediately?
4. Frame dimensions — resize to 640×640 (or 320×320 fallback) in platform code, or does ONNX wrapper handle resize?
**Build Status:** All non-platform targets clean (0 errors).
**References:** PR #40, `squad/6-camera-pipeline` branch

### 2026-03-09: ResistorReading Domain Model — Immutability & Type Consolidation
**Decision:** Convert `ResistorReading` to an immutable record type with explicit `BandCount` property and consolidated `ResistorBoundingBox` type.
**Rationale:** Detection pipeline is async/concurrent — immutability eliminates race conditions. Explicit `BandCount` allows validation. Single bounding box type (ResistorBoundingBox) reduces cognitive load.

**Record structure:**
```csharp
public record ResistorReading(
    Guid Id,
    double ValueInOhms,
    string FormattedValue,
    double TolerancePercent,
    int BandCount,
    IReadOnlyList<ColorBand> ColorBands,
    ResistorBoundingBox BoundingBox,
    double Confidence,
    DateTimeOffset Timestamp);
```

**Key changes:**
- ✅ Converted from mutable class to record (positional syntax)
- ✅ Added explicit `BandCount` property (was implicit from `ColorBands.Count`)
- ✅ Removed mutable `BoundingBox` class; standardized on `ResistorBoundingBox` (immutable record)
- ✅ All properties immutable by default

**Consequences:**
- ✅ Thread-safe model for concurrent detection pipeline
- ✅ Value-based equality (testing benefit)
- ⚠️ Breaking change for code constructing ResistorReading (must use new positional syntax) — **Mitigation:** No production code constructs this yet
- ⚠️ Cannot partially initialize (all 9 parameters required) — **Intentional:** Forces complete data

**Verification:**
- Build succeeded: `dotnet build src/VivaLaResistance.Core/VivaLaResistance.Core.csproj` (0 errors, 0 warnings)
- All acceptance criteria met (Issue #20)

**References:** PR #44, issue #20, `squad/20-resistor-reading-domain-model`

### 2026-03-09: Error Handling Strategy — Three-Tier Approach
**Decision:** Implement three-tier error handling across VivaLaResistance: (1) global unhandled exception handlers, (2) graceful degradation in ML pipeline, (3) event-based camera error propagation.

**Tier 1 — Global Exception Handling (App Lifecycle):**
- `AppDomain.CurrentDomain.UnhandledException` → logs fatal errors, displays user-facing alert on termination
- `TaskScheduler.UnobservedTaskException` → marks exceptions as observed, prevents crash, logs for diagnostics
- Last line of defense against unobserved async exceptions

**Tier 2 — ML Inference Error Handling (Graceful Degradation):**
- `OnnxResistorLocalizationService`: model load failures → log, return successfully (don't throw)
- Inference failures → catch `OnnxRuntimeException`, return empty list
- Principle: prefer "no detection" over "app crash"

**Tier 3 — Camera Error Handling (Structured Propagation):**
- Custom exception types: `CameraPermissionException` (user message: "Please grant camera access in Settings"), `CameraUnavailableException` (user message: "Camera currently unavailable")
- Added `event EventHandler<Exception>? ErrorOccurred` to `IFrameSource` interface
- Platform implementations raise event on capture failures; ViewModels subscribe and display banners
- Throwing for `StartAsync` failures (permission denied) is correct — camera cannot start; raising ErrorOccurred for frame capture failures is correct — camera is running but frames occasionally fail

**Error Logging Levels:**
- Critical: Unhandled exceptions terminating app
- Error: ML model load failures, unexpected inference errors
- Warning: Per-frame inference failures, camera frame processing errors (recoverable)
- Info: Expected scenarios (model not yet loaded, camera stopped)

**User-Facing Error Messages:** All written for end users, not technical jargon.

**Follow-Up Work:**
- Shuri (UI): Subscribe to `IFrameSource.ErrorOccurred`, display error banner
- Bruce (Camera): Replace TODO comments with full camera capture logic, ensure permission checks before `StartAsync` returns
- Natasha (Tests): Enable skipped test cases in `ExceptionTypeTests.cs` and error handler tests

**References:** PR #45, issue #29, `squad/29-error-handling`

### 2026-07-19: PR #41 Review — Platform Camera Frame Source
**Decision:** Request changes on PR #41 (`squad/6-camera-pipeline` → `main`). Three blocking issues require resolution before merge.

**Gate Checks (Approved):**
- ✅ MauiProgram.cs `IFrameSource` DI registration uses correct `#if ANDROID / #elif IOS` preprocessor
- ✅ Singleton lifetime correct for shared camera resource
- ✅ `Microsoft.Extensions.Logging.Debug 10.0.0` NuGet addition fixes pre-existing `AddDebug` build error
- ✅ No behavioral change to monetization logic (SupportModalPage is display-only)

**Blocking Issues:**
1. **Rebase onto main required** — PR #42 (support modal) merged after branch was cut; `mergeable_state: dirty`. Files with conflicts: SupportModalPage.xaml, SupportModalPage.xaml.cs, potentially MauiProgram.cs and VivaLaResistance.csproj.
2. **Remove `.squad/agents/bruce/history.md` from PR** — Guard CI enforces no `.squad/` files on main. Fix: `git rm --cached .squad/agents/bruce/history.md`.
3. **SupportModalPage version conflict** — Branch carries older version (single dismiss button, no store URL). Main has richer PR #42 version (support + dismiss buttons, store links). After rebase, accept main's version wholesale.

**Minor Issue:**
4. **Android double-close** — In `OnImageAvailable`, `using var image = reader.AcquireLatestImage()` combined with explicit `Close()` in finally block is redundant. Remove `using` keyword or explicit `Close()`.

**Architecture Notes (Informational):**
- Camera frame format contract (BGRA8888) correctly honored
- `FrameAvailable` fires on platform capture threads (documented; subscribers must marshal to main thread)
- Android YUV→BGRA conversion is optimization opportunity for later (SIMD/Span)
- iOS zero-conversion BGRA path is optimal
- No subscriber wiring in this PR; wiring happens in future PR

**Status:** REQUEST CHANGES — awaiting rebase and conflict resolution

**References:** PR #41, `squad/6-camera-pipeline` branch

### 2026-07-18: Support Modal UX Design — Hope Decision
**Decision:** Support modal UX specification finalized with bottom-sheet (phones) / centered-modal (tablets) presentation, 500ms delay after first camera frame, single-tap dismiss.

**Trigger & Timing:**
- Show 500ms after first camera frame renders (not on app launch, so camera can start first)
- Camera continues running behind 50% scrim
- Once per app launch, not per resume

**Presentation:**
- Bottom sheet on phones (< 600dp)
- Centered modal on tablets (≥ 600dp)
- 50% scrim (`#CC000000`) allows camera visibility
- Card: 16dp corner radius, `#1a1a2e` surface, `#0f3460` accent border

**Copy:**
- Headline: "Thanks for using Viva La Resistance!"
- Body: "This app is made by one person. If it's saved you time, consider showing some support."
- Button: "Got it, thanks"

**Dismiss Behavior:**
- Single tap on button dismisses
- Tap outside (scrim) dismisses
- Android back button dismisses
- Bottom sheet swipe-down dismisses
- No confirmation, no delay

**Accessibility:**
- All text exceeds WCAG AAA contrast (14.5:1 headline, 9.8:1 body)
- 48dp minimum touch target on button
- Screen reader announces "Support reminder" dialog
- Respects reduced motion preference

**Rationale:**
- Waiting for camera init avoids "wall" feeling; shows app works first
- "One person" humanizes ask without guilt-tripping
- Single-tap dismiss respects user time, maintains WinRAR-style honesty
- No "Support" button yet (no store page exists)

**Implementation:** Spec file: `design/support-modal-spec.md` | Implementer: Shuri (branch `squad/15-support-modal`)

**Pending:** Rhodes confirmation on tone and copy before Shuri implementation

**References:** Issue #15

### 2026-07-18: Camera View Implementation — Shuri Design Decisions
**Decision:** MainPage camera view implementation with optional `IFrameSource` injection, permission-denied state handling, defensive frame marshaling, no CommunityToolkit.Maui dependency.

**Decision 1: IFrameSource injected as nullable optional parameter**
- Constructor: `MainPage(MainViewModel viewModel, IFrameSource? frameSource = null)`
- If null, page shows "Camera not available" and stays in `IsCameraInitializing` state
- No crash if `IFrameSource` not registered

**Decision 2: Permission-denied state handled in code-behind + ViewModel**
- Added `IsPermissionDenied` to `MainViewModel` (data-bindable)
- `IsCameraInitializing` is computed property: `IsCameraNotReady && !IsPermissionDenied` (mutually exclusive)
- No boolean converter needed

**Decision 3: Frame marshaling strategy**
- `FrameAvailable` handler wraps `ProcessFrameAsync` in `MainThread.BeginInvokeOnMainThread` regardless of which thread event fires
- Defensive approach (Bruce's platform thread context unconfirmed)

**Decision 4: No CommunityToolkit.Maui**
- Camera surface is pure dark `Grid` background
- `IFrameSource` events deliver frame bytes
- No XAML `CameraView` control required

**Open Questions for Bruce (IFrameSource integration):**
| # | Question | Impact |
|---|---|---|
| 1 | Will `FrameAvailable` fire on capture thread or main thread? | Affects whether `BeginInvokeOnMainThread` is redundant or required |
| 2 | After `StopAsync()` returns, can `FrameAvailable` still fire? | Unsubscribe-before-stop order matters |
| 3 | Does `IsRunning` accurately reflect state immediately after `StartAsync()` completes? | Used to guard `StopAsync()` call in `OnDisappearing` |
| 4 | Will platform impl throttle frame rate? | `ProcessFrameAsync` skips when not initialized, but uncapped 60fps would flood pipeline |

**References:** `squad/12-main-camera-view`, issue #12, PR #40

### 2026-07-18: Support Modal Implementation — Shuri Decisions
**Decision:** Use standard MAUI `ContentPage` with `Navigation.PushModalAsync()`, direct instantiation in `MainPage.xaml.cs`, TappedEventArgs.Handled workaround for overlay tap-swallowing.

**Decision 1: ContentPage over Popup Library**
- No additional NuGet dependency required
- More controllable dismiss behavior (back button, overlay tap, button tap)
- Simpler XAML (semi-transparent overlay via `BackgroundColor="#CC000000"` on ContentPage)

**Decision 2: Direct Instantiation in MainPage**
- `SupportModalPage` instantiated directly (`new SupportModalPage()`) not via DI constructor injection
- Avoids polluting `MainPage` constructor with conditionally-needed page
- `SupportModalPage` has no service dependencies (pure presentation)
- Still registered in DI as Transient (for future use)

**Decision 3: TappedEventArgs.Handled Workaround**
- MAUI's `TappedEventArgs` does not expose `Handled` property (unlike WPF)
- Card tap-swallowing achieved with empty handler body
- In practice, MAUI does not propagate `TapGestureRecognizer` tap from child `Border` up through parent `Grid`'s separate `TapGestureRecognizer`

**Non-Decision (Deferred):**
- "Support the project" secondary button omitted per spec (no store link available yet; can add when decision made)

**References:** `squad/15-support-modal`, issue #15

### 2026-07-18: Camera Implementation — Bruce Threading & Frame Delivery
**Decision:** `FrameAvailable` raised on platform capture thread (not marshalled to main). No platform-side resize; ONNX wrapper handles resize. Software throttle on Android (15 FPS cap), hardware lock on iOS (15 FPS max).

**Decision 1: Frame Source Threading Model**
- `FrameAvailable` raised on platform capture thread (Android `HandlerThread`, iOS `DispatchQueue`)
- NOT marshalled to main thread
- Rationale: Marshalling every frame adds latency, risks UI jank; ONNX pipeline handles off-thread callbacks; consumers dispatch to main thread only for final UI updates

**Decision 2: No Resize in Platform Code**
- Frames delivered at native capture dimensions (Android: 640×480 requested; iOS: device-native medium preset, typically 1280×720)
- `OnnxResistorLocalizationService` responsible for resizing to 640px input
- Rationale: Resizing in platform code duplicates work if model input size changes; single responsibility principle

**Decision 3: Software Throttle on Android, Hardware Lock on iOS**
- Android: `Camera2` `SetRepeatingRequest` delivers at device preview rate (~30 FPS); frames skipped in `OnImageAvailable` if within 66ms of last delivered frame (~15 FPS cap)
- iOS: `AVCaptureDevice.ActiveVideoMinFrameDuration = CMTime(1, 15)` — hardware-limited to 15 FPS max
- Rationale: Camera2's `ControlAeTargetFpsRange` involves complex Java generics; software throttle is simpler and achieves same result; iOS AVFoundation frame rate locking is clean and well-documented

**Pre-existing Build Fix:**
- `Microsoft.Extensions.Logging.Debug 10.0.0` added to fix pre-existing `CS1061: AddDebug` error (not new dependency introduced by this feature)

**Answers to Shuri's Open Questions (from PR #40):**
1. **Frame budget:** Android: ~15 FPS (software throttle). iOS: 15 FPS max (hardware). No pipeline flooding.
2. **Thread context:** `FrameAvailable` fires on platform capture thread; Shuri's consumer must not do heavy UI work directly — post to main thread as needed.
3. **Buffer ownership:** Each frame creates fresh `byte[]` from `new byte[w*h*4]`. Safe to hold indefinitely across async hops; no aliasing.
4. **Frame dimensions:** Native capture dimensions passed through. ONNX wrapper handles resize to 640px.

**References:** `squad/6-camera-pipeline`, issue #6, PR #41

### 2026-07-18: iOS CameraFrameSource Compilation Fixes
**Decision:** Audited iOS CameraFrameSource.cs against real MAUI production code patterns; applied three critical corrections.

**Problem:** iOS file never compile-checked (CI only ran Android). Three compilation errors found via audit against ZXing.Net.MAUI, BarcodeScanner.Mobile, dotnet/macios source.

**Fixes Applied:**

| # | What was wrong | Changed to | Source of truth |
|---|---|---|---|
| 1 | Missing `using CoreFoundation;` | Added `using CoreFoundation;` | ZXing, DrawnUi import explicitly for `DispatchQueue` |
| 2 | `new NSString("PixelFormatType")` as WeakVideoSettings key | `CVPixelBuffer.PixelFormatTypeKey` | ZXing.Net.MAUI CameraManager uses typed constant |
| 3 | `SetSampleBufferDelegateQueue(delegate, queue)` | `SetSampleBufferDelegate(delegate, queue)` | ZXing, BarcodeScanner.Mobile, CameraScanner.Maui all use this method name; no `Queue` suffix exists |

**Confirmed Correct (No Change Needed):**
- `DispatchQueue(string, bool concurrent)` constructor
- `CVPixelFormatType.CV32BGRA`
- `new AVCaptureDeviceInput(device, out var error)` constructor
- `CMSampleBuffer.GetImageBuffer()` returning `CVImageBuffer?` with `is CVPixelBuffer` cast
- `AVCaptureSession.PresetMedium`

**Build Status:** `dotnet build -f net10.0-android` → ✅ 0 errors, 3 pre-existing warnings (unchanged)

**References:** `squad/6-camera-pipeline`, issue #6, commit `699909e`
