---
phase: 15-capability-and-descriptor-contract-normalization
plan: 01
subsystem: feature-descriptor-discovery
completed: 2026-04-08
---

# Phase 15 Plan 01 Summary

Added explicit runtime feature-descriptor discovery so hosts can inspect capabilities, services, and integrations without inferring support from facade/object shape.

Key changes:

- Added `GraphEditorFeatureDescriptorSnapshot` and `IGraphEditorQueries.GetFeatureDescriptors()` as the new canonical discovery read.
- Wired descriptor discovery through `GraphEditorSession`, `GraphEditorKernel`, and the retained `GraphEditorViewModelKernelAdapter` so factory and retained sessions expose the same runtime-level descriptor set.
- Kept `GraphEditorCapabilitySnapshot` intact as the compatibility boolean view while layering richer descriptors beside it.
- Extended `GraphEditorInspectionSnapshot` to carry the same feature descriptor list used by `Queries`, so diagnostics tooling sees one consistent discovery model.
- Added regression coverage for:
  - the public `IGraphEditorQueries` contract
  - public `GraphEditorFeatureDescriptorSnapshot` shape
  - runtime descriptor discovery with diagnostics sink and instrumentation enabled
  - descriptor parity across legacy and factory editor session paths
  - inspection snapshot parity with query descriptor discovery

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorDiagnosticsContractsTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`
  - exit 0
  - `40` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorDiagnosticsContractsTests|FullyQualifiedName~GraphEditorDiagnosticsInspectionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" -v minimal`
  - exit 0
  - `45` tests passed

Phase 15 status after this plan:

- Explicit runtime feature discovery now exists on the canonical session path.
- Inspection snapshots reuse the same descriptor surface instead of inventing a second discovery shape.
- `CAP-02` work remains open: command and menu contracts are still `ICommand` / MVVM-shaped and are now the next high-leverage target.
