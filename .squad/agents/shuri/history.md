# Shuri — History

## Project Context

**Project:** VivaLaResistance
**Role:** Mobile Dev
**User:** ThindalTV
**Stack:** C# .NET MAUI, iOS + Android only, SLNX solution, GraphicsView for AR overlays
**Mission:** MAUI camera app — live view with AR value overlays on detected resistors. 7-day trial + dismissible support modal.

## Learnings

### 2026-02-25: Vision Library Decision — Rhodes Approval & UX Requirements

**Rhodes approved the two-phase vision approach** after reviewing Bruce's vision-library-comparison.md:

**Phase 1 (SkiaSharp + HSV) — Immediate Implementation:**
- Bruce will implement classical CV with custom HSV color analysis for resistor detection and color band classification
- **Your critical action:** Design and implement lighting UX guidance (brightness indicator or "move to better light" prompt)
  - This is **not optional** — it's the mandatory mitigation for Phase 1's primary weakness: classical CV is lighting-sensitive
  - Will be essential for MVP reliability under real-world lighting conditions
- **Camera frame format contract:** Bruce and you must formalize BGRA8888 as the pixel format for `DetectResistorsAsync(byte[], int, int)` interface before implementation
  - Document in interface XML doc comments
  - BGRA8888 is native to both SkiaSharp and mobile camera APIs
- Services TFM: stays `net10.0` (SkiaSharp supports plain .NET)
- No ML infrastructure needed for Phase 1 — ships faster

**Phase 2 (ONNX Runtime + Lightweight Model) — Post-MVP:**
- ONNX Runtime v1.* + YOLOv8-nano model for robust localization
- Triggers after Phase 1 ships and real-world lighting data collected
- Before Phase 2 commitment: validate ONNX Runtime native lib resolution through the MAUI head project
  - If resolution doesn't work, multi-target Services; **do not** move detection to head project

**Decision context:** Color classification stays as SkiaSharp/HSV math — ML-ifying color detection is overkill when HSV math works.

**Key file:** `.squad/decisions/inbox/rhodes-vision-library-decision.md`

### 2026-02-25: ONNX-First Hybrid Approved — Your Action Items

**Rhodes approved Option B:** Collapse Phase 1/Phase 2 split into unified implementation starting day one. ONNX (YOLOv8-nano) for localization + HSV math for color bands.

**Your immediate action items (parallel with Bruce + Hope):**
1. **Camera frame pipeline:** Finalize BGRA8888 format contract with Bruce. Deliver byte[] in native BGRA8888 layout from camera platform code.
2. **Lighting UX is NOW MANDATORY:** HSV color classification remains lighting-sensitive. Design + implement brightness indicator or "move to better light" prompt. This is not optional for MVP reliability.
3. After Hope's ONNX native lib spike passes + Bruce's model is ready, integrate ONNX detection boxes into your AR overlay pipeline.

**Why this matters:**
- Camera frame format formalization is blocking Bruce's ONNX preprocessing (BGRA8888 → RGB float32 CHW tensor)
- Lighting UX is critical for user success under real-world conditions — HSV reads color bands; bad lighting = wrong color classification = wrong resistance value
- You can start frame capture work immediately (task 4 in sprint, parallelizable)

**Key files:**
- Approved decision: `.squad/decisions.md`
- Orchestration log: `.squad/orchestration-log/2026-02-25T22-40-05Z-rhodes.md`

### Camera Pipeline + Lighting UX (branch: squad/6-camera-pipeline, PR #40)

**Key files created:**
- `src/VivaLaResistance.Core/Interfaces/IFrameSource.cs` — camera frame delivery contract; events fire `CameraFrame` (BGRA8888)
- `src/VivaLaResistance.Core/Interfaces/ILightingAnalyzer.cs` — lighting analysis interface + `LightingQuality` enum (Good/TooDark/TooBright/Unknown)
- `src/VivaLaResistance.Core/Models/CameraFrame.cs` — record: `byte[] Data, int Width, int Height, DateTime Timestamp`
- `src/VivaLaResistance.Services/SkiaSharpLightingAnalyzer.cs` — BT.601 luminance sampling; center-1/4 region, every 4th pixel; thresholds: dark < 0.2, bright > 0.85
- `src/VivaLaResistance/Controls/LightingIndicatorView.xaml` + `.xaml.cs` — MAUI ContentView; `Quality` BindableProperty drives visibility + banner text

