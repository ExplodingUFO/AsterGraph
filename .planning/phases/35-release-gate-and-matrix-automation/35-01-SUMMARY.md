---
phase: 35-release-gate-and-matrix-automation
plan: 01
subsystem: build
tags: [build, ci, proof-ring, release]
requires: []
provides:
  - focused contract lane
  - packed HostSample release proof
  - release gate that covers contract plus package smoke
affects: [eng-ci, package-validation, host-sample]
tech-stack:
  added: []
  patterns: [scripted-gates, narrow-maintenance-lane]
key-files:
  created: []
  modified:
    - eng/ci.ps1
    - Directory.Packages.props
key-decisions:
  - "Keep `maintenance` narrow and introduce a separate `contract` lane for consumer/runtime/plugin/history proof."
  - "Make `release` run the focused contract proof first, then rerun `HostSample` against packed packages before `PackageSmoke` and `ScaleSmoke`."
patterns-established:
  - "Packed-package consumer proof should run through a real sample, not just the smoke tool."
requirements-completed: [REL-01, REL-03]
duration: working session
completed: 2026-04-16
---

# Phase 35 Plan 01: Release Gate Summary

**Extended `eng/ci.ps1` with a dedicated `contract` lane, folded that focused proof into the `release` lane, and fixed the missing central package-version entry that blocked packed `HostSample` restore.**

## Accomplishments

- Added `contract` to the repo-local CI script without widening the existing `maintenance` lane.
- Added `HostSample` to the `net8.0` build surface and taught the script to run it in:
  - project-reference mode for focused contract proof
  - packed-package mode for release validation
- Promoted the release gate to one official entry point that now executes:
  - focused contract proof
  - package pack + package validation
  - packed `HostSample`
  - packed `PackageSmoke`
  - `ScaleSmoke`
  - coverage collection/reporting
- Added `AsterGraph.Core` to `Directory.Packages.props` so packed consumer restore works under central package management.

## Task Commits

1. `16099d9` - `build(35): automate contract and release proof lanes`

## Self-Check: PASSED

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`

---
*Phase: 35-release-gate-and-matrix-automation*
*Completed: 2026-04-16*
