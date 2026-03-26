# Roadmap: AsterGraph

## Overview

AsterGraph v1 moves from a working internal editor into a publishable SDK that external hosts can adopt safely. The roadmap starts with package and compatibility guardrails, then establishes framework-neutral editor contracts, then decomposes the Avalonia shell into embeddable surfaces, and finally hardens host replacement and diagnostics on top of those stable seams.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

- [x] **Phase 1: Consumption & Compatibility Guardrails** - Make package adoption, runtime initialization, and migration safe for external hosts.
- [x] **Phase 2: Runtime Contracts & Service Seams** - Expose framework-neutral editor commands, queries, events, batching, and replaceable services.
- [x] **Phase 3: Embeddable Avalonia Surfaces** - Split the default Avalonia shell into independently hostable editor surfaces.
- [x] **Phase 4: Replaceable Presentation Kit** - Let hosts swap visual presenters while reusing editor behavior and data contracts.
- [ ] **Phase 5: Diagnostics & Integration Inspection** - Give hosts explicit inspection, logging, and troubleshooting contracts.

## Phase Details

### Phase 1: Consumption & Compatibility Guardrails
**Goal**: Hosts can adopt published AsterGraph packages through a clear package boundary, initialize the editor through public entry points, and migrate through a staged compatibility path.
**Depends on**: Nothing (first phase)
**Requirements**: PKG-01, PKG-02, PKG-03
**Success Criteria** (what must be TRUE):
  1. Host can install supported AsterGraph packages on the documented target frameworks and understand which packages form the supported SDK boundary.
  2. Host can initialize the editor runtime and default Avalonia composition through documented public registration or construction APIs instead of sample-only wiring.
  3. Existing host can move onto the reorganized package/API surface through a staged migration path instead of a one-shot rewrite.
**Plans**: 4 plans

Plans:
- [x] 01-01-PLAN.md — Remove the known build blocker and create explicit initialization/migration test entry points.
- [x] 01-02-PLAN.md — Add the public editor/Avalonia initialization factories and prove them through tests plus the host sample.
- [x] 01-03-PLAN.md — Preserve the constructor/view compatibility facade and add migration parity regression plus package smoke coverage.
- [x] 01-04-PLAN.md — Align all consumer docs with the new package boundary and close the phase with packed-package verification.

### Phase 2: Runtime Contracts & Service Seams
**Goal**: Hosts can drive editor behavior through stable runtime contracts in `AsterGraph.Editor` and replace core services without depending on Avalonia control internals.
**Depends on**: Phase 1
**Requirements**: API-01, API-02, API-03, API-04, SERV-01, SERV-02
**Success Criteria** (what must be TRUE):
  1. Host can execute editor commands and batch related mutations through public runtime APIs without calling Avalonia control internals.
  2. Host can query selection, viewport, document snapshot, and active capabilities from stable editor-session contracts.
  3. Host can subscribe to typed runtime events for document, selection, viewport, command, and recoverable-failure changes.
  4. Host can replace documented storage, clipboard, serialization, compatibility, localization, presentation, and diagnostics services, and the defaults work without demo-specific storage roots or host identity assumptions.
**Plans**: 5 plans

Plans:
- [x] 02-01-PLAN.md — Define the runtime session contracts and create Wave 0 session/transaction regression entry points.
- [x] 02-02-PLAN.md — Implement the concrete runtime session, factory session entry, and compatibility-facade bridge.
- [x] 02-03-PLAN.md — Publish replaceable service/diagnostics contracts and the corrected package-neutral storage-default policy.
- [x] 02-04-PLAN.md — Wire the service seams through the runtime stack and prove diagnostics plus host replacement behavior.
- [x] 02-05-PLAN.md — Prove the full Phase 2 host story through samples, smoke coverage, migration regressions, and docs.

