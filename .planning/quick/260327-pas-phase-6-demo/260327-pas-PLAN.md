---
phase: 260327-pas-phase-6-demo
plan: 1
type: execute
wave: 1
depends_on: []
files_modified:
  - src/AsterGraph.Demo/Views/MainWindow.axaml
  - src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
  - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
  - src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs
  - tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs
autonomous: true
requirements:
  - PHASE6-DEMO
must_haves:
  truths:
    - "Demo reads as a low-radius squared professional tool shell instead of a Fluent-style showcase."
    - "The center GraphEditorView fills the available height while side rails scroll independently."
    - "Hosts can independently hide the top bar, node library, right inspector rail, and bottom status bar in the live node graph shell."
  artifacts:
    - path: "src/AsterGraph.Demo/Views/MainWindow.axaml"
      provides: "Squared demo shell layout with full-height center editor composition"
    - path: "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs"
      provides: "Demo-level commands/state for independent chrome visibility toggles"
    - path: "src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml"
      provides: "Independent shell chrome visibility bindings and full-height graph layout"
    - path: "tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs"
      provides: "Regression coverage for demo shell density and full-height editor composition"
    - path: "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs"
      provides: "Regression coverage for per-region chrome visibility"
  key_links:
    - from: "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs"
      to: "src/AsterGraph.Demo/Views/MainWindow.axaml"
      via: "new toggle properties/commands bound into the demo shell"
      pattern: "Is.*Visible|Show.*"
    - from: "src/AsterGraph.Demo/Views/MainWindow.axaml"
      to: "src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml"
      via: "GraphEditorView instance receives independent chrome visibility configuration"
      pattern: "GraphEditorView.*Chrome|Is.*Visible"
    - from: "src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml"
      to: "tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs"
      via: "named shell regions and visibility assertions"
      pattern: "PART_.*Chrome|IsVisible"
---

<objective>
Apply one focused Phase 6 demo polish pass that honors the locked three-column showcase while making the shell more squared-off, keeping the main editor graph full-height, and exposing independent show/hide control for graph shell regions.

Purpose: Match the user's revised visual direction and embedding proof goals without changing the single-editor architecture story.
Output: Updated Demo shell styling/layout, GraphEditorView chrome visibility seams, and focused regression tests.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/STATE.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-CONTEXT.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/phases/06-demo/06-UI-SPEC.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-pas-phase-6-demo/260327-pas-CONTEXT.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-oqi-phase-6-demo-ui-fluent/260327-oqi-SUMMARY.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/Views/MainWindow.axaml
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml
@F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs

<interfaces>
From src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs:
```csharp
public GraphEditorViewModel Editor { get; }
public bool IsGridSnappingEnabled { get; set; }
public bool IsAlignmentGuidesEnabled { get; set; }
public bool IsReadOnlyEnabled { get; set; }
public bool AreWorkspaceCommandsEnabled { get; set; }
public bool AreFragmentCommandsEnabled { get; set; }
public bool AreHostMenuExtensionsEnabled { get; set; }
```

From src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml:
```xml
<Border x:Name="PART_HeaderChrome" ... />
<Border x:Name="PART_LibraryChrome" ... />
<controls:NodeCanvas x:Name="PART_NodeCanvas" ... />
<Border x:Name="PART_InspectorChrome" ... />
<Border x:Name="PART_StatusChrome" ... />
```

Use those existing shell regions directly. Do not introduce a second editor runtime or move visual-only shell toggles into editor behavior options.
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Restyle the demo shell into the low-radius squared direction</name>
  <files>src/AsterGraph.Demo/Views/MainWindow.axaml, src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs</files>
  <action>Update the Demo shell per the quick context visual decision and D-01/D-03/D-12: replace the current softer Fluent-like card treatment with a more squared, professional-tool look using lower corner radii, firmer borders, and denser section framing while preserving the locked three-column overview and single main editor. Keep the left rail as control/navigation and the right rail as explanation/diagnostics; do not add new pages, tabs, or duplicate editors. If the new hide/show entry point lives in the Demo shell, place it in a clear host-control area and label each control in Chinese.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests" -v minimal</automated>
  </verify>
  <done>The Demo shell visually reads as lower-radius and more squared-off, while existing three-column labels and one-main-editor structure still render.</done>
</task>

<task type="auto">
  <name>Task 2: Make the main graph surface fill height and support independent chrome visibility</name>
  <files>src/AsterGraph.Demo/Views/MainWindow.axaml, src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs, src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml, src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs</files>
  <action>Implement the quick context height and chrome decisions per D-03 plus the locked quick requirements: ensure the center GraphEditorView/NodeCanvas stretches to the full available height of its container while the left and right rails scroll independently and no longer compress the center graph area. Add independent visibility control for the top bar, node library, right inspector rail, and bottom status bar. Keep these as visual shell toggles in the Avalonia shell layer per project architecture, not editor behavior options. Prefer binding explicit booleans or a small visual-only chrome configuration over a single coarse mode switch; if the existing GraphEditorViewChromeMode remains, extend around it rather than replacing the independent per-region seams with another monolithic mode.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "GraphEditorViewTests|DemoMainWindowTests" -v minimal</automated>
  </verify>
  <done>The center graph shell fills available height, side rails own their own scrolling, and each of the four chrome regions can be hidden independently without rebuilding the editor session.</done>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Lock the quick fix with focused shell regressions</name>
  <files>tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs, tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs</files>
  <behavior>
    - Test 1: The Demo center editor container uses stretch/full-height composition instead of a fixed short frame.
    - Test 2: The Demo still renders the locked three-column showcase contract after the restyle.
    - Test 3: GraphEditorView can hide header, library, inspector, and status chrome independently while keeping the node canvas present.
  </behavior>
  <action>Update or add headless Avalonia regression tests that prove the new composition and per-region visibility seams. Assert against named shell parts and layout/visibility state instead of snapshotting pixel output. Keep tests focused on this quick pass and avoid broad workspace-noise coverage.</action>
  <verify>
    <automated>dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter "DemoMainWindowTests|GraphEditorViewTests" -v minimal</automated>
  </verify>
  <done>Focused tests fail before the changes and pass after them, covering both full-height composition and independent chrome visibility.</done>
</task>

</tasks>

<verification>
Run the focused editor UI test slice for DemoMainWindowTests and GraphEditorViewTests. Confirm the center graph no longer depends on a fixed short frame and each named chrome region can be toggled independently while the canvas remains mounted.
</verification>

<success_criteria>
- Demo shell keeps the locked Phase 6 three-column architecture story while adopting the requested low-radius squared styling.
- Main editor graph visually occupies the full available height of the center rail.
- Header, library, inspector, and status regions each have independent visibility control.
- Focused headless UI tests cover the restyle contract and new visibility seams.
</success_criteria>

<output>
After completion, create `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-pas-phase-6-demo/260327-pas-SUMMARY.md`
</output>
