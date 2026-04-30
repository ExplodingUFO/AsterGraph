# Phase 455 Context

## Beads

- Parent: `avalonia-node-map-y7i.5` - Phase 455: Layout services and alignment tools
- `avalonia-node-map-dvc` - 455.1: Layout contract and apply service
- `avalonia-node-map-21s` - 455.2: Workbench layout tools and canvas affordances
- `avalonia-node-map-xi9` - 455.3: Layout cookbook docs budgets and proof

## Current Baseline

- `IGraphLayoutProvider` and `GraphLayoutPlan` already provide host-owned preview plans through `IGraphEditorQueries.CreateLayoutPlan(...)`.
- Selection align and distribute exist as kernel commands backed by `NodeSelectionLayoutService`.
- Phase 455 must promote the layout surface into supported service/application behavior with undo boundaries, deterministic snap behavior, hosted workbench affordances, and proof-backed documentation.

## Constraints

- Keep the graph model single-owner; do not introduce a second layout engine or workflow runtime.
- Do not add compatibility, fallback, marketplace, scripting, or adapter-specific runtime behavior.
- Keep worktrees inside `.worktrees/` under this repository.
- Keep public claims tied to tests or measured proof.
