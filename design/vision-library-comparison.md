# Vision/ML Library Comparison for Resistor Color-Band Detection

**Author:** Bruce (Vision/ML Dev 🔧)  
**Date:** 2025-07-18  
**Reviewed by:** Rhodes (Lead) — pending  
**Status:** Draft — awaiting architectural decision

---

## Problem Statement

VivaLaResistance needs to detect resistors in real-time camera frames, identify their color bands (4–6 bands per resistor), and return structured `ResistorReading` results via `IResistorDetectionService.DetectResistorsAsync(byte[], int, int)`.

This is **not** a generic object detection task. The core challenge is **color segmentation under variable lighting** — identifying thin, ordered color bands on a cylindrical body. That shapes every tradeoff below.

### Detection Pipeline (Technology-Agnostic)

Regardless of library choice, the implementation will require these stages:

```
Camera Frame (byte[])
  └─ 1. Resistor Body Localization — find the rectangular/cylindrical body
       └─ 2. Band Region Extraction — crop perpendicular scan line across bands
            └─ 3. Color Classification — map sampled pixels to 13 resistor colors
                 └─ 4. Value Calculation — delegate to IResistorValueCalculatorService
```

Stages 1–2 benefit from ML/CV; stages 3–4 are pure color math regardless of library.

### Project Constraints

| Constraint | Detail |
|---|---|
| Target frameworks | `net10.0-ios` (iOS 15+), `net10.0-android` (API 21+) |
| Services project TFM | `net10.0` (plain .NET — no platform-specific APIs without restructuring) |
| Network access | None — fully on-device, offline |
| Processing budget | ~33ms per frame (30fps target) |
| Binary size | Matters — mobile app, minimize APK/IPA bloat |

---

## Candidate Library Evaluation

### 1. Microsoft ML.NET

**NuGet:** `Microsoft.ML` (v4.x), `Microsoft.ML.ImageAnalytics`

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ⚠️ **Poor for mobile** — designed for server/desktop; iOS/Android targets not supported in `net10.0-*` MAUI targets |
| NuGet availability & maintenance | ✅ Actively maintained by Microsoft |
| On-device inference | ❌ Primary training/inference designed for server; ONNX export possible but that's ONNX Runtime then |
| Real-time mobile performance | ❌ Not intended for real-time mobile frame processing |
| Color-band detection suitability | ⚠️ Would require custom image pipeline; no built-in color segmentation |
| IResistorDetectionService integration | ⚠️ Heavy runtime, awkward in Services (`net10.0`) project |
| Community/support | ✅ Good docs, active GitHub |

**Verdict:** ❌ **Not viable.** ML.NET is a training and inference framework for server-side .NET, not a mobile inference runtime. Its mobile story is essentially "export to ONNX, use ONNX Runtime" — so it's a detour, not a destination.

---

### 2. ONNX Runtime for .NET

**NuGet:** `Microsoft.ML.OnnxRuntime` + platform packages:
- `Microsoft.ML.OnnxRuntime` (base, CPU)
- Per-platform native binaries auto-resolved on iOS/Android via NuGet
- `Microsoft.ML.OnnxRuntime.Extensions` for preprocessing ops

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ✅ **Excellent** — official iOS and Android support, NuGet resolves native libs per-platform |
| NuGet availability & maintenance | ✅ Microsoft-maintained, v1.22.x current, regular releases |
| On-device inference | ✅ Purpose-built for on-device inference — the reference runtime |
| Real-time mobile performance | ✅ Optimized for ARM (NNAPI on Android, CoreML EP on iOS as optional backend) |
| Color-band detection suitability | ✅ Run any ONNX model (YOLOv8-nano for localization + color classifier head) |
| IResistorDetectionService integration | ✅ Clean: load `.onnx` from `MauiAsset`, run in `InitializeAsync`, call in `DetectResistorsAsync` |
| Community/support | ✅ Large community, Microsoft-backed, extensive .NET samples |

**Key consideration:** ONNX Runtime is a *model runner* — we still need an ONNX model. Options:
- **Pre-trained path:** Use a public YOLOv8-nano or MobileNetV3 model fine-tuned on resistor images (datasets exist on Roboflow/HuggingFace)
- **Classical hybrid path:** Use ONNX Runtime for resistor body detection only; handle color classification in pure C# (HSV math — no model needed for the color step)

