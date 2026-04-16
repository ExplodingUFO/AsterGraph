---
phase: 34-truth-alignment-and-proof-ring-closure
plan: 03
subsystem: tooling
tags: [host-sample, proof-ring, solution]
requires: [34-01, 34-02]
provides:
  - restored minimal consumer host sample
  - solution-level discoverability for the sample
  - package/project-reference switch for consumer-path proof
affects: [tools, solution, docs]
tech-stack:
  added: [Avalonia.Headless]
  patterns: [minimal-consumer-host, optional-packed-package-restore]
key-files:
  created:
    - tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj
    - tools/AsterGraph.HostSample/Program.cs
  modified:
    - avalonia-node-map.sln
    - README.md
    - docs/quick-start.md
    - docs/host-integration.md
    - src/AsterGraph.Editor/README.md
key-decisions:
  - "Restore `AsterGraph.HostSample` as a narrow sample instead of resurrecting the older proof-heavy program."
  - "Keep project-reference and packed-package modes in the sample so the consumer path can prove both local and package-fed usage."
patterns-established:
  - "Use a small headless Avalonia sample for canonical hosted-UI proof while leaving broader behavior coverage to PackageSmoke and ScaleSmoke."
requirements-completed: [PROOF-01]
duration: working session
completed: 2026-04-16
---

# Phase 34 Plan 03: Host Sample Restoration Summary

**Restored `AsterGraph.HostSample` as a real minimal consumer host sample, added it back to the canonical solution, and made the docs point at it deliberately instead of through stale historical references.**

## Accomplishments

- Reintroduced `tools/AsterGraph.HostSample` as a runnable `net8.0` sample.
- Added `UsePackedAsterGraphPackages=true` switching so the sample can run from local packed packages as well as project references.
- Kept the sample narrow:
  - runtime-first route via `CreateSession(...)`
  - hosted-UI route via `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
  - no retained-path proof burden
- Added the sample back to `avalonia-node-map.sln` under `tools`.
- Updated root and package-level docs so the sample is now named as the minimal consumer-facing entry point.

## Verification Highlights

- `dotnet build tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release --nologo -v minimal`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -c Release --nologo`
- Output:
  - `HOST_SAMPLE_RUNTIME_OK:True:41:1:1`
  - `HOST_SAMPLE_HOSTED_UI_OK:True:CanvasOnly:False:False:1`
  - `HOST_SAMPLE_OK:True`

---
*Phase: 34-truth-alignment-and-proof-ring-closure*
*Completed: 2026-04-16*
