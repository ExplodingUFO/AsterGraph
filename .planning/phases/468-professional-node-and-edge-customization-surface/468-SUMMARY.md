# Phase 468 Summary: Professional Node And Edge Customization Surface

Bead: `avalonia-node-map-v78.3`

Status: implementation beads complete; synthesis complete

Requirement: `CUSTOM-01`

## Scope

Phase 468 deepened the supported customization surface for hosts that need custom node visuals, editor affordances, node and edge actions, connection styling, edge overlay semantics, and shortcut-policy proof. The phase stayed on the existing package contracts and hosted Avalonia projection route. It did not introduce a marketplace, sandbox, fallback layer, direct Avalonia menu mutation contract, or Demo-only contract.

## Bead Evidence

| Bead | Result | Commit | Files | Tests |
| --- | --- | --- | --- | --- |
| `avalonia-node-map-v78.3.1` / 468.1 Custom node editor input-scope proof | Custom node body and custom node visual editors can mark input scopes so destructive canvas shortcuts are suppressed while command palette access remains intentional. | `08d8787` `Add custom input scope shortcut proof` | `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` | `CustomNodeBodyInputScope_SuppressesDestructiveShortcutButKeepsCommandPaletteShortcut`; `CustomNodeVisualInputScope_SuppressesDestructiveShortcutButKeepsCommandPaletteShortcut` |
| `avalonia-node-map-v78.3.2` / 468.2 Node and edge action contribution contract | Node and edge actions are proven through runtime registry placements, canonical context-menu descriptors, and hosted action descriptors that execute through the canonical command route. | `d300187` `Add node edge action contract coverage` | `tests/AsterGraph.Editor.Tests/GraphEditorActionContributionContractTests.cs` | `CommandRegistry_NodeAndEdgeActionsExposeDescriptorPlacementSurfaces`; `ContextMenus_NodeAndEdgeActionsUseCanonicalMenuDescriptors`; `HostedActions_NodeAndEdgeActionsProjectRuntimeDescriptorsToCanonicalCommandRoute` |
| `avalonia-node-map-v78.3.3` / 468.3 Connection style and edge overlay proof | Custom connection style values preserve committed route geometry, selection hit behavior, label overlay placement, and pending preview semantics. | `f07104e` `test: prove connection style overlay semantics` | `tests/AsterGraph.Editor.Tests/NodeCanvasConnectionSceneRendererTests.cs` | `RenderConnections_CustomConnectionStyle_PreservesRouteSelectionAndPendingPreviewSemantics` |
| `avalonia-node-map-v78.3.4` / 468.4 Effective shortcut conflict proof | Hosted shortcut conflict detection now uses effective shortcut policy after command-id and action-id overrides, with action-id overrides taking precedence. | `ab13069` `Prove effective hosted shortcut conflicts` | `src/AsterGraph.Avalonia/Hosting/AsterGraphHostedActionFactory.cs`; `tests/AsterGraph.Editor.Tests/GraphEditorPluginCommandContractTests.cs` | `HostedActionShortcutConflictDetector_WithCommandIdPolicyOverride_ReportsEffectiveShortcutConflicts`; `HostedActionShortcutConflictDetector_WithActionAndCommandIdPolicyOverrides_UsesActionOverride` |

## Supported Customization Route

The supported route is descriptor-backed and package-owned:

1. Host or plugin code contributes runtime definitions, presenters, command descriptors, placements, and menu descriptors through `AsterGraph.Editor` and `AsterGraph.Avalonia` package surfaces.
2. Node visuals and body editors use `AsterGraphPresentationOptions.NodeVisualPresenter` and `AsterGraphPresentationOptions.NodeBodyPresenter`. Custom editor controls that need text/editing behavior must participate in input-scope suppression through the existing `IGraphEditorInputScope` path.
3. Node and edge actions flow through the command registry and menu descriptor pipeline: `IGraphEditorQueries.GetCommandRegistry()`, `IGraphEditorQueries.BuildContextMenuDescriptors(...)`, and hosted action descriptors from `AsterGraphAuthoringToolActionFactory`.
4. Hosted UI executes actions through the canonical session command route, not by mutating Avalonia menus directly.
5. Connection styling uses `ConnectionStyleOptions` through the scene renderer style resolver. Edge overlay semantics remain tied to connection geometry snapshots, label placement, route data, and stock selection/pending-preview behavior.
6. Shortcut conflicts are computed after `AsterGraphCommandShortcutPolicy` is applied. Command-id policy overrides apply to command-backed hosted actions; action-id overrides remain the narrowest and highest-priority override.

## CUSTOM-01 Closure

`CUSTOM-01` is satisfied for Phase 468 because the supported customization route now has source-backed proof for:

- custom node visual and body editor input scopes
- node and edge action descriptor placement
- canonical context-menu descriptor projection
- hosted action execution through session commands
- connection style and edge overlay preservation
- override-aware hosted shortcut conflict reporting

The phase remains bounded to package contracts and hosted Avalonia projection. Demo assets may illustrate these surfaces later, but they are not the contract.

## Explicit Non-Goals

Phase 468 did not add or authorize:

- marketplace behavior, remote distribution, or remote install/update flows
- sandboxing, untrusted-code isolation, or plugin unload/reload lifecycle claims
- fallback layers, compatibility shims, or degradation routes
- a direct Avalonia menu mutation contract for customization
- a Demo-only contract or route that bypasses package APIs
- a second command runtime, interaction runtime, or workflow execution engine

## Phase 470 Handoff

Phase 470 should use this phase as the customization proof base for cookbook scenarios. Recommended cookbook coverage:

- a custom node visual/body editor recipe that calls out input-scope suppression and command palette access
- a node and edge action recipe that starts from command descriptors, registry placements, and canonical menu descriptors
- a connection styling recipe that shows supported style values and explains that overlays should follow geometry snapshots instead of owning routes
- a shortcut policy recipe that shows effective conflict reporting after host overrides

Keep the cookbook as code-plus-demo guidance over the package contracts above. Do not turn the Demo into a separate customization contract.

## Validation Notes

This summary is planning-only. It maps the completed 468.1 through 468.4 beads to branch evidence and does not modify production code, tests, docs, or `.beads`.
