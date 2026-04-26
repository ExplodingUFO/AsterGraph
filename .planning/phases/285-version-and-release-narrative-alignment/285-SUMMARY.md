# Phase 285: Version And Release Narrative Alignment - Summary

**Completed:** 2026-04-26
**Status:** Complete

## What Changed

- Added `eng/validate-public-versioning.ps1` to validate the public package version/tag vocabulary across README, bilingual versioning docs, issue template links, and public launch checklists.
- Wired the validation into `.github/workflows/release.yml` before release validation and into `eng/ci.ps1` release lane.
- Updated README and bilingual versioning docs to state that GitHub prerelease/Release records use NuGet package SemVer, while local planning milestones are private bookkeeping.
- Added release contract tests proving the public versioning gate runs, passes the real repo, and fails a mismatched README/versioning repo.

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-public-versioning.ps1 -RepoRoot . -PublicTag v0.11.0-beta`
- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter FullyQualifiedName~ReleaseClosureContractTests`
- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter FullyQualifiedName~DemoProofReleaseSurfaceTests`

## Requirements Covered

- REL-01
- REL-02
- REL-03
- REL-04
- PROOF-02
- PROOF-03

