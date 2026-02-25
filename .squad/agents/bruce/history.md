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
