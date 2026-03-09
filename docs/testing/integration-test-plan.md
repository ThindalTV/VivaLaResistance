# Integration Test Plan — Camera & Detection Pipeline

**Project:** VivaLaResistance  
**Author:** Natasha (Tester)  
**Issues:** #26  
**Last Updated:** 2026-03-10  
**Status:** Draft

---

## 1. Overview

This document describes the integration testing strategy for the full camera-to-overlay pipeline:

```
Camera Frame
    ↓
ONNX Localization (OnnxResistorLocalizationService)
    ↓
ResistorReading (Id: Guid, BoundingBox, ColorBands, Confidence)
    ↓
ResistorValueCalculatorService → double Ω + formatted string
    ↓
ResistorOverlayDrawable → AR badge rendered on canvas
```

Integration tests verify that these layers communicate correctly end-to-end — not just in isolation.

---

## 2. Test Scenarios

### 2.1 Single Resistor — End-to-End Detection to Overlay

| # | Scenario | Expected |
|---|----------|----------|
| E01 | Camera frame contains one 4-band resistor, well-lit, centred | Single badge rendered at correct bounding box; value formatted correctly |
| E02 | Camera frame contains one 5-band resistor | 3-digit base value calculated; badge shows correct kΩ/MΩ formatted value |
| E03 | Camera frame contains one 6-band resistor | Resistance calculated from first 5 bands; temperature coefficient band ignored in display |
| E04 | High-confidence detection (≥ 0.65) | Badge appears immediately on next frame |
| E05 | Low-confidence detection (< 0.65) | No badge rendered; hysteresis zone respected |
| E06 | Detection disappears from frame | Badge removed within 1 frame; no ghost overlays |

### 2.2 Multiple Resistors in Frame

| # | Scenario | Expected |
|---|----------|----------|
| M01 | 2 resistors simultaneously in frame | 2 independent badges; each tracks its own bounding box |
| M02 | 3 resistors simultaneously | 3 independent badges; all visible without overlap artifacts |
| M03 | 5+ resistors simultaneously | All detected resistors display badges; no missing or incorrect assignments |
| M04 | 2 resistors, one leaves frame | Remaining badge persists; departed badge disappears cleanly |
| M05 | Resistors enter/exit rapidly | Hysteresis prevents flickering; badges stable during transitions |
| M06 | 2 resistors with same colour bands | Both badges show correct (identical) value; no cross-contamination |

### 2.3 AR Overlay Rendering Correctness

| # | Scenario | Expected |
|---|----------|----------|
| O01 | Resistor moves across frame | Badge follows bounding box in real time; no lag > 1 frame |
| O02 | Resistor rotates ±45° | Badge remains anchored to bounding box centroid; text stays horizontal |
| O03 | Badge at screen edge | Badge clipped or repositioned to stay fully on-screen; never truncated |
| O04 | Portrait orientation | All badges legible; layout matches portrait aspect ratio |
| O05 | High DPI screen (3x) | Badge text sharp and readable; no pixelation |
| O06 | Theme: light background | Badge contrast sufficient; text readable against pale backgrounds |

---

## 3. Test Data Requirements

### 3.1 Sample Images

A reference image library must be assembled before integration testing can begin.

**Minimum image set:**

| Category | Count | Notes |
|----------|-------|-------|
| 4-band resistors | 20 | Cover common values: 100Ω, 470Ω, 1kΩ, 4.7kΩ, 10kΩ, 47kΩ, 100kΩ, 1MΩ |
| 5-band resistors | 15 | Cover precision values: 1kΩ 1%, 4.75kΩ 0.5%, 33.2kΩ 2% |
| 6-band resistors | 10 | Same values as 5-band with temperature coefficient band visible |
| Sub-ohm resistors | 5 | Gold/Silver multiplier bands (e.g., 0.22Ω, 1Ω) |
| Multiple-resistor frames | 15 | 2, 3, and 5+ resistors; various spacings |
| Edge-angle frames | 10 | 15°, 30°, 45° tilt from camera perpendicular |
| Partially obscured | 8 | One band hidden by finger, shadow, or adjacent object |
| Challenging lighting | 12 | See §7 for lighting conditions |

**Image formats:**
- Still images: JPEG and PNG, minimum 1280×720
- Video clips (for latency and tracking tests): 30 fps, ≥ 5 seconds each

**Recommended physical resistors for shooting:**
- E24 and E96 series values
- Both axial (through-hole) and SMD with visible bands where applicable

### 3.2 Reference Values

Each test image must have a corresponding ground-truth JSON record:

```json
{
  "image": "4band_4k7_470ohm.jpg",
  "bands": ["Yellow", "Violet", "Brown", "Gold"],
  "expected_resistance": 4700,
  "expected_formatted": "4.7kΩ",
  "tolerance_percent": 5.0
}
```

---

## 4. Success Criteria

### 4.1 Detection Accuracy

