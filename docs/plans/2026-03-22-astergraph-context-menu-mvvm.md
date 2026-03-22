# AsterGraph Context Menu And MVVM Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add nested right-click context menus and a stronger MVVM command flow to AsterGraph, while switching canvas panning from right-drag to middle-drag.

**Architecture:** `AsterGraph.Editor` will own menu descriptors, menu context, and command intent, while `AsterGraph.Avalonia` will render those descriptors as Avalonia context menus and route the resulting actions back into editor commands. The existing graph editor state remains the source of truth.

**Tech Stack:** .NET 9, Avalonia 11, CommunityToolkit.Mvvm, C#

---

## Implementation Note

This repository is currently **not a git repository**, so commit steps are intentionally omitted.

The user has repeatedly requested progress without TDD, so this plan uses build and smoke verification instead of red-green-refactor steps.

### Task 1: Add menu descriptor and menu context contracts to `AsterGraph.Editor`

**Files:**
- Create: `src/AsterGraph.Editor/Menus/ContextMenuTargetKind.cs`
- Create: `src/AsterGraph.Editor/Menus/ContextMenuContext.cs`
- Create: `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`

**Step 1: Create target-kind modeling**

Represent:

- canvas
- node
- port
- connection

as explicit menu target kinds.

**Step 2: Create menu context modeling**

Include the target object plus right-click world position and current selection state.

**Step 3: Create recursive menu descriptor modeling**

Support:

- nested children
- enable/disable state
- separators
- command + command parameter

**Step 4: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj
```

### Task 2: Move core editor actions into MVVM commands

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`

**Step 1: Add command properties**

Expose command properties for:

- save
- load
- fit view
- reset view
- delete selection
- add node
- delete connection
- cancel pending connection

**Step 2: Keep existing behavior but route through commands**

The command-backed actions should still update shared editor state and status messages.

**Step 3: Add right-click placement support**

Allow add-node actions to insert at a supplied world position instead of always using viewport center.

### Task 3: Add menu builders in the editor layer

**Files:**
- Create: `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`

**Step 1: Build canvas menu generation**

Generate:

- `Add Node >`
- `Paste`
- `Fit View`
- `Reset View`
- `Save Snapshot`
- `Load Snapshot`

**Step 2: Build node menu generation**

Generate:

- `Delete Node`
- `Duplicate Node`
- `Disconnect >`
- `Create Connection From >`
- `Inspect`
- `Center View Here`

**Step 3: Build port menu generation**

Generate:

- `Start Connection`
- `Break Connections`
- `Compatible Targets >`
- `Compatibility Info`

**Step 4: Build connection menu generation**

Generate:

- `Delete Connection`
- `Inspect Conversion`
- `Insert Conversion >`

### Task 4: Refactor Avalonia controls to render nested context menus

**Files:**
- Modify: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`

**Step 1: Replace right-button panning with middle-button panning**

Keep canvas pan on middle-button drag only.

**Step 2: Detect right-click target**

Determine whether the click landed on:

- canvas
- node
- port
- connection

**Step 3: Render nested `ContextMenu` instances from descriptors**

The Avalonia layer should turn editor-provided menu descriptors into nested menu items.

**Step 4: Preserve current pointer behaviors**

Do not regress:

- left-button drag node move
- wheel zoom
- selection

### Task 5: Wire toolbar actions through commands

**Files:**
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`

**Step 1: Replace direct button click wiring where practical**

Bind toolbar actions to command properties from `GraphEditorViewModel`.

**Step 2: Keep minimal code-behind only where Avalonia menu/control bridging requires it**

The goal is stronger MVVM, not zero code-behind dogma.

### Task 6: Update documentation

**Files:**
- Modify: `README.md`
- Optionally modify: `src/AsterGraph.Avalonia/README.md`

**Step 1: Document right-click behavior**

Explain:

- right-click opens menus
- middle-button pans
- menus are target-sensitive

**Step 2: Document command/MVVM direction**

Explain that toolbar and menu actions share editor-layer commands.

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

**Step 3: Verify interaction outcomes**

Check:

- right-click opens a menu on the canvas
- right-click on nodes opens node-specific menu
- nested `Add Node >` menu works
- middle-button panning still works
- toolbar actions still work
- command-driven status messages still update

**Step 4: Record residual gaps**

Document anything intentionally deferred, especially:

- connection multiselect
- plugin-contributed menu entries
- advanced clipboard flow
- parameter editor dialogs
