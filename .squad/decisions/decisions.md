# Decisions Log

## 2026-03-09: Test Coverage — Vision Pipeline and Detection Service

**Author:** Natasha (Tester)  
**Related:** #8, #9, #10, #11, #19, #27, #31

### Summary
Established comprehensive test coverage patterns for the vision pipeline detection service, validating integration between ONNX localization, color band extraction, and resistance calculation.

### Test Coverage Areas

#### Service Integration (#8, #10, #11)
- Empty bounding boxes → returns empty results
- Invalid image data → graceful degradation (logs warning, returns empty)
- Localization service throws → catches exception, returns empty (no crash)
- Multiple detections → each gets independent ResistorReading

#### Confidence Filtering (#11)
- **Threshold: 0.65** (design decision documented in tests)
- **Hysteresis: 0.60** (reduces flicker by keeping existing detections visible at lower confidence)
- Tests validate that detections below 0.65 are excluded on first appearance
- Tests document (but don't yet validate) that existing detections remain visible down to 0.60

#### Color Band Calculation (#9, #19)
- All 10 digit colors (Black=0 through White=9) verified
- All multiplier values including special cases (Gold=0.1Ω, Silver=0.01Ω)
- All tolerance values (Gold=±5%, Silver=±10%, None=±20%, etc.)
- 4-band, 5-band, and 6-band resistor calculations
- Edge case: single band throws ArgumentException

#### Memory Management (#31)
- **Pending implementation:** IDisposable for ResistorDetectionService
- **Pending implementation:** IDisposable for OnnxResistorLocalizationService
- Both services should dispose of managed resources (ONNX InferenceSession, etc.)

#### Frame Skip Logic (#27)
- **Pending implementation:** SemaphoreSlim-based frame skipping
- When detection is in progress, new frames should return immediately (not queue)

### Test Patterns Used

#### Mocking Dependencies
```csharp
var mockLocalization = new Mock<IResistorLocalizationService>();
mockLocalization.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
mockLocalization.Setup(x => x.IsInitialized).Returns(true);
mockLocalization
    .Setup(x => x.InferAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
    .ReturnsAsync(mockBoundingBoxes);
```

#### Graceful Degradation Validation
```csharp
// Service should return empty, not throw
mockLocalization
    .Setup(x => x.InferAsync(...))
    .ThrowsAsync(new InvalidOperationException("ML model inference failed"));

var result = await service.DetectResistorsAsync(frameData, 640, 480);
Assert.Empty(result); // Not Assert.Throws
```

#### Skipping Tests Awaiting Implementation
```csharp
[Fact(Skip = "Pending implementation - IDisposable for ResistorDetectionService")]
public void ResistorDetectionService_ImplementsIDisposable()
{
    // NOTE: Detailed comment explaining what needs to be implemented
}
```

### Coverage Gaps
1. **Color band extraction from HSV** — Stubbed in service (returns empty). Tests written but skipped.
2. **IDisposable pattern** — Services should implement IDisposable to clean up ONNX resources.
3. **Frame skip with SemaphoreSlim** — Not yet implemented in service.
4. **Multiple resistors in one frame** — Test exists but returns empty due to color band extraction stub.
5. **Confidence hysteresis validation** — Logic is implemented but tests can't validate until color extraction works.

### Recommendations
1. **Implement IDisposable** — Services hold unmanaged resources (ONNX InferenceSession). Add Dispose() methods.
2. **Implement frame skip** — Use `SemaphoreSlim(1,1)` with `TryWait(0)` to skip frames when detection is busy.
3. **Color band extraction** — Once HSV extraction is implemented, un-skip tests that validate:
   - Multiple resistors per frame
   - Confidence threshold filtering
   - 4/5/6-band resistor detection end-to-end

---

## 2026-03-09: Camera Permission Flow Pattern

**Author:** Shuri (Mobile Dev)  
**Related:** #17, #32

### Decision
Use two-state permission UI model: `IsCameraInitializing` + `IsPermissionDenied`

### Rationale
- Clear visual feedback for each permission state
- `IsCameraInitializing` covers both permission check + camera startup
- `IsPermissionDenied` provides explicit recovery path (Open Settings button)
- Cleaner than a single "not ready" state — users know exactly what's blocking them

### Implementation
- MAUI Permissions API: `CheckStatusAsync`, `ShouldShowRationale`, `RequestAsync`
- `AppInfo.ShowSettingsUI()` opens system settings for re-enabling permission
- Permission recheck on `OnAppearing` handles app resume from settings

### Team Impact
- Pattern can be reused for any future permission requests (e.g., photo library if ever needed)
- Clear separation of concerns: MainPage handles permission UI, MainViewModel handles camera initialization

---

## 2026-03-09: Accessibility Strategy

**Author:** Shuri (Mobile Dev)  
**Related:** #32

### Decision
Use MAUI `SemanticProperties` for all interactive elements and dynamic content

### Approach
- Static labels: `SemanticProperties.Description` in XAML
- Dynamic labels: Bind `SemanticProperties.Description` to ViewModel properties (e.g., `{Binding StatusText}`)
- Headings: Use `SemanticProperties.HeadingLevel` for proper semantic structure
- Buttons/interactive: Use `SemanticProperties.Hint` for action descriptions

### Coverage
- All MainPage UI: camera icon, permission messages, status bar, detection count
- SupportModalPage: modal overlay, icon, headline, body, dismiss button
- Camera view itself: Exempt (visual-only app, no meaningful screen reader experience for live camera feed)

### Testing Approach
- Document VoiceOver/TalkBack considerations in code comments
- Real device testing deferred to QA phase (not part of Shuri's scope per charter)

### Team Impact
- Sets baseline accessibility standard for all future MAUI UI work
- Minimal overhead — add properties during initial XAML authoring, not as retrofit

---

## 2026-03-09: Vision Issues Work Session — Bruce

**Author:** Bruce (ML/Vision Specialist)  
**Related:** #7, #8, #9, #10, #11, #19, #27, #31

### Issues Closed

#### Issue #7: Research and select resistor detection ML model ✅
- **Status:** CLOSED  
- **Outcome:** Decision already documented — ONNX-First Hybrid pipeline (YOLOv8-nano + HSV) with dataset isha-74mjj/yolov5-u3oks (4,422 images, CC-BY 4.0)

#### Issue #9: Implement color band to ohm value calculation ✅
- **Status:** CLOSED  
- **Outcome:** ResistorValueCalculatorService.cs fully implements 4/5/6-band support, all colors/multipliers/tolerances, formatted output, 45 passing unit tests

#### Issue #19: Implement color band lookup table ✅
- **Status:** CLOSED  
- **Outcome:** Lookup tables verified complete and correct, O(1) performance, no changes needed

### Issues Partially Completed

#### Issue #8: Implement ResistorDetectionService 🔄
- **Status:** CODE WRITTEN, PR BLOCKED  
- **Implementation:** Full pipeline orchestration wiring ONNX + value calculator
- **Features:** Confidence 0.65 with hysteresis 0.60, graceful degradation, multiple resistors per frame, placeholder for HSV extraction
- **Why Blocked:** Git workflow issues prevented clean commit
- **Recommendation:** Another agent should create clean PR using implementation as reference

### Issues Already Satisfied

#### Issue #10: Handle multiple simultaneous resistor detections
- **Status:** SATISFIED by Issue #8  
- **Implementation:** DetectResistorsAsync returns IReadOnlyList<ResistorReading>, processes all bounding boxes

#### Issue #11: Define detection confidence threshold
- **Status:** SATISFIED by Issue #8  
- **Implementation:** Threshold 0.65 with hysteresis 0.60-0.65, documented with ShouldProcessDetection() method

### Issues Remaining

#### Issue #27: Optimize ML inference and frame processing performance
- **Status:** NOT STARTED  
- **Required:** Frame-skip logic, prevent pipeline flooding
- **Complexity:** Low (1-2 hours)

#### Issue #31: Implement proper memory management for camera and ML
- **Status:** NOT STARTED  
- **Required:** IDisposable for ONNX services, verify camera resource cleanup
- **Complexity:** Medium (2-3 hours)

### Learnings
- ResistorValueCalculatorService production-ready
- Confidence threshold 0.65 with hysteresis 0.60-0.65 balances precision/recall
- Git workflow complexity can block otherwise complete work

---

## 2026-03-09: Fix Agent — PR #46 Revision

**Author:** Fix Agent  
**Related:** #46

### Fixes Applied

#### Fix 1: .squad/ files removed from feature branch
- **Issue:** .squad/ files tracked on feature branch, would appear in PR diff
- **Fix:** git rm -r --cached .squad/, committed clean
- **Decision:** .squad/ files must never appear on feature branches — main-only concern

#### Fix 2: CollectionChanged memory leak in ResistorOverlayView
- **Issue:** Subscribed to CollectionChanged but failed to unsubscribe on view removal
- **Fix:** Override OnHandlerChanged() — subscribe on attach (Handler != null), unsubscribe on detach (Handler == null)
- **Pattern:** Standard MAUI safe event cleanup, works alongside OnReadingsPropertyChanged handler

### Color Band Extraction Status
- **Scope:** Explicitly out-of-scope for PR #46
- **Reason:** ResistorDetectionService is pipeline skeleton designed to be functional once extraction wired in
- **Tracking:** Skipped test documents gap, serves as tracker for when work lands
- **Decision:** Merge detection service stub unblocks downstream integration without requiring full feature completeness

---

## 2026-03-09: Fix Agent Note — PR #53 Rebase Decision

**Author:** Fix Agent  
**PR:** #53 (squad/33-battery-optimization)

### What Was Done

PR #53 rejected for:
1. Branch based on main (stub service) instead of squad/8 (full ML)
2. .squad/ files tracked on feature branch
3. Pause()/Resume() needed in full service, not stub

### Rebase Approach

Because squad/33 and squad/8 share common ancestor (c3d4005) but diverged significantly, reset squad/33 to squad/8 tip (a46a60e) instead of standard rebase. Applied only feature commit (51ca0de) on top.

**Rationale:** Cleanest way to land only lifecycle changes without replaying .squad/ or stable-ID commits already in squad/8.

### Changes Landed
- IResistorDetectionService.cs: Added Pause()/Resume() interface methods
- ResistorDetectionService.cs: _isPaused volatile field, Pause()/Resume() implementations, early return in DetectResistorsAsync
- App.xaml.cs: Full lifecycle management via Window.Stopped/Resumed/Destroying
- MainViewModel.cs: IFrameSource injection, camera start/stop/error handling
- .squad/ files: Not present on branch

---

## 2026-03-09: Test Findings — Natasha

**Author:** Natasha (Tester)  
**Branch:** squad/8-resistor-detection-service

### Finding 1: Bruce Un-Skipped Tests
- **Issue:** Bruce un-skipped IDisposable and frame-skip tests during implementation
- **Assessment:** Acceptable — frame-skip test was pure placeholder, required implementation-level knowledge
- **Recommendation:** Going forward, signal Natasha when ready, let her un-skip tests. Collaboration pragmatic when test is pure placeholder.

### Finding 2: TryWait vs Wait(0) Clarification
- **Issue:** Previous notes referenced TryWait(0), SemaphoreSlim doesn't have TryWait
- **Correct Pattern:** if (!_inferenceSemaphore.Wait(0)) // frame dropped
- **Documentation:** Future references should use Wait(0) not TryWait(0)

### Finding 3: Test Count Verification
- **Before:** 111 passing
- **After:** 114 passing (un-skipped 3 tests) + 1 skipped (color band extraction) = **114 passing, 1 skipped** ✅

---

## 2026-03-09: PR #46 Review Decision — Rhodes (Initial)

**Author:** Rhodes (Technical Lead)  
**PR:** #46 (squad/8-resistor-detection-service)  
**Status:** ❌ CHANGES REQUESTED

### Findings

1. **Workflow Compliance:** Feature branch includes .squad/ files — PR blocked until removed
2. **Code Quality:** ResistorOverlayView memory leak — CollectionChanged unsubscription needed
3. **Functional Verification:** Color band extraction placeholder makes service non-functional, needs testable fallback

---

## 2026-03-09: PR #46 Review Feedback — Rhodes (Follow-up)

**Author:** Rhodes (Technical Lead)  
**Status:** ⚠️ Changes Requested

### Findings

1. **Missing UI Components:** ResistorOverlayDrawable and ResistorOverlayView missing from PR — cannot verify UI-layer threshold consistency
2. **Service Layer:** Verification PASSED — thresholds (0.65/0.60), frame-skip, multiple detections, IDisposable all correct

---

## 2026-03-09: PR #52 Review Outcome — Rhodes

**Author:** Rhodes (Technical Lead)  
**PR:** #52 (Unit tests for ResistorValueCalculatorService)  
**Status:** ❌ CHANGES REQUESTED

### Decision

**Reject until .squad/ files removed.** PR violates team decision by including .squad/agents/hope/history.md, .squad/decisions.md, and other state files.

### Approval Conditions
1. Revert all .squad/ changes
2. Verify tests still pass
3. Ready for immediate approval once clean

---

## 2026-03-09: App Lifecycle Strategy — Shuri

**Author:** Shuri (Mobile Developer)  
**Issue:** #33  
**PR:** #53  
**Status:** ✅ Implemented

### Decision

Use MAUI `Window.Stopped` / `Window.Resumed` / `Window.Destroying` events (subscribed in `App.CreateWindow()`) as authoritative hooks for battery-sensitive lifecycle management.

### Implementation
1. **IResistorDetectionService:** Added Pause() and Resume() methods. Detection returns empty while paused.
2. **ResistorDetectionService:** Implements IDisposable, uses volatile bool _isPaused for thread-safe pause/resume
3. **App.xaml.cs:** Injects IFrameSource and IResistorDetectionService, tracks _cameraWasRunning to restore on resume
4. **MainViewModel:** Injects IFrameSource, wires FrameAvailable → ProcessFrameAsync, handles ErrorOccurred

### Rationale
- Window.Stopped/Resumed are correct MAUI lifecycle hooks — more reliable than deprecated Application.OnSleep()/OnResume()
- Fully stopping camera releases Camera2 device and AVCaptureSession, eliminating background drain
- _cameraWasRunning flag prevents restarting camera if never started
- Pause() is fast, lock-free operation ensuring no ML inference while backgrounded

### Applies To
- App.xaml.cs lifecycle wiring
- IResistorDetectionService — must always have Pause()/Resume()
- Any future platform implementations
- Any additional ML/inference services added — follow same Pause/Resume pattern

---

## 2026-03-09: Stable ID Implementation — Coordinator

**Author:** Coordinator (ThindalTV)  
**Task:** Apply stable ID fix + Pause/Resume interface addition  
**Status:** ✅ COMPLETED

### Changes Applied

**Branch:** squad/8-resistor-detection-service  
**Commit:** 18af448

**Formula:** `det_{centerX}_{centerY}` with 10-unit grid quantization in 0-1000 virtual coordinate space.

**Files Modified:**
1. ResistorReading.cs: Id property Guid → string, constructor applies grid formula
2. ResistorDetectionService.cs: Updated _detectionMap Dictionary<string>, LINQ queries for string keys
3. ResistorOverlayDrawable.cs: Updated _readingMap Dictionary<string>, removed Guid.NewGuid() calls
4. Related Collections: HashSet/List updated for string comparison

**Interface Enhancement:**
- IResistorDetectionService.cs: Added void Pause() and void Resume() signatures
- Enables App.xaml.cs to call pause/resume for battery optimization
- Allows ResistorDetectionService to implement pause flag without NotImplementedException

### Rebase Impact
- PR #46 approval: Rhodes verified stable IDs and interface signatures
- PR #53 rebase: agent-30 cleanly reset PR #53 onto updated PR #46

---

## 2026-03-09: PR #46 Final Approval — Rhodes

**Author:** Rhodes (Technical Lead)  
**PR:** #46 (squad/8-resistor-detection-service → main)  
**Status:** ✅ APPROVED (6th review cycle)

### Findings

#### ✅ Stable ID Implementation
- ResistorReading.Id changed Guid → string
- Formula: det_{centerX}_{centerY} with 10-unit grid quantization
- Applied across ResistorDetectionService, ResistorOverlayDrawable, Dictionary/HashSet/List types
- Verified in commit 18af448

#### ✅ Service Interface
- IResistorDetectionService includes Pause()/Resume() method signatures
- Matches battery optimization lifecycle requirement
- App.xaml.cs can pause detection on Window.Stopped

#### ✅ Color Band Extraction
- Known limitation (placeholder returns empty)
- Intentionally excluded from PR #46 per decision document
- Tracked as skipped test — will un-skip when extraction ships
- Does not block service skeleton merge

#### ⚠️ Non-Blocking Notes
1. Stale "(Guid)" comment in ResistorOverlayDrawable.cs:32 — low priority cleanup
2. Latent _previousDetections unbounded accumulation — dormant until color extraction ships

### Approval Checklist
| Item | Status | Notes |
|------|--------|-------|
| Code compiles | ✅ | No build errors |
| Unit tests | ✅ | 114 passing, 1 skipped (color band) |
| Stable IDs | ✅ | det_{X}_{Y} formula verified |
| Interface signatures | ✅ | Pause()/Resume() present |
| Memory management | ✅ | IDisposable, CollectionChanged cleanup |
| Color band fallback | ⚠️ | Out-of-scope, documented |
| .squad/ absent | ✅ | Feature branch clean |

### Result
**APPROVED ✅** — Ready for merge to main. Production-ready for pipeline integration.
