# VivaLaResistance — Team Decisions

## Active Decisions

### 2026-02-25: Target platforms
**Decision:** iOS and Android only. No Windows, macOS, or other desktop targets.
**Rationale:** User directive — keep the app mobile-focused.

### 2026-02-25: Solution format
**Decision:** Use SLNX format for the solution file.
**Rationale:** User directive — must be openable in full Visual Studio.

### 2026-02-25: Monetization model
**Decision:** Crippleware / awareness-ware (WinRAR-style). App is fully functional for 7 days from first launch. After that, display a dismissible modal on each app start: "You like this. Would you like to support it?" — user can click away to enter the full application. No feature lockout.
**Rationale:** User directive.

### 2026-02-25: Multiple resistors
**Decision:** Support detecting and displaying values for multiple resistors simultaneously in a single camera view.
**Rationale:** Core feature requirement.

### 2026-02-25: Display style
**Decision:** AR-style overlay — display calculated resistor value as text/badge rendered directly next to each detected resistor in the live camera view.
**Rationale:** Core UX requirement.

### 2025-07-18: Vision/ML Library Selection (Phase 1 & 2)
**Decision:** Two-phase approach — SkiaSharp + custom HSV (Phase 1) + ONNX Runtime + lightweight model (Phase 2).
**Rationale:** De-risks project: Phase 1 ships faster with zero ML infrastructure; Phase 2 adds robustness when real-world lighting data reveals classical CV limitations. Color classification via HSV math sufficient; ML-ifying color detection is overkill.

**Phase 1 (SkiaSharp + HSV):**
- SkiaSharp v3.* for classical CV resistor detection and color band classification
- No ML model required; minimal binary impact (~2MB)
- Services project TFM: `net10.0` (SkiaSharp supports plain .NET)
- Requires lighting UX guidance (brightness indicator/"move to better light" prompt)
- Camera frame format contract: BGRA8888 recommended

**Phase 2 (ONNX Runtime):**
- ONNX Runtime v1.* + lightweight detection model (YOLOv8-nano preferred)
- Triggers after Phase 1 ships and real-world lighting data collected
- Color classification remains SkiaSharp/HSV — no ML-ification
- Prefer pre-trained Roboflow/HuggingFace model over training custom

**Rejected:** ML.NET (not mobile), TFLite .NET (no .NET 10 packages), CoreML/ML Kit (platform-only), Emgu CV (70MB binary unacceptable)

**Conditions:**
1. Camera frame format contractually defined (BGRA8888 recommended) before implementation
2. Services TFM stays `net10.0` for Phase 1
3. Phase 2: validate ONNX Runtime native lib resolution through MAUI head before committing
4. Phase 1 lighting UX mitigation mandatory — classical CV is lighting-sensitive

### 2026-02-25: Revised Vision Pipeline — ONNX-First Hybrid (Supersedes Phase 1/Phase 2 Split)
**Decision:** Collapse two-phase plan into single unified pipeline — ONNX (YOLOv8-nano) for resistor body localization + HSV math for color band classification. Both from day one.
**Rationale:** Bruce's research found a 4,422-image labeled dataset (isha-74mjj/yolov5-u3oks) and confirmed the YOLOv8→ONNX pipeline is clean. Building throwaway classical CV localization is waste when the ONNX path takes comparable effort. Color classification stays deterministic HSV — never ML-ify color reading.

**Pipeline:** Camera frame (BGRA8888) → ONNX model detects resistor bodies → crop → HSV classifies color bands → IResistorValueCalculatorService calculates value → ResistorReading[]

**Dependencies (both required from start):**
- `SkiaSharp` v3.* — pixel sampling, HSV conversion, color classification
- `Microsoft.ML.OnnxRuntime` v1.* — ONNX model inference for localization

**Hard gates:**
1. Bruce must verify Roboflow dataset license BEFORE training (CC-BY 4.0 acceptable with attribution)
2. Hope must spike ONNX Runtime native lib resolution through MAUI head project
3. Camera frame format contract: BGRA8888 (formalized)
4. Lighting UX guidance still mandatory (HSV is lighting-sensitive)
5. 640px ONNX input default; 320px fallback based on real-device profiling

**Binary size:** ~17MB (ONNX Runtime ~15MB + model ~6MB) — accepted tradeoff for localization robustness
**Interface changes:** None — IResistorDetectionService already accommodates this architecture

**Sprint ordering (revised):**
| Task | Owner | Blocker |
|---|---|---|
| Verify dataset license | Bruce | — |
| ONNX native lib spike | Hope | — |
| Train + export ONNX model | Bruce | License ✓ |
| Camera frame pipeline (BGRA8888) | Shuri | — |
| ONNX inference wrapper | Bruce | Spike ✓, Model ✓ |
| HSV color classifier | Bruce | Frame format ✓ |
| Compose full pipeline | Bruce | Wrapper + Classifier |
| AR overlay rendering | Shuri | Pipeline |
| Lighting UX guidance | Shuri | Frame format |

**Decision files:**
- `.squad/decisions/inbox/bruce-onnx-model-research.md` (research & recommendation)
- `.squad/decisions/inbox/rhodes-onnx-first-decision.md` (architecture decision)
