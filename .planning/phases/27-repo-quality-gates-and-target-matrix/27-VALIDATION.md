---
phase: 27
slug: repo-quality-gates-and-target-matrix
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-14
---

# Phase 27 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus static file checks |
| **Config file** | `.editorconfig`, `Directory.Packages.props`, `eng/ci.ps1`, `.github/workflows/ci.yml` |
| **Quick run command** | `dotnet build avalonia-node-map.sln --nologo -v minimal` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` |
| **Estimated runtime** | quick build ~45-60 seconds; full suite ~2-4 minutes |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build avalonia-node-map.sln --nologo -v minimal`
- **After every plan wave:** Run the newly created lane that wave introduces:
  - after Wave 1: `dotnet build avalonia-node-map.sln --nologo -v minimal`
  - after Wave 2: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
  - after Wave 3: same full-suite script plus static workflow inspection
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** ~60 seconds for commit-level sampling, ~4 minutes for end-of-wave full validation

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 27-01-01 | 01 | 1 | QUAL-01 | build+static | `dotnet build avalonia-node-map.sln --nologo -v minimal` | ✅ | ⬜ pending |
| 27-02-01 | 02 | 2 | QUAL-02 | script lane | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` | ✅ | ⬜ pending |
| 27-03-01 | 03 | 3 | QUAL-02 | workflow+script | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers the baseline build verification needed to start the phase:

- `dotnet build avalonia-node-map.sln --nologo -v minimal`
- static inspection of root/project files with `rg`

The shared script and workflow validation paths are created by the phase itself, so they become available after Wave 2 and Wave 3.

---

## Manual-Only Verifications

No manual-only verification is required for this phase.

The GitHub Actions workflow cannot be fully executed from within the local workspace, but the workflow file structure is validated statically and the same repo-local script it calls is executed locally.

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 60s for commit-level sampling
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-04-14
