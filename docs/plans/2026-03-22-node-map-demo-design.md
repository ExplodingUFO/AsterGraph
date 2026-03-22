# Avalonia Node Map Demo Design

**Date:** 2026-03-22
**Status:** Assumption-driven design for first demo delivery

## Goal

Build a first runnable Avalonia demo that shows a reusable node graph component with:

- draggable nodes
- zoomable and pannable canvas
- visual connections between node ports
- a project layout split into multiple libraries
- a demo application that references only `NodeMap.Avalonia`

## Assumptions

- The repository is starting from an empty directory.
- The first version optimizes for a convincing demo rather than a full production editor.
- The user explicitly wants progress without requirement interviews, so this design chooses sensible defaults.
- TDD is intentionally not used for this first pass; validation will rely on build and smoke-run verification.

## Approaches Considered

### Approach A: Single Avalonia application project

Fastest to start, but it mixes demo concerns with reusable control code and makes later packaging harder.

### Approach B: Layered libraries with an Avalonia facade

Use a pure model library, a layout/scene library, and an Avalonia library that hosts the visual editor control. The demo app references only the Avalonia library. This keeps the public integration path simple while preserving internal layering.

**Recommendation:** choose this approach.

### Approach C: Full MVVM editor framework with commands, undo/redo, serialization, and runtime graph execution

Too large for the first demo and likely to slow delivery with little added value for the current goal.

## Proposed Architecture

### Solution layout

- `src/NodeMap.Core`
  Pure graph domain objects: nodes, ports, edges, positions, and demo graph factories.
- `src/NodeMap.Layout`
  Geometry helpers for port anchoring, edge path generation, and viewport/grid calculations.
- `src/NodeMap.Avalonia`
  Reusable Avalonia controls, view models, styles, templates, and the public `NodeEditorView`.
- `src/NodeMap.Demo`
  Avalonia desktop app that references only `NodeMap.Avalonia`.

### Dependency direction

- `NodeMap.Layout` depends on `NodeMap.Core`
- `NodeMap.Avalonia` depends on `NodeMap.Core` and `NodeMap.Layout`
- `NodeMap.Demo` depends only on `NodeMap.Avalonia`

## Component Design

### Core graph model

The core layer will define immutable-ish graph records or simple classes for:

- graph document
- graph node
- node port
- graph connection

This layer also provides sample data generation so the demo can boot with a populated graph immediately.

### Layout layer

The layout layer will provide:

- port anchor coordinate calculation
- cubic bezier path creation for connections
- viewport helpers for pan/zoom conversion
- grid snapping helpers where useful

This keeps math and scene calculation out of Avalonia controls.

### Avalonia layer

The Avalonia library will expose:

- a `NodeEditorView` user control
- a `NodeEditorViewModel`
- reusable item templates for nodes and ports
- canvas interaction behavior for panning, zooming, selection, and dragging
- a `NodeEditorDemoFactory` or equivalent convenience entry point so the demo app can stay thin

The main canvas will use Avalonia items and transforms rather than immediate-mode drawing for nodes, with a dedicated drawing surface for background grid and connection curves.

### Demo application

The demo app will:

- start with a single shell window
- host the `NodeEditorView`
- obtain demo content from `NodeMap.Avalonia`
- contain no direct references to `NodeMap.Core` or `NodeMap.Layout`

## Interaction Scope For V1

Included:

- drag nodes
- mouse-wheel zoom around pointer
- middle-button or right-button pan
- static sample connections
- selection highlight
- responsive side panel with brief instructions

Deferred:

- creating new connections interactively
- deleting nodes or edges
- undo/redo
- persistence
- auto-layout
- minimap

## Verification Strategy

Because the user requested no TDD for this pass, verification will be:

- `dotnet restore`
- `dotnet build`
- `dotnet run --project src/NodeMap.Demo`
- smoke-check that the window opens and the demo graph renders

## Outcome

This design yields a demo-first node editor with a clean upgrade path. The app integration surface stays minimal, while the internal libraries remain separated enough for future packaging and extension.
