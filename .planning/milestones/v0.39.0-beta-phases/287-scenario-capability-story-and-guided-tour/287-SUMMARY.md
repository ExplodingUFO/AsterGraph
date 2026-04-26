# Phase 287: Scenario Capability Story And Guided Tour - Summary

**Completed:** 2026-04-26
**Status:** Complete

## Delivered

- Added a `Tour` host menu group and right-side drawer surface in `AsterGraph.Demo`.
- Added scenario tour steps for creating a node, connecting ports, editing parameters, trusting a plugin, running automation, and save/load/export.
- Added executable tour actions that use existing session commands, plugin trust policy, automation, workspace save/load, and SVG export.
- Added scenario evidence lines for custom nodes, typed connection, parameter editing, plugin trust, automation, save/load, and export.
- Updated Demo automation focus nodes to use `output`, which exists in both the default graph and the AI pipeline scenario.
- Updated English and Chinese Demo Guide docs with the AI scenario launch and Tour path.
- Added tests for tour model state, executable tour actions, drawer rendering, and menu exposure.

## Files Changed

- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.GuidedTour.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Localization.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Showcase.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.RuntimeProjection.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Automation.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- `src/AsterGraph.Demo/Views/MainWindow.axaml`
- `tests/AsterGraph.Demo.Tests/DemoCapabilityShowcaseTests.cs`
- `docs/en/demo-guide.md`
- `docs/zh-CN/demo-guide.md`

## Deferred

- ConsumerSample scenario onboarding remains Phase 288.
- Thin host builder/facade and adoption proof gate remain Phase 289.
