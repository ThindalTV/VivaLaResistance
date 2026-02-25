# Bruce — ONNX Runtime NuGet Spike Result

**Date:** 2026-07-18
**Branch:** `squad/5-onnx-spike`
**PR:** #39
**Author:** Bruce (Vision/ML Dev)

## Spike Result: ✅ PASS

### NuGet Package
- **Package:** `Microsoft.ML.OnnxRuntime`
- **Version:** `1.20.1`
- **Target project:** `src/VivaLaResistance.Services/VivaLaResistance.Services.csproj`
- **TFM:** `net10.0`

### Build Results

| Step | Result | Notes |
|---|---|---|
| `dotnet restore src/VivaLaResistance.slnx` | ✅ PASS | Completed in 0.9s |
| `dotnet build VivaLaResistance.Core` (net10.0) | ✅ PASS | Clean, no warnings |
| `dotnet build VivaLaResistance.Services` (net10.0) | ✅ PASS | 1 expected warning (CS0649 on stub `_session` field — never assigned, stub behavior) |

### No build-blocking issues. ONNX Runtime 1.20.1 resolves cleanly through the MAUI head build.

## Files Delivered

| File | Purpose |
|---|---|
| `src/VivaLaResistance.Core/Interfaces/IResistorLocalizationService.cs` | Contract for ONNX localization service (BGRA8888 input, normalized bounding box output) |
| `src/VivaLaResistance.Core/Models/ResistorBoundingBox.cs` | Value type for bounding box results |
| `src/VivaLaResistance.Services/OnnxResistorLocalizationService.cs` | Stub implementation compiling against ONNX Runtime `InferenceSession` |
| `src/VivaLaResistance/MauiProgram.cs` | DI registration (⚠️ **Rhodes review gate required before merge**) |

## Pending Gates

1. **Rhodes review** of `MauiProgram.cs` DI change before PR #39 merges
2. **Model training pipeline** — isha-74mjj/yolov5-u3oks dataset → YOLOv8n fine-tune → ONNX export (~1.5 weeks)
3. Implement real inference in `OnnxResistorLocalizationService` once trained `.onnx` model is available as MauiAsset
