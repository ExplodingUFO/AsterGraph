---
phase: 36-consumer-path-and-state-contract-closure
validated: 2026-04-16
status: planned
requirements:
  - CONS-01
  - CONS-02
  - HIST-01
  - HIST-02
---

# Phase 36 Validation

## Goals

- prove that the minimal consumer route is documented as a real canonical path instead of an implied sample
- prove that runtime-only, default UI, trust/discovery, and automation routes are documented with required packages and verification commands
- prove that history/save/dirty behavior is published as an explicit contract and tied back to the official proof lane

## Planned Validation Commands

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
rg -n "runtime-only|default Avalonia UI|trust/discovery|automation|history/save/dirty|contract lane|HostSample" README.md docs .planning/codebase
```

## Expected Evidence

- docs point at one compact route matrix and the existing `HostSample` proof path
- `HostSample` still runs successfully in packed-package mode
- the official `contract` lane still passes and is explicitly documented as the state-contract proof gate
- history/save/dirty rules are described as product behavior, not just inferred from test names
