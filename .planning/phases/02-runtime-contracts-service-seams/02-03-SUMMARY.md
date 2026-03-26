---
phase: 02-runtime-contracts-service-seams
plan: 03
subsystem: service-seams
tags: [services, diagnostics, storage, clipboard, contracts, tests]
requires:
  - phase: 02-01
    provides: runtime contract framing and Wave 0 testing pattern
provides:
  - replaceable persistence/template/clipboard service contracts
  - machine-readable diagnostics sink contract
  - package-neutral storage-default policy
  - Wave 0 service-seam regression file
affects: [02-04, 02-05, host-replacement-story]
tech-stack:
  added: []
  patterns:
    - public service interfaces before factory rewiring
    - explicit storage-root resolver without host-identity inference
key-files:
  created:
    - src/AsterGraph.Editor/Services/IGraphWorkspaceService.cs
    - src/AsterGraph.Editor/Services/IGraphFragmentWorkspaceService.cs
    - src/AsterGraph.Editor/Services/IGraphFragmentLibraryService.cs
    - src/AsterGraph.Editor/Services/IGraphClipboardPayloadSerializer.cs
    - src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs
    - src/AsterGraph.Editor/Diagnostics/GraphEditorDiagnostic.cs
    - src/AsterGraph.Editor/Services/GraphEditorStorageDefaults.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs
  modified:
    - src/AsterGraph.Editor/Services/GraphSelectionFragment.cs
key-decisions:
  - "diagnostics sink 只保留 machine-readable publish 合同，不变成 logging framework。"
  - "默认存储路径通过 shared helper 解析，不再推断宿主身份。"
patterns-established:
  - "Service seams become public interfaces before options/factory switch-over."
  - "Storage defaults are explicit and package-neutral."
requirements-completed: [SERV-01, SERV-02]
duration: 1h+
completed: 2026-03-26
---

# Phase 02 Plan 03 Summary

**发布了第一批 replaceable service/diagnostics 合同，并把默认存储路径策略改成 package-neutral 的显式规则。**

## Accomplishments

- 增加工作区、片段工作区、模板库、剪贴板负载四个 public service interface。
- 增加 `IGraphEditorDiagnosticsSink` 与 `GraphEditorDiagnostic`。
- 引入 `GraphEditorStorageDefaults`，统一默认路径解析。
- 新增 `GraphEditorServiceSeamsTests.cs` 作为后续 wiring 的回归锚点。

## Task Commits

1. `fbd511f` `feat(02-03): add service seam contracts`
2. `6434405` `test(02-03): add storage default seam coverage`

## Decisions Made

- 保持这一步是 contract-only，不提前切换 `AsterGraphEditorOptions` 的 public composition。
- 默认路径策略彻底避开 demo branding 和 host identity 猜测。

## Deviations from Plan

None.

## Issues Encountered

None recorded beyond normal contract extraction.

## Outcome

`02-04` 已经具备把默认实现和 public options 切到 interface 驱动模式所需的合同基础。
