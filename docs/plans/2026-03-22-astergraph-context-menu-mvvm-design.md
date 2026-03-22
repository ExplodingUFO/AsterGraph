# AsterGraph Context Menu And MVVM Design

**Date:** 2026-03-22
**Status:** Approved design for nested context menus and stronger MVVM interaction flow

## Goal

Add a reusable right-click menu system to AsterGraph with:

- nested context menus
- different menus for canvas, node, port, and connection targets
- a stronger MVVM command model
- right-click reserved for menus
- middle-button drag used for canvas panning

## Interaction Decision

The interaction priority is fixed as:

- right-click opens context menus
- middle-button drag pans the canvas

Right-click is no longer used for panning.

## Architecture Direction

### Editor layer owns menu intent

`AsterGraph.Editor` should define:

- target kinds
- menu context model
- menu item descriptor model
- editor commands
- menu tree generation

This keeps business actions and availability rules out of Avalonia controls.

### Avalonia layer owns menu rendering

`AsterGraph.Avalonia` should:

- detect what was right-clicked
- build an Avalonia `ContextMenu` from editor-provided descriptors
- route command execution back into editor view-model commands

The UI renders menus, but it should not decide what actions exist.

## Menu Model

### Target kinds

Use a target discriminator for menu generation:

- `Canvas`
- `Node`
- `Port`
- `Connection`

### Menu context

The menu context should include enough state to generate the correct menu:

- target kind
- current world position
- selected node
- selected connection if any
- clicked node if any
- clicked port if any
- clicked connection if any
- available node definitions

### Menu item descriptor

Each menu item should be described by data, not by Avalonia controls:

- `Id`
- `Header`
- `IconKey` or placeholder icon token
- `Command`
- `CommandParameter`
- `Children`
- `IsEnabled`
- `IsSeparator`

Nested menus are represented recursively through `Children`.

## Recommended Menu Structure

### Canvas menu

- `Add Node >`
  - grouped by category
  - for example:
    - `Inputs > Time Driver`
    - `Procedural > Noise Field`
    - `Color > Palette Ramp`
- `Paste`
- `Fit View`
- `Reset View`
- `Save Snapshot`
- `Load Snapshot`

When a node is added from this menu, it should appear at the right-click world position, not at viewport center.

### Node menu

- `Delete Node`
- `Duplicate Node`
- `Disconnect >`
  - `Incoming`
  - `Outgoing`
  - `All`
- `Create Connection From >`
  - list output ports
- `Inspect`
- `Center View Here`

Right-clicking a node should first select it, then open the node menu.

### Port menu

- `Start Connection`
- `Break Connections`
- `Compatible Targets >`
  - only for output ports
  - only list targets that pass compatibility rules
- `Compatibility Info`

### Connection menu

- `Delete Connection`
- `Inspect Conversion`
- `Insert Conversion >`

For the first pass, `Insert Conversion >` may be structural/preparatory rather than fully realized.

## MVVM Command Model

### Commands to add

`GraphEditorViewModel` should expose command properties for at least:

- `SaveCommand`
- `LoadCommand`
- `FitViewCommand`
- `ResetViewCommand`
- `DeleteSelectionCommand`
- `AddNodeCommand`
- `StartConnectionCommand`
- `DeleteConnectionCommand`

These commands should be reusable across:

- toolbar buttons
- keyboard shortcuts
- context menus

### Why this matters

At the moment, toolbar actions still rely on code-behind click handling. Moving command intent into the editor layer makes:

- Avalonia thinner
- testing easier
- future menu expansion cleaner
- plugin/menu extension more plausible later

## Compatibility Behavior In Menus

The menu system should reflect the current compatibility policy:

- exact matches are enabled normally
- safe implicit conversions are enabled with a visible label
- rejected connections do not show up as normal targets

Example:

- `Connect To > Lighting Mix / Pulse (implicit: int -> float)`

This makes the menu system part of the developer-facing explanation of compatibility.

## Error Handling And UX Rules

- disabled actions should be disabled before click, not fail silently after click
- incompatible targets should be omitted or explained through a compatibility-info item
- empty-space right-click shows only canvas actions
- if a pending connection exists, the menu should surface `Cancel Pending Connection`
- every menu action should update the shared status message path

## Scope

### In scope

- nested right-click menus
- middle-button panning
- menu descriptor model in editor layer
- MVVM commands for current common actions
- Avalonia menu rendering from descriptors
- toolbar actions moved toward command binding

### Out of scope

- full command bus
- connection multiselect
- advanced clipboard workflow
- parameter editor dialogs
- plugin-contributed menu injection

## Documentation Updates

After implementation:

- update root `README.md` with context-menu and MVVM command notes
- keep this feature documented separately from the broader AsterGraph refactor plan

## Expected Outcome

After this pass, AsterGraph should feel more like an actual editor:

- right-click is a first-class action surface
- menu logic is layered and extensible
- toolbar and menu actions share the same command model
- the editor moves closer to MVVM without a full framework rewrite
