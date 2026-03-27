# Requirements: AsterGraph

**Defined:** 2026-03-25
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## v1 Requirements

### Package Consumption

- [x] **PKG-01**: Host can consume published AsterGraph packages on supported target frameworks with a documented package boundary and version/support story
- [x] **PKG-02**: Host can initialize editor runtime and Avalonia components through documented registration or construction APIs instead of demo-only wiring patterns
- [x] **PKG-03**: Existing hosts can migrate to the reorganized API surface through a staged compatibility path rather than a single breaking rewrite

### Embeddable Surfaces

- [x] **EMBD-01**: Host can embed the full default editor shell as a convenience composition
- [x] **EMBD-02**: Host can embed a standalone graph viewport or canvas surface without the full shell
- [x] **EMBD-03**: Host can embed the mini map independently of the full shell
- [x] **EMBD-04**: Host can embed the inspector independently of the full shell
- [x] **EMBD-05**: Host can embed or omit default menu and chrome presenters without rebuilding editor state

### Public Runtime API

- [x] **API-01**: Host can execute editor commands through stable public APIs instead of invoking Avalonia control internals
- [x] **API-02**: Host can query current selection, viewport, document snapshot, and active capabilities through stable public APIs
- [x] **API-03**: Host can subscribe to typed events for document changes, selection changes, viewport changes, command execution, and recoverable failures
- [x] **API-04**: Host can group related editor mutations into a transaction or equivalent batched operation surface

### Replaceable Presentation

- [x] **PRES-01**: Host can replace node visuals without reimplementing selection, drag, or connection behavior
- [x] **PRES-02**: Host can replace context-menu presentation while reusing menu intent and command models
- [x] **PRES-03**: Host can replace inspector presentation while reusing editor-provided data contracts
- [x] **PRES-04**: Host can replace mini-map rendering or presentation without forking the editor runtime

### Replaceable Services

- [x] **SERV-01**: Host can replace storage, clipboard, serialization, compatibility, localization, presentation, and diagnostics services through documented interfaces or options
- [x] **SERV-02**: Default service implementations do not rely on demo-branded storage roots or host identity assumptions

### Diagnostics

- [ ] **DIAG-01**: Host can receive machine-readable warnings, errors, and operation diagnostics without parsing UI status text
- [ ] **DIAG-02**: Host can request inspection snapshots or equivalent debug-state output for troubleshooting integrations
- [ ] **DIAG-03**: Host can attach logging or tracing sinks through public diagnostics contracts

## v2 Requirements

### Automation And Tooling

- **AUTO-01**: Host can run scripted editor workflows and macro-style automation through a richer public automation surface
- **AUTO-02**: Host can use a dedicated diagnostics workbench UI built on top of the public diagnostics contracts

### Capability Expansion

- **CAPS-01**: Host can detect optional SDK capabilities through explicit versioned capability contracts
- **CAPS-02**: Host can extend the editor through runtime plugin loading on top of the stabilized public API surface

## Out of Scope

| Feature | Reason |
|---------|--------|
| New end-user graph editing features unrelated to extensibility | This milestone is about SDK modularization and host integration, not expanding editing scope |
| Replacing Avalonia with another UI stack | The current goal is to decouple and modularize the Avalonia offering, not to build a second presentation stack |
| Full diagnostics workbench UI in v1 | Useful, but the stable diagnostics contracts must exist first |
| Runtime plugin loading in v1 | Depends on a stable public API and package boundary, so it stays deferred |
| Algorithm execution engine | Separate product direction from the component-library refactor |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| PKG-01 | Phase 1 | Complete |
| PKG-02 | Phase 1 | Complete |
| PKG-03 | Phase 1 | Complete |
| EMBD-01 | Phase 3 | Complete |
| EMBD-02 | Phase 3 | Complete |
| EMBD-03 | Phase 3 | Complete |
| EMBD-04 | Phase 3 | Complete |
| EMBD-05 | Phase 3 | Complete |
| API-01 | Phase 2 | Complete |
| API-02 | Phase 2 | Complete |
| API-03 | Phase 2 | Complete |
| API-04 | Phase 2 | Complete |
| PRES-01 | Phase 4 | Complete |
| PRES-02 | Phase 4 | Complete |
| PRES-03 | Phase 4 | Complete |
| PRES-04 | Phase 4 | Complete |
| SERV-01 | Phase 2 | Complete |
| SERV-02 | Phase 2 | Complete |
| DIAG-01 | Phase 5 | Pending |
| DIAG-02 | Phase 5 | Pending |
| DIAG-03 | Phase 5 | Pending |

**Coverage:**
- v1 requirements: 21 total
- Mapped to phases: 21
- Unmapped: 0

---
*Requirements defined: 2026-03-25*
*Last updated: 2026-03-26 after phase 3 completion*
