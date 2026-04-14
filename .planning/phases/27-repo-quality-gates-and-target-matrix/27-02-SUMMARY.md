## Summary

Plan 27-02 added the shared repo-local validation entry point for Phase 27. `eng/ci.ps1` now restores the solution, builds the four publishable packages for explicit `net8.0` and `net9.0` lanes, and runs the current focused validation surface for each lane from one reusable command path.

## Changes

- added `eng/ci.ps1` with `Lane`, `Framework`, and `Configuration` switches
- encoded the four publishable packages as the shared build surface for both target frameworks
- encoded the `net8.0` validation lane around `Serialization.Tests`, `PackageSmoke`, and `ScaleSmoke`
- encoded the `net9.0` validation lane around `Editor.Tests`, `TestPlugins`, and `AsterGraph.Demo`
- updated the Phase 27 plan artifact so the net9 host/build lane is reflected in the execution plan

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
