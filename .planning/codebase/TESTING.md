# Testing Patterns

**Analysis Date:** 2026-04-14

## Test Frameworks

- `xunit` `2.9.2` is the base test framework for both projects under `tests/`.
- `Microsoft.NET.Test.Sdk` `17.11.1` provides test runner integration.
- `xunit.runner.visualstudio` `2.8.2` is included for IDE and CLI discovery.
- `Avalonia.Headless.XUnit` `11.3.10` and `Avalonia.Themes.Fluent` `11.3.10` support headless UI tests in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- No `FluentAssertions`, snapshot framework, or mocking library is used.

## Test Project Layout

- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` targets `net9.0` and references all four library projects plus `src/AsterGraph.Demo`.
- `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj` targets `net8.0` and focuses on persistence compatibility.
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
- `src/AsterGraph.Demo` remains a sample-only host rather than a supported package, and demo-focused suites such as `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs`, `tests/AsterGraph.Editor.Tests/DemoHostMenuControlTests.cs`, and `tests/AsterGraph.Editor.Tests/GraphEditorDemoShellTests.cs` keep that composition path under regression coverage.
- The repository does not contain browser automation, screenshot approval tests, or end-to-end desktop automation.

## Smoke And Proof Tools

- `tools/AsterGraph.PackageSmoke/Program.cs` is a package-surface regression tool that exercises both legacy and factory/session paths.
- `tools/AsterGraph.ScaleSmoke/Program.cs` emits repeatable `SCALE_*` markers for large-graph and runtime-state continuity checks.
- Together with the editor and serialization xUnit projects, these tools make up the maintained proof surface even though they are not formal test projects.

## Commands

```powershell
dotnet test avalonia-node-map.sln
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```

## Coverage And Gaps

- No coverage collector, threshold, or `runsettings` file is tracked.
- No checked-in CI workflow was detected under `.github/workflows/`.
- The repo relies on xUnit regressions plus smoke/proof tools rather than benchmark automation or coverage enforcement.
- Current risk areas are less about missing tests entirely and more about maintaining alignment across:
  - kernel-first runtime path
  - retained `GraphEditorViewModel` compatibility path
  - Avalonia full-shell and standalone surfaces

## Testing Conventions

- Test files follow the same `PascalCaseTests.cs` naming pattern as the rest of the repo.
- Handwritten doubles often throw `NotSupportedException` for unused members to keep proof code small and explicit.
- Assertions use `Xunit.Assert`; no custom assertion DSL is present.
- The test suite is a mix of unit, integration, contract, and migration-proof coverage rather than a strict pyramid.

---

*Testing analysis refreshed: 2026-04-14*
