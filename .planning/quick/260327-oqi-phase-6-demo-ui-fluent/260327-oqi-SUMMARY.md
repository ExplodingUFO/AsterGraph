---
mode: quick
slug: 260327-oqi-phase-6-demo-ui-fluent
objective: Tighten the Phase 6 Demo shell into a denser non-Fluent showcase layout, fix GraphEditorView header overlap, and preserve the single-editor architecture story.
commits:
  - 9390c13
  - 47e6aae
  - 9dd6cb7
files_modified:
  - src/AsterGraph.Demo/Views/MainWindow.axaml
  - src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
  - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
  - tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs
  - tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs
completed: 2026-03-27
---

# Quick Summary — 260327-oqi Phase 6 Demo UI Fluent

**The Phase 6 demo now reads as a denser SDK showcase, and the stock `GraphEditorView` header chrome no longer crowds title text, badges, and toolbar actions into one collision-prone top row.**

## What Changed

### Task 1 — Tighten the Demo shell layout and card density
- Compacted the left, center, and right rail card padding in `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/Views/MainWindow.axaml`.
- Reduced the hero footprint and editor framing so the single live `GraphEditorView` remains dominant without the roomy dashboard feel.
- Shortened the runtime helper copy in `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` to fit the denser right rail.
- Commit: `9390c13`

### Task 2 — Rework GraphEditorView header and toolbar into a compact non-Fluent shell
- Rebuilt the `PART_HeaderChrome` composition in `F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`.
- Split metadata badges into a dedicated right-aligned stack and gave the toolbar its own wrapping region with fixed compact action widths.
- Reduced header padding and title/body sizing so long text and actions can coexist cleanly in the Demo center rail.
- Commit: `47e6aae`

### Task 3 — Add regression coverage for the compact shell and non-overlapping top chrome
- Added density assertions for major demo cards and editor frame height in `F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs`.
- Added structural header assertions in `F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` for separated badges and wrapping toolbar layout.
- Updated `F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` to match the shortened helper copy.
- Commit: `9dd6cb7`

## Verification

Executed:
- `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|DemoDiagnosticsProjectionTests" -v minimal`
- `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "GraphEditorViewTests|GraphEditorLocalizationTests" -v minimal`
- `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|GraphEditorViewTests|GraphEditorLocalizationTests|DemoDiagnosticsProjectionTests" -v minimal`

Result: all filtered tests passed.

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None.

## Self-Check: PASSED
- Found summary target: `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-oqi-phase-6-demo-ui-fluent/260327-oqi-SUMMARY.md`
- Found commit `9390c13`
- Found commit `47e6aae`
- Found commit `9dd6cb7`
