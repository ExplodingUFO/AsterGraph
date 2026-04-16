---
phase: 44
title: Public Launch Checklist And Entry Tightening
status: planned
last_updated: 2026-04-16
---

# Phase 44 Research

## Current Doc Drift

- `README.md` already presents the repo as a public alpha surface, but it still lacks one explicit public-launch checklist link and does not yet mention the new `.NET 10` packed-consumer proof.
- `docs/en/alpha-status.md` still frames prerelease publication as a major limitation even though Phase 43 closed the local release-lane and workflow-structure gaps. The remaining work is now operational: visibility, branch protection, and the first public tag/release pass.
- `docs/en/quick-start.md` still describes NuGet.org as merely planned and does not point maintainers to one concrete checklist before flipping visibility.

## Phase Direction

Phase 44 should not reopen workflow or runtime changes. It should:

1. Update the public docs so they describe only the real blockers left after Phases 42-43.
2. Add one small launch checklist that maintainers can follow before making the repository public.
3. Keep `HostSample` vs `AsterGraph.Demo` responsibilities explicit and mention the new `.NET 10` consumer proof where relevant.
