---
phase: 28
slug: proof-surface-and-regression-lane-alignment
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-14
---

# Phase 28 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus static file checks |
| **Config file** | `avalonia-node-map.sln`, `eng/ci.ps1`, `.github/workflows/ci.yml`, `README.md`, `docs/quick-start.md`, `docs/host-integration.md` |
| **Quick run command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net8.0 -Configuration Release` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` |
| **Estimated runtime** | quick lane ~20-40 seconds; full suite ~45-90 seconds |

---

## Sampling Rate

- **After every task commit:** Run the narrowest lane affected by the task
  - proof-tool / solution alignment work: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net8.0 -Configuration Release`
  - demo-lane split work: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net9.0 -Configuration Release`
  - public-doc alignment work: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
- **After every plan wave:** Run the full suite command
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** ~40 seconds for lane checks, ~90 seconds for full validation

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 28-01-01 | 01 | 1 | PROOF-01 | proof-entrypoint | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net8.0 -Configuration Release` | ✅ | ⬜ pending |
| 28-02-01 | 02 | 2 | PROOF-02 | regression-lane | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net9.0 -Configuration Release` | ✅ | ⬜ pending |
| 28-03-01 | 03 | 3 | PROOF-01 | docs+full-proof | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure is enough to start the phase:

- static inspection with `rg`
- current `eng/ci.ps1` lane execution

Known baseline note:

- a fresh worktree currently fails the `net8.0` lane because `ScaleSmoke` is outside the solution restore surface. That failure is acceptable as pre-phase evidence and is expected to disappear after Plan 28-01.

---

## Manual-Only Verifications

No manual-only verification is required for this phase.

The GitHub Actions workflow remains statically validated through the repo-local script path rather than being run remotely from within the workspace.

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 60s for lane checks
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-04-14
