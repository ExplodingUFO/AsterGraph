# AsterGraph Editor Facade Refactor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Reduce the internal responsibility load of the editor and Avalonia host layers without breaking the current host-facing API surface.

**Architecture:** Keep `GraphEditorViewModel` and `NodeCanvas` as stable public facades, but move behavior clusters behind internal collaborators. Start with the editor layer because it has the largest concentration of state mutation, projection logic, and command invalidation. Extract parameter normalization next so future host-defined editors and runtime extensions can compose behavior without subclassing view models.

**Tech Stack:** .NET 8/9, Avalonia 11, CommunityToolkit.Mvvm, xUnit

---

### Task 1: Extract editor state projection and parameter orchestration from `GraphEditorViewModel`

**Files:**
- Create: `src/AsterGraph.Editor/Services/GraphEditorInspectorProjection.cs`
- Create: `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void RebuildSelectedNodeParameters_UsesProjectionServiceAndPreservesSharedBatchEditing()
{
    var editor = GraphEditorTestFactory.CreateEditorWithSharedDefinitionSelection();

    editor.SetSelection(editor.SelectedNodes, editor.SelectedNodes[0]);

    Assert.Equal(2, editor.SelectedNodeParameters.Count);
    Assert.Contains(editor.SelectedNodeParameters, parameter => parameter.HasMixedValues);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorFacadeRefactorTests`
Expected: FAIL because the new test fixture or refactored collaborator does not exist yet.

**Step 3: Write minimal implementation**

```csharp
internal sealed class GraphEditorInspectorProjection
{
    public IReadOnlyList<NodeParameterViewModel> BuildParameters(...)
    {
        ...
    }
}
```

Move inspector string formatting, related-node captions, and parameter list reconstruction into internal helpers. Keep all public properties and methods on `GraphEditorViewModel` unchanged.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorFacadeRefactorTests`
Expected: PASS

**Step 5: Commit**

```bash
git add tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs src/AsterGraph.Editor/Services/GraphEditorInspectorProjection.cs src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
git commit -m "refactor: extract editor projection services"
```

### Task 2: Extract parameter value normalization and validation into a reusable service

**Files:**
- Create: `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/NodeParameterViewModel.cs`
- Test: `tests/AsterGraph.Editor.Tests/NodeParameterValueAdapterTests.cs`

**Step 1: Write the failing test**

```csharp
[Theory]
[InlineData("1,5", 1.5)]
[InlineData("1.5", 1.5)]
public void Normalize_NumberEditor_AcceptsCurrentAndInvariantCultureInputs(string raw, double expected)
{
    var result = NodeParameterValueAdapter.Normalize(..., raw);

    Assert.True(result.IsSuccess);
    Assert.Equal(expected, (double)result.Value!);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter NodeParameterValueAdapterTests`
Expected: FAIL because the adapter does not exist yet.

**Step 3: Write minimal implementation**

```csharp
internal static class NodeParameterValueAdapter
{
    public static NodeParameterValueNormalizationResult Normalize(...)
    {
        ...
    }
}
```

Move JSON normalization, enum validation, boolean parsing, numeric coercion, and string fallback into the adapter. Keep `NodeParameterViewModel` focused on observable state and command routing.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter NodeParameterValueAdapterTests`
Expected: PASS

**Step 5: Commit**

```bash
git add tests/AsterGraph.Editor.Tests/NodeParameterValueAdapterTests.cs src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs src/AsterGraph.Editor/ViewModels/NodeParameterViewModel.cs
git commit -m "refactor: extract parameter value adapter"
```

### Task 3: Extract `NodeCanvas` interaction state machine behind an internal controller

**Files:**
- Create: `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasInteractionController.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void ApplyDragOffset_UsesAbsoluteOriginPositionsInsteadOfAccumulatedDeltas()
{
    var editor = GraphEditorTestFactory.CreateEditorWithSelection();
    var origin = editor.GetNodePositions().ToDictionary(x => x.NodeId, x => x.Position);

    editor.ApplyDragOffset(origin, 24, 16);
    editor.ApplyDragOffset(origin, 24, 16);

    Assert.All(editor.SelectedNodes, node => Assert.Equal(origin[node.Id].X + 24, node.X));
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorFacadeRefactorTests`
Expected: FAIL if the interaction split changes drag semantics incorrectly or if fixture is not present yet.

**Step 3: Write minimal implementation**

```csharp
internal sealed class NodeCanvasInteractionController
{
    public void HandlePointerPressed(...)
    {
        ...
    }
}
```

Move pointer-gesture session state, marquee bookkeeping, drag-assist decision flow, and middle-button panning orchestration behind the controller. Keep actual Avalonia control lookup and rendering in `NodeCanvas`.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorFacadeRefactorTests`
Expected: PASS

**Step 5: Commit**

```bash
git add tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasInteractionController.cs src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs
git commit -m "refactor: extract node canvas interaction controller"
```

### Task 4: Update README and package-facing documentation for the stable facade boundary

**Files:**
- Modify: `README.md`
- Modify: `src/AsterGraph.Editor/README.md`
- Modify: `src/AsterGraph.Avalonia/README.md`

**Step 1: Write the failing test**

There is no meaningful automated test for README text. Use documentation diff review only after code tasks are green.

**Step 2: Run test to verify it fails**

Skip. Documentation-only task.

**Step 3: Write minimal implementation**

Document that `GraphEditorViewModel` and `GraphEditorView` remain the stable host entry points while editor internals continue moving behind collaborators.

**Step 4: Run test to verify it passes**

Run: `dotnet build avalonia-node-map.sln`
Expected: PASS

**Step 5: Commit**

```bash
git add README.md src/AsterGraph.Editor/README.md src/AsterGraph.Avalonia/README.md
git commit -m "docs: clarify editor facade boundary"
```
