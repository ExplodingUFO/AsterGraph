# Phase 468 Handoff: Customization Surface To Cookbook Scenarios

Bead: `avalonia-node-map-v78.3.5`

Status: complete

## What Is Ready

Phase 468 closes the `CUSTOM-01` implementation proof for professional node and edge customization. The supported route is:

- descriptors and registry placements for command/action discovery
- canonical menu descriptors for node and edge context actions
- hosted action descriptors that execute through `IGraphEditorSession.Commands.TryExecuteCommand(...)`
- custom node visual/body presenters through `AsterGraphPresentationOptions`
- input-scope suppression for embedded custom editors
- connection style and edge overlay semantics tied to stock route, selection, label, and pending preview behavior
- effective hosted shortcut conflicts after host shortcut policy overrides

## Evidence To Carry Forward

| Area | Evidence |
| --- | --- |
| Input scope | Commit `08d8787`; `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`; custom body and custom visual presenter tests prove destructive shortcut suppression while `Ctrl+Shift+P` still opens the command palette. |
| Action contributions | Commit `d300187`; `tests/AsterGraph.Editor.Tests/GraphEditorActionContributionContractTests.cs`; registry placement, menu descriptor, and hosted action projection tests prove node and edge actions stay on the canonical command route. |
| Connection styling and overlays | Commit `f07104e`; `tests/AsterGraph.Editor.Tests/NodeCanvasConnectionSceneRendererTests.cs`; renderer proof preserves route geometry, style values, label placement, committed-edge selection, and pending preview behavior. |
| Shortcut conflicts | Commit `ab13069`; `src/AsterGraph.Avalonia/Hosting/AsterGraphHostedActionFactory.cs`; `tests/AsterGraph.Editor.Tests/GraphEditorPluginCommandContractTests.cs`; effective shortcut conflicts use command-id overrides unless an action-id override exists. |

## Phase 470 Cookbook Scenarios

Use these as the Phase 470 customization cookbook candidates:

1. Custom node editor scenario
   - Show `NodeVisualPresenter` or `NodeBodyPresenter` replacement.
   - Include an embedded editor/control that participates in input-scope suppression.
   - State that destructive canvas shortcuts are suppressed inside the editor, while command palette access remains intentional.

2. Node and edge action scenario
   - Show actions as command descriptors, registry placements, and menu descriptors.
   - Execute through hosted action descriptors and `TryExecuteCommand(...)`.
   - Avoid direct Avalonia menu mutation as a contract.

3. Connection style and overlay scenario
   - Show supported `ConnectionStyleOptions` customization.
   - Keep overlays derived from geometry snapshots and stock route semantics.
   - Confirm committed edges remain selectable and pending previews remain non-selecting previews.

4. Shortcut policy scenario
   - Show default shortcuts, command-id overrides, action-id overrides, and conflict reporting after policy application.
   - Explain that action-id overrides win when both action and command overrides exist.

## Boundaries For Next Work

Do not widen Phase 470 into:

- marketplace, remote distribution, or plugin install/update workflows
- sandboxing or untrusted-code isolation
- fallback layers or compatibility shims
- a direct Avalonia menu mutation contract
- a Demo-only customization route
- a second command, interaction, or workflow runtime

## Suggested Phase 470 Acceptance Hooks

- Cookbook text maps each scenario back to a package API or hosted helper.
- Demo/sample code is labeled as illustration, not as the contract.
- Customization examples preserve the Phase 468 route: descriptor/registry/menu/hosted action, input-scope suppression, connection style/overlay semantics, and effective shortcut conflicts.
- Validation includes documentation scan for forbidden scope expansion and a focused proof/test command chosen by the Phase 470 owner.

## Session Notes

No production code, tests, docs, or `.beads` files were changed for this synthesis. The two planning artifacts are the only intended outputs.
