# Technology Stack

**Project:** AsterGraph
**Researched:** 2026-03-25
**Focus:** Publishable general-purpose .NET + Avalonia component library

## Recommended Stack

### Core Framework
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| .NET SDK | `10.0.201+` | Build, pack, package validation, Source Link, CI baseline | Use the current SDK line for packaging features and current servicing. Keep build infra on .NET 10 even if some packages still target `net8.0`. |
| Packable target frameworks | `net8.0;net10.0` | Supported consumer runtime matrix for publishable packages | Prefer LTS-to-LTS targeting for a library. `net9.0` is an active STS release on 2026-03-25, but it adds maintenance cost without a durable support window. Move the package line from `net8.0;net9.0` to `net8.0;net10.0`. |
| Avalonia | `11.3.12` | UI framework for the host-facing control library | Stay on the current Avalonia 11.3 stable train. The repo is on `11.3.10`; update to the latest 11.3 patch before publishing a public component story. |

### Package Layout
| Package | Target | Purpose | Why |
|---------|--------|---------|-----|
| `AsterGraph.Abstractions` | `net8.0;net10.0` | Stable contracts, identifiers, styling tokens, diagnostics contracts | This is the package hosts should compile against for long-lived extension points. Keep it free of Avalonia references. |
| `AsterGraph.Core` | `net8.0;net10.0` | Document model, serialization, compatibility rules | Keep persistence and graph-domain rules isolated from editor orchestration and UI. |
| `AsterGraph.Editor` | `net8.0;net10.0` | Headless editor state, commands, events, host extensibility, diagnostics surface | This should be the non-UI integration package. Do not let Avalonia types leak into it. |
| `AsterGraph.Avalonia` | `net8.0;net10.0` | Avalonia controls, themes, input adapters, default shell composition | This is the optional UI package. Hosts that only need contracts or headless state should not need to reference it. |

