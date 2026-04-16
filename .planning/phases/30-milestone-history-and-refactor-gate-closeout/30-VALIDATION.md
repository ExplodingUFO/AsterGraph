---
phase: 30
slug: milestone-history-and-refactor-gate-closeout
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-16
---

# Phase 30 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus static planning/doc checks |
| **Config file** | `.planning/MILESTONES.md`, `.planning/milestones/v1.4-ROADMAP.md`, `.planning/milestones/v1.4-REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md`, `.planning/PROJECT.md`, `eng/ci.ps1`, `README.md`, `docs/host-integration.md` |
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMutationCompatibilityTests|FullyQualifiedName~GraphEditorFacadeMutationParityTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorViewModelProjectionTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests" -v minimal` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` |
| **Estimated runtime** | static archive/doc checks <10 seconds; focused hotspot lane expected ~30-120 seconds after implementation |

---

## Sampling Rate

- **After every task commit:** run the narrowest command that proves the touched surface
  - archive-only planning work: static existence/content checks against `.planning/milestones/` and `.planning/MILESTONES.md`
  - maintenance-lane script work: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
  - top-level doc sync after the lane exists: static doc checks plus the maintenance lane
- **After every plan wave:** run the full suite command
- **Before phase verification:** the maintenance lane plus archive/doc checks must be green
- **Max feedback latency:** under ~2 minutes for the new maintenance lane; under ~10 seconds for static archive/doc checks

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 30-01-01 | 01 | 1 | CLOSE-01 | archive-snapshot | `pwsh -NoProfile -Command "$ok = (Test-Path '.planning/milestones/v1.4-ROADMAP.md') -and (Test-Path '.planning/milestones/v1.4-REQUIREMENTS.md'); if (-not $ok) { exit 1 }"` | ⬜ | ⬜ pending |
| 30-01-02 | 01 | 1 | CLOSE-01 | milestone-ledger | `pwsh -NoProfile -Command "$hit = Select-String -Path '.planning/MILESTONES.md' -Pattern '## v1.4 Plugin Loading and Automation Execution'; if (-not $hit) { exit 1 }"` | ⬜ | ⬜ pending |
| 30-02-01 | 02 | 2 | GUARD-01 | maintenance-lane | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` | ⬜ | ⬜ pending |
| 30-03-01 | 03 | 3 | CLOSE-02, GUARD-01 | planning+doc-sync | `pwsh -NoProfile -Command "$hits = @((Select-String -Path '.planning/ROADMAP.md' -Pattern 'v1.4-ROADMAP.md' -Quiet),(Select-String -Path '.planning/PROJECT.md' -Pattern 'maintenance gate' -Quiet),(Select-String -Path '.planning/STATE.md' -Pattern 'Phase 31|execute-phase 30|maintenance' -Quiet),(Select-String -Path 'README.md' -Pattern 'maintenance gate|Lane maintenance' -Quiet),(Select-String -Path 'docs/host-integration.md' -Pattern 'maintenance gate|Lane maintenance' -Quiet)); if ($hits -contains $false) { exit 1 }"` | ⬜ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure is enough to start the phase:

- historical `v1.4` requirements and roadmap snapshots are available in git history
- the current milestone ledger and planning files already surface the missing archive gap
- `eng/ci.ps1` already provides the checked-in script-first command path that the maintenance lane can extend
- the hotspot-sensitive suites and `ScaleSmoke` already exist in the repo; the gap is one maintained wrapper command, not missing proof assets

Known baseline notes:

- `eng/ci.ps1` does not yet expose a dedicated maintenance/refactor lane
- `.planning/milestones/` does not yet include `v1.4` archive files
- current top-level planning/docs still require minor synchronization once those two gaps close

---

## Manual-Only Verifications

No manual-only verification should remain after Phase 30.

If the phase ends with archive reconstruction or hotspot guardrail guidance that still depends on remembered git commands or stale phase directories, treat that as a phase failure rather than acceptable residue.

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify target
- [x] Sampling continuity keeps refactor guardrail work on the tracked repo-local command path
- [x] Wave 0 explains the missing archive and missing maintenance-lane baseline explicitly
- [x] No watch-mode or manual-only verification is planned
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-04-16
