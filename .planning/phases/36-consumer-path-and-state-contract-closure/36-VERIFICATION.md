---
phase: 36-consumer-path-and-state-contract-closure
status: passed
verified: 2026-04-16
requirements:
  - CONS-01
  - CONS-02
  - HIST-01
  - HIST-02
---

# Phase 36 Verification

## Status

Verified on 2026-04-16 after Phase 36 implementation.

## Commands

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
rg -n "runtime-only or custom UI|Plugin trust/discovery|History, Save, and Dirty Contract|SCALE_HISTORY_CONTRACT_OK|consumer/state-contract gate" README.md docs .planning/codebase
```

## Results

- Packed `HostSample` still ran successfully and emitted:
  - `HOST_SAMPLE_PATHS:CreateSession:Create:AsterGraphAvaloniaViewFactory`
  - `HOST_SAMPLE_RUNTIME_OK:True:41:1:1`
  - `HOST_SAMPLE_HOSTED_UI_OK:True:CanvasOnly:False:False:1`
  - `HOST_SAMPLE_OK:True`
- The focused `contract` lane remained green:
  - `HostSample` project-reference proof passed
  - focused contract filter passed `156/156`
- Docs and testing maps now contain explicit references to:
  - runtime-only/custom UI route
  - plugin trust/discovery route
  - the `History, Save, and Dirty Contract`
  - `SCALE_HISTORY_CONTRACT_OK`
  - `consumer/state-contract gate`

## Proven Scope

- Consumers can now see packages, entry points, and verification commands together instead of inferring them across long-form docs.
- The history/save/dirty rules now exist as written product behavior and are tied back to the gate that proves them.
- The repo no longer depends on test names alone to explain state semantics.
