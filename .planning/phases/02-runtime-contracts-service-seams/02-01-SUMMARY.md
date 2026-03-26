---
phase: 02-runtime-contracts-service-seams
plan: 01
subsystem: runtime-api
tags: [runtime-session, contracts, snapshots, events, batching, tests]
requires:
  - phase: 01-consumption-compatibility-guardrails
    provides: canonical factory entry points and retained compatibility facade
provides:
  - framework-neutral runtime session contract family
  - snapshot contracts for selection, viewport, and capabilities
  - typed command-executed and recoverable-failure event payloads
  - Wave 0 session and transaction regression scaffolding
affects: [02-02, 02-04, 02-05, host-runtime-story]
tech-stack:
  added: []
  patterns:
    - commands/queries/events split under IGraphEditorSession
    - Wave 0 regression files before concrete runtime implementation
key-files:
  created:
    - src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorEvents.cs
    - src/AsterGraph.Editor/Runtime/IGraphEditorMutationScope.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorSelectionSnapshot.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorViewportSnapshot.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorCapabilitySnapshot.cs
    - src/AsterGraph.Editor/Events/GraphEditorCommandExecutedEventArgs.cs
    - src/AsterGraph.Editor/Events/GraphEditorRecoverableFailureEventArgs.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs
  modified: []
key-decisions:
  - "先冻结 runtime 合同，再在后续计划里接入真实实现。"
  - "事件负载复用现有 typed EventArgs，不再引入第二套并行概念。"
patterns-established:
  - "Runtime API lives in AsterGraph.Editor and stays free of Avalonia types."
  - "Session contracts are named and test-scaffolded before implementation work begins."
requirements-completed: [API-01, API-02, API-03, API-04]
duration: 1h+
completed: 2026-03-26
---

# Phase 02 Plan 01 Summary

**建立了 `IGraphEditorSession` 为根的 runtime 合同家族，并把 session/batching 的 Wave 0 回归文件放进测试树。**

## Accomplishments

- 引入 `IGraphEditorSession`、`IGraphEditorCommands`、`IGraphEditorQueries`、`IGraphEditorEvents`、`IGraphEditorMutationScope`。
- 引入选择、视口、能力三个 snapshot 契约以及命令执行/可恢复失败事件负载。
- 新增 `GraphEditorSessionTests.cs` 和 `GraphEditorTransactionTests.cs`，把后续实现的公开表面先锁定下来。

## Task Commits

1. `26e5f38` `test(02-01): add runtime session contract scaffolding`
2. `279bbab` `test(02-01): add runtime batching contract scaffolding`

## Decisions Made

- 先定义 framework-neutral public surface，再在 `02-02` 接入真实行为。
- batching 合同保持轻量，不在这一步引入额外 bus/mediator 架构。

## Deviations from Plan

None.

## Issues Encountered

None recorded beyond normal contract scaffolding work.

## Outcome

Phase 2 后续计划有了稳定的 runtime API 骨架和明确的回归锚点。
