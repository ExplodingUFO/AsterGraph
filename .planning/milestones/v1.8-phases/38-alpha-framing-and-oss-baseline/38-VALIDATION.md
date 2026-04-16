---
phase: 38
title: Alpha Framing And OSS Baseline
status: completed
last_updated: 2026-04-16
---

# Phase 38 Validation

## Required Outcomes

1. Public package metadata, README, planning artifacts, and top-level docs describe the same public-alpha version story.
2. External readers can find a single alpha-status and known-limitations contract.
3. The repo contains explicit contributor, conduct, security, issue, PR, and SDK-baseline files.

## Checks

- `dotnet --version`
- `dotnet msbuild src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -getProperty:Version`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`

## Expected Signals

- SDK resolves successfully under `global.json`
- project version resolves to `0.2.0-alpha.1`
- CI lane completes with exit code `0`
