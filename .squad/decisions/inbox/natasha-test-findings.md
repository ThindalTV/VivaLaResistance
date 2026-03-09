# Natasha Test Findings — IDisposable & Frame-Skip Verification

**Date:** 2026-03-09  
**Author:** Natasha (Tester)  
**Branch:** `squad/8-resistor-detection-service`

## Finding 1: Bruce Un-Skipped Natasha's Tests

During implementation of IDisposable (#31) and frame-skip throttle (#27), Bruce included test file changes in his commit (`914a76a`):

- Un-skipped `ResistorDetectionService_ImplementsIDisposable`
- Un-skipped `OnnxResistorLocalizationService_ImplementsIDisposable`
- Replaced the frame-skip placeholder test with a real concurrency test using `TaskCompletionSource`

**Status:** Acceptable for this case. The frame-skip test was explicitly a placeholder stub (`// TODO: Implement frame skip logic`) that required genuine implementation-level knowledge to replace with a proper concurrency test. The result is correct and passes.

**Recommendation:** Going forward, the preferred workflow is for Bruce to signal Natasha when implementations are ready, and let Natasha un-skip and update tests. This preserves separation of concerns (test authoring = Natasha's domain). However, in cases where a test is a pure placeholder with implementation-specific logic, collaboration is pragmatic.

## Finding 2: `TryWait` vs `Wait(0)` on SemaphoreSlim

The previous test notes and this task's brief referenced `TryWait(0)` as the frame-skip API. `SemaphoreSlim` does not have a `TryWait` method. The correct non-blocking pattern is:

```csharp
if (!_inferenceSemaphore.Wait(0))  // Wait(millisecondsTimeout: 0)
{
    // frame dropped
    return Array.Empty<ResistorReading>();
}
```

This is what Bruce implemented and it is correct. Future documentation should use `Wait(0)` not `TryWait(0)`.

## Finding 3: Test Count Expectation Was Off

The task brief expected 118 passing after un-skipping. Correct math:
- 111 passing (before) + 3 un-skipped tests = **114 passing**
- 1 color band confidence test remains skipped (ONNX color band extraction not implemented)
- Total: **114 passing, 1 skipped** ✅

The brief may have been calculated from an earlier count or included tests not yet committed.

## Current State

| Test Suite | Count |
|---|---|
| Passing | 114 |
| Skipped (color band extraction) | 1 |
| Failing | 0 |

Branch `squad/8-resistor-detection-service` is clean and ready for PR merge review.
