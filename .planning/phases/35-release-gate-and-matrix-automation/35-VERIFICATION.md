---
phase: 35-release-gate-and-matrix-automation
status: passed
verified: 2026-04-16
requirements:
  - REL-01
  - REL-02
  - REL-03
---

# Phase 35 Verification

## Status

Verified on 2026-04-16 after Phase 35 implementation.

## Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## Results

- `contract` lane passed:
  - `HostSample` project-reference route emitted:
    - `HOST_SAMPLE_PATHS:CreateSession:Create:AsterGraphAvaloniaViewFactory`
    - `HOST_SAMPLE_RUNTIME_OK:True:41:1:1`
    - `HOST_SAMPLE_HOSTED_UI_OK:True:CanvasOnly:False:False:1`
    - `HOST_SAMPLE_OK:True`
  - focused contract/proof filter passed `156/156`
- `release` lane passed end to end:
  - ran the same focused contract proof successfully before package packing
  - packed all four publishable packages with package validation enabled
  - restored and ran `HostSample` against packed packages successfully
  - restored and ran `PackageSmoke` against packed packages successfully
  - ran `ScaleSmoke` successfully with:
    - `SCALE_HISTORY_CONTRACT_OK:True:True:False:True:False:21`
    - `PHASE25_SCALE_AUTOMATION_OK:True:6:181:180:2`
    - `PHASE18_SCALE_READINESS_OK:True:True:41:1:1`
  - coverage report completed with `COVERAGE_REPORT_OK:6901:8787:78.54`

## Proven Scope

- One repo-local publish gate now covers contract proof, pack, packed consumer proof, smoke proof, coverage, and package validation.
- CI can now express framework-matrix, focused contract proof, and release validation as separate jobs instead of one blended gate.
- Packed `HostSample` is now a real release-proof surface rather than a README-only manual command.

## Residual Notes

- The release verification failure found during implementation was resolved by adding `AsterGraph.Core` to `Directory.Packages.props`.
