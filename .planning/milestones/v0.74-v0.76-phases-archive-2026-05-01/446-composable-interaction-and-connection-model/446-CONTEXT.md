# Phase 446: Composable Interaction And Connection Model - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Bead:** `avalonia-node-map-mqm.3`
**Branch:** `phase446-interaction-contracts`

<domain>
## Phase Boundary

Phase 446 strengthens the interaction and connection model through small host-facing contracts. The Phase 444 audit identified wire selection, route-vertex editing, shortcut docs, and optional compact state as the relevant gaps. Existing route-vertex commands and authoring controls already exist, so this pass focuses on the missing canonical route for direct connection selection and stale shortcut documentation.

</domain>

<decisions>
## Implementation Decisions

### Interaction Contract
- Add a compact command route for connection selection rather than introducing a new interaction framework.
- Keep the existing selected connection snapshot as the host inspection surface.
- Route direct wire clicks through the same command path used by hosts.
- Leave multi-wire gestures and richer route editing for later if concrete adopter evidence appears.

### Scope Control
- Do not add macro, scripting, or query-language behavior.
- Do not change rendering/performance foundations.
- Do not change customization extension surfaces except where interaction docs need to mention existing behavior.
- Keep the implementation inside existing session, kernel, renderer, docs, and focused tests.

### The Agent's Discretion
- Use the smallest command descriptor and routing changes needed to make direct connection selection inspectable and documented.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `GraphEditorSessionCommands.SetConnectionSelection(...)`
- `GraphEditorKernelCommandRouter`
- `GraphEditorCommandDescriptorCatalog`
- `NodeCanvasConnectionSceneRenderer`
- Existing wire selection, slicing, and command contract tests.

### Established Patterns
- Commands are surfaced through descriptor ids and routed through the kernel command router.
- Host-facing state is inspected through session query snapshots.
- Avalonia pointer behavior should delegate to session/kernel routes where possible.

### Integration Points
- Runtime command descriptor catalog and session command facade.
- Kernel command router and host seam.
- Connection renderer pointer handling.
- English and Chinese interaction docs.

</code_context>

<specifics>
## Specific Ideas

- Add `selection.connections.set` as the canonical command id for connection selection.
- Support repeated `connectionId` arguments and optional `primaryConnectionId`.
- Use left-click on rendered connection paths to invoke the command route.
- Repair shortcut docs to use the current shortcut opt-out API.

</specifics>

<deferred>
## Deferred Ideas

- Multi-select wire gestures beyond the existing command API.
- Direct drag handles on wire paths beyond existing route-vertex authoring controls.
- Optional compact interaction telemetry.

</deferred>
