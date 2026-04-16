---
gsd_state_version: 1.0
milestone: v1.9
milestone_name: Public Launch Gate and CI Stabilization
status: active
stopped_at: Milestone audit ready
last_updated: "2026-04-16T16:00:00.0000000Z"
last_activity: 2026-04-16 -- Completed Phase 44 public launch checklist and entry tightening
progress:
  total_phases: 3
  completed_phases: 3
  total_plans: 9
  completed_plans: 9
  percent: 100
---

# Project State

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-04-16)

**Core value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.
**Current focus:** v1.9 launch-gate stabilization for public opening

## Current Position

Phase: complete
Plan: —
Status: Ready to audit milestone
Last activity: 2026-04-16 -- Completed Phase 44 public launch checklist and entry tightening

## Accumulated Context

- v1.8 is fully shipped and archived. The public-alpha framing, governance files, canonical demo path, bilingual docs, localization proof, and proof-artifact workflows are already part of the repo baseline.
- The first post-archive GitHub Actions runs exposed three concrete blockers on clean hosted runners:
  - plugin proof tests used to resolve `AsterGraph.TestPlugins` from `bin/Debug/net9.0`, but Phase 42 moved that proof surface onto the active configuration/TFM path and revalidated both `contract` and `all` lanes
  - `actions/setup-dotnet` cache cleanup used to fail because the default global package directory was absent; Phase 43 moved both workflows onto a workspace-local `NUGET_PACKAGES` path
  - `.github/workflows/release.yml` used to fail before jobs scheduled because the publish gate referenced `secrets.*` in a job-level `if:`; Phase 43 moved the secret gate inside the job and revalidated the release lane
- Phase 43 also added one explicit packed `.NET 10` HostSample proof marker (`HOST_SAMPLE_NET10_OK:True`) to the release validation surface.
- Phase 44 closed the remaining public-entry narrative gap by linking one launch checklist and aligning README/alpha-status/quick-start with the actual maintained proof surface.
- All planned v1.9 phase work is complete; only milestone audit and archive remain.

## Next Action

Audit and archive v1.9.

## Session Continuity

Last session: 2026-04-16T16:00:00.0000000Z
Stopped at: Milestone audit ready
Resume file: .planning/ROADMAP.md
