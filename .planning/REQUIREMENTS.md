# Requirements: AsterGraph v1.5

**Defined:** 2026-04-14
**Milestone:** v1.5 Runtime Boundary Cleanup and Quality Gates
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Runtime Boundary

- [ ] **BOUND-01**: Host can discover compatible connection targets through canonical DTO/snapshot queries without depending on `CompatiblePortTarget` or other MVVM-shaped public runtime types
- [ ] **BOUND-02**: Retained `GraphEditorViewModel` / `GraphEditorView` hosts continue to work as compatibility facades over the same kernel/session-owned runtime state instead of implying a separate runtime path
- [ ] **BOUND-03**: Host receives explicit migration guidance and compiler-visible deprecation signals for remaining compatibility-only runtime APIs, with a documented staged removal plan

### Quality Gates

- [ ] **QUAL-01**: Contributors build and test the repo under one tracked source/package configuration baseline, including shared editor rules and centralized package version management
- [ ] **QUAL-02**: The supported package boundary is validated automatically for both `net8.0` and `net9.0` through checked-in CI or equivalent scripted automation
- [ ] **QUAL-03**: Release validation automatically checks package smoke, coverage reporting or thresholds, and public API or package-compatibility regressions instead of relying mainly on manual README commands

### Docs, Proof, And Samples

- [ ] **PROOF-01**: README, planning docs, solution/project lists, and proof tooling reference the same current verification surface with no stale sample or tool claims
- [ ] **PROOF-02**: Core SDK regression coverage is distinguishable from demo/sample integration coverage so failures reveal whether the SDK boundary or the showcase host regressed
- [ ] **PROOF-03**: Host can find a short canonical integration path for runtime-only, default UI, and migration scenarios from one synchronized doc or proof entry point

## Future Requirements

### Deferred Platform Work

- **TRUST-01**: Host can enforce plugin trust, signing, version policy, or isolation rules beyond the first in-process loader baseline
- **MARKET-01**: Host can discover, install, or update plugins through marketplace/feed-oriented workflows
- **SCRIPT-01**: Host can author automation through a dedicated scripting language, script host, or workflow designer instead of command-step composition only

## Out of Scope

| Feature | Reason |
|---------|--------|
| New graph-editing end-user features unrelated to boundary cleanup, release validation, or host integration clarity | v1.5 is about SDK hardening rather than broadening the editor feature surface |
| Plugin marketplace, remote distribution, signing, trust UI, or stronger isolation policy work | Those remain follow-on platform investments after the current boundary and validation work lands |
| Dedicated scripting language, workflow-designer UI, or richer automation authoring product layers | The shipped command/query automation runner remains the baseline for now |
| Replacing Avalonia or rewriting the retained compatibility story from scratch | This milestone should harden the current stack rather than reopen product positioning |
| A one-shot removal of all compatibility APIs | Staged migration remains part of the product promise until stronger warnings, docs, and proof close the gap |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| BOUND-01 | Pending roadmap | Pending |
| BOUND-02 | Pending roadmap | Pending |
| BOUND-03 | Pending roadmap | Pending |
| QUAL-01 | Pending roadmap | Pending |
| QUAL-02 | Pending roadmap | Pending |
| QUAL-03 | Pending roadmap | Pending |
| PROOF-01 | Pending roadmap | Pending |
| PROOF-02 | Pending roadmap | Pending |
| PROOF-03 | Pending roadmap | Pending |

**Coverage:**
- milestone requirements: 9 total
- mapped to phases: 0
- unmapped: 9

---
*Requirements defined: 2026-04-14*
*Last updated: 2026-04-14 after initial definition*
