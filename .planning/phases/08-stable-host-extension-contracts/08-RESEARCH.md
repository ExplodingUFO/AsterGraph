# Phase 8: Stable Host Extension Contracts - Research

## Summary

The main Phase 8 problem is not missing functionality; it is overly concrete seam design.

Current host extension seams still expose:

- `GraphEditorViewModel` in `IGraphContextMenuAugmentor`
- `NodeViewModel` in `INodePresentationProvider`

And the full-shell Avalonia entry still lacks the shell-level opt-out knobs already available on standalone canvas.

## Recommended Direction

1. Keep the retained MVVM seams for compatibility.
2. Add new host-facing seam contracts built around smaller DTO/context types.
3. Shift HostSample and docs to prefer the new seams once they exist.
4. Add shell-level `EnableDefaultContextMenu` / `EnableDefaultCommandShortcuts` style options to the full-shell path.

## Contract Shape Guidance

### Menu Augmentation

Preferred shape:

- host gets a narrow editor/session host context
- menu augmentation receives:
  - menu context
  - stock items
  - stable selection/document/runtime state access

Avoid:

- passing the entire mutable `GraphEditorViewModel`

### Node Presentation

Preferred shape:

- node presentation provider receives a stable snapshot/context with:
  - node id
  - titles/descriptions/categories
  - selected state
  - connection/port counts or other already-derived metadata as needed

Avoid:

- passing the mutable `NodeViewModel` as the public seam root

### Full-Shell Opt-Out

Preferred shape:

- `AsterGraphAvaloniaViewOptions` grows shell-level toggles matching standalone canvas semantics
- `GraphEditorView` routes those toggles to its existing key/menu handling

## Risks

- creating “god” host context objects that just repackage `GraphEditorViewModel`
- changing defaults in a way that silently alters current host behavior
- moving too much input policy into Phase 8 instead of keeping it for Phase 9
