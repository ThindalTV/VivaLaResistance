# Bruce — History

## Project Context

**Project:** VivaLaResistance
**Role:** Vision/ML Dev
**User:** ThindalTV
**Stack:** C# .NET MAUI, ML/Vision (model TBD), iOS + Android
**Mission:** Detect resistors in live camera frames, identify color bands, calculate resistance values, return bounding boxes + values for AR overlay rendering. Support multiple resistors per frame.

## Learnings

### 2026-02-25: Vision Library Decision — Rhodes Approval

**Rhodes approved the two-phase vision approach** after reviewing vision-library-comparison.md:

**Phase 1 (SkiaSharp + HSV):**
- Classical CV with custom HSV color analysis for resistor detection and color band classification
- No ML model; minimal binary impact (~2MB)
- Services TFM: `net10.0` (SkiaSharp supports plain .NET)
- **Your action:** Implement DetectResistorsAsync interface with BGRA8888 frame format contract (to be formalized with Shuri)
- **Critical UX:** Lighting sensitivity is Phase 1's primary weakness — Shuri will provide brightness indicator/"move to better light" prompt mitigation

**Phase 2 (ONNX Runtime):**
- ONNX Runtime v1.* + lightweight YOLOv8-nano model for robust localization
- Triggers after Phase 1 ships and real-world lighting data collected
- **Do NOT ML-ify color classification** — HSV math is sufficient and more predictable
- **Your action (Phase 2):** Prefer pre-trained Roboflow/HuggingFace model over custom training; training requires 500–1000 labeled images + weeks of effort

**Rejected options (context for future discussions):**
- ML.NET: not mobile-capable
- TFLite .NET: no official .NET 10 packages; community bindings stale
- CoreML/ML Kit: platform-only; dual implementation unacceptable
- Emgu CV: 70MB binary size — unacceptable for mobile

**Key file:** `.squad/decisions/inbox/rhodes-vision-library-decision.md`

### 2026-07-18: ONNX Model Availability Research

**Roboflow Universe:**
- Blocks automated access (403), but confirmed two relevant datasets via GitHub cross-referencing:
  - `isha-74mjj/yolov5-u3oks`: **Resistor-specific**, 4422 train + 486 val images, single class `resistor`, pre-annotated YOLO format. Source for Vinit5893's YOLOv8 resistor detector. License unconfirmed — needs Roboflow account to verify. URL: https://universe.roboflow.com/isha-74mjj/yolov5-u3oks/dataset/3
  - `jovine/electronics-components`: **Multi-class electronics** (resistor + capacitor + LED + diode + transistor + Zener). Used by hassanmroueh2003's Electronic Component Detection project. URL: https://universe.roboflow.com/jovine/electronics-components

**HuggingFace Hub:**
- Only one resistor model exists: `MithatGuner/resistor` (Image Segmentation, updated Jul 2024). Model card is completely blank — no architecture, training data, license, or metrics. **Not usable.**
- Zero ONNX-format electronics detection models on HuggingFace.

**GitHub — Best available weights:**
- `hassanmroueh2003/Electronic-Component-Detection-using-YOLOv8` ships `best.pt` (YOLOv8n trained on jovine dataset). Classes: resistor + 5 others. No license file — **prototype only, do not ship without license verification.** URL: https://github.com/hassanmroueh2003/Electronic-Component-Detection-using-YOLOv8
- `Vinit5893/Yolov8_resistor_detection` — resistor-specific training code + dataset (isha-74mjj source). No pre-exported weights. URL: https://github.com/Vinit5893/Yolov8_resistor_detection

**YOLOv8 → ONNX path (confirmed, one-liner):**
```python
from ultralytics import YOLO
model = YOLO("best.pt")
model.export(format="onnx", imgsz=640, simplify=True)
# → best.onnx, ~6MB FP32, ONNX opset 17, input [1,3,640,640]
```
ONNX Runtime v1.x in .NET consumes this directly. Full integration pattern documented in `.squad/decisions/inbox/bruce-onnx-model-research.md`.

