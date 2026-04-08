---
phase: 18-plugin-and-automation-readiness-proof-ring
plan: 01
subsystem: readiness-descriptors-and-contract-proof
completed: 2026-04-08
---

# Phase 18 Plan 01 Summary

Made plugin/automation readiness seams explicit at the canonical runtime boundary by extending feature descriptors and locking those descriptors with focused contract/service/proof tests.

Key changes:

- Extended `GraphEditorSessionDescriptorSupport` so runtime sessions can advertise optional seam availability instead of leaving that information implicit in `AsterGraphEditorOptions`.
- Extended `GraphEditorSession.GetFeatureDescriptors()` with explicit readiness descriptors for:
  - fragment workspace
  - fragment library
  - clipboard payload serialization
  - context-menu augmentation
  - node presentation
  - localization
  - diagnostics sink
  - instrumentation logger and activity source
- Updated factory and retained-session descriptor support creation so `CreateSession(...)` and `GraphEditorViewModel.Session` expose the same canonical readiness descriptor set when configured with the same seam surface.
- Extended focused tests in `GraphEditorSessionTests` and `GraphEditorServiceSeamsTests` so missing readiness descriptors now fail loudly.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorDiagnosticsContractsTests|FullyQualifiedName~GraphEditorServiceSeamsTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
  - exit 0
  - `43` tests passed

Phase 18 status after this plan:

- `PLUG-READY-01` is materially advanced: readiness seams are now explicit at the canonical runtime boundary instead of being discoverable only by reading implementation code.
- `18-02` is next: surface the same readiness story in HostSample, PackageSmoke, and ScaleSmoke.
