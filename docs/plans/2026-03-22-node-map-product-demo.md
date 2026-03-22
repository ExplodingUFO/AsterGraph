# Node Map Product Demo Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Upgrade the current Avalonia node-map demo into a product-like editor demo with a usable editing workflow and persistence.

**Architecture:** Extend the existing layered design with editor-state operations in `NodeMap.Avalonia` and JSON serialization in `NodeMap.Core`. Keep the demo host thin so it still depends only on the Avalonia facade.

**Tech Stack:** .NET 9, Avalonia 11, CommunityToolkit.Mvvm, System.Text.Json, C#

---

## Implementation Note

The user requested that this project continue without TDD. Verification for this plan therefore relies on build and runtime smoke checks instead of red-green-refactor loops.

### Task 1: Add persistence and editor-state primitives

**Files:**
- Create: `src/NodeMap.Core/Serialization/GraphDocumentSerializer.cs`
- Modify: `src/NodeMap.Avalonia/ViewModels/NodeEditorViewModel.cs`
- Modify: `src/NodeMap.Avalonia/ViewModels/NodeViewModel.cs`
- Modify: `src/NodeMap.Avalonia/ViewModels/ConnectionViewModel.cs`
- Create: `src/NodeMap.Avalonia/ViewModels/NodeTemplateViewModel.cs`
- Create: `src/NodeMap.Avalonia/Services/NodeEditorWorkspaceService.cs`

**Step 1: Add JSON save/load support**

Implement graph serialization in `NodeMap.Core` with indented JSON output and a stable API for save/load.

**Step 2: Make editor collections mutable**

Update `NodeEditorViewModel` so nodes and connections can be added, removed, exported, and reloaded.

**Step 3: Add workspace and status state**

Track:

- workspace file path
- dirty state
- status message
- pending connection source
- node template catalog

**Step 4: Verify**

Run:

```powershell
dotnet build src/NodeMap.Core/NodeMap.Core.csproj
dotnet build src/NodeMap.Avalonia/NodeMap.Avalonia.csproj
```

### Task 2: Add editing workflows to the canvas

**Files:**
- Modify: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml`
- Modify: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml.cs`

**Step 1: Make ports interactive**

Allow output ports to start a pending connection and input ports to complete one.

**Step 2: Render pending-connection preview**

Show a live preview curve while the pointer moves with a connection in progress.

**Step 3: Support scene updates for dynamic add/remove**

Rebuild or refresh canvas state when nodes and connections change.

**Step 4: Add view-fit support**

Expose a way for the shell to fit the current graph to the visible canvas.

### Task 3: Productize the editor shell

**Files:**
- Modify: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml`
- Modify: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml.cs`
- Modify: `src/NodeMap.Avalonia/Themes/NodeMapTheme.axaml`

**Step 1: Add toolbar**

Expose buttons for:

- add-node workflow guidance
- save
- load
- delete selection
- fit view
- reset view

**Step 2: Add left library panel**

Show node templates as product-style cards with category and port counts.

**Step 3: Add bottom status strip**

Surface dirty state, workspace path, and current operation status.

### Task 4: Wire the demo host and verify runtime flow

**Files:**
- Modify: `src/NodeMap.Demo/ViewModels/MainWindowViewModel.cs`
- Modify: `src/NodeMap.Demo/Views/MainWindow.axaml`
- Modify: `src/NodeMap.Demo/App.axaml` as needed for resources

**Step 1: Keep the host thin**

Continue to host `NodeEditorView` only and keep app logic inside `NodeMap.Avalonia`.

**Step 2: Run fresh verification**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

**Step 3: Smoke-run**

Run:

```powershell
dotnet run --project src/NodeMap.Demo/NodeMap.Demo.csproj
```

Expected:

- the demo opens
- nodes can be added
- connections can be created
- selected nodes can be deleted
- save/load path works
- fit/reset view works
