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
