---
phase: 36-consumer-path-and-state-contract-closure
plan: 01
subsystem: docs
tags: [docs, onboarding, consumer, host-integration]
requires: []
provides:
  - compact consumer route matrix
  - package-to-entrypoint guidance
  - route-specific verification commands
affects: [readme, quick-start, host-docs]
tech-stack:
  added: []
  patterns: [route-matrix, proof-command-per-route]
key-files:
  created: []
  modified:
    - README.md
    - docs/quick-start.md
    - docs/host-integration.md
key-decisions:
  - "Use one compact route matrix instead of expanding narrative duplication across multiple docs."
patterns-established:
  - "Each consumer route should name packages, canonical entry point, and verification command together."
requirements-completed: [CONS-01, CONS-02]
duration: working session
completed: 2026-04-16
---

# Phase 36 Plan 01: Consumer Route Matrix Summary

**Added a compact route matrix that tells consumers which packages to reference, which public API to start from, and which proof command verifies each supported adoption path.**

## Accomplishments

- Expanded Quick Start from a simple path list into a route matrix covering:
  - runtime-only/custom UI
  - default Avalonia UI
  - plugin trust/discovery
  - automation
  - retained migration
- Added the same route-matrix shape to the host-integration guide so the short and long docs tell the same story.
- Linked README back to the route matrix instead of making readers reconstruct the package/verification story from multiple sections.

## Task Commits

1. `c987a95` - `docs(36): publish consumer route and state contracts`

## Self-Check: PASSED

- `rg -n "Packages to start with|Verify with|Plugin trust/discovery|Automation" README.md docs`

---
*Phase: 36-consumer-path-and-state-contract-closure*
*Completed: 2026-04-16*
