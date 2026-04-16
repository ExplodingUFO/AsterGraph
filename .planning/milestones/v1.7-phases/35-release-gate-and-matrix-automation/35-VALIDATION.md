---
phase: 35-release-gate-and-matrix-automation
validated: 2026-04-16
status: ready
---

# Phase 35 Validation Contract

## Requirements

- `REL-01`: one official verification entry point executes build, test, pack, smoke, and compatibility checks
- `REL-02`: CI covers the supported `net8.0` / `net9.0` matrix with explicit build, smoke, and focused contract lanes
- `REL-03`: publishable package compatibility remains machine-guarded during release verification

## Validation Strategy

### Repo-gate proof

- `eng/ci.ps1 -Lane release` should clearly run:
  - restore/build/test work
  - contract proof
  - pack with package validation
  - HostSample / PackageSmoke / ScaleSmoke
  - coverage validation

### CI-lane proof

- `.github/workflows/ci.yml` should show explicit jobs for:
  - framework-matrix validation
  - focused contract proof
  - release validation

### Compatibility proof

- package validation should remain enabled through the existing SDK-integrated pack flow
- release validation should still succeed after the lane changes

## Required Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## Exit Criteria

- `contract` lane exists and passes
- `release` lane remains the single official verification entry point
- release lane executes packed-package `HostSample`, `PackageSmoke`, and `ScaleSmoke`
- CI shows explicit matrix + contract + release jobs
- package validation remains enabled for packable projects
