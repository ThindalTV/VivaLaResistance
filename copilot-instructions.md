# copilot-instructions.md

You are @copilot, the GitHub Copilot Coding Agent for **VivaLaResistance**.

## Project

A C# .NET MAUI mobile app (iOS + Android only — no desktop) that uses computer vision to detect resistors in a live camera view, calculate their resistance values from color bands, and display AR-style overlays next to each detected resistor. Supports multiple resistors per frame.

**Solution format:** SLNX (Visual Studio compatible)

**Monetization:** 7-day free trial from first launch, then a dismissible "You like this. Would you like to support it?" modal on each subsequent app start. Full functionality is always available — modal can always be dismissed.

## Tech Stack

- C# .NET MAUI
- iOS & Android targets only
- ML/Vision for resistor detection (ONNX or TensorFlow Lite preferred for on-device inference)
- GraphicsView/Canvas for AR overlay rendering
- SLNX solution format

## Team

- **Rhodes** — Lead (architecture, review gate)
- **Shuri** — Mobile Dev (UI, XAML, camera view, overlays, trial modal)
- **Bruce** — Vision/ML Dev (detection pipeline, color band algorithm)
- **Natasha** — Tester (unit tests, platform tests, quality)
- **Scribe** — Session Logger (silent)
- **Ralph** — Work Monitor
- **@copilot** — that's you

## Conventions

- Follow MVVM pattern
- Prefer dependency injection via `MauiProgram.cs`
- All new classes in appropriate namespace under `VivaLaResistance.*`
- Tests in a separate test project in the solution
- iOS and Android platform-specific code goes under `Platforms/iOS/` and `Platforms/Android/`
- No Windows or macOS targets — do not add them

## Working Style

- Create a branch named `copilot/{issue-number}-{short-slug}` for each issue
- Reference the issue number in commit messages
- Open a draft PR when work is in progress
- Convert to ready PR when complete

## Key Decisions

See `.squad/decisions.md` for authoritative team decisions.
