# Phase 456 Context: Designer Workbench Authoring UX

## Beads

- Parent: `avalonia-node-map-y7i.6` — Phase 456: Designer workbench authoring UX
- Child: `avalonia-node-map-y7i.6.1` — Navigator and outline projection
- Child: `avalonia-node-map-y7i.6.2` — Route group layout recovery affordances
- Child: `avalonia-node-map-y7i.6.3` — Designer cookbook proof and docs

## Goal

Raise the Avalonia workbench into a designer-grade authoring surface without creating a second state owner. The new UX should project existing graph, group, scope, selection, route, layout, and validation state through small source-backed query/view surfaces.

## Constraints

- Use beads for task split, status, and handoff.
- Keep implementation worktrees inside `.worktrees/`.
- Keep 456.1 and 456.2 write scopes disjoint unless current code shape makes that unsafe.
- Do not add compatibility layers, fallback behavior, macro/query engines, or generated runnable demo code.
- Do not name external inspiration projects or packages in docs or planning.
- Avoid god UI code: each new projection should be a small type or host method with focused tests.

## Existing Surface

- `GraphEditorView.axaml` already has left library, central canvas, right inspector, authoring tools, minimap, status row, command palette, validation repair, and shortcut help regions.
- `GraphEditorView.AuthoringTools.cs` already projects selection, node, and connection actions from session tool descriptors and command descriptors.
- `DemoCookbookWorkspaceProjection` already owns cookbook navigation/content projection for the demo.
- Group, route, layout, and validation command seams already exist from earlier phases.

## Independence

- `456.1` should prefer demo/workspace or editor projection types for navigator/outline state.
- `456.2` should prefer Avalonia authoring tool and command descriptor projection for route/group/layout/recovery affordances.
- `456.3` depends on both and should only update cookbook/docs/proof/planning closeout.
