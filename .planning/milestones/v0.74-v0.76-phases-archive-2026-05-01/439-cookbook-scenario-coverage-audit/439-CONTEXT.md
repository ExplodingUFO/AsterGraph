# Phase 439: Cookbook Scenario Coverage Audit - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Bead:** `avalonia-node-map-h3e.1`

<domain>
## Phase Boundary

Create a narrow coverage map for the professional cookbook surface before changing behavior. The phase should classify active cookbook recipes across graph/demo/code/docs/proof posture, identify intentionally deferred gaps, and produce executable targets for later phases.

This phase is audit/planning work only. It should not add a tutorial CMS, code execution path, fallback layer, broad compatibility route, or runtime architecture change.
</domain>

<decisions>
## Implementation Decisions

- Use beads as the task spine: parent bead `avalonia-node-map-h3e.1`; child beads `h3e.1.1`, `h3e.1.2`, and `h3e.1.3` split catalog, UI, and docs/proof audit responsibilities.
- Use isolated worktrees/branches for parallel audit lanes:
  - `agents/avalonia-node-map-h3e-1-1-catalog`
  - `agents/avalonia-node-map-h3e-1-2-ui`
  - `agents/avalonia-node-map-h3e-1-3-docs-proof`
- Keep subagents read-only. Main thread owns the final matrix and bead closure.
- Treat current cookbook code as evidence to classify, not as a target for Phase 439 production edits.
</decisions>

<code_context>
## Existing Code Insights

- `DemoCookbookRecipe` stores recipe identity, category, code anchors, demo anchors, documentation anchors, scenario points, proof markers, and support boundary.
- `DemoCookbookCatalog` validates required categories, required scenario kinds, anchor presence, and scenario evidence tie-back.
- `DemoCookbookWorkspaceProjection` projects recipes into navigation groups, graph anchors, code examples, docs links, scenario points, route posture, deferred gaps, and support boundary.
- `DemoCookbookProof` validates cookbook contract, route coverage, visual hierarchy, navigation feedback, detail readability, interaction states, professional interaction, scenario depth, and ownership boundaries.
- The Avalonia UI preserves the live graph host above the cookbook detail/code panel.
</code_context>

<specifics>
## Specific Ideas

- Produce a matrix with one row per active recipe and columns for graph posture, demo posture, code posture, docs posture, proof posture, route status, support boundary, deferred gaps, scenario kinds, validating tests, and next-phase target.
- Separate "supported SDK route" from "proof/demo route" and "Demo-only scaffolding" when classifying code anchors.
- Treat scenario-linked graph cues as a Phase 440 target because scenario points are currently textual and not independently selectable.
- Treat source route clarity as a Phase 441 target because code anchors exist but are not classified by supported SDK/sample/demo-only posture.
- Treat proof/docs marker alignment as a Phase 443 target because Demo proof emits cookbook markers but demo-guide docs do not list the full cookbook proof closure set.
</specifics>

<deferred>
## Deferred Ideas

- No generated runnable code snippets inside the Demo.
- No macro/query/scripting engine.
- No runtime marketplace, remote plugin distribution, sandboxing, or WPF parity expansion.
- No broad public API change in Phase 439.
</deferred>
