---
phase: 32
slug: grapheditorviewmodel-facade-convergence
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-04-16
---

# Phase 32 - Validation Strategy

> Per-phase validation contract for facade-convergence feedback during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `dotnet` CLI plus the scripted maintenance lane |
| **Config file** | `tests/AsterGraph.Editor.Tests/*.cs`, `eng/ci.ps1` |
| **Quick run command** | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphContextMenuBuilderTests|FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests" -v minimal` |
| **Full suite command** | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` |
| **Estimated runtime** | focused facade parity tests <90 seconds; maintenance lane ~30-120 seconds |

---

## Sampling Rate

- **After every task commit:** run the narrowest command that proves the touched facade seam
  - guardrail additions: focused facade parity tests
  - constructor/bootstrap extraction: focused facade parity tests plus `GraphEditorSessionTests`
  - compatibility/fragment extraction: focused facade parity tests plus `EditorClipboardAndFragmentCompatibilityTests`
- **After every plan wave:** run the maintenance lane
- **Before phase verification:** both the focused facade parity command and the maintenance lane must be green
- **Max feedback latency:** under ~2 minutes for maintenance; under ~90 seconds for focused facade parity tests

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 32-01-01 | 01 | 1 | FACADE-01 | facade-guardrails | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphContextMenuBuilderTests" -v minimal` | ⬜ | ⬜ pending |
| 32-01-02 | 01 | 1 | FACADE-02 | maintenance-gate | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` | ⬜ | ⬜ pending |
| 32-02-01 | 02 | 2 | FACADE-01 | bootstrap-extraction | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal` | ⬜ | ⬜ pending |
| 32-02-02 | 02 | 2 | FACADE-02 | retained-runtime-parity | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal` | ⬜ | ⬜ pending |
| 32-03-01 | 03 | 3 | FACADE-01 | fragment-menu-extraction | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --filter "FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphContextMenuBuilderTests" -v minimal` | ⬜ | ⬜ pending |
| 32-03-02 | 03 | 3 | FACADE-02 | phase-final-gate | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\eng\\ci.ps1 -Lane maintenance -Framework all -Configuration Release` | ⬜ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure is enough to start the phase:

- maintenance already covers `GraphEditorSessionTests` and `GraphEditorFacadeRefactorTests`
- retained menu and fragment behavior already has direct coverage in `GraphEditorServiceSeamsTests`, `GraphContextMenuBuilderTests`, `EditorClipboardAndFragmentCompatibilityTests`, and `GraphEditorMigrationCompatibilityTests`
- Phase 31 already stabilized history/save semantics, so Phase 32 can focus on facade narrowing without inheriting a known semantic mismatch

Known baseline notes:

- `GraphEditorViewModel.cs` is still the constructor/composition hotspot even though most runtime state now lives in the kernel
- compatibility-menu and fragment command collaborators already exist but are still nested under `GraphEditorViewModel` partial files
- `GraphEditorSession(ViewModels.GraphEditorViewModel editor, ...)` still depends on retained-facade bootstrap helpers from the view model

---

## Manual-Only Verifications

No manual-only verification should remain after Phase 32.

If the phase ends with "the constructor is probably thinner now" but without focused parity tests over menu, fragment, and session descriptor behavior, treat the phase as incomplete.

---

## Validation Sign-Off

- [x] All planned tasks have an automated verify target
- [x] Sampling continuity stays on the shipped maintenance gate plus focused retained parity suites
- [x] Wave 0 records the current constructor/bootstrap hotspot and retained collaborator nesting explicitly
- [x] No watch-mode or manual-only verification is planned
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** auto-approved 2026-04-16 for autonomous Phase 32 planning
