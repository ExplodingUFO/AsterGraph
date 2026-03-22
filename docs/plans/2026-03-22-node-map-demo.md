# Node Map Demo Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a first runnable Avalonia node-map demo with modular libraries and a demo app that references only `NodeMap.Avalonia`.

**Architecture:** The solution is split into pure graph/domain logic, layout helpers, an Avalonia UI facade, and a thin demo application. The reusable integration surface is `NodeMap.Avalonia`, while the demo app serves only as a host.

**Tech Stack:** .NET 9, Avalonia 11, CommunityToolkit.Mvvm, C#

---

## Implementation Note

The user explicitly requested that this first pass should not use TDD. Each task therefore uses scaffold/build/smoke verification instead of red-green-refactor steps.

### Task 1: Scaffold solution and project structure

**Files:**
- Create: `avalonia-node-map.sln`
- Create: `src/NodeMap.Core/NodeMap.Core.csproj`
- Create: `src/NodeMap.Layout/NodeMap.Layout.csproj`
- Create: `src/NodeMap.Avalonia/NodeMap.Avalonia.csproj`
- Create: `src/NodeMap.Demo/NodeMap.Demo.csproj`
- Create: `Directory.Build.props`

**Step 1: Create solution and folders**

Run:

```powershell
dotnet new sln -n avalonia-node-map
dotnet new classlib -n NodeMap.Core -o src/NodeMap.Core
dotnet new classlib -n NodeMap.Layout -o src/NodeMap.Layout
dotnet new classlib -n NodeMap.Avalonia -o src/NodeMap.Avalonia
dotnet new avalonia.mvvm -n NodeMap.Demo -o src/NodeMap.Demo
```

**Step 2: Normalize project settings**

- target `net9.0`
- enable nullable reference types
- centralize common properties in `Directory.Build.props`
- add Avalonia package references only where needed

**Step 3: Wire project references**

- `NodeMap.Layout -> NodeMap.Core`
- `NodeMap.Avalonia -> NodeMap.Core`
- `NodeMap.Avalonia -> NodeMap.Layout`
- `NodeMap.Demo -> NodeMap.Avalonia`

**Step 4: Add projects to solution**

Run:

```powershell
dotnet sln avalonia-node-map.sln add src/NodeMap.Core/NodeMap.Core.csproj
dotnet sln avalonia-node-map.sln add src/NodeMap.Layout/NodeMap.Layout.csproj
dotnet sln avalonia-node-map.sln add src/NodeMap.Avalonia/NodeMap.Avalonia.csproj
dotnet sln avalonia-node-map.sln add src/NodeMap.Demo/NodeMap.Demo.csproj
```

**Step 5: Verify scaffold**

Run:

```powershell
dotnet restore avalonia-node-map.sln
dotnet build avalonia-node-map.sln
```

### Task 2: Implement graph core and demo data

**Files:**
- Create: `src/NodeMap.Core/Models/GraphDocument.cs`
- Create: `src/NodeMap.Core/Models/GraphNode.cs`
- Create: `src/NodeMap.Core/Models/GraphPort.cs`
- Create: `src/NodeMap.Core/Models/GraphConnection.cs`
- Create: `src/NodeMap.Core/Models/PortDirection.cs`
- Create: `src/NodeMap.Core/Demo/DemoGraphFactory.cs`

**Step 1: Create core graph types**

Implement the basic document model with node IDs, port IDs, labels, categories, and positions.

**Step 2: Create demo graph factory**

Generate a visually useful sample graph with multiple categories, varying node sizes, and several cross-node connections.

**Step 3: Keep the model UI-agnostic**

Do not reference Avalonia types in this project. Use primitive numeric values for coordinates and sizes.

**Step 4: Verify core compiles**

Run:

```powershell
dotnet build src/NodeMap.Core/NodeMap.Core.csproj
```

### Task 3: Implement layout and connection geometry helpers

**Files:**
- Create: `src/NodeMap.Layout/Geometry/PortAnchor.cs`
- Create: `src/NodeMap.Layout/Geometry/ConnectionPathBuilder.cs`
- Create: `src/NodeMap.Layout/Viewport/ViewportState.cs`
- Create: `src/NodeMap.Layout/Viewport/ViewportMath.cs`

**Step 1: Add viewport model**

Represent pan offset and zoom scale, with helper methods to translate between screen and world coordinates.

**Step 2: Add port-anchor math**

Given node bounds and port index, compute deterministic anchor points for input and output ports.

**Step 3: Add connection path generation**

Produce bezier control points or path segments suitable for Avalonia path drawing.

**Step 4: Verify layout library**

Run:

```powershell
dotnet build src/NodeMap.Layout/NodeMap.Layout.csproj
```

### Task 4: Implement the Avalonia node editor library

**Files:**
- Create: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml`
- Create: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml.cs`
- Create: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml`
- Create: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml.cs`
- Create: `src/NodeMap.Avalonia/ViewModels/NodeEditorViewModel.cs`
- Create: `src/NodeMap.Avalonia/ViewModels/NodeViewModel.cs`
- Create: `src/NodeMap.Avalonia/ViewModels/PortViewModel.cs`
- Create: `src/NodeMap.Avalonia/ViewModels/ConnectionViewModel.cs`
- Create: `src/NodeMap.Avalonia/Services/NodeEditorDemoFactory.cs`
- Create: `src/NodeMap.Avalonia/Themes/NodeMapTheme.axaml`

**Step 1: Map core data into UI view models**

Create the view-model layer that wraps the core graph document and exposes nodes, ports, connections, selection, and viewport state.

**Step 2: Build the editor surface**

Create a canvas with:

- background grid
- node cards
- input/output ports
- connection overlays
- zoom and pan transforms

**Step 3: Add interactions**

Support:

- node dragging
- wheel zoom
- panning
- selection

**Step 4: Package the public integration surface**

Expose a simple way for host apps to instantiate the demo editor without touching lower-level libraries directly.

**Step 5: Verify UI library builds**

Run:

```powershell
dotnet build src/NodeMap.Avalonia/NodeMap.Avalonia.csproj
```

### Task 5: Wire the demo host application

**Files:**
- Modify: `src/NodeMap.Demo/App.axaml`
- Modify: `src/NodeMap.Demo/App.axaml.cs`
- Modify: `src/NodeMap.Demo/MainWindow.axaml`
- Modify: `src/NodeMap.Demo/MainWindow.axaml.cs`
- Modify: `src/NodeMap.Demo/ViewModels/MainWindowViewModel.cs`
- Delete or simplify: template-generated sample files not needed by the final shell

**Step 1: Replace template content with the node editor demo shell**

Use a layout that highlights the node editor and provides a compact instruction panel.

**Step 2: Keep the demo app dependency surface clean**

Ensure the demo app references only `NodeMap.Avalonia`.

**Step 3: Validate runtime startup**

Run:

```powershell
dotnet run --project src/NodeMap.Demo/NodeMap.Demo.csproj
```

Expected:

- the application starts successfully
- the node map renders immediately
- nodes can be dragged
- wheel zoom and panning work

### Task 6: Final cleanup and repository verification

**Files:**
- Modify: any files required for final polish after smoke verification

**Step 1: Run final solution verification**

Run:

```powershell
dotnet restore avalonia-node-map.sln
dotnet build avalonia-node-map.sln
```

**Step 2: Smoke-run the demo**

Run:

```powershell
dotnet run --project src/NodeMap.Demo/NodeMap.Demo.csproj
```

**Step 3: Record any remaining limitations**

Document deferred features and any known rough edges in the final handoff.
