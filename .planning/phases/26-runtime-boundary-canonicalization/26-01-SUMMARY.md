## Summary

Plan 26-01 removed the legacy `GetCompatibleTargets(...)` shim from the internal runtime host path so the kernel, compatibility query helper, and retained-session adapter now expose only the canonical snapshot query. The public obsolete runtime query still exists, but it is now implemented inside `GraphEditorSessionQueries` as a compatibility bridge layered on top of `GetCompatiblePortTargets(...)` plus a document snapshot lookup.

## Changes

- removed `GetCompatibleTargets(...)` from `IGraphEditorSessionHost`
- removed the internal shim implementation from `GraphEditorKernel`
- removed the legacy shim helper from `GraphEditorKernelCompatibilityQueries`
- removed the legacy shim forwarding method from `GraphEditorViewModelKernelAdapter`
- reimplemented `GraphEditorSessionQueries.GetCompatibleTargets(...)` as a snapshot-backed compatibility bridge
- added focused regression tests for the cleaned runtime boundary and the public compatibility bridge

## Verification

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests" --nologo -v minimal`
- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj --nologo -v minimal`
