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

### 2026-07-18: ONNX NuGet Spike — PASS

**Spike branch:** `squad/5-onnx-spike` → PR #39

**Result: ✅ PASS**
- `Microsoft.ML.OnnxRuntime 1.20.1` added to `VivaLaResistance.Services.csproj`
- `dotnet restore src/VivaLaResistance.slnx` → succeeded (0.9s)
- `dotnet build VivaLaResistance.Core` (net10.0) → succeeded
- `dotnet build VivaLaResistance.Services` (net10.0) → succeeded with 1 expected warning (CS0649: `_session` never assigned — stub behavior, not an error)
- NuGet version locked: **1.20.1** (stable as of early 2026)

**Scaffolding delivered:**
- `IResistorLocalizationService` in `Core.Interfaces` (BGRA8888 frame contract)
- `ResistorBoundingBox` record in `Core.Models` (normalized coordinates [0,1])
- `OnnxResistorLocalizationService` stub in `Services` (compiles against ONNX Runtime, returns empty list)
- DI registration in `MauiProgram.cs` (⚠️ awaiting Rhodes review gate before merge)

**Next actions:**
1. Rhodes must review/approve MauiProgram.cs DI change before PR merges
2. Begin model training pipeline: isha-74mjj dataset → YOLOv8n → ONNX export
3. Implement real inference in `OnnxResistorLocalizationService.InitializeAsync` + `InferAsync` once model is available

**Key file:** `.squad/decisions/inbox/bruce-onnx-spike-result.md`

### 2026-07-18: Camera Frame Source Implementation — Issue #6

**Branch:** `squad/6-camera-pipeline` → commit `14a9248`

**Deliverables:**
- `Platforms/Android/CameraFrameSource.cs` — Camera2 `ImageReader` with YUV_420_888 → BGRA8888 BT.601 conversion; `HandlerThread` for callbacks; software throttle to ~15 FPS (skip frames within 66ms window)
- `Platforms/iOS/CameraFrameSource.cs` — AVFoundation `AVCaptureSession` + `AVCaptureVideoDataOutput`; hardware frame rate locked 15/10 FPS via `ActiveVideoMin/MaxFrameDuration`; `kCVPixelFormatType_32BGRA` so no pixel conversion needed; row-stride-aware `Marshal.Copy` extraction
- `MauiProgram.cs` — `IFrameSource` registered via `#if ANDROID / #elif IOS` platform guard
- `VivaLaResistance.csproj` — Added `Microsoft.Extensions.Logging.Debug 10.0.0` (fixed pre-existing `AddDebug` build error)

**Answers to Shuri's open questions (from PR #40 decision):**
1. **Frame budget:** Android: software throttle (skip frames < 66ms since last), so effective cap is ~15 FPS. iOS: hardware-locked at 15 FPS max. No pipeline flooding.
2. **Thread context:** `FrameAvailable` fires on the platform capture thread (Android `HandlerThread`, iOS `DispatchQueue`). Shuri's consumer **must not** do heavy UI work directly in the handler — post to main thread as needed.
3. **Buffer ownership:** Each frame creates a fresh `byte[]` from `new byte[w*h*4]`. Safe to hold indefinitely across async hops; no aliasing issues.
4. **Frame dimensions:** Native capture dimensions passed through (640×480 on Android, device-native on iOS). **ONNX wrapper handles resize to 640px.** Platform code does not resize.

**Build:** `dotnet build VivaLaResistance.csproj -f net10.0-android` → ✅ 0 errors, 1 pre-existing warning (CS0649 in OnnxResistorLocalizationService stub)

**Key files:** `.squad/decisions/inbox/bruce-camera-impl.md`

### 2026-07-18: iOS CameraFrameSource Compilation Fixes — Issue #6 follow-up

**Branch:** `squad/6-camera-pipeline` → commit `699909e`

**Problem:** iOS file `Platforms/iOS/CameraFrameSource.cs` was never compile-checked (CI only ran Android). Three confirmed compilation errors found via audit against real-world MAUI iOS production code (ZXing.Net.MAUI, BarcodeScanner.Mobile, dotnet/macios source).

**Fixes applied:**

| # | What was wrong | What it was changed to | Source of truth |
|---|---|---|---|
| 1 | Missing `using CoreFoundation;` | Added `using CoreFoundation;` | All real MAUI iOS files (ZXing, DrawnUi) import this explicitly for `DispatchQueue` |
| 2 | `new NSString("PixelFormatType")` as WeakVideoSettings key | `CVPixelBuffer.PixelFormatTypeKey` | ZXing.Net.MAUI CameraManager uses typed constant |
| 3 | `SetSampleBufferDelegateQueue(delegate, queue)` | `SetSampleBufferDelegate(delegate, queue)` | ZXing, BarcodeScanner.Mobile, CameraScanner.Maui all use this method name; no `Queue` suffix exists in the .NET MAUI binding |

**Confirmed correct (no change needed):**
- `DispatchQueue(string, bool concurrent)` — constructor exists in dotnet/macios `src/CoreFoundation/Dispatch.cs`
- `CVPixelFormatType.CV32BGRA` — confirmed in ZXing.Net.MAUI and BarcodeScanner.Mobile
- `new AVCaptureDeviceInput(device, out var error)` — constructor confirmed in BarcodeScanner.Mobile iOS code
- `CMSampleBuffer.GetImageBuffer()` returning `CVImageBuffer?` with `is CVPixelBuffer` cast — confirmed in ZXing CaptureDelegate
- `AVCaptureSession.PresetMedium` — confirmed in ZXing.Net.MAUI

**Android build after changes:** `dotnet build -f net10.0-android` → ✅ 0 errors, 3 pre-existing warnings (unchanged)
