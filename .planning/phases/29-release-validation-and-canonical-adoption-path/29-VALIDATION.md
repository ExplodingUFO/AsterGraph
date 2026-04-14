---
phase: 29
slug: release-validation-and-canonical-adoption-path
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-14
---

# Phase 29 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus static doc/config checks |
| **Config file** | `eng/ci.ps1`, `.github/workflows/ci.yml`, `README.md`, `docs/quick-start.md`, `docs/host-integration.md`, future checked-in coverage/API-compat config |
| **Quick run command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` |
| **Estimated runtime** | current baseline ~45-90 seconds; release lane expected ~60-150 seconds after smoke execution and reporting |

---

## Sampling Rate

- **After every task commit:** Run the narrowest command path affected by the task
  - release-lane/config work: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`
  - adoption-path doc work before release lane is stable: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
  - final proof/doc sync: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`
- **After every plan wave:** Run the full suite command
- **Before phase verification:** Full release-lane command must be green
- **Max feedback latency:** ~90 seconds for current baseline, ~150 seconds once the release lane executes smoke tools and reporting

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 29-01-01 | 01 | 1 | QUAL-03 | release-lane | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` | ⬜ | ⬜ pending |
| 29-01-02 | 01 | 1 | QUAL-03 | release-config | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` | ⬜ | ⬜ pending |
| 29-02-01 | 02 | 2 | PROOF-03 | adoption-path | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` | ✅ | ⬜ pending |
| 29-03-01 | 03 | 3 | QUAL-03, PROOF-03 | proof+docs sync | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` | ⬜ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure is enough to start the phase:

- `eng/ci.ps1` already provides a stable repo-local validation path to extend
- `.github/workflows/ci.yml` already reuses the repo-local script path
- `PackageSmoke`, `ScaleSmoke`, and the split test lanes are already live proof surfaces

Known baseline note:

- the current `all` lane builds smoke-tool projects but does not execute them, collect coverage, or run API/package-compatibility checks. That gap is the pre-phase evidence Phase 29 is meant to close.

---

## Manual-Only Verifications

No manual-only verification should remain after Phase 29.

If any doc step still depends on README-only command bundles by the end of implementation, treat that as a phase failure rather than an acceptable leftover.

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify target
- [x] Sampling continuity keeps release-lane work on the repo-local script path
- [x] Wave 0 explains the current release-validation gap explicitly
- [x] No watch-mode or manual-only test dependency is planned
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-04-14
