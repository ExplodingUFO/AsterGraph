---
gsd_state_version: 1.0
milestone: v1.6
milestone_name: Facade Convergence and Proof Guardrails
status: executing
stopped_at: Completed 30-01-PLAN.md
last_updated: "2026-04-16T03:55:35.273Z"
last_activity: 2026-04-16
progress:
  total_phases: 4
  completed_phases: 0
  total_plans: 3
  completed_plans: 1
  percent: 33
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 30 Wave 2 prep - milestone-history-and-refactor-gate-closeout

## Current Position

Phase: 30 (milestone-history-and-refactor-gate-closeout) - EXECUTING
Plan: 1 of 3 complete
Status: In progress
Last activity: 2026-04-16 - completed 30-01 archive reconstruction

## Performance Metrics

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| Phase 30 P01 | 1 min | 2 tasks | 7 files |

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
- [Phase 30]: Reconstruct the missing v1.4 archive from historical snapshot commits instead of rewriting current milestone docs. — The archive needed explicit git-backed evidence so the recovered roadmap and requirements keep their original framing.
- [Phase 30]: Record later pre-v1.5 trust, discovery, staging, and proof follow-up in the v1.4 ledger notes instead of editing it back into the archived snapshots. — The delivered pre-v1.5 surface exceeded the original four-phase roadmap, so the ledger needed to separate original framing from later follow-up honestly.

### Pending Todos

None captured yet.

### Blockers/Concerns

- `v1.4` archive files now exist, but the live roadmap/current-state closeout still needs synchronization in later Phase 30 plans.
- The known `STATE_HISTORY_OK` mismatch remains a carried baseline issue until the new history/save semantics work closes it.
- `GraphEditorViewModel`, `GraphEditorKernel`, and `NodeCanvas` remain the obvious internal hotspots even after the earlier coordinator extractions.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T03:54:47.309Z
Stopped at: Completed 30-01-PLAN.md
Resume file: .planning/phases/30-milestone-history-and-refactor-gate-closeout/30-02-PLAN.md
