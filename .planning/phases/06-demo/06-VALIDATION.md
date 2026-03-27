---
phase: 6
slug: demo
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-27
---

# Phase 6 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.2 + Avalonia.Headless.XUnit 11.3.10 |
| **Config file** | none — convention via test projects |
| **Quick run command** | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter Demo` |
| **Full suite command** | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj"` |
| **Estimated runtime** | ~45 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter Demo`
- **After every plan wave:** Run `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj"`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 45 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 6-01-01 | 01 | 1 | DEMO-LAYOUT-01 | headless UI | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter DemoMainWindowTests` | ❌ W0 | ⬜ pending |
| 6-01-02 | 01 | 1 | DEMO-I18N-01 | headless UI / view-model | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter GraphEditorLocalizationTests` | ❌ W0 | ⬜ pending |
| 6-02-01 | 02 | 2 | DEMO-SURFACES-01 | headless UI | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter GraphEditorViewTests` | ✅ | ⬜ pending |
| 6-02-02 | 02 | 2 | DEMO-PRESENTATION-01 | host sample / focused test | `dotnet run --project "F:/CodeProjects/DotnetCore/avalonia-node-map/tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj"` | ✅ | ⬜ pending |
| 6-03-01 | 03 | 3 | DEMO-DIAG-01 | focused VM test / host sample | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter DemoDiagnosticsProjectionTests` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` — stubs for DEMO-LAYOUT-01
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs` — stubs for DEMO-I18N-01
- [ ] `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` — stubs for DEMO-DIAG-01
- [ ] Isolate or fix current editor-test noise before treating unrelated failures as Phase 6 regressions

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Demo desktop readability across normal window sizes | DEMO-LAYOUT-01 | Requires visual judgment of three-column density and center-editor dominance | Run `dotnet run --project "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/AsterGraph.Demo.csproj"`, confirm left rail, center main editor, and right evidence rail are visible without route/tab switching. |
| Chinese-first shell copy feels coherent with technical seam labels | DEMO-I18N-01 | Need human review of UX language quality while preserving API names in English | In the running Demo, inspect toolbar, side labels, helper copy, diagnostics copy, and any API seam labels; confirm user-facing UX copy is Chinese while technical seam names remain English where intended. |
| Runtime/diagnostics proof cards explain live data clearly | DEMO-DIAG-01 | Requires human assessment that cards communicate machine-readable diagnostics rather than status-bar text only | In the running Demo, trigger state changes and verify the right rail surfaces inspection/diagnostic data from session-backed cards, distinct from the bottom status text. |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 45s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
