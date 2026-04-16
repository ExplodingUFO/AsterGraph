---
phase: 33-kernel-canvas-and-guardrail-follow-through
researched: 2026-04-16
status: complete
---

# Phase 33 Research

## Focus

Phase 33 should finish the next hotspot follow-through after the retained facade contraction:

1. reduce the remaining host/router ownership burden in `GraphEditorKernel`
2. reduce the remaining lifecycle/composition burden in `NodeCanvas`
3. replace the repo-wide `CS1591` blanket with scoped debt handling

## Evidence Collected

### 1. `GraphEditorKernel` still carries direct command-router host ownership

- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` is still 427 lines.
- The type still directly implements `IGraphEditorKernelCommandRouterHost` and keeps seven explicit host members inline:
  - `BehaviorOptions`
  - `Document`
  - `SelectedNodeCount`
  - `PendingConnection`
  - `ViewportWidth`
  - `ViewportHeight`
  - `WorkspaceExists`
- The mutation and workspace paths already use dedicated host adapters:
  - `GraphEditorKernelNodeMutationHost`
  - `GraphEditorKernelConnectionMutationHost`
  - `GraphEditorKernelWorkspaceSaveCoordinatorHost`
  - `GraphEditorKernelWorkspaceLoadCoordinatorHost`

### 2. `NodeCanvas` still carries lifecycle/composition glue inline

- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` is still 351 lines.
- Interaction, scene, overlay, context-menu, and observation behavior is already delegated to focused collaborators.
- The remaining inline cross-cutting logic is:
  - constructor bootstrap for the coordinator bundle
  - `OnPropertyChanged(...)` routing for `ViewModelProperty` and `NodeVisualPresenterProperty`
  - visual-tree seam binding in `OnAttachedToVisualTree(...)` / `OnDetachedFromVisualTree(...)`
- Existing host adapters in `src/AsterGraph.Avalonia/Controls/Internal/Hosting/NodeCanvas.HostAdapters.cs` show the file is already using the preferred narrow-host pattern, but lifecycle routing has not yet been extracted.

### 3. XML-doc debt is not evenly distributed anymore

I ran these commands with repo-wide `CS1591` suppression effectively removed for the build:

```powershell
dotnet build src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release /p:NoWarn= /warnaserror:CS1591 -v minimal
dotnet build src/AsterGraph.Core/AsterGraph.Core.csproj -c Release /p:NoWarn= /warnaserror:CS1591 -v minimal
dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release /p:NoWarn= /warnaserror:CS1591 -v minimal
```

Results:

- `AsterGraph.Abstractions` built clean with `CS1591` treated as an error.
- `AsterGraph.Core` built clean with `CS1591` treated as an error.
- `AsterGraph.Editor` failed with 236 `CS1591` errors across runtime snapshots, events, automation outputs, and session members.

Implication:

- The current `Directory.Build.props` suppression is broader than the real debt boundary.
- Phase 33 can safely move away from repo-wide suppression and scope the remaining debt to the packages that still need it.
- `AsterGraph.Avalonia` should be revalidated after `AsterGraph.Editor` gets project-scoped suppression, because its build currently inherits the editor failure through project references.

## Conclusions

### Conclusion A

The next kernel reduction should target command-router host ownership, not node/connection/workspace mutation. Those mutation/workspace paths already sit behind host adapters; command routing is the remaining obvious inline host burden in `GraphEditorKernel.cs`.

### Conclusion B

The next `NodeCanvas` reduction should target lifecycle/composition routing, not drag/pointer math. Pointer/drag/wheel/context-menu logic already has dedicated coordinators and tests. The cross-cutting attach/property-change/bootstrap routing is the next highest-value seam.

### Conclusion C

`CS1591` cleanup should be implemented as scoped debt, not another blanket rule:

- remove the repo-wide suppression from `Directory.Build.props`
- keep suppression only where current public API debt still exists
- prove clean packages stay clean under `warnaserror:CS1591`

## Phase 33 Shape

Recommended three-plan execution:

1. extract `GraphEditorKernel` command-router host ownership into a dedicated internal host adapter and guard it with focused kernel tests
2. extract `NodeCanvas` lifecycle/bootstrap routing into a dedicated internal collaborator and guard it with focused lifecycle/standalone tests
3. scope XML-doc debt handling to real projects, then verify packable-project doc behavior explicitly
