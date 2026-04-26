# Roadmap

## Current Milestone

**v0.39.0-beta Productized SDK Adoption Path** — active locally

**Goal:** Make AsterGraph easier to evaluate and integrate externally by productizing the demo, aligning public version/release messaging, shortening the first-run onboarding path, and adding thin host-friendly API affordances without changing the runtime architecture.

**Focus:** Demo/README first impression, public version vocabulary, five-minute hosted onboarding, realistic ConsumerSample scenario, thin API builder, and proof gates.

**Phase numbering:** continues from `v0.38.0-beta` and starts at **Phase 285**.

## Phases

- [x] **Phase 285: Version And Release Narrative Alignment** — Remove public version ambiguity across README, versioning docs, release workflow output, and issue/release links while preserving local planning labels as private bookkeeping.
- [ ] **Phase 286: README First View And Scenario Demo Launch** — Productize the first public impression with a concrete visual and a launchable prebuilt scenario demo.
- [ ] **Phase 287: Scenario Capability Story And Guided Tour** — Make the Demo show a coherent SDK story covering custom nodes, parameters, validation, trust, automation, save/load, and export.
- [ ] **Phase 288: Five-Minute Onboarding And ConsumerSample Scenario** — Turn ConsumerSample and quick-start docs into a realistic copyable host path between HelloWorld and the full Demo.
- [ ] **Phase 289: Thin Host Builder And Adoption Proof Gate** — Add a thin hosted builder/facade over the canonical route and defend the productized adoption path with tests/proof markers.

## Phase Details

### Phase 285: Version And Release Narrative Alignment

**Status:** planned

**Goal:** remove public version ambiguity across README, versioning docs, release workflow output, and issue/release links while preserving local planning labels as private bookkeeping.

**Depends on:** Phase 284 (previous milestone)

**Requirements:** REL-01, REL-02, REL-03, REL-04, PROOF-02, PROOF-03

**Success criteria:**
1. README, English/Chinese versioning docs, issue templates, and release docs agree on package version, matching public tag/release convention, and historical `v1.x` tag caveat.
2. Release validation fails if package version, public tag, README/version docs, or generated release note header disagree.
3. Public performance and plugin-trust wording continues to avoid overclaiming 5000-node defended budgets or untrusted plugin sandboxing.

### Phase 286: README First View And Scenario Demo Launch

**Status:** planned

**Goal:** productize the first public impression with a concrete visual and a launchable prebuilt scenario demo.

**Depends on:** Phase 285

**Requirements:** DEMO-01, DEMO-02

**Success criteria:**
1. README first viewport includes a concrete screenshot/GIF or equivalent visual that shows drag, connect, parameter editing, automation, and export in one scenario.
2. The Demo host can launch a prebuilt scenario from command line or an equivalent explicit entry point without requiring a blank-canvas setup.
3. Automated or documented proof confirms the scenario launch path is stable.

### Phase 287: Scenario Capability Story And Guided Tour

**Status:** planned

**Goal:** make the Demo show a coherent SDK story covering custom nodes, parameters, validation, trust, automation, save/load, and export.

**Depends on:** Phase 286

**Requirements:** DEMO-03, DEMO-04

**Success criteria:**
1. The scenario demo exercises custom nodes, parameter editing, connection validation, plugin/trust visibility, save/load, automation/proof output, and export.
2. A guided tour or equivalent in-demo flow walks the evaluator through creating nodes, connecting them, editing parameters, loading plugin content, and exporting output.
3. Demo tests or proof markers fail if the scenario loses any required capability signal.

### Phase 288: Five-Minute Onboarding And ConsumerSample Scenario

**Status:** planned

**Goal:** turn ConsumerSample and quick-start docs into a realistic copyable host path between HelloWorld and the full Demo.

**Depends on:** Phase 287

**Requirements:** ONB-01, ONB-02, ONB-03, ONB-04

**Success criteria:**
1. A new Avalonia host can follow the five-minute quick start from package install or starter run to first graph load/save and first custom node definition.
2. README and quick-start docs clearly explain when to copy Starter, HelloWorld, HelloWorld.Avalonia, ConsumerSample, or the full Demo.
3. ConsumerSample opens with a scenario graph, host-owned actions, parameter editing, trusted plugin flow, support-bundle proof, and stable onboarding markers.

### Phase 289: Thin Host Builder And Adoption Proof Gate

**Status:** planned

**Goal:** add a thin hosted builder/facade over the canonical route and defend the productized adoption path with tests/proof markers.

**Depends on:** Phase 288

**Requirements:** API-01, API-02, API-03, PROOF-01

**Success criteria:**
1. Avalonia hosts can compose common hosted setup through a thin builder/facade accepting document, catalog, compatibility, plugin trust, localization, and diagnostics inputs.
2. Tests prove the builder/facade delegates to the existing canonical factories and does not introduce a parallel runtime path.
3. CI or docs tests defend README first-viewport claims, five-minute quick start, scenario demo launch, and ConsumerSample scenario markers.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| Phase 285: Version And Release Narrative Alignment | 1/1 | Completed | 2026-04-26 |
| Phase 286: README First View And Scenario Demo Launch | 0/1 | Planned | — |
| Phase 287: Scenario Capability Story And Guided Tour | 0/1 | Planned | — |
| Phase 288: Five-Minute Onboarding And ConsumerSample Scenario | 0/1 | Planned | — |
| Phase 289: Thin Host Builder And Adoption Proof Gate | 0/1 | Planned | — |

