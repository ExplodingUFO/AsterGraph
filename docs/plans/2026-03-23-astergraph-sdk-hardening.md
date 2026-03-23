# AsterGraph SDK Hardening Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Strengthen AsterGraph as an embeddable SDK by adding menu-augmentor fault isolation, clearer host integration docs, targeted Chinese XML comments for key public APIs, and a smoke test that consumes packed packages.

**Architecture:** Keep behavior changes narrow. `GraphEditorViewModel.BuildContextMenu(...)` becomes the single fault-isolation boundary for host menu augmentation and falls back to stock menus on host errors. Public API docs stay focused on the host-facing surface already exposed in README, while `tools/AsterGraph.PackageSmoke` validates the shipped package graph rather than introducing a second sample host stack.

**Tech Stack:** .NET 8/9, Avalonia, CommunityToolkit.Mvvm, NuGet pack/smoke verification.

---

### Task 1: Document The Execution Scope

**Files:**
- Create: `docs/plans/2026-03-23-astergraph-sdk-hardening.md`
- Modify: none
- Test: none

**Step 1: Record the SDK-hardening scope**

- Keep this plan limited to:
  - augmentor exception isolation
  - README host integration guidance
  - targeted Chinese XML comments on current host-facing APIs
  - packed package smoke verification

**Step 2: Preserve context hygiene**

- Do not expand into new editor features.
- Do not touch unrelated legacy XML-comment warnings.

### Task 2: Add Augmentor Fault Isolation

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`

**Step 1: Isolate host augmentor failures**

- Wrap `ContextMenuAugmentor.Augment(this, context, stockItems)` in a `try/catch`.
- On failure:
  - keep the editor alive
  - return `stockItems`
  - set `StatusMessage` to a short host-facing diagnostic

**Step 2: Keep the boundary single-purpose**

- Do not move menu generation into Avalonia.
- Do not add a second augmentor abstraction.

### Task 3: Tighten Public API Comments

**Files:**
- Modify: `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`
- Modify: `src/AsterGraph.Editor/Configuration/GraphEditorCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/WorkspaceCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/HistoryCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/NodeCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/ConnectionCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/ClipboardCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/LayoutCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/FragmentCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/Configuration/HostCommandPermissions.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`

**Step 1: Add Chinese XML comments only to the active host-facing surface**

- Cover:
  - augmentor interface
  - command permission records and `ReadOnly`
  - current host-facing methods/properties/events in `GraphEditorViewModel`

**Step 2: Avoid repository-wide XML cleanup**

- Leave unrelated warning-heavy files alone.

### Task 4: Expand Host Integration README

**Files:**
- Modify: `README.md`

**Step 1: Add a practical host integration section**

- Show:
  - how to inject `IGraphContextMenuAugmentor`
  - how to apply `GraphEditorCommandPermissions`
  - how to persist node positions

**Step 2: Keep examples minimal**

- Use concise snippets that match the current API.

### Task 5: Update Package Smoke To Consume Current Packages

**Files:**
- Modify: `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`
- Modify: `tools/AsterGraph.PackageSmoke/Program.cs`

**Step 1: Point package smoke at the current package version**

- Replace stale preview package versions with the current shipped version property.

**Step 2: Exercise current host-facing package APIs**

- Reference:
  - `GraphEditorView`
  - `IGraphContextMenuAugmentor`
  - `GraphEditorCommandPermissions.ReadOnly`
  - `NodePositionSnapshot`

### Task 6: Verify And Ship

**Files:**
- Modify: `Directory.Build.props` if package version needs bump

**Step 1: Run verification**

- `dotnet build avalonia-node-map.sln`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true`
- `dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages`

**Step 2: Commit**

- Commit only the SDK-hardening files and the version bump if one is made.
