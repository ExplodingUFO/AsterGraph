---
phase: 15-capability-and-descriptor-contract-normalization
plan: 02
subsystem: command-and-menu-descriptors
completed: 2026-04-08
---

# Phase 15 Plan 02 Summary

Normalized the canonical command and menu control plane around stable descriptor and invocation records instead of `ICommand` / `RelayCommand` / VM projection types.

Key changes:

- Added canonical runtime command/menu contract types:
  - `GraphEditorCommandArgumentSnapshot`
  - `GraphEditorCommandInvocationSnapshot`
  - `GraphEditorCommandDescriptorSnapshot`
  - `GraphEditorMenuItemDescriptorSnapshot`
- Extended `IGraphEditorCommands` with `TryExecuteCommand(...)` so descriptor-driven hosts can execute stable command IDs without binding to MVVM command objects.
- Extended `IGraphEditorQueries` with:
  - `GetCommandDescriptors()`
  - `BuildContextMenuDescriptors(ContextMenuContext)`
- Implemented runtime command descriptors and descriptor-driven menu generation in `GraphEditorSession`, with runtime command execution rooted in `IGraphEditorSessionHost`.
- Implemented runtime host command descriptor/dispatch support in `GraphEditorKernel`, and compatibility-only command extensions in `GraphEditorViewModelKernelAdapter`.
- Added `GraphContextMenuCompatibilityAdapter` and switched retained `GraphEditorViewModel.BuildContextMenu(...)` to consume canonical menu descriptors and adapt them back into `MenuItemDescriptor` for compatibility rendering/augmentor flows.
- Extended `GraphContextMenuAugmentationContext` to include canonical `StockItemDescriptors` alongside compatibility `StockItems`.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorDiagnosticsContractsTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
  - exit 0
  - `46` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphContextMenuBuilderTests|FullyQualifiedName~GraphEditorLocalizationTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
  - exit 0
  - `20` tests passed

Additional investigation:

- `GraphEditorTransactionTests.GraphEditorViewModel_HistoryInteraction_PreservesUndoAndDirtySemantics`
- `GraphEditorTransactionTests.GraphEditorViewModel_SaveBoundary_PreservesUndoRedoDirtySemantics`

still fail on both current `HEAD` and the pre-15-02 baseline commit `9a5e5b9`, so they were treated as pre-existing and not caused by this plan.

Phase 15 status after this plan:

- Canonical command/menu descriptors now exist on the session/query surface.
- Retained `BuildContextMenu(...)` is now descriptor-backed through a compatibility adapter.
- `15-03` remains to lock the descriptor-first story into proof/sample/smoke outputs and clearer compatibility annotations.
