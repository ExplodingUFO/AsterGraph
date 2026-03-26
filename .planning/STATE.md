---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: Executing
stopped_at: Phase 4 plan 02 completed
last_updated: "2026-03-26T10:19:13.4129966Z"
progress:
  total_phases: 5
  completed_phases: 3
  total_plans: 17
  completed_plans: 15
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-26)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 4 - Replaceable Presentation Kit

## Current Position

Phase: 4 of 5 (Replaceable Presentation Kit)
Plan: 04-03 next

## Performance Metrics

**Velocity:**

- Total plans completed: 15
- Average duration: 17 min
- Total execution time: 4.0 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-consumption-compatibility-guardrails | 4 | 69 min | 17 min |
| 02-runtime-contracts-service-seams | 5 | 84 min | 17 min |
| 03-embeddable-avalonia-surfaces | 4 | 65 min | 16 min |
| 04-replaceable-presentation-kit | 2 | 49 min | 25 min |

**Recent Trend:**

- Last 5 plans: 03-02 (13 min), 03-03 (16 min), 03-04 (15 min), 04-01 (28 min), 04-02 (21 min)
- Trend: Stable

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
- [Phase 01-consumption-compatibility-guardrails]: Use DefaultItemExcludes in Directory.Build.props so project-local artifacts trees stay out of SDK default compile globs.
- [Phase 01-consumption-compatibility-guardrails]: Represent Phase 1 initialization and migration contracts as explicit skipped xUnit cases with stable method names for later implementation plans.
- [Phase 01-consumption-compatibility-guardrails]: Keep the new host entry surface factory-first and place it under the existing Hosting namespaces in AsterGraph.Editor and AsterGraph.Avalonia.
- [Phase 01-consumption-compatibility-guardrails]: Preserve direct GraphEditorViewModel and GraphEditorView construction as the compatibility path while making the new factories the canonical host-sample composition route.
- [Phase 01-consumption-compatibility-guardrails]: Use smoke markers for legacy editor/view creation and factory editor/view creation so migration-stage package validation is machine-checkable.
- [Phase 01-consumption-compatibility-guardrails]: Keep the retained GraphEditorViewModel constructor and GraphEditorView Editor-assignment path documented as supported compatibility facades instead of adding obsoletions in Phase 1.
- [Phase 01-consumption-compatibility-guardrails]: Document the supported SDK boundary around the four publishable packages and treat AsterGraph.Editor as a standard host-facing runtime package.
- [Phase 01-consumption-compatibility-guardrails]: Make the factory/options path canonical in docs while keeping GraphEditorViewModel and GraphEditorView documented as supported migration facades.
- [Phase 01-consumption-compatibility-guardrails]: Treat packed-package failures in this workspace as restore/cache environment blockers only after confirming the freshly packed nupkgs still expose the new public APIs.

### Pending Todos

None yet.

### Blockers/Concerns

- Compatibility shim scope and migration window need explicit planning before public API breaks land.
- Large-graph performance budgets should be defined before aggressive surface decomposition.
- Diagnostics taxonomy still needs stable codes and payload design before implementation.

## Session Continuity

Last session: 2026-03-26T10:19:13.4129966Z
Stopped at: Phase 4 04-02 completed
Resume file: .planning/phases/04-replaceable-presentation-kit/04-03-PLAN.md
