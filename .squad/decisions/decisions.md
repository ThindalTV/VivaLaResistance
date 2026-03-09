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
