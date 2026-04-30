# Phase 445 Context: Rendering And Viewport Performance Foundation

**Bead:** `avalonia-node-map-mqm.2`
**Branch:** `phase445-rendering-viewport`
**Worker:** 445
**Date:** 2026-04-30
**Status:** Passed

## Inputs Read

- Parent planning input: `.planning/ROADMAP.md`
- Parent planning input: `.planning/REQUIREMENTS.md`
- Parent planning input: `.planning/phases/444-desktop-graph-library-capability-audit/444-CAPABILITY-AUDIT.md`

The Phase 445 worktree did not contain the inherited `.planning` directory at start. The requested inputs were read from the parent repository path and this phase directory was created locally in the Phase 445 worktree.

## Requirement Boundary

RENDER-01 requires measurable rendering, viewport, controls, minimap, and large-graph scene projection proof without a fallback rendering layer or unsupported graph-size claim.

## Chosen Slice

The smallest coherent implementation slice is:

- add a viewport visible-scene projection calculator with a machine-readable budget marker
- refresh a latest projection snapshot from the Avalonia scene host during scene rebuild, viewport transform, and connection render paths
- expose minimap cadence and budget markers from the existing hosted performance policy
- cover the contracts with focused tests in the touched test project

This does not introduce culling, virtualization, a new renderer, a compatibility layer, or a larger graph-size claim.