**Verdict:** ✅ **Top tier.** Best cross-platform inference runtime for .NET MAUI. The `net10.0` Services project can reference ONNX Runtime; native libs resolve at link time when the MAUI head project builds.

---

### 3. TensorFlow Lite (.NET Bindings)

**NuGet:** Community packages only — `TensorFlowLite.Binding` (Xamarin-era), unofficial wrappers

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ⚠️ **No official .NET 10 package** — Google provides Java/Swift SDKs; .NET bindings are community-maintained and lag behind |
| NuGet availability & maintenance | ❌ No first-party .NET NuGet; community packages last updated ~2022–2023 |
| On-device inference | ✅ TFLite runtime itself is excellent on-device |
| Real-time mobile performance | ✅ Fast on mobile with GPU delegate |
| Color-band detection suitability | ✅ Any TFLite model works (object detection, classifier) |
| IResistorDetectionService integration | ⚠️ Binding friction, P/Invoke marshaling required |
| Community/support | ⚠️ .NET community small; most resources are Java/Kotlin/Swift |

**Verdict:** ⚠️ **Avoid.** The .NET ecosystem story for TFLite is fragmented and not .NET 10-ready. ONNX Runtime has better .NET support and can run TFLite-origin models after conversion.

---

### 4. Apple Vision Framework / Core ML (iOS Native)

**NuGet/Binding:** `Microsoft.iOS` (built-in MAUI iOS binding) — `CoreML`, `Vision` frameworks available via `using CoreML;`, `using Vision;`

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ✅ Fully supported on `net10.0-ios` via .NET iOS bindings |
| NuGet availability & maintenance | ✅ Bundled with iOS SDK — always current |
| On-device inference | ✅ Runs on Apple Neural Engine (ANE) — fastest possible on iPhone |
| Real-time mobile performance | ✅ Exceptional — ANE acceleration, hardware-optimized |
| Color-band detection suitability | ✅ `VNCoreMLRequest` + any CoreML model; `VNDetectRectanglesRequest` for body localization |
| IResistorDetectionService integration | ❌ **iOS-only** — requires `#if IOS` guards and a parallel Android implementation |
| Community/support | ✅ Excellent Apple docs, .NET iOS binding well-documented |

**Key constraint:** The `VivaLaResistance.Services` project targets `net10.0`, not `net10.0-ios`. To use Vision/CoreML, the detection logic would need to live in the MAUI head project (platform-specific folder) or the Services project would need multi-targeting. This forces a split implementation.

**Verdict:** ⚠️ **iOS-only luxury option.** Delivers the best iOS performance, but forces a dual-implementation architecture (iOS + Android) which doubles maintenance burden. Only justified if ONNX Runtime proves insufficient on iOS.

---

### 5. Google ML Kit (Android Native)

**NuGet/Binding:** Community Java binding required — `Xamarin.Google.MLKit.*` or manual Android binding

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ⚠️ **Binding friction** — ML Kit is Java/Kotlin; .NET Android binding packages are community-maintained |
| NuGet availability & maintenance | ⚠️ `Xamarin.Google.MLKit.Vision.Common` exists but Xamarin-era naming; .NET 10 support uncertain |
| On-device inference | ✅ On-device models available |
| Real-time mobile performance | ✅ Good Android performance |
| Color-band detection suitability | ❌ No built-in resistor/color-band detector — would need Custom Model API with a trained model |
| IResistorDetectionService integration | ❌ Android-only; same dual-implementation problem as CoreML |
| Community/support | ⚠️ Strong for Java/Kotlin; weak for .NET |

**Verdict:** ❌ **Not recommended.** Android-only, requires binding maintenance, and offers no advantage over ONNX Runtime for our use case. ML Kit's strengths (text recognition, face detection) are irrelevant here.

---

### 6. SkiaSharp + Custom CV (Pure .NET)

