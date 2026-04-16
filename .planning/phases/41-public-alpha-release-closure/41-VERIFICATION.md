---
phase: 41
title: Public Alpha Release Closure
status: completed
last_updated: 2026-04-16
---

# Phase 41 Verification

## Commands

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`

## Results

- contract lane passed: `156/156`
- release lane passed end to end
- proof files now exist under `artifacts/proof`:
  - `hostsample-project.txt`
  - `hostsample-packed.txt`
  - `package-smoke.txt`
  - `scale-smoke.txt`
  - `coverage-report.txt`
- coverage summary remained `COVERAGE_REPORT_OK:6901:8787:78.54`
- tracked workflows now declare concurrency, caching, Linux validation, artifact upload, and a tag-driven prerelease flow
