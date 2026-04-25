# AsterGraph

## What This Is

AsterGraph is a modular .NET graph editor SDK organized around an editor kernel, a scene/interaction layer, and UI adapters. Avalonia is the shipped adapter today; the product direction is to turn the current desktop prerelease beta line into a platform-grade embedded graph editor for .NET hosts without collapsing host/runtime boundaries.

## Core Value

External hosts can embed AsterGraph and get a host-native, definition-driven authoring experience where commands, editing, inspection, trust decisions, and semantic workflows feel product-complete without rebuilding the toolkit.

## Current State

- `v1.17 Host-Native Authoring UX` is archived locally as complete.
- `v1.18 Progressive Node Surface` is archived locally as complete.
- `v1.19 Adaptive Node Group Bounds` is archived locally as complete.
- `v1.20 Tiered Surface Layout` is archived locally as complete.
- `v1.21 Semantic Graph Composition` is archived locally as complete.
- `v1.22 Canvas Interaction Stabilization` is archived locally as complete.
- `v1.23 Graph Surface Usability Convergence` is archived locally as complete.
- `v1.24 Interaction Feedback And Port-Local Authoring` is archived locally as complete.
- `v1.25 Command Pipeline, Inspector Deepening, And Semantic Authoring` is archived locally as complete.
- `v0.3.0-alpha Platform Skeleton Freeze` is archived locally as complete.
- `v0.4.0-alpha Official Capability Modules` is archived locally as complete.
- `v0.4.0-alpha` closed with shipped proof still green and with explicitly accepted process debt for missing local phase-local closeout artifacts under `.planning/phases/125-133`.
- `v0.5.0-alpha Performance Gate` is archived locally as complete.
- `v0.6.0-alpha Advanced Graph Editing` is complete locally.
- `v0.7.0-alpha Custom Model And Native UX` is complete locally.
- `v0.8.0-alpha DX, Docs, And Devtools` is archived locally as complete.
- `v0.9.0-beta Second Adapter Validation` is archived locally as complete.
- `v0.10.0-beta SDK Stabilization` is archived locally as complete.
- `v0.11.0-beta Defended Surface Hold` is archived locally as complete.
- `v0.12.0-beta Adopter Validation Loop` is archived locally as complete.
- `v0.13.0-beta Externally Validated Platform Capability` is archived locally as complete.
- `v0.14.0-beta Supported Route Friction Closure` is archived locally as complete.
- `v0.15.0-beta Copyable Hosted Recipes` is archived locally as complete.
- `v0.16.0-beta Retained Migration Recipes` is archived locally as complete.
- `v0.19.0-beta Adoption Evidence Cohesion` is archived locally as complete.
- `v0.20.0-beta Evidence Loop Durability` is archived locally as complete.
- `v0.21.0-beta Evaluation Path Friction Closure` is archived locally as complete.
- `v0.22.0-beta Copyable Host-Owned Parameter And Metadata Editing Polish` is archived locally as complete.
- `v0.23.0-beta Parameter And Metadata Copyability Proof Closure` is archived locally as complete.
- `v0.24.0-beta Real Adopter Intake Conversion` is archived locally as complete.
- `v0.25.0-beta Host-Owned Parameter/Metadata Adopter Proof Polish` is archived locally as complete.
- `v0.26.0-beta Parameter/Metadata Intake Visibility` is archived locally as complete.
- `v0.27.0-beta External Intake Dry Run` is archived locally as complete.
- `v0.28.0-beta Public Verification Loop` is archived locally as complete.
- `v0.29.0-beta Authoring Surface Polish` is archived locally as complete.
- The widened canonical route now has defended breadth, performance, and automation-backed accessibility validation; the next active bottleneck is whether the validation-only second adapter can reproduce the shared tool and stencil slice of that capability-breadth contract without widening support promises.

## Current Milestone

**v0.38.0-beta External Adopter Feedback Loop**

