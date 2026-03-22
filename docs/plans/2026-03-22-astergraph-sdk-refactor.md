# AsterGraph SDK Refactor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rename and refactor the current node-graph demo into the `AsterGraph` SDK structure, with cleaner layering, C# extension points, strict type compatibility plus safe implicit conversions, and developer documentation.

**Architecture:** The refactor introduces `AsterGraph.Abstractions` for stable contracts, `AsterGraph.Core` for pure graph and compatibility logic, `AsterGraph.Editor` for editor-state orchestration, `AsterGraph.Avalonia` for UI, and `AsterGraph.Demo` for sample registration. The public extension model is definition-driven and compile-time friendly, while leaving a clean seam for future runtime plugins.

**Tech Stack:** .NET 9, Avalonia 11, CommunityToolkit.Mvvm, System.Text.Json, C#

---

## Implementation Note

This repository is currently **not a git repository**, so commit steps are intentionally omitted.

The user explicitly asked that this project continue **without TDD**, so this plan uses build and smoke verification rather than red-green-refactor steps.

### Task 1: Rename solution, projects, namespaces, and public editor surface

**Files:**
- Modify: `avalonia-node-map.sln`
- Modify: `src/NodeMap.Core/NodeMap.Core.csproj`
- Modify: `src/NodeMap.Layout/NodeMap.Layout.csproj`
- Modify: `src/NodeMap.Avalonia/NodeMap.Avalonia.csproj`
- Modify: `src/NodeMap.Demo/NodeMap.Demo.csproj`
- Create: `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`
- Create: `src/AsterGraph.Editor/AsterGraph.Editor.csproj`
- Move/rename: existing source files into `src/AsterGraph.*`

**Step 1: Create the new project layout**

Create:

- `src/AsterGraph.Abstractions`
- `src/AsterGraph.Core`
- `src/AsterGraph.Editor`
- `src/AsterGraph.Avalonia`
- `src/AsterGraph.Demo`

**Step 2: Update solution membership**

Replace old project paths in the solution with the new `AsterGraph.*` projects.

**Step 3: Rename namespaces and public types**

Apply the following renames across code and XAML:

- `NodeMap.*` -> `AsterGraph.*`
- `NodeEditorView` -> `GraphEditorView`
- `NodeEditorViewModel` -> `GraphEditorViewModel`

**Step 4: Verify the rename baseline**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

Expected:

- all projects build under the new names
- no stale `NodeMap.` references remain in compile paths

### Task 2: Introduce stable extension contracts in `AsterGraph.Abstractions`

**Files:**
- Create: `src/AsterGraph.Abstractions/Identifiers/NodeDefinitionId.cs`
- Create: `src/AsterGraph.Abstractions/Identifiers/PortTypeId.cs`
- Create: `src/AsterGraph.Abstractions/Identifiers/ConversionId.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/PortDefinition.cs`
- Create: `src/AsterGraph.Abstractions/Definitions/NodeParameterDefinition.cs`
- Create: `src/AsterGraph.Abstractions/Catalog/INodeDefinitionProvider.cs`
- Create: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`
- Create: `src/AsterGraph.Abstractions/Compatibility/IPortCompatibilityService.cs`
- Create: `src/AsterGraph.Abstractions/README.md`

**Step 1: Create stable identifiers and records**

Define immutable identifier/value types for:

- node definitions
- port types
- implicit conversion IDs

**Step 2: Define node and port contracts**

Introduce definition-level contracts that describe:

- display metadata
- input/output ports
- default node size
- optional parameter metadata

**Step 3: Define catalog and compatibility service contracts**

Add extension-facing interfaces for:

- providing definitions
- registering and resolving definitions
- evaluating compatibility between ports

**Step 4: Add concise architecture comments**

Document why this layer exists and why it must remain stable for future plugin scenarios.

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj
```

### Task 3: Move graph data and compatibility rules into `AsterGraph.Core`

**Files:**
- Modify: graph model files currently under `src/NodeMap.Core/Models/*`
- Modify: serialization files currently under `src/NodeMap.Core/Serialization/*`
- Create: `src/AsterGraph.Core/Compatibility/PortCompatibilityResult.cs`
- Create: `src/AsterGraph.Core/Compatibility/ImplicitConversionRule.cs`
- Create: `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`
- Modify: graph connection model to carry `ConversionId`

**Step 1: Rehome pure graph models**

Move the existing graph document, node, port, connection, and serialization logic into `AsterGraph.Core`.

**Step 2: Add compatibility result modeling**

Implement:

- `Exact`
- `ImplicitConversion`
- `Rejected`

as explicit result states.

**Step 3: Add a minimal safe implicit conversion registry**

