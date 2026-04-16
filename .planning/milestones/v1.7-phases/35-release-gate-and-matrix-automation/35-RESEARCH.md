---
phase: 35-release-gate-and-matrix-automation
researched: 2026-04-16
status: complete
---

# Phase 35 Research

## Focus

Phase 35 needs to turn the current release validation baseline into a clearer machine gate:

1. make one official verification entry point obviously cover build, test, pack, smoke, and compatibility checks
2. make CI express the `net8.0` / `net9.0` matrix, focused contract proof, and full release proof as separate responsibilities
3. keep compatibility validation machine-enforced inside the release path

## Evidence Collected

### 1. The repo already has a solid scripted baseline

- `eng/ci.ps1` already supports `all`, `maintenance`, and `release`.
- `release` already restores, packs publishable packages, runs `PackageSmoke`, runs `ScaleSmoke`, collects coverage, and relies on SDK-integrated package validation during `dotnet pack`.
- `.github/workflows/ci.yml` already runs `eng/ci.ps1 -Lane all` across `net8.0` and `net9.0`, then runs `eng/ci.ps1 -Lane release`.

### 2. The remaining gap is lane clarity, not missing automation from scratch

- `all` is a framework-matrix build/test lane, but the workflow calls it `quality-gates`, which hides the matrix role.
- There is no explicit focused contract/proof job in CI even though the repo already has the right suites:
  - `GraphEditorSessionTests`
  - `GraphEditorProofRingTests`
  - `GraphEditorDiagnosticsContractsTests`
  - `GraphEditorAutomationContractsTests`
  - `GraphEditorPluginContractsTests`
  - `GraphEditorPluginDiscoveryTests`
  - `GraphEditorPluginInspectionContractsTests`
  - focused history/save suites
- `AsterGraph.HostSample` now exists, but the current scripted lanes do not build or run it.

### 3. Compatibility validation is already present, but it should be surfaced more explicitly

- `Directory.Build.props` already sets `<EnablePackageValidation>true</EnablePackageValidation>` for packable projects.
- That means the repo already gets SDK-integrated compatibility/package validation during `dotnet pack`.
- The missing piece is making the release gate and CI lanes describe and prove that behavior clearly rather than leaving it implicit.

## Conclusions

### Conclusion A

Phase 35 should add a dedicated `contract` lane to `eng/ci.ps1` instead of overloading `maintenance`. `maintenance` is a hotspot refactor lane; it should stay narrow.

### Conclusion B

The official `release` lane should run the minimal consumer host sample in packed-package mode so the release gate covers both:

- the narrow canonical consumer sample
- the broader package/runtime smoke tools

### Conclusion C

CI should expose three distinct responsibilities:

1. framework-matrix validation (`all`)
2. focused contract proof (`contract`)
3. full release proof (`release`)

## Phase 35 Shape

Recommended three-plan execution:

1. extend `eng/ci.ps1` with a focused contract lane and explicit host-sample execution
2. update GitHub Actions to show the matrix, contract, and release jobs explicitly
3. refresh consumer/testing docs so the official verification path and lane roles match the new automation
