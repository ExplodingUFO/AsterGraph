# Phase 458 Plan: Command Platform Architecture And Input Audit

## Success Criteria

1. Current command descriptors, input routing, menu/tool surfaces, and undo boundaries are mapped with file/class evidence.
2. Host-owned versus stock workbench-owned command responsibilities are explicit.
3. Later implementation phases have narrow write scopes and dependency order.
4. No macro/query scripting system, fallback layer, compatibility shim, or second command runtime is planned.
5. Beads, Git, Dolt, branches, and `.worktrees` are clean after closeout.

## Completed Child Beads

### 458.1 - Editor Command And Undo Seam Audit

Bead: `avalonia-node-map-48w.1.1`

Output:

- `.planning/phases/458-command-platform-architecture-and-input-audit/458.1-EDITOR-COMMAND-AUDIT.md`

Result:

- Runtime descriptor, session command, kernel router, plugin command, menu/tool descriptor, and undo/redo boundaries are mapped.
- Phase 459 risk is concentrated around keeping registry work out of `GraphEditorKernelCommandRouter` god-code growth.

### 458.2 - Avalonia Command Surface Audit

Bead: `avalonia-node-map-48w.1.2`

Output:

- `.planning/phases/458-command-platform-architecture-and-input-audit/458.2-AVALONIA-COMMAND-SURFACE-AUDIT.md`

Result:

- Avalonia shortcut routing, hosted action projection, command palette, header toolbar, authoring tools, context menus, and pointer interaction boundaries are mapped.
- Phase 459 risk is concentrated around keeping registry/conflict logic out of `GraphEditorView.axaml.cs`.

### 458.3 - Command Platform Architecture Plan

Bead: `avalonia-node-map-48w.1.3`

Output:

- `.planning/phases/458-command-platform-architecture-and-input-audit/458-CONTEXT.md`
- `.planning/phases/458-command-platform-architecture-and-input-audit/458-COMMAND-PLATFORM-AUDIT.md`
- `.planning/phases/458-command-platform-architecture-and-input-audit/458-PLAN.md`
- `.planning/phases/458-command-platform-architecture-and-input-audit/458-HANDOFF.md`

## Phase 459 Recommended Split

### 459.1 - Runtime Command Registry Contracts

Scope:

- Add or harden registry snapshots/contracts for command identity, source, grouping, placement metadata, default shortcut metadata, availability, recovery, and undo classification.
- Keep execution through `IGraphEditorCommands.TryExecuteCommand(...)`.

Candidate files:

- `src/AsterGraph.Editor/Runtime/*Command*`
- `src/AsterGraph.Editor/Runtime/Internal/*Command*`
- `tests/AsterGraph.Editor.Tests/*Command*`

### 459.2 - Keybinding Conflict Detection

Scope:

- Add pure conflict detection for effective shortcuts after host shortcut policy is applied.
- Cover stock and host/plugin command scenarios.

Candidate files:

- `src/AsterGraph.Editor/Runtime/*Keybinding*` or focused Avalonia-hosting service if the policy type remains Avalonia-owned.
- `src/AsterGraph.Avalonia/Hosting/AsterGraphCommandShortcutPolicy.cs`
- `tests/AsterGraph.Editor.Tests` or `tests/AsterGraph.Avalonia.Tests` if a project exists.

### 459.3 - Avalonia Registry Projection

Scope:

- Move stock header/palette/tool/menu projection to consume the unified registry while keeping layout decisions in Avalonia.
- Keep `AsterGraphHostedActionFactory` as the bridge from descriptors to actions.

Candidate files:

- `src/AsterGraph.Avalonia/Hosting/AsterGraphHostedActionFactory.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/GraphEditorDefaultCommandShortcutRouter.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.AuthoringTools.cs`
- focused UI tests around header, palette, authoring tools, and shortcuts.

### 459.4 - Contract Proof And Phase Closeout

Scope:

- Update tests, public API inventory if needed, planning state, and handoff.
- Keep docs brief and source-backed; cookbook expansion waits for Phase 464.

## Verification For Phase 459

- Focused command registry tests.
- Focused keybinding conflict tests.
- Focused Avalonia command surface tests.
- Prohibited external-name scan across `.planning`, `docs`, and `src`.
- `dotnet test` filters for command router/session/view command surface coverage.

## Do Not Do

- Do not add a second command dispatcher.
- Do not add a macro/query/scripting layer.
- Do not add fallback or compatibility shims.
- Do not move undo/redo semantics away from the existing history coordinator.
- Do not make Demo own command execution semantics.
