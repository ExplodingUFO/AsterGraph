# Roadmap: AsterGraph

## Milestones

- 🚧 **v1.10 Public Repo Hygiene and Documentation Surface** - ready to plan
- ✅ **[v1.9 Public Launch Gate and CI Stabilization](./milestones/v1.9-ROADMAP.md)** - Phases 42-44, shipped 2026-04-16, archived 2026-04-16
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

v1.9 already closed the public-alpha launch gate: CI is green on hosted runners, the prerelease workflow validates correctly, `HostSample` proves packed `.NET 10` compatibility, and the public docs plus launch checklist describe a coherent external story.

The remaining problem is the public repository surface itself. The repo still tracks `.planning/`, `AGENTS.md`, `CLAUDE.md`, `build.log`, and the real `NuGet.config`, while README still points at `.planning` as maintainer context. v1.10 exists to remove those workflow traces from the public git surface, migrate any still-useful high-level guidance into normal docs, and leave one clean open-source entry story behind.

## Milestone

**Milestone:** v1.10 Public Repo Hygiene and Documentation Surface
**Goal:** Remove internal planning and AI-workflow traces from the tracked public repo, then reorganize the remaining public docs so external readers only see normal open-source project surfaces.

## Phases

- [ ] **Phase 45: Public Repo Hygiene Baseline** - Remove tracked internal workflow artifacts and lock in ignore rules so they do not re-enter the public repo.
- [ ] **Phase 46: Public Docs Migration** - Migrate any still-public roadmap, status, and launch guidance out of `.planning/` into `docs/en` and `docs/zh-CN`.
- [ ] **Phase 47: Public Entry And Hygiene Gate** - Tighten README and bilingual entry points, preserve the proof/sample story, and add one explicit hygiene verification pass.

## Phase Details

### Phase 45: Public Repo Hygiene Baseline
**Goal**: Remove tracked internal workflow artifacts and lock in ignore rules so they do not re-enter the public repo.
**Depends on**: v1.9 archived baseline
**Requirements**: REPO-01, REPO-02
**Success Criteria**:
1. `.planning/`, `AGENTS.md`, `CLAUDE.md`, `build.log`, and the real `NuGet.config` are no longer tracked in the public repository.
2. `.gitignore` explicitly blocks internal workflow files, local config files, and stray logs from being recommitted.
3. The cleanup is done through normal git history, without relying on uncommitted local-only removal.
**Plans**: 3 plans

### Phase 46: Public Docs Migration
**Goal**: Migrate any still-public roadmap, status, and launch guidance out of `.planning/` into `docs/en` and `docs/zh-CN`.
**Depends on**: Phase 45
**Requirements**: DOCS-01, DOCS-02
**Success Criteria**:
1. Any public roadmap, project-status, or launch guidance that should remain visible has a normal docs home outside `.planning/`.
2. English and `zh-CN` docs each expose a clear landing path for status and roadmap context.
3. The migration preserves useful public context instead of deleting it with the internal planning tree.
**Plans**: 3 plans

### Phase 47: Public Entry And Hygiene Gate
**Goal**: Tighten README and bilingual entry points, preserve the proof/sample story, and add one explicit hygiene verification pass.
**Depends on**: Phase 46
**Requirements**: REPO-03, DOCS-03, ENTRY-01, ENTRY-02, ENTRY-03
**Success Criteria**:
1. README and `README.zh-CN.md` point only at public docs, public samples, proof tools, workflows, and governance files.
2. `HostSample`, `AsterGraph.Demo`, `PackageSmoke`, and `ScaleSmoke` remain clearly positioned after the cleanup.
3. One checked-in hygiene verification path proves the cleaned public repo no longer tracks internal workflow traces or unsafe local config files.
**Plans**: 3 plans

## Progress

| Phase | Requirements | Status |
|-------|--------------|--------|
| 45. Public Repo Hygiene Baseline | REPO-01, REPO-02 | Planned |
| 46. Public Docs Migration | DOCS-01, DOCS-02 | Planned |
| 47. Public Entry And Hygiene Gate | REPO-03, DOCS-03, ENTRY-01, ENTRY-02, ENTRY-03 | Planned |

## Next Action

**Next action:** plan Phase 45
