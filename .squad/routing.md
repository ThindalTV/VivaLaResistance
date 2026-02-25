# VivaLaResistance — Routing Rules

## Domain → Agent Mapping

| Domain / Signal | Route To | Notes |
|-----------------|----------|-------|
| Architecture, scope decisions, code review, tech choices | Rhodes | Lead on all structural decisions |
| MAUI UI, XAML layouts, navigation, platform handlers | Shuri | iOS & Android specifics |
| Camera integration, ML model, resistor detection algorithm, color band logic | Bruce | Vision/ML domain owner |
| Tests, edge cases, quality gates, CI test runs | Natasha | All test types |
| UX design, interaction flows, overlay appearance, accessibility, monetization modal feel | Hope | Design specs; Shuri implements |
| Session logging, decision merging, history updates | Scribe | Silent — never user-facing |
| Work queue scanning, issue triage, PR monitoring | Ralph | Loop-driven monitor |
| Well-defined implementation issues, test scaffolding, boilerplate | @copilot | See capability profile in team.md |
| Monetization modal, trial logic, app lifecycle | Shuri + Rhodes | UI (Shuri) + logic gate (Rhodes) |
| AR overlay rendering, GraphicsView canvas | Shuri | With input from Bruce on coordinate mapping |
| GitHub issues, PRs, CI | Ralph first, then appropriate agent | Ralph triages, routes to member |

## Escalation

- If Bruce's vision output needs UI work → hand off to Shuri
- If Shuri needs ML coordinate data → request from Bruce
- If any agent is unsure of scope → escalate to Rhodes
- Reviewer rejection → Coordinator enforces lockout, assigns new author

## Auto-Triggers

- Task touching `*.csproj`, `*.slnx`, or project structure → Rhodes reviews
- Task touching `Platforms/iOS` or `Platforms/Android` → Natasha writes platform tests
- Any new feature merged → Natasha runs regression check
