# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.4 Plugin Loading and Automation Execution** — Phases 22-25 (planned)
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** — Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** — Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** — Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** — Phases 01-06, shipped before milestone archive split

## Overview

The kernel-first runtime, descriptor contracts, readiness proof, and graph-first demo showcase have all shipped. The next problem is no longer whether hosts can see or understand the seams. The next problem is whether those same canonical session, descriptor, command, and diagnostics surfaces can now support real plugin loading and automation execution without falling back to facade-coupled or Avalonia-coupled APIs.

This milestone turns the deferred `PLUG-01` and `AUTO-01` style follow-on work into the next concrete product increment: a first in-process plugin-loading path, a descriptor-first automation runner, and a proof ring that keeps those claims machine-checkable for package consumers.

## Milestone

**Milestone:** v1.4 Plugin Loading and Automation Execution
**Goal:** Deliver the first real public plugin-loading and automation-execution baseline on top of the shipped kernel-first session boundary, descriptor contracts, and proof-ring discipline.

## Phases

- [ ] **Phase 22: Plugin Composition Contracts** - Publish the first public plugin composition/loading path over `AsterGraphEditorFactory`, options, and canonical runtime descriptors.
- [ ] **Phase 23: Runtime Plugin Integration And Inspection** - Let loaded plugins contribute additive seams and make their loaded state inspectable from the canonical runtime boundary.
- [ ] **Phase 24: Automation Execution Runner** - Add the first descriptor-first automation runner over command IDs, query snapshots, batching, and typed runtime diagnostics/events.
- [ ] **Phase 25: Plugin And Automation Proof Ring** - Lock the new extension story into focused tests, sample hosts, smoke tools, scale proof, and docs.

## Phase Details

### Phase 22: Plugin Composition Contracts
**Goal**: Publish the first public plugin composition/loading path over `AsterGraphEditorFactory`, options, and canonical runtime descriptors.
**Depends on**: v1.3 shipped baseline
**Requirements**: PLUG-01
**Success Criteria**:
1. Hosts can register or load plugins through public factory/options entry points without reaching into `GraphEditorViewModel` or Avalonia control internals.
2. The plugin composition contract stays additive and rooted in the kernel/session boundary rather than introducing a parallel host runtime.
3. Plugin availability is surfaced through canonical descriptors or related inspection reads so hosts can detect the loader baseline intentionally.
**Plans**: 3 plans

### Phase 23: Runtime Plugin Integration And Inspection
**Goal**: Let loaded plugins contribute additive seams and make their loaded state inspectable from the canonical runtime boundary.
**Depends on**: Phase 22
**Requirements**: PLUG-02, PLUG-03
**Success Criteria**:
1. Loaded plugins can contribute additive services, menus, presentation, diagnostics, or similar seams through explicit contracts that compose with the shipped runtime boundary.
2. Hosts can inspect which plugins loaded, what descriptors they expose, and what failures occurred without scraping UI state.
3. The implementation preserves compatibility posture by extending canonical contracts rather than replacing retained host paths abruptly.
**Plans**: 3 plans

### Phase 24: Automation Execution Runner
**Goal**: Add the first descriptor-first automation runner over command IDs, query snapshots, batching, and typed runtime diagnostics/events.
**Depends on**: Phase 23
**Requirements**: AUTO-01, AUTO-02
**Success Criteria**:
1. Hosts can execute automation or macro steps through canonical command IDs and query/batch surfaces rather than `GraphEditorViewModel` methods.
2. Automation progress, failures, and results surface through typed runtime events or diagnostics suitable for headless and non-Avalonia consumers.
3. The automation path remains explicitly session-first and does not require a scripting language or workflow-designer product surface to be useful.
**Plans**: 3 plans

### Phase 25: Plugin And Automation Proof Ring
**Goal**: Lock the new extension story into focused tests, sample hosts, smoke tools, scale proof, and docs.
**Depends on**: Phase 24
**Requirements**: PROOF-01, PROOF-02
**Success Criteria**:
1. `HostSample`, `PackageSmoke`, and focused regression coverage prove plugin composition and automation execution from the canonical host boundary.
2. `ScaleSmoke` or equivalent large-graph proof confirms the automation path remains credible on larger sessions.
3. Docs point hosts to the canonical plugin/automation path and the same proof surfaces used to back the public claims.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 22. Plugin Composition Contracts | PLUG-01 | Planned |
| 23. Runtime Plugin Integration And Inspection | PLUG-02, PLUG-03 | Planned |
| 24. Automation Execution Runner | AUTO-01, AUTO-02 | Planned |
| 25. Plugin And Automation Proof Ring | PROOF-01, PROOF-02 | Planned |

## Next Action

**Phase 22** is planned. The next workflow step is to execute the first public plugin composition path.
