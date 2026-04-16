---
phase: 41
title: Public Alpha Release Closure
status: in_progress
last_updated: 2026-04-16
---

# Phase 41 Validation

## Required Outcomes

1. CI adds concurrency, caching, Linux validation, and artifact uploads without weakening the existing Windows release lane.
2. Tag-triggered release automation exists and keeps pull requests verification-only.
3. Public proof outputs from the release lane are captured as files and included in release artifacts.

## Checks

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`

## Expected Signals

- contract lane remains green
- release lane remains green
- proof artifacts exist under `artifacts/`
- CI and release workflows show Linux validation plus tag-only prerelease behavior in tracked YAML
