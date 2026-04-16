---
phase: 37-maintainability-and-extension-contract-hardening
plan: 01
subsystem: docs
tags: [docs, stability, compatibility, retirement]
requires: []
provides:
  - explicit stability tiers
  - published compatibility-retirement guidance
  - clearer retained-facade positioning
affects: [readme, host-docs, editor-readme, extension-contracts]
tech-stack:
  added: []
  patterns: [stability-tiers, staged-retirement]
key-files:
  created:
    - docs/extension-contracts.md
  modified:
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Editor/README.md
key-decisions:
  - "Keep the phase documentation-backed: publish the maintainability contract instead of reopening runtime-boundary code churn."
patterns-established:
  - "Document canonical runtime surfaces, retained facades, and compatibility-only shims as separate stability tiers."
requirements-completed: [MAINT-01, EXT-01]
duration: working session
completed: 2026-04-16
---

# Phase 37 Plan 01: Stability And Retirement Summary

**Published explicit stability tiers and compatibility-retirement guidance so maintainers and consumers can see which surfaces are canonical, which remain retained bridges, and which are on a staged removal path.**

## Accomplishments

- Added `docs/extension-contracts.md` as the dedicated contract for:
  - stable canonical runtime/session surfaces
  - retained compatibility facades
  - compatibility-only shims
- Linked README, host docs, and the editor package README back to that contract.
- Published the staged retirement guidance for:
  - `GetCompatibleTargets(...)`
  - `CompatiblePortTarget`
  - older MVVM-rooted extension shims

## Task Commits

1. `4965bee` - `docs(37): publish extension and lane contracts`

## Self-Check: PASSED

- `rg -n "stability tiers|compatibility-only shims|future major release" README.md docs src/AsterGraph.Editor/README.md`

---
*Phase: 37-maintainability-and-extension-contract-hardening*
*Completed: 2026-04-16*
