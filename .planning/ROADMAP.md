# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.8 Public Alpha Readiness and Canonical Demo** - Phase 40 in progress
- ✅ **[v1.7 Consumer Closure / Release Hardening](./milestones/v1.7-ROADMAP.md)** - Phases 34-37, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.6 Facade Convergence and Proof Guardrails](./milestones/v1.6-ROADMAP.md)** - Phases 30-33, shipped 2026-04-16, archived 2026-04-16
- ✅ **[v1.5 Runtime Boundary Cleanup and Quality Gates](./milestones/v1.5-ROADMAP.md)** - Phases 26-29, shipped 2026-04-14
- ✅ **[v1.4 Plugin Loading and Automation Execution](./milestones/v1.4-ROADMAP.md)** - Phases 22-25, shipped 2026-04-08, archived 2026-04-16
- ✅ **[v1.3 Demo Showcase](./milestones/v1.3-ROADMAP.md)** - Phases 19-21, shipped 2026-04-08
- ✅ **[v1.2 Kernel Extraction, Capability Contracts, and Plugin Readiness](./milestones/v1.2-ROADMAP.md)** - Phases 13-18, shipped 2026-04-08
- ✅ **v1.1 Host Boundary, Native Integration, and Scaling** - Phases 07-12, shipped before milestone archive split
- ✅ **v1.0 Foundation Milestone** - Phases 01-06, shipped before milestone archive split

## Overview

AsterGraph already ships a credible SDK baseline: the four-package boundary is real, the canonical factory/session path exists, plugin loading and automation execute on the runtime boundary, `HostSample` proves minimal adoption, and the release lane runs pack, smoke, coverage, and compatibility validation.

The next gap is public-alpha readiness. v1.8 therefore focuses on making the shipped baseline understandable and usable for outside consumers: one public version story, explicit open-source collaboration files, a canonical-path demo that actually showcases plugins and automation, bilingual public docs, and a tag-driven prerelease release flow with public artifacts.

## Milestone

**Milestone:** v1.8 Public Alpha Readiness and Canonical Demo
**Goal:** Turn the already-shipped SDK baseline into a repo and demo that are suitable for public alpha evaluation, adoption, and contribution without reopening the runtime-boundary work that v1.2-v1.7 already settled.

## Phases

- [ ] **Phase 38: Alpha Framing And OSS Baseline** - Unify public versioning and alpha-status language, then add the public governance files and SDK pinning that an open alpha needs.
- [ ] **Phase 39: Canonical Demo And Capability Showcase** - Move the main demo onto the canonical host path and make plugin, automation, standalone-surface, and presenter-replacement capabilities visible instead of implied.
- [ ] **Phase 40: Bilingual Docs And Localization Proof** - Publish the core public guides in English and `zh-CN`, and make the demo's localization seam visible through a real language toggle plus proof coverage.
- [ ] **Phase 41: Public Alpha Release Closure** - Extend CI and release automation for public alpha consumption with Linux validation, artifacts, and tag-driven prerelease publishing.

## Phase Details

### Phase 38: Alpha Framing And OSS Baseline
**Goal**: Unify public versioning and alpha-status language, then add the public governance files and SDK pinning that an open alpha needs.
**Depends on**: v1.7 archived baseline
**Requirements**: FRAM-01, FRAM-02, FRAM-03, OSS-01, OSS-02
**Success Criteria**:
1. Public package metadata, README, planning artifacts, and consumer docs all describe the same public-alpha version story.
2. External readers can find one explicit alpha-status and known-limitations contract without relying on milestone archaeology.
3. `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, issue/PR templates, and `global.json` exist and match the repo's real branch, lane, and SDK expectations.
**Plans**: 3 plans

### Phase 39: Canonical Demo And Capability Showcase
**Goal**: Move the main demo onto the canonical host path and make plugin, automation, standalone-surface, and presenter-replacement capabilities visible instead of implied.
**Depends on**: Phase 38
**Requirements**: DEMO-01, DEMO-02, DEMO-03, DEMO-04, CONS-01
**Success Criteria**:
1. `AsterGraph.Demo` no longer uses direct `new GraphEditorViewModel(...)` construction as its main host route.
2. Plugin trust/discovery/loading and automation execution are visible in the demo as real user-facing surfaces rather than test-only or doc-only claims.
3. Standalone surfaces and presenter replacement are demonstrated as operable routes, and the demo clearly points minimal consumers toward `HostSample`.
**Plans**: 3 plans

### Phase 40: Bilingual Docs And Localization Proof
**Goal**: Publish the core public guides in English and `zh-CN`, and make the demo's localization seam visible through a real language toggle plus proof coverage.
**Depends on**: Phase 39
**Requirements**: DOCS-01, L10N-01, TEST-01
**Success Criteria**:
1. The core public guides exist in paired English and `zh-CN` forms under a stable docs structure.
2. The demo can switch between Chinese and English, and that switch proves host/editor localization seams rather than hardcoded mixed-language copy.
3. `AsterGraph.Demo.Tests` covers the canonical demo route, plugin and automation panes, standalone/presenter routes, and bilingual toggle.
**Plans**: 3 plans

### Phase 41: Public Alpha Release Closure
**Goal**: Extend CI and release automation for public alpha consumption with Linux validation, artifacts, and tag-driven prerelease publishing.
**Depends on**: Phase 40
**Requirements**: OSS-03, REL-01, REL-02
**Success Criteria**:
1. CI adds public-friendly concurrency, restore caching, artifact uploads, and at least one Linux validation lane without weakening the Windows release lane.
2. Tag-triggered prerelease workflows publish the public alpha path while pull requests remain verification-only.
3. Public release artifacts include smoke markers, coverage summary, and verification outputs that let outside consumers inspect what shipped.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 38. Alpha Framing And OSS Baseline | FRAM-01, FRAM-02, FRAM-03, OSS-01, OSS-02 | Completed |
| 39. Canonical Demo And Capability Showcase | DEMO-01, DEMO-02, DEMO-03, DEMO-04, CONS-01 | Completed |
| 40. Bilingual Docs And Localization Proof | DOCS-01, L10N-01, TEST-01 | In progress |
| 41. Public Alpha Release Closure | OSS-03, REL-01, REL-02 | Planned |

## Next Action

**Next action:** publish bilingual docs and wire the demo language toggle for Phase 40
