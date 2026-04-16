---
phase: 37-maintainability-and-extension-contract-hardening
plan: 03
subsystem: testing-docs
tags: [docs, testing, lanes, maintainability]
requires: []
provides:
  - explicit lane-ownership contract
  - clearer maintenance-surface localization
affects: [testing-map, readme, host-docs, editor-readme]
tech-stack:
  added: []
  patterns: [lane-ownership]
key-files:
  created:
    - docs/extension-contracts.md
  modified:
    - .planning/codebase/TESTING.md
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Editor/README.md
key-decisions:
  - "Treat lane ownership as part of the maintainability contract, not just CI implementation detail."
patterns-established:
  - "Classify failures by lane before changing code."
requirements-completed: [MAINT-02, EXT-02]
duration: working session
completed: 2026-04-16
---

# Phase 37 Plan 03: Lane Contract Summary

**Published lane ownership so maintainers can map failures to the right proof surface before they start changing code.**

## Accomplishments

- Documented lane ownership for:
  - `all`
  - `contract`
  - `maintenance`
  - `release`
  - demo/sample tests
- Refreshed the testing map so `maintenance` is clearly the compatibility-hotspot gate rather than a generic smoke run.
- Linked lane ownership back to the new extension/maintenance contract doc from host-facing and package-facing docs.

## Task Commits

1. `4965bee` - `docs(37): publish extension and lane contracts`

## Self-Check: PASSED

- `rg -n "lane ownership|hotspot-refactor|consumer/state-contract gate" README.md docs src/AsterGraph.Editor/README.md .planning/codebase/TESTING.md`

---
*Phase: 37-maintainability-and-extension-contract-hardening*
*Completed: 2026-04-16*
