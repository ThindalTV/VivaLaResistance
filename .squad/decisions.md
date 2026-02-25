# VivaLaResistance — Team Decisions

## Active Decisions

### 2026-02-25: Target platforms
**Decision:** iOS and Android only. No Windows, macOS, or other desktop targets.
**Rationale:** User directive — keep the app mobile-focused.

### 2026-02-25: Solution format
**Decision:** Use SLNX format for the solution file.
**Rationale:** User directive — must be openable in full Visual Studio.

### 2026-02-25: Monetization model
**Decision:** Crippleware / awareness-ware (WinRAR-style). App is fully functional for 7 days from first launch. After that, display a dismissible modal on each app start: "You like this. Would you like to support it?" — user can click away to enter the full application. No feature lockout.
**Rationale:** User directive.

### 2026-02-25: Multiple resistors
**Decision:** Support detecting and displaying values for multiple resistors simultaneously in a single camera view.
**Rationale:** Core feature requirement.

### 2026-02-25: Display style
**Decision:** AR-style overlay — display calculated resistor value as text/badge rendered directly next to each detected resistor in the live camera view.
**Rationale:** Core UX requirement.

### 2026-02-25: Workflow discipline
**Decision:** Work on one issue at a time. Create a new branch for each issue (branch naming: squad/{issue-number}-{slug}). When an issue is done, open a pull request before moving to the next.
**Rationale:** User directive — structured workflow for team coordination.

### 2026-02-25: Documentation structure
**Decision:** /design for design docs (visual and technical), /tech for technical documentation, /docs for user documentation.
**Rationale:** User directive — clear separation of concerns.

### 2026-02-25: Gallery permissions
**Decision:** The app will NEVER need access to the user's picture gallery. Live camera view only. Do not request gallery/media permissions on either platform.
**Rationale:** User directive — scope decision, camera-only interaction model.

### 2026-02-25: Responsive design
**Decision:** The app must work well on standard phones, foldable phones (Samsung Fold inner/outer), and tablets. Layout must adapt to different screen sizes and aspect ratios.
**Rationale:** User directive — broad device coverage.

### 2026-02-25: Offline-first architecture
**Decision:** All computation must run fully on-device. The app must function without any internet connection. No cloud APIs, no remote ML inference, no network calls for core functionality.
**Rationale:** User directive — important application requirement.

### 2026-02-25: UX Design Guidelines
**Decision:** Established comprehensive UX guidelines covering color palette (dark-first, #121212 primary, #4FC3F7 accent), typography, overlay badge design, support dialog tone, portrait-only orientation, and no active detection indicators.
**Rationale:** Design best practices for camera apps; WCAG AAA compliance for overlay badges (13.5:1 contrast).
**Key Details:**
- Overlay badges: `{type} · {value} {tolerance}` format (e.g., "5-band · 10kΩ ±1%")
- Badge positioning: above resistor bounding box (flips below when near screen top)
- No persistent chrome — camera view fills 100% of screen
- Support dialog: respectful tone, "If useful, consider supporting development"
- Portrait-only (no landscape complexity)
**Full Guidelines:** `docs/design-guidelines.md`

### 2026-02-25: Responsive design breakpoints
**Decision:** Three-tier responsive system using device width:
- Compact (<400dp): Value + tolerance only (e.g., "10kΩ ±1%")
- Standard (400-599dp): Full format with type (e.g., "5-band · 10kΩ ±1%")
- Expanded (≥600dp): Full format, larger text (18sp)
**Rationale:** Support Samsung Fold (360-720dp) and tablets with appropriate UI density.
**Implementation:** Use `DeviceDisplay.Current.MainDisplayInfo` and `Window.SizeChanged`, animate 200ms fade transitions.

### 2026-02-25: CI pipeline architecture
**Decision:** Three separate GitHub Actions jobs (test, build-android, build-ios) instead of matrix. iOS build uses `/p:BuildIpa=false`. Test job does not install MAUI workload. NuGet cache keyed per OS.
**Rationale:** OS-specific runners (ubuntu for test/Android, macos for iOS); test job is net9.0 plain .NET; prevents cache cross-contamination.

### 2026-02-25: Issue labeling system
**Decision:** Six-category labeling system: infrastructure, vision, mobile, monetization, ux, testing.
**Rationale:** Clear scope boundaries; supports multi-label categorization; no overload.

### 2026-02-25: GitHub issue creation (34 issues)
**Decision:** Created 34 GitHub issues covering architecture, ML/vision, UI/MAUI, core services, design, testing, and additional work (performance, docs, error handling, accessibility, battery, app store).
**Rationale:** Comprehensive roadmap from project kickoff.
