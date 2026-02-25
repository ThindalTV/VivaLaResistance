# VivaLaResistance — Squad Team

## Project Context

**Project:** VivaLaResistance — C# .NET MAUI mobile app using computer vision to detect resistors, calculate their values, and display AR-style overlays next to each resistor in the live camera view.

**Tech Stack:** C# .NET MAUI, iOS & Android (no desktop targets), ML/Vision for resistor color-band detection, SLNX solution format (Visual Studio compatible)

**User:** ThindalTV

**Mission:** Support multiple resistors in one camera view simultaneously with overlaid value displays. Crippleware monetization (WinRAR-style): free use for 7 days, then on each subsequent launch show a dismissible "You like this. Would you like to support it?" modal that users can click away to enter the full app.

---

## Members

| Name | Role | Model | Emoji |
|------|------|-------|-------|
| Rhodes | Lead | auto | 🏗️ |
| Shuri | Mobile Dev | claude-sonnet-4.5 | ⚛️ |
| Bruce | Vision/ML Dev | claude-sonnet-4.5 | 🔧 |
| Natasha | Tester | claude-sonnet-4.5 | 🧪 |
| Scribe | Session Logger | claude-haiku-4.5 | 📋 |
| Ralph | Work Monitor | — | 🔄 |
| @copilot | Coding Agent | — | 🤖 |

---

## Issue Source

**Repository:** thindaltv/VivaLaResistance
**Connected:** 2026-02-25
**Filters:** state: open

---

## Copilot Coding Agent

<!-- copilot-auto-assign: false -->

**Capability Profile:**

| Capability | Level | Notes |
|------------|-------|-------|
| C# / .NET | 🟢 Strong | MAUI, MVVM, dependency injection |
| MAUI iOS/Android | 🟡 Moderate | Platform-specific handlers may need human review |
| ML / Vision integration | 🟡 Moderate | Wiring up models; ML model training is 🔴 |
| Unit / integration tests | 🟢 Strong | xUnit, NUnit, mocking |
| UI layout / XAML | 🟢 Strong | Controls, bindings, responsive layouts |
| AR overlay rendering | 🟡 Moderate | Canvas/GraphicsView; complex transforms need review |
| Build / CI configuration | 🟢 Strong | GitHub Actions, MAUI workloads |

**Best fit for:** Well-defined implementation tasks with clear acceptance criteria, test scaffolding, boilerplate, CI setup.
**Route to human review when:** Platform-specific native interop, ML model selection/training, monetization/IAP flows.
