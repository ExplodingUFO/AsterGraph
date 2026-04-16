# Phase 42 Verification

## Results

- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~TestPluginArtifactPathTests"` => passed (`1/1`)
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~GraphEditorPlugin"` => passed (`52/52`)
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release` => passed (`156/156`)
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework net9.0 -Configuration Release` => passed (`373/373` editor tests, `35/35` demo tests)
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~GraphEditorPluginContractsTests"` => passed (`16/16`) after normalizing path-based contract assertions to `Path.GetFullPath(...)`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release` => passed (`8/8` serialization tests, `373/373` editor tests, `35/35` demo tests) after the hosted-runner parity correction
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release` => passed after the hosted-runner parity correction

## Notes

- Plugin proof tests no longer resolve `tests/AsterGraph.TestPlugins/bin/Debug/net9.0/...`; they now bind to the active test configuration and target framework.
- The repo-local `contract` and `all` lanes stayed on the existing command path after the fix, which keeps local validation aligned with clean GitHub-hosted runner behavior.
- The first GitHub-hosted Linux run still exposed one remaining clean-runner gap in `GraphEditorPluginContractsTests`: several assertions compared raw Windows literals instead of the canonical filesystem paths exposed by the runtime contracts. The follow-up parity correction now asserts canonicalized paths and keeps Windows/Linux expectations aligned.
