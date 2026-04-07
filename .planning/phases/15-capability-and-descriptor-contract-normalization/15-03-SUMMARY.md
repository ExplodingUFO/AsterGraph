---
phase: 15-capability-and-descriptor-contract-normalization
plan: 03
subsystem: descriptor-proof-and-sample-lock
completed: 2026-04-08
---

# Phase 15 Plan 03 Summary

Closed Phase 15 by locking the descriptor-first control plane into proof tests, HostSample, PackageSmoke, and compatibility-facing docs.

Key changes:

- Extended proof coverage so migration/proof tests now assert:
  - legacy and factory editor sessions expose equivalent command descriptors
  - legacy and factory editor sessions expose equivalent canvas menu descriptors
  - proof-ring runtime coverage includes descriptor reads alongside existing session/diagnostics proof
- Updated `tools/AsterGraph.HostSample/Program.cs` to print human-readable Phase 15 descriptor markers:
  - `Descriptor runtime: ...`
  - `Descriptor retained: ...`
- Updated `tools/AsterGraph.PackageSmoke/Program.cs` to emit machine-checkable descriptor markers:
  - `COMMAND_DESCRIPTOR_OK:...`
  - `MENU_DESCRIPTOR_OK:...`
  - `RETAINED_MENU_ADAPTER_OK:...`
- Clarified compatibility posture in menu contracts by documenting `MenuItemDescriptor` and `GraphContextMenuAugmentationContext.StockItems` as compatibility-adapter surfaces over canonical descriptor generation.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorServiceSeamsTests" -v minimal`
  - exit 0
  - `42` tests passed
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - exit 0
  - printed descriptor runtime/retained markers
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
  - exit 0
  - printed `COMMAND_DESCRIPTOR_OK:True:True:True`
  - printed `MENU_DESCRIPTOR_OK:True:True:True`
  - printed `RETAINED_MENU_ADAPTER_OK:True:True`

Residual note:

- `GraphEditorTransactionTests.GraphEditorViewModel_HistoryInteraction_PreservesUndoAndDirtySemantics`
- `GraphEditorTransactionTests.GraphEditorViewModel_SaveBoundary_PreservesUndoRedoDirtySemantics`

still fail on both the current branch and the pre-15-02 baseline commit `9a5e5b9`. They were treated as pre-existing and are not evidence of a Phase 15 regression.

Phase 15 result after this plan:

- `CAP-01` and `CAP-02` are now satisfied.
- Canonical runtime discovery covers features, commands, and menus with stable descriptors.
- Retained menu rendering and augmentor flows now sit on explicit compatibility adapters rather than defining the control-plane shape.
