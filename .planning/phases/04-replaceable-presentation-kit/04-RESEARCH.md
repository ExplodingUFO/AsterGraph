# Phase 4: Replaceable Presentation Kit - Research

**Date:** 2026-03-26
**Phase:** 04-replaceable-presentation-kit
**Source Inputs:** `04-CONTEXT.md`, `PROJECT.md`, `REQUIREMENTS.md`, `STATE.md`, current Avalonia control code

## Research Question

How should AsterGraph expose replaceable Avalonia presenters for nodes, menus, inspector, and mini map while preserving editor-owned behavior, Phase 3’s embeddable surface matrix, and the current migration-safe host story?

## Current State

### What Already Exists

- `NodeCanvas` owns node rendering plus interaction behavior in `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `GraphContextMenuPresenter` is already public and translates `MenuItemDescriptor` into Avalonia menu controls
- `GraphInspectorView` is already a standalone surface that binds directly to `GraphEditorViewModel`
- `GraphMiniMap` is already a standalone surface that binds directly to `GraphEditorViewModel`
- `GraphEditorView` is now a thin full-shell composition root over standalone surfaces
- `INodePresentationProvider` and `IGraphContextMenuAugmentor` already let hosts affect display state and menu intent from the editor layer

### Where The Tight Coupling Still Lives

- Node visuals are created inline through `CreateNodeVisual(...)` and updated through `UpdateNodeVisual(...)` inside `NodeCanvas`
- Menu presentation is fixed to one concrete stock presenter inside `NodeCanvas`
- `GraphInspectorView` is a concrete XAML control with no replacement contract around it yet
- `GraphMiniMap` is a concrete control with rendering and viewport logic in one type

## Key Architectural Constraint

Phase 4 must not move Avalonia presentation concerns into `AsterGraph.Editor`. The editor layer should continue to own state, intent, and typed projections. Avalonia should continue to own control composition and rendering contracts.

That implies:

- replacement contracts belong in `src/AsterGraph.Avalonia`
- editor-facing contracts such as `MenuItemDescriptor`, `INodePresentationProvider`, and parameter/selection projections stay the input model
- default stock presenters remain available and are simply swapped out through optional host configuration

## Options Considered

### Option A: Presenter Interfaces + Stock Defaults

Define presenter interfaces in `AsterGraph.Avalonia` for the four replaceable surfaces and keep stock implementations as the default.

Examples:

- node visual presenter for `NodeCanvas`
- context-menu presenter for menu display
- inspector presenter or inspector view factory seam
- mini-map presenter or mini-map view factory seam

**Pros**

- matches Phase 3’s medium-grain surfaces
- preserves current stock host story
- keeps replacements opt-in and per-surface
- easy to prove through host sample and smoke markers

**Cons**

- interface design must be careful not to leak too much Avalonia-internal complexity
- node presenter seam needs a clean contract for anchors, badges, status bar, and parameter state

### Option B: Full Custom Control Ownership

Tell hosts to replace entire controls and only preserve `GraphEditorViewModel`.

**Pros**

- minimal new API work

**Cons**

- fails `PRES-01` through `PRES-04`
- pushes drag/selection/connection/menu/viewport behavior reimplementation onto hosts
- contradicts the product goal of stable replaceable visuals over preserved behavior

### Option C: Data Templates Only

Expose Avalonia `DataTemplate` or template-key injection points without explicit presenter contracts.

**Pros**

- lightweight Avalonia-native customization model

**Cons**

- too weak for context-menu and mini-map replacement
- difficult to validate as a public host story
- insufficiently explicit for a published SDK

## Recommended Direction

Choose **Option A**: explicit presenter seams in `AsterGraph.Avalonia` with stock defaults preserved.

This best fits:

- Phase 3’s surface decomposition
- the project’s opt-in customization philosophy
- the requirement that hosts reuse behavior rather than fork it

## Recommended Surface Boundaries

### 1. Node Visual Replacement

Recommended approach:

- keep `NodeCanvas` as the interaction owner
- extract node visual construction/update behind an Avalonia presenter contract
- presenter receives node state plus style data and returns/updates the node visual tree

This preserves:

- selection behavior
- drag behavior
- connection behavior
- marquee and viewport behavior
- anchor/port orchestration

Risk:

- if the contract is too low-level, hosts will need to understand too much internal canvas machinery
- if the contract is too high-level, it will not cover real replacement scenarios

### 2. Menu Presentation Replacement

Recommended approach:

- preserve `GraphEditorViewModel.BuildContextMenu(...)` and `MenuItemDescriptor`
- introduce an Avalonia menu presenter interface around the existing stock `GraphContextMenuPresenter`
- default path still uses stock presenter

This keeps:

- editor intent in `AsterGraph.Editor`
- augmentation in `IGraphContextMenuAugmentor`
- visual replacement in `AsterGraph.Avalonia`

### 3. Inspector Replacement

Recommended approach:

- keep `GraphInspectorView` as the stock default control
- add an inspector presenter/factory seam that still binds to the current editor-owned inspector projections
- avoid inventing a second inspector state model

### 4. Mini Map Replacement

Recommended approach:

- keep `GraphMiniMap` as the stock default control
- add a mini-map presenter/factory seam around overview rendering and viewport navigation
- keep the contract narrow; do not let it become a shell-status abstraction

## Likely Public API Shape

The exact names are still planner discretion, but the safe pattern is:

- surface-specific presenter contracts in `AsterGraph.Avalonia`
- stock presenter implementations next to them
- host-facing options in `Hosting/` that accept optional custom presenters
- `GraphEditorView` and standalone surface factories resolve stock defaults when presenters are omitted

This is safer than:

- forcing all custom presentation through one giant kit object
- or replacing the stock factories entirely

## Validation Implications

Phase 4 needs focused verification across four axes:

1. Node replacement keeps drag/selection/connection behavior intact
2. Menu replacement keeps editor intent and commands intact
3. Inspector replacement keeps editor projections and parameter editing intact
4. Mini-map replacement keeps viewport navigation intact

Additional proof points:

- `GraphEditorView` still works unchanged with no custom presenters
- standalone surface factories can accept custom presenters independently
- host sample demonstrates at least one custom replacement path
- package smoke emits machine-checkable markers for stock and replacement paths

## Risks To Plan Around

- Node presenter contract becoming too entangled with current `NodeVisual` internals
- Introducing replacement seams that accidentally bypass `INodePresentationProvider` and `IGraphContextMenuAugmentor`
- Breaking Phase 3’s default shell and standalone surface defaults while adding replacement paths
- Over-scoping into diagnostics/debug tooling that belongs in Phase 5

## Planning Guidance

The phase should likely split into 3-4 plans:

1. node visual presenter seam + focused tests
2. menu presenter seam + focused tests
3. inspector/minimap replacement seams + full-shell composition updates
4. host sample, package smoke, and docs

## Validation Architecture

### Quick Loop

- focused temp harness in `%TEMP%` or `$env:TEMP` that links only Phase 4 presenter tests
- avoid dependency on the unresolved workspace-local `GraphEditorViewTests.cs`

### Full Ring

- focused presenter harness
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`
- `dotnet build src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -v minimal`

## Conclusion

Phase 4 should be planned around **optional, per-surface presenter seams in `AsterGraph.Avalonia` with stock defaults preserved**, keeping `AsterGraph.Editor` as the owner of state, intent, and typed projections.
