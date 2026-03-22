# AsterGraph Node Parameter Editing Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add decoupled, MVVM-friendly node content editing to AsterGraph, including enum parameter support and Inspector-based editing.

**Architecture:** Parameter schema lives in `AsterGraph.Abstractions`, node instance values live in `AsterGraph.Core`, editing and validation state live in `AsterGraph.Editor`, and the Inspector UI in `AsterGraph.Avalonia` binds to editor-layer parameter view models.

**Tech Stack:** .NET 9, Avalonia 11, CommunityToolkit.Mvvm, C#

---

## Implementation Note

This repository is currently **not a git repository**, so commit steps are intentionally omitted.

The user has explicitly asked to keep moving without TDD, so this plan uses build and smoke verification instead of red-green-refactor steps.

### Task 1: Extend parameter schema in `AsterGraph.Abstractions`

**Files:**
- Modify: `src/AsterGraph.Abstractions/Definitions/NodeParameterDefinition.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/ParameterEditorKind.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/ParameterOptionDefinition.cs`
- Optionally create: `src/AsterGraph.Abstractions/Definitions/ParameterConstraints.cs`

**Step 1: Add parameter editor kinds**

Define:

- `Text`
- `Number`
- `Boolean`
- `Enum`
- `Color`

**Step 2: Add enum option metadata**

Support stable option definitions with:

- value
- label
- optional description

**Step 3: Add optional constraints**

Support simple constraints such as:

- min/max
- read-only
- allowed options

**Step 4: Update parameter definition contract**

Ensure `NodeParameterDefinition` can express:

- UI kind
- default value
- enum options
- simple validation metadata

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj
```

### Task 2: Add parameter instance values to `AsterGraph.Core`

**Files:**
- Create: `src/AsterGraph.Core/Models/GraphParameterValue.cs`
- Modify: `src/AsterGraph.Core/Models/GraphNode.cs`
- Modify: `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs` if needed

**Step 1: Add parameter instance value model**

Create a serializable representation for current node parameter values.

**Step 2: Extend `GraphNode`**

Add `ParameterValues` to node instances without mutating node definitions.

**Step 3: Preserve serialization**

Ensure parameter values save/load correctly.

**Step 4: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Core/AsterGraph.Core.csproj
```

### Task 3: Add parameter editing state to `AsterGraph.Editor`

**Files:**
- Create: `src/AsterGraph.Editor/ViewModels/NodeParameterViewModel.cs`
- Create: `src/AsterGraph.Editor/ViewModels/NodeParameterOptionViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`

**Step 1: Build merged parameter view models**

Combine:

- definition metadata
- current instance values

into Inspector-ready parameter view models.

**Step 2: Expose selected-node parameters**

Add a property on `GraphEditorViewModel` for the selected node’s parameter editors.

**Step 3: Add update commands or update methods**

Route parameter edits through editor-layer state handling.

**Step 4: Add validation**

Handle:

- required values
- parse failures
- min/max
- enum option membership

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj
```

### Task 4: Render parameter editing UI in the Inspector

**Files:**
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- Optionally create: value converter or helper files if needed

**Step 1: Add a Parameters section to the Inspector**

Show parameter editors only for the selected node.

**Step 2: Render controls by editor kind**

At minimum:

- text -> `TextBox`
- number -> `TextBox`
- boolean -> `CheckBox`
- enum -> `ComboBox`
- color -> text input initially

**Step 3: Show validation state**

Display invalid state or validation messages without moving validation rules into Avalonia.

### Task 5: Seed demo node definitions with sample parameters

**Files:**
- Modify: `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs`
- Modify: `src/AsterGraph.Demo/DemoGraphFactory.cs`

**Step 1: Add representative demo parameters**

Examples:

- noise scale: number
- iteration count: number
- enabled: boolean
- blend mode: enum

**Step 2: Seed node instance parameter values**

Make sure the demo graph starts with meaningful editable values.

### Task 6: Update documentation

**Files:**
- Modify: `README.md`
- Optionally modify: `src/AsterGraph.Abstractions/README.md`

**Step 1: Document node parameter editing**

Explain:

- schema vs instance values
- enum support
- Inspector editing flow

**Step 2: Document extension expectations**

Explain how custom node-definition providers add parameter schema.

### Task 7: Final verification

**Files:**
- Modify: any files required for final polish after verification

**Step 1: Run full build**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

**Step 2: Smoke-run the demo**

Run:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

**Step 3: Verify behavior**

Check:

- selecting a node reveals parameter editors
- editing a text/number/bool parameter updates editor state
- enum parameters render and select correctly
- invalid values are handled visibly
- save/load preserves parameter values

**Step 4: Record residual gaps**

Document anything deferred, such as:

- richer color picker
- parameter editor dialogs
- undo/redo for parameter edits
