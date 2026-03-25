---
phase: 02
slug: runtime-contracts-service-seams
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-03-26
---

# Phase 02 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.2 + existing `tests/AsterGraph.Editor.Tests` project |
| **Config file** | none — existing test project conventions |
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal` |
| **Full suite command** | `dotnet test avalonia-node-map.sln -v minimal` |
| **Estimated runtime** | ~30-45 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
- **After every plan wave:** Run `dotnet test avalonia-node-map.sln -v minimal`
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 45 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 02-01-01 | 01 | 0 | API-01, API-02, API-03 | unit scaffold | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal` | ❌ created by Plan 01 Task 1 | ⬜ pending |
| 02-01-02 | 01 | 0 | API-04 | unit scaffold | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorTransactionTests" -v minimal` | ❌ created by Plan 01 Task 2 | ⬜ pending |
| 02-03-02 | 03 | 0 | SERV-01, SERV-02 | unit scaffold | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal` | ❌ created by Plan 03 Task 2 | ⬜ pending |
| 02-02-01 | 02 | 1 | API-01, API-02, API-03 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal` | ✅ | ⬜ pending |
| 02-02-02 | 02 | 1 | API-04 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorTransactionTests" -v minimal` | ✅ | ⬜ pending |
| 02-04-02 | 04 | 2 | SERV-01, SERV-02 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal` | ✅ | ⬜ pending |
| 02-05-01 | 05 | 3 | API-01, API-02, API-03, API-04, SERV-01 | unit + consumer | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal && dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj && dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` | ✅ | ⬜ pending |
| 02-05-02 | 05 | 3 | SERV-01, SERV-02 | smoke | `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` — created in `02-01` Task 1 before runtime implementation
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs` — created in `02-01` Task 2 before batching implementation
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` — created in `02-03` Task 2 before service/runtime wiring

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Runtime session surface is coherent for external hosts | API-01, API-02, API-03 | API ergonomics and naming still need human review | Read the public runtime contract and factory/session entry path as a host author; confirm commands, queries, and events are discoverable without reading Avalonia code |
| Lightweight transaction API is understandable and not over-engineered | API-04 | Human judgment needed to distinguish practical batching from accidental architecture bloat | Review the public transaction surface and a host-side usage example; confirm it supports grouped edits and coherent notifications without introducing a bus or mediator system |
| Default storage behavior is package-neutral and host-safe | SERV-02 | Human judgment needed to verify the package story matches the storage contract | Review `GraphEditorStorageDefaults` and the host docs; confirm the default root is package-neutral and that host identity is not inferred anywhere |
| Compatibility-service replacement remains coherent through the new runtime/session path | SERV-01 | Human judgment needed to confirm the retained seam still reads as one supported migration story | Review `AsterGraphEditorOptions.CompatibilityService`, the host sample, and migration docs together; confirm a host can still supply its own compatibility policy while adopting the new session/service APIs |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [x] Feedback latency < 60s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