| Metric | Target |
|--------|--------|
| Correct band identification (all bands) | ≥ 85% on reference image set |
| Correct resistance value ± tolerance | ≥ 90% of correctly identified resistors |
| False positive rate (overlay on non-resistor) | < 2% |
| Missed detection rate (resistor in frame, no badge) | < 10% under optimal lighting |

### 4.2 Overlay Latency

| Metric | Target |
|--------|--------|
| Badge first-appear latency (frame containing resistor → badge visible) | ≤ 2 frames (≈ 67 ms at 30 fps) |
| Badge tracking lag (resistor moves → badge follows) | ≤ 1 frame |
| Badge removal after resistor leaves frame | ≤ 1 frame |

### 4.3 Badge Legibility

| Metric | Target |
|--------|--------|
| Text minimum height on device screen | ≥ 14 sp |
| Contrast ratio (badge text vs. badge background) | ≥ 4.5:1 (WCAG AA) |
| Badge does not obscure resistor body | Badge anchored above bounding box top edge |

---

## 5. Manual Testing Procedures

### 5.1 iOS Device Test Procedure

**Prerequisites:** Xcode installed, iOS device on USB, app deployed in Debug configuration.

1. **Launch** app on physical iPhone (minimum iPhone SE 2nd gen / iOS 16).
2. **Baseline sanity** — point camera at empty table for 5 seconds; confirm no badges appear.
3. **Single resistor** — hold one 4.7kΩ resistor (Yellow-Violet-Red-Gold) in front of camera.
   - Confirm badge appears within ~2 frames of resistor becoming fully visible.
   - Confirm displayed value is "4.7kΩ".
   - Move resistor slowly left/right; confirm badge tracks.
   - Remove resistor; confirm badge disappears immediately.
4. **Repeat step 3** with: 470Ω (Yellow-Violet-Brown-Gold), 10kΩ (Brown-Black-Orange-Gold), 1MΩ (Brown-Black-Green-Gold).
5. **5-band** — use 4.75kΩ 0.5% (Yellow-Violet-Green-Brown-Green); confirm "4.75kΩ".
6. **Multiple resistors** — hold 2 resistors simultaneously; confirm 2 separate badges.
7. **Angle test** — tilt resistor to ~30°; confirm badge persists (may reduce confidence).
8. **Shadow test** — partially cover one band with a finger; confirm graceful degradation (badge may disappear or show wrong value; must NOT crash).
9. **Rapid movement** — quickly wave resistor past camera; confirm no persistent ghost badges.
10. **Background noise** — hold resistor near striped fabric or PCB; confirm no false positives on background stripes.

Record results in `docs/testing/results/ios-manual-YYYY-MM-DD.md`.

### 5.2 Android Device Test Procedure

**Prerequisites:** Android device with USB debugging enabled, app deployed via `dotnet-android`.

Follow the same steps as §5.1 with the following additional Android-specific checks:

- **Camera2 API** — verify `CameraService` initialises correctly (check logcat for errors).
- **Permissions dialog** — on first launch, confirm camera permission dialog appears and granting it opens the camera view correctly.
- **Low-RAM device** — repeat steps 3–6 on a device with ≤ 3 GB RAM; confirm no OOM or significant FPS drop.
- **Background/foreground cycle** — during detection, press Home, then return to app; confirm detection resumes without restart.
- **Screen rotation guard** — confirm app stays in portrait; auto-rotate must be ignored.

Record results in `docs/testing/results/android-manual-YYYY-MM-DD.md`.

---

## 6. Automated Integration Test Feasibility

### 6.1 What CAN Be Automated (No Hardware Required)

