# Phase 458 Command Platform Architecture Audit

## Conclusion

The project already has a usable canonical command spine:

- Runtime descriptors expose stable command identity, grouping, default shortcuts, enabled state, disabled reasons, recovery hints, and recovery command IDs.
- Kernel routing owns source-backed availability and dispatch.
- Session queries and commands expose descriptors and execution through public package boundaries.
- Avalonia consumes descriptors for shortcuts, hosted actions, command palette, toolbars, authoring tools, and context menus.

Phase 459 should therefore unify and harden the existing command registry surface rather than introduce a second command runtime. The right move is to make command discovery, shortcut metadata, placement metadata, conflict detection, and host/plugin registration explicit while preserving the existing session and kernel execution path.

## Architecture Boundary

### Package Runtime Owns

- Stable command IDs and command descriptor metadata.
- Command availability and recovery state derived from session state.
- Command execution through `IGraphEditorCommands.TryExecuteCommand(...)`.
- Menu and tool descriptor snapshots that point at stable command invocations.
- Host/plugin command descriptor registration and execution seams.

### Avalonia Owns

- Keyboard event handling and input-scope exclusions.
- Shortcut policy projection and host override application.
- Button, menu, toolbar, palette, tooltip, and focus behavior.
- Placement decisions for header, palette, authoring tools, context menus, and standalone canvas.
- Pointer gestures and canvas interaction coordinators.

### Demo Owns

- Cookbook proof scenarios and sample-specific flow composition.
- Visual demonstration of package-supported command surfaces.
- No command runtime, generator, macro layer, or compatibility shim.

## Evidence Summary

Editor/runtime evidence:

- `GraphEditorCommandDescriptorSnapshot` already carries the command shape needed by UI and automation.
- `GraphEditorCommandDescriptorCatalog` centralizes stock metadata and default shortcuts.
- `GraphEditorKernelCommandRouter` builds stateful descriptors and dispatches stable command IDs.
- `GraphEditorSessionCommands` exposes descriptor-first execution through the session facade.
- `GraphEditorSessionMenus` and stock tool/menu builders already consume descriptors.
- Kernel/router tests cover enabled state, dispatch, recovery hints, and invalid argument rejection.

Avalonia evidence:

- `GraphEditorDefaultCommandShortcutRouter` consumes descriptors and shortcut policy before calling session commands.
- `AsterGraphHostedActionFactory` converts descriptors and tool descriptors into hosted actions.
- `GraphEditorView.AuthoringTools.cs` projects selection, node, and connection actions from session tool descriptors.
- `GraphEditorView.axaml.cs` builds header actions and command palette from hosted action projections.
- `NodeCanvas` owns pointer and standalone shortcut wiring, while semantic operations still end at session/view model boundaries.

## Phase 459 Implementation Shape

Phase 459 should add a small command registry contract that can answer:

- What commands exist in the current session?
- Which commands are stock, host, or plugin sourced?
- What default and effective shortcuts are assigned?
- Which commands are eligible for header/menu/tool/palette placement?
- Which shortcuts conflict after host policy is applied?
- Which command is executable through the canonical session boundary?

It should not add:

- A second command dispatcher.
- A new history stack.
- A broad compatibility layer.
- A scripting or macro command system.
- Demo-owned command semantics.

## High-Risk Areas

- `GraphEditorKernelCommandRouter` is large. Keep new registry assembly and conflict checks in focused types; only touch router where stateful command availability must remain source-backed.
- `GraphEditorView.axaml.cs` is large. Avoid placing registry construction or conflict algorithms in the view.
- `HeaderCommandIds` is currently local Avalonia placement state. Phase 459 should let runtime/registry metadata identify supported placement while keeping final layout in Avalonia.
- Plugin and host command fallback metadata can hide missing supported stock metadata. Stock command IDs should be explicit and test-covered.
- Shortcut conflicts must evaluate effective shortcuts after `AsterGraphCommandShortcutPolicy`, not only catalog defaults.

## Recommended Phase 459 Ownership

- Runtime registry/query model: new focused files under `src/AsterGraph.Editor/Runtime` or `src/AsterGraph.Editor/Runtime/Internal`.
- Conflict detection: a small pure service with tests in `tests/AsterGraph.Editor.Tests`.
- Avalonia projection changes: limited to hosted action factory, shortcut router tests, and command surface construction.
- Demo/cookbook changes: defer until Phase 464 unless a minimal proof marker is needed.

## Verification

- `458.1-EDITOR-COMMAND-AUDIT.md` documents Editor/runtime command and undo seams.
- `458.2-AVALONIA-COMMAND-SURFACE-AUDIT.md` documents Avalonia input and workbench command surfaces.
- Phase 458 made planning artifacts only; source code and tests were intentionally not changed.
