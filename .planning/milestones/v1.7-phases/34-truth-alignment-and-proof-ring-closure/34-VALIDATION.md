---
phase: 34-truth-alignment-and-proof-ring-closure
validated: 2026-04-16
status: ready
---

# Phase 34 Validation Contract

## Requirements

- `ALIGN-01`: maintainer can read README, ROADMAP, STATE, PROJECT, and current codebase maps without conflicting milestone, proof-ring, or capability claims
- `ALIGN-02`: consumer-facing docs no longer contradict themselves around capabilities, target support, or proof-tool availability
- `PROOF-01`: the official proof ring is explicitly defined around real, discoverable entry points for `PackageSmoke`, `ScaleSmoke`, focused regressions, and a minimal consumer host path

## Validation Strategy

### Narrative alignment proof

- README, quick start, host integration guide, ROADMAP, STATE, PROJECT, and current codebase maps should all describe v1.7 as the active milestone and point to the same proof entry points.

### Proof-ring discoverability proof

- `AsterGraph.HostSample` must exist as a real project again.
- the solution should list the sample together with the other tooling entry points.
- docs should refer only to entry points that can actually be found in the live tree.

### Minimal host-path proof

- the restored host sample must build and run successfully
- it should emit stable output proving the canonical consumer route works

## Required Commands

```powershell
dotnet build tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release --nologo -v minimal
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release --nologo
dotnet sln .\avalonia-node-map.sln list
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
```

## Exit Criteria

- host sample project exists and runs
- solution membership exposes the maintained proof tools and host sample
- current docs and codebase maps describe one proof story
- no README contradiction remains around undo/redo capability versus non-goal
- maintenance lane remains green after the doc and tooling-surface changes
