# Demo Showcase Design

**Date:** 2026-04-08
**Status:** Approved for milestone framing

## Goal

Reframe `AsterGraph.Demo` as a graph-first SDK showcase where the first thing a user sees is the node graph plus a host-level menu, not a wall of capability cards and side-panel explanations.

## Problem

The current demo proves many capabilities, but its information architecture is backwards for first-read understanding:

- the graph is visually secondary to the surrounding explanation panels
- host-level controls are fragmented across multiple side cards
- runtime and seam proof is present, but too explanation-heavy for a fast first impression

This makes the demo feel like an internal capability console instead of a deliberate SDK showcase.

## Approaches Considered

### 1. Top Menu + Drawer Panels

Use a host-level top menu as the first navigation layer, keep the graph dominant, and reveal compact grouped controls in drawers or flyouts.

**Pros**
- best match for the desired first impression
- clearly communicates "host controls the SDK"
- keeps view, behavior, and runtime controls available without permanent side clutter

**Cons**
- needs careful grouping to stay discoverable

### 2. Ribbon / Toolbar-Heavy Shell

Expose most controls in a visible grouped toolbar above the graph.

**Pros**
- strong desktop-tool feel
- high discoverability

**Cons**
- consumes too much vertical space
- weakens graph-first emphasis

### 3. Floating Debug Panels

Make the graph nearly full-screen and overlay floating control panels.

**Pros**
- maximizes graph visibility
- easy to prototype

**Cons**
- reads as a debug UI instead of a host-driven showcase
- weaker menu identity

## Approved Direction

Use **Top Menu + Drawer Panels**.

The new demo should behave like a host-driven showcase shell:

- a host-level menu is always visible first
- the active node graph remains the primary visual surface
- deeper controls open on demand in compact grouped surfaces
- every control acts on the same live `Editor` / `Session`

## Information Architecture

### Primary Layout

- top host menu
- large central graph surface
- compact secondary proof surfaces only when needed

### Menu Groups

- **View**: header/library/inspector/status visibility, chrome mode, layout density, theme-affecting host options if added
- **Behavior**: read-only, snapping, guides, menu extensions, command availability slices that are meaningful in the demo
- **Runtime**: live graph/session-facing toggles and compact state readouts
- **Diagnostics / Proof**: current configuration, seam ownership hints, and selected runtime signals

### Proof Strategy

Replace long explanatory cards with short in-context evidence:

- small labels near the graph or controls
- compact current-state summaries
- clear wording about whether a change is host-owned, editor-owned, or runtime-observed

## Boundaries

This redesign should **not**:

- turn the demo into a fake end-user product
- add unrelated graph-editing features
- depend on scene switching to demonstrate capabilities
- rebuild the editor session for ordinary UI toggles

## Milestone Implications

This design implies a milestone focused on:

1. graph-first shell restructuring
2. compact host control menu consolidation
3. in-context proof and narrative cleanup

## Success Signals

- A first-time user immediately sees the graph and host-level menu.
- View, behavior, and runtime controls are all discoverable from the host menu without side-panel overload.
- Users can tell which parts are host-controlled seams and which parts belong to the shared editor/runtime boundary.
- The same live graph session remains active while showcase controls change the experience.
