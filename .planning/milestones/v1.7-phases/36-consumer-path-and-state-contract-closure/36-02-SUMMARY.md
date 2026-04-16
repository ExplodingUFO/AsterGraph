---
phase: 36-consumer-path-and-state-contract-closure
plan: 02
subsystem: docs
tags: [docs, state-contract, history, dirty]
requires: []
provides:
  - explicit history/save/dirty contract
  - proof mapping from docs to tests and smoke markers
affects: [state-contract-doc, host-docs, readme]
tech-stack:
  added: []
  patterns: [published-behavior-contract]
key-files:
  created:
    - docs/state-contracts.md
  modified:
    - README.md
    - docs/quick-start.md
    - docs/host-integration.md
key-decisions:
  - "Publish the history/save/dirty rules as product behavior instead of leaving them implicit in test names."
patterns-established:
  - "State-contract docs should point directly to the enforcing proof lane and smoke marker."
requirements-completed: [HIST-01, HIST-02]
duration: working session
completed: 2026-04-16
---

# Phase 36 Plan 02: State Contract Summary

**Published the history/save/dirty rules as an explicit host-facing contract and linked them back to the tests and smoke markers that enforce them.**

## Accomplishments

- Added `docs/state-contracts.md` to describe the actual rules for:
  - save and clean baseline
  - undo after save
  - redo back to the saved state
  - no-op interactions
  - one kernel-owned history/save authority across retained + runtime flows
- Linked the new contract from README, Quick Start, and the host-integration guide.
- Pointed the contract back to:
  - `eng/ci.ps1 -Lane contract`
  - focused history/save suites
  - `SCALE_HISTORY_CONTRACT_OK`

## Task Commits

1. `c987a95` - `docs(36): publish consumer route and state contracts`

## Self-Check: PASSED

- `rg -n "History, Save, and Dirty Contract|SCALE_HISTORY_CONTRACT_OK|state-contracts" README.md docs`

---
*Phase: 36-consumer-path-and-state-contract-closure*
*Completed: 2026-04-16*
