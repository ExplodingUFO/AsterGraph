# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.7 Consumer Closure / Release Hardening** - Phases 34-37 planned
- ✅ **[v1.6 Facade Convergence and Proof Guardrails](./milestones/v1.6-ROADMAP.md)** - Phases 30-33, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.5 Runtime Boundary Cleanup and Quality Gates](./milestones/v1.5-ROADMAP.md)** - Phases 26-29, shipped 2026-04-14
- ✅ **[v1.4 Plugin Loading and Automation Execution](./milestones/v1.4-ROADMAP.md)** - Phases 22-25, shipped 2026-04-08, archived 2026-04-16
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** - Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** - Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** - Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** - Phases 01-06, shipped before milestone archive split

## Overview

AsterGraph now has the runtime-first SDK boundary, archived milestone history, explicit retained history/save proof, a maintained proof lane, and a narrower retained facade. The next milestone is not about widening capability. It is about making the shipped surface read coherently, verify coherently, onboard coherently, and age more predictably for external consumers.

v1.7 focuses on productization closure: align repo narrative and proof entry points, collapse release verification into a machine gate, give consumers one minimal canonical host path, publish state and extension contracts explicitly, and keep hotspot reduction moving without reopening runtime ownership drift.

## Milestone

**Milestone:** v1.7 Consumer Closure / Release Hardening
**Goal:** Turn the shipped runtime, plugin, automation, and proof surfaces into a tighter consumer-facing product story with one truthful narrative, one executable proof ring, stronger release automation, and clearer long-term extension contracts.

## Phases

- [ ] **Phase 34: Truth Alignment And Proof Ring Closure** - Align live docs and planning artifacts to one current story, then make the proof ring explicit around real tool entry points. (planned 2026-04-16)
- [ ] **Phase 35: Release Gate And Matrix Automation** - Turn the documented build/test/pack/smoke path into one machine gate with explicit framework-matrix and compatibility coverage. (planned 2026-04-16)
- [ ] **Phase 36: Consumer Path And State Contract Closure** - Close the minimal consumer onboarding path and publish the explicit history/save/dirty contract through docs plus proof. (planned 2026-04-16)
- [ ] **Phase 37: Maintainability And Extension Contract Hardening** - Keep shrinking compatibility hotspots, separate maintenance lanes more clearly, and document extension and retirement rules. (planned 2026-04-16)

## Phase Details

### Phase 34: Truth Alignment And Proof Ring Closure
**Goal**: Align live docs and planning artifacts to one current story, then make the proof ring explicit around real tool entry points.
**Depends on**: v1.6 archived baseline
**Requirements**: ALIGN-01, ALIGN-02, PROOF-01
**Success Criteria**:
1. `README`, `ROADMAP`, `STATE`, `PROJECT`, and current codebase maps all describe the same milestone state, shipped capability claims, and proof-ring composition.
2. Capability, non-goal, target-matrix, and proof-tool references no longer contradict the live repository layout.
3. The official proof ring lists only real, discoverable entry points for smoke tools, focused regressions, and the minimal consumer host path.
**Plans**: 3 plans

### Phase 35: Release Gate And Matrix Automation
**Goal**: Turn the documented build/test/pack/smoke path into one machine gate with explicit framework-matrix and compatibility coverage.
**Depends on**: Phase 34
**Requirements**: REL-01, REL-02, REL-03
**Success Criteria**:
1. One official verification entry point runs build, test, pack, smoke, and compatibility checks instead of relying on stitched README commands.
2. CI covers the supported `net8.0` / `net9.0` matrix through explicit build, smoke, and contract lanes.
3. The publishable package surface is protected by automated compatibility validation during release verification.
**Plans**: 3 plans

### Phase 36: Consumer Path And State Contract Closure
**Goal**: Close the minimal consumer onboarding path and publish the explicit history/save/dirty contract through docs plus proof.
**Depends on**: Phase 35
**Requirements**: CONS-01, CONS-02, HIST-01, HIST-02
**Success Criteria**:
1. Consumers can open one minimal host sample that proves the canonical package-consumption path without reading the demo shell first.
2. Consumer docs provide short routes for runtime-only, default Avalonia UI, trust/discovery, and automation adoption, each with minimal code and verification steps.
3. History/save/dirty semantics are documented explicitly and no longer survive only as implicit test knowledge or carried planning concern.
4. Focused history/save proof is part of the official verification gate.
**Plans**: 3 plans

### Phase 37: Maintainability And Extension Contract Hardening
**Goal**: Keep shrinking compatibility hotspots, separate maintenance lanes more clearly, and document extension and retirement rules.
**Depends on**: Phase 36
**Requirements**: MAINT-01, MAINT-02, EXT-01, EXT-02
**Success Criteria**:
1. `GraphEditorViewModel` can keep thinning as a compatibility facade without reopening runtime ownership drift.
2. Core contract, proof/integration, and demo/sample lanes are clearer enough that failures localize to the right maintenance surface.
3. Compatibility-retirement and extension-precedence rules are documented so consumers know which surfaces are stable, transitional, or host-overridable.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 34. Truth Alignment And Proof Ring Closure | ALIGN-01, ALIGN-02, PROOF-01 | Not started |
| 35. Release Gate And Matrix Automation | REL-01, REL-02, REL-03 | Not started |
| 36. Consumer Path And State Contract Closure | CONS-01, CONS-02, HIST-01, HIST-02 | Not started |
| 37. Maintainability And Extension Contract Hardening | MAINT-01, MAINT-02, EXT-01, EXT-02 | Not started |

## Next Action

**Next action:** plan Phase 34 with `$gsd-plan-phase 34`
