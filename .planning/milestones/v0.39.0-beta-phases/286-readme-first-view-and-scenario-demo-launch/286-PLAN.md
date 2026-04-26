# Phase 286: README First View And Scenario Demo Launch - Plan

## Goal

Productize the first public impression with a concrete visual and a launchable prebuilt scenario demo.

## Success Criteria

1. README first viewport includes a concrete visual that shows drag, connect, parameter editing, automation, and export in one scenario.
2. The Demo host can launch a prebuilt scenario from command line without requiring blank-canvas setup.
3. Automated proof confirms the scenario launch path is stable.

## Tasks

1. Add a scenario startup option.
   - Parse `--scenario ai-pipeline` and `--scenario=ai-pipeline`.
   - Disable last-workspace restore for scenario launch.
   - Reject unknown scenario names.

2. Add the AI pipeline demo document.
   - Add only the demo node definitions required for the scenario.
   - Create a prewired graph through `DemoGraphFactory`.
   - Keep the default terrain graph unchanged.

3. Add README first-view proof.
   - Add a deterministic visual asset under `docs/assets`.
   - Link it near the top of both English and Chinese READMEs.
   - Include the exact scenario launch command.

4. Verify with focused tests.
   - Parser and graph-factory unit assertions.
   - README marker assertions.
   - Existing demo proof/release-surface tests.

## Verification Commands

```powershell
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoScenarioLaunchTests|FullyQualifiedName~DemoProofReleaseSurfaceTests|FullyQualifiedName~DemoCapabilityShowcaseTests"
```
