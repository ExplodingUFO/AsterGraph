## Summary

Plan 26-02 kept the MVVM-shaped compatibility projection explicitly at the retained `GraphEditorViewModel` facade edge and strengthened parity proof that retained hosts still sit on the same adapter-backed runtime session path. The runtime session remains kernel-owned, while the retained facade continues to map compatible-target snapshots back to the editor's live `NodeViewModel` and `PortViewModel` instances.

## Changes

- clarified in `GraphEditorViewModel` that `Session` is still exposed through the adapter-backed kernel path
- documented in the retained facade method that `GetCompatibleTargets(...)` is a retained-edge compatibility bridge over runtime snapshots
- added migration tests proving retained facade targets reuse live retained node/port instances while the session shim stays detached
- added proof-ring coverage that both legacy and factory-created retained editors still project compatible targets back to retained facade instances

## Verification

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests" --nologo -v minimal`
- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj --nologo -v minimal`
