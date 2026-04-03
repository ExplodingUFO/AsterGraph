# Requirements: AsterGraph v1.1

**Defined:** 2026-04-03
**Milestone:** v1.1 Host Boundary, Native Integration, and Scaling
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Runtime Host Boundary

- [ ] **HOST-01**: Custom UI hosts can perform graph selection, node placement/movement, connection authoring, and viewport navigation through stable runtime/session contracts without directly depending on `GraphEditorViewModel`
- [ ] **HOST-02**: Runtime compatibility and graph-inspection queries return stable contracts or DTOs instead of `NodeViewModel` / `PortViewModel`
- [ ] **HOST-03**: Host extension seams for menus and node presentation bind to stable host/runtime abstractions rather than concrete MVVM types

### Avalonia Host Integration

- [ ] **UX-01**: The full-shell Avalonia host path can disable or replace stock shortcut routing and stock context-menu behavior just as the standalone canvas path can
- [ ] **UX-02**: Wheel, panning, and related pointer gestures can cooperate with host scroll/input conventions instead of always consuming input
- [ ] **UX-03**: Keyboard-invoked menus, focus behavior, and graph-item interaction feel like native desktop control behavior instead of pointer-only custom canvas behavior

### Performance And Scale

- [ ] **PERF-01**: Connection rendering and connection preview updates avoid whole-layer rebuilds plus repeated linear node lookups on drag/preview hot paths
- [ ] **PERF-02**: Marquee selection avoids graph-wide selection recomputation and full projection rebuilds on every pointer sample
- [ ] **PERF-03**: Canvas scene updates avoid whole-scene node/control-tree rebuilds for small graph deltas and scale better with larger node/port counts
- [ ] **PERF-04**: Inspector/status/computed-state refresh avoids unrelated graph-wide rescans when viewport or selection changes
- [ ] **PERF-05**: History, dirty tracking, and snapshot validation avoid full-document serialization costs on routine edit flows

### Proof And Validation

- [ ] **VALID-01**: Host-boundary and Avalonia host-integration changes are covered by focused regressions plus HostSample and PackageSmoke validation
- [ ] **VALID-02**: Large-graph performance/scaling behavior is checked by repeatable validation scenarios or targeted harnesses so regressions are caught before release

## Deferred / Later

- **PLUG-01**: Runtime plugin loading over the stabilized public SDK boundary
- **AUTO-01**: Rich automation/macro workflows over a broader runtime control plane
- **WB-01**: Dedicated diagnostics workbench UI on top of the public diagnostics contracts

## Out of Scope

| Feature | Reason |
|---------|--------|
| New end-user editing features unrelated to host boundary, native integration, or scaling | This milestone is about hardening what already exists |
| Replacing Avalonia with another UI stack | The goal is to improve the current Avalonia host story, not start a second presentation stack |
| Wholesale visual redesign of the shipped shell | Native-feeling interaction and host cooperation matter more than visual novelty here |
| Runtime plugin loading in v1.1 | Depends on a thinner and more stable host/runtime boundary first |
| Algorithm execution engine | Separate product direction from current SDK hardening work |

## Traceability

| Requirement | Planned Phase | Status |
|-------------|---------------|--------|
| HOST-01 | Phase 7 | Planned |
| HOST-02 | Phase 7 | Planned |
| HOST-03 | Phase 8 | Planned |
| UX-01 | Phase 8 | Planned |
| UX-02 | Phase 9 | Planned |
| UX-03 | Phase 9 | Planned |
| PERF-01 | Phase 10 | Planned |
| PERF-02 | Phase 10 | Planned |
| PERF-03 | Phase 10 | Planned |
| PERF-04 | Phase 11 | Planned |
| PERF-05 | Phase 11 | Planned |
| VALID-01 | Phase 12 | Planned |
| VALID-02 | Phase 12 | Planned |

**Coverage:**
- milestone requirements: 13 total
- mapped to phases: 13
- unmapped: 0

---
*Requirements defined: 2026-04-03*
