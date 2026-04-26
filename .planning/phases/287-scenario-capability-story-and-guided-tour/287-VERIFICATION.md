---
status: passed
phase: 287
completed: 2026-04-26
---

# Phase 287 Verification

## Result

Passed.

## Automated Checks

```powershell
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoScenarioLaunchTests|FullyQualifiedName~DemoCapabilityShowcaseTests|FullyQualifiedName~DemoHostMenuControlTests|FullyQualifiedName~DemoMainWindowTests"
```

Result: passed, 38 tests.

```powershell
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter FullyQualifiedName~DemoProofReleaseSurfaceTests
```

Result: passed, 32 tests.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-public-versioning.ps1 -RepoRoot . -PublicTag v0.11.0-beta
```

Result: `PUBLIC_VERSIONING_OK:0.11.0-beta:v0.11.0-beta`.

## Success Criteria

1. Scenario demo exercises custom nodes, parameters, validation, trust, automation, save/load, proof output, and export.
   - Verified by `MainWindowViewModel_RunsScenarioTourActionsAndProducesProofArtifacts`.

2. Guided tour or equivalent in-demo flow walks the evaluator through the scenario.
   - Verified by `MainWindowViewModel_ExposesScenarioTourForAiPipelineCapabilities` and `MainWindow_RendersScenarioTourDrawerControls`.

3. Demo tests or proof markers fail if scenario capability signals disappear.
   - Verified by tour action tests and Demo release-surface tests.

## Manual Checks

No manual GUI check was required for this phase. The Tour drawer surface and executable flow are covered by Avalonia headless tests and view-model action tests.
