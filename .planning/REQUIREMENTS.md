# Requirements: AsterGraph v1.6

**Defined:** 2026-04-16
**Milestone:** v1.6 Facade Convergence and Proof Guardrails
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Milestone Closeout

- [x] **CLOSE-01**: Maintainer can archive the shipped `v1.4` milestone into checked-in milestone files so current planning artifacts no longer imply contradictory active-vs-archived states
- [x] **CLOSE-02**: Maintainer can identify the live proof ring, carried concerns, and next milestone entry point from current planning/docs without consulting stale phase directories

### Runtime Facade Convergence

- [ ] **FACADE-01**: Host can keep using the current public factory, session, and retained `GraphEditorViewModel` / `GraphEditorView` entry points while hotspot refactors move remaining orchestration out of `GraphEditorViewModel`
- [ ] **FACADE-02**: Internal runtime mutations, selection flow, and menu/projection orchestration that move during refactoring continue to execute against kernel-owned state instead of introducing a second mutable runtime owner
- [ ] **FACADE-03**: Contributors can change one `GraphEditorViewModel` hotspot collaborator at a time because the remaining responsibilities are split into narrower internal seams with focused tests
- [ ] **FACADE-04**: Contributors can continue hotspot reduction in `GraphEditorKernel` and `NodeCanvas` without changing public embedding behavior because cross-cutting responsibilities are isolated behind internal coordinators or helpers

### History And Save Semantics

- [ ] **STATE-01**: Host sees one explicit undo/redo/dirty/save contract across retained facade flows, including save-boundary behavior after undo/redo
- [ ] **STATE-02**: Contributors can detect regressions in history interaction, drag, and save-boundary semantics through smaller focused regression tests instead of one broad transaction suite alone
- [ ] **STATE-03**: Machine-checkable proof outputs that cover state/history semantics no longer rely on a carried `STATE_HISTORY_OK` known mismatch

### Maintenance Guardrails

- [x] **GUARD-01**: Contributors can run one checked-in maintenance/refactor gate that exercises the hotspot-sensitive regression surface without manually curating commands
- [ ] **GUARD-02**: Publishable packages touched during hotspot refactors stop extending blanket public XML-doc debt, using real docs or scoped suppressions instead of relying on one repo-wide `CS1591` blanket forever

## Future Requirements

### Deferred Platform Work

- **TRUST-01**: Host can enforce plugin trust, signing, version policy, or isolation rules beyond the first in-process loader baseline
- **MARKET-01**: Host can discover, install, or update plugins through marketplace/feed-oriented workflows
- **SCRIPT-01**: Host can author automation through a dedicated scripting language, script host, or workflow designer instead of command-step composition only

## Out of Scope

| Feature | Reason |
|---------|--------|
| New plugin marketplace, trust/distribution policy, signing, or stronger isolation product work | v1.6 is a contraction milestone, not the next plugin feature band |
| Dedicated scripting language, workflow-designer UI, or broader automation authoring product layers | The shipped command/query automation runner remains the baseline for now |
| New graph-editing end-user features unrelated to facade convergence, state semantics, or maintenance guardrails | This milestone is about internal contraction rather than broadening the editor surface |
| Replacing Avalonia or rewriting the retained compatibility story from scratch | The milestone should harden the current stack instead of reopening product positioning |
| A one-shot removal of all compatibility APIs or other public breaking changes | Staged migration remains part of the product promise |
| Repeating v1.5 baseline work such as `.editorconfig`, central package management, CI setup, or `ScaleSmoke` solution alignment unless current repo evidence shows a live regression | Those tasks already landed in the shipped baseline and are not the current highest-value gap |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| CLOSE-01 | Phase 30 | Complete |
| CLOSE-02 | Phase 30 | Complete |
| FACADE-01 | Phase 32 | Pending |
| FACADE-02 | Phase 32 | Pending |
| FACADE-03 | Phase 33 | Pending |
| FACADE-04 | Phase 33 | Pending |
| STATE-01 | Phase 31 | Pending |
| STATE-02 | Phase 31 | Pending |
| STATE-03 | Phase 31 | Pending |
| GUARD-01 | Phase 30 | Complete |
| GUARD-02 | Phase 33 | Pending |

**Coverage:**
- milestone requirements: 11 total
- mapped to phases: 11
- unmapped: 0

---
*Requirements defined: 2026-04-16*
*Last updated: 2026-04-16 after Phase 30 completion*
