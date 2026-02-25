# Architecture Decision: Revised Vision Pipeline — ONNX-First Hybrid

**Author:** Rhodes (Lead/Architect 🏗️)  
**Date:** 2026-07-18  
**Status:** APPROVED — supersedes the Phase 1/Phase 2 split from 2025-07-18  
**Triggered by:** Bruce's ONNX model research + user preference for ONNX-first

---

## Decision

**Option B: ONNX for localization + HSV for color bands — collapsed into a single implementation phase.**

I am **revising** the previous two-phase decision. The phases are collapsed, not skipped. Here's why.

---

## Rationale

### Why not Option A (Pure ONNX-first)?

My earlier ruling stands: **color classification should never be ML-ified.** HSV math for mapping pixels to the 13 resistor colors is deterministic, auditable, and doesn't need training data. An ML model for color classification would be:
- Less predictable (what does the model think "gold" looks like under fluorescent vs. daylight?)
- Harder to debug (black box vs. a lookup table)
- Unnecessary — the color classification problem is solved math, not learned features

ONNX adds value for **localization** (finding the resistor body in a cluttered frame), where learned features genuinely outperform classical CV. It does NOT add value for color reading.

### Why not Option C (Keep two-phase)?

The original two-phase plan was designed to de-risk when we had no data. Bruce's research changes the calculus:
- A 4,422-image labeled dataset exists (isha-74mjj/yolov5-u3oks)
- The YOLOv8 → ONNX pipeline is proven and one-command
- C# ONNX Runtime integration is straightforward
- The estimated effort is 1–1.5 weeks, not months

Building a full classical CV localization pipeline in Phase 1 (contour detection, edge analysis, body segmentation) would take comparable effort to just training a YOLO model — and would be thrown away in Phase 2. That's waste.

### Why Option B?

**Best of both worlds:**
1. **ONNX (YOLOv8-nano) handles localization** — finds resistor bodies in the frame, returns bounding boxes. This is what ML excels at: object detection under variable backgrounds, angles, lighting.
2. **HSV math handles color classification** — given a localized resistor body (tight crop), sample the band regions and classify colors via calibrated HSV lookup. This is deterministic, testable, and doesn't need a model.
3. **Single implementation phase** — no throwaway work. The pipeline is: ONNX detect → crop → HSV classify → calculate value.

---

## Risk Assessment

### 1. Dataset Domain Fit
**Risk: MEDIUM.** The isha-74mjj dataset is likely benchtop/PCB photos. Our app sees handheld phone camera frames — different angles, backgrounds, motion blur, lighting.

**Mitigation:**
- Train with aggressive augmentation (rotation, blur, brightness jitter, background variation)
- Collect 100–200 handheld photos early and add to training set
- The HSV color classification is isolated from this risk — if ONNX finds the resistor body (even roughly), HSV can read the bands
- **Fallback:** If ONNX underperforms on real-world frames, the failure mode is graceful — we get no detections (not wrong detections). The app simply shows "no resistor found" until the user adjusts angle/lighting. This is acceptable for v1.

### 2. Dataset License
**Risk: BLOCKING.** The license on isha-74mjj/yolov5-u3oks is unverified. This is a hard gate.

**Resolution:**
- Bruce creates a Roboflow account and verifies the license BEFORE any training begins
- If CC-BY 4.0: acceptable — attribute in app credits/about screen
- If CC-BY-NC or proprietary: we cannot ship a model trained on it. Fallback is to collect our own dataset (300–500 images minimum) or find an alternative licensed dataset
- **This is Bruce's first action item. No training starts until license is confirmed.**

### 3. Mobile Performance
**Risk: LOW.** Bruce's research shows YOLOv8n at 640px runs 50-100ms on mid-range phones. At 320px input, ~15-25ms.

**Decision:** Start at 640px. If real-device profiling shows it's too slow for the target 30fps pipeline, drop to 320px. The accuracy tradeoff at 320px is acceptable for localization (we only need a rough bounding box — HSV does the precise work).

### 4. ONNX Runtime Native Lib Resolution
**Risk: LOW but must be validated.** The Services project stays `net10.0`. ONNX Runtime NuGet should resolve native libs through the MAUI head project at build time.

