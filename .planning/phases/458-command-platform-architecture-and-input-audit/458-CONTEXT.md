# Phase 458 Context: Command Platform Architecture And Input Audit

## Beads

- Parent: `avalonia-node-map-48w.1` - Phase 458: Command Platform Architecture And Input Audit
- Child: `avalonia-node-map-48w.1.1` - Editor command and undo seam audit
- Child: `avalonia-node-map-48w.1.2` - Avalonia input and workbench command surface audit
- Child: `avalonia-node-map-48w.1.3` - Command platform architecture plan

## Goal

Define the supported command and input architecture before implementation so Phase 459 can add a unified command registry and keybinding surface without creating a second command runtime, broad compatibility layer, or god UI coordinator.

## Constraints

- Use beads as the task split, status, and handoff spine.
- Keep parallel worktrees inside `.worktrees/`.
- Keep 458.1 and 458.2 write scopes disjoint; 458.3 integrates their reports after both land.
- Do not add source code, tests, compatibility layers, fallback behavior, macro/query engines, generated runnable demo code, or unsupported adapter claims in Phase 458.
- Do not name external inspiration projects or packages in planning artifacts, docs, source comments, or tests.
- Keep Phase 459 implementation seams narrow: package contracts own command identity/execution state, Avalonia owns projection/input handling, and Demo remains proof/sample surface only.

## Audit Split

- `458.1` maps Editor/runtime command descriptors, execution routing, disabled/recovery states, undo/redo boundaries, and host extension seams.
- `458.2` maps Avalonia input routing, keyboard/pointer handling, hosted action projection, menus, toolbars, palettes, and workbench composition.
- `458.3` turns both audits into the architecture report, implementation plan, and handoff for Phase 459.

## Known Starting Points

- `IGraphEditorSession`, `IGraphEditorCommands`, and `IGraphEditorQueries` already expose session command and descriptor boundaries.
- `GraphEditorKernelCommandRouter` and `GraphEditorCommandDescriptorCatalog` already own many canonical command descriptors, enabled state, shortcuts, disabled reasons, recovery hints, and recovery command IDs.
- `GraphEditorSessionMenus` and stock tool/menu descriptor builders already project menu and tool surfaces from command descriptors.
- `GraphEditorView.AuthoringTools.cs`, `AsterGraphHostedActionFactory`, and Avalonia input coordinators already project command and tool actions into the workbench.

## Independence

- `458.1` writes only `458.1-EDITOR-COMMAND-AUDIT.md`.
- `458.2` writes only `458.2-AVALONIA-COMMAND-SURFACE-AUDIT.md`.
- `458.3` writes only integration artifacts and roadmap/state closeout after both child reports are merged.
