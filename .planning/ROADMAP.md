# Roadmap: v0.37.0-beta Authoring Surface Polish

**Goal:** 让宿主舒服地做参数、节点、端口和边，而不是再证明 AsterGraph 已经有多少能力。Deliver the public `0.12.0-beta` authoring-surface line by hardening the parameter/metadata contract, custom node/port/edge authoring affordances, host copyability recipes, and performance guardrails on the canonical session route.

## Current Milestone

- **Version:** `v0.37.0-beta`
- **Name:** `Authoring Surface Polish`
- **Status:** active locally
- **Focus:** parameter/metadata contract completion, custom node/port/edge authoring affordances, host copyability recipes, performance guardrails
- **Phase numbering:** continues from `v0.36.0-beta` and starts at **Phase 274**
- **Versioning note:** local planning uses `v0.37.0-beta` as the next internal beta label; this milestone delivers the public `0.12.0-beta` authoring-surface line.

## Phases

- [x] **Phase 274 Parameter/Metadata Contract Core** — Establish the shared parameter metadata vocabulary as one complete contract driving inspector and selected-node seam.
- [x] **Phase 275 Parameter/Metadata Validation And Evidence** — Harden validation, read-only reason, and support-bundle `parameterSnapshots` evidence on the canonical route.
- [x] **Phase 276 Custom Node/Port/Edge Core** — Ship multi-handle support, port grouping/validation, and node resize affordance on the canonical session route.
- [x] **Phase 277 Custom Node/Port/Edge Tools And Gestures** — Add node/edge quick tools, reconnect, temporary edge preview, and delete-on-drop with shared command routing.
- [x] **Phase 278 Host Copyability Docs And Recipes** — Make the hosted route ladder explicitly "copy-from-here" friendly with plugin-host and custom-node host recipes.
- [x] **Phase 279 Migration Route And External Intake** — Publish migration route comparison and activate support-bundle / issue-template intake for real adopter reports.
- [x] **Phase 280 Performance Guardrails** — Harden authoring-interaction latency budgets and proof markers; keep 5000-node stress tier as informational telemetry only.

## Phase Details

### Phase 274 Parameter/Metadata Contract Core

**Status:** planned

**Goal:** establish the shared parameter metadata vocabulary as one complete contract driving inspector and selected-node seam.

**Depends on:** Phase 273 (previous milestone)

**Requirements:** PARAM-01, PARAM-02

**Success criteria:**
1. Hosts can define `defaultValue`, `isAdvanced`, `helpText`, `placeholderText`, `constraints`, and `groupName` through one shared vocabulary.
2. The same contract drives inspector rendering, selected-node read/write seam, and multi-selection batch editing without host-side duplication.
3. Proof markers fail loudly if the contract diverges across inspector, node-side editors, or batch editing paths.

### Phase 275 Parameter/Metadata Validation And Evidence

**Status:** planned

**Goal:** harden validation, read-only reason, and support-bundle `parameterSnapshots` evidence on the canonical route.

**Depends on:** Phase 274

**Requirements:** PARAM-03, PARAM-04

**Success criteria:**
1. Parameter validation messages and read-only reasons surface consistently across inspector, node-side editors, badges, and status cues.
2. The support-bundle `parameterSnapshots` evidence captures exact parameter state, validation results, and metadata contract version.
3. ConsumerSample.Avalonia emits dedicated parameter-contract proof markers that fail on regression.

### Phase 276 Custom Node/Port/Edge Core

**Status:** planned

**Goal:** ship multi-handle support, port grouping/validation, and node resize affordance on the canonical session route.

**Depends on:** Phase 275

**Requirements:** AUTHOR-01, AUTHOR-02

**Success criteria:**
1. Custom nodes support multiple input/output handles with host-declared port grouping and validation rules.
2. Nodes expose resize affordance (hover, drag, live feedback) within the defended latency budget.
3. Port grouping and validation errors are visible in the default Avalonia host without custom discovery plumbing.

### Phase 277 Custom Node/Port/Edge Tools And Gestures

**Status:** planned

**Goal:** add node/edge quick tools, reconnect, temporary edge preview, and delete-on-drop with shared command routing.

**Depends on:** Phase 276

