## Summary

Plan 28-01 aligned the tracked proof-tool entry points with the live tree. `AsterGraph.ScaleSmoke` is now part of the tracked solution surface, and the current planning/codebase reference docs no longer describe `HostSample` as a live project.

## Changes

- added `AsterGraph.ScaleSmoke` to [avalonia-node-map.sln](/F:/CodeProjects/DotnetCore/avalonia-node-map/avalonia-node-map.sln)
- kept `ScaleSmoke` under the existing `tools` solution folder with full solution configuration wiring
- removed stale `HostSample` claims from [.planning/codebase/STACK.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/codebase/STACK.md), [.planning/codebase/STRUCTURE.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/codebase/STRUCTURE.md), and [.planning/codebase/TESTING.md](/F:/CodeProjects/DotnetCore/avalonia-node-map/.planning/codebase/TESTING.md)
- refreshed current planning/codebase maps so they point at `PackageSmoke`, `ScaleSmoke`, the sample-only `AsterGraph.Demo`, and the current v1.5 roadmap state

## Verification

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net8.0 -Configuration Release`
