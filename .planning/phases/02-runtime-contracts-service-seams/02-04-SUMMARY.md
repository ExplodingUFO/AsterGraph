---
phase: 02-runtime-contracts-service-seams
plan: 04
subsystem: service-seams
tags: [services, diagnostics, storage, runtime, factory, compatibility]
requires:
  - phase: 02-02
    provides: concrete runtime session and factory/session bridge
  - phase: 02-03
    provides: public service and diagnostics contracts plus storage defaults
provides:
  - interface-driven default services and factory options
  - diagnostics publication through runtime and compatibility facade
  - tested service replacement and package-neutral defaults
affects: [02-05, host-consumers, migration-story]
tech-stack:
  added: []
  patterns:
    - public options accept interfaces plus explicit storage root
    - recoverable failures flow to diagnostics sink and compatibility status text
key-files:
  created: []
  modified:
    - src/AsterGraph.Editor/Services/GraphWorkspaceService.cs
    - src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs
    - src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs
    - src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs
    - src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs
    - src/AsterGraph.Editor/Runtime/GraphEditorSession.cs
    - src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
    - tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs
key-decisions:
  - "兼容 facade 保留 StatusMessage 路径，同时新增 diagnostics sink 的 machine-readable publication。"
  - "默认实现全部通过 GraphEditorStorageDefaults 解析路径。"
patterns-established:
  - "Service replacement is driven through AsterGraphEditorOptions, not concrete service types."
  - "CompatibilityService remains publicly replaceable alongside the new service seams."
requirements-completed: [SERV-01, SERV-02]
duration: 1h+
completed: 2026-03-26
---

# Phase 02 Plan 04 Summary

**把 service seam 合同真正接入了默认实现、runtime/session 和 factory/options 组合路径。**

## Accomplishments

- 默认工作区、片段、模板、剪贴板实现切到 interface 驱动。
- `AsterGraphEditorOptions` 和 factory/session 组合路径接受 interface、diagnostics sink 与显式 `StorageRootPath`。
- recoverable failures 同时进入 diagnostics sink 和兼容 `StatusMessage` 路径。
- `GraphEditorServiceSeamsTests.cs` 扩展为真实行为验证，覆盖 host replacement 与 package-neutral default path。

## Task Commits

1. `71a3989` `feat(02-04): wire service seams through runtime`

## Decisions Made

- diagnostics publication 与 retained compatibility UX 同时保留，而不是二选一。
- CompatibilityService replacement 继续作为 public host seam，与新增服务 seam 并存。

## Deviations from Plan

None.

## Issues Encountered

None recorded beyond normal wiring work.

## Outcome

Phase 2 的 runtime/service 基础能力已经到位，`02-05` 只剩 consumer-facing sample、smoke、doc 和验证闭环。
