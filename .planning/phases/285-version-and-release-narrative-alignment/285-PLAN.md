# Phase 285: Version And Release Narrative Alignment - Plan

**Created:** 2026-04-26
**Status:** Ready for execution

## Goal

Remove public version ambiguity across README, versioning docs, release workflow output, and issue/release links while preserving local planning labels as private bookkeeping.

## Requirements

REL-01, REL-02, REL-03, REL-04, PROOF-02, PROOF-03

## Tasks

1. Add a focused public versioning validation script.
   - Verify: script passes on current repo and fails on a temp repo with mismatched README/version docs.

2. Wire the script into release validation.
   - Verify: `eng/ci.ps1` release lane references the script and `.github/workflows/release.yml` runs it before prerelease creation/publish.

3. Tighten bilingual versioning/release wording.
   - Verify: docs distinguish NuGet/package SemVer, GitHub prerelease tags/Releases, and local planning labels.

4. Extend focused tests for release/version contracts and guardrail wording.
   - Verify: targeted Demo.Tests filters pass.

## Verification

- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter FullyQualifiedName~ReleaseClosureContractTests`
- `dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter FullyQualifiedName~DemoProofReleaseSurfaceTests`

