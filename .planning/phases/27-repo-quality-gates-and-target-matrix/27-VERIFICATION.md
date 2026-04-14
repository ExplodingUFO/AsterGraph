# Phase 27 Verification

## Status

Verified on 2026-04-14 after Phase 27 implementation.

## Commands

```powershell
dotnet build avalonia-node-map.sln --nologo -v minimal
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
```

## Results

- `dotnet build` succeeded for the solution after introducing `.editorconfig`, `Directory.Packages.props`, and repo-local `NuGet.config`, with `0 warnings` and `0 errors`.
- `eng/ci.ps1` completed successfully for `net8.0` and `net9.0` in `Release`, building all four publishable packages in both lanes.
- The `net8.0` validation lane built `PackageSmoke` and `ScaleSmoke` and passed `AsterGraph.Serialization.Tests` with 8/8 tests.
- The `net9.0` validation lane built `AsterGraph.Demo` and `AsterGraph.TestPlugins` and passed `AsterGraph.Editor.Tests` with 390/390 tests.
- `.github/workflows/ci.yml` now checks in a Windows GitHub Actions workflow that reuses `eng/ci.ps1` for `push`, `pull_request`, and `workflow_dispatch` runs across explicit `net8.0` and `net9.0` matrix entries.

## Proven Scope

- Contributors now build and test the repo under one tracked baseline that includes `.editorconfig`, `Directory.Packages.props`, and a deterministic repo-local `NuGet.config`.
- The supported package boundary is now validated automatically through one shared command path rather than manual README-only command memory.
- Local execution and checked-in CI now point at the same matrix entrypoint, so later phases can extend the same lane for proof-surface cleanup and release-grade validation.
