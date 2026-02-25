# Skill: ONNX Model Sourcing for Mobile Deployment

## When to Use
When Phase 2 of a mobile ML feature requires a detection model and the question is: "Can we find a pre-trained model, or do we need to train?"

## The Reality Check Heuristic

**For niche detection targets (electronics, specialized objects):**
- HuggingFace Hub: near-empty. Great for LLMs and image generation; sparse for specialized CV detection models.
- Roboflow Universe: the real source. Contains thousands of community-annotated detection datasets. Models are in PyTorch `.pt` format (not ONNX directly) but trivially exportable.
- Pre-trained ONNX models for specialized tasks essentially don't exist as downloadable artifacts. You will always need a training step.

## Standard Pipeline: Roboflow Dataset → ONNX Runtime .NET

```
1. Find dataset on Roboflow Universe (free account)
   → Verify license (CC-BY 4.0 is common; some are non-commercial)
   → Download in "YOLOv8 PyTorch" format

2. Train YOLOv8n on Google Colab (free GPU, ~2-3h for 50 epochs, 4000 images)
   from ultralytics import YOLO
   model = YOLO('yolov8n.pt')
   model.train(data='data.yaml', epochs=50, imgsz=640)

3. Export to ONNX (one-liner, ~30 min)
   model.export(format="onnx", imgsz=640, simplify=True)
   → Produces: best.onnx (~6MB FP32, ONNX opset 17)

4. Embed in .NET MAUI as MauiAsset
   → Load with Microsoft.ML.OnnxRuntime InferenceSession
   → Input: float32 tensor [1, 3, H, W] normalized to [0,1], CHW layout
   → Output: [1, 4+num_classes, anchors] — bboxes + class scores, requires NMS post-processing
```

## License Verification — Do This First
Before training on a Roboflow dataset, verify the license:
- Log in to Roboflow, navigate to dataset, check the "License" field
- CC-BY 4.0 = usable in commercial apps with attribution
- "Roboflow Public" with no explicit license = ask dataset author before shipping

## Mobile Performance Sizing
| Input Resolution | YOLOv8n Inference (mid-range phone CPU) | Notes |
|---|---|---|
| 640×640 | ~50-100ms | Best accuracy, may miss 30fps target |
| 416×416 | ~30-50ms | Good balance |
| 320×320 | ~15-25ms | Real-time safe, accuracy reduction ~5-10% mAP |

Start with 640px for development/eval, then benchmark on target device and reduce if needed.

## Anti-Patterns
- **Don't look for pre-exported ONNX artifacts** for specialized detection targets — they don't exist. The workflow is always: dataset → train → export.
- **Don't start from COCO weights zero-shot** if your target class isn't in COCO. Fine-tune instead.
- **Don't skip license verification** before building training pipelines on dataset.
- **Don't train on only benchtop/PCB photos** if the app will see handheld objects — add variety to training data.

## Minimum Viable Dataset Size
- 300-500 images: usable (0.65-0.75 mAP50 typical for simple shapes)
- 1000-2000 images: good (0.75-0.85 mAP50)
- 4000+ images: production-ready for most cases