**BGRA8888 contract location:**
- `IFrameSource` XML doc: "All frames are delivered in BGRA8888 format"
- `IResistorDetectionService` XML doc: "Input frames MUST be in BGRA8888 format" (method + class level)
- `CameraFrame` record param doc: "Raw pixel data, BGRA8888"

**Lighting analyzer approach:**
- Pure math — no SkiaSharp API calls needed (BGRA8888 byte[] processed directly)
- Samples center 1/4 of frame every 4th row/col for performance
- BT.601 perceived luminance: `0.299R + 0.587G + 0.114B` normalized 0..1
- `IFrameSource` platform implementation deferred — stub registration not needed (interface only, no DI registration required until platform handler lands)

### 2026-07-18: MainPage Camera View + Permission Handling (branch: squad/12-main-camera-view)

**What was built:**
- `MainPage.xaml` — replaced single placeholder with two distinct UI states:
  - `IsCameraInitializing` state: shows 📷 + "Starting Camera…" while waiting for permission + IFrameSource start
  - `IsPermissionDenied` state: shows 🔒 + "Camera Access Required" + "Open Settings" inline button
- `MainPage.xaml.cs` — full IFrameSource lifecycle wiring:
  - Constructor accepts `IFrameSource? frameSource = null` (optional — safe before Bruce's DI registration lands)
  - `OnAppearing`: resets state flags → `InitializeAsync()` → permission check → `StartAsync()` → set `IsCameraNotReady = false`
  - `OnDisappearing`: `StopAsync()` (guarded by `IsRunning`) → unsubscribe event → `Cleanup()`
  - `OnFrameAvailable`: marshals to main thread via `MainThread.BeginInvokeOnMainThread` before calling `ProcessFrameAsync`
  - Permission flow: rationale alert → `RequestAsync` → denied → `DisplayAlertAsync` with "Open Settings" / "Cancel"
  - `OnOpenSettingsClicked`: `AppInfo.ShowSettingsUI()` for inline button in permission-denied state
- `MainViewModel.cs` — added `IsPermissionDenied` (bool) and `IsCameraInitializing` (computed: `IsCameraNotReady && !IsPermissionDenied`)

**Build status:** Clean — no new errors. Pre-existing `CS1061 AddDebug` error in `MauiProgram.cs` unchanged (not my scope).

**Open Questions for Bruce (IFrameSource integration):**
1. Will `FrameAvailable` be raised on the capture thread? (Code-behind uses `MainThread.BeginInvokeOnMainThread` to be safe either way)
2. After `StopAsync()`, is it guaranteed no more `FrameAvailable` events fire? (Unsubscribing first as precaution)
3. Does `IsRunning` reflect the live capture state immediately after `StartAsync()` completes?
4. Any frame rate throttling in the platform impl? (ViewModel's `ProcessFrameAsync` skips if not initialized, but pipeline flooding is still a risk)

**References:** branch `squad/12-main-camera-view`, closes issue #12

### 2026-07-18: Support Modal Implementation (branch: squad/15-support-modal, closes #15)

**What was built:**
- `src/VivaLaResistance/Views/SupportModalPage.xaml` — dark-themed full-screen ContentPage with `BackgroundColor="#CC000000"` overlay and centered card (`#1a1a2e`, `CornerRadius 24`)
- `src/VivaLaResistance/Views/SupportModalPage.xaml.cs` — dismiss via: dismiss button tap, Android back button (`OnBackButtonPressed`), tap outside card (`TapGestureRecognizer` on background Grid)
- `MainViewModel.cs` — added `ShouldShowSupportModal` bool property delegating to `ITrialService.ShouldShowSupportModal()`; also added `IsPermissionDenied` and `IsCameraInitializing` computed property (were missing)
- `MainPage.xaml.cs` — after `InitializeAsync()`, check `vm.ShouldShowSupportModal` and call `Navigation.PushModalAsync(new SupportModalPage(), animated: true)`
- `MauiProgram.cs` — registered `SupportModalPage` as Transient

**Design choices:**
- Used `ContentPage` + `Navigation.PushModalAsync` (not a popup library) — simpler, more controllable, per issue spec
- `TappedEventArgs.Handled` does not exist in MAUI — swallowed card taps via empty handler body (MAUI bubbling does not propagate through separate TapGestureRecognizers on nested views)
- `SupportModalPage` instantiated directly in `MainPage.xaml.cs` (not via DI) — avoids the need to inject it into `MainPage` constructor, keeps wiring simple
- Modal is purely dismissible — no store links, no paywall. Full app accessible after dismiss.

**Build status:** Clean — 0 errors. Pre-existing `CS0649` warning on `OnnxResistorLocalizationService` unchanged (not my scope).

**References:** branch `squad/15-support-modal`, closes issue #15

### 2026-07-18: Store Purchase Button Added to Support Modal (branch: squad/15-support-modal)

**What changed:**
- `SupportModalPage.xaml` — added "Support the project" primary button (above dismiss) with `#e94560` accent stroke; updated dismiss button to `BackgroundColor="Transparent"` + `Stroke="#444466"` (secondary style); updated body copy to reference "picking up a copy on the store" instead of donation
- `SupportModalPage.xaml.cs` — added `OnSupportTapped` async handler: uses `DeviceInfo.Platform` to select iOS App Store vs Android Play Store URI; calls `Launcher.CanOpenAsync` + `Launcher.OpenAsync`; dismisses modal after opening store

**TODO placeholders (must be replaced before release):**
- iOS URL: `https://apps.apple.com/app/idTODO` — replace `idTODO` with real Apple App ID once published
- Android URL: `https://play.google.com/store/apps/details?id=com.vivalaresistance.TODO` — replace package ID suffix once Play Store listing is live

**Build status:** Clean — 0 errors. Pre-existing `CS0649` warning on `OnnxResistorLocalizationService` unchanged (not my scope).

**References:** branch `squad/15-support-modal`, commit `a128ca5`

### 2026-03-09: Error Handling Strategy — Cross-Agent Update (Rhodes #29)

**New IFrameSource contract:** Rhodes implemented `event EventHandler<Exception>? ErrorOccurred` on `IFrameSource` interface for structured camera error propagation.

**Your action items (Shuri — UI Team):**
1. **Subscribe to ErrorOccurred** in `MainViewModel` or `MainPage.xaml.cs`
2. **Display error banner** when camera errors occur:
   - `CameraPermissionException` → "Camera permission required. Please grant access in Settings."
   - `CameraUnavailableException` → "Camera currently unavailable, no other app using it"
3. **Dismiss banner** when error state clears

**Why this matters:**
- Camera failures (permission denied, hardware unavailable) are now structured events, not silent failures
- UI must respond with user-friendly messages, not technical jargon
- Keep banner visible until user acknowledges or permission is granted

**Related decisions:**
- `.squad/decisions.md` — Error Handling Strategy (2026-03-09)
- Rhodes' PR #45 (`squad/29-error-handling`) added `IFrameSource.ErrorOccurred` event + custom exception types
- Follow-up work for Shuri listed in error handling decision

**References:** Rhodes PR #45, issue #29, `.squad/orchestration-log/2026-03-09T13-38-25Z-rhodes-sonnet-2.md`

### 2026-03-09: Camera Permission Flow & Accessibility — Sprint Complete

**Completed:** #17 (camera permission flow), #32 (accessibility)  
**PR:** #47 (Mobile UI framework — ready for review)

**Camera Permission Flow (#17):**
- Implemented two-state UI model: `IsCameraInitializing` + `IsPermissionDenied`
- Clear visual feedback for each permission state
- Settings integration via `AppInfo.ShowSettingsUI()`
- Permission recheck on `OnAppearing` for app resume handling

**Accessibility Implementation (#32):**
- All MainPage UI elements tagged with `SemanticProperties`
- Dynamic binding for status text and labels
- SupportModalPage accessibility complete
- VoiceOver/TalkBack considerations documented in code comments

**Team Impact:**
- Vision pipeline (Bruce) integrated; awaiting PR merge to unblock
- Mobile UI framework ready; blocked on camera view implementation (#12)
- 5 dependent issues ready once full-screen camera view completes (#13, #14, #16, #33)

**Open Issues:**
- #12: Full-screen camera view (blocker for #13, #14, #16, #33)
- Waiting on ResistorDetectionService PR merge from Bruce for integration

**Decision Documentation:**
- Camera permission flow pattern documented in `.squad/decisions.md`
- Accessibility baseline with SemanticProperties documented
- Pattern templates available for future permission requests

**References:** `.squad/orchestration-log/2026-03-09T16-53-52Z-shuri.md`, PR #47