Seed only:

- `int -> float`
- `int -> double`
- `float -> double`

**Step 4: Extend connections for conversion metadata**

Persist the selected `ConversionId` when a connection uses implicit conversion.

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Core/AsterGraph.Core.csproj
```

### Task 4: Move editor-state orchestration into `AsterGraph.Editor`

**Files:**
- Move/modify: `src/NodeMap.Avalonia/ViewModels/NodeViewModel.cs`
- Move/modify: `src/NodeMap.Avalonia/ViewModels/PortViewModel.cs`
- Move/modify: `src/NodeMap.Avalonia/ViewModels/ConnectionViewModel.cs`
- Move/modify: `src/NodeMap.Avalonia/ViewModels/NodeTemplateViewModel.cs`
- Move/modify: `src/NodeMap.Avalonia/ViewModels/NodeEditorViewModel.cs`
- Create: `src/AsterGraph.Editor/Catalog/NodeCatalog.cs`
- Create: `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
- Create: `src/AsterGraph.Editor/README.md` (optional if needed)

**Step 1: Move editor state out of the Avalonia project**

The editor-state layer should depend on `AsterGraph.Core` and `AsterGraph.Abstractions`, but not Avalonia.

**Step 2: Replace demo-specific template assumptions with definition-driven instances**

Ensure node creation comes from catalog-backed definitions rather than hard-coded template-only view models.

**Step 3: Route compatibility checks through the compatibility service**

Connection creation must ask the compatibility service whether a link is:

- exact
- implicitly convertible
- rejected

**Step 4: Keep inspector data graph-aware**

Preserve and improve:

- selected node details
- incoming/outgoing summary
- upstream/downstream related nodes

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj
```

### Task 5: Refactor `AsterGraph.Avalonia` into a pure UI shell over editor state

**Files:**
- Modify: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml`
- Modify: `src/NodeMap.Avalonia/Controls/NodeCanvas.axaml.cs`
- Modify: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml`
- Modify: `src/NodeMap.Avalonia/Controls/NodeEditorView.axaml.cs`
- Modify: `src/NodeMap.Avalonia/Themes/NodeMapTheme.axaml`
- Create: `src/AsterGraph.Avalonia/README.md`

**Step 1: Rename controls and update bindings**

Rename public controls to:

- `GraphEditorView`
- supporting canvas types under `AsterGraph.Avalonia`

**Step 2: Remove demo content assumptions**

The Avalonia layer should work with the editor contracts and editor state, not with demo-specific graph factories.

**Step 3: Preserve real-control-based anchors**

Keep the current real port-dot anchor approach, with succinct comments explaining why it exists.

**Step 4: Surface implicit conversion information in the UI**

At minimum, ensure conversion-backed edges can be displayed and inspected in a debuggable way.

**Step 5: Verify**

Run:

```powershell
dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj
```

### Task 6: Move sample algorithm content into `AsterGraph.Demo`

**Files:**
- Modify/move: current demo graph seed and demo factory
- Create: `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs`
- Modify: `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- Modify: `src/AsterGraph.Demo/Views/MainWindow.axaml`

**Step 1: Remove sample node definitions from core/editor**

The demo graph and demo node registrations should live only in the demo host.

**Step 2: Register demo node-definition providers**

Seed the demo by registering provider(s) into the editor catalog.

**Step 3: Keep the host thin**

`AsterGraph.Demo` should still only host the reusable editor UI and supply demo registrations.

### Task 7: Add root documentation and focused comments

**Files:**
- Create: `README.md`
- Modify: public interfaces and compatibility logic files
- Modify: serialization files
- Modify: anchor-resolution code in the Avalonia canvas

**Step 1: Add the root README**

Document:

- project purpose
- library layout
- quick start
- extension flow
- compatibility and implicit conversion policy
- roadmap

**Step 2: Add focused comments**

Only add comments that explain:

- extension boundaries
- safe conversion policy
- serialization contract stability
- why real rendered controls are used for port anchors

### Task 8: Final verification and smoke checks

**Files:**
- Modify: any files required for final polish after verification

**Step 1: Run full solution verification**

Run:

```powershell
dotnet build avalonia-node-map.sln
```

**Step 2: Run a compile-time extension smoke check**

Verify that the demo still loads node definitions through the catalog/provider path.

**Step 3: Run UI smoke checks**

Run the demo and verify:

- node library renders from registered definitions
- add node works
- selection works
- strict type checking still blocks incompatible links
- safe implicit conversion links are accepted and serialized
- save/load still works

**Step 4: Record residual gaps**

Document what remains deferred, especially:

- runtime plugin loading
- execution engine
- undo/redo redesign
