# VivaLaResistance

[![Build](https://github.com/ThindalTV/VivaLaResistance/actions/workflows/ci.yml/badge.svg)](https://github.com/ThindalTV/VivaLaResistance/actions/workflows/ci.yml)

A mobile app for iOS and Android that detects resistors in real-time using your device camera and displays their calculated color-band values as AR-style overlays. Supports multiple resistors simultaneously with high-accuracy on-device computer vision.

**Platform:** iOS 15.0+, Android 21.0+

## Project Overview

VivaLaResistance uses ONNX-based computer vision to detect resistor bodies and HSV-based color analysis to read color bands from a live camera feed. The app displays calculated resistance values instantly next to each detected resistor, making component identification quick and accurate in the field or at the workbench.

**Key Features:**
- Real-time resistor detection via ONNX (YOLOv8-nano)
- Automatic color band classification using HSV math
- Multi-resistor simultaneous detection and display
- Lighting quality assessment (brightness warnings)
- AR-style text overlays positioned next to each resistor
- 7-day trial with optional support modal (fully functional after trial)

## Prerequisites

### Required
- **.NET 10 SDK** (download at https://dotnet.microsoft.com/download)
- **MAUI workloads** — install with: `dotnet workload install maui`

### iOS Build (macOS only)
- **Xcode 15.0+** with iOS 15.0+ SDK
- Command-line tools: `xcode-select --install`

### Android Build (Windows / macOS / Linux)
- **Android SDK** (API level 21+, typically via Android Studio)
- **JAVA_HOME** environment variable pointing to JDK 11+
- Optional: Android Studio for visual tooling and emulator management

## Build Instructions

### Solution Structure
The SLNX solution (`src/VivaLaResistance.slnx`) contains four projects:

| Project | TFM | Purpose |
|---------|-----|---------|
| `VivaLaResistance` | `net10.0-ios`, `net10.0-android` | MAUI app head — UI, camera platform integration, trial modal |
| `VivaLaResistance.Core` | `net10.0` | Domain models, enums, service interfaces (zero MAUI deps for testability) |
| `VivaLaResistance.Services` | `net10.0` | Service implementations — color calculation, ONNX inference, lighting analysis |
| `VivaLaResistance.Tests` | `net10.0` | xUnit test suite (66+ tests covering calculator, trial, and detection services) |

### Restore Dependencies
```bash
dotnet restore src/VivaLaResistance.slnx
```

### Build for iOS
On macOS:
```bash
dotnet build src/VivaLaResistance.slnx -f net10.0-ios -c Release
```
Then open `.ipa` in Xcode or deploy via Apple Devices window.

### Build for Android
On any platform:
```bash
dotnet build src/VivaLaResistance.slnx -f net10.0-android -c Release
```
Creates `.apk` in `bin/Release/net10.0-android/` for sideloading or Play Store signing.

### Run on Emulator / Device
**iOS (macOS):**
```bash
dotnet run -f net10.0-ios
```

**Android:**
```bash
dotnet run -f net10.0-android
```
(Ensure an Android emulator is running or device is connected via `adb`)

### Run Tests
```bash
dotnet test src/VivaLaResistance.Tests/VivaLaResistance.Tests.csproj
```
Expected: 66+ tests passing in <10 seconds.

## Architecture Overview

### Design Philosophy
- **Platform isolation:** Platform-specific code (camera capture, frame delivery) isolated in MAUI head; core vision/detection logic in platform-agnostic Services layer
- **Service-oriented:** All compute logic in stateless service classes; never in ViewModels
- **Testability:** Core library has zero MAUI dependencies; services unit-testable in isolation
- **MVVM:** Data binding via MainViewModel; DI via MauiProgram.cs

### Vision Pipeline Architecture

```
Live Camera Feed (BGRA8888 frames, 640×480)
        ↓
IFrameSource (platform: iOS AVFoundation / Android Camera2)
        ↓
MainViewModel.ProcessFrameAsync()
        ↓
IResistorLocalizationService (ONNX YOLOv8-nano inference)
        ↓
Detected Bounding Boxes (normalized coords)
        ↓
HSV Color Classifier (SkiaSharp sampling of color bands)
        ↓
IResistorValueCalculatorService (4/5/6-band resistance calculation)
        ↓
ResistorReading[] (detected value, tolerance, bands)
        ↓
MainViewModel (group readings by location)
        ↓
AR Overlay View (render badges next to each resistor)
```

### Key Contracts

**CameraFrame Record:**
```csharp
public record CameraFrame(
    byte[] Data,        // BGRA8888 pixel data (4 bytes per pixel)
    int Width,          // 640 (or 320 fallback)
    int Height,         // 480 (or 240 fallback)
    DateTime Timestamp
);
```

**ResistorReading Model:**
```csharp
public class ResistorReading
{
    public int OhmValue { get; set; }
    public string FormattedValue { get; set; }      // e.g., "4.7kΩ"
    public string Tolerance { get; set; }           // e.g., "±5%"
    public ColorBand[] Bands { get; set; }          // [Brown, Black, Red, Gold]
    public ResistorBoundingBox BoundingBox { get; set; }  // Normalized (0-1)
}
```

**Lighting Quality:**
The `ILightingAnalyzer` classifies frame brightness:
- **Good:** Center brightness 0.2–0.85 (BT.601 luminance)
- **TooDark / TooBright:** Visual warnings via `LightingIndicatorView`
- Resistor color bands are lighting-sensitive (HSV-based); users need guidance

### Dependencies

**NuGet Packages (Core):**
- `Microsoft.Maui.Controls` 10.0.20+ — MAUI framework
- `SkiaSharp` 3.* — Pixel sampling, HSV color analysis, drawing
- `Microsoft.ML.OnnxRuntime` 1.* — ONNX model inference
- `Microsoft.Extensions.Logging.Debug` 10.0.0 — Debug logging

**NuGet Packages (Tests):**
- `xunit` 2.* — Test framework
- `xunit.runner.visualstudio` 2.* — Test runner

### Solution File Format

The solution uses **SLNX format** (simplified Visual Studio 2022+ format):
```xml
<Solution>
  <Project Path="VivaLaResistance/VivaLaResistance.csproj" />
  <Project Path="VivaLaResistance.Core/VivaLaResistance.Core.csproj" />
  <Project Path="VivaLaResistance.Services/VivaLaResistance.Services.csproj" />
  <Project Path="VivaLaResistance.Tests/VivaLaResistance.Tests.csproj" />
</Solution>
```
SLNX is fully compatible with Visual Studio 2022 and supports all IDE features (refactoring, debugging, project management).

## Team & Responsibilities

| Role | Focus |
|------|-------|
| **Rhodes** (Lead) | Architecture, solution structure, ONNX integration, design decisions |
| **Shuri** | UI implementation, MAUI views, camera frame delivery, overlays |
| **Bruce** | Vision algorithms, ONNX model training/export, HSV color classification |
| **Natasha** | Test coverage, test utilities, integration validation |
| **Hope** | Build infrastructure, CI/CD, dependency management, native lib spiking |

## License

Licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

Copyright © 2026 Thindal

## Additional Resources

- **Design Documentation:** See `/design` directory for vision pipeline specifications and UI mockups
- **Decision Log:** `.squad/decisions.md` documents architectural decisions and sprint sequencing
- **CI/CD:** GitHub Actions workflow in `.github/workflows/ci.yml` — auto-builds on push to main
- **Issues:** GitHub Issues tracked at https://github.com/ThindalTV/VivaLaResistance/issues
