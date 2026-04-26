---
status: passed
phase: 286
completed: 2026-04-26
---

# Phase 286 Verification

## Result

Passed.

## Automated Checks

```powershell
dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~DemoScenarioLaunchTests|FullyQualifiedName~DemoProofReleaseSurfaceTests|FullyQualifiedName~DemoCapabilityShowcaseTests"
```

Result: passed, 45 tests.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\validate-public-versioning.ps1 -RepoRoot . -PublicTag v0.11.0-beta
```

Result: `PUBLIC_VERSIONING_OK:0.11.0-beta:v0.11.0-beta`.

## Success Criteria

1. README first viewport includes a concrete visual showing drag, connect, parameter editing, automation, and export.
   - Verified by `PublicReadmes_ShowPrebuiltScenarioInFirstView`.

2. Demo host can launch a prebuilt scenario from command line.
   - Verified by `StartupOptionsParser_ParsesScenarioFlagAndKeepsAvaloniaArgs` and `StartupOptionsParser_ParsesScenarioEqualsForm`.

3. Automated proof confirms scenario launch path is stable.
   - Verified by `DemoGraphFactory_CreatesPrewiredAiPipelineScenario` and README marker tests.

## Manual Checks

No manual GUI check was required for this phase. The scenario entry point and README first-view contract are covered by unit/doc tests.
