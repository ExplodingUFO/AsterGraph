# Phase 286: README First View And Scenario Demo Launch - Summary

**Completed:** 2026-04-26
**Status:** Complete

## Delivered

- Added a deterministic README first-view SVG visual for the AI workflow scenario.
- Added `--scenario ai-pipeline` and `--scenario=ai-pipeline` parsing for `AsterGraph.Demo`.
- Added an AI pipeline scenario document with input, prompt, tool, LLM, parser, and output nodes.
- Scenario launch disables last-workspace/autosave restore so the prebuilt graph opens directly.
- Unknown scenario names fail directly instead of silently opening the default graph.
- Added tests for startup parsing, scenario graph shape, and README first-view markers.

## Files Changed

- `README.md`
- `README.zh-CN.md`
- `docs/assets/astergraph-ai-pipeline-demo.svg`
- `src/AsterGraph.Demo/DemoStartupOptionsParser.cs`
- `src/AsterGraph.Demo/DemoGraphFactory.cs`
- `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs`
- `src/AsterGraph.Demo/Program.cs`
- `src/AsterGraph.Demo/App.axaml.cs`
- `src/AsterGraph.Demo/Shell/MainWindowShellOptions.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Shell.cs`
- `tests/AsterGraph.Demo.Tests/DemoScenarioLaunchTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoProofReleaseSurfaceTests.cs`

## Deferred

- In-demo guided tour and broader capability walkthrough remain Phase 287.
- ConsumerSample scenario and five-minute onboarding remain Phase 288.
- Thin hosted builder and adoption proof gate remain Phase 289.
