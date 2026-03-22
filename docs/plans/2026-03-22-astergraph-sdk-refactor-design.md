# AsterGraph SDK Refactor Design

**Date:** 2026-03-22
**Status:** Approved design for the next refactor pass

## Goal

Refactor the current node-graph demo into a cleaner, reusable SDK-style structure named `AsterGraph`, with:

- lower-collision product naming
- stronger separation between contracts, core rules, editor state, UI, and demo content
- first-class compile-time C# extensibility for custom algorithms
- a compatibility model based on strict types plus a small set of safe implicit conversions
- clearer code comments and developer-facing documentation

## Naming

### Solution and project names

The codebase will move from the current `NodeMap.*` naming to:

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`
- `AsterGraph.Demo`

### Public UI names

The public Avalonia control surface should use editor-oriented naming rather than demo-oriented naming:

- `NodeEditorView` -> `GraphEditorView`
- `NodeEditorViewModel` -> `GraphEditorViewModel`

This keeps the public surface aligned with a reusable product rather than a proof-of-concept sample.

## Architecture

### `AsterGraph.Abstractions`

This project contains the most stable extension contracts and identifiers:

- `NodeDefinitionId`
- `PortTypeId`
- `ConversionId`
- `INodeDefinition`
- `INodeDefinitionProvider`
- `INodeCatalog`
- `IPortCompatibilityService`
- immutable port-definition records

Future runtime plugin loading should depend on this layer, but the plugin loader itself is explicitly out of scope for this pass.

### `AsterGraph.Core`

This project contains pure data and default domain logic:

- graph document models
- node instance models
- connection models
- serialization
- strict compatibility checks
- safe implicit conversion rules

This layer must remain UI-agnostic.

### `AsterGraph.Editor`

This project contains editor state and orchestration:

- graph editor state/view model
- node definition catalog
- selected node / pending connection state
- node instantiation from definitions
- save/load workflow
- inspector-derived data

This layer may depend on MVVM tooling, but must not depend on Avalonia.

### `AsterGraph.Avalonia`

This project contains Avalonia-specific UI:

- `GraphEditorView`
- graph canvas control
- themes and styling
- input handling
- real-control-based anchor resolution

It should depend on `Editor` and `Abstractions`, not on demo-specific node content.

### `AsterGraph.Demo`

This project is only a host and sample content registration:

- sample node-definition provider(s)
- sample graph document seed
- demo shell window

The demo proves the extension flow but should not carry framework responsibilities.

## Extension Model

### Split definitions from instances

The design intentionally separates:

- **node definition**
  What a node type is
- **node instance**
  One graph-local occurrence of that definition

Each node definition should describe:

- stable definition ID such as `aster.math.add`
- display name
- category
- description
- input port definitions
- output port definitions
- default size
- optional parameter schema

Each node instance should store:

- instance ID
- `DefinitionId`
- current position
- current size
- parameter values

This makes compile-time registration straightforward and keeps runtime plugin loading viable later.

### Compile-time C# extension

The primary extension mechanism for this pass is:

1. create an `INodeDefinitionProvider` in a consumer library
2. return one or more `INodeDefinition` values
3. register the provider into the editor catalog at startup

This supports multiple algorithm libraries such as:

- `AsterGraph.Algorithms.Math`
- `AsterGraph.Algorithms.Image`
- `AsterGraph.Algorithms.Signal`

### Future plugin seam

This pass should only reserve the seam for later dynamic loading by:

- keeping contracts in `AsterGraph.Abstractions`
- keeping definition registration explicit and provider-based
- avoiding editor code that assumes definitions are compiled into the same assembly

## Type Compatibility and Safe Implicit Conversion

### Default rule

Connections are rejected unless the target input accepts:

- the exact same `PortTypeId`, or
- a registered safe implicit conversion

### Safe implicit conversions in scope

The initial conversion set should be intentionally small and unsurprising:

- `int -> float`
- `int -> double`
- `float -> double`

Everything else should default to rejection unless explicitly registered.

### Rejected by default

Do not add implicit conversions with semantic loss or ambiguity, for example:

- `color -> float`
- `string -> number`
- `bool -> float`

### Compatibility result model

Compatibility should be modeled as a result, not just a boolean:

- `Exact`
- `ImplicitConversion`
- `Rejected`

When a connection uses `ImplicitConversion`, the serialized connection should carry a `ConversionId`.

This supports:

- correct save/load behavior
- inspector visibility for conversion-backed edges
- future extension by plugin-defined rules

## Documentation Strategy

### Top-level README

Add a real root `README.md` that covers:

- project overview
- current feature set and non-goals
- solution structure
- quick start
- how to add a custom node-definition provider
- compatibility and implicit-conversion policy
- serialization behavior
- roadmap

### Library README files

Add focused README files only where they provide strong value:

- `src/AsterGraph.Abstractions/README.md`
- `src/AsterGraph.Avalonia/README.md`

The goal is to help SDK consumers, not to create redundant documentation in every folder.

## Comment Strategy

Comments should explain architectural intent, not syntax. Priority areas:

- public extension interfaces
- compatibility and conversion rules
- graph-canvas anchor logic
- serialization contracts that should remain stable

Avoid line-by-line narrative comments that merely restate code.

## Refactor Scope

### In scope

- rename solution/projects/namespaces to `AsterGraph`
- introduce `AsterGraph.Abstractions`
- introduce `AsterGraph.Editor`
- move demo node content out of core where appropriate
- centralize compatibility and implicit conversion rules
- document public extension points
- add root README and key library README files
- add targeted architecture comments

### Out of scope

- runtime plugin loader
- algorithm execution engine
- undo/redo redesign
- large visual redesign
- broad new feature work unrelated to the SDK boundary

## Expected Outcome

After this refactor, the repository should look like a reusable SDK with a demo host, rather than a demo-first app with reusable pieces hidden inside it. New algorithm nodes should be straightforward to add in compile-time C#, and the type system should be explicit, documented, and extensible.
