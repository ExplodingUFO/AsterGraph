# Phase 447 Context

Bead: `avalonia-node-map-mqm.4`

Branch: `phase447-extension-surface`

## Inputs Read

- `.planning/ROADMAP.md` from the parent checkout because this worktree did not contain `.planning`.
- `.planning/REQUIREMENTS.md` from the parent checkout.
- `.planning/phases/444-desktop-graph-library-capability-audit/444-CAPABILITY-AUDIT.md` from the parent checkout.
- Local extension-surface files under `src/AsterGraph.Avalonia/Presentation`, `src/AsterGraph.Editor/Runtime/IGraphRuntimeOverlayProvider.cs`, `tools/AsterGraph.ConsumerSample.Avalonia`, `docs/en`, `docs/zh-CN`, and focused tests.

## Constraints

- Do not modify beads state; `.beads/issues.jsonl` was already dirty and left untouched.
- Keep the phase inside customization, extension, API docs, proof files, and focused tests.
- Do not expose `NodeCanvas` internal `OverlayLayer`.
- Do not create a new execution model or public custom edge presenter.

## Existing Evidence

- Custom node presentation is already exposed through `AsterGraphPresentationOptions.NodeVisualPresenter`, `IGraphNodeVisualPresenter`, `GraphNodeVisual`, and `GraphNodeVisualContext`.
- Port anchors and typed connection target anchors already exist through `GraphNodeVisual.PortAnchors` and `GraphNodeVisual.ConnectionTargetAnchors`.
- Edge geometry snapshots already exist through `IGraphEditorQueries.GetConnectionGeometrySnapshots()`.
- Runtime decoration already exists through `IGraphRuntimeOverlayProvider` and `GetRuntimeOverlaySnapshot()`.
- ConsumerSample already carries copyable custom node, parameter editor, and edge overlay code.
