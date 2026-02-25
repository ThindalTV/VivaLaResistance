# Shuri — History

## Project Context

**Project:** VivaLaResistance
**Role:** Mobile Dev
**User:** ThindalTV
**Stack:** C# .NET MAUI, iOS + Android only, SLNX solution, GraphicsView for AR overlays
**Mission:** MAUI camera app — live view with AR value overlays on detected resistors. 7-day trial + dismissible support modal.

## Learnings

### 2026-02-25: Vision Library Decision — Rhodes Approval & UX Requirements

**Rhodes approved the two-phase vision approach** after reviewing Bruce's vision-library-comparison.md:

**Phase 1 (SkiaSharp + HSV) — Immediate Implementation:**
- Bruce will implement classical CV with custom HSV color analysis for resistor detection and color band classification
- **Your critical action:** Design and implement lighting UX guidance (brightness indicator or "move to better light" prompt)
  - This is **not optional** — it's the mandatory mitigation for Phase 1's primary weakness: classical CV is lighting-sensitive
  - Will be essential for MVP reliability under real-world lighting conditions
- **Camera frame format contract:** Bruce and you must formalize BGRA8888 as the pixel format for `DetectResistorsAsync(byte[], int, int)` interface before implementation
  - Document in interface XML doc comments
  - BGRA8888 is native to both SkiaSharp and mobile camera APIs
- Services TFM: stays `net10.0` (SkiaSharp supports plain .NET)
- No ML infrastructure needed for Phase 1 — ships faster

**Phase 2 (ONNX Runtime + Lightweight Model) — Post-MVP:**
- ONNX Runtime v1.* + YOLOv8-nano model for robust localization
- Triggers after Phase 1 ships and real-world lighting data collected
- Before Phase 2 commitment: validate ONNX Runtime native lib resolution through the MAUI head project
  - If resolution doesn't work, multi-target Services; **do not** move detection to head project

**Decision context:** Color classification stays as SkiaSharp/HSV math — ML-ifying color detection is overkill when HSV math works.

**Key file:** `.squad/decisions/inbox/rhodes-vision-library-decision.md`

### 2026-02-25: ONNX-First Hybrid Approved — Your Action Items

**Rhodes approved Option B:** Collapse Phase 1/Phase 2 split into unified implementation starting day one. ONNX (YOLOv8-nano) for localization + HSV math for color bands.

**Your immediate action items (parallel with Bruce + Hope):**
1. **Camera frame pipeline:** Finalize BGRA8888 format contract with Bruce. Deliver byte[] in native BGRA8888 layout from camera platform code.
2. **Lighting UX is NOW MANDATORY:** HSV color classification remains lighting-sensitive. Design + implement brightness indicator or "move to better light" prompt. This is not optional for MVP reliability.
3. After Hope's ONNX native lib spike passes + Bruce's model is ready, integrate ONNX detection boxes into your AR overlay pipeline.

**Why this matters:**
- Camera frame format formalization is blocking Bruce's ONNX preprocessing (BGRA8888 → RGB float32 CHW tensor)
- Lighting UX is critical for user success under real-world conditions — HSV reads color bands; bad lighting = wrong color classification = wrong resistance value
- You can start frame capture work immediately (task 4 in sprint, parallelizable)

**Key files:**
- Approved decision: `.squad/decisions.md`
- Orchestration log: `.squad/orchestration-log/2026-02-25T22-40-05Z-rhodes.md`

### 2026-02-25: Camera Pipeline COMPLETE + IFrameSource Scaffold + Lighting UX Live

**Shuri's deliverables (PR #40):**
- IFrameSource interface (BGRA8888 contract, frames with timestamp)
- CameraFrame record model (pixel data, width, height, timestamp)
- ILightingAnalyzer interface + SkiaSharpLightingAnalyzer
  - Analyzes center 1/4 of frame via BT.601 luminance
  - Classifies as: Good / TooDark / TooBright / Unknown
- LightingIndicatorView XAML control (BindableProperty Quality)
  - Shows semi-transparent warning banner when lighting insufficient
- Updated IResistorDetectionService XML docs: BGRA8888 mandate explicitly documented
- Registered ILightingAnalyzer as singleton in MauiProgram.cs

**Open questions posted to Bruce:**
1. Frame delivery timing (30 fps? adaptive? on-demand?)
2. Thread context (main or background?)
3. Buffer ownership (who allocates/frees byte[]?)
4. Resize location (IFrameSource or Bruce's preprocessing?)

**Status:** Architecture locked. Platform implementation (iOS/Android IFrameSource) is next — blocked waiting on Bruce's responses.
