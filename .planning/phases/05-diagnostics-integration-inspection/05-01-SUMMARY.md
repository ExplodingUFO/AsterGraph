---
phase: 05-diagnostics-integration-inspection
plan: 01
subsystem: editor
tags: [diagnostics, contracts, inspection, runtime, instrumentation]
requires:
  - phase: 02-runtime-contracts-service-seams
    provides: diagnostics sink baseline, recoverable-failure publication, session-root runtime API
  - phase: 04-replaceable-presentation-kit
    provides: retained host migration path that Phase 5 must layer on top of
provides:
  - session-root diagnostics contract for `IGraphEditorSession`
  - immutable inspection snapshot records composed from existing query snapshots
  - optional instrumentation options contract using `ILoggerFactory` and `ActivitySource`
  - focused temp harness for Phase 5 contract validation
affects: [05-02, 05-03, 05-04, host-samples, smoke-tests]
tech-stack:
  added:
    - Microsoft.Extensions.Logging.Abstractions 9.0.0
    - src/AsterGraph.Editor/Diagnostics/*
  patterns:
    - diagnostics and inspection stay in AsterGraph.Editor
    - inspection contracts reuse existing runtime snapshots instead of exposing live editor internals
key-files:
  created:
    - src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnostics.cs
    - src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs
    - src/AsterGraph.Editor/Diagnostics/GraphEditorPendingConnectionSnapshot.cs
    - src/AsterGraph.Editor/Diagnostics/GraphEditorStatusSnapshot.cs
    - src/AsterGraph.Editor/Diagnostics/GraphEditorInstrumentationOptions.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsContractsTests.cs
  modified:
    - src/AsterGraph.Editor/AsterGraph.Editor.csproj
    - src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs
key-decisions:
  - "Diagnostics 入口直接挂到 `IGraphEditorSession.Diagnostics`，不新开第二套 factory-only 发现路径。"
  - "Inspection snapshot 只暴露 document/selection/viewport/capabilities/pending/status/node positions/recent diagnostics，不暴露 live view model。"
  - "Instrumentation contract 先只定义 `ILoggerFactory` 和 `ActivitySource`，具体 wiring 延后到 05-03。"
patterns-established:
  - "Phase 5 focused contract validation continues to use `%TEMP%\\astergraph-phase5-contract-validation\\AsterGraph.Phase5.Contracts.Validation.csproj` to avoid workspace-local test noise."
  - "Contract-first diagnostics work can add a minimal session-side implementation to preserve compile/runtime continuity before richer publication lands in 05-02."
requirements-completed: [DIAG-01, DIAG-02, DIAG-03]
duration: 26min
completed: 2026-03-26
---

# Phase 05-01 Summary

**Phase 5 的 diagnostics/inspection 公共合同面已经锁定，后续 05-02 和 05-03 可以在不再改宿主入口命名的前提下继续实现运行时检查与 instrumentation。**

## Performance

- **Duration:** 26 min
- **Started:** 2026-03-26T18:57:00+08:00
- **Completed:** 2026-03-26T19:23:29+08:00
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments

- 新增 `AsterGraph.Editor.Diagnostics` 下的 `IGraphEditorDiagnostics`、`GraphEditorInspectionSnapshot`、`GraphEditorPendingConnectionSnapshot`、`GraphEditorStatusSnapshot`、`GraphEditorInstrumentationOptions`。
- `IGraphEditorSession` 现在公开 `Diagnostics` 入口，宿主可以从 canonical runtime root 发现 diagnostics/inspection 合同。
- `AsterGraphEditorOptions` 新增 `Instrumentation` 属性，明确 Phase 5 的 logger/tracing 配置入口仍然属于 `AsterGraph.Editor`。
- 新增 `GraphEditorDiagnosticsContractsTests`，并通过 `%TEMP%` focused harness 验证了新的合同面。

## Task Commits

This plan was delivered in one execution commit:

1. **05-01 execution** - `335d5fb` (`feat(05-01): add diagnostics contract surface`)

## Files Created/Modified

- `src/AsterGraph.Editor/Diagnostics/*` - 新增 diagnostics/inspection 合同类型与 instrumentation 选项对象。
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - 为 runtime session root 新增 `Diagnostics` 属性。
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` - 为新合同面提供最小实现，保证当前 runtime/session 路径继续可编译、可实例化。
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` - 增加 `Instrumentation` 入口，为后续 05-03 factory/session wiring 预留配置面。
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsContractsTests.cs` - 用反射方式锁定新公共 API，避免 feature 缺失时直接编译失败。

## Decisions Made

- inspection aggregate 使用现有 `GraphEditorSelectionSnapshot`、`GraphEditorViewportSnapshot`、`GraphEditorCapabilitySnapshot` 和 `NodePositionSnapshot`，而不是复制字段。
- `GraphEditorInstrumentationOptions` 暂时只承载宿主标准日志/跟踪类型，不引入 AsterGraph 自定义 telemetry abstraction。
- 在 `05-01` 就给 `GraphEditorSession` 接上最小 `Diagnostics` 实现，以维持 `IGraphEditorSession` 改动后的编译与兼容路径连续性。

## Deviations from Plan

- **[Rule 3 - Blocking] 补充最小 session 侧实现** — 原计划文件没有把 [GraphEditorSession.cs](F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\Runtime\GraphEditorSession.cs) 列入 `files_modified`，但 `IGraphEditorSession` 新增 `Diagnostics` 属性后，现有 `GraphEditorSession` 不补实现会直接导致编译失败。已在同一轮中加入最小 `Diagnostics` facade 和 inspection snapshot 组装逻辑，未提前引入 05-02 的 recent-diagnostics 历史行为。

## Issues Encountered

- 初版 contract test 把 `GraphDocument` 错误地假定为 `AsterGraph.Editor` 程序集内类型，导致 GREEN 阶段留下 1 个假失败；已改为跨已加载程序集解析类型，验证通过。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `05-02` 现在可以在已锁定命名下补 `recent diagnostics` 历史、inspection 细节和 richer machine-readable publication。
- `05-03` 可以直接沿用 `GraphEditorInstrumentationOptions` 和 `AsterGraphEditorOptions.Instrumentation`，专注 runtime/factory wiring。

---
*Phase: 05-diagnostics-integration-inspection*
*Completed: 2026-03-26*