**Decision:** Hope validates this in a spike build before Bruce starts integration. If native libs don't resolve, the ONNX wrapper moves to a platform-targeted project.

---

## What Changes

### Superseded Decision
The 2025-07-18 "Phase 1 (SkiaSharp HSV) → Phase 2 (ONNX)" split is **replaced** by a single unified pipeline.

### Architecture Changes

| Component | Old Plan | New Plan |
|---|---|---|
| Resistor localization | Phase 1: SkiaSharp contour detection; Phase 2: ONNX | ONNX (YOLOv8-nano) from day one |
| Color classification | HSV math (both phases) | HSV math (unchanged) |
| SkiaSharp role | Phase 1: localization + color; Phase 2: color only | Color classification only (pixel sampling, HSV conversion) |
| ONNX Runtime | Phase 2 addition | Required from day one |
| Model file | Phase 2 deliverable | Required from day one (~6MB MauiAsset) |
| Binary size impact | Phase 1: ~2MB; Phase 2: ~17MB | ~17MB from start (ONNX Runtime ~15MB + model ~6MB) |

### Interface Changes
**None.** The `IResistorDetectionService` interface is already correct:
- `InitializeAsync()` loads the ONNX model
- `DetectResistorsAsync()` runs ONNX inference → HSV classification → returns `ResistorReading[]`
- `IsInitialized` gates frame processing until model is loaded

### New Dependencies
```xml
<!-- VivaLaResistance.Services.csproj -->
<PackageReference Include="SkiaSharp" Version="3.*" />
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.*" />
```
Both are required from the start. This is a change from the phased dependency plan.

### Sprint Ordering (Revised)

| Order | Task | Owner | Depends On |
|---|---|---|---|
| 1 | Verify isha-74mjj dataset license on Roboflow | Bruce | — |
| 2 | Spike: ONNX Runtime native lib resolution in MAUI build | Hope | — |
| 3 | Train YOLOv8n on dataset, export to ONNX | Bruce | #1 license confirmed |
| 4 | Camera frame capture → BGRA8888 byte[] pipeline | Shuri | — |
| 5 | ONNX inference wrapper (load model, run detection, parse boxes) | Bruce | #2 spike passes, #3 model ready |
| 6 | HSV color band classifier (given cropped resistor body) | Bruce | #4 frame format confirmed |
| 7 | Compose pipeline: ONNX detect → crop → HSV classify → ResistorReading | Bruce | #5 + #6 |
| 8 | AR overlay rendering from ResistorReading[] | Shuri | #7 |
| 9 | Lighting UX guidance (brightness indicator) | Shuri | #4 |

**Parallelizable:** Tasks 1, 2, and 4 can run simultaneously. Task 9 can run alongside 5-7.

### Camera Frame Format Contract
**BGRA8888** is confirmed as the contract format for `DetectResistorsAsync`. This was listed as a condition in the previous decision and I'm formalizing it now:
- Camera platform code (Shuri) delivers `byte[]` in BGRA8888 layout
- ONNX preprocessing converts BGRA8888 → RGB float32 CHW tensor
- HSV classification converts BGRA8888 → HSV via SkiaSharp

---

## Conditions for This Decision

1. **License gate is hard.** Bruce verifies the Roboflow dataset license before ANY training. If the license is incompatible, we fall back to collecting our own dataset (which adds ~1 week for data collection + labeling).
2. **Hope's ONNX build spike must pass.** If ONNX Runtime native libs don't resolve through the MAUI head project, we need to restructure before Bruce starts integration.
3. **CC-BY 4.0 is acceptable.** If the dataset is CC-BY 4.0, we attribute in the app's About/Credits screen. No further approval needed.
4. **Lighting UX is still mandatory.** ONNX improves localization robustness but HSV color classification is still lighting-sensitive. The brightness indicator / "move to better light" prompt is not optional.
5. **640px default, 320px fallback.** Start with 640px ONNX input resolution. Drop to 320px only if real-device profiling shows unacceptable latency. Bruce documents the accuracy delta.

---

*Decision by Rhodes (Lead/Architect). This supersedes the 2025-07-18 Phase 1/Phase 2 split decision.*
