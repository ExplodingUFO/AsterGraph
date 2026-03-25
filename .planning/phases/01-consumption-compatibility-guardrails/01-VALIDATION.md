---
phase: 01
slug: consumption-compatibility-guardrails
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-25
---

# Phase 01 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.2 + Avalonia.Headless.XUnit 11.3.10 |
| **Config file** | none — test behavior comes from project files and existing test assembly attributes |
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~GraphEditorFacadeRefactorTests" -v minimal` |
| **Full suite command** | `dotnet test avalonia-node-map.sln -v minimal` |
| **Estimated runtime** | ~60 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~GraphEditorFacadeRefactorTests" -v minimal`
- **After every plan wave:** Run `dotnet test avalonia-node-map.sln -v minimal`
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 01-01-01 | 01 | 0 | PKG-01 | build hygiene | `dotnet test avalonia-node-map.sln -v minimal` | ❌ W0 | ⬜ pending |
| 01-02-01 | 02 | 1 | PKG-02 | unit + smoke | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitializationTests" -v minimal` | ❌ W0 | ⬜ pending |
| 01-03-01 | 03 | 1 | PKG-03 | unit + smoke | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorFacadeRefactorTests" -v minimal` | ❌ W0 | ⬜ pending |
| 01-04-01 | 04 | 2 | PKG-01, PKG-02, PKG-03 | package smoke | `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true` | ✅ | ⬜ pending |
| 01-04-02 | 04 | 2 | PKG-02, PKG-03 | host smoke | `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `src/**/artifacts/**` generated `.cs` outputs excluded from default compile items or cleaned before validation
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` — initialization helper and options/factory coverage
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` — compatibility facade and migration parity coverage
- [ ] `tools/AsterGraph.PackageSmoke/Program.cs` updated to reflect the finalized public package boundary and entry story

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Consumer-facing migration guidance is understandable and staged | PKG-03 | Docs quality and migration clarity still need human review | Review `README.md`, package READMEs, and `docs/host-integration.md`; confirm old and new entry paths are both documented with a clear staged migration story |
| Package boundary narrative is internally consistent | PKG-01 | Human judgment needed to detect mixed messaging across docs and samples | Compare root README, package READMEs, host integration guide, package smoke, and host sample; confirm they present the same host dependency story |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
