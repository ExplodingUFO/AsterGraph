---
gsd_state_version: 1.0
milestone: v1.5
milestone_name: Runtime Boundary Cleanup and Quality Gates
status: v1.5 milestone archived
stopped_at: v1.5 archive recorded
last_updated: "2026-04-14T21:23:51.6311355+08:00"
progress:
  total_phases: 4
  completed_phases: 4
  total_plans: 12
  completed_plans: 12
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-14)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** v1.4 archive cleanup and next-milestone framing

## Current Position

Milestone: v1.5 archived
Latest shipped phase: 29 (release-validation-and-canonical-adoption-path)
Plans: 12 of 12 complete

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

- v1.4 still has no archived milestone files in `.planning/milestones/`, so milestone history is only partially normalized to the newer archive format.
- The known `STATE_HISTORY_OK` mismatch remains a pre-existing baseline issue if future work touches history/save semantics.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-14T20:20:09.9120467+08:00
Stopped at: v1.5 archive recorded
Resume file: .planning/MILESTONES.md
