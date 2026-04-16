# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.6 Facade Convergence and Proof Guardrails** - Phases 30-33 planned
- ✅ **[v1.5 Runtime Boundary Cleanup and Quality Gates](./milestones/v1.5-ROADMAP.md)** - Phases 26-29, shipped 2026-04-14
- ✅ **[v1.4 Plugin Loading and Automation Execution](./milestones/v1.4-ROADMAP.md)** - Phases 22-25, shipped 2026-04-08, archived 2026-04-16
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** - Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** - Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** - Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** - Phases 01-06, shipped before milestone archive split

## Overview

AsterGraph now has the runtime-first SDK boundary, archived milestone history, an explicit history/save proof contract, and a scripted maintenance lane. The remaining v1.6 work is the hotspot contraction itself: keep shrinking `GraphEditorViewModel`, then continue the follow-through work in `GraphEditorKernel`, `NodeCanvas`, and the remaining doc guardrail debt without public API churn.

v1.6 deliberately avoids reopening already-shipped baseline work such as `.editorconfig`, central package management, CI setup, or `ScaleSmoke` solution alignment unless a live regression appears in the repo itself.

## Milestone

**Milestone:** v1.6 Facade Convergence and Proof Guardrails
**Goal:** Reduce the remaining internal complexity around the retained facade path, close the carried history/save semantic concern, and tighten the maintenance guardrails needed for continued hotspot refactoring without changing the public SDK surface.

## Phases

- [x] **Phase 30: Milestone History And Refactor Gate Closeout** - Archive the missing milestone history and normalize the maintainer entry points for refactor-sensitive proof. (planned 2026-04-16) (completed 2026-04-16)
- [x] **Phase 31: History And Save Semantic Closure** - Remove the carried `STATE_HISTORY_OK` mismatch and harden focused history/save regressions. (planned 2026-04-16) (completed 2026-04-16)
- [ ] **Phase 32: GraphEditorViewModel Facade Convergence** - Continue moving retained-facade orchestration out of `GraphEditorViewModel` while preserving the public SDK surface. (planned 2026-04-16)
- [ ] **Phase 33: Kernel, Canvas, And Guardrail Follow-Through** - Finish the next hotspot splits around downstream collaborators and tighten the remaining documentation/maintenance debt. (planned 2026-04-16)

## Phase Details

### Phase 30: Milestone History And Refactor Gate Closeout
**Goal**: Archive the missing milestone history and normalize the maintainer entry points for refactor-sensitive proof.
**Depends on**: v1.5 shipped baseline
**Requirements**: CLOSE-01, CLOSE-02, GUARD-01
**Success Criteria**:
1. `v1.4` is archived into checked-in milestone files and top-level planning artifacts no longer imply contradictory active-vs-archived milestone state.
2. Current planning/docs identify the live proof ring, carried concerns, and next step without depending on stale phase-directory context.
3. Contributors can run one checked-in maintenance/refactor gate that exercises the hotspot-sensitive regression surface from a documented command path.
**Plans**: 3 plans

### Phase 31: History And Save Semantic Closure
**Goal**: Remove the carried `STATE_HISTORY_OK` mismatch and harden focused history/save regressions.
**Depends on**: Phase 30
**Requirements**: STATE-01, STATE-02, STATE-03
**Success Criteria**:
1. Undo/redo/dirty/save behavior is expressed through passing focused tests that make the retained-facade contract explicit.
2. The carried `STATE_HISTORY_OK` mismatch is removed or replaced with proof markers that match the actual shipped semantics.
3. History interaction, drag, and save-boundary failures localize to smaller focused suites instead of only one broad transaction file.
**Plans**: 3 plans

### Phase 32: GraphEditorViewModel Facade Convergence
**Goal**: Continue moving retained-facade orchestration out of `GraphEditorViewModel` while preserving the public SDK surface.
**Depends on**: Phase 31
**Requirements**: FACADE-01, FACADE-02
**Success Criteria**:
1. Public factory/session/view-model entry points remain unchanged while moved orchestration leaves `GraphEditorViewModel`.
2. Kernel-owned state remains canonical and no second mutable runtime owner appears during the refactor.
3. Facade/runtime parity regressions protect moved selection, mutation, menu, and projection behavior while the retained facade becomes materially narrower.
**Plans**: 3 plans

### Phase 33: Kernel, Canvas, And Guardrail Follow-Through
**Goal**: Finish the next hotspot splits around downstream collaborators and tighten the remaining documentation/maintenance debt.
**Depends on**: Phase 32
**Requirements**: FACADE-03, FACADE-04, GUARD-02
**Success Criteria**:
1. `GraphEditorKernel` and `NodeCanvas` hotspot behavior is further isolated behind internal helpers/coordinators without changing public embedding behavior.
2. Contributors can change hotspot collaborators in isolation with focused tests rather than broad cross-surface edits.
3. Touched publishable packages no longer extend the blanket `CS1591` debt; docs or scoped suppressions make the remaining debt boundary explicit.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 30. Milestone History And Refactor Gate Closeout | CLOSE-01, CLOSE-02, GUARD-01 | Complete |
| 31. History And Save Semantic Closure | STATE-01, STATE-02, STATE-03 | Complete |
| 32. GraphEditorViewModel Facade Convergence | FACADE-01, FACADE-02 | Planned |
| 33. Kernel, Canvas, And Guardrail Follow-Through | FACADE-03, FACADE-04, GUARD-02 | Planned |

## Next Action

**Next action:** plan Phase 32 with `$gsd-plan-phase 32`
