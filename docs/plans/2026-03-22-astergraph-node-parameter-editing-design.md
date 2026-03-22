# AsterGraph Node Parameter Editing Design

**Date:** 2026-03-22
**Status:** Approved design for decoupled MVVM-based node content editing

## Goal

Add editable node content to AsterGraph in a way that is:

- decoupled across abstraction, core, editor, and Avalonia layers
- compatible with MVVM
- extensible for future custom algorithm nodes
- able to support enum parameters as first-class citizens

## Scope

This pass covers:

- parameter schema definition
- parameter instance values
- editor-state view models for parameter editing
- Inspector-based parameter editing UI
- validation and status flow

This pass does not cover:

- runtime plugin loading
- full parameter dialog systems
- undo/redo
- algorithm execution

## Architecture

### `AsterGraph.Abstractions`

This layer defines what a node parameter can be.

`NodeParameterDefinition` should evolve to describe:

- `Key`
- `DisplayName`
- `TypeId`
- `EditorKind`
- `DefaultValue`
- `Description`
- optional constraints
  - min/max
  - allowed values
  - read-only

For enum support, definitions should also support a stable list of options rather than hard-coding C# enum reflection into the UI.

### `AsterGraph.Core`

This layer defines what the current parameter value is for a node instance.

Each graph node instance should carry parameter values independently from its definition.

Recommended model:

- `GraphParameterValue`
  - `Key`
  - `TypeId`
  - `Value`

Then `GraphNode` stores:

- `ParameterValues`

This keeps definitions immutable and reusable while allowing instances to diverge.

### `AsterGraph.Editor`

This layer owns editing behavior.

Recommended editor types:

- `NodeParameterViewModel`
- `NodeParameterOptionViewModel`
- editor commands for applying parameter changes
- validation state per parameter

Responsibilities:

- merge `NodeDefinition.Parameters` with `GraphNode.ParameterValues`
- expose editable parameter view models for the selected node
- enforce validation
- track dirty state
- update graph-node instance values

### `AsterGraph.Avalonia`

This layer only renders the editor state.

The Avalonia Inspector should bind to `NodeParameterViewModel` and choose controls by `EditorKind`.

It should not contain hard-coded business rules for parameter compatibility or storage.

## Enum Support

Enum parameters should be supported as a first-class schema concept.

### Recommended representation

Do not serialize instance values as CLR enum objects.

Instead:

- schema defines allowed values
- instance stores the selected value as a stable scalar, typically a string

Recommended schema data:

- `EditorKind = Enum`
- `AllowedValues = [value, label, description?]`

Example:

- `Key = "blendMode"`
- `TypeId = "enum"`
- options:
  - `add`
  - `multiply`
  - `screen`

This is safer for:

- serialization
- cross-assembly extension
- future plugin compatibility

## Editor Kinds

The first supported parameter editor kinds should be:

- `Text`
- `Number`
- `Boolean`
- `Enum`
- `Color`

These are enough to make the system practical without overbuilding.

## Inspector Rendering Strategy

The Inspector should be the primary editing surface.

Recommended control mapping:

- `Text` -> `TextBox`
- `Number` -> `TextBox` with numeric validation or a numeric up/down control if introduced later
- `Boolean` -> `CheckBox`
- `Enum` -> `ComboBox`
- `Color` -> string field first, richer picker later

The node card itself should continue to show summary information only. Heavy editing belongs in the Inspector, not in the node body.

## Update Model

Use MVVM commands or explicit update methods in the editor layer, not direct model mutation from the Avalonia controls.

Recommended behavior:

- simple values can update on change
- invalid values should remain visible but marked invalid
- successful edits update:
  - parameter instance value
  - dirty state
  - status message when useful

## Validation

Validation should be defined in the editor layer using definition metadata.

Examples:

- required text cannot be empty
- numeric values must parse
- numeric values must respect min/max if present
- enum values must exist in the allowed-value set

Validation should produce user-facing state that the Inspector can render without embedding rules in the UI.

## Why This Fits MVVM

- schema lives in `Abstractions`
- data lives in `Core`
- editing state and commands live in `Editor`
- controls in `Avalonia` only bind

That keeps the parameter system testable, layered, and extensible.

## Expected Outcome

After this pass, selecting a node should reveal editable parameters in the Inspector. Those parameters should be driven from node definitions, stored as instance values, validated in the editor layer, and rendered through Avalonia bindings without breaking architectural boundaries.
