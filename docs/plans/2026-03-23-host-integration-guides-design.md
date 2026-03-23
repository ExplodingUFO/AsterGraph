# Host Integration Guides Design

## Background

AsterGraph already exposes the core host integration seams required by external applications:

- `IGraphLocalizationProvider`
- `INodePresentationProvider`
- `ContextMenuContext.HostContext`
- `GraphEditorStyleOptions`

The current gap is not raw API capability. The gap is integration guidance. Hosts still need to infer:

- how to compose the four extension points together
- how to consume `HostContext` safely without brittle casts
- how to keep node presentation states visually consistent across products

This round focuses on maintainable host-facing guidance and a small ergonomics improvement without breaking the current package layering.

## Goals

- add one host-facing helper surface that keeps `AsterGraph.Editor` UI-framework-neutral
- expand `tools/AsterGraph.HostSample` into a runnable composition sample covering the four main host seams
- document node presentation guidance so multiple hosts do not drift into incompatible visual patterns
- keep implementation small, explicit, and low-coupling

## Non-Goals

- no Avalonia-specific types added to `AsterGraph.Editor`
- no new runtime plugin model
- no redesign of node presentation data contracts
- no business-specific menu model changes

## Constraints

- maintainability is preferred over compatibility
- public APIs touched in this round should receive Chinese XML comments
- new behavior changes must follow TDD
- implementation should remain package-boundary-safe:
  - `AsterGraph.Editor` stays framework-neutral
  - `AsterGraph.Avalonia` remains the rendering shell

## Options Considered

### Option A: Documentation only

Only add README pages and keep APIs unchanged.

Pros:

- lowest implementation risk
- no public-surface growth

Cons:

- host code still needs repeated type checks and casting boilerplate
- runnable sample remains weaker than the documented capabilities

### Option B: Recommended

Add framework-neutral host context helper extensions, expand the runnable host sample, and add two focused docs.

Pros:

- best balance of ergonomics and layering
- gives hosts a copyable, runnable integration reference
- keeps UI types out of editor-layer contracts
- lets docs standardize presentation usage without freezing product-specific semantics

Cons:

- slightly larger surface area than docs-only
- requires smoke updates and helper tests

### Option C: Add Avalonia-centric host APIs

Expose `Window`-style properties or Avalonia helpers directly on the editor-layer contracts.

Pros:

- easiest for current Avalonia hosts

Cons:

- breaks framework-neutral layering
- makes future non-Avalonia hosts harder to support
- tightly couples editor contracts to one UI stack

## Chosen Design

Choose Option B.

### 1. Host context helper API

Add framework-neutral extension methods in `AsterGraph.Editor.Hosting`:

- `TryGetOwner<T>(this IGraphHostContext? hostContext, out T? owner)`
- `TryGetTopLevel<T>(this IGraphHostContext? hostContext, out T? topLevel)`
- `TryGetOwner<T>(this ContextMenuContext context, out T? owner)`
- `TryGetTopLevel<T>(this ContextMenuContext context, out T? topLevel)`

These helpers:

- keep the contract typed by caller demand instead of hard-coding UI classes
- reduce repeated host-side `is` and null checks
- preserve the existing `HostContext` object model

### 2. Runnable host sample

Upgrade `tools/AsterGraph.HostSample` from a narrow smoke sample into a host composition sample that demonstrates:

- a custom `IGraphLocalizationProvider`
- a custom `INodePresentationProvider`
- use of `ContextMenuContext.HostContext` together with the new helper API
- a customized `GraphEditorStyleOptions`

The sample remains console-driven and lightweight, but its output will explicitly prove that the integration hooks are active.

### 3. Documentation

Add:

- `docs/host-integration.md`
- `docs/node-presentation-guidelines.md`

Also update the root `README.md` to point to the deeper integration guide and sample host.

The node presentation guide will standardize recommendation-level usage, not enforce business semantics.

Recommended guidance:

- top-right badges:
  - recommended count: `0-3`
  - if more than `3`, aggregate before display
  - concise labels only
- status bar:
  - recommended label length: short operational summary
  - overflow should move into tooltip, not expand card semantics
- tooltip text:
  - explain state, reason, and next action where useful
  - avoid repeating badge/status text verbatim when no extra value is added

## Data Flow

Host composition flow after this round:

1. Host creates style options, localizer, and presentation provider.
2. Host constructs `GraphEditorViewModel`.
3. Host creates menu augmentation logic.
4. Menu handlers read typed owner/top-level objects through the helper extensions.
5. Node cards render host-provided presentation descriptors using the existing editor-to-Avalonia boundary.

## Testing Strategy

- add editor-layer tests for the new host helper extensions
- keep existing editor and serialization suites green
- verify the host sample output covers all intended seams
- keep package smoke green

## Risks

- host sample can become too product-like if it grows beyond composition guidance
- helper methods must stay small and avoid hidden runtime dependencies
- documentation can drift if not linked from the root README

## Mitigations

- keep helpers as thin extension methods only
- keep sample output explicit and stable
- keep docs narrowly scoped to integration and presentation guidance
