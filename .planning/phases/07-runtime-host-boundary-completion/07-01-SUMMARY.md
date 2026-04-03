---
phase: 07-runtime-host-boundary-completion
plan: 01
subsystem: runtime-host-boundary
tags: [runtime-session, contracts, compatibility, dto, pending-connection, tests]
requires:
  - phase: v1.0-foundation
    provides: package boundary, runtime session root, retained compatibility facade
provides:
  - additive core custom-host runtime command/query/event surface
  - DTO-based compatible-target query path
  - pending-connection event/query path on the session runtime boundary
  - focused contract validation through repo tests and temp harness
affects: [07-02, 07-03, host-runtime-story, migration-parity]
tech-stack:
  added: []
  patterns:
    - additive default interface expansion for runtime contracts
    - compatibility-only retention of MVVM query shapes
    - session event relay over editor-owned pending connection state
key-files:
  created:
    - src/AsterGraph.Editor/Runtime/GraphEditorCompatiblePortTargetSnapshot.cs
    - src/AsterGraph.Editor/Events/GraphEditorPendingConnectionChangedEventArgs.cs
  modified:
    - src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorCapabilitySnapshot.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
    - src/AsterGraph.Editor/Menus/CompatiblePortTarget.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs
key-decisions:
  - "Keep IGraphEditorSession as the only runtime root; do not introduce a second parallel host runtime API."
  - "Add core runtime interactions by ID/DTO and keep GraphEditorViewModel as a compatibility facade."
  - "Preserve BeginConnection compatibility while shifting the canonical runtime surface toward StartConnection."
  - "Treat CompatiblePortTarget as compatibility-only and move the canonical runtime query to a DTO."
patterns-established:
  - "Session contracts can grow additively via default interface members."
  - "Pending connection changes are relayed from the editor state owner through the session event surface."
requirements-completed: [HOST-01, HOST-02]
duration: 1 session
completed: 2026-04-03
---

# Phase 07 Plan 01 Summary

**Completed the first runtime host-boundary step by expanding the session contract for core custom-host interaction while preserving the compatibility facade.**

## Accomplishments

- Expanded the runtime session command/query/event surface to cover:
  - selection mutation
  - node position mutation
  - connection lifecycle
  - viewport sizing/fit/centering
  - pending-connection state
- Added `GraphEditorCompatiblePortTargetSnapshot` as the canonical runtime-safe compatible-target DTO.
- Added `GraphEditorPendingConnectionChangedEventArgs` and wired pending connection state through the session event surface.
- Preserved compatibility by:
  - keeping `IGraphEditorSession` as the single runtime root
  - retaining `CompatiblePortTarget` as compatibility-only
  - forwarding the old `BeginConnection(...)` path to the new canonical runtime command
- Extended focused proof coverage in:
  - `GraphEditorSessionTests`
  - `GraphEditorTransactionTests`
  - `GraphEditorInitializationTests`
- Recreated the plan-required `%TEMP%` contract harness at:
  - `%TEMP%\astergraph-phase7-contract-validation\AsterGraph.Phase7.Contracts.Validation.csproj`

## Verification

- `dotnet build src/AsterGraph.Editor/AsterGraph.Editor.csproj --no-restore -v minimal`
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionTests|FullyQualifiedName~GraphEditorTransactionTests|FullyQualifiedName~GraphEditorInitializationTests" -v minimal`
- `dotnet test %TEMP%\astergraph-phase7-contract-validation\AsterGraph.Phase7.Contracts.Validation.csproj -v minimal`

All passed at completion.

## Task Commits

1. `517f3d5` `feat: finish phase 07 runtime session surface`
2. `eb61fe0` `fix: complete phase 07 session contract alignment`
3. `683c0e5` `fix: preserve runtime session compatibility`
4. `c7b9049` `fix: tighten pending connection compatibility`

## Decisions Made

- The runtime/session host story now uses the session boundary as the canonical custom-host surface for core interaction.
- Pending connection state is treated as runtime-observable state, not just a viewmodel implementation detail.
- Compatibility-safe migration matters more than aggressively pruning old names in this phase.

## Deviations from Plan

- The focused `%TEMP%` harness was added manually after implementation to satisfy the exact proof path written into the plan.

## Issues Encountered

- Initial implementation drifted from the plan around command naming and pending-connection events; that was corrected before closeout.
- A compatibility review surfaced that the `%TEMP%` contract harness named in the plan did not yet exist; it was recreated and validated.

## Outcome

Phase 7 now has a stable first-wave runtime host boundary that custom hosts can use for core interaction without dropping to `GraphEditorViewModel` for the covered scenarios, and the proof ring for that contract is in place for the next plans.
