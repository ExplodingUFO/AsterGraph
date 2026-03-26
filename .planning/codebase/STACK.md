# Technology Stack

**Analysis Date:** 2026-03-25

## Languages

**Primary:**
- C# - Main implementation language across the solution in `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tests/AsterGraph.Editor.Tests`, `tests/AsterGraph.Serialization.Tests`, `tools/AsterGraph.HostSample`, and `tools/AsterGraph.PackageSmoke`.

**Secondary:**
- Avalonia XAML (`.axaml`) - Desktop UI markup for the Avalonia shell in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml`, `src/AsterGraph.Demo/App.axaml`, and `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- MSBuild XML - Build, packaging, and solution metadata in `Directory.Build.props`, `avalonia-node-map.sln`, `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `src/AsterGraph.Core/AsterGraph.Core.csproj`, `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, `src/AsterGraph.Demo/AsterGraph.Demo.csproj`, `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`, `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`, `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`, `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`, and `NuGet.config.sample`.

## Runtime

**Environment:**
- .NET SDK `10.0.201` detected locally via `dotnet --version`.
- Target frameworks are `net8.0` and `net9.0` for the packable libraries in `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `src/AsterGraph.Core/AsterGraph.Core.csproj`, `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, and `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`.
- The demo desktop application targets `net9.0` in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- Test projects target `net8.0` in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.

**Package Manager:**
- NuGet via the .NET CLI and MSBuild project restore flow, as implied by all `*.csproj` files and `NuGet.config.sample`.
- Lockfile: missing. `packages.lock.json`, `Directory.Packages.props`, and `global.json` are not detected in the repository root.

## Frameworks

**Core:**
- Microsoft.NET.Sdk - Base SDK for every project file under `src/`, `tests/`, and `tools/`.
- Avalonia `11.3.10` - Desktop UI framework used by the shell package in `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` and by the demo app in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- CommunityToolkit.Mvvm `8.2.1` - MVVM support used by `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- System.Text.Json - Built-in JSON serialization stack used in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentCompatibility.cs`, `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`, and `src/AsterGraph.Editor/Services/GraphClipboardPayloadCompatibility.cs`.

**Testing:**
- xUnit `2.9.2` - Unit test framework in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Microsoft.NET.Test.Sdk `17.11.1` - Test runner integration in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Avalonia.Headless.XUnit `11.3.10` - Headless Avalonia UI testing in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.

**Build/Dev:**
- `dotnet` CLI - Build, run, test, and pack flows documented in `README.md`.
- Visual Studio solution metadata - Multi-project orchestration in `avalonia-node-map.sln`.
- Shared MSBuild packaging settings - Versioning, symbols, docs, CI build flag, and NuGet metadata in `Directory.Build.props`.

## Key Dependencies

**Critical:**
- `Avalonia` `11.3.10` - Required for the reusable UI shell package in `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`.
- `Avalonia.Desktop` `11.3.10` - Required for the runnable desktop host in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Avalonia.Themes.Fluent` `11.3.10` - Fluent theme resources for the demo host in `src/AsterGraph.Demo/AsterGraph.Demo.csproj` and headless UI tests in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- `Avalonia.Fonts.Inter` `11.3.10` - Bundled font dependency for the demo app in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `CommunityToolkit.Mvvm` `8.2.1` - View-model and command support for editor and host layers in `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.

**Infrastructure:**
- `Avalonia.Diagnostics` `11.3.10` - Debug-only diagnostics support in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `xunit.runner.visualstudio` `2.8.2` - IDE and test runner adapter in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Local AsterGraph package references - Smoke-test package consumption through `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`.

## Configuration

**Environment:**
- Shared package metadata, version, symbol settings, nullable context, implicit usings, and optional CI build flag are configured in `Directory.Build.props`.
- No `.env` files are detected at the repository root.
- No runtime secrets or required environment variables are defined in tracked files.
- Optional `CI=true` enables `ContinuousIntegrationBuild` in `Directory.Build.props`.

**Build:**
- Solution entry point: `avalonia-node-map.sln`.
- Shared build metadata: `Directory.Build.props`.
- Project-specific build and package definitions: every `*.csproj` under `src/`, `tests/`, and `tools/`.
- Local NuGet feed sample: `NuGet.config.sample`.
- Windows desktop manifest for the demo host: `src/AsterGraph.Demo/app.manifest`.

## Platform Requirements

**Development:**
- Use a .NET SDK that can restore and build both `net8.0` and `net9.0` projects. The current workspace has SDK `10.0.201` installed.
- Use the `dotnet` CLI for the documented flows in `README.md`: `dotnet build`, `dotnet run`, and `dotnet pack`.
- Use Avalonia-compatible desktop tooling when working on `src/AsterGraph.Avalonia` and `src/AsterGraph.Demo`.

**Production:**
- Primary deliverables are NuGet packages from `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, and `src/AsterGraph.Avalonia`, packed to `artifacts/packages` as documented in `README.md`.
- The runnable application target in the repo is the Avalonia desktop demo in `src/AsterGraph.Demo`.
- No server runtime, container target, or cloud deployment target is defined in tracked configuration files.

---

*Stack analysis: 2026-03-25*
