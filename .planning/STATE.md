---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: Ready to execute
stopped_at: Completed quick 260327-rs7
last_updated: "2026-03-27T12:27:21Z"
progress:
  total_phases: 6
  completed_phases: 6
  total_plans: 24
  completed_plans: 24
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-26)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 06 — demo

## Current Position

Phase: 06 (demo) — EXECUTING
Plan: 3 of 3

## Performance Metrics

**Velocity:**

- Total plans completed: 21
- Average duration: 17 min
- Total execution time: 4.6 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-consumption-compatibility-guardrails | 4 | 69 min | 17 min |
| 02-runtime-contracts-service-seams | 5 | 84 min | 17 min |
| 03-embeddable-avalonia-surfaces | 4 | 65 min | 16 min |
| 04-replaceable-presentation-kit | 4 | 93 min | 23 min |

**Recent Trend:**

- Last 5 plans: 04-01 (28 min), 04-02 (21 min), 04-03 (31 min), 04-04 (13 min), 05-04 (final closeout)
- Trend: Stable

| Phase 06 P01 | 20min | 3 tasks | 5 files |
| Phase 06 P02 | 14min | 3 tasks | 2 files |
| Phase 260327-pas-phase-6-demo P1 | 14min | 3 tasks | 6 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Phase 1]: Start with package boundary, public initialization, and migration guardrails before deeper refactors.
- [Phase 2]: Keep host-facing editor contracts in `AsterGraph.Editor` and avoid Avalonia leakage in runtime APIs.
- [Phase 3]: Split Avalonia surfaces only after shared editor-session seams are stable.
- [Phase 4]: Keep presenter replacement opt-in per surface and route it through canonical full-shell plus standalone factory options.
- [Phase 04-replaceable-presentation-kit]: Lock the presenter contract surface in `AsterGraph.Avalonia` before implementing stock-vs-custom behavior replacement.
- [Phase 04-replaceable-presentation-kit]: Keep NodeCanvas as the interaction owner and treat node/menu presenters as visual-only replacement seams over existing editor intent.
- [Phase 04-replaceable-presentation-kit]: Let stock standalone inspector/minimap surfaces act as presenter hosts instead of introducing a second mandatory shell abstraction.
- [Phase 04-replaceable-presentation-kit]: Prove presentation replacement through focused harnesses, host sample output, package smoke markers, and aligned consumer docs.
- [Phase 05-diagnostics-integration-inspection]: Extend the existing diagnostics sink and recoverable-failure baseline instead of replacing it with UI text or a log-only story.
- [Phase 05-diagnostics-integration-inspection]: Keep diagnostics and inspection rooted in `AsterGraph.Editor` and expose them through the canonical runtime/session path plus the retained compatibility facade.
- [Phase 05-diagnostics-integration-inspection]: Model inspection as immutable medium-grain snapshots built from current query/read-model assets instead of exposing live editor internals.
- [Phase 05-diagnostics-integration-inspection]: Make logging and tracing opt-in through standard .NET host tooling at runtime boundaries, not through Avalonia control instrumentation.
- [Phase 05-diagnostics-integration-inspection]: Put the canonical diagnostics discovery point on `IGraphEditorSession.Diagnostics` rather than creating a second factory-only diagnostics service root.
- [Phase 05-diagnostics-integration-inspection]: Keep Phase 5 contract tests reflection-based and isolated in a `%TEMP%` harness so missing APIs fail as assertions instead of being masked by workspace-local test noise.
- [Phase 05-diagnostics-integration-inspection]: Store recent diagnostics in a bounded session-local window and publish support-relevant info/warning/error diagnostics only at meaningful runtime boundaries.
- [Phase 05-diagnostics-integration-inspection]: Route logger and tracing emission through `GraphEditorSession.PublishDiagnostic` so diagnostics sink, recent history, logs, and activities stay correlated off the same operation codes.
- [Phase 01-consumption-compatibility-guardrails]: Use DefaultItemExcludes in Directory.Build.props so project-local artifacts trees stay out of SDK default compile globs.
- [Phase 01-consumption-compatibility-guardrails]: Represent Phase 1 initialization and migration contracts as explicit skipped xUnit cases with stable method names for later implementation plans.
- [Phase 01-consumption-compatibility-guardrails]: Keep the new host entry surface factory-first and place it under the existing Hosting namespaces in AsterGraph.Editor and AsterGraph.Avalonia.
- [Phase 01-consumption-compatibility-guardrails]: Preserve direct GraphEditorViewModel and GraphEditorView construction as the compatibility path while making the new factories the canonical host-sample composition route.
- [Phase 01-consumption-compatibility-guardrails]: Use smoke markers for legacy editor/view creation and factory editor/view creation so migration-stage package validation is machine-checkable.
- [Phase 01-consumption-compatibility-guardrails]: Keep the retained GraphEditorViewModel constructor and GraphEditorView Editor-assignment path documented as supported compatibility facades instead of adding obsoletions in Phase 1.
- [Phase 01-consumption-compatibility-guardrails]: Document the supported SDK boundary around the four publishable packages and treat AsterGraph.Editor as a standard host-facing runtime package.
- [Phase 01-consumption-compatibility-guardrails]: Make the factory/options path canonical in docs while keeping GraphEditorViewModel and GraphEditorView documented as supported migration facades.
- [Phase 01-consumption-compatibility-guardrails]: Treat packed-package failures in this workspace as restore/cache environment blockers only after confirming the freshly packed nupkgs still expose the new public APIs.
- [Phase 06]: Keep the Demo on one retained GraphEditorViewModel and project architecture proof from runtime/session APIs instead of creating subordinate runtime objects.
- [Phase 06]: Bind capability metadata from the ViewModel into navigation and the right rail so the shell teaches actual API seams without duplicated copy islands.
- [Phase 06]: Use a dedicated DemoMainWindowTests entry point for Phase 6 shell regression coverage while leaving GraphEditorView coverage isolated.
- [Phase 06]: Translate only user-facing GraphEditorView shell copy to Chinese while preserving English host API/type identifiers.
- [Phase 06]: Validate Demo runtime captions through the existing IGraphLocalizationProvider path without editing MainWindowViewModel in plan 06-02.
- [Phase 260327-pas-phase-6-demo]: Expose GraphEditorView per-region chrome booleans and compose with ChromeMode for independent host visibility control.
- [Phase 260327-pas-phase-6-demo]: Use star-row full-height center editor composition in Demo instead of fixed editor frame height.

### Pending Todos

None yet.

### Blockers/Concerns

- Focused temp harnesses remain the safe primary gate while workspace-local test noise in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` and the local test project edits remain unresolved.
- Final host sample, package smoke, docs proof ring, and focused `%TEMP%` Phase 5 harness are complete for `05-04`.
- `src/AsterGraph.Editor/README.md` has local modifications, so Phase 5 closeout docs should stay in root docs/sample proof unless that noise is resolved first.

## Session Continuity

Last session: 2026-03-27T10:35:34.212Z
Stopped at: Completed quick 260327-pas
Resume file: None
