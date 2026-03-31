---
mode: quick
slug: 260327-oqi-phase-6-demo-ui-fluent
objective: Tighten the Phase 6 Demo shell into a denser non-Fluent showcase layout, fix GraphEditorView header overlap, and preserve the single-editor architecture story.
files_modified:
  - src/AsterGraph.Demo/Views/MainWindow.axaml
  - src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
  - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
  - tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs
constraints:
  - Honor D-01, D-02, D-03: keep the single-page three-column shell with one primary live editor.
  - Honor D-09, D-10, D-11: keep user-facing copy Chinese-first while preserving English API/type identifiers.
  - Do not introduce a second runtime model, new UI stack, or a separate diagnostics workbench.
---

<objective>
Make the Demo read like a compact SDK showcase instead of a roomy Fluent-style dashboard, while fixing the main editor toolbar/header collision and keeping the retained single-editor/session proof intact.

Purpose: address the current Phase 6 usability issue without changing the architecture demonstrated by the page.
Output: denser shell spacing, tighter cards, a corrected GraphEditorView header/toolbar layout, and focused regression coverage.
</objective>

<context>
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/STATE.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-CONTEXT.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-RESEARCH.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-UI-SPEC.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-03-SUMMARY.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/Views/MainWindow.axaml
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
@F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs
@F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs
</context>

<tasks>

<task type="auto">
  <name>Task 1: Tighten the Demo shell layout and card density</name>
  <files>src/AsterGraph.Demo/Views/MainWindow.axaml, src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs</files>
  <action>Refine the three-column shell per D-01, D-02, D-03, D-12, and D-13 so it stays a single-page architecture showcase but feels noticeably denser and less Fluent-like. Reduce overly large paddings, heading footprints, and empty vertical space; tighten left/right rail cards and the center proof area; keep only one dominant live GraphEditorView; and preserve the existing capability/runtime bindings instead of inventing new data islands. If any helper/proof copy becomes too long for the tighter layout, shorten the Chinese wording in `MainWindowViewModel` without removing the four required capability areas or the canonical seam names.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|DemoDiagnosticsProjectionTests"</automated>
  </verify>
  <done>The window still satisfies the three-column single-editor Phase 6 contract, but the shell spacing, card rhythm, and proof sections are visibly more compact.</done>
</task>

<task type="auto">
  <name>Task 2: Rework GraphEditorView header and toolbar into a compact non-Fluent shell</name>
  <files>src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml</files>
  <action>Fix the main editor top-area overlap by restructuring the GraphEditorView header/toolbar composition so long title/description text, stats pills, and action buttons can coexist without collision. Keep the Chinese-first shell copy from Phase 6, but restyle the header away from roomy Fluent spacing: tighten hero padding, reduce oversized gaps, and make the toolbar wrap or segment cleanly under constrained width. Do not move chrome-mode logic out of `GraphEditorViewChromeMode`, and do not remove the default full-shell value proof required by D-05 and D-11.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "GraphEditorViewTests|GraphEditorLocalizationTests"</automated>
  </verify>
  <done>The stock GraphEditorView reads as a tighter dark SDK shell, and its top header/actions no longer overlap when hosted inside the Demo center rail.</done>
</task>

<task type="auto">
  <name>Task 3: Add regression coverage for the compact shell and non-overlapping top chrome</name>
  <files>tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs, tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs</files>
  <action>Extend the focused Avalonia/headless tests so the quick fix is locked in. Add assertions for the denser Demo shell contract that matter for this bugfix (for example reduced major margins/paddings or tighter section sizing) and add/adjust GraphEditorView coverage to prove the top chrome uses a non-overlapping layout structure rather than a single crowded row. Keep the tests focused on structure and bindings the headless suite can verify reliably; do not add screenshot-style golden tests.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|GraphEditorViewTests|GraphEditorLocalizationTests|DemoDiagnosticsProjectionTests"</automated>
  </verify>
  <done>Focused UI regressions fail if the shell spacing balloons again or if GraphEditorView reverts to a collision-prone top chrome layout.</done>
</task>

</tasks>

<success_criteria>
- Phase 6 Demo keeps the locked three-column single-editor architecture story.
- The Demo shell and cards are materially denser and no longer read as a roomy Fluent-style dashboard.
- GraphEditorView top chrome no longer lets toolbar buttons and text collide in the center showcase.
- Focused headless tests cover the compact shell contract and top-chrome structure.
</success_criteria>
