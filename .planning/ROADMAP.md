# Roadmap: AsterGraph v1.2

## Overview

The v1.1 hardening milestone is complete. AsterGraph now has a substantially stronger host boundary, more native Avalonia embedding behavior, better scaling under larger graphs, and an explicit proof ring.

The next architectural risk is different: the product still behaves like a `GraphEditorViewModel`-centered system with a session facade, rather than a true editor kernel with adapters on top. That limits automation, capability evolution, plugin loading, and future non-shell hosting because too much public behavior still flows through mutable MVVM state and Avalonia-oriented assumptions.

This roadmap therefore focuses on extracting the real editor kernel, normalizing capability/command/menu contracts, and turning the current compatibility façade into an adapter rather than the architectural center of gravity.

## Milestone

**Milestone:** v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness  
**Goal:** Make the editor runtime truly kernel-first, reduce MVVM and Avalonia assumptions in the public control plane, and leave the SDK ready for future plugin and automation work without another deep boundary refactor.

## Phases

- [x] **Phase 13: Editor Kernel State Owner Extraction** - Extract the canonical mutable editor state owner out of `GraphEditorViewModel` so session/runtime composition no longer depends on the VM façade.
- [x] **Phase 14: Session And Compatibility Facade Decoupling** - Rebuild `IGraphEditorSession` and `GraphEditorViewModel` around the extracted kernel so hosts get a kernel-first path and legacy hosts keep a staged adapter path.
- [x] **Phase 15: Capability And Descriptor Contract Normalization** - Replace MVVM-oriented command/menu/state exposure with explicit capability, descriptor, and read-only query contracts.
- [x] **Phase 16: Avalonia Adapter Boundary Cleanup** - Thin the Avalonia layer so shell/canvas/input/clipboard/host-context behavior consumes shared kernel contracts instead of duplicating policy against the VM.
- [x] **Phase 17: Compatibility Lock And Migration Proof** - Prove that the retained `GraphEditorViewModel` / `GraphEditorView` path stays behaviorally aligned while the kernel path becomes canonical.
- [x] **Phase 18: Plugin And Automation Readiness Proof Ring** - Validate the resulting architecture with focused regressions, sample/smoke coverage, and explicit readiness checks for later plugin/automation milestones.

## Phase Details

### Phase 13: Editor Kernel State Owner Extraction
**Goal**: Extract the editor's canonical mutable state owner out of `GraphEditorViewModel`.
**Depends on**: v1.1 completed baseline
**Requirements**: KERN-01, KERN-02
**Success Criteria**:
1. A host can compose the core editor runtime without first constructing `GraphEditorViewModel`.
2. Session/runtime state reads and writes have a kernel-owned source of truth.
3. Kernel extraction does not require an Avalonia dependency.
**Plans**: 3 plans

### Phase 14: Session And Compatibility Facade Decoupling
**Goal**: Turn `IGraphEditorSession` into a kernel-facing API and reduce `GraphEditorViewModel` to an adapter/compatibility façade.
**Depends on**: Phase 13
**Requirements**: KERN-03, CAP-03
**Success Criteria**:
1. `GraphEditorSession` no longer treats `GraphEditorViewModel` as its primary implementation dependency.
2. Public state exposed to hosts is read-only or snapshot-based by default.
3. The compatibility façade still supports staged migration for existing hosts.
**Plans**: 3 plans

### Phase 15: Capability And Descriptor Contract Normalization
**Goal**: Replace MVVM-shaped public control-plane seams with explicit capability, command, and menu descriptors.
**Depends on**: Phase 14
**Requirements**: CAP-01, CAP-02
**Success Criteria**:
1. Capability discovery is explicit rather than inferred from object shape.
2. Editor-layer menu/command contracts no longer require MVVM command objects as the canonical model.
3. Optional surface/service support is versionable through explicit descriptors.
**Plans**: 3 plans

### Phase 16: Avalonia Adapter Boundary Cleanup
**Goal**: Make the Avalonia layer a thinner adapter over shared kernel/facade contracts.
**Depends on**: Phase 15
**Requirements**: ADAPT-01, ADAPT-02
**Success Criteria**:
1. Shell and canvas controls no longer duplicate command/input policy unnecessarily.
2. Clipboard and host-context adaptation stays UI-owned but is no longer coupled to VM internals more than necessary.
3. Avalonia controls consume thinner contracts and are easier to evolve without runtime-boundary churn.
**Plans**: 3 plans
**UI hint**: yes

### Phase 17: Compatibility Lock And Migration Proof
**Goal**: Prove that the new kernel-first path and the retained façade path stay aligned during migration.
**Depends on**: Phase 16
**Requirements**: MIG-01, MIG-02
**Success Criteria**:
1. Legacy hosts using `GraphEditorViewModel` / `GraphEditorView` still work through the migration window.
2. The kernel-first path is clearly the canonical composition route in tests, samples, and docs.
3. Focused regressions catch drift between canonical and compatibility paths.
**Plans**: 3 plans

### Phase 18: Plugin And Automation Readiness Proof Ring
**Goal**: Validate that the extracted kernel and capability contracts are strong enough to support later plugin and automation work.
**Depends on**: Phase 17
**Requirements**: PLUG-READY-01
**Success Criteria**:
1. Samples, smoke tools, and focused regressions prove the final kernel-first architecture.
2. Readiness checks explicitly cover extension/capability seams needed by later plugin and automation milestones.
3. The milestone closes with a clear next-step runway rather than another hidden façade dependency.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 13. Editor Kernel State Owner Extraction | KERN-01, KERN-02 | Complete |
| 14. Session And Compatibility Facade Decoupling | KERN-03, CAP-03 | Completed |
| 15. Capability And Descriptor Contract Normalization | CAP-01, CAP-02 | Completed |
| 16. Avalonia Adapter Boundary Cleanup | ADAPT-01, ADAPT-02 | Completed |
| 17. Compatibility Lock And Migration Proof | MIG-01, MIG-02 | Completed |
| 18. Plugin And Automation Readiness Proof Ring | PLUG-READY-01 | Completed |

## Next Action

**Milestone v1.2** is ready to complete. All planned phases are now complete, and the proof ring covers migration posture, readiness seam discovery, package-consumption markers, and large-graph session-driven validation. The next workflow step is milestone completion.