### Supporting Libraries
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `CommunityToolkit.Mvvm` | `8.4.0` | Observable state and command implementation | Keep using it inside `AsterGraph.Editor` and demo/sample hosts. Avoid exposing toolkit-specific types in public contracts. |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.4` | Standard .NET logging surface for diagnostics | Add to `AsterGraph.Editor` for host-supplied logging and debug visibility. Accept `ILoggerFactory` at composition boundaries and default to `NullLoggerFactory.Instance`. |
| `Microsoft.SourceLink.GitHub` | `10.0.200` | Step-into-source debugging from NuGet packages | Add as a private build dependency to every packable project. The repo already emits `snupkg`; Source Link closes the debugging loop. |
| Avalonia `ControlTheme` + `StyledProperty`/`DirectProperty` patterns | Avalonia 11 | Public control theming and API exposure | Use for reusable controls in `AsterGraph.Avalonia`. This is the correct public surfacing mechanism for host-restylable controls. |
| `Avalonia.Headless.XUnit` | `11.3.12` | Headless verification for control splitting and contract-safe visual behavior | Keep for package-level UI tests around control composition, template behavior, and host extensibility seams. |

## Recommended Stack Decisions

### 1. Keep Four Publishable Packages, But Harden the Boundaries

Do not collapse the package line into one monolith. The existing split is already the right product shape for a general-purpose SDK:

- `AsterGraph.Abstractions` is the contract package.
- `AsterGraph.Core` is the persistence and model package.
- `AsterGraph.Editor` is the headless behavior package.
- `AsterGraph.Avalonia` is the default UI package.

The implementation implication is that new host-facing seams for component splitting, debugging, and secondary development should land in `AsterGraph.Abstractions` or `AsterGraph.Editor` first, and only then be adapted in `AsterGraph.Avalonia`.

### 2. Publish Lookless Controls, Not App-Shaped `UserControl`s

For reusable Avalonia components:

- Use `TemplatedControl` for generic, host-restylable pieces such as inspector panes, mini-map shells, toolbars, and menu presenters.
- Use `Control` for render-heavy primitives such as graph canvas and mini-map rendering surfaces.
- Keep `UserControl` for demo/sample composition or convenience wrappers such as a top-level `GraphEditorView`, not for the reusable primitives themselves.

This matches Avalonia's own guidance: `UserControl` is best for application-specific views, while `TemplatedControl` is best for generic controls shared across apps.

### 3. Use Avalonia 11 Control Themes as the Styling Contract

Do not expose internal child controls or expect hosts to replace XAML trees wholesale.

Instead:

- expose behavior and state through public properties, commands, interfaces, and events
- expose visual customization through `StyledProperty`, `DirectProperty`, template parts, and `ControlTheme`
- keep default visuals in `Themes/Generic.axaml` or package-local theme resource dictionaries

This makes component splitting compatible with host replacement. A host should be able to restyle or re-template a control without forking editor behavior.

### 4. Standardize Diagnostics on .NET Logging + Explicit Snapshot APIs

Do not invent a library-specific logging system.

Recommended diagnostics stack:

- `Microsoft.Extensions.Logging.Abstractions` in `AsterGraph.Editor`
- explicit public snapshot/query types for current editor state
- explicit lifecycle events for document, selection, viewport, command execution, and extension failures
- debugger-friendly attributes on high-value state types where useful

Keep Avalonia developer tooling optional and host-side. `Avalonia.Diagnostics` or Avalonia Developer Tools integration belongs in demo/sample hosts and local debugging flows, not in the core library dependency graph.

## Public API Surfacing Patterns

### Package Responsibilities

| Area | Public Surface Should Live In | Avoid |
|------|-------------------------------|-------|
| Node definitions, identifiers, style tokens | `AsterGraph.Abstractions` | Avalonia types, view models, concrete control classes |
| Document model and serialization | `AsterGraph.Core` | UI events, host context, visual state |
| Commands, queries, subscriptions, diagnostics interfaces | `AsterGraph.Editor` | `Control`, `Brush`, `TopLevel`, `DataTemplate`, or any Avalonia-only types |
| Reusable controls, control themes, input adapters | `AsterGraph.Avalonia` | Business-specific host callbacks hardcoded into control code |

### Exposure Pattern

Use this pattern consistently:

1. Public contract or data type in `AsterGraph.Abstractions` when the type must survive UI replacement.
2. Public orchestration/service interface in `AsterGraph.Editor` when the host needs behavior, queries, or subscriptions.
3. Optional Avalonia adapter in `AsterGraph.Avalonia` when the concern is rendering, input, themes, or default composition.

### Control API Pattern

For each split component:

- prefer a small control-specific public model over the full `GraphEditorViewModel`
- expose state through Avalonia properties and command-like callbacks
- expose extension seams as interfaces or descriptor records, not direct references to internal child controls
- keep default composition in `GraphEditorView` as a convenience wrapper around smaller public controls

Recommended first-class public controls:

- `GraphCanvasControl`
- `GraphMiniMapControl`
- `GraphInspectorControl`
- `GraphContextMenuPresenter`
- `GraphCommandBar`
- `GraphEditorView` as the opinionated full-shell wrapper

## Packaging And Versioning Concerns

### Immediate Changes

The repo already has good basics:

- XML docs are enabled
- symbol packages are enabled with `snupkg`
- package readmes are declared

Before public release, add the missing packaging hardening:

1. Move package versions into `Directory.Packages.props` and enable Central Package Management.
2. Add Source Link to every packable project.
3. Turn on package validation for every packable project.
4. Validate each package against the last stable published version with `PackageValidationBaselineVersion`.
5. Replace `net9.0` with `net10.0` in packable libraries.

### MSBuild Settings To Adopt

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <PackageVersion Include="Avalonia" Version="11.3.12" />
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.4" />
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="10.0.200" />
  </ItemGroup>
</Project>
```

```xml
<PropertyGroup>
  <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <EnablePackageValidation>true</EnablePackageValidation>
  <Deterministic>true</Deterministic>
  <DebugType>portable</DebugType>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
</ItemGroup>
```

For stable releases, set:

```xml
<PropertyGroup>
  <PackageValidationBaselineVersion>PREVIOUS_STABLE_VERSION</PackageValidationBaselineVersion>
</PropertyGroup>
```

