# Roadmap: AsterGraph v1.1

## Overview

The v1.0 foundation work is complete. AsterGraph now has a publishable package boundary, runtime/session contracts, embeddable Avalonia surfaces, replaceable presentation, diagnostics hooks, and validated sample/smoke coverage. The next milestone focuses on hardening what shipped so hosts can treat AsterGraph as a more production-grade embedded component line.

This roadmap targets four gaps surfaced by the latest review:

- the runtime/session host boundary is still incomplete for serious custom UI hosts
- several host extension seams still expose concrete MVVM implementation types
- the Avalonia full-shell experience still fights host-native command, focus, and input conventions
- large-graph interaction hot paths still rely on graph-wide rebuild/requery behavior

## Milestone

**Milestone:** v1.1 Host Boundary, Native Integration, and Scaling  
**Goal:** Make the shipped SDK foundation easier to extend, more native to embed in desktop hosts, and more scalable under larger graph workloads without rewriting the existing architecture.

## Phases

- [x] **Phase 7: Runtime Host Boundary Completion** - Finish the runtime/session host story so custom UI hosts no longer need the concrete editor facade for core graph interaction.
- [x] **Phase 8: Stable Host Extension Contracts** - Decouple the main host extension seams from MVVM implementation types and stabilize the contract surface.
- [x] **Phase 9: Native Avalonia Host Integration** - Make the full-shell and standalone Avalonia surfaces cooperate with host-native shortcuts, wheel input, focus, and keyboard menu behavior.
- [x] **Phase 10: Canvas And Interaction Hot-Path Scaling** - Remove graph-wide rebuild/requery patterns from drag, connection preview, marquee selection, and small scene deltas.
- [x] **Phase 11: State And History Scaling** - Reduce whole-document and whole-graph recomputation in inspector/status/history/dirty-tracking paths.
- [x] **Phase 12: Proof Ring For Hosts And Large Graphs** - Prove the hardening work through focused regressions, sample output, package smoke, and large-graph validation scenarios.

## Phase Details

### Phase 7: Runtime Host Boundary Completion
**Goal**: Make `IGraphEditorSession` and related runtime contracts self-sufficient for custom UI hosts that do not want to depend on `GraphEditorViewModel`.
**Depends on**: v1.0 foundation
**Requirements**: HOST-01, HOST-02
**Success Criteria**:
1. Custom hosts can perform selection, node placement/movement, connection authoring, and viewport operations through stable runtime/session contracts.
2. Runtime-side compatibility and inspection queries no longer require hosts to consume `NodeViewModel` or `PortViewModel`.
3. The retained `GraphEditorViewModel` path remains available as a compatibility facade rather than the only complete host entry.
**Plans**: 3 plans

### Phase 8: Stable Host Extension Contracts
**Goal**: Move menu and presentation extension seams toward stable host/runtime contracts instead of concrete MVVM implementation types.
**Depends on**: Phase 7
**Requirements**: HOST-03, UX-01
**Success Criteria**:
1. Host menu augmentation does not require `GraphEditorViewModel` as the primary public extension root.
2. Host node/presentation extension does not require `NodeViewModel` as the only public context shape.
3. Full-shell Avalonia hosts can disable or replace stock shortcut/menu behavior through public options instead of forking the control.
**Plans**: 3 plans

### Phase 9: Native Avalonia Host Integration
**Goal**: Make embedded Avalonia surfaces behave more like cooperative native desktop controls.
**Depends on**: Phase 8
**Requirements**: UX-02, UX-03
**Success Criteria**:
1. Wheel and panning behavior can be configured to cooperate with host scroll/input conventions.
2. Keyboard-invoked menus anchor and behave like desktop menus instead of pointer-only popups.
3. Focus behavior across graph items, menus, and command routing is more host-native and keyboard-usable.
**Plans**: 3 plans
**UI hint**: yes

### Phase 10: Canvas And Interaction Hot-Path Scaling
**Goal**: Remove the most obvious graph-wide work from node drag, connection preview, marquee selection, and scene delta handling.
**Depends on**: Phase 9
**Requirements**: PERF-01, PERF-02, PERF-03
**Success Criteria**:
1. Connection rendering no longer fully rebuilds the connection layer on routine drag/preview updates.
2. Marquee selection avoids full-graph selection rewrites on every pointer sample.
3. Node/control-tree updates are incremental enough that small graph deltas do not trigger whole-scene rebuilds.
**Plans**: 4 plans

### Phase 11: State And History Scaling
**Goal**: Reduce non-visual whole-graph and whole-document recomputation in inspector/status/history paths.
**Depends on**: Phase 10
**Requirements**: PERF-04, PERF-05
**Success Criteria**:
1. Inspector/status/computed-state refresh paths avoid repeated global connection/node rescans on unrelated state changes.
2. Dirty tracking and history entry creation avoid expensive whole-document serialization in routine edit flows.
3. The resulting state model still preserves current correctness and compatibility behavior.
**Plans**: 3 plans

### Phase 12: Proof Ring For Hosts And Large Graphs
**Goal**: Validate the hardening work through focused host and scale-oriented proof instead of relying on local reasoning alone.
**Depends on**: Phase 11
**Requirements**: VALID-01, VALID-02
**Success Criteria**:
1. HostSample and PackageSmoke visibly prove the updated host boundary and Avalonia host-integration story.
2. Focused regressions cover the main host-command, input, menu, and presentation behaviors.
3. Repeatable large-graph validation scenarios or targeted harnesses catch regressions in drag, selection, connection, and state-refresh hot paths.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 7. Runtime Host Boundary Completion | HOST-01, HOST-02 | Completed |
| 8. Stable Host Extension Contracts | HOST-03, UX-01 | Completed |
| 9. Native Avalonia Host Integration | UX-02, UX-03 | Completed |
| 10. Canvas And Interaction Hot-Path Scaling | PERF-01, PERF-02, PERF-03 | Completed |
| 11. State And History Scaling | PERF-04, PERF-05 | Completed |
| 12. Proof Ring For Hosts And Large Graphs | VALID-01, VALID-02 | Completed |

## Next Action

Milestone v1.1 is complete. The next action is milestone audit / merge preparation rather than more v1.1 phase execution.
