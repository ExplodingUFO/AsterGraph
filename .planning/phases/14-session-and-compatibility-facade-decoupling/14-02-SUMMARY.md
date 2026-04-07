---
phase: 14-session-and-compatibility-facade-decoupling
plan: 02
subsystem: canonical-query-boundary
completed: 2026-04-08
---

# Phase 14 Plan 02 Summary

Tightened the canonical query boundary so DTO/snapshot reads remain the default runtime model while the MVVM-shaped compatibility query path is now explicitly framed as a retained shim.

Key changes:

- Marked `IGraphEditorQueries.GetCompatibleTargets(string, string)` as an explicit compatibility-only shim via `[Obsolete(...)]` in `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs`.
- Marked `CompatiblePortTarget` as an explicit compatibility-only shim via `[Obsolete(...)]` in `src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs`.
- Tightened the XML remarks on both the interface method and the compatibility type so the canonical host path is clearly `GetCompatiblePortTargets(...)`.
- Added focused regression tests in `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` that assert:
  - `IGraphEditorQueries.GetCompatibleTargets(...)` is marked as compatibility-only
  - `CompatiblePortTarget` itself is marked as compatibility-only
- Added local `#pragma warning disable/restore CS0618` scopes around intentional compatibility-shim declarations and uses so the build/test output stays clean while the compatibility warnings remain meaningful at the public boundary.

Compatibility boundary after this plan:

- Canonical runtime-safe compatible-target discovery remains `GraphEditorCompatiblePortTargetSnapshot` through `IGraphEditorQueries.GetCompatiblePortTargets(...)`.
- `CompatiblePortTarget` remains available for retained MVVM integrations and editor/menu compatibility seams, but it is now explicitly signposted as non-canonical.

Verification run:

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests" -v minimal`
  - exit 0
  - `21` tests passed
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorDiagnosticsInspectionTests" -v minimal`
  - exit 0
  - `26` tests passed

Important boundary after this plan:

- The runtime query surface now better communicates canonical vs compatibility usage, but the underlying MVVM-shaped compatibility path still exists for retained hosts and editor-menu composition.
- Phase 14-03 remains responsible for locking this story into migration/proof-ring/sample output.
