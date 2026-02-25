# Skill: Mobile ML Library Selection for .NET MAUI

## When to Use
Evaluating ML/CV libraries for on-device inference in a .NET MAUI mobile app (iOS + Android).

## Key Heuristic
**Cross-platform first.** Never choose a platform-only library (CoreML, ML Kit) as the primary path — dual implementations double maintenance. Use ONNX Runtime as the cross-platform inference runtime for .NET MAUI.

## Decision Framework

1. **Is the problem fundamentally color/pixel math?** → Start with SkiaSharp + classical CV. No model needed.
2. **Do you need learned feature extraction (object detection, segmentation)?** → ONNX Runtime + lightweight model (YOLOv8-nano, MobileNetV3).
3. **Is binary size critical?** → SkiaSharp (~2MB) >> ONNX Runtime (~15MB) >> Emgu CV (~70MB).
4. **Is the Services project `net10.0` (non-platform)?** → SkiaSharp and ONNX Runtime both work. Native libs resolve at MAUI head link time.

## Anti-Patterns
- **Don't ML-ify color classification.** HSV math is deterministic and faster than a neural network for mapping pixels to discrete color categories.
- **Don't use TFLite in .NET.** No official .NET 10 packages; community bindings are stale.
- **Don't use Emgu CV on mobile.** Binary size (~50-80MB) is unacceptable for mobile apps.

## Phase Pattern
Ship classical CV first (fast, no ML infrastructure), add ML robustness later when you have real-world data showing where classical fails.
