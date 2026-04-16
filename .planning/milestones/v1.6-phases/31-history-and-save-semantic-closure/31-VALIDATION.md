---
phase: 31
slug: history-and-save-semantic-closure
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-16
---

# Phase 31 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus `ScaleSmoke` and the scripted maintenance lane |
| **Config file** | `tests/AsterGraph.Editor.Tests/*.cs`, `tools/AsterGraph.ScaleSmoke/Program.cs`, `eng/ci.ps1` |
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorHistorySemanticTests|FullyQualifiedName~GraphEditorHistoryInteractionTests|FullyQualifiedName~GraphEditorSaveBoundaryTests|FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests|FullyQualifiedName~GraphEditorProofRingTests" -v minimal` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` |
| **Estimated runtime** | focused history/save tests <60 seconds; `ScaleSmoke` <30 seconds; maintenance lane ~30-120 seconds |

---

## Sampling Rate

- **After every task commit:** run the narrowest command that proves the touched surface
  - retained semantic fix work: focused history/save tests
  - maintenance lane filter updates: focused history/save tests plus `eng/ci.ps1 -Lane maintenance`
  - `ScaleSmoke` proof updates: `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo`
- **After every plan wave:** run the full suite command
- **Before phase verification:** the maintenance lane and direct `ScaleSmoke` run must both be green
- **Max feedback latency:** under ~2 minutes on the maintenance lane; under ~30 seconds for direct `ScaleSmoke`; under ~60 seconds for focused history/save suites

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 31-01-01 | 01 | 1 | STATE-01 | mixed-path-regression | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorHistorySemanticTests|FullyQualifiedName~GraphEditorTransactionTests" -v minimal` | ⬜ | ⬜ pending |
| 31-01-02 | 01 | 1 | STATE-01 | retained-mutation-contract | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorHistorySemanticTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal` | ⬜ | ⬜ pending |
| 31-02-01 | 02 | 2 | STATE-02 | focused-suite-split | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorHistoryInteractionTests|FullyQualifiedName~GraphEditorSaveBoundaryTests|FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests|FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests" -v minimal` | ⬜ | ⬜ pending |
| 31-02-02 | 02 | 2 | STATE-02 | maintenance-gate | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` | ⬜ | ⬜ pending |
| 31-03-01 | 03 | 3 | STATE-03 | scale-proof | `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj -c Release --framework net8.0 --nologo` | ⬜ | ⬜ pending |
| 31-03-02 | 03 | 3 | STATE-03 | proof-ring-alignment | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorHistoryInteractionTests|FullyQualifiedName~GraphEditorSaveBoundaryTests" -v minimal` | ⬜ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure is enough to start the phase:

- the maintenance gate already exists in `eng/ci.ps1`
- retained history/save tests already exist in `GraphEditorTransactionTests.cs`
- `ScaleSmoke` already emits a dedicated history marker that can be upgraded into an explicit contract proof
- Phase 30 already aligned the milestone archive and maintainer command story, so Phase 31 can stay focused on semantics and proof

Known baseline notes:

- `ScaleSmoke` currently emits `SCALE_HISTORY_OK:True:False:True:True`, which contradicts the intended save-boundary contract
- `GraphEditorViewModel` still mixes kernel capability checks with local retained history state
- `GraphEditorTransactionTests.cs` is still a broad suite carrying both runtime batching and retained history/save coverage
- the maintenance lane still targets `GraphEditorTransactionTests` as one broad bundle instead of dedicated history/save suites

---

## Manual-Only Verifications

No manual-only verification should remain after Phase 31.

If the phase ends with a history/save story that still requires maintainers to remember which booleans from `ScaleSmoke` are the "good" mismatch, treat that as a phase failure rather than acceptable residue.

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify target
- [x] Sampling continuity keeps history/save closure on the existing maintenance and proof-ring command surfaces
- [x] Wave 0 records the current `ScaleSmoke` mismatch and mixed history-owner baseline explicitly
- [x] No watch-mode or manual-only verification is planned
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-04-16
