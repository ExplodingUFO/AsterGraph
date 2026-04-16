# Testing Patterns

**Analysis Date:** 2026-04-16

## Test Frameworks

- `xunit` `2.9.2` is the base test framework for all test projects.
- `Microsoft.NET.Test.Sdk` `17.11.1` provides test runner integration.
- `xunit.runner.visualstudio` `2.8.2` is included for IDE and CLI discovery.
- `Avalonia.Headless.XUnit` `11.3.10` and `Avalonia.Themes.Fluent` `11.3.10` support headless UI tests in both UI test projects.
- No `FluentAssertions`, snapshot framework, or mocking library is used.

## Test Project Layout

- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` targets `net9.0` and focuses on core SDK runtime/behavior proofing.
- `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj` targets `net8.0` and focuses on persistence compatibility.
- `tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj` targets `net9.0` and owns demo/sample-host integration surface tests and host-shell coverage.
- Test code is kept in dedicated projects rather than colocated beside production code.

## Main Test Areas

- Runtime session contracts: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- Migration parity between retained and factory/session paths: `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`
- Proof-ring coverage for host and runtime scenarios: `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs`
- Diagnostics contracts, snapshots, and instrumentation: `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsContractsTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInstrumentationTests.cs`
- Surface composition and standalone view factories: `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs`, `tests/AsterGraph.Editor.Tests/GraphInspectorStandaloneTests.cs`, `tests/AsterGraph.Editor.Tests/GraphMiniMapStandaloneTests.cs`, `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs`
- View-model/editor behavior: `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`
- Serialization compatibility: `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`

## Test Style

- Most tests are plain `[Fact]` methods with explicit arrange/act/assert structure.
- Suites usually create real `GraphDocument`, `NodeCatalog`, `GraphEditorViewModel`, and `IGraphEditorSession` instances rather than mocking the product core.
- Host/platform seams are replaced with handwritten doubles inside the test file when needed.
- Local static factory methods are preferred over shared helper libraries.

## UI And Integration Testing

- Headless Avalonia tests validate controls and surface behavior without spinning up the demo app interactively.
- Integration-style suites cover:
  - runtime command/query/event behavior
  - diagnostics continuity
  - host-service overrides
  - menu/presentation replacement seams
  - legacy-vs-factory migration parity
- `tests/AsterGraph.Demo.Tests` is the dedicated lane for demo/sample-host coverage, including:
  - `tests/AsterGraph.Demo.Tests/DemoMainWindowTests.cs`
  - `tests/AsterGraph.Demo.Tests/DemoHostMenuControlTests.cs`
  - `tests/AsterGraph.Demo.Tests/GraphEditorDemoShellTests.cs`
  - `tests/AsterGraph.Demo.Tests/GraphEditorLocalizationDemoTests.cs`
- `tests/AsterGraph.Editor.Tests` is no longer responsible for demo host regression suites.
- The repository does not contain browser automation, screenshot approval tests, or end-to-end desktop automation.

## Smoke And Proof Tools

- `tools/AsterGraph.HostSample/Program.cs` is the minimal consumer-facing host sample. It proves the canonical runtime-first and hosted-UI routes and can switch to packed-package restore.
- `tools/AsterGraph.PackageSmoke/Program.cs` is the package-surface regression tool that exercises runtime-first, hosted-UI, and retained compatibility paths.
- `tools/AsterGraph.ScaleSmoke/Program.cs` emits repeatable `SCALE_*` markers for large-graph, history/save, and runtime-state continuity checks.
- Together with the three xUnit projects and `eng/ci.ps1`, these tools form the maintained proof ring.

## Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```

## Coverage And Gaps

- No coverage collector, threshold, or `runsettings` file is tracked.
- `.github/workflows/ci.yml` is checked in and invokes `eng/ci.ps1` for matrixed lane validation plus release verification.
- The repo relies on xUnit regressions plus proof tools rather than benchmark automation or coverage enforcement.
- `eng/ci.ps1 -Lane release` is the official scripted gate; raw `dotnet run` commands remain useful when contributors want direct sample or smoke markers while debugging.
- Current risk areas are less about missing tests entirely and more about maintaining alignment across:
  - kernel-first runtime path
  - retained `GraphEditorViewModel` compatibility path
  - split-lane proof alignment (core SDK vs demo/sample host)
  - the minimal consumer host sample versus the heavier smoke tools

## Testing Conventions

- Test files follow the same `PascalCaseTests.cs` naming pattern as the rest of the repo.
- Handwritten doubles often throw `NotSupportedException` for unused members to keep proof code small and explicit.
- Assertions use `Xunit.Assert`; no custom assertion DSL is present.
- The test suite is a mix of unit, integration, contract, and migration-proof coverage rather than a strict pyramid.

---

*Testing analysis refreshed: 2026-04-16*