### Phase 3: Embeddable Avalonia Surfaces
**Goal**: Hosts can compose only the Avalonia editor surfaces they need while keeping the full default shell available as a convenience composition.
**Depends on**: Phase 2
**Requirements**: EMBD-01, EMBD-02, EMBD-03, EMBD-04, EMBD-05
**Success Criteria** (what must be TRUE):
  1. Host can embed the full default editor shell as a ready-made composition.
  2. Host can embed a standalone graph viewport or canvas surface without also taking inspector, menu, or other shell chrome.
  3. Host can embed the mini map and inspector independently and connect them to the same editor session.
  4. Host can include, replace, or omit default menu and chrome presenters without rebuilding editor state.
**Plans**: 4 plans
**UI hint**: yes

Plans:
- [x] 03-01-PLAN.md — Publish standalone canvas composition, stock menu presenter reuse, and focused surface verification.
- [x] 03-02-PLAN.md — Extract standalone inspector and standalone mini map surfaces with factory-first host entry points.
- [x] 03-03-PLAN.md — Recompose the default full shell over the standalone surfaces while preserving the retained host path.
- [x] 03-04-PLAN.md — Prove and document the full surface matrix through host sample, smoke markers, and consumer docs.

### Phase 4: Replaceable Presentation Kit
**Goal**: Hosts can replace default visual presenters in `AsterGraph.Avalonia` while preserving editor-owned behavior, interaction rules, and data contracts.
**Depends on**: Phase 3
**Requirements**: PRES-01, PRES-02, PRES-03, PRES-04
**Success Criteria** (what must be TRUE):
  1. Host can replace node visuals without reimplementing selection, drag, or connection behavior.
  2. Host can replace context-menu presentation while reusing editor intent and command models.
  3. Host can replace inspector presentation while reusing editor-provided data contracts.
  4. Host can replace mini-map rendering or presentation without forking the editor runtime.
**Plans**: 4 plans
**UI hint**: yes

Plans:
- [x] 04-01-PLAN.md — Define the Avalonia presenter contracts and canonical host option plumbing for stock-default, opt-in replacement.
- [x] 04-02-PLAN.md — Make node visuals and context-menu presentation replaceable while keeping `NodeCanvas` behavior and editor menu intent intact.
- [x] 04-03-PLAN.md — Make inspector and mini-map presentation replaceable while preserving editor-owned inspector projections and viewport navigation.
- [x] 04-04-PLAN.md — Prove the full presenter replacement kit through focused harnesses, sample/smoke markers, migration regressions, and docs.

### Phase 5: Diagnostics & Integration Inspection
**Goal**: Hosts can inspect editor behavior, capture failures, and troubleshoot integrations through stable diagnostics contracts rather than UI-only signals.
**Depends on**: Phase 4
**Requirements**: DIAG-01, DIAG-02, DIAG-03
**Success Criteria** (what must be TRUE):
  1. Host can receive machine-readable warnings, errors, and operation diagnostics without parsing UI status text.
  2. Host can request inspection snapshots or equivalent debug-state output to troubleshoot the current editor/session state.
  3. Host can attach public logging or tracing sinks and observe editor diagnostics through host-standard tooling.
**Plans**: 4 plans

Plans:
- [ ] 05-01-PLAN.md — Define the public diagnostics/inspection contracts, session-root entry point, and focused contract harness.
- [ ] 05-02-PLAN.md — Implement aggregate inspection snapshots and bounded recent-diagnostics publication over the existing runtime/session baseline.
- [ ] 05-03-PLAN.md — Wire opt-in `ILogger`/`ActivitySource` instrumentation through editor options, factory/session composition, and runtime operations.
- [ ] 05-04-PLAN.md — Prove diagnostics and inspection through compatibility regressions, host sample output, smoke markers, focused validation, and docs.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Consumption & Compatibility Guardrails | 4/4 | Complete | 01-01, 01-02, 01-03, 01-04 |
| 2. Runtime Contracts & Service Seams | 5/5 | Complete | 02-01, 02-02, 02-03, 02-04, 02-05 |
| 3. Embeddable Avalonia Surfaces | 4/4 | Complete | 03-01, 03-02, 03-03, 03-04 |
| 4. Replaceable Presentation Kit | 4/4 | Complete | 04-01, 04-02, 04-03, 04-04 |
| 5. Diagnostics & Integration Inspection | 0/4 | Planned | - |
