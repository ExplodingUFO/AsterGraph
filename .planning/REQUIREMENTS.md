# Requirements: AsterGraph v1.8

**Defined:** 2026-04-16
**Milestone:** v1.8 Public Alpha Readiness and Canonical Demo
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Public Alpha Framing

- [x] **FRAM-01**: Public package metadata, README, planning artifacts, and top-level consumer docs all describe the same public-alpha version story, anchored on a stable prerelease semantic such as `0.2.0-alpha.1`
- [x] **FRAM-02**: External readers can find one explicit alpha-status and known-limitations contract without having to infer stability from milestone notes or test names
- [x] **FRAM-03**: The repo's public entry path prioritizes `README` plus `docs/` for consumers, while `.planning` remains available as secondary maintainer context instead of the first discovery surface

### Open Source Governance And CI

- [x] **OSS-01**: External contributors have explicit collaboration and reporting guidance through `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, issue templates, and a PR template
- [x] **OSS-02**: Local development and CI agree on a pinned SDK baseline through `global.json` instead of relying on floating `10.0.x` resolution alone
- [x] **OSS-03**: CI exposes public-friendly behavior through concurrency control, restore caching, uploaded verification artifacts, and at least one Linux validation lane alongside the full Windows release lane
- [x] **REL-01**: Public prerelease publishing is tag-driven, so pull requests validate the alpha surface without running publish logic
- [x] **REL-02**: Public alpha releases attach smoke markers, coverage summary, and release-proof artifacts that let external evaluators verify what shipped without rerunning every lane locally first

### Canonical Demo And Consumer Separation

- [x] **DEMO-01**: `AsterGraph.Demo` uses the canonical factory/session/view-factory composition path instead of constructing `GraphEditorViewModel` directly as its main host route
- [x] **DEMO-02**: The demo visibly showcases plugin trust, candidate discovery, trust decisions, loaded plugins, and contribution shape instead of leaving those capabilities only in docs and focused tests
- [x] **DEMO-03**: The demo visibly showcases automation execution through canned runs, step/progress output, and linked diagnostics/events instead of exposing automation only as API surface
- [x] **DEMO-04**: The demo includes real standalone-surface and presenter-replacement routes that can be operated directly, rather than capability text that only describes those seams
- [x] **CONS-01**: `HostSample` is clearly positioned as the minimal consumer sample while `AsterGraph.Demo` is clearly positioned as the showcase host, in both docs and the demo itself

### Bilingual Experience And Demo Proof

- [x] **DOCS-01**: Core public guides exist in both English and `zh-CN`, with a stable structure for `README`, quick start, host integration, state contracts, extension contracts, demo guide, and alpha status
- [x] **L10N-01**: The demo can switch between Chinese and English, and that toggle proves the host localization seam instead of just hardcoding mixed-language shell copy
- [x] **TEST-01**: `AsterGraph.Demo.Tests` proves the canonical demo route, plugin/automation panes, standalone/presenter routes, and bilingual toggle so the showcase surface does not regress silently

## Future Requirements

### Deferred Platform And Distribution Work

- **TRUST-01**: Host can enforce stronger plugin signing, isolation, or remote-distribution policy beyond the current in-process trust baseline
- **MARKET-01**: Host can browse, install, and update plugins through a first-class marketplace/feed experience
- **SCRIPT-01**: Host can author automation flows through richer scripting or workflow-designer surfaces beyond canned execution and command/query composition
- **DOCS-02**: Public docs expand beyond the core bilingual guides into broader tutorials, cookbook examples, and API-reference publishing

## Out of Scope

| Feature | Reason |
|---------|--------|
| New graph-editing end-user features unrelated to public alpha readiness, demo completeness, or public release workflows | v1.8 is a public-alpha productization milestone, not a general feature-expansion band |
| Replacing Avalonia, reopening kernel/session ownership, or redesigning the canonical runtime boundary | The runtime-first architecture is already the shipped baseline and should remain stable through alpha preparation |
| One-shot removal of retained compatibility APIs | Migration remains staged; this milestone should clarify and demonstrate the boundary, not force an abrupt break |
| Marketplace UX, remote plugin installation UI, or stronger plugin isolation products | Those belong after the public alpha baseline is in front of external users |
| Rich automation authoring UI or embedded scripting IDE work | The goal is to make the shipped automation baseline visible and testable, not to broaden it into a new product tier |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| FRAM-01 | Phase 38 | Completed |
| FRAM-02 | Phase 38 | Completed |
| FRAM-03 | Phase 38 | Completed |
| OSS-01 | Phase 38 | Completed |
| OSS-02 | Phase 38 | Completed |
| DEMO-01 | Phase 39 | Completed |
| DEMO-02 | Phase 39 | Completed |
| DEMO-03 | Phase 39 | Completed |
| DEMO-04 | Phase 39 | Completed |
| CONS-01 | Phase 39 | Completed |
| DOCS-01 | Phase 40 | Completed |
| L10N-01 | Phase 40 | Completed |
| TEST-01 | Phase 40 | Completed |
| OSS-03 | Phase 41 | Completed |
| REL-01 | Phase 41 | Completed |
| REL-02 | Phase 41 | Completed |

**Coverage:**
- milestone requirements: 16 total
- mapped to phases: 16
- unmapped: 0

---
*Requirements defined: 2026-04-16*
*Last updated: 2026-04-16 after Phase 41 completion*
