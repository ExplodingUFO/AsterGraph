---
gsd_state_version: 1.0
milestone: v1.6
milestone_name: Facade Convergence and Proof Guardrails
status: planning
stopped_at: Phase 30 completed and verified
last_updated: "2026-04-16T04:16:00.000Z"
last_activity: 2026-04-16
progress:
  total_phases: 4
  completed_phases: 1
  total_plans: 3
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** Phase 31 ready to plan - history-and-save-semantic-closure

## Current Position

Phase: 31 (history-and-save-semantic-closure) - READY TO PLAN
Plan: Not started
Status: Ready for planning
Last activity: 2026-04-16 - completed Phase 30 archive, maintenance gate, review, and verification

## Performance Metrics

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| Phase 30 P01 | 1 min | 2 tasks | 7 files |
| Phase 30 P02 | 1 min | 2 tasks | 1 file |
| Phase 30 P03 | 1 min | 2 tasks | 5 files |

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
- [Phase 30]: Keep the maintenance guardrail inside `eng/ci.ps1` as a dedicated `maintenance` lane instead of introducing a second script path. — Contributors and docs now have one repo-local refactor gate to point at.
- [Phase 30]: Keep `maintenance` narrower than `release` by running focused hotspot suites plus `ScaleSmoke`, without pack, `PackageSmoke`, coverage, or package validation. — The new lane stays fast enough for refactor loops while still exercising one runnable readiness proof.

### Pending Todos

None captured yet.

### Blockers/Concerns

- The known `STATE_HISTORY_OK` mismatch remains a carried baseline issue until Phase 31 closes the history/save semantic contract.
- `GraphEditorViewModel`, `GraphEditorKernel`, and `NodeCanvas` remain the obvious internal hotspots even after the earlier coordinator extractions.

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-sx0 | add quick-start discovery links and release guidance | 2026-03-27 | 99bdc04 | [260327-sx0-add-quick-start-discovery-links](./quick/260327-sx0-add-quick-start-discovery-links/) |
| 260413-wsy | extract GraphEditorSession stock menu descriptor builder | 2026-04-13 | pending | [260413-wsy-extract-grapheditorsession-stock-menu-de](./quick/260413-wsy-extract-grapheditorsession-stock-menu-de/) |
| 260413-x3v | keep GraphEditorSession stock menu builder reading live node catalog definitions | 2026-04-13 | pending | [260413-x3v-follow-up-fix-keep-grapheditorsession-st](./quick/260413-x3v-follow-up-fix-keep-grapheditorsession-st/) |

## Session Continuity

Last session: 2026-04-16T04:16:00.000Z
Stopped at: Phase 30 completed and verified
Resume file: .planning/ROADMAP.md