| Component | Approach | Tooling |
|-----------|----------|---------|
| `OnnxResistorLocalizationService` with mock frames | Inject pre-rendered bitmap frames; assert `ResistorReading` list | xUnit + mock camera provider |
| `ResistorValueCalculatorService` (already unit tested) | Full value calculation pipeline with all band combinations | xUnit (existing) |
| `ResistorOverlayDrawable` hysteresis logic | Source-file linking pattern (already implemented in PR #54) | xUnit (existing) |
| Full pipeline: mock frame → readings → formatted value | Provide static JPEG, verify formatted string | xUnit + mock ICamera |
| Value formatting round-trip | CalculateResistance + FormatResistance for all E24/E96 values | xUnit data-driven |

**Recommended next automation task:**  
Create `OnnxPipelineIntegrationTests.cs` using pre-cropped test images stored under `src/VivaLaResistance.Tests/TestData/`. Feed images through `OnnxResistorLocalizationService` and assert that returned `ResistorReading` values match ground-truth JSON.

### 6.2 What REQUIRES Real Hardware

| Test | Why Manual |
|------|------------|
| Camera initialisation and frame capture | MAUI camera APIs unavailable in unit test host |
| AR overlay rendering on actual screen | Canvas rendering requires GPU/display |
| Touch interaction (dismissal of trial modal) | UI automation requires device |
| iOS/Android permission dialogs | OS-level UI, not testable in process |
| Frame rate / latency under real camera load | Emulator does not represent production CPU/GPU |
| HSV colour extraction accuracy | Depends on real sensor; currently skipped (see `DetectResistorsAsync_WithConfidenceThreshold_FiltersLowConfidenceDetections`) |

---

## 7. Lighting Conditions

All detection accuracy targets (§4.1) must be met under each condition below.

| Condition | Description | Pass Threshold |
|-----------|-------------|----------------|
| **Optimal** | Diffuse indoor daylight, no shadows | ≥ 90% band ID accuracy |
| **Bright direct** | Sunlight or spotlight directly on resistor | ≥ 75% (specular hotspot may obscure bands) |
| **Dim** | Low-light interior (~50 lux) | ≥ 70% |
| **Mixed/backlit** | Window behind resistor (strong backlight) | ≥ 65% |
| **Fluorescent flicker** | Some 60 Hz fluorescent tubes | ≥ 80% (rolling shutter artefacts expected) |
| **Coloured ambient** | Coloured LED grow light, warm tungsten | ≥ 70% (white balance shift affects band colours) |

---

## 8. Edge Cases

### 8.1 Angle and Orientation

| Case | Expected Behaviour |
|------|--------------------|
| Resistor tilted 15° from camera | Detection maintained; badge tracks correctly |
| Resistor tilted 30° | Detection may drop below 0.65 confidence; badge disappears without error |
| Resistor tilted 45° | Acceptable miss; no crash |
| Resistor at 90° (face-on, bands not visible) | No detection; no badge; no crash |
| Upside-down resistor | May detect with reversed band order; out-of-scope for v1 |

### 8.2 Occlusion and Proximity

| Case | Expected Behaviour |
|------|--------------------|
| One band partially occluded by finger | Confidence drops; badge may disappear; no crash |
| Two bands occluded | Confidence below threshold; no badge |
| Two resistors overlapping (touching) | Each may be independently detected or merged into one bounding box; no crash; no duplicate Guid |
| Very close together (< 5 mm gap) | Bounding boxes may overlap; hysteresis prevents flickering |
| Resistor on PCB with other components | No false badge on non-resistor components adjacent to real resistor |

### 8.3 Extreme Values

| Case | Expected Behaviour |
|------|--------------------|
| Sub-ohm resistor (Gold/Silver multiplier) | Value displayed as fractional Ω (e.g., "0.22Ω") |
| 1 GΩ resistor (White multiplier) | Badge shows "1GΩ"; no overflow |
| Very long resistor body (wirewound) | Detection bounding box may be wide; badge still centred |

---

## 9. Regression Test Checklist

Run this checklist after **any** change to detection or overlay code before merging to `main`.

### 9.1 Automated (run locally before push)

```bash
cd src && dotnet test VivaLaResistance.Tests/VivaLaResistance.Tests.csproj -p:TargetFramework=net10.0
```

Verify:
- [ ] All previously passing tests still pass
- [ ] No new failures or unexpected skips
- [ ] Test count has not decreased (guard against accidental test deletion)

### 9.2 Manual Smoke Test (run on at least one physical device)

After every detection or overlay PR:

- [ ] **E01** — Single 4-band resistor detected, correct value displayed
- [ ] **E04/E05** — High-confidence badge appears; low-confidence (hand covering lens) no badge
- [ ] **M01** — Two resistors → two independent badges
- [ ] **O01** — Badge tracks moving resistor smoothly
- [ ] **O06** — Badge legible on both light and dark backgrounds
- [ ] No crash on any of the above scenarios
- [ ] Trial modal still appears correctly if ≥ 7 days since first launch (or on reset)

### 9.3 After Changes to `ResistorValueCalculatorService`

- [ ] Run `ResistorValueCalculatorServiceTests` and `ResistorValueCalculatorEdgeCaseTests`
- [ ] Manually verify at least one 4-band and one 5-band resistor display correctly
- [ ] Spot-check `FormatResistance` output for sub-ohm, kΩ, MΩ, and GΩ ranges

### 9.4 After Changes to `ResistorOverlayDrawable`

- [ ] Run `ResistorOverlayDrawableTests` (13 hysteresis tests)
- [ ] Manually verify badge appears/disappears at 0.65/0.60 thresholds by covering camera slowly
- [ ] Verify multi-resistor independence (step M01 above)

---

## 10. Open Items

| # | Item | Owner | Priority |
|---|------|-------|----------|
| I01 | Assemble reference image library (§3.1) | Bruce / Natasha | High |
| I02 | Create ground-truth JSON records for all reference images | Natasha | High |
| I03 | Implement `OnnxPipelineIntegrationTests.cs` with mock frames | Natasha | Medium |
| I04 | Define Android minimum API level for camera tests (§5.2) | Rhodes / Shuri | Medium |
| I05 | Establish CI job to run integration tests against reference images | Rhodes | Medium |
| I06 | Determine latency measurement tooling on device | Natasha | Low |