**NuGet:** `SkiaSharp` (v3.x), `SkiaSharp.Views.Maui.Controls` for MAUI integration

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ✅ **Excellent** — first-class MAUI support, .NET 10 ready |
| NuGet availability & maintenance | ✅ Microsoft-maintained (was Google/Xamarin), v3.116.x current |
| On-device inference | ✅ Pure computation — no model, no network |
| Real-time mobile performance | ✅ SkiaSharp is GPU-accelerated via Skia; pixel manipulation is fast |
| Color-band detection suitability | ✅ **Particularly strong here** — color analysis is the *primary* task; HSV sampling + histogram analysis maps perfectly to SkiaSharp's `SKBitmap`/`SKPixmap` APIs |
| IResistorDetectionService integration | ✅ Simple: `SKBitmap.FromImage()` in `DetectResistorsAsync`, no `InitializeAsync` model load needed |
| Community/support | ✅ Large, well-documented |

**Approach details for color-band detection:**
1. Convert `byte[]` → `SKBitmap` (RGBA format, zero-copy if aligned)
2. Apply Gaussian blur to reduce noise
3. Detect resistor body via Canny-equivalent edge detection on luminance channel
4. Find bounding rectangle via contour analysis (horizontal scan for parallel edges)
5. Extract a 1-pixel-tall scan line perpendicular to the resistor axis
6. Segment scan line into bands by HSV discontinuities
7. Map each band's average HSV to the 13 `ColorBand` enum values using calibrated lookup table

**Weakness:** Sensitive to lighting variations; no learned feature extraction. Under low contrast or strong glare, classical CV degrades. Requires careful HSV calibration ranges for the 13 resistor colors (some are perceptually close: brown/red, gold/yellow).

**Verdict:** ✅ **Strong option for MVP.** No model training, no model distribution, minimal binary size impact. Appropriate as the initial implementation while ONNX model is trained/sourced. Can co-exist with ONNX approach (fallback path).

---

### 7. Emgu CV (OpenCV .NET Binding)

**NuGet:** `Emgu.CV` (v4.x / v5.x), platform packages: `Emgu.CV.runtime.maui.android`, `Emgu.CV.runtime.maui.ios`

| Criterion | Assessment |
|---|---|
| .NET 10 / MAUI compatibility | ⚠️ MAUI-specific runtime packages exist (`Emgu.CV.runtime.maui.*`) but are community-tier; .NET 10 support is trailing |
| NuGet availability & maintenance | ⚠️ Maintained but slower-moving; last MAUI packages ~2024; licensing: dual GPL/commercial |
| On-device inference | ✅ OpenCV runs fully on-device |
| Real-time mobile performance | ⚠️ Large binary (~50–80MB native libs); CPU-only on MAUI mobile |
| Color-band detection suitability | ✅ OpenCV is purpose-built for this: `cv::inRange` for HSV color masking, `cv::findContours`, `cv::HoughLines` |
| IResistorDetectionService integration | ⚠️ Manageable but binary size and license are concerns |
| Community/support | ✅ Largest CV community globally; most resistor-detection tutorials use OpenCV |

**Verdict:** ⚠️ **Viable but heavyweight.** The best classical CV toolkit available, and many existing resistor detection projects use it. However, the binary size penalty (~50–80MB added to APK/IPA) and commercial licensing cost make it a harder sell vs. a lighter SkiaSharp implementation. Consider if SkiaSharp's capabilities prove insufficient.

---

## Summary Comparison Table

| Library | iOS ✓ | Android ✓ | .NET 10 Ready | On-Device | Perf | Color-Band Fit | Binary Size | Maintenance |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **ML.NET** | ❌ | ❌ | ❌ | ❌ | N/A | ⚠️ | Heavy | ✅ |
| **ONNX Runtime** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | Medium (~15MB) | ✅ |
| **TFLite (.NET)** | ⚠️ | ⚠️ | ❌ | ✅ | ✅ | ✅ | Medium | ❌ |
| **Apple Vision/CoreML** | ✅ | ❌ | ✅ | ✅ | ✅✅ | ✅ | Zero | ✅ |
| **Google ML Kit** | ❌ | ⚠️ | ⚠️ | ✅ | ✅ | ⚠️ | Medium | ⚠️ |
| **SkiaSharp + CV** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | Tiny (~2MB) | ✅ |
| **Emgu CV** | ⚠️ | ✅ | ⚠️ | ✅ | ⚠️ | ✅ | Heavy (~70MB) | ⚠️ |

---

## Recommendation

### Primary Recommendation: Two-Phase Approach

