# Requirements: AsterGraph v1.7

**Defined:** 2026-04-16
**Milestone:** v1.7 Consumer Closure / Release Hardening
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Narrative And Truth Alignment

- [x] **ALIGN-01**: Maintainer can read `README`, `ROADMAP`, `STATE`, `PROJECT`, and current codebase maps without seeing conflicting milestone status, proof-ring composition, or shipped capability claims
- [x] **ALIGN-02**: Consumer-facing docs no longer contain internal contradictions around capability vs non-goal statements, target-framework support, or proof-tool availability

### Proof Ring And Release Gates

- [x] **PROOF-01**: The official proof ring is explicitly defined around real, discoverable entry points for `PackageSmoke`, `ScaleSmoke`, focused regressions, and a minimal consumer host path
- [x] **REL-01**: Maintainer can run one official verification entry point that executes build, test, pack, smoke, and compatibility checks instead of stitching together README-only commands
- [x] **REL-02**: CI covers the supported `net8.0` / `net9.0` matrix with explicit build, smoke, and focused contract lanes so framework-specific regressions surface automatically
- [x] **REL-03**: Publishable package compatibility remains machine-guarded during release verification through public API and/or package validation checks

### Consumer Adoption And State Contracts

- [ ] **CONS-01**: Consumer can open one minimal host sample that proves the canonical package-consumption path without relying on the demo shell or project-reference-only setup
- [ ] **CONS-02**: Consumer docs provide short, explicit routes for runtime-only, default Avalonia UI, plugin trust/discovery, and automation adoption, each with required packages and verification steps
- [ ] **HIST-01**: History/save/dirty semantics are documented as an explicit product contract instead of living only inside tests or carried planning concerns
- [ ] **HIST-02**: Focused history/save proof remains part of the official verification gate so semantic regressions fail automation instead of surviving as known drift

### Maintainability And Extension Contracts

- [ ] **MAINT-01**: `GraphEditorViewModel` can continue shrinking as a compatibility facade without reopening runtime ownership drift or creating a second mutable state owner
- [ ] **MAINT-02**: Core contract, proof/integration, and demo/sample test lanes are more clearly separated so failures localize to the right maintenance surface
- [ ] **EXT-01**: Obsolete compatibility shims have a documented retirement plan that tells consumers what to migrate to and when removals are expected
- [ ] **EXT-02**: Extension precedence and stability rules are documented so consumers know which surfaces are stable contracts, which are migration bridges, and how host overrides interact with plugin contributions

## Future Requirements

### Deferred Platform Work

- **TRUST-01**: Host can enforce deeper plugin trust, distribution, signing, or stronger isolation workflows beyond the shipped in-process policy baseline
- **MARKET-01**: Host can discover, install, and update plugins through marketplace/feed-oriented workflows
- **SCRIPT-01**: Host can author automation through richer scripting or workflow-designer surfaces beyond the shipped command/query runner baseline

## Out of Scope

| Feature | Reason |
|---------|--------|
| New end-user graph-editing capabilities unrelated to consumer closure, release hardening, or maintenance boundaries | v1.7 is a productization-closeout milestone, not a broad feature-expansion band |
| Replacing Avalonia or re-arguing kernel/session runtime ownership | The current runtime-first direction is already the shipped baseline and should not be reopened |
| One-shot removal of all compatibility APIs | Migration remains staged; this milestone should document and prepare retirement rather than force an abrupt break |
| Marketplace UX, remote plugin distribution UI, or stronger trust-isolation products | Those are later platform bets after the consumer/release closure work lands |
| Workflow-designer UI or deeper automation authoring product layers | The shipped automation runner remains the baseline while this milestone tightens consumer closure |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| ALIGN-01 | Phase 34 | Completed |
| ALIGN-02 | Phase 34 | Completed |
| PROOF-01 | Phase 34 | Completed |
| REL-01 | Phase 35 | Completed |
| REL-02 | Phase 35 | Completed |
| REL-03 | Phase 35 | Completed |
| CONS-01 | Phase 36 | Pending |
| CONS-02 | Phase 36 | Pending |
| HIST-01 | Phase 36 | Pending |
| HIST-02 | Phase 36 | Pending |
| MAINT-01 | Phase 37 | Pending |
| MAINT-02 | Phase 37 | Pending |
| EXT-01 | Phase 37 | Pending |
| EXT-02 | Phase 37 | Pending |

**Coverage:**
- milestone requirements: 14 total
- mapped to phases: 14
- unmapped: 0

---
*Requirements defined: 2026-04-16*
*Last updated: 2026-04-16 after Phase 35 completion*
