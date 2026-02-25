# CI Architecture Decisions — Rhodes

**Date:** 2026-02-25
**Issue:** #1 — Set up CI pipeline (GitHub Actions)
**Author:** Rhodes (Lead)

## Decisions Made

### 1. Three-job CI structure (not matrix)

**Decision:** Separate `test`, `build-android`, and `build-ios` jobs rather than a single matrix job.

**Rationale:** iOS requires `macos-latest` while Android and tests work fine on `ubuntu-latest` (lower cost, faster). A matrix would force all three onto the same runner type. Separate jobs give us the right runner per target with explicit naming.

### 2. iOS build uses `/p:BuildIpa=false`

**Decision:** iOS job passes `-p:BuildIpa=false` to dotnet build.

**Rationale:** CI has no Apple provisioning profile or signing certificate. `BuildIpa=false` lets the build compile and link without attempting to produce a signed `.ipa`, so the job validates the code compiles correctly without requiring secrets.

### 3. Test job does not install MAUI workload

**Decision:** The `test` job runs `dotnet test` without installing the MAUI workload.

**Rationale:** `VivaLaResistance.Tests` targets `net9.0` (plain .NET, no MAUI). Installing the MAUI workload adds ~3 minutes to the job for no benefit. Tests remain fast and workload-independent.

### 4. NuGet cache keyed per OS

**Decision:** Cache key is `${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`.

**Rationale:** macOS and Linux NuGet caches should not be shared — native binaries differ. OS prefix prevents cache cross-contamination between the ios and android/test jobs.

### 5. Workflow file named `ci.yml` (not overwriting squad workflows)

**Decision:** New workflow is `.github/workflows/ci.yml`. All existing `squad-*.yml` and `sync-squad-labels.yml` files are untouched.

**Rationale:** Squad automation workflows are infrastructure for the agent system and must not be disrupted by app CI changes.
