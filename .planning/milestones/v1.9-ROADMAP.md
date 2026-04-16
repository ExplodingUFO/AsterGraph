# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.9 Public Launch Gate and CI Stabilization** - All phases complete, ready to audit
- ✅ **[v1.8 Public Alpha Readiness and Canonical Demo](./milestones/v1.8-ROADMAP.md)** - Phases 38-41, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.7 Consumer Closure / Release Hardening](./milestones/v1.7-ROADMAP.md)** - Phases 34-37, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.6 Facade Convergence and Proof Guardrails](./milestones/v1.6-ROADMAP.md)** - Phases 30-33, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.5 Runtime Boundary Cleanup and Quality Gates](./milestones/v1.5-ROADMAP.md)** - Phases 26-29, shipped 2026-04-14
- ✅ **[v1.4 Plugin Loading and Automation Execution](./milestones/v1.4-ROADMAP.md)** - Phases 22-25, shipped 2026-04-08, archived 2026-04-16
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** - Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** - Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** - Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** - Phases 01-06, shipped before milestone archive split

## Overview

v1.8 already shipped the external-facing alpha surface: canonical host composition, `HostSample`, showcase demo, bilingual public docs, governance files, and a tag-driven prerelease workflow. The remaining blockers are now operational rather than architectural.

The first GitHub-hosted runs after archiving v1.8 exposed three concrete gaps that still block a confident public opening:
- plugin proof tests still bind to `tests/AsterGraph.TestPlugins/bin/Debug/net9.0/...` on clean runners
- `actions/setup-dotnet` cache cleanup can fail because the expected global package path does not exist
- `.github/workflows/release.yml` is failing before any jobs schedule
- there is still no explicit `.NET 10` consumer compatibility proof in the checked-in validation story

v1.9 exists to close those blockers with the smallest possible scope, then publish one explicit public-launch checklist instead of reopening feature work.

## Milestone

**Milestone:** v1.9 Public Launch Gate and CI Stabilization
**Goal:** Make the shipped `0.2.0-alpha.1` surface operationally ready for a public repo opening by fixing clean-runner CI drift, stabilizing the prerelease workflow, and documenting one short launch checklist.

## Phases

- [ ] **Phase 42: Clean Runner CI Parity** - Remove hidden local-output assumptions from plugin proof tests and make the current `ci.yml` validation lanes representative of clean GitHub-hosted runners.
- [ ] **Phase 43: Workflow And Cache Stabilization** - Fix the `.NET` cache cleanup failure and make the prerelease workflow valid, tag-scoped, and operationally clear.
- [ ] **Phase 44: Public Launch Checklist And Entry Tightening** - Publish the short public-launch checklist and tighten README/alpha-status messaging around the true remaining blockers and current host-entry guidance.

## Phase Details

### Phase 42: Clean Runner CI Parity
**Goal**: Remove hidden local-output assumptions from plugin proof tests and make the current `ci.yml` validation lanes representative of clean GitHub-hosted runners.
**Depends on**: v1.8 archived baseline
**Requirements**: CI-01, CI-02
**Success Criteria**:
1. Plugin discovery/loading/inspection/package-staging tests stop hard-coding `bin/Debug/net9.0` payload paths and instead consume deterministic built test-plugin outputs.
2. The Windows and Linux `all` / `contract` validations exercise the same proof assumptions locally and on clean GitHub-hosted runners.
3. The current CI failure can be reproduced and closed without introducing a second validation path.
**Plans**: 3 plans

### Phase 43: Workflow And Cache Stabilization
**Goal**: Fix the `.NET` cache cleanup failure and make the prerelease workflow valid, tag-scoped, and operationally clear.
**Depends on**: Phase 42
**Requirements**: CI-03, CI-04, REL-01, REL-02, REL-03
**Success Criteria**:
1. `actions/setup-dotnet` no longer fails during post-job cleanup on successful matrix jobs.
2. `.github/workflows/release.yml` evaluates successfully and only schedules work on tags or manual dispatch.
3. One checked-in `.NET 10` consumer compatibility proof runs from the same validation story as the other launch gates.
4. Branch CI and tag prerelease publication have one explicit, non-contradictory operational story.
**Plans**: 3 plans

### Phase 44: Public Launch Checklist And Entry Tightening
**Goal**: Publish the short public-launch checklist and tighten README/alpha-status messaging around the true remaining blockers and current host-entry guidance.
**Depends on**: Phase 43
**Requirements**: OSS-01, OSS-02, OSS-03
**Success Criteria**:
1. README and alpha-status docs describe only the real launch blockers left after CI/workflow stabilization.
2. The repo contains one small launch checklist covering visibility, required checks, first prerelease tag, and artifact review.
3. `HostSample` vs `Demo` responsibilities and canonical-path guidance remain explicit after the launch-gate cleanup.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 42. Clean Runner CI Parity | CI-01, CI-02 | Complete |
| 43. Workflow And Cache Stabilization | CI-03, CI-04, REL-01, REL-02, REL-03 | Complete |
| 44. Public Launch Checklist And Entry Tightening | OSS-01, OSS-02, OSS-03 | Complete |

## Next Action

**Next action:** audit and archive v1.9
