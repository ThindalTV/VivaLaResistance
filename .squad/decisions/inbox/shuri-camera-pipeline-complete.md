# Camera Pipeline Implementation Complete — Shuri

**Date:** 2026-07-18  
**Branch:** `squad/6-camera-pipeline`  
**PR:** #40

## What Was Built

### New Interfaces (Core)
- **`IFrameSource`** — camera frame delivery contract. Emits `CameraFrame` events. BGRA8888 is the contractual pixel format per Rhodes mandate.
- **`ILightingAnalyzer`** — lighting quality assessment interface, paired with `LightingQuality` enum (Good / TooDark / TooBright / Unknown).

### New Models (Core)
- **`CameraFrame`** — record carrying `byte[] Data` (BGRA8888), `Width`, `Height`, `DateTime Timestamp`.

### New Services (Services)
- **`SkiaSharpLightingAnalyzer`** — samples center 1/4 of each frame (every 4th row/col for speed), computes BT.601 perceived luminance, classifies against thresholds (dark < 0.2, bright > 0.85). No actual SkiaSharp API calls — operates directly on the BGRA8888 byte array.

### New UI Control (MAUI head)
- **`LightingIndicatorView`** — semi-transparent dark banner anchored to the bottom of the camera view. `Quality` BindableProperty drives visibility. Displays:
  - `"⚠️ Too dark — move to better light"` when `TooDark`
  - `"⚠️ Too bright — avoid direct light"` when `TooBright`
  - Hidden for `Good` / `Unknown`

### Updated
- **`IResistorDetectionService`** — class-level and method-level XML docs now explicitly state BGRA8888 requirement.
- **`MauiProgram.cs`** — `ILightingAnalyzer` registered as singleton via `SkiaSharpLightingAnalyzer`.

## Build Status
All non-platform targets (Core, Services, Tests) build clean — 0 errors.

## Open Questions for Bruce

1. **Frame delivery timing:** At what rate will `IFrameSource` emit frames? Is there a target frame budget (e.g., every 100ms / 10fps cap) Shuri should enforce in the platform implementation to avoid flooding the detection pipeline?
2. **Thread context:** Should `FrameAvailable` be raised on the capture thread or marshalled to the main thread? Bruce's ONNX preprocessing likely wants a background thread — confirming this will inform the platform handler implementation.
3. **Buffer ownership:** Is the `byte[]` in `CameraFrame` safe to hold across async hops, or should Bruce copy it immediately in the `FrameAvailable` handler?
4. **Frame dimensions:** Should Shuri resize the frame to 640×640 (or 320×320 fallback) before raising the event, or does Bruce's ONNX wrapper handle the resize itself?
