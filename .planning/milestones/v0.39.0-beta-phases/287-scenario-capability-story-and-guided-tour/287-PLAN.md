# Phase 287: Scenario Capability Story And Guided Tour - Plan

## Goal

Make the Demo show a coherent SDK story covering custom nodes, parameters, validation, trust, automation, save/load, and export.

## Success Criteria

1. The scenario demo exercises custom nodes, parameter editing, connection validation, plugin/trust visibility, save/load, automation/proof output, and export.
2. A guided tour or equivalent in-demo flow walks the evaluator through creating nodes, connecting them, editing parameters, loading plugin content, and exporting output.
3. Demo tests or proof markers fail if the scenario loses any required capability signal.

## Tasks

1. Add scenario tour view-model state.
   - Define tour steps and proof signals.
   - Add step navigation and related-panel routing.
   - Add `RunScenarioTourStep` action handling.

2. Add the Tour drawer surface.
   - Add localized menu strings.
   - Add drawer section in `MainWindow.axaml`.
   - Keep layout within the existing host drawer design.

3. Make scenario actions execute real existing paths.
   - Add node via session command.
   - Connect ports via session command.
   - Edit selected node parameter via session command.
   - Trust plugin candidate through host trust policy.
   - Run plugin automation.
   - Save, reload, and export through workspace/export commands.

4. Defend with tests and docs.
   - Add tour model/action tests.
   - Add Avalonia rendering test for drawer controls.
   - Update Demo Guide for scenario launch and tour.

## Verification Commands

```powershell
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoScenarioLaunchTests|FullyQualifiedName~DemoCapabilityShowcaseTests|FullyQualifiedName~DemoHostMenuControlTests|FullyQualifiedName~DemoMainWindowTests"
```