## Completed Milestones

- **v0.38.0-beta** — External Adopter Feedback Loop — completed locally 2026-04-25 ([roadmap](./milestones/v0.38.0-beta-ROADMAP.md), [requirements](./milestones/v0.38.0-beta-REQUIREMENTS.md))
- **v0.37.0-beta** — Authoring Surface Polish — completed locally 2026-04-25 ([roadmap](./milestones/v0.37.0-beta-ROADMAP.md), [requirements](./milestones/v0.37.0-beta-REQUIREMENTS.md))
- **v0.36.0-beta** — Adapter-2 Capability Breadth Validation — completed locally 2026-04-25 ([roadmap](./milestones/v0.36.0-beta-ROADMAP.md), [requirements](./milestones/v0.36.0-beta-REQUIREMENTS.md))
- **v0.35.0-beta** — Adapter-2 Performance Validation — completed locally 2026-04-23 ([roadmap](./milestones/v0.35.0-beta-ROADMAP.md), [requirements](./milestones/v0.35.0-beta-REQUIREMENTS.md))
- **v0.34.0-beta** — Adapter-2 Accessibility Validation — completed locally 2026-04-23 ([roadmap](./milestones/v0.34.0-beta-ROADMAP.md), [requirements](./milestones/v0.34.0-beta-REQUIREMENTS.md))
- **v0.33.0-beta** — Automation-Backed Accessibility Validation — completed locally 2026-04-23 ([roadmap](./milestones/v0.33.0-beta-ROADMAP.md), [requirements](./milestones/v0.33.0-beta-REQUIREMENTS.md))
- **v0.32.0-beta** — Hosted Accessibility Closure — completed locally 2026-04-23 ([roadmap](./milestones/v0.32.0-beta-ROADMAP.md), [requirements](./milestones/v0.32.0-beta-REQUIREMENTS.md))
- **v0.31.0-beta** — Widened Surface Performance Hardening — completed locally 2026-04-23 ([roadmap](./milestones/v0.31.0-beta-ROADMAP.md), [requirements](./milestones/v0.31.0-beta-REQUIREMENTS.md))
- **v0.30.0-beta** — Capability Breadth Closure — completed locally 2026-04-23 ([roadmap](./milestones/v0.30.0-beta-ROADMAP.md), [requirements](./milestones/v0.30.0-beta-REQUIREMENTS.md))
- **v0.29.0-beta** — Authoring Surface Polish — completed locally 2026-04-23 ([roadmap](./milestones/v0.29.0-beta-ROADMAP.md), [requirements](./milestones/v0.29.0-beta-REQUIREMENTS.md))
- **v0.28.0-beta** — Public Verification Loop — completed locally 2026-04-23 ([roadmap](./milestones/v0.28.0-beta-ROADMAP.md), [requirements](./milestones/v0.28.0-beta-REQUIREMENTS.md))
- **v0.27.0-beta** — External Intake Dry Run — completed locally 2026-04-23 ([roadmap](./milestones/v0.27.0-beta-ROADMAP.md), [requirements](./milestones/v0.27.0-beta-REQUIREMENTS.md))
- **v0.26.0-beta** — Parameter/Metadata Intake Visibility — completed locally 2026-04-23 ([roadmap](./milestones/v0.26.0-beta-ROADMAP.md), [requirements](./milestones/v0.26.0-beta-REQUIREMENTS.md))
- **v0.25.0-beta** — Host-Owned Parameter/Metadata Adopter Proof Polish — completed locally 2026-04-23 ([roadmap](./milestones/v0.25.0-beta-ROADMAP.md), [requirements](./milestones/v0.25.0-beta-REQUIREMENTS.md))
- **v0.24.0-beta** — Real Adopter Intake Conversion — completed locally 2026-04-23 ([roadmap](./milestones/v0.24.0-beta-ROADMAP.md), [requirements](./milestones/v0.24.0-beta-REQUIREMENTS.md))
- **v0.23.0-beta** — Parameter And Metadata Copyability Proof Closure — completed locally 2026-04-23 ([roadmap](./milestones/v0.23.0-beta-ROADMAP.md), [requirements](./milestones/v0.23.0-beta-REQUIREMENTS.md))
- **v0.22.0-beta** — Copyable Host-Owned Parameter And Metadata Editing Polish — completed locally 2026-04-23 ([roadmap](./milestones/v0.22.0-beta-ROADMAP.md), [requirements](./milestones/v0.22.0-beta-REQUIREMENTS.md))
- **v0.21.0-beta** — Evaluation Path Friction Closure — completed locally 2026-04-23 ([roadmap](./milestones/v0.21.0-beta-ROADMAP.md), [requirements](./milestones/v0.21.0-beta-REQUIREMENTS.md))
- **v0.20.0-beta** — Evidence Loop Durability — completed locally 2026-04-23 ([roadmap](./milestones/v0.20.0-beta-ROADMAP.md), [requirements](./milestones/v0.20.0-beta-REQUIREMENTS.md))
- **v0.19.0-beta** — Adoption Evidence Cohesion — completed locally 2026-04-22 ([roadmap](./milestones/v0.19.0-beta-ROADMAP.md), [requirements](./milestones/v0.19.0-beta-REQUIREMENTS.md))
- **v0.18.0-beta** — Usability And Sample Polish — completed locally 2026-04-22 ([roadmap](./milestones/v0.18.0-beta-ROADMAP.md), [requirements](./milestones/v0.18.0-beta-REQUIREMENTS.md))
- **v0.17.0-beta** — Adopter Pressure Follow-up — completed locally 2026-04-22 ([roadmap](./milestones/v0.17.0-beta-ROADMAP.md), [requirements](./milestones/v0.17.0-beta-REQUIREMENTS.md))
- **v0.16.0-beta** — Retained Migration Recipes — completed locally 2026-04-22 ([roadmap](./milestones/v0.16.0-beta-ROADMAP.md), [requirements](./milestones/v0.16.0-beta-REQUIREMENTS.md))
- **v0.15.0-beta** — Copyable Hosted Recipes — completed locally 2026-04-22 ([roadmap](./milestones/v0.15.0-beta-ROADMAP.md), [requirements](./milestones/v0.15.0-beta-REQUIREMENTS.md))
- **v0.14.0-beta** — Supported Route Friction Closure — completed locally 2026-04-22 ([roadmap](./milestones/v0.14.0-beta-ROADMAP.md), [requirements](./milestones/v0.14.0-beta-REQUIREMENTS.md))
- **v0.13.0-beta** — Externally Validated Platform Capability — completed locally 2026-04-22 ([roadmap](./milestones/v0.13.0-beta-ROADMAP.md), [requirements](./milestones/v0.13.0-beta-REQUIREMENTS.md))
- **v0.12.0-beta** — Adopter Validation Loop — completed locally 2026-04-22 ([roadmap](./milestones/v0.12.0-beta-ROADMAP.md), [requirements](./milestones/v0.12.0-beta-REQUIREMENTS.md))
- **v0.11.0-beta** — Defended Surface Hold — completed 2026-04-22 ([roadmap](./milestones/v0.11.0-beta-ROADMAP.md), [requirements](./milestones/v0.11.0-beta-REQUIREMENTS.md))
- **v0.10.0-beta** — SDK Stabilization — completed 2026-04-22 ([roadmap](./milestones/v0.10.0-beta-ROADMAP.md), [requirements](./milestones/v0.10.0-beta-REQUIREMENTS.md))
- **v0.9.0-beta** — Second Adapter Validation — completed 2026-04-22 ([roadmap](./milestones/v0.9.0-beta-ROADMAP.md), [requirements](./milestones/v0.9.0-beta-REQUIREMENTS.md))
- **v0.8.0-alpha** — DX, Docs, And Devtools — completed 2026-04-22 ([roadmap](./milestones/v0.8.0-alpha-ROADMAP.md), [requirements](./milestones/v0.8.0-alpha-REQUIREMENTS.md))
- **v0.7.0-alpha** — Custom Model And Native UX — completed 2026-04-22 ([roadmap](./milestones/v0.7.0-alpha-ROADMAP.md), [requirements](./milestones/v0.7.0-alpha-REQUIREMENTS.md))
- **v0.6.0-alpha** — Advanced Graph Editing — completed 2026-04-21 ([roadmap](./milestones/v0.6.0-alpha-ROADMAP.md), [requirements](./milestones/v0.6.0-alpha-REQUIREMENTS.md))
- **v0.5.0-alpha** — Performance Gate — completed 2026-04-21 ([roadmap](./milestones/v0.5.0-alpha-ROADMAP.md), [requirements](./milestones/v0.5.0-alpha-REQUIREMENTS.md), [audit](./v0.5.0-alpha-MILESTONE-AUDIT.md))
- **v0.4.0-alpha** — Official Capability Modules — completed 2026-04-21 ([roadmap](./milestones/v0.4.0-alpha-ROADMAP.md), [requirements](./milestones/v0.4.0-alpha-REQUIREMENTS.md), [audit](./v0.4.0-alpha-MILESTONE-AUDIT.md))
- **v0.3.0-alpha** — Platform Skeleton Freeze — completed 2026-04-21 ([roadmap](./milestones/v0.3.0-alpha-ROADMAP.md), [requirements](./milestones/v0.3.0-alpha-REQUIREMENTS.md), [audit](./v0.3.0-alpha-MILESTONE-AUDIT.md))
- **v1.25** — Command Pipeline, Inspector Deepening, And Semantic Authoring — completed 2026-04-21 ([roadmap](./milestones/v1.25-ROADMAP.md), [requirements](./milestones/v1.25-REQUIREMENTS.md), [audit](./v1.25-MILESTONE-AUDIT.md))
