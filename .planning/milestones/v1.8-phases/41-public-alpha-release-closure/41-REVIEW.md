---
phase: 41
title: Public Alpha Release Closure
status: completed
last_updated: 2026-04-16
---

# Phase 41 Review

## What Landed

- `eng/ci.ps1` now writes release-proof outputs for `HostSample`, `PackageSmoke`, `ScaleSmoke`, and coverage into `artifacts/proof`
- the main CI workflow now has concurrency control, NuGet caching, Linux validation, release-artifact upload, and a Markdown release summary
- the repo now has a tag-driven `release.yml` workflow for prerelease validation, GitHub prerelease creation, and conditional NuGet publication

## Outcome

The repo now has a public-alpha automation story that matches the product surface documented in v1.8: validation stays script-first, pull requests remain verification-only, and tag-driven prerelease workflows have explicit proof artifacts to publish.
