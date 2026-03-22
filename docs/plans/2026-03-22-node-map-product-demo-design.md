# Avalonia Node Map Product Demo Design

**Date:** 2026-03-22
**Status:** Assumption-driven extension of the first demo

## Goal

Turn the existing visual demo into a product-like editor demo with a usable edit loop:

- add nodes from a library
- create connections interactively
- remove selected nodes
- save and reload the current graph
- reset and fit the viewport
- expose clear status and workflow affordances in the shell

## Assumptions

- The user explicitly asked to keep pushing without requirement interviews.
- Product-like means “credible editing workflow and polished shell”, not full production architecture.
- This round should preserve the rule that the host app references only `NodeMap.Avalonia`.
- TDD remains intentionally disabled for this project per user request.

## Approaches Considered

### Approach A: Keep the current static demo and only polish visuals

Fast, but it would still feel like a passive mock-up rather than a usable product demo.

### Approach B: Add a minimal editor workflow and supportive shell

Introduce node templates, interactive connection creation, selection-based delete, save/load snapshot, and viewport controls. This creates a coherent “edit, persist, reload” loop while keeping scope under control.

**Recommendation:** choose this approach.

### Approach C: Build full editor infrastructure now

Undo/redo, file pickers, node property editing, edge selection, minimap, and auto-layout would add real value later, but they would slow the current delivery without being required to make the demo feel usable.

## Product-Demo Scope

### Included

- product-style top toolbar
- left-side node library / palette
- right-side inspector and help content
- bottom status bar
- add-node workflow from templates
- output-to-input interactive connection creation
- delete selected node
- reset view and fit view
- save snapshot to a stable local JSON path
- reload snapshot from the same path

### Deferred

- freeform property editing
- connection selection and deletion
- undo/redo
- file picker integration
- multi-document tabs
- auto-layout and minimap

## Architecture Changes

### Core

Add graph serialization so the current editor state can round-trip as JSON without Avalonia dependencies.

### Avalonia

Promote the current view model from read-only scene state to lightweight editor state:

- mutable node and connection collections
- selected node tracking
- pending connection state
- template catalog for node creation
- status and workspace metadata
- save/load and edit commands

### UI

Refactor the shell into a three-panel editor:

- left: node library and quick actions
- center: canvas and toolbar
- right: inspector
- bottom: status strip

## Interaction Design

### Node creation

Click a template in the left library to insert a node near the current viewport center.

### Connection creation

Click an output port to start a pending connection, then click an input port to complete it. A preview curve follows the cursor while pending.

### Save/load

The editor saves and loads `demo-graph.json` in an app-controlled workspace location. The UI surfaces the resolved path and success/failure messages.

### View commands

- `Reset View`: return to the default camera
- `Fit View`: fit all current nodes into the canvas

## Verification

- `dotnet build avalonia-node-map.sln`
- launch the demo executable or `dotnet run --project src/NodeMap.Demo/NodeMap.Demo.csproj`
- smoke-check:
  - add node
  - create connection
  - delete selected node
  - save snapshot
  - reload snapshot
  - fit/reset viewport
