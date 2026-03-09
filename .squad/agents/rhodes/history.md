# Rhodes — Technical Lead History

## Project Context
**Project:** VivaLaResistance — C# .NET MAUI mobile app (iOS & Android) that uses computer vision to detect resistors in a live camera view, calculates their values from color bands, and displays AR-style overlays.

**Tech Stack:** C# .NET MAUI, iOS & Android only, ML/Vision for color-band detection, SLNX solution format.

**User:** ThindalTV

**Key Responsibilities:** Architecture oversight, code review, decision arbitration, team coordination.

## Learnings

### 2026-03-09 — PR Review: App Store Listing Materials (Issue #34, PR #51)

**Status:** ✅ APPROVED

**Reviewed Deliverables:**
- `design/app-store/description-ios.md` — iOS App Store listing
- `design/app-store/description-android.md` — Google Play listing
- `design/app-store/screenshots-spec.md` — Screenshot requirements and specs
- `design/app-store/privacy-policy.md` — Privacy policy with brand positioning
- `design/app-store/release-notes-v1.0.md` — v1.0 release notes

**Assessment:**
- All copy, messaging, and store listing materials meet quality standards
- Brand positioning ("Point. See. Know.") is strong, memorable, and differentiated
- Privacy-first messaging aligns with product reality and is rare in competitor landscape
- Monetization description is honest and compliant with Apple/Google store policies
- Screenshot strategy is well-planned and actionable

**Outstanding Items (reviewer comments posted to PR #51):**
1. **Privacy Policy Placeholders:** Fill date and email placeholders before submission
2. **Android Minimum Version:** Verify Android 8.0 (API 26) minimum version matches Shuri's build config
3. **Screenshot Capture:** Actual app screenshots needed per provided spec and overlay templates
4. **Symbol Compatibility:** Verify Ω (Ohm) symbol rendering across target Android and iOS versions

**Approved For:** Merge to squad/34-app-store-listing (without .squad/ commits per directive).

---

## Core Context

- **Git Workflow:** `.squad/` commits only on main branch; feature PRs contain only source code (per ThindalTV directive 2026-03-09).
- **Design System:** Dark-first color palette (#121212 primary, #4FC3F7 accent), camera-focused UI, semi-transparent badge overlays.
- **Monetization:** WinRAR-style support dialog — respectful, optional, no feature lockout.
- **Quality Gate:** Code review sign-off required for all PRs before merge.
