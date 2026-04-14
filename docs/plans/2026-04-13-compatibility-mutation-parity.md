# Compatibility Mutation Parity Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Eliminate retained-facade vs kernel/session drift for compatibility-only node mutation commands, starting with duplicate and targeted delete flows.

**Architecture:** Add explicit guardrail tests that compare retained `GraphEditorViewModel` visible state with its session/kernel snapshots after compatibility-only node mutations. Then move mutation ownership back into kernel-backed flows so the compatibility helper becomes a routing layer instead of a second mutable engine.

**Tech Stack:** C#, .NET 8/9, xUnit, CommunityToolkit.Mvvm, existing AsterGraph editor/session abstractions

---

### Task 1: Add failing guardrail tests for compatibility-only node mutation parity

**Files:**
- Create/Test: `tests/AsterGraph.Editor.Tests/GraphEditorFacadeMutationParityTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorMutationCompatibilityTests.cs`

**Step 1: Write failing parity tests**

- Add tests that call retained-only compatibility entry points:
  - `GraphEditorViewModel.DuplicateNode(string nodeId)`
  - `GraphEditorViewModel.DeleteNodeById(string nodeId)`
- Assert the retained visible state and kernel/session-backed snapshots stay aligned after each operation:
  - `editor.Nodes` / `editor.FindNode(...)`
  - `editor.CreateDocumentSnapshot()`
  - `editor.Session.Queries.CreateDocumentSnapshot()`
  - selection / dirty state when relevant

**Step 2: Run targeted tests to confirm the drift exists**

Run:

```bash
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorFacadeMutationParityTests|FullyQualifiedName~GraphEditorMutationCompatibilityTests" -v minimal
```

Expected:
- New parity tests fail before implementation because compatibility commands still mutate facade collections directly.

**Step 3: Keep the tests focused on public behavior**

- Do not assert internal call paths.
- Lock observable parity only.

**Step 4: Commit**

```bash
git add tests/AsterGraph.Editor.Tests/GraphEditorFacadeMutationParityTests.cs tests/AsterGraph.Editor.Tests/GraphEditorMutationCompatibilityTests.cs
git commit -m "test: lock compatibility mutation parity"
```

---

### Task 2: Route duplicate and targeted delete back through kernel-owned mutations

**Files:**
- Modify: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`
- Modify: `src/AsterGraph.Editor/Kernel/Internal/GraphEditorKernelDocumentMutator.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorFacadeMutationParityTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorMutationCompatibilityTests.cs`

**Step 1: Add kernel-backed mutation entry points**

- Introduce minimal kernel operations for:
  - duplicate a node by id
  - delete a single node by id
- Keep permission checks, selection updates, dirty/history, and document-changed events owned by kernel.

**Step 2: Push document-shape work into the existing document mutator helper**

- Extend `GraphEditorKernelDocumentMutator` with the pure document transformations needed for:
  - single-node deletion with affected connection removal
  - node duplication with new id and offset position
- Keep id generation / status wording / selection policy in `GraphEditorKernel`, not in the mutator helper.

**Step 3: Thin the compatibility helper**

- Change `GraphEditorCompatibilityCommands` so:
  - `DeleteNodeById(...)` routes into kernel-backed deletion
  - `DuplicateNode(...)` routes into kernel-backed duplication
- Preserve current public `GraphEditorViewModel` methods and command ids.

**Step 4: Run targeted parity verification**

Run:

```bash
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorFacadeMutationParityTests|FullyQualifiedName~GraphEditorMutationCompatibilityTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests" -v minimal
```

Expected:
- All targeted parity and session/history tests pass.

**Step 5: Run full regression**

Run:

```bash
dotnet test avalonia-node-map.sln -v minimal
```

Expected:
- Full suite passes.

**Step 6: Commit**

```bash
git add src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs src/AsterGraph.Editor/Kernel/Internal/GraphEditorKernelDocumentMutator.cs src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs tests/AsterGraph.Editor.Tests/GraphEditorFacadeMutationParityTests.cs tests/AsterGraph.Editor.Tests/GraphEditorMutationCompatibilityTests.cs
git commit -m "refactor: route compatibility mutations through kernel"
```

---

## Execution Order

1. Task 1 first, so drift is documented before changing behavior.
2. Task 2 second, keeping the write scope centered on kernel + compatibility helper.

## Not In Scope

- Fragment / clipboard flows just extracted in Task 4
- New public commands or command ids
- Large session/runtime API redesign
- `NodeCanvas` or Avalonia interaction changes

## Expected Outcome

- Compatibility-only duplicate/delete flows no longer maintain a second mutable source of truth.
- Retained `GraphEditorViewModel` UI state stays aligned with kernel/session snapshots.
- The next remaining God Code hotspot becomes structural coupling, not hidden mutation divergence.
