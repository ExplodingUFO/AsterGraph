# Phase 28 Verification

## Status

Verified on 2026-04-14 after Phase 28 implementation.

## Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
```

## Results

- Restore succeeded against `avalonia-node-map.sln`.
- The `net8.0` lane built all four publishable packages plus `tools/AsterGraph.PackageSmoke` and `tools/AsterGraph.ScaleSmoke`, then passed `AsterGraph.Serialization.Tests` with 8/8 tests.
- The `net9.0` lane built `tests/AsterGraph.TestPlugins` and `src/AsterGraph.Demo`, then passed `AsterGraph.Editor.Tests` with 362/362 tests and `AsterGraph.Demo.Tests` with 28/28 tests.
- `eng/ci.ps1` completed successfully for `-Lane all -Framework all -Configuration Release`.

## Proven Scope

- `AsterGraph.ScaleSmoke` is now part of the tracked proof surface used by restore/build flows, and stale `HostSample` claims are removed from current planning/codebase maps.
- Core SDK regression coverage is now separable from demo/sample integration coverage through the checked-in `AsterGraph.Editor.Tests` / `AsterGraph.Serialization.Tests` versus `AsterGraph.Demo.Tests` lane split.
- Public docs now describe the same live proof surface as the tree: `PackageSmoke`, `ScaleSmoke`, sample-only `AsterGraph.Demo`, and the split regression lanes.
- The repo-local script entrypoint accurately covers package builds, smoke-tool project build wiring, and split-lane tests; explicit `dotnet run` of `PackageSmoke` and `ScaleSmoke` remains a separate proof step for Phase 29 release validation work.
