# Issue Labeling Strategy Decision

**Date:** 2026-02-25  
**Owner:** Rhodes  
**Status:** Active

## Decision

Established a 6-category labeling system for GitHub issues to enable effective team triage and filtering.

## Labels Defined

| Label | Color | Purpose | Example Issues |
|-------|-------|---------|-----------------|
| `infrastructure` | #0075ca | Build, CI, project setup, configuration | #1 (CI/GitHub Actions), #2-3 (permissions), #4 (DI), #28-29 (docs, error handling) |
| `vision` | #e4e669 | ML/computer vision, resistor detection, camera | #5-11 (ML framework, camera, detection, multi-resistor) |
| `mobile` | #d73a4a | MAUI, UI, platform-specific mobile concerns | #12-17 (camera view, overlays, layout, modals, permissions, Fold) |
| `monetization` | #a2eeef | Trial logic, modals, payment flows | #15 (support modal), #18 (TrialService), #22 (modal design) |
| `ux` | #cfd3d7 | Design, overlays, user experience | #13 (AR overlays), #21-22 (badge/modal design), #30 (icons/splash) |
| `testing` | #0e8a16 | Tests, quality, edge cases | #23-26 (unit tests for calculator/trial, color bands, integration tests) |

## Rationale

1. **Clear Scope Boundaries:** Each label maps to a distinct technical concern. A developer can filter by label to see all work relevant to their area.

2. **Multi-Label Support:** Some issues span multiple concerns (e.g., #15 is both `mobile` and `monetization` because the support modal is a UI element AND a business feature). Multi-labeling is permitted and encouraged when appropriate.

3. **No Label Overload:** Only 6 core labels to avoid decision fatigue. Existing GitHub labels (bug, enhancement, documentation, etc.) are not overridden.

4. **Future Issues:** All new issues should be categorized using these labels on creation.

## Current Distribution (34 Issues)

- `infrastructure`: 7 issues (+ 1 shared with vision)
- `vision`: 9 issues (+ 1 shared with infrastructure)
- `mobile`: 7 issues (+ 1 shared with monetization, + 2 shared with UX)
- `monetization`: 3 issues (1 solo, 1 shared with mobile, 1 shared with UX)
- `ux`: 4 issues (1 solo, 2 shared with mobile, 1 shared with monetization)
- `testing`: 4 issues (solo)

Well-distributed across the project's major concerns.

## Implementation

Applied to all 34 open issues on 2026-02-25. Added to Rhodes' responsibility for triage and assignment going forward.
