# Session Log: ONNX Architecture Decision
**Date:** 2026-02-25T22:40:05Z  
**Topic:** Architecture Decision — ONNX-First Hybrid Pipeline  
**Status:** APPROVED

## Decision
Rhodes approved ONNX-first hybrid architecture: ONNX (YOLOv8-nano) for resistor body localization + HSV math for color band classification. Collapses prior Phase 1/Phase 2 split into single unified implementation. Key rationale: 4,422-image Roboflow dataset exists; YOLOv8→ONNX pipeline proven; ~1–1.5 week estimate comparable to throwaway classical CV.

## Blockers
1. Dataset license verification (Roboflow isha-74mjj/yolov5-u3oks — CC-BY 4.0 acceptable)
2. ONNX Runtime native lib resolution spike (Hope)
3. Lighting UX mandatory (Shuri)
4. Camera frame format contract BGRA8888 (Shuri + Bruce)

## References
- `.squad/decisions/inbox/bruce-onnx-model-research.md`
- `.squad/decisions/inbox/rhodes-onnx-first-decision.md`
- `.squad/orchestration-log/2026-02-25T22-40-05Z-bruce.md`
- `.squad/orchestration-log/2026-02-25T22-40-05Z-rhodes.md`
