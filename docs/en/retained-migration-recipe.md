# Retained-To-Session Migration Recipe

Choose retained only when you are migrating an existing host in batches. This recipe is migration-only, not the preferred long-term extension path. Use it when an existing host still constructs `GraphEditorViewModel` directly but wants to move toward the canonical runtime/session seams without rewriting everything at once.

Copy this recipe only for an existing retained host slice. It stays secondary to the canonical session/runtime route and the shipped Avalonia route, and it does not create a new compatibility promise.

This is the single bounded retained recipe set. If an existing host still constructs `GraphEditorViewModel` or `GraphEditorView`, use this doc; otherwise start on `CreateSession(...)` or `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`.

## Target State

New code should land on:

- `AsterGraphEditorFactory.CreateSession(...)` for runtime-only or custom UI hosts
- `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for the shipped Avalonia UI path
- `IGraphEditorSession.Commands`, `Queries`, `Events`, and diagnostics snapshots

If you are starting new work, begin with [Quick Start](./quick-start.md) or [Host Integration](./host-integration.md) instead of the retained bridge.

## Stage 0: Keep the retained bridge

Keep temporarily:

- `GraphEditorViewModel`
- `GraphEditorView`
- host-owned `AsterGraphEditorOptions` composition

Replace with canonical seams:

- create runtime-only slices with `AsterGraphEditorFactory.CreateSession(...)`
- route new commands and queries through `IGraphEditorSession`

Ownership after this stage:

- the host still owns shell composition and options wiring
- the session owns commands, queries, events, diagnostics, and automation
- the retained bridge only adapts the UI surface

## Stage 1: Move command and query ownership into the session

Keep temporarily:

- the retained view-model bridge for untouched host slices
- the shipped Avalonia shell if the host still needs it

Replace with canonical seams:

- use `editor.Session.Commands` for new actions
- use DTO/snapshot queries instead of retained projections
- prefer `CreateSession(...)` for runtime-only callers

Compatibility bridges that can stay only during this stage:

- canonical connection discovery: `GetCompatiblePortTargets(...)` and `GraphEditorCompatiblePortTargetSnapshot`
- temporary node-group helpers: `TrySetNodeExpansionState(...)` and `TrySetNodeGroupExtraPadding(...)`
- canonical connection control: `connections.disconnect-*`, especially `connections.disconnect-all`

Ownership after this stage:

- commands and queries are session-owned
- host code owns orchestration and composition only
- compatibility shims are legacy-only and do not get new behavior

## Stage 2: Replace retained-only helpers and retire bridge-only behavior

Keep temporarily:

- `GraphEditorViewModel` and `GraphEditorView` only as migration scaffolding for untouched legacy code

Replace with canonical seams:

- `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` for shipped Avalonia UI
- `GetCompatiblePortTargets(...)` and snapshot APIs for compatible-target projection
- the session/runtime route for new work

Ownership after this stage:

- UI composition is factory-owned
- commands and queries are session-owned
- retained helpers are legacy-only and stop being the extension path

## Proof and Evidence Handoff

For retained migration evidence, use the same defended hosted beta route as the rest of the public docs:

- run `src/AsterGraph.Demo -- --proof`
- if you need a local artifact, attach `artifacts/consumer-support-bundle.json` using [Beta Support Bundle](./support-bundle.md)
- review the same public proof markers: `CONSUMER_SAMPLE_OK:True`, `COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`, the `HOST_NATIVE_METRIC:*` lines, and the support-bundle markers `SUPPORT_BUNDLE_OK:True` and `SUPPORT_BUNDLE_PATH:...`
- if `CONSUMER_SAMPLE_PARAMETER_OK` or `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` fail, keep the failed proof-marker lines with the support bundle's `parameterSnapshots` rows on the same bounded intake record

The retained bridge does not add a separate support boundary or a retained-only evidence lane, so maintainers can review evidence with the same public proof and bundle docs.

## Phase 498 removal execution gate

Phase 498 is GitHub #119 / `avalonia-node-map-3um`. It defines the retained migration removal execution gate only; this recipe update authorizes no retained API removal, no public API baseline change, no runtime behavior change, and no UI change.

Any later removal PR must open a new API-change issue and include all of the following evidence before changing code or `eng/public-api-baseline.txt`:

1. exact public API metadata lines from `eng/public-api-baseline.txt`, with the affected symbol list copied into the issue before deletion.
2. Replacement route for each removed symbol, with docs pointing to the canonical session/runtime route or shipped Avalonia factory route.
3. blocker tests that prove the replacement route covers the old retained use case, and that any remaining public docs no longer teach the removed API.
4. support-window decision that names the version or milestone where the compatibility period ends.
5. migration evidence from the defended hosted beta proof lane and support bundle, including any affected adopter notes.
6. Public API baseline approval path that reviews the exact baseline diff in the later API-change issue.

The removal candidate list starts with these retained or compatibility-only surfaces:

| Surface | Replacement / evidence required before removal |
| --- | --- |
| `GraphEditorViewModel`, `GraphEditorView`, and `GraphEditorViewModel.Session` retained bridge usage | Prove hosts can use `AsterGraphEditorFactory.CreateSession(...)` for runtime-owned work or `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)` for shipped Avalonia UI. |
| `GraphDocumentSerializer.ImportLegacy(...)` explicit import path | Prove no supported migration flow still needs direct legacy import, or keep it because legacy payload conversion is still a supported import boundary. |
| `IGraphContextMenuAugmentor.Augment(GraphEditorViewModel, ...)` | Prove all host menu augmentors can implement `Augment(GraphContextMenuAugmentationContext)` instead. |
| `INodePresentationProvider.GetNodePresentation(NodeViewModel)` | Prove host presenters can implement `GetNodePresentation(NodePresentationContext)` instead. |
| `NodeViewModel.ExpansionState` and `NodeViewModel.IsExpanded` | Prove hosted UI decisions use `ActiveSurfaceTier`, `Surface.ExpansionState`, or session/query snapshots. |
| `TrySetNodeExpansionState(...)` and `TrySetNodeGroupExtraPadding(...)` retained helpers | Prove callers moved to `Session.Commands.TrySetNodeExpansionState(...)`, size/group commands, and session query snapshots. |

## Success Criteria

A host is effectively off the migration-critical path once:

- new commands and queries land on `IGraphEditorSession`
- UI composition uses the factory route instead of direct retained construction
- compatibility-only shims are only serving untouched legacy host code
- new features start from the canonical session route or the shipped Avalonia route, not from retained constructors

See also:

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
- [Plugin Host Recipe](./plugin-host-recipe.md)
- [Custom Node Host Recipe](./custom-node-host-recipe.md)