**Requirements:** AUTHOR-03, AUTHOR-04, AUTHOR-05

**Success criteria:**
1. Node and edge quick tools project from the shared command descriptor surface without adapter-specific presenter code.
2. Reconnect, temporary edge preview, and delete-on-drop gestures work on the canonical route with explicit command routing and undo support.
3. Authoring actions stay on one command descriptor across toolbar, context menu, shortcuts, palette, and host rails.

### Phase 278 Host Copyability Docs And Recipes

**Status:** planned

**Goal:** make the hosted route ladder explicitly "copy-from-here" friendly with plugin-host and custom-node host recipes.

**Depends on:** Phase 277

**Requirements:** RECIPE-01, RECIPE-02, RECIPE-03

**Success criteria:**
1. Public docs for `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia` contain explicit "copy-from-here" paragraphs.
2. A plugin-host recipe documents how real hosts load, trust, and communicate with plugins on the canonical session route.
3. A custom-node host recipe documents how real hosts register and style custom nodes, ports, and edges without adapter-specific runtime API.

### Phase 279 Migration Route And External Intake

**Status:** planned

**Goal:** publish migration route comparison and activate support-bundle / issue-template intake for real adopter reports.

**Depends on:** Phase 278

**Requirements:** RECIPE-04, RECIPE-05

**Success criteria:**
1. A migration route comparison doc explains when to choose `CreateSession(...)` vs retained `GraphEditorViewModel` / `GraphEditorView`, with staging steps.
2. The support-bundle format and issue template are actively used to collect and triage external adopter feedback.
3. The milestone tracks progress toward 3–5 real external reports with explicit intake criteria.

### Phase 280 Performance Guardrails

**Status:** planned

**Goal:** harden authoring-interaction latency budgets and proof markers; keep 5000-node stress tier as informational telemetry only.

**Depends on:** Phase 279

**Requirements:** PERF-01, PERF-02, PERF-03

**Success criteria:**
1. Authoring-interaction latency budgets (inspector open, node resize, edge create, command palette) have defended proof markers that fail loudly on regression.
2. The 1000-node `large` ScaleSmoke tier remains a defended budget with explicit pass/fail contract in release validation.
3. The 5000-node `stress` tier publishes p50/p95 telemetry but is NOT marketed as a defended public claim.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| Phase 274 Parameter/Metadata Contract Core | 1/1 | Completed | 2026-04-25 |
| Phase 275 Parameter/Metadata Validation And Evidence | 1/1 | Completed | 2026-04-25 |
| Phase 276 Custom Node/Port/Edge Core | 1/1 | Completed | 2026-04-25 |
| Phase 277 Custom Node/Port/Edge Tools And Gestures | 1/1 | Completed | 2026-04-25 |
| Phase 278 Host Copyability Docs And Recipes | 1/1 | Completed | 2026-04-25 |
| Phase 279 Migration Route And External Intake | 1/1 | Completed | 2026-04-25 |
| Phase 280 Performance Guardrails | 1/1 | Completed | 2026-04-25 |

## Planned Follow-On Milestones

- Once `v0.37.0-beta` is closed, the next candidate is bounded external adopter feedback analysis and follow-up polish, but it is intentionally not initialized yet.

## Archived Milestones

