# Technology Stack

**Analysis Date:** 2026-04-14

## Languages

- C# is the primary implementation language across `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tests/AsterGraph.Editor.Tests`, `tests/AsterGraph.Serialization.Tests`, `tools/AsterGraph.PackageSmoke`, and `tools/AsterGraph.ScaleSmoke`.
- Avalonia XAML (`.axaml`) is used for desktop UI markup in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/GraphInspectorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml`, `src/AsterGraph.Demo/App.axaml`, and `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- MSBuild XML drives the solution, package metadata, and project graph in `Directory.Build.props`, `avalonia-node-map.sln`, `avalonia-node-map.slnx`, and every `*.csproj` under `src/`, `tests/`, and `tools/`.
- Markdown is used for public documentation and GSD artifacts in `README.md`, `docs/*.md`, and `.planning/**/*.md`.

## Runtime

- Local SDK: `.NET SDK 10.0.201` via `dotnet --version`.
- Publishable libraries in `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `src/AsterGraph.Core/AsterGraph.Core.csproj`, `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, and `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` target `net8.0;net9.0`.
- The demo desktop app in `src/AsterGraph.Demo/AsterGraph.Demo.csproj` targets `net9.0`.
- The main UI-heavy test project in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` targets `net9.0`.
- The serialization regression project in `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj` targets `net8.0`.
- The maintained proof tools in `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` and `tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj` target `net8.0`.
- Package restore and build run through the `dotnet` CLI and NuGet/MSBuild restore flow.
- No `global.json`, `Directory.Packages.props`, or `packages.lock.json` is tracked at the repository root.

## Frameworks And Libraries

- `Microsoft.NET.Sdk` is the base SDK for every project.
- Avalonia `11.3.10` is the UI framework used by `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Avalonia.Markup.Xaml.Loader` `11.3.10` is used by `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`.
- `Avalonia.Desktop` `11.3.10`, `Avalonia.Themes.Fluent` `11.3.10`, `Avalonia.Fonts.Inter` `11.3.10`, and `Avalonia.Diagnostics` `11.3.10` are used by the demo host in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Avalonia.Headless.XUnit` `11.3.10` and `Avalonia.Themes.Fluent` `11.3.10` support headless UI tests in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- `CommunityToolkit.Mvvm` `8.2.1` is used by `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Microsoft.Extensions.Logging.Abstractions` `9.0.0` is referenced by `src/AsterGraph.Editor/AsterGraph.Editor.csproj` to support optional instrumentation hooks.
- `System.Text.Json` is the built-in serializer used in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs` and `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`.
- `xunit` `2.9.2`, `xunit.runner.visualstudio` `2.8.2`, and `Microsoft.NET.Test.Sdk` `17.11.1` back the test projects under `tests/`.

## Key Dependencies

- `src/AsterGraph.Abstractions` stays BCL-only so hosts can reference contracts without UI baggage.
- `src/AsterGraph.Core` depends only on `src/AsterGraph.Abstractions`.
- `src/AsterGraph.Editor` depends on `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `CommunityToolkit.Mvvm`, and `Microsoft.Extensions.Logging.Abstractions`.
- `src/AsterGraph.Avalonia` depends on `src/AsterGraph.Core`, `src/AsterGraph.Editor`, Avalonia, and `CommunityToolkit.Mvvm`.
- `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj` validates both direct project references and packed package consumption of `AsterGraph.Abstractions`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- `tools/AsterGraph.ScaleSmoke/Program.cs` is now part of the maintained proof surface for large-graph and session-state continuity.

## Configuration

- Shared package metadata, versioning, XML-doc generation, CI toggles, and artifact exclusions live in `Directory.Build.props`.
- Current package version is `0.1.0-preview.8` in `Directory.Build.props`.
- `Directory.Build.props` suppresses `CS1591` while the repo continues to pay down XML-doc debt and excludes `artifacts/**` from default compile globs.
- Solution entry points are `avalonia-node-map.sln` and `avalonia-node-map.slnx`.
- `NuGet.config.sample` documents a public NuGet source plus a local `artifacts/packages` feed flow.
- `src/AsterGraph.Demo/app.manifest` contains the demo's Windows desktop manifest.
- No `.env` files or required environment variables are tracked.

## Platform And Delivery

- The primary shipped deliverables are the four NuGet packages under `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, and `src/AsterGraph.Avalonia`.
- Local package output is documented at `artifacts/packages` in `README.md`.
- The only runnable UI application in-repo is the sample-only Avalonia desktop demo in `src/AsterGraph.Demo`.
- The repository has no server runtime, container target, database runtime, or cloud deployment manifests.
- Runtime validation is split between `dotnet test`, `tools/AsterGraph.PackageSmoke`, and `tools/AsterGraph.ScaleSmoke`.

---

*Stack analysis refreshed: 2026-04-14*