**Goal:** Turn the hardened `0.12.0-beta` authoring-surface line into an active feedback-collection and rapid-response loop. Gather real adopter signals through support-bundle intake, issue templates, and direct host evaluations; triage and fix friction points in documentation, samples, and the canonical route before committing to the next capability wave.

**Target features:**
- **Active Intake Channel:** Support-bundle format and issue template are actively used to collect structured adopter reports; target is 3–5 real external reports with explicit intake criteria.
- **Feedback-Driven Polish:** Rapid iteration on docs, samples, and onboarding based on the first batch of real adopter friction reports.
- **Trust and Telemetry Hardening:** Improve support-bundle fidelity (environment, metrics, reproduction steps) so external reports are actionable without back-and-forth.
- **Documentation Gap Closure:** Fix any "copy-from-here" gaps discovered by real hosts attempting to replicate the ladder.

**Versioning note:** local planning uses `v0.38.0-beta` as the next internal beta label; this milestone is intentionally bounded and feedback-driven, not a new capability wave.

## Most Recently Completed Milestone

**v0.37.0-beta Authoring Surface Polish** (completed locally 2026-04-25)

Shipped the public `0.12.0-beta` authoring-surface line: parameter/metadata contract, custom node/port/edge authoring, host copyability recipes, and performance guardrails.

## Release Ladder

1. **v0.3.0-alpha — Platform Skeleton Freeze**
   - Freeze canonical session/runtime entry points, establish scene/input/platform seams, and harden Avalonia as adapter 1.
2. **v0.4.0-alpha — Official Capability Modules**
   - Ship `Selection / History / Clipboard / Keyboard / Snapline / MiniMap / Stencil / Export` as explicit capability modules plus baseline edge authoring affordances.
3. **v0.5.0-alpha — Performance Gate**
   - Turn 1000-node performance into a defended release gate and publish trustworthy 5000-node telemetry.
4. **v0.6.0-alpha — Advanced Graph Editing**
   - Ship routers, connectors, anchors, port locators, collapse/expand, parent/child graph relations, and richer node/edge tools.
5. **v0.7.0-alpha — Custom Model And Native UX**
   - Ship `NodeTemplate / EdgeTemplate / ToolProvider`, hybrid draw-first node composition, and stronger desktop-native interaction/accessibility behavior.
6. **v0.8.0-alpha — DX, Docs, And Devtools**
   - Ship devtools overlays, cookbook-quality docs, project templates, migration guides, and a clean authoring path from empty app to custom graph editor.
7. **v0.9.0-beta — Second Adapter Validation**
   - Validate one non-Avalonia adapter, publish capability matrix/fallback rules, and prove host code portability across adapters.
8. **v0.10.0-beta — SDK Stabilization**
   - Freeze the candidate prerelease API, define compatibility/deprecation policy, finalize pre-1.0 serialization/plugin/trust contracts, and publish defended support guarantees.
9. **v0.11.0-beta — Defended Surface Hold**
   - Hold the defended stabilization surface on a prerelease beta line while the contracts and support guarantees stay coherent.
10. **v0.12.0-beta — Adopter Validation Loop**
   - Turn the defended beta surface into an externally repeatable evaluation, support-bundle, and feedback-triage loop before any stable-readiness decision.
11. **v0.13.0-beta — Externally Validated Platform Capability**
   - Convert the defended beta evidence loop into reproducible external validation, evidence-led capability claims, and next-step criteria without widening scope.
12. **v0.14.0-beta — Supported Route Friction Closure**
   - Reduce friction on the supported hosted and retained routes before widening capabilities, adapters, or support guarantees.
13. **v0.15.0-beta — Copyable Hosted Recipes**
   - Make the defended hosted route easier to copy into real hosts before widening the platform surface again.

## Archived Milestones