#### Phase 1 (Ship Now): SkiaSharp + Custom HSV Color Analysis

**Why:** The resistor color-band problem is intrinsically a color analysis problem. We do not need deep learning to classify pixels as "red" or "gold" — we need accurate HSV sampling with good lighting. SkiaSharp gives us full pixel-level access with zero model distribution, zero binary size impact, and zero training data requirements.

**Implementation sketch:**
```csharp
// In ResistorDetectionService.InitializeAsync():
// Load HSV calibration table from JSON asset (MauiAsset)
// (or embed as compile-time const dictionary)

// In DetectResistorsAsync(byte[] imageData, int width, int height):
// 1. Decode bytes → SKBitmap (assume BGRA8888 from camera)
// 2. Convert to HSV in a fast scan
// 3. Find resistor contours (scan for parallel horizontal edges of beige/tan body color)
// 4. For each resistor: extract band scan line, segment by HSV delta, classify bands
// 5. Return ResistorReading[] with BoundingBox + ColorBands[] + calculated value
```

**Risk:** Challenging under poor or mixed lighting. Mitigation: Apply white-balance normalization before HSV analysis; document minimum lighting requirements; add confidence gating.

---

#### Phase 2 (Enhancement): ONNX Runtime for Robust Localization

Once Phase 1 is shipping and we have real-world lighting data, augment with:

- A **lightweight ONNX model** (YOLOv8-nano, ~3–6MB) trained on resistor images for robust body localization under varied conditions
- ONNX Runtime handles both iOS and Android with the same model file, embedded as a `MauiAsset`
- Color classification remains the SkiaSharp HSV approach — **no need to ML-ify color reading**
- The `Services.csproj` can reference `Microsoft.ML.OnnxRuntime` directly (it resolves platform native libs at MAUI head project link time)

**Why not CoreML/ML Kit for Phase 2?** Dual implementation doubles code surface. ONNX Runtime's cross-platform story is cleaner for this team's structure.

---

### Decision Summary

| Decision | Choice | Rationale |
|---|---|---|
| Phase 1 inference strategy | SkiaSharp + HSV math | No model needed; fits color-classification nature of the problem |
| Phase 2 inference runtime | ONNX Runtime | Best cross-platform .NET mobile inference runtime; single model file |
| Phase 2 model format | ONNX (YOLOv8-nano or similar) | Portable, .NET-first, hardware-accelerated options per platform |
| Rejected: ML.NET | — | Not mobile-capable |
| Rejected: TFLite .NET | — | No official .NET 10 packages |
| Rejected: CoreML / ML Kit | — | Platform-only; dual implementation cost |
| Rejected: Emgu CV | — | Binary size (~70MB) unacceptable for mobile |

---

## NuGet Packages Required

### Phase 1 (SkiaSharp)
```xml
<!-- In VivaLaResistance.Services.csproj -->
<PackageReference Include="SkiaSharp" Version="3.*" />
```

### Phase 2 (ONNX Runtime — add when model is ready)
```xml
<!-- In VivaLaResistance.Services.csproj -->
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.*" />
<!-- MAUI head project builds resolve platform native libs automatically -->
```

---

## Open Questions for Rhodes

1. **Services project TFM:** Currently `net10.0`. ONNX Runtime's NuGet resolves iOS/Android native libs correctly when the consuming project is the MAUI head (`net10.0-ios`/`net10.0-android`). Services can stay as `net10.0` for Phase 1. For Phase 2 ONNX, verify the native lib resolution chain works — if not, Services may need multi-targeting or detection logic may need to move to a MAUI-targeted project.

2. **Model acquisition:** For Phase 2, do we train our own model (requires labeled dataset ~500–1000 resistor images) or use a pre-trained Roboflow resistor detection model? Time investment differs significantly.

3. **Camera frame format:** The `DetectResistorsAsync` signature takes `byte[]` without specifying pixel format. We need a contract on format (BGRA8888? YUV NV21?). SkiaSharp and ONNX Runtime both need to know this to decode correctly.

4. **Lighting requirements:** Phase 1 SkiaSharp approach needs controlled lighting. Are we designing for this (e.g., UI guidance overlay telling user to hold under good light) or do we need Phase 2 robustness from the start?

---

*Document prepared by Bruce (Vision/ML Dev) for architectural review.*
