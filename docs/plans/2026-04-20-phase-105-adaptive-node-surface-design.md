# Phase 105 Adaptive Node Surface Design

## Summary

Phase 105 should make node surface sizing and disclosure content-driven instead of presenter-hardcoded. The default node size must fully cover the base chrome: title, all visible required inputs, outputs, and status chrome. Extra content such as defaultable inputs, value summaries, and inline editors should appear only after width or height crosses automatically measured breakpoints.

The system should not rely on hand-authored per-node pixel thresholds. It should derive breakpoints from definition facts plus a shared measurement policy, so new node kinds remain maintainable and the Avalonia presenter stays a renderer instead of becoming a policy god object.

## Approved Constraints

1. Default width and height only guarantee the base chrome and must not pre-reserve side rails or inline editors.
2. Height can unlock more endpoint rows, such as defaultable inputs that do not need to be visible in the compact baseline.
3. Width can unlock richer content in stages, starting from labels only, then value summaries, then inline editors.
4. Nodes should "auto decide" through definition-driven measurement, not through ad hoc presenter conditionals.
5. Code must stay split into small, role-focused types.

## Recommended Architecture

### 1. Definition Facts, Not Pixels

Keep author-owned metadata in the definition layer and keep it semantic:

- whether an input is required for compact authoring visibility
- whether an input has a default value path
- whether an input can expose an inline editor
- whether the node can ever show a side rail
- whether status chrome is expected

Do not store raw width and height thresholds in most definitions. Allow rare overrides only as escape hatches for special nodes.

### 2. Surface Planning Layer

Introduce an internal planner that converts definition facts plus projected node state into a renderable plan:

- `NodeSurfaceContentPlan`
- `NodeSurfaceMeasurement`
- `NodeSurfaceBreakpointSet`
- `NodeSurfaceDisclosurePlan`

This layer belongs in editor/runtime projection, not in Avalonia. It decides:

- baseline content rows
- expandable content rows
- width breakpoints for summary and editor disclosure
- height breakpoints for additional endpoint groups
- minimum/default size for the current node

### 3. Tier Resolution by Measured Breakpoints

Replace the current mostly profile-sized tier behavior with measured adaptive disclosure:

- baseline tier: required inputs + outputs + status
- summary tier: extra width can show richer endpoint summaries
- editor tier: enough width can show inline editors in the side rail
- expanded-height tier: enough height can show defaultable input rows

The active tier should be resolved from current node size against measured breakpoints, not from arbitrary presenter checks.

### 4. Avalonia as Renderer

`DefaultGraphNodeVisualPresenter` should consume the precomputed disclosure plan and only decide how to draw:

- which port rows are visible
- whether the side rail exists
- whether side rail sections show summary-only or real editors
- rendered height for the current content plan

It should not decide business rules such as "required inputs first" or "summary before editor". Those belong to the planner.

## Proposed Type Split

### Abstractions / definition seam

- `NodeSurfaceAuthoringRole`
  - `RequiredInput`
  - `DefaultableInput`
  - `Output`
- `NodeInlineEditorCapability`
  - `None`
  - `Summary`
  - `Editor`
- `NodeSurfaceLayoutHints`
  - optional fallback overrides only for exceptional cases

These can live beside existing definition/parameter metadata, but should stay small and semantic.

### Editor/runtime seam

- `GraphEditorNodeSurfacePlanner`
  Builds a `NodeSurfaceContentPlan` from node definition + projected node state.

- `GraphEditorNodeSurfaceMeasurer`
  Computes:
  - `DefaultSize`
  - `MinimumSize`
  - `HeightBreakpoints`
  - `WidthBreakpoints`

- `GraphEditorNodeSurfaceDisclosureResolver`
  Chooses what is visible for the current node size.

- `GraphEditorNodeSurfaceSnapshot`
  Can carry the computed active disclosure/tier snapshot if needed by hosts.

### Avalonia seam

- `DefaultGraphNodeVisualPresenter`
  Renders the resolved plan.

- Optional helper:
  - `NodeSurfaceRailRenderer`
  - `NodeSurfacePortListRenderer`

These should only translate plan -> controls.

## Measurement Rules

### Default Height

Default height should be measured to fit:

- header/title/subtitle chrome
- all required input rows
- all output rows
- status bar if active
- body paddings and row spacing

Defaultable inputs are excluded from the baseline unless the definition marks them as compact-required.

### Height Breakpoints

Height breakpoints should be derived from row groups:

- `H0`: baseline rows only
- `H1`: baseline + defaultable input rows
- optional future `H2`: baseline + defaultable rows + additional explanations

This keeps the vertical expansion rule deterministic and easy to test.

### Default Width

Default width should fit:

- port dot
- port label
- required type/status chrome
- title/header chrome

It should not fit summary pills or inline editors by default.

### Width Breakpoints

Width breakpoints should unlock richer disclosure in order:

- `W0`: labels only
- `W1`: value summaries / connection summaries
- `W2`: inline editors in side rail

The thresholds should be measured from actual endpoint content needs, including the minimum editor host width where relevant.

## Data Flow

1. `NodeDefinition` and parameter/endpoint metadata describe authoring facts.
2. Runtime projection builds a `NodeSurfaceContentPlan`.
3. Measurer derives default size + breakpoints.
4. Tier/disclosure resolver compares current node size to measured breakpoints.
5. `NodeViewModel.ActiveSurfaceTier` or a successor disclosure snapshot exposes the active result.
6. Avalonia renders from the resolved disclosure plan.

## Why This Is Maintainable

1. New node kinds mostly change metadata, not presenter code.
2. Threshold logic is centralized in one planner/measurer path.
3. Rendering remains decoupled from authoring policy.
4. Behavior is easy to regression-test without booting the full UI.
5. Future richer tiers can be added without rewriting the baseline sizing model.

## Risks And Boundaries

1. Do not let every definition hand-author raw pixel thresholds, or the system will fragment.
2. Do not encode policy inside Avalonia controls.
3. Do not make phase 105 solve non-obscuring inspector behavior; that belongs to phase 106.
4. Do not widen public API more than needed; prefer internal planners first, then expose only what host surfaces actually need.

## Phase 105 Success Shape

When phase 105 is done:

- default node size fully covers visible required inputs, outputs, and status chrome
- increasing height reveals defaultable endpoint groups without clipping
- increasing width reveals summary content and then inline editors in stable stages
- connected inputs suppress competing inline value editors
- presenter logic becomes thinner, with measurement and disclosure decided before rendering