- [v0.4.0-alpha Official Capability Modules roadmap](./milestones/v0.4.0-alpha-ROADMAP.md)
- [v0.4.0-alpha Official Capability Modules requirements](./milestones/v0.4.0-alpha-REQUIREMENTS.md)
- [v0.5.0-alpha Performance Gate roadmap](./milestones/v0.5.0-alpha-ROADMAP.md)
- [v0.5.0-alpha Performance Gate requirements](./milestones/v0.5.0-alpha-REQUIREMENTS.md)
- [v0.8.0-alpha DX, Docs, And Devtools roadmap](./milestones/v0.8.0-alpha-ROADMAP.md)
- [v0.8.0-alpha DX, Docs, And Devtools requirements](./milestones/v0.8.0-alpha-REQUIREMENTS.md)
- [v0.11.0-beta Defended Surface Hold roadmap](./milestones/v0.11.0-beta-ROADMAP.md)
- [v0.11.0-beta Defended Surface Hold requirements](./milestones/v0.11.0-beta-REQUIREMENTS.md)
- [v0.12.0-beta Adopter Validation Loop roadmap](./milestones/v0.12.0-beta-ROADMAP.md)
- [v0.12.0-beta Adopter Validation Loop requirements](./milestones/v0.12.0-beta-REQUIREMENTS.md)
- [v0.13.0-beta Externally Validated Platform Capability roadmap](./milestones/v0.13.0-beta-ROADMAP.md)
- [v0.13.0-beta Externally Validated Platform Capability requirements](./milestones/v0.13.0-beta-REQUIREMENTS.md)
- [v0.14.0-beta Supported Route Friction Closure roadmap](./milestones/v0.14.0-beta-ROADMAP.md)
- [v0.14.0-beta Supported Route Friction Closure requirements](./milestones/v0.14.0-beta-REQUIREMENTS.md)
- [v1.25 Command Pipeline, Inspector Deepening, And Semantic Authoring roadmap](./milestones/v1.25-ROADMAP.md)
- [v1.25 Command Pipeline, Inspector Deepening, And Semantic Authoring requirements](./milestones/v1.25-REQUIREMENTS.md)
- [v1.24 Interaction Feedback And Port-Local Authoring roadmap](./milestones/v1.24-ROADMAP.md)
- [v1.24 Interaction Feedback And Port-Local Authoring requirements](./milestones/v1.24-REQUIREMENTS.md)
- [v1.23 Graph Surface Usability Convergence roadmap](./milestones/v1.23-ROADMAP.md)
- [v1.23 Graph Surface Usability Convergence requirements](./milestones/v1.23-REQUIREMENTS.md)
- [v1.22 Canvas Interaction Stabilization roadmap](./milestones/v1.22-ROADMAP.md)
- [v1.22 Canvas Interaction Stabilization requirements](./milestones/v1.22-REQUIREMENTS.md)
- [v1.21 Semantic Graph Composition roadmap](./milestones/v1.21-ROADMAP.md)
- [v1.21 Semantic Graph Composition requirements](./milestones/v1.21-REQUIREMENTS.md)
- [v1.20 Tiered Surface Layout roadmap](./milestones/v1.20-ROADMAP.md)
- [v1.20 Tiered Surface Layout requirements](./milestones/v1.20-REQUIREMENTS.md)
- [v1.19 Adaptive Node Group Bounds roadmap](./milestones/v1.19-ROADMAP.md)
- [v1.19 Adaptive Node Group Bounds requirements](./milestones/v1.19-REQUIREMENTS.md)
- [v1.18 Progressive Node Surface roadmap](./milestones/v1.18-ROADMAP.md)
- [v1.18 Progressive Node Surface requirements](./milestones/v1.18-REQUIREMENTS.md)
- [v1.17 Host-Native Authoring UX roadmap](./milestones/v1.17-ROADMAP.md)
- [v1.17 Host-Native Authoring UX requirements](./milestones/v1.17-REQUIREMENTS.md)

## Requirements

### Validated

