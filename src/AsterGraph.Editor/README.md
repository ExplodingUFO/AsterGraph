# AsterGraph.Editor

`AsterGraph.Editor` is the canonical host-facing runtime package for AsterGraph.

It belongs to the supported published package set with `AsterGraph.Abstractions`, `AsterGraph.Core`, and `AsterGraph.Avalonia`, and it targets `net8.0` and `net9.0`.

## Reference This Package When

- the host owns its own UI and wants the runtime/session boundary directly
- the host needs `IGraphEditorSession`, session commands/queries/events, mutation batching, or runtime diagnostics
- the host wants plugin discovery, trust policy, load inspection, automation execution, localization, or presentation seams
- the host is still migrating through the retained `GraphEditorViewModel` compatibility surface

## This Package Owns

- `IGraphEditorSession` plus `Commands`, `Queries`, `Events`, and mutation batching
- definition-driven parameter snapshots, validation-aware inspector data, and batch parameter editing
- node-surface queries and mutations for persisted node size, resolved width/height tiers, fixed node-group frames, geometry-based membership, and editor-only node groups
- `AsterGraphEditorFactory` and `AsterGraphEditorOptions`
- replaceable storage, clipboard, diagnostics, localization, menu, presentation, and inline-editor registry seams
- plugin discovery, trust policy, load inspection, and automation entry points
- retained migration facades such as `GraphEditorViewModel`

## This Package Does Not Own

- Avalonia visual controls
- demo content
- host-specific business commands

Those responsibilities live in `AsterGraph.Avalonia` or the consuming host.

## Stability Guidance

- stable canonical surfaces:
  - `AsterGraphEditorFactory.CreateSession(...)`
  - `IGraphEditorSession`
- hosted-UI composition helper:
  - `AsterGraphEditorFactory.Create(...)`
- DTO/snapshot queries such as `GetCompatiblePortTargets(...)`, `GetNodeSurfaceSnapshots()`, `GetHierarchyStateSnapshot()`, `GetNodeGroups()`, and `GetNodeGroupSnapshots()`
- node/group mutations such as `TrySetNodeSize(...)`, `TrySetNodeGroupPosition(...)`, `TrySetNodeGroupSize(...)`, and `TrySetNodeGroupMemberships(...)`
- retained compatibility surfaces:
  - `GraphEditorViewModel`
  - `GraphEditorView`
  - `GraphEditorViewModel.Session`
- compatibility-only shims:
  - `GetCompatibleTargets(...)`
  - `CompatiblePortTarget`
  - `TrySetNodeExpansionState(...)`
  - `TrySetNodeGroupExtraPadding(...)`

Keep new code on the stable canonical surfaces. Treat retained and compatibility-only APIs as migration support.

## Start Here

- quickest runtime-only first run: [`tools/AsterGraph.HelloWorld`](../../tools/AsterGraph.HelloWorld/)
- canonical onboarding: [Quick Start](../../docs/en/quick-start.md)
- route and package boundary details: [Host Integration](../../docs/en/host-integration.md)
- advanced editing surface map: [Advanced Editing Guide](../../docs/en/advanced-editing.md)
- definition-driven inspector recipe: [Authoring Inspector Recipe](../../docs/en/authoring-inspector-recipe.md)
- tiered node-surface route: width/height resize, node-side parameter editing, fixed user-owned group frames, and geometry-based group membership travel through the same session/runtime path
- hosts can consume `GetNodeSurfaceSnapshots()`, `GetHierarchyStateSnapshot()`, and `GetNodeGroupSnapshots()` and drive `TrySetNodeSize(...)`, `TrySetNodeGroupSize(...)`, and `TrySetNodeGroupMemberships(...)` instead of recomputing canvas geometry or hierarchy ownership in UI code
- plugin and custom-node starting point: [Plugin And Custom Node Recipe](../../docs/en/plugin-recipe.md)
- retained-to-session migration guide: [Retained-To-Session Migration Recipe](../../docs/en/retained-migration-recipe.md)
- stability, precedence, and retirement rules: [Extension Contracts](../../docs/en/extension-contracts.md)
- product overview: [Root README](../../README.md)
