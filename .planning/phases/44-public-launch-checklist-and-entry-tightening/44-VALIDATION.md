---
phase: 44
title: Public Launch Checklist And Entry Tightening
status: planned
last_updated: 2026-04-16
---

# Phase 44 Validation

## Required Checks

1. Public docs mention only the real remaining blockers:
   - `rg -n "public launch|launch checklist|net10|HostSample|AsterGraph.Demo|visibility|branch protection|prerelease" README.md docs/en`
2. The new checklist is checked in and linked from the public entry docs:
   - `Get-Content docs/en/public-launch-checklist.md`
3. The existing release proof surface still passes after doc-only edits:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`

## Acceptance

- README and `docs/en/alpha-status.md` describe launch status in terms of remaining operational steps, not old CI/workflow blockers that are already closed.
- One checked-in checklist covers visibility flip, branch protection, first prerelease tag, and proof-artifact review.
- Public docs still distinguish `HostSample` as the minimal consumer proof and `AsterGraph.Demo` as the showcase host.
