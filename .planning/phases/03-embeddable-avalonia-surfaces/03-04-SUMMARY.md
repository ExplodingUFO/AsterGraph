---
phase: 03-embeddable-avalonia-surfaces
plan: 04
subsystem: ui
tags: [docs, host-sample, smoke, avalonia, embedding]
requires:
  - phase: 03-01
    provides: standalone canvas surface and stock menu presenter
  - phase: 03-02
    provides: standalone inspector and mini map surfaces
  - phase: 03-03
    provides: full-shell composition over standalone surfaces
provides:
  - consumer-facing host sample for full shell plus standalone surfaces
  - package smoke markers for the Phase 3 surface matrix
  - aligned Phase 3 host documentation across root and package docs
affects: [phase-4 planning, external consumers, docs]
tech-stack:
  added: []
  patterns:
    - sample and smoke output use explicit machine-checkable surface markers
    - docs describe full shell and standalone composition as one coherent matrix
key-files:
  created: []
  modified:
    - README.md
    - docs/host-integration.md
    - src/AsterGraph.Avalonia/README.md
    - tools/AsterGraph.HostSample/Program.cs
    - tools/AsterGraph.PackageSmoke/Program.cs
key-decisions:
  - "用 host sample 的可读输出和 package smoke 的 machine-checkable markers 共同证明 Phase 3 surface matrix。"
  - "文档明确 header/library/status 仍是 shell-only，完整 presenter replacement 继续推迟到 Phase 4。"
patterns-established:
  - "Consumer docs, sample output, and smoke markers all describe the same full-shell-plus-standalone-surface story."
  - "Standalone canvas behavior toggles are documented as explicit opt-outs, not hidden implementation knobs."
requirements-completed: [EMBD-01, EMBD-02, EMBD-03, EMBD-04, EMBD-05]
duration: 3min
completed: 2026-03-26
---

# Phase 03-04 Summary

**host sample、package smoke 与三份消费者文档现已统一到同一套 Phase 3 surface matrix 叙事，并由最终验证环证明可运行。**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-26T16:01:18+08:00
- **Completed:** 2026-03-26T16:03:25+08:00
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments

- host sample 同时展示了 full shell convenience path 与 standalone canvas/inspector/minimap composition path。
- package smoke 保留了 legacy/factory/session markers，并新增了 `SURFACE_*` 与 `MENU_STOCK_PRESENTER_OK` 标记。
- README、host integration guide、`AsterGraph.Avalonia` README 现在都明确说明 standalone surface factories、canvas opt-out toggles，以及 shell-only chrome 边界。

## Task Commits

This plan was delivered in one execution commit:

1. **03-04 execution** - `860859f` (`feat(03-04): document and prove embeddable surfaces`)

## Files Created/Modified

- `tools/AsterGraph.HostSample/Program.cs` - 输出 full shell 与 standalone surfaces 共用 editor state 的证据。
- `tools/AsterGraph.PackageSmoke/Program.cs` - 增加 Phase 3 surface markers 与 stock presenter smoke marker。
- `README.md` - 根 README 增加 standalone surface matrix 与 canvas opt-out toggles 说明。
- `docs/host-integration.md` - 集成指南增加 host-managed Avalonia surface composition 路径。
- `src/AsterGraph.Avalonia/README.md` - 包 README 明确 full shell 和 standalone surface 的支持矩阵。

## Decisions Made

- consumer-facing 证明分成两层：host sample 负责人类可读叙事，package smoke 负责机器可校验标记。
- 文档不夸大能力边界，明确 presenter replacement 仍在 Phase 4，而不是在 Phase 3 暗示已支持。

## Deviations from Plan

None - plan executed as specified.

## Issues Encountered

- `PackageSmoke` 中把泛型 `FindControl<T>` 直接塞进插值表达式时，编译器解析不稳定；后续改为先取局部变量再输出 marker。

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 3 的 full shell、standalone surfaces、opt-out toggles、stock presenter、sample、smoke、docs 已经形成完整闭环。
- Phase 4 可以在此基础上继续做 presenter replacement，而不需要再回头补 consumer-facing Phase 3 叙事。

---
*Phase: 03-embeddable-avalonia-surfaces*
*Completed: 2026-03-26*
