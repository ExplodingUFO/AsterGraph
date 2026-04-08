---
phase: 17-compatibility-lock-and-migration-proof
plan: 01
subsystem: migration-parity-regression-matrix
completed: 2026-04-08
---

# Phase 17 Plan 01 Summary

Strengthened the migration-proof regression matrix so retained constructor/view paths are compared against the canonical factory/session/factory-view paths through clearer shared signatures instead of only scattered one-off assertions.

Key changes:

- Extended `GraphEditorMigrationCompatibilityTests` with full-shell and standalone surface snapshots so direct `GraphEditorView`, factory full shell, and standalone canvas routes now prove equivalent migration-facing behavior across legacy and factory editors.
- Extended `GraphEditorViewTests` so direct `GraphEditorView` with a factory-created editor is explicitly covered as a supported compatibility surface rather than an implicit side effect.
- Extended `GraphEditorSessionTests` and `GraphEditorProofRingTests` with shared retained-vs-runtime session signatures:
  - the canonical `CreateSession(...)` path remains `GraphEditorViewModel`-free
  - the retained `editor.Session` path remains adapter-backed
  - both routes expose the same shared canonical descriptor/menu subset
  - the retained path still carries extra compatibility commands such as `nodes.duplicate`

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
  - exit 0
  - `54` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorMigrationCompatibilityTests|FullyQualifiedName~GraphEditorProofRingTests|FullyQualifiedName~GraphEditorInitializationTests|FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~NodeCanvasStandaloneTests|FullyQualifiedName~GraphEditorSessionTests" -v minimal`
  - exit 0
  - `76` tests passed

Phase 17 status after this plan:

- `MIG-01` is materially advanced: retained constructor/view routes are more explicitly locked as supported migration behavior.
- `MIG-02` is materially advanced: the proof matrix now distinguishes shared canonical parity from retained-only compatibility surface area instead of assuming total route equality.
- `17-02` is next: align API remarks and host-facing docs with the migration story the tests now prove.
