# Requirements: AsterGraph v1.2

**Defined:** 2026-04-04
**Milestone:** v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Kernel Extraction

- [x] **KERN-01**: The canonical runtime/editor state owner can be composed without constructing `GraphEditorViewModel`
- [x] **KERN-02**: `IGraphEditorSession` commands, queries, events, diagnostics, and batching operate over kernel-owned state/contracts rather than delegating through mutable MVVM projections
- [x] **KERN-03**: `GraphEditorViewModel` becomes a compatibility-oriented adapter/facade over the kernel instead of the primary state owner

### Capability And Contract Normalization

- [x] **CAP-01**: Public capability descriptors make optional surfaces, services, and host features explicit instead of forcing hosts to infer support from object shape
- [x] **CAP-02**: Editor-layer command and menu contracts avoid depending on `RelayCommand`, `ObservableCollection`, `NodeTemplateViewModel`, or other MVVM implementation details where stable descriptors/IDs are sufficient
- [x] **CAP-03**: Public graph/selection state exposed to hosts is read-only or snapshot-based by default, so external code cannot mutate core editor state by editing live collections

### Avalonia Adapter Cleanup

- [ ] **ADAPT-01**: `AsterGraph.Avalonia` consumes thinner kernel/facade contracts and stops duplicating command-routing policy between shell and canvas controls
- [ ] **ADAPT-02**: Clipboard, host context, and input-routing seams remain Avalonia-owned adapters, but their editor-facing contract no longer requires direct dependence on `GraphEditorViewModel` internals

### Migration And Readiness

- [ ] **MIG-01**: Existing `GraphEditorViewModel` / `GraphEditorView` hosts keep a staged migration path while the kernel path becomes the canonical composition root
- [ ] **MIG-02**: HostSample, PackageSmoke, and focused regressions prove that the new kernel path and the retained compatibility path stay behaviorally aligned
- [ ] **PLUG-READY-01**: The resulting architecture leaves runtime plugin loading and richer automation on top of explicit kernel/capability seams rather than another round of façade-bound refactors

## Deferred / Later

- **PLUG-01**: Actual runtime plugin loading, discovery, and trust model
- **AUTO-01**: Rich automation/macro APIs over the extracted kernel
- **WB-01**: Dedicated diagnostics workbench UI on top of the public diagnostics/probe surface

## Out of Scope

| Feature | Reason |
|---------|--------|
| New graph-editing end-user features unrelated to kernel extraction or contract cleanup | This milestone is architectural hardening, not feature expansion |
| Replacing Avalonia with another UI stack | The goal is to make the existing Avalonia layer thinner and better-behaved, not to start a second presentation stack |
| Immediate runtime plugin loading in v1.2 | This milestone only creates the kernel and capability seams that plugin loading will depend on |
| Major visual redesign of the shipped shell | The priority is adapter separation and contract cleanup, not visual restyling |

## Traceability

| Requirement | Planned Phase | Status |
|-------------|---------------|--------|
| KERN-01 | Phase 13 | Complete |
| KERN-02 | Phase 13 | Complete |
| KERN-03 | Phase 14 | Complete |
| CAP-01 | Phase 15 | Complete |
| CAP-02 | Phase 15 | Complete |
| CAP-03 | Phase 14 | Complete |
| ADAPT-01 | Phase 16 | Planned |
| ADAPT-02 | Phase 16 | Planned |
| MIG-01 | Phase 17 | Planned |
| MIG-02 | Phase 17 | Planned |
| PLUG-READY-01 | Phase 18 | Planned |

**Coverage:**
- milestone requirements: 11 total
- mapped to phases: 11
- unmapped: 0

---
*Requirements defined: 2026-04-04*