- ✓ Public repo hygiene, bilingual docs, semver-aligned public proof flow, and four published package boundaries are established.
- ✓ Session-first runtime ownership and the canonical `CreateSession(...)` / `Create(...)` adoption routes are shipped.
- ✓ The platform skeleton now has adapter-neutral scene, input, hit-test, and platform-service seams with Avalonia hardened as adapter 1.
- ✓ Host-native command descriptors, shell workflows, trust transparency, and command-latency proof markers are shipped.
- ✓ Progressive node surfaces, user-owned group frames, tiered disclosure, and non-obscuring port-local inline authoring are shipped.
- ✓ Graph scopes, composite nodes, public boundary ports, scoped navigation, edge notes, and explicit disconnect semantics are shipped.
- ✓ Resize-hover feedback, live-resize stability, and group snap or alignment parity are shipped in the default Avalonia host family.
- ✓ Stock commands, host actions, and plugin contributions now share one executable command pipeline with host-governed authority.
- ✓ Menus, context menu, toolbar, host rail, keyboard shortcuts, and command palette project from one shared command source.
- ✓ The shipped definition-driven inspector now exposes mixed/default state, advanced disclosure, stable ordering, richer help, and clearer validation or read-only metadata.
- ✓ Composite and edge authoring workflows now ship as explicit semantic flows, including breadcrumbs, wrap-to-composite, expose or unexpose, note edit, disconnect, reconnect, and shared command routing.
- ✓ The developer solution entry is aligned to `AsterGraph.sln`, and proof contracts make command, inspector, and semantic workflow regressions obvious before release.
- ✓ The pre-1.0 line now has enough defended surface area that the next bottleneck is beta-hold closure rather than another shell-polish or capability-extraction cycle.
- ✓ `Selection` and `History` already exist as explicit runtime capability surfaces and should now serve as defended baselines rather than new feature targets.
- ✓ `Clipboard`, `Shortcut Policy`, `Layout`, `MiniMap`, `Stencil`, `Fragment Library`, `Export`, and baseline edge authoring are now explicit official capability modules on the canonical route.
- ✓ Public docs and proof lanes now describe and demonstrate the official module split coherently enough for host adopters to evaluate.
- ✓ `ScaleSmoke` now supports repeated sampling with a machine-readable `SCALE_PERF_SUMMARY` marker while staying on the existing public proof route.
- ✓ The 1000-node `large` ScaleSmoke tier now has an explicit defended budget contract instead of an informational-only marker.
- ✓ Release validation and prerelease proof reporting now execute and surface both defended `baseline` and defended `large` ScaleSmoke tiers.
- ✓ The 5000-node `stress` tier now publishes p50/p95 telemetry on the maintained proof path and in prerelease proof summaries without becoming a defended budget.
- ✓ Public beta docs now expose one explicit evaluation path from first-run sample to realistic hosted proof.
- ✓ `ConsumerSample.Avalonia` can now emit one structured local beta support bundle with route/version/proof/environment/reproduction context.
- ✓ Prerelease notes, issue intake, and maintainer triage now share one availability-aware beta evidence contract and bilingual checklist.
- ✓ The beta evidence contract now uses aligned route vocabulary across the evaluator ladder, adoption issue intake, and bilingual adoption-feedback docs.
- ✓ Project status and release closure now publish externally proven vs deferred capability claims from one evidence-led source of truth.
- ✓ Next-beta selection criteria are now documented as an explicit public contract instead of implicit maintainer judgment.
- ✓ Public entry docs and hosted sample READMEs now name one defended hosted route ladder instead of leaving the supported progression implicit.
- ✓ Plugin-capable evaluators now hit `ConsumerSample.Avalonia` as an explicit defended hosted trust hop with front-loaded trust-policy links.
- ✓ The retained `GraphEditorViewModel` / `GraphEditorView` route is now described as an explicit migration-only decision instead of a shadow primary path.
- ✓ Hosts can now consume one shared parameter or metadata contract across inspector, node-side editors, validation, badges, status cues, and command enablement without maintaining duplicate translators.
- ✓ The canonical route now has official copyable custom node, port, and edge authoring recipes, including multiple handles, resizable nodes, node-toolbar surfaces, and custom edges.
- ✓ Authoring actions now stay on one command descriptor and execution route across toolbar, context menu, shortcuts, palette, and host rails.
- ✓ `ConsumerSample.Avalonia` and release proof now fail loudly on authoring-surface regressions through dedicated markers and defended latency metrics.
- ✓ Hosts can now expose searchable stencil content on the shipped canonical route without custom discovery plumbing.
- ✓ The shipped canonical route now supports grouped and collapsible stencil sections with stable ordering and host-owned category labels.
- ✓ Hosts can now export `SVG`, `PNG`, and `JPEG` from one defended canonical route with a shared runtime contract, shared descriptor surface, and host-replaceable export services.
- ✓ Export breadth now supports practical output-shaping inputs through a small public options contract instead of sample-only helpers.
- ✓ The validation-only WPF route now emits bounded export-breadth proof through shared feature descriptors plus `svg/png/jpeg` export commands without widening adapter support claims.

