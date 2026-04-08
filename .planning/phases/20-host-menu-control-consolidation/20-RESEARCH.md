# Phase 20: Host Menu Control Consolidation - Research

**Completed:** 2026-04-08
**Goal:** Identify the cleanest Avalonia-native way to turn the new graph-first demo shell into a compact menu-driven control surface without reintroducing session drift or UI clutter.

## Current Codebase Baseline

The current demo already has almost all of the required state:

- `MainWindow.axaml` already ships a top host `Menu` and a right-side `SplitView` pane.
- `MainWindowViewModel` already owns all required shell and behavior toggles:
  - `IsHeaderChromeVisible`
  - `IsLibraryChromeVisible`
  - `IsInspectorChromeVisible`
  - `IsStatusChromeVisible`
  - `IsReadOnlyEnabled`
  - `IsGridSnappingEnabled`
  - `IsAlignmentGuidesEnabled`
  - `AreWorkspaceCommandsEnabled`
  - `AreFragmentCommandsEnabled`
  - `AreHostMenuExtensionsEnabled`
- `ApplyHostOptions()` already pushes behavior changes into the same `Editor`.
- Runtime readouts already come from the canonical session/diagnostics path rather than ad-hoc UI state.

Conclusion: Phase 20 should be a control-surface consolidation phase, not a runtime feature phase.

## Avalonia Findings

### Menu control

Avalonia's `Menu` is intended for a top-level window menu, and `MenuItem` directly supports nested entries, commands, icons, separators, and checkable state.

The current official docs also confirm that `MenuItem` supports:

- `ToggleType="CheckBox"` for independent checked items
- `ToggleType="Radio"` with `GroupName` for grouped selection
- `IsChecked` binding for view-model-backed menu state

Implication for this phase:

- view and behavior quick toggles should use `MenuItem ToggleType="CheckBox"` with direct binding to the existing boolean properties
- Phase 20 does not need a custom fake menu-check pattern or a second toolbar

### SplitView

Avalonia's `SplitView` remains the right tool for the compact control surface:

- `PanePlacement="Right"` is supported directly
- `DisplayMode="Overlay"` is appropriate when the pane should stay secondary to the main content
- the documented compact/overlay pattern fits a temporary tool-pane style UI

Implication for this phase:

- keep the graph as the dominant content zone
- keep the right-side pane as the place for grouped controls and runtime readouts
- do not replace the Phase 19 shell with inline rails or permanent panels

## Recommended Implementation Posture

### 1. Menu-first, pane-backed

Use the host menu as the first control plane and the right-side pane as the compact dense surface.

Recommended breakdown:

- `展示`: shell summary and “open showcase pane” entry
- `视图`: direct checkable menu items for chrome visibility plus an entry that focuses the pane's view section
- `行为`: direct checkable menu items for behavior/permission toggles plus an entry that focuses the pane's behavior section
- `运行时`: open the runtime section and expose compact session/diagnostic readouts
- `证明`: keep reserved for Phase 21 narrative/proof cleanup

### 2. Single source of truth

Do not add parallel “menu state” copies of the control booleans.

Instead:

- bind menu `IsChecked` directly to the existing `MainWindowViewModel` properties
- keep grouped control rows in the pane bound to those same properties
- keep runtime summaries bound to the existing session projection properties

### 3. Runtime stays read-mostly

The runtime group does not need fabricated “control” features to satisfy the phase.

The proof for this phase is:

- runtime summaries and recent diagnostics are discoverable from the same host menu structure
- they update on the same `Editor.Session`
- opening those readouts does not switch scenes or rebuild the editor

## Risks And Guardrails

- Avoid menu overgrowth. If too many items are placed directly in the dropdown, the shell will regress toward a ribbon or admin console.
- Avoid duplicating `ApplyHostOptions()` logic in command handlers. The existing property-changed flow is already correct.
- Avoid adding a second runtime summary model separate from `Editor.Session.Diagnostics`.
- Avoid runtime “refresh” buttons that only mirror already-live bindings unless there is a real need.

## Recommended Wave Structure

### Wave 1

Consolidate `视图` into real host menu toggles plus a compact pane section for chrome/shell controls.

### Wave 2

Consolidate `行为` into real host menu toggles plus a compact pane section for behavior/permission controls.

### Wave 3

Consolidate `运行时` into compact readouts from the canonical session path and lock all host-menu control behavior with targeted headless tests.

## Sources

- [Avalonia Menu](https://docs.avaloniaui.net/controls/menus/menu)
- [Avalonia Menu Controls](https://docs.avaloniaui.net/docs/reference/controls/menu-controls)
- [Avalonia SplitView](https://docs.avaloniaui.net/docs/reference/controls/splitview)
