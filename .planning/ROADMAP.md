# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.5 Runtime Boundary Cleanup and Quality Gates** - Phases 26-29 complete, ready to archive
- 🚧 **v1.4 Plugin Loading and Automation Execution** - Phases 22-25 complete, ready to archive
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** - Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** - Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** - Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** - Phases 01-06, shipped before milestone archive split

## Overview

The kernel-first runtime, descriptor contracts, graph-first demo showcase, plugin loading, automation execution, tracked repo-level validation baseline, proof-surface alignment, release-validation lane, and canonical host-adoption path have all shipped. The v1.5 execution work is complete; the remaining milestone task is archive/history follow-through rather than another delivery phase.

This milestone turned the v1.4 baseline into a more durable SDK release posture. The work first tightened the canonical runtime boundary, then established repo-level validation gates, then aligned docs/proof/test lanes with the real tree, and finally closed the loop with automated release validation and a shorter canonical host-adoption path.

## Milestone

**Milestone:** v1.5 Runtime Boundary Cleanup and Quality Gates
**Goal:** Reduce the remaining gap between the canonical runtime boundary and retained compatibility facades, then automate the validation and documentation surface that protects the SDK boundary.

## Phases

- [x] **Phase 26: Runtime Boundary Canonicalization** - Tighten the retained compatibility facade around the kernel/session-owned runtime and continue retiring MVVM-shaped runtime shims. (completed 2026-04-14)
- [x] **Phase 27: Repo Quality Gates And Target Matrix** - Add tracked repo-level quality and CI baseline across the supported package and target-framework surface. (completed 2026-04-14)
- [x] **Phase 28: Proof Surface And Regression Lane Alignment** - Make docs, solution membership, proof tools, and test lanes describe the same verification surface. (completed 2026-04-14)
- [x] **Phase 29: Release Validation And Canonical Adoption Path** - Close the milestone with automated release checks and one synchronized canonical integration path for hosts. (completed 2026-04-14)

## Phase Details

### Phase 26: Runtime Boundary Canonicalization
**Goal**: Tighten the retained compatibility facade around the kernel/session-owned runtime and continue retiring MVVM-shaped runtime shims.
**Depends on**: v1.4 shipped baseline
**Requirements**: BOUND-01, BOUND-02, BOUND-03
**Success Criteria**:
1. Hosts can use canonical DTO/snapshot compatibility queries for connection-target discovery without depending on `CompatiblePortTarget` as the primary runtime surface.
2. Retained `GraphEditorViewModel` / `GraphEditorView` paths continue to run over the same kernel/session-owned runtime state, with proof that canonical and retained paths stay behaviorally aligned.
3. Remaining compatibility-only runtime APIs carry explicit staged migration guidance, stronger deprecation signals, and a documented exit path that does not force a one-shot break.
**Plans**: 3 plans

### Phase 27: Repo Quality Gates And Target Matrix
**Goal**: Add tracked repo-level quality and CI baseline across the supported package and target-framework surface.
**Depends on**: Phase 26
**Requirements**: QUAL-01, QUAL-02
**Success Criteria**:
1. The repo has tracked shared style/package configuration such as editor rules and centralized package version management instead of relying on scattered per-project defaults.
2. Checked-in automation validates the supported package boundary across both `net8.0` and `net9.0` lanes instead of relying mainly on manual local command memory.
3. Contributors can see baseline failures early through one repeatable CI or scripted matrix path rather than ad hoc milestone-only verification.
**Plans**: 3 plans

### Phase 28: Proof Surface And Regression Lane Alignment
**Goal**: Make docs, solution membership, proof tools, and test lanes describe the same verification surface.
**Depends on**: Phase 27
**Requirements**: PROOF-01, PROOF-02
**Success Criteria**:
1. README, planning docs, and tool references describe the same live proof surface, with stale `HostSample` claims either removed or replaced by a real maintained entry point.
2. Core SDK regression coverage is separable from demo/sample integration coverage so failures identify whether the SDK boundary or the showcase host regressed.
3. Solution/project membership and proof-tool guidance are internally consistent, so contributors can tell which tools are first-class verification surfaces and how to run them.
**Plans**: 3 plans

### Phase 29: Release Validation And Canonical Adoption Path
**Goal**: Close the milestone with automated release checks and one synchronized canonical integration path for hosts.
**Depends on**: Phase 28
**Requirements**: QUAL-03, PROOF-03
**Success Criteria**:
1. Release validation automatically checks package smoke, public API or package compatibility, and coverage/reporting expectations instead of depending mainly on README-only manual commands.
2. Hosts can find one short synchronized integration decision path that covers runtime-only usage, default Avalonia UI composition, and staged migration from retained compatibility hosts.
3. The final proof/documentation surface points to the same canonical release-validation commands and integration entry points used by the repo itself.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 26. Runtime Boundary Canonicalization | BOUND-01, BOUND-02, BOUND-03 | Complete |
| 27. Repo Quality Gates And Target Matrix | QUAL-01, QUAL-02 | Complete |
| 28. Proof Surface And Regression Lane Alignment | PROOF-01, PROOF-02 | Complete |
| 29. Release Validation And Canonical Adoption Path | QUAL-03, PROOF-03 | Complete |

## Next Action

**Next action:** archive the completed v1.5 milestone and record its shipped history.
