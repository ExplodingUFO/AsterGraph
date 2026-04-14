# Phase 28.03 Summary: Sync Proof Surface With Live Regression Lanes

## What changed

- Updated `README.md` to describe the live proof surface as `AsterGraph.PackageSmoke` and `AsterGraph.ScaleSmoke`, and to explicitly separate regression coverage into:
  - core SDK regression lane: `AsterGraph.Editor.Tests` and `AsterGraph.Serialization.Tests`
  - demo/sample regression lane: `AsterGraph.Demo.Tests`
- Added `eng/ci.ps1` as the primary release/validation command in `README.md` and replaced the old undifferentiated `dotnet test` guidance.
- Updated `docs/quick-start.md` to reference `PackageSmoke`, `ScaleSmoke`, and explicit lane checks for the three test projects.
- Updated `docs/host-integration.md` to describe the same live proof surface and split regression lanes, and to point validation flow to `eng/ci.ps1`.

## Verification

- Ran `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`.

## Result

- Documentation is now aligned with the current proof tooling and regression split used by the repository automation.