### Active

- [ ] Parameter/metadata vocabulary (`defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, `constraints`, `groupName`) must ship as one complete contract driving inspector, selected-node seam, multi-selection batch editing, validation, read-only reason, and support-bundle evidence.
- [ ] Custom node, port, and edge authoring must move from "possible" to "easy" on the canonical route: multi-handle, port grouping/validation, resize affordance, quick tools, reconnect, temporary edge, delete-on-drop.
- [ ] The hosted route ladder (`Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`) must expose explicit "copy-from-here" paragraphs, plugin-host recipe, custom-node host recipe, and migration route comparison.
- [ ] Support-bundle / issue-template intake must be activated to target 3–5 real external adopter reports before widening performance claims or capability breadth.
- [ ] Authoring-interaction latency budgets and proof markers must be hardened; the 5000-node stress tier stays informational telemetry, not a defended public claim.

### Excluded from this milestone

- WPF parity work remains validation-only; adapter-2 is NOT a second onboarding route.
- Version/release narrative cleanup is noted but not a milestone headline.

### Deferred

- reusable composite templates with shared definitions and multiple instances
- executable edge semantics beyond authoring-time note editing, reconnect or reroute flows, and explicit disconnect
- pointer-only edge gestures such as delete-on-drop, drag-to-reconnect previews, and drop-to-empty disconnect
- scoped-reference validation for composite child graph ids, boundary-port targets, and per-scope connection endpoints
- larger defended performance budgets beyond the first 1000-node gate and 5000-node telemetry tier
- new UI stacks beyond the first shipped adapter and one follow-on adapter validation
- plugin marketplace, remote install/update workflows, unload lifecycle, or untrusted sandboxing
- browser-only concerns such as SSR, localStorage-style persistence, and multiplayer whiteboard features

## Context

- The public package line remains `0.11.0-beta`; local planning now uses `v0.37.0-beta` as the next internal beta label to deliver the public `0.12.0-beta` authoring-surface line.
- The current bottleneck is no longer adapter-2 validation or canonical route breadth; it is whether external hosts can comfortably copy, extend, and validate the authoring surface on the defended canonical route.
- Relative to mature graph platforms (X6, React Flow), the immediate gap is not another capability wave; it is low-friction author experience: searchable stencil grouping, unified export, custom nodes/handles/NodeResizer/NodeToolbar patterns, and clear copy-from-here recipes.
- Avalonia remains the first official adapter; `WPF` remains validation-only/partial and is intentionally not the driver of this milestone's runtime API changes.
- The most valuable near-term execution order is: parameter/metadata contract completion, custom node/port/edge authoring affordances, host copyability recipes and intake activation, then performance guardrail hardening.

## Constraints

- **Public repo hygiene**: `.planning`, AI workflow traces, and other local-only artifacts must remain untracked.
- **Prererelease boundary**: preserve the four-package split plus the canonical `CreateSession(...)` and `Create(...)` route story.
- **Adapter-first architecture**: new scene/input/platform seams must make adapter portability easier without forcing a multi-adapter implementation in the same milestone.
- **Avalonia-first delivery**: improve the shipped Avalonia adapter before validating any second adapter.
- **Thin architecture only**: refactor only when it directly enables adapter-neutral rendering/input/platform seams or public-surface clarity.
- **Compatibility remains secondary**: treat `GraphEditorViewModel` and `GraphEditorView` as retained compatibility surfaces, not the primary extension path.
- **Performance must become explicit**: future capability work must be designed so 1000-node defended performance is achievable without another architecture rewrite.
- **No goal drift**: do not pull algorithm execution engines, marketplaces, remote install, unload lifecycle, sandboxing, browser SSR, or multiplayer whiteboard work into the pre-1.0 path.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Define AsterGraph as `Editor Kernel + Scene/Interaction + UI Adapter` | This matches the strongest existing seams and gives the product a better long-term frame than “Avalonia node editor with extras” | Active in v0.3.0-alpha |
| Start the next public ladder at `v0.3.0-alpha` instead of another local-only `v1.x` milestone label | The next roadmap should align to the public package/version story and future prerelease path | Active in v0.3.0-alpha |
| Freeze `CreateSession(...)` and `IGraphEditorSession` as the canonical host surface before expanding capabilities | Platform portability is blocked more by surface ambiguity than by missing one-off features | Active in v0.3.0-alpha |
| Keep Avalonia as adapter 1 and delay adapter 2 until `v0.9.0-beta` | A second adapter should validate the abstraction layer, not compete with platform-skeleton work in the same milestone | Active in roadmap |
| Keep richer capability work behind the platform skeleton and performance gate | Shipping stencil/export/router/custom-node breadth before the skeleton hardens would create the wrong public contracts | Completed in v0.3.0-alpha and still active in roadmap |
| Split `v0.4.0-alpha` into small capability slices instead of one "official modules" dump | Clipboard, keyboard, layout, minimap, stencil, fragments, export, and edge baseline have different seams and should not collapse into a god-service milestone | Completed in v0.4.0-alpha |
| Treat `Selection` and `History` as defended baseline modules, not active re-architecture work | They are already explicit on the runtime surface; forcing feature work there would create churn without shrinking any real gap | Reaffirmed in v0.4.0-alpha |
| Close `v0.4.0-alpha` without retro-backfilling local phase artifacts | The shipped code and proof were green, and the user explicitly accepted process debt instead of reopening planning history | Accepted at v0.4.0-alpha closeout |
| Preserve the non-goals around marketplace, sandboxing, unload, and browser-whiteboard scope | The project is still a desktop embedded SDK first, and those tracks would diffuse the prerelease path | Reaffirmed in roadmap |
| After `v0.11.0-beta`, prioritize external evaluation repeatability over another capability wave | The defended beta surface is broad enough; the next real risk is adopters failing to evaluate or report issues consistently | Completed in v0.12.0-beta |
| After `v0.12.0-beta`, prioritize externally validated capability claims over more internal contract polishing | The evidence loop now exists; the next risk is making capability claims without enough external proof or reproducible maintainer decisions | Completed in v0.13.0-beta |
| After `v0.13.0-beta`, prioritize supported-route friction closure over speculative platform widening | External capability claims are now bounded; the immediate risk is that supported routes still require too much maintainer interpretation | Completed in v0.14.0-beta |
| After `v0.14.0-beta`, prioritize copyable hosted recipes over another public capability wave | The remaining evidence cluster is about recipe copyability on the defended hosted route, not about widening the SDK surface | Completed in v0.15.0-beta |
| After `v0.15.0-beta`, prioritize retained migration recipe clarity over another capability wave | The remaining seeded usability pressure is that retained hosts still need careful reading to choose and stage migration on the maintained route | Completed in v0.16.0-beta |
| After `v0.16.0-beta`, keep the next beta line on bounded adopter-pressure follow-up instead of jumping to performance or a new capability wave | The public docs still point to usability/sample polish, trust clarity, and bounded retained cleanup before broader claims or new defended bets | Active in v0.17.0-beta |
| After `v0.28.0-beta`, prioritize authoring surface polish over WPF parity or broader capability expansion | The current bottleneck is whether hosts can copy one polished authoring surface from shared metadata through custom presentation and commands | Active in v0.29.0-beta |
| Keep local planning on `v0.29.0-beta` while treating this work as the future public `0.12.0-beta` authoring-surface line | Archived local `v0.12.0-beta` milestone files already exist, so reusing that label locally would collide with planning history | Active in v0.29.0-beta |
| After `v0.29.0-beta`, prioritize capability breadth closure over another metadata-polish cycle or adapter-hardening pass | The authoring surface is now copyable; the next evaluator-facing gap is platform-grade stencil organization, export breadth, and quick tools on the defended canonical route | Completed in v0.30.0-beta |
| Keep local planning on `v0.30.0-beta` while treating this work as the future public `0.13.0-beta` capability-breadth line | Archived local `v0.13.0-beta` milestone files already exist, so reusing that label locally would collide with planning history | Completed in v0.30.0-beta |
| After `v0.30.0-beta`, prioritize widened-surface performance hardening over another capability wave or adapter-hardening pass | Capability breadth is now proofed; the next risk is latency and large-tier regressions as the defended hosted surface grows | Active in v0.31.0-beta |
| Keep local planning on `v0.31.0-beta` while treating this work as the next future public post-`0.13` performance-hardening beta line | Archived local `v0.14.0-beta` milestone files already exist, so reusing that label locally would collide with planning history | Active in v0.31.0-beta |
| After `v0.31.0-beta`, prioritize hosted accessibility closure over adapter-hardening or another capability wave | The widened canonical route now has breadth and defended performance; the next evaluator-facing gap is explicit keyboard/focus/accessibility semantics | Completed in v0.32.0-beta |
| After `v0.32.0-beta`, prioritize automation-backed accessibility validation over adapter-2 work or another capability wave | The accessibility contract now exists, but evaluators still need repeatable proof-harness and automation-backed evidence instead of manual maintainer walkthroughs | Completed in v0.33.0-beta |
| After `v0.33.0-beta`, prioritize validation-only adapter-2 accessibility evidence over another capability wave | The canonical route is now defended through automation-backed accessibility proof; the next gap is bounded adapter validation, not wider platform promises | Completed in v0.34.0-beta |
| After `v0.34.0-beta`, prioritize validation-only adapter-2 performance evidence over another capability wave | The adapter-2 accessibility story is now explicit; the next gap is bounded performance validation on the same route, not wider platform promises | Completed in v0.35.0-beta |
| After `v0.35.0-beta`, prioritize validation-only adapter-2 capability-breadth evidence over another capability wave or parity push | The next missing adapter-2 evidence cluster is bounded export/tool/stencil breadth on shared contracts, not a second full UI route | Completed in v0.36.0-beta |
| After `v0.36.0-beta`, prioritize authoring surface polish for real host adoption over adapter-2 parity or performance expansion | The defended canonical route now has breadth, performance, and accessibility evidence; the next real risk is whether external hosts can comfortably copy and extend the authoring surface | Active in v0.37.0-beta |
| Keep local planning on `v0.37.0-beta` while treating this work as the public `0.12.0-beta` authoring-surface line | Archived local `v0.12.0-beta` milestone files already exist, so reusing that label locally would collide with planning history | Active in v0.37.0-beta |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition:**
1. Requirements invalidated? Move them to deferred or out-of-scope with a reason.
2. Requirements validated? Move them into the validated section with phase references.
3. New requirements emerged? Add them to the active section.
4. Decisions to log? Add them to Key Decisions.
5. "What This Is" still accurate? Update it if drifted.

**After each milestone:**
1. Full review of all sections.
2. Core Value check: verify the milestone still advanced the right product promise.
3. Audit deferred items: keep only intentionally unresolved follow-ups.
4. Update Context with the newest shipped state and next bottleneck.

---
*Last updated: 2026-04-25 after completing `v0.36.0-beta Adapter-2 Capability Breadth Validation` and initializing `v0.37.0-beta Authoring Surface Polish`*
