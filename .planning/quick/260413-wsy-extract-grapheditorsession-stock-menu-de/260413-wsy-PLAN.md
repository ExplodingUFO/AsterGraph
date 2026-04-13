---
mode: quick
slug: 260413-wsy-extract-grapheditorsession-stock-menu-de
objective: Extract GraphEditorSession stock context-menu descriptor building into an internal helper without changing public API or plugin augmentor orchestration.
files_modified:
  - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
  - src/AsterGraph.Editor/Runtime/Internal/GraphEditorSessionStockMenuDescriptorBuilder.cs
  - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
constraints:
  - Keep `GraphEditorSession.BuildContextMenuDescriptors(...)` responsible for argument validation, command snapshot capture, stock-builder dispatch, and plugin augmentor fallback.
  - Do not change public API or touch kernel command routing, event batching, automation, diagnostics, or old retained menu-builder cleanup.
  - Preserve stock menu `Id/Header/IconKey/IsEnabled/DisabledReason/Command args/children` behavior.
  - Avoid reverting unrelated working-tree changes from other in-flight refactors.
---

<objective>
Split the stock context-menu descriptor builder block out of `GraphEditorSession` into a dedicated internal collaborator while preserving runtime behavior and regression signal.
</objective>

<context>
@F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/STATE.md
@F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
@F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add extraction guard and stock menu signature regression coverage</name>
  <files>tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs</files>
  <action>Add a reflection-based guard that fails while stock menu builder methods still live on `GraphEditorSession`, plus a session-level menu signature regression covering canvas/selection/node/port/connection descriptor trees.</action>
  <verify>
    <automated>Run a focused temporary test harness linked only to `GraphEditorSessionTests.cs` and confirm the new extraction guard fails before implementation.</automated>
  </verify>
  <done>The test suite proves both the intended split boundary and the stock menu descriptor behavior that must remain stable.</done>
</task>

<task type="auto">
  <name>Task 2: Extract stock menu building into an internal runtime helper</name>
  <files>src/AsterGraph.Editor/Runtime/GraphEditorSession.cs, src/AsterGraph.Editor/Runtime/Internal/GraphEditorSessionStockMenuDescriptorBuilder.cs</files>
  <action>Create an internal stock menu descriptor builder with only the minimum delegates/data it needs (document snapshot, selection snapshot, compatible-target query, localization, default definitions). Update `GraphEditorSession` to instantiate and call it while keeping the outer plugin augmentor orchestration unchanged.</action>
  <verify>
    <automated>dotnet build "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Editor/AsterGraph.Editor.csproj" -v minimal</automated>
  </verify>
  <done>`GraphEditorSession` no longer owns the stock builder methods, and runtime menu output remains unchanged.</done>
</task>

<task type="auto">
  <name>Task 3: Re-run focused verification and document unrelated test-project blockage</name>
  <files>.planning/quick/260413-wsy-extract-grapheditorsession-stock-menu-de/260413-wsy-SUMMARY.md, .planning/STATE.md</files>
  <action>Re-run the focused harness for all `GraphEditorSessionTests`, capture the successful commands, and note that the main `AsterGraph.Editor.Tests` project currently has unrelated compile errors in other test files that were not modified in this quick task.</action>
  <verify>
    <automated>Run the focused temporary harness for all `GraphEditorSessionTests` and confirm green.</automated>
  </verify>
  <done>The quick task records both the successful focused verification and the reason the full shared test project was not used.</done>
</task>

</tasks>

<success_criteria>
- `GraphEditorSession` no longer contains the stock menu builder methods named in the task.
- Stock menu descriptor trees remain byte-for-byte stable for the covered runtime contexts.
- `GraphEditorSession.BuildContextMenuDescriptors(...)` still performs argument validation, command snapshot capture, stock item generation, and plugin augmentor fallback in that order.
- Verification is recorded without touching unrelated user changes.
</success_criteria>