### Versioning Strategy

Use SemVer with explicit preview suffixes until the public component split is stable.

Recommended shape:

- `0.x.y-preview.n` while the public API is still moving
- `1.0.0` only after the package boundaries, control model, and diagnostics surface are intentionally stabilized
- minor versions for additive host APIs
- major versions for package-boundary or binary-breaking changes

Do not treat optional parameters on public methods as a safe compatibility move. .NET package validation will catch some source-compatible but binary-breaking changes, and that is exactly what should protect the library line.

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Target frameworks | `net8.0;net10.0` | Keep `net8.0;net9.0` | `net9.0` is current but short-lived; it is not the right long-term published support story. |
| UI split strategy | `TemplatedControl`/`Control` primitives + full-shell wrapper | `UserControl`-heavy split | `UserControl` is easier initially but creates app-shaped APIs that are harder for hosts to restyle and replace. |
| Diagnostics | `ILoggerFactory` + explicit events/snapshots | custom ad hoc debug callbacks only | Standard logging integrates with host ecosystems, Avalonia tools, and test infrastructure. |
| Package management | Central Package Management | Inline versions in every `.csproj` | The library line already spans multiple packable projects; inline versions will drift. |
| Debug tooling dependency | optional host-side Avalonia DevTools | hard dependency on `Avalonia.Diagnostics` in library packages | Debug tooling should stay optional and not bleed into the runtime dependency graph of consumers. |

## Installation

```bash
# Centralize package versions
dotnet new packagesprops

# Runtime/UI dependencies
dotnet add src/AsterGraph.Avalonia package Avalonia --version 11.3.12
dotnet add src/AsterGraph.Editor package CommunityToolkit.Mvvm --version 8.4.0
dotnet add src/AsterGraph.Editor package Microsoft.Extensions.Logging.Abstractions --version 10.0.4

# Build/debugging dependency
dotnet add src/AsterGraph.Avalonia package Microsoft.SourceLink.GitHub --version 10.0.200
```

## Sources

- Avalonia Docs: How to Create and Reference a Custom Control Library — https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-a-custom-controls-library
- Avalonia Docs: Types of Control — https://docs.avaloniaui.net/docs/guides/custom-controls/types-of-control
- Avalonia Docs: How to create templated controls — https://docs.avaloniaui.net/docs/custom-controls/templated-controls
- Avalonia Docs: Defining properties — https://docs.avaloniaui.net/docs/custom-controls/defining-properties
- Avalonia Docs: Control Themes — https://docs.avaloniaui.net/docs/basics/user-interface/styling/control-themes
- Avalonia Docs: Logs tool / Developer Tools logging integration — https://docs.avaloniaui.net/tools/developer-tools/logs-tool
- NuGet Gallery: Avalonia `11.3.12` — https://www.nuget.org/packages/Avalonia
- NuGet Gallery: Avalonia.Desktop `11.3.12` — https://www.nuget.org/packages/Avalonia.Desktop
- NuGet Gallery: CommunityToolkit.Mvvm `8.4.0` — https://www.nuget.org/packages/CommunityToolkit.Mvvm
- NuGet Gallery: Microsoft.Extensions.Logging.Abstractions `10.0.4` — https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions
- NuGet Gallery: Microsoft.SourceLink.GitHub `10.0.200` — https://www.nuget.org/packages/Microsoft.SourceLink.GitHub
- Microsoft Learn: .NET Support Policy — https://dotnet.microsoft.com/en-us/platform/support/policy
- Microsoft Learn: .NET Standard guidance for reusable libraries — https://learn.microsoft.com/en-us/dotnet/standard/net-standard
- Microsoft Learn: Create a NuGet package with the dotnet CLI — https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package-dotnet-cli
- Microsoft Learn: Central Package Management — https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management
- Microsoft Learn: NuGet and .NET libraries — https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/nuget
- Microsoft Learn: Source Link and .NET libraries — https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink
- Microsoft Learn: Logging guidance for .NET library authors — https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/library-guidance
- Microsoft Learn: .NET package validation overview — https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview
- Microsoft Learn: Validate against a baseline package version — https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator
