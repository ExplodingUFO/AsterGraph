# Phase 14-01 Kernel Adapter Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rebuild `GraphEditorViewModel` so its retained `Session` path is kernel-backed and adapter-driven instead of treating the VM as the runtime state owner.

**Architecture:** Keep `GraphEditorKernel` as the canonical `IGraphEditorSessionHost` for both `CreateSession(...)` and retained `GraphEditorViewModel.Session`. Add a VM-side projection helper that mirrors kernel-owned snapshots/events into existing `ObservableCollection` and inspector state, while preserving the current public `GraphEditorViewModel` constructor and VM-facing API shape.

**Tech Stack:** C#, .NET 8/9, xUnit, Avalonia-compatible MVVM patterns already used in `AsterGraph.Editor`.

---

### Task 1: Lock The Desired Behavior With Focused Failing Tests

**Files:**
- Modify: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- Modify: `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`

**Step 1: Add a retained-facade ownership regression test**

Add a test near the existing kernel-owner assertions in `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`:

```csharp
[Fact]
public void AsterGraphEditorFactory_Create_EditorSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner()
{
    var editor = AsterGraphEditorFactory.Create(CreateOptions(new NodeDefinitionId("tests.session.editor-kernel-owner")));
    var session = editor.Session;

    Assert.DoesNotContain(
        session.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
        field => field.FieldType == typeof(GraphEditorViewModel));
}
```

**Step 2: Add a migration-path parity test**

Add a test in `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` proving the retained `editor.Session` path is still behaviorally aligned after adapterization:

```csharp
[Fact]
public void LegacyAndFactoryEditorSessions_RemainBehaviorallyAlignedAfterAdapterBackedSessionCommands()
{
    using var harness = CreateHarness();
    var legacyEditor = CreateLegacyEditor(harness);
    var factoryEditor = CreateFactoryEditor(harness);

    legacyEditor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
    factoryEditor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);

    Assert.Equal(
        legacyEditor.Session.Queries.GetSelectionSnapshot().SelectedNodeIds,
        factoryEditor.Session.Queries.GetSelectionSnapshot().SelectedNodeIds);
    Assert.Equal(legacyEditor.SelectedNodes.Count, factoryEditor.SelectedNodes.Count);
}
```

**Step 3: Run the focused tests and confirm they fail for the right reason**

Run:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal
```

Expected before implementation:
- the new `Create(...).Session` ownership test fails because `GraphEditorSession` still stores/constructs around `GraphEditorViewModel`
- parity assertions may fail once the first test is in place or after the next task wires the adapter partially

**Step 4: Commit the red tests**

```powershell
git add tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
git commit -m "test: lock phase 14 adapter-backed session behavior"
```

### Task 2: Introduce A Dedicated VM Projection Helper Around The Kernel

**Files:**
- Create: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Reference: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`

**Step 1: Create the new internal adapter type**

Create `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs` with a narrow responsibility:

```csharp
namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorViewModelKernelAdapter : IDisposable
{
    private readonly GraphEditorKernel _kernel;
    private readonly GraphEditorViewModel _owner;

    public GraphEditorViewModelKernelAdapter(GraphEditorKernel kernel, GraphEditorViewModel owner)
    {
        _kernel = kernel;
        _owner = owner;
    }

    public void Initialize()
    {
        ApplyDocumentSnapshot();
        ApplySelectionSnapshot();
        ApplyViewportSnapshot();
        ApplyPendingConnectionSnapshot();
        ApplyStatus();
        Attach();
    }

    public void Dispose()
    {
        Detach();
    }

    // Attach(), Detach(), ApplyDocumentSnapshot(), ApplySelectionSnapshot(),
    // ApplyViewportSnapshot(), ApplyPendingConnectionSnapshot(), ApplyStatus()
}
```

**Step 2: Subscribe the adapter to kernel-owned host events**

Use the kernel's existing `IGraphEditorSessionHost` event surface to drive projection refresh:
- `DocumentChanged` -> rebuild/reconcile nodes and connections from `CreateDocumentSnapshot()`
- `SelectionChanged` -> refresh `SelectedNodes`, `SelectedNode`, inspector projection
- `ViewportChanged` -> refresh `Zoom`, `PanX`, `PanY`, `ViewportWidth`, `ViewportHeight`
- `PendingConnectionChanged` -> refresh `PendingSourceNode` / `PendingSourcePort`
- `FragmentExported`, `FragmentImported`, `RecoverableFailureRaised`, `DiagnosticPublished` -> relay to the existing VM event/status surface

Do not make this type a second runtime host. It is a one-way projection layer from kernel state into VM state.

**Step 3: Add internal apply helpers on the VM**

Add narrowly scoped internal methods on `GraphEditorViewModel` for the adapter to call, for example:

```csharp
internal void ApplyKernelDocument(GraphDocument snapshot, bool markClean) { ... }
internal void ApplyKernelSelection(GraphEditorSelectionSnapshot snapshot) { ... }
internal void ApplyKernelViewport(GraphEditorViewportSnapshot snapshot) { ... }
internal void ApplyKernelPendingConnection(GraphEditorPendingConnectionSnapshot snapshot) { ... }
internal void ApplyKernelStatus(string statusMessage) { ... }
```

Each helper must:
- avoid recursively generating history/dirty events while applying kernel-owned state
- preserve existing `ObservableCollection` instances
- keep inspector/cache recomputation in one place

**Step 4: Run compile-orienting tests**

Run:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal
```

Expected:
- build may still fail until Task 3 rewires VM construction
- if it runs, the new ownership test should still be red

**Step 5: Commit the scaffolding**

```powershell
git add src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
git commit -m "refactor: add graph editor vm kernel adapter scaffolding"
```

### Task 3: Rewire GraphEditorViewModel To Compose The Kernel Instead Of Acting As The Host

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:40`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:143-262`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:1296`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:1705-1796`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:2706-2977`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:3421-3549`
- Modify: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs` only if a tiny internal hook is truly required

**Step 1: Remove `IGraphEditorSessionHost` from the class declaration**

Change:

```csharp
public sealed partial class GraphEditorViewModel : ObservableObject, IGraphContextMenuHost, IGraphEditorSessionHost
```

to:

```csharp
public sealed partial class GraphEditorViewModel : ObservableObject, IGraphContextMenuHost
```

**Step 2: Add private kernel-owned fields**

Add fields such as:

```csharp
private readonly GraphEditorKernel _kernel;
private readonly GraphEditorViewModelKernelAdapter _kernelAdapter;
```

**Step 3: Rebuild constructor composition**

Inside the constructor:
- keep service resolution exactly as it is now
- create `Nodes`, `Connections`, `SelectedNodes`, `SelectedNodeParameters`, `NodeTemplates`, `FragmentTemplates`
- construct `_kernel` from the same runtime inputs currently used by `CreateSession(...)`
- construct `Session` with the kernel:

```csharp
Session = new GraphEditorSession(_kernel, diagnosticsSink);
```

- create `_kernelAdapter`
- replace the direct `LoadDocument(document, ...)` bootstrapping call with `_kernelAdapter.Initialize()`

Important: do not remove the public constructor or change its signature.

**Step 4: Convert state-owner methods into facade delegations**

For methods that currently own canonical runtime state, make the VM delegate to kernel/session-backed operations instead of mutating VM collections directly.

Prioritize these groups first because they map directly to `IGraphEditorSessionHost` responsibilities:
- selection: `SetSelection(...)`, `ClearSelection(...)`
- node/graph changes: add/delete nodes, connection lifecycle, delete connection, break connections
- viewport: `PanBy`, `ZoomAt`, `UpdateViewportSize`, `ResetView`, `FitToViewport`, `CenterViewOnNode`, `CenterViewAt`
- persistence/history: `Undo`, `Redo`, `SaveWorkspace`, `LoadWorkspace`
- pending connection state

Rule: when a method changes canonical graph/runtime state, the VM should request that change through the kernel and then let the adapter update the projection.

**Step 5: Delete the explicit host-implementation block**

Remove the explicit `IGraphEditorSessionHost` block around `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs:3421-3549`.

Do not replace it with another VM-owned host implementation. The kernel is already the host.

**Step 6: Keep UI-only concerns in the VM**

Do not migrate these in this task:
- `IGraphContextMenuHost` implementation
- localization lookups
- presentation-provider application
- clipboard bridge wiring
- fragment template list UI
- command objects and Avalonia-facing convenience members

**Step 7: Run the focused Phase 14-01 verification**

Run:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal
```

Expected:
- the new `editor.Session` ownership test passes
- pre-existing tests proving session commands still drive VM-visible selection, pending connection, and inspector state continue to pass

**Step 8: Commit the kernel-backed VM conversion**

```powershell
git add src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
git commit -m "refactor: make graph editor view model kernel-backed"
```

### Task 4: Tighten The Tests Around Projection Safety

**Files:**
- Modify: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- Optional modify: `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`

**Step 1: Strengthen the existing retained-session regression**

Expand the current retained-path tests so they do all of the following after adapterization:

```csharp
var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
var session = editor.Session;

session.Commands.StartConnection(SourceNodeId, SourcePortId);
Assert.True(editor.HasPendingConnection);

session.Commands.CancelPendingConnection();
Assert.False(editor.HasPendingConnection);

session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
Assert.Single(editor.SelectedNodes);
```

**Step 2: Verify direct VM operations still surface through the shared runtime**

Add or extend a test that mixes direct VM calls with runtime reads:

```csharp
editor.SelectSingleNode(editor.FindNode(SourceNodeId), updateStatus: false);
var selection = editor.Session.Queries.GetSelectionSnapshot();
Assert.Equal(SourceNodeId, selection.PrimarySelectedNodeId);
```

This catches projection drift in both directions.

**Step 3: Run the same focused test command again**

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal
```

Expected: green.

**Step 4: Commit the tightened coverage**

```powershell
git add tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs
git commit -m "test: harden phase 14 kernel adapter regressions"
```

### Task 5: Prepare The Handoff To Phase 14-02

**Files:**
- Review only: `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`
- Review only: `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`
- Review only: `.planning/phases/14-session-and-compatibility-facade-decoupling/14-02-PLAN.md`

**Step 1: Confirm Phase 14-01 stopping point**

Before moving on, verify all of these are true:
- `GraphEditorViewModel` no longer implements `IGraphEditorSessionHost`
- `editor.Session` is kernel-backed rather than VM-backed
- VM collections and inspector/pending-connection state still track kernel mutations
- no public constructor or public VM member was removed

**Step 2: Do not prematurely normalize MVVM-shaped query seams**

Specifically defer:
- changing public `CompatiblePortTarget` shape
- removing compatibility query APIs
- broad documentation changes to canonical vs compatibility query docs

Those belong to Phase `14-02`.

**Step 3: Optional checkpoint commit tag**

```powershell
git status --short
```

Expected: clean worktree before starting `14-02`.

---

## Suggested Internal Type Names

- Preferred new helper: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs`
- Acceptable alternate if responsibilities stay tiny: nested private type inside `GraphEditorViewModel.cs`

Use `GraphEditorViewModelKernelAdapter` unless a nested type is obviously smaller and clearer.

## Explicit Non-Goals For 14-01

- Do not remove the public `GraphEditorViewModel` constructor.
- Do not thin the Avalonia layer.
- Do not redesign `CompatiblePortTarget`.
- Do not remove obsolete compatibility APIs yet.
- Do not try to finish proof-ring/sample output updates from `14-03`.

## Verification Summary

Primary command:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal
```

Stretch command after green:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorDiagnosticsInspectionTests" -v minimal
```

