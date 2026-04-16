---
phase: 43
title: Workflow And Cache Stabilization
status: planned
last_updated: 2026-04-16
---

# Phase 43 Research

## Current Failure Evidence

- The branch `ci.yml` already carries matrix, Linux validation, contract proof, artifact upload, and release validation, but the first post-v1.8 hosted runs still exposed an operational gap outside the codebase itself.
- GitHub-hosted `framework-matrix (net8.0)` failed during `Post Setup .NET SDK` with `Cache folder path is retrieved for .NET CLI but doesn't exist on disk`, which means the workflow-level cache path is not stable enough for a clean runner.
- `.github/workflows/release.yml` has produced failed runs with zero scheduled jobs. The workflow itself is present, so the likely issue is workflow evaluation rather than lane behavior after scheduling.
- The new public-open minimum also requires one explicit `.NET 10` consumer proof instead of assuming forward compatibility from `net8.0;net9.0` package targets.

## Source-Level Findings

- `ci.yml` and `release.yml` both use `actions/setup-dotnet` with `cache: true`, but neither workflow pins `NUGET_PACKAGES` to a deterministic workspace-local path nor pre-creates that directory before the post-job cache writeback.
- `eng/ci.ps1` already provides the canonical validation entry point. Phase 43 should extend that script instead of inventing a second verification path.
- `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj` still targets only `net8.0`, so there is no checked-in consumer proof that exercises package consumption from a `.NET 10` host.

## External Constraint

- GitHub's `setup-dotnet` guidance explicitly supports setting `NUGET_PACKAGES` to a workspace-local directory for dependency caching, which gives us a deterministic cache path on hosted runners.
- GitHub's secrets guidance explicitly states that secrets cannot be referenced directly in `if:` conditionals. The current `release.yml` job-level condition `secrets.NUGET_API_KEY != ''` therefore matches the observed zero-job workflow failure pattern and should be replaced with an in-job secret gate.

## Phase Direction

Phase 43 should stay narrow:

1. Stabilize workflow cache behavior by pinning `NUGET_PACKAGES` in both workflows and ensuring the directory exists before `setup-dotnet` cache teardown.
2. Make `release.yml` structurally valid by removing direct `secrets.*` usage from job-level `if:` expressions while keeping publish-to-NuGet gated behind the secret.
3. Add one explicit `.NET 10` packed-consumer proof through `HostSample`, wire it into `eng/ci.ps1`, and surface its marker in release artifacts.
