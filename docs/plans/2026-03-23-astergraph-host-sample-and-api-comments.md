# AsterGraph Host Sample And API Comments Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a realistic minimal host sample and continue targeted Chinese XML comment coverage for the active host-facing SDK surface.

**Architecture:** Keep the scope narrow. The sample host should demonstrate real host composition through `NodeCatalog`, `GraphEditorViewModel`, `GraphEditorCommandPermissions`, and `IGraphContextMenuAugmentor`, while remaining a simple console-based integration sample under `tools/`. XML comments should focus on the currently active public surface used by hosts rather than attempting a repo-wide warning cleanup.

**Tech Stack:** .NET 8/9, Avalonia packages, CommunityToolkit.Mvvm, AsterGraph project references and packed-package smoke validation.

---

### Task 1: Add A Minimal Host Sample

**Files:**
- Create: `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- Create: `tools/AsterGraph.HostSample/Program.cs`
- Modify: `avalonia-node-map.sln`

**Step 1: Create a console host sample**

- Reference:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`

**Step 2: Demonstrate real host composition**

- In `Program.cs`, show:
  - a custom `INodeDefinitionProvider`
  - a custom `IGraphContextMenuAugmentor`
  - `GraphEditorCommandPermissions`
  - `GraphEditorBehaviorOptions`
  - event subscriptions
  - position persistence APIs
  - a sample `BuildContextMenu(...)` call

**Step 3: Add the project to the solution**

- Ensure `dotnet build avalonia-node-map.sln` includes the sample.

### Task 2: Expand Targeted Public XML Comments

**Files:**
- Modify: `src/AsterGraph.Editor/Catalog/NodeCatalog.cs`
- Modify: `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- Modify: `src/AsterGraph.Editor/Events/GraphEditorDocumentChangeKind.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`

**Step 1: Comment the host-facing support services**

- Add concise Chinese XML comments for:
  - catalog entry points
  - workspace/fragment services
  - default compatibility service
  - event kind enum values

**Step 2: Comment the highest-value `GraphEditorViewModel` host surface**

- Focus on the members hosts are expected to call directly:
  - key properties
  - key events
  - menu/behavior/position APIs
  - import/export and workspace APIs

### Task 3: Update Documentation

**Files:**
- Modify: `README.md`

**Step 1: Reference the new sample host**

- Add a short note pointing hosts to `tools/AsterGraph.HostSample`.

**Step 2: Keep README aligned with current API**

- Ensure augmentor, permissions, events, and position persistence sections still match the sample.

### Task 4: Verify And Ship

**Files:**
- Modify: `Directory.Build.props` only if version bump is needed

**Step 1: Run verification**

- `dotnet build avalonia-node-map.sln`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true`

**Step 2: Commit**

- Commit only the sample-host, targeted comments, and README/solution changes.
