# Requirements: AsterGraph v1.10

**Defined:** 2026-04-16
**Milestone:** v1.10 Public Repo Hygiene and Documentation Surface
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Public Repo Hygiene

- [ ] **REPO-01**: The public repository no longer tracks internal workflow artifacts such as `.planning/`, `AGENTS.md`, `CLAUDE.md`, `build.log`, or the real `NuGet.config`
- [ ] **REPO-02**: The repo's ignore rules prevent internal workflow files, local configs, stray logs, and similar local-only artifacts from being reintroduced after cleanup
- [ ] **REPO-03**: The repo exposes one checked-in hygiene verification path that proves internal workflow traces and unsafe local config files are not still tracked before the repo is opened publicly

### Public Docs Migration

- [ ] **DOCS-01**: Any roadmap, project-status, or launch guidance that should remain public after cleanup is migrated from `.planning/` into normal public docs under `docs/en` and `docs/zh-CN`
- [ ] **DOCS-02**: English and `zh-CN` public docs each expose a clear landing path for roadmap/status/alpha guidance without requiring readers to browse `.planning/`
- [ ] **DOCS-03**: The public docs set remains explicit about the supported package boundary, samples, proof tools, and current alpha posture after the internal-file cleanup

### Public Entry Surface

- [ ] **ENTRY-01**: `README.md` and `README.zh-CN.md` no longer advertise `.planning/` or AI-workflow files as part of the published repo entry story
- [ ] **ENTRY-02**: Public docs and launch guidance continue to distinguish `HostSample` as the minimal consumer proof and `AsterGraph.Demo` as the showcase host after the repo cleanup
- [ ] **ENTRY-03**: The cleaned public root still gives contributors and evaluators obvious entry points to source, tests, proof tools, workflows, and governance files without exposing internal execution traces

## Future Requirements

### Deferred Internal Workflow Handling

- **INT-01**: Move internal planning history, quick-task notes, and agent workflow state into a private or local-only workflow store outside the public repository
- **DOCS-04**: Publish richer public roadmap or status pages beyond the minimal alpha/project-status surface preserved in this milestone
- **SAFE-01**: Add stronger automated secret scanning or provenance checks beyond the repo-hygiene pass defined here

## Out of Scope

| Feature | Reason |
|---------|--------|
| New SDK runtime, plugin, automation, or demo capabilities | v1.10 is a public-repo cleanup milestone, not another product feature band |
| Reworking the canonical runtime boundary, supported package set, or proof-ring architecture | Those product and architecture decisions are already part of the shipped baseline |
| Removing public samples, proof tools, governance files, or public docs that external users actually need | The goal is to remove internal traces, not to make the repo less usable |
| Full history rewriting or force-pushed secret scrubbing unless a real sensitive leak is confirmed | This milestone should first close the tracked-file surface cleanly in normal history |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| REPO-01 | Phase 45 | Planned |
| REPO-02 | Phase 45 | Planned |
| REPO-03 | Phase 47 | Planned |
| DOCS-01 | Phase 46 | Planned |
| DOCS-02 | Phase 46 | Planned |
| DOCS-03 | Phase 47 | Planned |
| ENTRY-01 | Phase 47 | Planned |
| ENTRY-02 | Phase 47 | Planned |
| ENTRY-03 | Phase 47 | Planned |

**Coverage:**
- milestone requirements: 9 total
- mapped to phases: 9
- unmapped: 0

---
*Requirements defined: 2026-04-16*
*Last updated: 2026-04-16 after roadmap creation*
