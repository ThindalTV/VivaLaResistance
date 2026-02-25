# Architecture Setup Decision

**Date:** 2026-02-25  
**Author:** Rhodes (Lead)  
**Status:** Implemented

## Summary

Established the full solution architecture for VivaLaResistance under the `/src` folder with SLNX solution format and clean project separation.

## Projects Created

| Project | Target | Purpose |
|---------|--------|---------|
| `VivaLaResistance` | net9.0-ios;net9.0-android | Main MAUI app |
| `VivaLaResistance.Core` | net9.0 | Domain models & interfaces (no MAUI) |
| `VivaLaResistance.Services` | net9.0 | Service implementations |
| `VivaLaResistance.Tests` | net9.0 | xUnit tests |

## Key Architectural Patterns

### 1. Services Pattern
All compute tasks live in service classes:
- `IResistorValueCalculatorService` — color band → ohm calculation
- `IResistorDetectionService` — ML/vision processing (stub)
- `ITrialService` — 7-day trial monetization logic

### 2. Dependency Injection
All services registered in `MauiProgram.cs`:
- Singletons for core services
- Transient for ViewModels and Pages

### 3. Testability
- Core library has zero MAUI dependencies
- `IPreferencesWrapper` abstracts MAUI Preferences API
- `IDateTimeProvider` enables time-based testing
- 66 tests pass covering calculator and trial logic

### 4. MVVM
- ViewModels in `VivaLaResistance/ViewModels/`
- Views in main project
- Data binding via `BindingContext`

## Platform Targets

- **iOS:** 15.0+
- **Android:** 21.0+
- **No desktop targets** (per project decision)

## Files Created

```
src/
├── VivaLaResistance.slnx
├── VivaLaResistance/
│   ├── VivaLaResistance.csproj
│   ├── MauiProgram.cs
│   ├── App.xaml / App.xaml.cs
│   ├── AppShell.xaml / AppShell.xaml.cs
│   ├── MainPage.xaml / MainPage.xaml.cs
│   ├── ViewModels/MainViewModel.cs
│   ├── Platforms/Android/
│   ├── Platforms/iOS/
│   └── Resources/
├── VivaLaResistance.Core/
│   ├── VivaLaResistance.Core.csproj
│   ├── Models/ColorBand.cs
│   ├── Models/ResistorReading.cs
│   └── Interfaces/ (3 interfaces)
├── VivaLaResistance.Services/
│   ├── VivaLaResistance.Services.csproj
│   ├── ResistorValueCalculatorService.cs
│   ├── TrialService.cs
│   └── ResistorDetectionService.cs
└── VivaLaResistance.Tests/
    ├── VivaLaResistance.Tests.csproj
    ├── ResistorValueCalculatorServiceTests.cs
    └── TrialServiceTests.cs
```

## Next Steps

1. **Shuri:** Implement MainPage UI with actual camera view
2. **Bruce:** Implement `ResistorDetectionService` with ML model
3. **Natasha:** Expand test coverage as features are added
