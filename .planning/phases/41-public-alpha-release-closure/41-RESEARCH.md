---
phase: 41
title: Public Alpha Release Closure
status: in_progress
last_updated: 2026-04-16
---

# Phase 41 Research

## Goal

Extend CI and release automation so public-alpha validation, artifacts, and tag-driven prerelease publication are explicit and machine-checkable.

## Findings

1. `.github/workflows/ci.yml` still only ran on Windows and only executed lanes; it had no concurrency control, no package cache, and no artifact uploads.
2. The repo already had a strong script entrypoint in `eng/ci.ps1`, so the safest path was to keep workflow logic thin and let the script remain the source of truth for validation behavior.
3. `eng/ci.ps1 -Lane release` already produced the right runtime checks, but the smoke outputs only lived in console logs and were not captured as reusable artifacts.
4. The alpha milestone requires tag-driven prerelease behavior, not publication on every push.

## Execution Direction

- keep `eng/ci.ps1` as the authoritative lane runner
- capture smoke/proof output into files under `artifacts/`
- add workflow concurrency, NuGet caching, Linux validation, and artifact upload to `ci.yml`
- add a tag-only release workflow that validates, uploads proof artifacts, creates a prerelease, and conditionally pushes NuGet packages when the secret is present
