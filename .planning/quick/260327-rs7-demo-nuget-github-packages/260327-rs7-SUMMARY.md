---
phase: quick-260327-rs7-demo-nuget-github-packages
plan: 1
subsystem: packaging-docs
tags: [nuget, github-packages, docs, publishing]
requires:
  - phase: 06-demo
    provides: Current package boundaries and host integration guidance baseline
provides:
  - Clear single-main-entry package story (`AsterGraph.Avalonia`) and protocol entry (`AsterGraph.Abstractions`)
  - GitHub Packages private feed setup and publish runbook for the four non-demo SDK libraries
  - Confirmed real publish of preview.7 non-demo packages to GitHub Packages
affects: [README, host-integration-docs, package-consumption, release-ops]
tech-stack:
  added: []
  patterns:
    - Repo docs are the canonical package boundary and feed setup reference
    - Demo package remains excluded from publish and consumer dependency paths
key-files:
  created:
    - .planning/quick/260327-rs7-demo-nuget-github-packages/260327-rs7-SUMMARY.md
  modified:
    - README.md
    - docs/host-integration.md
    - NuGet.config.sample
key-decisions:
  - "Default host UI entry package is AsterGraph.Avalonia; protocol/contract entry remains AsterGraph.Abstractions."
  - "Publish scope is locked to AsterGraph.Abstractions, AsterGraph.Core, AsterGraph.Editor, and AsterGraph.Avalonia (Demo excluded)."
  - "GitHub Packages setup remains credential-free in tracked config; secrets stay local/CI."
requirements-completed: []
duration: 1 session
completed: 2026-03-27
---

# Phase quick-260327-rs7 Plan 1: GitHub Packages publish + package guidance summary

**This quick pass finalized package boundary guidance, documented private-feed consumption/publish workflow, and successfully pushed the four non-demo libraries to GitHub Packages.**

## Performance

- **Date:** 2026-03-27
- **Tasks completed:** 3/3
- **Task commits:** 2 (docs/config tasks)
- **Publish result:** successful push for 4 `.nupkg` packages

## Accomplishments

- Clarified the package boundary and entry strategy in repository docs:
  - publish scope explicitly limited to `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`
  - main UI entry package explicitly documented as `AsterGraph.Avalonia`
  - protocol/contract entry package explicitly documented as `AsterGraph.Abstractions`
  - direct `Core`/`Editor` references described as conditional, API-driven choices
- Added actionable GitHub Packages setup and publish instructions:
  - source URL format and `dotnet nuget add source` usage
  - token scope expectations (`read:packages` / `write:packages`)
  - concrete pack + push sequence for the four supported non-demo packages
- Updated `NuGet.config.sample` with an optional `github-astergraph` source template without any secrets.
- Packed and pushed all four non-demo packages to GitHub Packages source `github-private`.

## Task Commits

1. **Task 1: Clarify package boundary and entry strategy in repo docs** — `e0d82c6` (`chore`)
2. **Task 2: Add GitHub Packages consumption and publish instructions** — `b7ba5f8` (`chore`)
3. **Task 3: Pack and publish four non-demo libraries** — No repo file changes required (operational publish step completed successfully)

## Files Created/Modified

- `F:/CodeProjects/DotnetCore/avalonia-node-map/README.md`
- `F:/CodeProjects/DotnetCore/avalonia-node-map/docs/host-integration.md`
- `F:/CodeProjects/DotnetCore/avalonia-node-map/NuGet.config.sample`
- `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-rs7-demo-nuget-github-packages/260327-rs7-SUMMARY.md`

## Verification

Executed:

- `dotnet pack "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj" -c Release -o "F:/CodeProjects/DotnetCore/avalonia-node-map/artifacts/packages"`
- `dotnet pack "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Core/AsterGraph.Core.csproj" -c Release -o "F:/CodeProjects/DotnetCore/avalonia-node-map/artifacts/packages"`
- `dotnet pack "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Editor/AsterGraph.Editor.csproj" -c Release -o "F:/CodeProjects/DotnetCore/avalonia-node-map/artifacts/packages"`
- `dotnet pack "F:/CodeProjects/DotnetCore/avalonia-node-map/src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj" -c Release -o "F:/CodeProjects/DotnetCore/avalonia-node-map/artifacts/packages"`
- `dotnet nuget push "...AsterGraph.Abstractions.0.1.0-preview.7.nupkg" --source github-private --skip-duplicate`
- `dotnet nuget push "...AsterGraph.Core.0.1.0-preview.7.nupkg" --source github-private --skip-duplicate`
- `dotnet nuget push "...AsterGraph.Editor.0.1.0-preview.7.nupkg" --source github-private --skip-duplicate`
- `dotnet nuget push "...AsterGraph.Avalonia.0.1.0-preview.7.nupkg" --source github-private --skip-duplicate`

Result:

- All four `.nupkg` packages pushed successfully.
- `dotnet nuget push` emitted a warning recommending `--api-key` for GitHub Packages; publish still succeeded against configured authenticated source.

Published package set:

- `AsterGraph.Abstractions` `0.1.0-preview.7`
- `AsterGraph.Core` `0.1.0-preview.7`
- `AsterGraph.Editor` `0.1.0-preview.7`
- `AsterGraph.Avalonia` `0.1.0-preview.7`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Initial publish source alias mismatch**
- **Found during:** Task 3
- **Issue:** Plan examples used `github-astergraph`, but environment had authenticated source named `github-private`.
- **Fix:** Switched push command source to existing configured `github-private` after verifying source list.
- **Files modified:** none
- **Commit:** n/a (operational command adjustment)

## Issues Encountered

- A prior build validation command for Task 1 failed due to unrelated pre-existing test compile errors in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs` (outside this quick scope).
- Task 2 verification (pack) and Task 3 publish still completed successfully.

## User Setup Required

None for this run. Publish used existing authenticated source:

- source name: `github-private`
- source URL: `https://nuget.pkg.github.com/ExplodingUFO/index.json`

## Known Stubs

None.

## Self-Check: PASSED
- FOUND: `F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/quick/260327-rs7-demo-nuget-github-packages/260327-rs7-SUMMARY.md`
- FOUND: commit `e0d82c6`
- FOUND: commit `b7ba5f8`
- VERIFIED: published four non-demo packages to `github-private`