- v0.36.0-beta - Adapter-2 Capability Breadth Validation - completed locally 2026-04-25 ([roadmap](./milestones/v0.36.0-beta-ROADMAP.md), [requirements](./milestones/v0.36.0-beta-REQUIREMENTS.md))
- v0.35.0-beta - Adapter-2 Performance Validation - completed locally 2026-04-23 ([roadmap](./milestones/v0.35.0-beta-ROADMAP.md), [requirements](./milestones/v0.35.0-beta-REQUIREMENTS.md))
- v0.34.0-beta - Adapter-2 Accessibility Validation - completed locally 2026-04-23 ([roadmap](./milestones/v0.34.0-beta-ROADMAP.md), [requirements](./milestones/v0.34.0-beta-REQUIREMENTS.md))
- v0.33.0-beta - Automation-Backed Accessibility Validation - completed locally 2026-04-23 ([roadmap](./milestones/v0.33.0-beta-ROADMAP.md), [requirements](./milestones/v0.33.0-beta-REQUIREMENTS.md))
- v0.32.0-beta - Hosted Accessibility Closure - completed locally 2026-04-23 ([roadmap](./milestones/v0.32.0-beta-ROADMAP.md), [requirements](./milestones/v0.32.0-beta-REQUIREMENTS.md))
- v0.31.0-beta - Widened Surface Performance Hardening - completed locally 2026-04-23 ([roadmap](./milestones/v0.31.0-beta-ROADMAP.md), [requirements](./milestones/v0.31.0-beta-REQUIREMENTS.md))
- v0.30.0-beta - Capability Breadth Closure - completed locally 2026-04-23 ([roadmap](./milestones/v0.30.0-beta-ROADMAP.md), [requirements](./milestones/v0.30.0-beta-REQUIREMENTS.md))
- v0.29.0-beta - Authoring Surface Polish - completed locally 2026-04-23 ([roadmap](./milestones/v0.29.0-beta-ROADMAP.md), [requirements](./milestones/v0.29.0-beta-REQUIREMENTS.md))
- v0.28.0-beta - Public Verification Loop - completed locally 2026-04-23 ([roadmap](./milestones/v0.28.0-beta-ROADMAP.md), [requirements](./milestones/v0.28.0-beta-REQUIREMENTS.md))
- v0.27.0-beta - External Intake Dry Run - completed locally 2026-04-23 ([roadmap](./milestones/v0.27.0-beta-ROADMAP.md), [requirements](./milestones/v0.27.0-beta-REQUIREMENTS.md))
- v0.26.0-beta - Parameter/Metadata Intake Visibility - completed locally 2026-04-23 ([roadmap](./milestones/v0.26.0-beta-ROADMAP.md), [requirements](./milestones/v0.26.0-beta-REQUIREMENTS.md))
- v0.25.0-beta - Host-Owned Parameter/Metadata Adopter Proof Polish - completed locally 2026-04-23 ([roadmap](./milestones/v0.25.0-beta-ROADMAP.md), [requirements](./milestones/v0.25.0-beta-REQUIREMENTS.md))
- v0.24.0-beta - Real Adopter Intake Conversion - completed locally 2026-04-23 ([roadmap](./milestones/v0.24.0-beta-ROADMAP.md), [requirements](./milestones/v0.24.0-beta-REQUIREMENTS.md))
- v0.23.0-beta - Parameter And Metadata Copyability Proof Closure - completed locally 2026-04-23 ([roadmap](./milestones/v0.23.0-beta-ROADMAP.md), [requirements](./milestones/v0.23.0-beta-REQUIREMENTS.md))
- v0.22.0-beta - Copyable Host-Owned Parameter And Metadata Editing Polish - completed locally 2026-04-23 ([roadmap](./milestones/v0.22.0-beta-ROADMAP.md), [requirements](./milestones/v0.22.0-beta-REQUIREMENTS.md))
- v0.21.0-beta - Evaluation Path Friction Closure - completed locally 2026-04-23 ([roadmap](./milestones/v0.21.0-beta-ROADMAP.md), [requirements](./milestones/v0.21.0-beta-REQUIREMENTS.md))
- v0.20.0-beta - Evidence Loop Durability - completed locally 2026-04-23 ([roadmap](./milestones/v0.20.0-beta-ROADMAP.md), [requirements](./milestones/v0.20.0-beta-REQUIREMENTS.md))
- v0.19.0-beta - Adoption Evidence Cohesion - completed locally 2026-04-22 ([roadmap](./milestones/v0.19.0-beta-ROADMAP.md), [requirements](./milestones/v0.19.0-beta-REQUIREMENTS.md))
- v0.18.0-beta - Usability And Sample Polish - completed locally 2026-04-22 ([roadmap](./milestones/v0.18.0-beta-ROADMAP.md), [requirements](./milestones/v0.18.0-beta-REQUIREMENTS.md))
- v0.17.0-beta - Adopter Pressure Follow-up - completed locally 2026-04-22 ([roadmap](./milestones/v0.17.0-beta-ROADMAP.md), [requirements](./milestones/v0.17.0-beta-REQUIREMENTS.md))
- v0.16.0-beta - Retained Migration Recipes - completed locally 2026-04-22 ([roadmap](./milestones/v0.16.0-beta-ROADMAP.md), [requirements](./milestones/v0.16.0-beta-REQUIREMENTS.md))
- v0.15.0-beta - Copyable Hosted Recipes - completed locally 2026-04-22 ([roadmap](./milestones/v0.15.0-beta-ROADMAP.md), [requirements](./milestones/v0.15.0-beta-REQUIREMENTS.md))
- v0.14.0-beta - Supported Route Friction Closure - completed locally 2026-04-22 ([roadmap](./milestones/v0.14.0-beta-ROADMAP.md), [requirements](./milestones/v0.14.0-beta-REQUIREMENTS.md))
- v0.13.0-beta - Externally Validated Platform Capability - completed locally 2026-04-22 ([roadmap](./milestones/v0.13.0-beta-ROADMAP.md), [requirements](./milestones/v0.13.0-beta-REQUIREMENTS.md))
- v0.12.0-beta - Adopter Validation Loop - completed locally 2026-04-22 ([roadmap](./milestones/v0.12.0-beta-ROADMAP.md), [requirements](./milestones/v0.12.0-beta-REQUIREMENTS.md))
- v0.11.0-beta - Defended Surface Hold - completed 2026-04-22 ([roadmap](./milestones/v0.11.0-beta-ROADMAP.md), [requirements](./milestones/v0.11.0-beta-REQUIREMENTS.md))
- v0.10.0-beta - SDK Stabilization - completed 2026-04-22 ([roadmap](./milestones/v0.10.0-beta-ROADMAP.md), [requirements](./milestones/v0.10.0-beta-REQUIREMENTS.md))
- v0.9.0-beta - Second Adapter Validation - completed 2026-04-22 ([roadmap](./milestones/v0.9.0-beta-ROADMAP.md), [requirements](./milestones/v0.9.0-beta-REQUIREMENTS.md))
- v0.8.0-alpha - DX, Docs, And Devtools - completed 2026-04-22 ([roadmap](./milestones/v0.8.0-alpha-ROADMAP.md), [requirements](./milestones/v0.8.0-alpha-REQUIREMENTS.md))
- v0.7.0-alpha - Custom Model And Native UX - completed 2026-04-22 ([roadmap](./milestones/v0.7.0-alpha-ROADMAP.md), [requirements](./milestones/v0.7.0-alpha-REQUIREMENTS.md))
- v0.6.0-alpha - Advanced Graph Editing - completed 2026-04-21 ([roadmap](./milestones/v0.6.0-alpha-ROADMAP.md), [requirements](./milestones/v0.6.0-alpha-REQUIREMENTS.md))
- v0.5.0-alpha - Performance Gate - completed 2026-04-21 ([roadmap](./milestones/v0.5.0-alpha-ROADMAP.md), [requirements](./milestones/v0.5.0-alpha-REQUIREMENTS.md), [audit](./v0.5.0-alpha-MILESTONE-AUDIT.md))
- v0.4.0-alpha - Official Capability Modules - completed 2026-04-21 ([roadmap](./milestones/v0.4.0-alpha-ROADMAP.md), [requirements](./milestones/v0.4.0-alpha-REQUIREMENTS.md), [audit](./v0.4.0-alpha-MILESTONE-AUDIT.md))
- v0.3.0-alpha - Platform Skeleton Freeze - completed 2026-04-21 ([roadmap](./milestones/v0.3.0-alpha-ROADMAP.md), [requirements](./milestones/v0.3.0-alpha-REQUIREMENTS.md), [audit](./v0.3.0-alpha-MILESTONE-AUDIT.md))
- v1.25 - Command Pipeline, Inspector Deepening, And Semantic Authoring - completed 2026-04-21 ([roadmap](./milestones/v1.25-ROADMAP.md), [requirements](./milestones/v1.25-REQUIREMENTS.md), [audit](./v1.25-MILESTONE-AUDIT.md))

---
Roadmap updated: 2026-04-25 after completing `v0.36.0-beta Adapter-2 Capability Breadth Validation` and initializing `v0.37.0-beta Authoring Surface Polish`
