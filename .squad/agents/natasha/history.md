# Natasha — History

## Project Context

**Project:** VivaLaResistance
**Role:** Tester
**User:** ThindalTV
**Stack:** C# .NET MAUI, iOS + Android, xUnit or NUnit for tests
**Mission:** Ensure quality across resistor detection logic, AR overlay rendering, trial/monetization logic, and multi-resistor handling. Platform tests for iOS and Android.

## Learnings

### 2026-03-08: Issue #29 Error Handling Tests
**Task:** Write comprehensive error handling tests for issue #29 (error handling strategy).
**Branch:** `squad/29-error-handling`
**Deliverables:**
- `OnnxResistorLocalizationServiceErrorTests.cs` — 6 tests validating ML inference error handling (null frames, zero dimensions, model not found, pre-initialization, logging verification)
- `ExceptionTypeTests.cs` — 10 tests for `CameraPermissionException` and `CameraUnavailableException` (inheritance, constructors, messages, sibling relationships)
- `ResistorReadingModelTests.cs` — 13 tests for `ResistorReading` and `BoundingBox` domain models (properties, confidence ranges, unique IDs, center calculations, record equality)
**Total:** 29 new tests, all passing (96 total tests in suite)
**Key Findings:**
- Rhodes had already implemented error handling in `OnnxResistorLocalizationService` with graceful degradation (returns empty results on errors, never throws)
- Exception types (`CameraPermissionException`, `CameraUnavailableException`) were already implemented in `Core/Exceptions` with standard constructors and meaningful default messages
- `ResistorReading` model was reverted from record to class (with `BoundingBox` class for nested bounding box)
- `ResistorBoundingBox` (different from `BoundingBox`) is a record used for localization service output
- Added `Moq` and `Microsoft.Extensions.Logging.Abstractions` packages to test project for mocking ILogger
- Floating-point precision matters: used `Assert.Equal(expected, actual, precision: 10)` for center coordinate calculations
**Test Patterns Learned:**
- xUnit warns on `Assert.NotNull()` for value types (e.g., `DateTimeOffset`) — use meaningful assertions instead
- Record equality tests are simple with positional records (e.g., `ResistorBoundingBox`)
- Class equality requires custom implementation or instance comparison
- Mocking `ILogger<T>` with Moq: verify log calls with `LogLevel`, `EventId`, and message content matchers

