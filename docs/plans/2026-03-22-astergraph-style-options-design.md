# AsterGraph Style Options Design

**Date:** 2026-03-22
**Status:** Approved design for a unified style configuration entry point

## Goal

Add a maintainable style customization system to AsterGraph that supports both:

- high-level appearance theming
- component-specific visual customization for nodes, ports, connections, inspector, and context menus

The design must prioritize:

- maintainability
- MVVM-friendly boundaries
- framework decoupling
- stable public configuration contracts

## Recommendation

Use a style configuration model plus an Avalonia-side style adapter.

This means:

- `AsterGraph.Abstractions` defines the public style option models
- `AsterGraph.Editor` carries the selected style options as part of editor state
- `AsterGraph.Avalonia` maps those options into actual Avalonia resources and control visuals

This avoids leaking Avalonia-specific types into the public SDK surface.

## Public Style Entry Point

Expose one stable root configuration:

- `GraphEditorStyleOptions`

This root object should contain typed sub-options rather than a flat bag of values.

## Style Subsections

### `ShellStyleOptions`

Stable tokens for global shell look:

- shell background
- panel background
- panel border
- headline/body/eyebrow/highlight colors
- default shell radius
- default shell spacing

### `CanvasStyleOptions`

Tokens for graph surface styling:

- canvas background
- primary grid color
- secondary grid color
- grid spacing
- canvas frame border

### `NodeCardStyleOptions`

Tokens for node card rendering:

- background
- selected background
- border
- selected border
- header tint intensity
- default width
- default height
- radius
- card padding

### `PortStyleOptions`

Tokens for ports:

- dot size
- port label color
- type label color
- row spacing
- hover/selected accent behavior

### `ConnectionStyleOptions`

Tokens for edges:

- line thickness
- preview thickness
- line opacity
- label background
- label border
- conversion marker styling

### `InspectorStyleOptions`

Tokens for inspector sections:

- panel background
- card background
- input control background
- validation error color
- section spacing

### `ContextMenuStyleOptions`

Tokens for menu visuals:

- menu background
- menu border
- hover background
- disabled foreground
- submenu indentation/icon gutter

## Placement By Layer

### `AsterGraph.Abstractions`

This layer owns:

- `GraphEditorStyleOptions`
- all typed sub-option records
- any optional style override records tied to public identifiers

It should use framework-neutral values only, such as:

- strings for colors
- numeric sizes
- spacing values
- booleans

It must not expose:

- Avalonia `Brush`
- Avalonia `Thickness`
- Avalonia `CornerRadius`
- Avalonia control/template references

### `AsterGraph.Editor`

This layer owns:

- current style selection/reference
- propagation into editor state if needed

It should not translate style data into Avalonia types.

### `AsterGraph.Avalonia`

This layer owns:

- mapping style options into actual Avalonia resources
- applying resources to controls
- fallback defaults when no host-supplied overrides exist

This is where a style adapter or resource-builder belongs.

## Node-Level And Type-Level Overrides

To support more than global themeing while keeping the system maintainable, allow optional targeted overrides:

- `NodeStyleOverride`
  - keyed by `NodeDefinitionId`
- `PortStyleOverride`
  - keyed by `PortTypeId` or direction
- `ConnectionStyleOverride`
  - keyed by `ConversionId` or connection kind

This keeps the base theme simple while still allowing certain node categories or conversion-backed edges to stand out.

## What Should Stay Public

Public stable tokens should include:

- colors
- sizes
- spacing
- radii
- line widths
- basic text sizing
- simple state colors

## What Should Stay Internal

Do not expose these in the public contract:

- exact Avalonia resource names
- specific `Border` / `TextBlock` / `Canvas` implementation details
- template part names
- low-level control-specific exceptions

In short:

- public contracts describe design tokens
- Avalonia internals decide how to render them

## Maintainability Rules

- keep the public style model small and typed
- prefer additive sub-records over dozens of loose properties
- prefer global theme plus optional override maps over one-off flags everywhere
- do not encode visual logic in the editor layer
- keep default values centralized

## Expected Outcome

After this pass, AsterGraph should have:

- one obvious style configuration entry point
- support for both global theming and targeted component overrides
- a public style contract that is stable and framework-neutral
- an Avalonia implementation that can evolve without breaking the host-side style API
