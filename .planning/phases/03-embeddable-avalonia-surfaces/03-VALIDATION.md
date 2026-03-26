---
phase: 03
slug: embeddable-avalonia-surfaces
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-26
---

# Phase 03 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.2 + Avalonia.Headless.XUnit |
| **Config file** | none — existing test project conventions |
| **Quick run command** | `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj -v minimal` |
| **Full suite command** | `dotnet test avalonia-node-map.sln -v minimal` |
| **Estimated runtime** | ~25 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj -v minimal`
- **After every plan wave:** Run `dotnet test avalonia-node-map.sln -v minimal`
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 03-01-01 | 01 | 1 | EMBD-02 | integration | `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj --filter "FullyQualifiedName~NodeCanvasStandaloneTests|FullyQualifiedName~GraphContextMenuPresenterTests" -v minimal` | ❌ W0 harness + tests | ⬜ pending |
| 03-02-01 | 02 | 2 | EMBD-03 | integration | `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj --filter "FullyQualifiedName~GraphMiniMapStandaloneTests" -v minimal` | ❌ W0 | ⬜ pending |
| 03-02-02 | 02 | 2 | EMBD-04 | integration | `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj --filter "FullyQualifiedName~GraphInspectorStandaloneTests" -v minimal` | ❌ W0 | ⬜ pending |
| 03-03-01 | 03 | 3 | EMBD-01 | integration | `dotnet test $env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj --filter "FullyQualifiedName~GraphEditorSurfaceCompositionTests|FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal` | ❌ W0 | ⬜ pending |
| 03-04-01 | 04 | 4 | EMBD-05 | smoke | `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` | ✅ existing tool | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `$env:TEMP\astergraph-phase3-surface-tests\AsterGraph.Phase3.SurfaceTests.csproj` — focused harness for current workspace
- [ ] `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` — stubs for EMBD-02
- [ ] `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs` — stubs for EMBD-03
- [ ] `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs` — stubs for EMBD-04
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs` — composition regression coverage for EMBD-01 and EMBD-05

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Host can compose `Canvas + Inspector + MiniMap` without shell chrome and verify the interaction feels coherent | EMBD-02, EMBD-03, EMBD-04, EMBD-05 | The composition quality and host-embedding ergonomics are easiest to evaluate from a live sample host | Run `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`, exercise full shell and standalone-surface compositions, confirm menu/shortcut opt-out behavior and shell omission behave as documented |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all missing references
- [ ] No watch-mode flags
- [ ] Feedback latency < 45s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
