---
phase: 05-diagnostics-integration-inspection
plan: 03
subsystem: editor
tags: [diagnostics, instrumentation, logging, tracing, runtime]
requires:
  - phase: 05-diagnostics-integration-inspection
    plan: 01
    provides: instrumentation options contract on editor options
  - phase: 05-diagnostics-integration-inspection
    plan: 02
    provides: support-relevant diagnostics publication at runtime boundaries
provides:
  - canonical factory wiring for opt-in logger and activity instrumentation
  - runtime log emission correlated to diagnostics codes and operations
  - runtime ActivitySource spans for support-relevant operation diagnostics
  - focused validation for enabled vs disabled instrumentation paths
affects: [05-04, host-samples, smoke-tests]
tech-stack:
  added:
    - tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInstrumentationTests.cs
  patterns:
    - instrumentation hangs off diagnostics publication instead of adding a second telemetry pipeline
    - disabled instrumentation leaves diagnostics sink behavior unchanged
key-files:
  created:
    - tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInstrumentationTests.cs
  modified:
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
key-decisions:
  - "factory 在创建 editor 后直接配置底层 `GraphEditorSession` instrumentation，避免改动 `GraphEditorViewModel` 构造函数。"
  - "runtime instrumentation 统一挂在 `PublishDiagnostic` 上，让 05-02 已经定义的 support-relevant diagnostics 自动同时进入 logger 和 tracing。"
  - "Activity 名称直接复用 stable `Operation` 值，例如 `workspace.save`、`workspace.load`、`contextmenu.augment`。"
patterns-established:
  - "Phase 5 instrumentation validation continues to use `%TEMP%\\astergraph-phase5-instrumentation-validation\\AsterGraph.Phase5.Instrumentation.Validation.csproj` to stay isolated from workspace-local test noise."
  - "Host can enable diagnostics sink only, or sink plus logger/tracing, without changing editor API shape."
requirements-completed: [DIAG-03]
duration: 9min
completed: 2026-03-26
---

# Phase 05-03 Summary

**Phase 5 的 logger/tracing 接线已经落到 canonical editor factory/runtime 路径上，现有 diagnostics publication 现在可以被宿主的标准 .NET logging 和 Activity tooling 直接观察到。**

## Performance

- **Duration:** 9 min
- **Started:** 2026-03-26T19:37:05+08:00
- **Completed:** 2026-03-26T19:45:52+08:00
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments

- `AsterGraphEditorFactory` 现在会把 `AsterGraphEditorOptions.Instrumentation` 配置到创建出来的底层 `GraphEditorSession`。
- `GraphEditorSession` 现在会在 `PublishDiagnostic` 时同时向 `ILogger` 和 `ActivitySource` 发射关联信息。
- 日志级别按 `GraphEditorDiagnosticSeverity` 映射为 `Information` / `Warning` / `Error`，活动名直接复用稳定 operation 值。
- 新增 `GraphEditorDiagnosticsInstrumentationTests`，验证了 enabled instrumentation、host seam failure instrumentation 和 disabled instrumentation 三条路径。

## Task Commits

This plan was delivered in one execution commit:

1. **05-03 execution** - `a0f69d9` (`feat(05-03): wire diagnostics instrumentation`)

## Files Created/Modified

- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - 把 instrumentation 通过 canonical factory 路径注入到底层 runtime session。
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` - 新增 logger/activity 配置与 emission 逻辑，并把 diagnostics code/operation/message/severity 映射到日志和 tracing tags。
- `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInstrumentationTests.cs` - focused instrumentation regression 文件。

## Decisions Made

- 没有再改 `GraphEditorInstrumentationOptions` 和 `AsterGraph.Editor.csproj`，因为 05-01 已经把 contract 和 `Microsoft.Extensions.Logging.Abstractions` 依赖准备好了。
- logger/tracing emission 没有扩展到 Avalonia，也没有对普通 pointer/frame 级交互加 telemetry。
- instrumentation 的 enabled/disabled 语义完全由 `AsterGraphEditorOptions.Instrumentation` 是否传入控制，没有新增全局静态配置。

## Deviations from Plan

- **[Rule 3 - Blocking] 跳过不必要的 contract/package重复修改** — `05-03-PLAN` 列出了 [AsterGraph.Editor.csproj](F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\AsterGraph.Editor.csproj) 和 [GraphEditorInstrumentationOptions.cs](F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\Diagnostics\GraphEditorInstrumentationOptions.cs)，但 05-01 已经提供了所需依赖与 contract，继续重复改动只会制造无效 churn。本轮直接复用既有 contract，把实现集中在 factory/runtime wiring 和 focused validation 上。

## Issues Encountered

- 首轮 RED 先撞到了 test harness 只能看到 logging abstractions、看不到 `LoggerFactory.Create(...)` 实现的问题；已改成自带 `RecordingLoggerFactory`，随后转成真实 instrumentation 行为失败并完成绿色实现。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- `05-04` 现在可以直接用 host sample 和 smoke markers 去证明 diagnostics sink、inspection history、logger/tracing 这三层 story。
- Phase 5 代码侧核心合同和 runtime wiring 已齐，剩下主要是 proof ring 与文档收尾。

---
*Phase: 05-diagnostics-integration-inspection*
*Completed: 2026-03-26*
