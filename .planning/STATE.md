---
gsd_state_version: 1.0
milestone: v1.6
milestone_name: Facade Convergence and Proof Guardrails
status: v1.6 initialized
stopped_at: Roadmap approved
last_updated: "2026-04-16T11:17:10.5846951+08:00"
progress:
  total_phases: 4
  completed_phases: 0
  total_plans: 12
  completed_plans: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 30 planning - Milestone History And Refactor Gate Closeout

## Current Position

Phase: 30 (milestone-history-and-refactor-gate-closeout) - READY TO PLAN
Plan: 0 of 3 complete
Status: Roadmap approved for milestone v1.6
Last activity: 2026-04-16 - roadmap approved for v1.6

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

New v1.6 framing decisions:

- Use the next milestone for contraction and hotspot guardrails rather than another plugin/automation capability band.
- Build on the v1.5 quality baseline instead of reopening already-shipped `.editorconfig`, central package management, CI, or `ScaleSmoke` alignment work.
- Treat the missing `v1.4` archive and the carried `STATE_HISTORY_OK` mismatch as current milestone work, not passive background debt.
- Continue phase numbering from 30 because the latest executed phase is 29.

### Pending Todos

None captured yet.

### Blockers/Concerns

- `v1.4` still has no archived milestone files in `.planning/milestones/`, so milestone history is only partially normalized.
- The known `STATE_HISTORY_OK` mismatch remains a carried baseline issue until the new history/save semantics work closes it.
- `GraphEditorViewModel`, `GraphEditorKernel`, and `NodeCanvas` remain the obvious internal hotspots even after the earlier coordinator extractions.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T11:17:10.5846951+08:00
Stopped at: Roadmap approved
Resume file: .planning/ROADMAP.md
