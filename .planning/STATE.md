---
gsd_state_version: 1.0
milestone: v1.10
milestone_name: Public Repo Hygiene and Documentation Surface
status: active
stopped_at: Defining requirements
last_updated: "2026-04-16T15:34:02.0858865Z"
last_activity: 2026-04-16 -- Milestone v1.10 started
progress:
  total_phases: 0
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
  percent: 0
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** v1.10 public repo hygiene and documentation surface cleanup

## Current Position

Phase: Not started (defining requirements)
Plan: —
Status: Defining requirements
Last activity: 2026-04-16 -- Milestone v1.10 started

## Accumulated Context

- v1.9 is fully shipped and archived. Hosted CI parity, prerelease workflow validity, packed `.NET 10` consumer proof, and the public launch checklist are already part of the repo baseline.
- The remaining concern is no longer SDK readiness. It is the public git surface: `.planning/`, `AGENTS.md`, `CLAUDE.md`, `build.log`, and the tracked real `NuGet.config` still read like internal workflow or local-environment residue.
- The repo already has the public-facing docs and governance surface needed for a normal open-source alpha: `README.md`, `README.zh-CN.md`, `docs/en`, `docs/zh-CN`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, `global.json`, `HostSample`, `PackageSmoke`, and `ScaleSmoke`.
- `README.md` still explicitly points readers at `.planning` as maintainer context, which conflicts with the new requirement to keep workflow traces out of the public repo surface.
- This milestone should preserve any useful high-level roadmap or launch guidance by moving it under `docs/` instead of simply deleting it with the internal planning directory.

## Next Action

Define scoped requirements for v1.10, then create the roadmap starting at Phase 45.

## Session Continuity

Last session: 2026-04-16T15:34:02.0858865Z
Stopped at: Defining requirements
Resume file: .planning/PROJECT.md
