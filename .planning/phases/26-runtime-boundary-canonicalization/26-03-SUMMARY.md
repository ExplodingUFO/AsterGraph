## Summary

Plan 26-03 tightened the migration guidance around the remaining compatibility-only compatible-target APIs so they are hard to mistake for the canonical runtime path. The runtime query, retained shim type, retained facade, README, and focused tests now all describe the same staged retirement posture for v1.5 and beyond.

## Changes

- updated `IGraphEditorQueries.GetCompatibleTargets(...)` remarks and obsolete text to point to `GetCompatiblePortTargets(...)` as the canonical runtime query
- updated `CompatiblePortTarget` remarks and obsolete text to describe it as a retained compatibility shim with a staged retirement path
- clarified in `GraphEditorViewModel` remarks that the retained constructor path is a compatibility facade, not the canonical runtime composition root
- added a migration section to `src/AsterGraph.Editor/README.md` covering the v1.5 shim posture, stronger future minor warnings, and possible future major removal
- strengthened focused tests to assert the actual obsolete guidance text instead of only checking attribute presence

## Verification

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorMigrationCompatibilityTests" --nologo -v minimal`
- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj --nologo -v minimal`
