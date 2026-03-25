---
phase: 02
slug: runtime-contracts-service-seams
status: draft
nyquist_compliant: false
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
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession|FullyQualifiedName~GraphEditorServiceSeams|FullyQualifiedName~GraphEditorTransaction" -v minimal` |
| **Full suite command** | `dotnet test avalonia-node-map.sln -v minimal` |
| **Estimated runtime** | ~60 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession|FullyQualifiedName~GraphEditorServiceSeams|FullyQualifiedName~GraphEditorTransaction" -v minimal`
- **After every plan wave:** Run `dotnet test avalonia-node-map.sln -v minimal`
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 02-01-01 | 01 | 1 | API-01, API-02, API-03 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession"` | ❌ W0 | ⬜ pending |
| 02-01-02 | 01 | 1 | API-04 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorTransaction"` | ❌ W0 | ⬜ pending |
| 02-02-01 | 02 | 2 | SERV-01, SERV-02 | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorServiceSeams"` | ❌ W0 | ⬜ pending |
| 02-03-01 | 03 | 3 | API-01, API-02, API-03, API-04, SERV-01 | full regression | `dotnet test avalonia-node-map.sln -v minimal` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` — session commands/queries/runtime event coverage
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs` — lightweight batching / transaction coverage
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` — service seam replacement and default storage-root behavior coverage

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Runtime session surface is coherent for external hosts | API-01, API-02, API-03 | API ergonomics and naming still need human review | Read the public runtime contract and factory entry path as a host author; confirm commands, queries, and events are discoverable without reading Avalonia code |
| Lightweight transaction API is understandable and not over-engineered | API-04 | Human judgment needed to distinguish practical batching from accidental architecture bloat | Review the public transaction surface and a host-side usage example; confirm it supports grouped edits and coherent notifications without introducing a bus/mediator system |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
