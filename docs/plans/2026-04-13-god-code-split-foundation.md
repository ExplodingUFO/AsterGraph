# AsterGraph God Code Split Foundation

**Goal:** Reduce the highest-risk God Code hotspots without changing the public API surface. Add guardrail tests around the current compatibility/runtime behavior, continue the kernel-first split inside `AsterGraph.Editor`, and move `GraphEditorViewModel` toward a compatibility facade instead of a second editor engine.

**Architecture:** Keep `GraphEditorViewModel`, `GraphEditorSession`, and `NodeCanvas` as the stable entry points for now. This wave only extracts internal collaborators and moves remaining mutation ownership toward `GraphEditorKernel`. The key constraint is avoiding a second mutable source of truth inside `GraphEditorViewModel`.

**Baseline:** `dotnet test avalonia-node-map.sln -v minimal` passes on `phase/35-god-code-split-foundation`.

---

## Task 1: Add guardrail tests for facade/runtime parity and mutation behavior

**Why first:** The current retained façade is still in migration. Before splitting large classes, lock the externally visible behavior that must not drift.

**Files:**
- Create/Test: `tests/AsterGraph.Editor.Tests/GraphEditorMutationCompatibilityTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`

**Scope:**
- Add focused tests for façade/runtime parity around selection, node/connection mutations, dirty/history behavior, and retained-host compatibility.
- Prefer new dedicated test files over broad rewrites of the existing large suites.
- Test public API results, events, and snapshots; avoid asserting internal call sequences.

**Verification:**
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMutationCompatibilityTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`

---

## Task 2: Split `GraphEditorKernel` into internal collaborators while preserving ownership

**Why parallel:** `GraphEditorKernel` is already the runtime state owner. This wave should keep that ownership but move dense document/history/query logic behind internal helpers.

**Files:**
- Modify: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`
- Create: `src/AsterGraph.Editor/Kernel/Internal/GraphEditorKernelDocumentMutator.cs`
- Create: `src/AsterGraph.Editor/Kernel/Internal/GraphEditorKernelViewportCoordinator.cs`
- Create: `src/AsterGraph.Editor/Kernel/Internal/GraphEditorKernelCompatibilityQueries.cs`
- Create/Test: `tests/AsterGraph.Editor.Tests/GraphEditorKernelBehaviorTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`

**Scope:**
- Move document mutations, viewport math/orchestration, and compatibility-target query logic behind internal helpers.
- Keep `GraphEditorKernel` as the single runtime state owner and the event/status orchestration boundary.
- Do not expand this wave into `GraphEditorSession` or public runtime interfaces.

**Verification:**
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorKernelBehaviorTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests" -v minimal`

---

## Task 3: Extract `GraphEditorViewModel` document and selection projection collaborators

**Why parallel:** This stays in the façade/projection layer and can run beside the kernel split as long as no second task touches `GraphEditorViewModel.cs`.

**Files:**
- Create: `src/AsterGraph.Editor/Services/GraphEditorDocumentProjectionApplier.cs`
- Create: `src/AsterGraph.Editor/Services/GraphEditorSelectionProjection.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`
- Create/Test: `tests/AsterGraph.Editor.Tests/GraphEditorViewModelProjectionTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`

**Scope:**
- Move `ApplyKernel*`, `LoadDocument(...)`, node/connection index rebuild, selection-derived text, and selected-parameter projection behind internal collaborators.
- Keep all public properties, commands, and constructor signatures stable.
- Do not start the clipboard/template/context-menu compatibility-command split in this task.

**Verification:**
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~GraphEditorViewModelProjectionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`

---

## Task 4: Follow-up compatibility-command split inside `GraphEditorViewModel`

**Why later:** This still writes the same façade file as Task 3. It should only start after the projection/sync layer has stabilized.

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Create: `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorCompatibilityCommands.cs`
- Create: `src/AsterGraph.Editor/ViewModels/Internal/GraphEditorFragmentCommands.cs`
- Test: `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphContextMenuBuilderTests.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs`

**Scope:**
- Extract clipboard/fragment/template operations, compatibility-only node commands, and menu-related compatibility orchestration into internal helpers.
- Keep direct `GraphEditorViewModel` methods available, but reduce them to thin routing methods.

**Verification:**
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphContextMenuBuilderTests|FullyQualifiedName~GraphEditorLocalizationTests" -v minimal`

---

## Execution Order

1. Task 1 first, to establish behavioral guardrails before structural edits.
2. Task 2 and Task 3 can run in parallel because they have disjoint write sets (`Kernel/` vs `ViewModels/Services`).
3. Task 4 starts only after Task 3 lands, because it rewrites the same façade file.

## Not In Scope For This Wave

- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- `src/AsterGraph.Editor/Plugins/Internal/AsterGraphPluginLoader.cs`
- Public API renames, removals, or constructor signature changes
- Clipboard/template behavior changes beyond wiring fallout from refactors

## Expected Outcome

- New guardrail tests protect migration-sensitive behavior across retained façade and runtime/session paths.
- `GraphEditorKernel` keeps sole runtime ownership while shrinking internally into focused collaborators.
- `GraphEditorViewModel` loses more of its projection/synchronization mass and trends toward a projection/compatibility façade.