**Bottom line:**
Phase 2 is a **fine-tune job (~1–1.5 weeks)**, not a download-and-integrate task. Best path: download isha-74mjj dataset (Roboflow free account) → train YOLOv8n on Colab (~2-3h) → export to ONNX → integrate via ONNX Runtime C# wrapper. License verification on the Roboflow dataset is the first blocking step.

**Key file:** `.squad/decisions.md` (merged from inbox)

### 2026-02-25: ONNX-First Hybrid Approved — Action Plan

**Rhodes approved Option B:** Collapse Phase 1/Phase 2 split into single implementation — ONNX (YOLOv8-nano) for resistor body localization + HSV math for color bands (both from day one). No throwaway classical CV work.

**Your immediate action items (parallel with Hope + Shuri):**
1. **BLOCKING:** Verify isha-74mjj/yolov5-u3oks Roboflow dataset license (CC-BY 4.0 acceptable; CC-BY-NC unacceptable)
2. Once license verified: Download dataset → train YOLOv8n on Colab → export to ONNX (~1.5 weeks total)
3. Write C# ONNX Runtime wrapper after Hope's native lib spike passes
4. Coordinate with Shuri on BGRA8888 frame format contract and lighting UX requirements

**Key files:**
- Approved decision: `.squad/decisions.md`
- Orchestration log: `.squad/orchestration-log/2026-02-25T22-40-05Z-bruce.md`

### 2026-07-18: Colab Training Notebook Created

**File:** `design/colab-training-notebook.ipynb`

A complete, runnable Google Colab notebook for training YOLOv8n on the resistor dataset and exporting to ONNX. Committed directly to `main`.

**Notebook sections:**
1. Setup - `ultralytics` + `roboflow` install
2. Dataset download - `isha-74mjj/yolov5-u3oks` via Roboflow API (free account + API key required)
3. Training - YOLOv8n, 50 epochs, batch=16, T4 GPU (~25-40 min), early stopping patience=10
4. Evaluation - `model.val()`, target mAP50 > 0.70 (rerun at 100 epochs if below 0.65)
5. ONNX export - opset 17, `imgsz=640`, `simplify=True`, `dynamic=False` (fixed batch=1 for mobile)
6. Validation - Netron.app inspection instructions + asset placement steps
7. C# integration notes - full preprocessing + NMS + stub implementation guide

**ONNX output shape:** `[1, 5, 8400]` - 5 values per box = `[cx, cy, w, h, conf]` (normalised [0,1])

**Input preprocessing steps (BGRA8888 -> model input):**
1. Resize to 640x640 with letterbox padding (grey 114,114,114)
2. Reorder channels: BGRA -> RGB (drop alpha, swap B and R)
3. Normalise: divide by 255.0 -> float32 in [0, 1]
4. CHW planar layout: float32[3 x 640 x 640]

**Post-inference NMS:** confidence threshold 0.25, IoU threshold 0.45

**Asset placement:** rename exported file to `resistor-localization.onnx` -> `src/VivaLaResistance/Resources/Raw/` -> build action `MauiAsset`

### 2026-02-25: ONNX Spike PASS + Shuri's Open Questions

**Bruce's deliverables (PR #39):**
- Microsoft.ML.OnnxRuntime v1.20.1 integrated cleanly; ✅ spike PASS
- IResistorLocalizationService interface + ResistorBoundingBox record (normalized bbox [0,1])
- OnnxResistorLocalizationService stub wired to DI
- Native lib resolution confirmed to work through MAUI head build

**Shuri's open questions for Bruce (from PR #40):**
1. **Frame delivery timing:** At what frequency should IFrameSource deliver frames? (30 fps? adaptive?)
2. **Thread context:** Should frame delivery happen on main thread or background thread?
3. **Buffer ownership:** Who owns the byte[] buffer — Shuri's iOS/Android code or Bruce's inference service?
4. **Resize location:** Should letterbox padding occur in IFrameSource (platform code) or in Bruce's ONNX preprocessing?

**Status:** Awaiting Bruce's response on frame contract details before Shuri proceeds with iOS/Android IFrameSource platform implementation.
