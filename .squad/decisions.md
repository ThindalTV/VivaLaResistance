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
