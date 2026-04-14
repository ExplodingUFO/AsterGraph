---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Runtime Boundary Cleanup and Quality Gates
status: Ready to execute
stopped_at: Phase 29 planned
last_updated: "2026-04-14T13:01:50.775Z"
progress:
  total_phases: 4
  completed_phases: 3
  total_plans: 12
  completed_plans: 11
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-14)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 29 — release-validation-and-canonical-adoption-path

## Current Position

Phase: 29 (release-validation-and-canonical-adoption-path) — EXECUTING
Plan: 3 of 3

## Accumulated Context

### Decisions

Carry-forward decisions from shipped milestones:

- Keep the four-package SDK boundary (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`) as the supported publish surface.
- Keep `CreateSession(...)` and `Create(...)` as the canonical composition routes, while `GraphEditorViewModel` / `GraphEditorView` remain the retained compatibility window.
- Prefer descriptor and snapshot control-plane contracts over exposing MVVM implementation types as the canonical host surface.
- Keep Avalonia as an adapter layer over shared runtime contracts and proof-backed platform seams.
- Preserve `PackageSmoke` and `ScaleSmoke` as runnable proof surfaces for migration, readiness, and future extension work.
- Keep plugin loading and automation execution rooted in `IGraphEditorSession`, descriptors, and command IDs rather than retained MVVM compatibility APIs.
- Keep plugin load failures and automation telemetry recoverable and machine-readable through canonical runtime diagnostics/events.

New v1.5 framing decisions:

- Start v1.5 around runtime boundary cleanup, repo-level quality gates, and proof/doc alignment rather than another new feature band.
- Continue phase numbering from 26 because v1.5 builds directly on the shipped v1.4 baseline.
- Skip milestone research by default because this milestone is brownfield SDK hardening rather than a new external domain exploration.

### Pending Todos

None captured yet.

### Blockers/Concerns

- `MILESTONES.md` still stops at v1.3 even though v1.4 execution is complete; archive/history cleanup is now part of the v1.5 proof/doc alignment scope.
- `eng/ci.ps1` now gives one accurate repo-local build/test entry point, but release-grade package smoke execution, coverage/reporting, and public API/package compatibility are still not automated.
- Hosts still do not have one short synchronized adoption guide that covers runtime-only, shipped-UI, and retained-facade migration decisions from a single entry point.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if v1.5 touches history/save semantics.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-14T20:20:09.9120467+08:00
Stopped at: Phase 29 planned
Resume file: .planning/phases/29-release-validation-and-canonical-adoption-path/29-01-PLAN.md
