<!-- GSD:project-start source:PROJECT.md -->
## Project

**AsterGraph**

AsterGraph is a modular node-graph editor toolkit for .NET with a reusable editor state layer and an Avalonia UI shell. This project is now focused on turning that foundation into a more decoupled, host-friendly component library that external consumers can embed, replace, extend, and debug without forking the default implementation.

**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

### Constraints

- **Tech stack**: Keep the solution centered on .NET, C#, and Avalonia — the existing packages and host story already depend on that stack
- **Compatibility strategy**: API reorganization is allowed, but it should be phased and deliberate rather than a single uncontrolled break — the user accepts planned migration
- **Product positioning**: The result must be publishable as a general-purpose component library, not just a private host refactor — public API quality matters
- **Architecture**: Existing validated editing capabilities must remain available during the transition — the project is a refactor and SDK hardening effort, not a rewrite
- **Extensibility**: Hosts should be able to replace or embed subcomponents independently — package and API design must support partial adoption
- **Observability**: Debuggability is now part of the product requirement, not a local developer convenience — diagnostics need explicit public seams
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# - Main implementation language across the solution in `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tests/AsterGraph.Editor.Tests`, `tests/AsterGraph.Serialization.Tests`, `tools/AsterGraph.HostSample`, and `tools/AsterGraph.PackageSmoke`.
- Avalonia XAML (`.axaml`) - Desktop UI markup for the Avalonia shell in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml`, `src/AsterGraph.Demo/App.axaml`, and `src/AsterGraph.Demo/Views/MainWindow.axaml`.
- MSBuild XML - Build, packaging, and solution metadata in `Directory.Build.props`, `avalonia-node-map.sln`, `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `src/AsterGraph.Core/AsterGraph.Core.csproj`, `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, `src/AsterGraph.Demo/AsterGraph.Demo.csproj`, `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`, `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`, `tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`, `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`, and `NuGet.config.sample`.
## Runtime
- .NET SDK `10.0.201` detected locally via `dotnet --version`.
- Target frameworks are `net8.0` and `net9.0` for the packable libraries in `src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj`, `src/AsterGraph.Core/AsterGraph.Core.csproj`, `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, and `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`.
- The demo desktop application targets `net9.0` in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- Test projects target `net8.0` in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- NuGet via the .NET CLI and MSBuild project restore flow, as implied by all `*.csproj` files and `NuGet.config.sample`.
- Lockfile: missing. `packages.lock.json`, `Directory.Packages.props`, and `global.json` are not detected in the repository root.
## Frameworks
- Microsoft.NET.Sdk - Base SDK for every project file under `src/`, `tests/`, and `tools/`.
- Avalonia `11.3.10` - Desktop UI framework used by the shell package in `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj` and by the demo app in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- CommunityToolkit.Mvvm `8.2.1` - MVVM support used by `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- System.Text.Json - Built-in JSON serialization stack used in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentCompatibility.cs`, `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`, and `src/AsterGraph.Editor/Services/GraphClipboardPayloadCompatibility.cs`.
- xUnit `2.9.2` - Unit test framework in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Microsoft.NET.Test.Sdk `17.11.1` - Test runner integration in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Avalonia.Headless.XUnit `11.3.10` - Headless Avalonia UI testing in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- `dotnet` CLI - Build, run, test, and pack flows documented in `README.md`.
- Visual Studio solution metadata - Multi-project orchestration in `avalonia-node-map.sln`.
- Shared MSBuild packaging settings - Versioning, symbols, docs, CI build flag, and NuGet metadata in `Directory.Build.props`.
## Key Dependencies
- `Avalonia` `11.3.10` - Required for the reusable UI shell package in `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`.
- `Avalonia.Desktop` `11.3.10` - Required for the runnable desktop host in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Avalonia.Themes.Fluent` `11.3.10` - Fluent theme resources for the demo host in `src/AsterGraph.Demo/AsterGraph.Demo.csproj` and headless UI tests in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- `Avalonia.Fonts.Inter` `11.3.10` - Bundled font dependency for the demo app in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `CommunityToolkit.Mvvm` `8.2.1` - View-model and command support for editor and host layers in `src/AsterGraph.Editor/AsterGraph.Editor.csproj`, `src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj`, and `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `Avalonia.Diagnostics` `11.3.10` - Debug-only diagnostics support in `src/AsterGraph.Demo/AsterGraph.Demo.csproj`.
- `xunit.runner.visualstudio` `2.8.2` - IDE and test runner adapter in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Local AsterGraph package references - Smoke-test package consumption through `tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`.
## Configuration
- Shared package metadata, version, symbol settings, nullable context, implicit usings, and optional CI build flag are configured in `Directory.Build.props`.
- No `.env` files are detected at the repository root.
- No runtime secrets or required environment variables are defined in tracked files.
- Optional `CI=true` enables `ContinuousIntegrationBuild` in `Directory.Build.props`.
- Solution entry point: `avalonia-node-map.sln`.
- Shared build metadata: `Directory.Build.props`.
- Project-specific build and package definitions: every `*.csproj` under `src/`, `tests/`, and `tools/`.
- Local NuGet feed sample: `NuGet.config.sample`.
- Windows desktop manifest for the demo host: `src/AsterGraph.Demo/app.manifest`.
## Platform Requirements
- Use a .NET SDK that can restore and build both `net8.0` and `net9.0` projects. The current workspace has SDK `10.0.201` installed.
- Use the `dotnet` CLI for the documented flows in `README.md`: `dotnet build`, `dotnet run`, and `dotnet pack`.
- Use Avalonia-compatible desktop tooling when working on `src/AsterGraph.Avalonia` and `src/AsterGraph.Demo`.
- Primary deliverables are NuGet packages from `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, `src/AsterGraph.Editor`, and `src/AsterGraph.Avalonia`, packed to `artifacts/packages` as documented in `README.md`.
- The runnable application target in the repo is the Avalonia desktop demo in `src/AsterGraph.Demo`.
- No server runtime, container target, or cloud deployment target is defined in tracked configuration files.
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

## Naming Patterns
- Use `PascalCase.cs` for source files, usually matching the primary type in the file, for example `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, and `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Avalonia views use paired markup/code-behind files such as `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Test files use `PascalCaseTests.cs` under `tests/`, for example `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.
- Public and internal methods use `PascalCase` verb phrases, for example `NormalizeValue` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`, `ApplyChromeMode` in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`, and `Serialize` in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Async methods end with `Async`, for example `CopySelectionAsync` and `PasteSelectionAsync` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- CommunityToolkit-generated property hooks use `partial void On{Name}Changed`, for example `OnIsGridSnappingEnabledChanged` in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` and `OnPresentationChanged` in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Private fields use `_camelCase`, for example `_nodeCatalog` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `_parameterValues` in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Method parameters and locals use `camelCase`, for example `displayName`, `normalizedValue`, and `parsedNumber` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`.
- Constants use `PascalCase`, not screaming snake case, for example `DefaultZoom` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `DemoSnapTolerance` in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.
- Interfaces use an `I` prefix, for example `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`, and `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`.
- Immutable contracts and value objects prefer `sealed record` or `readonly record struct`, for example `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Core/Models/GraphDocument.cs`, and `src/AsterGraph.Editor/Viewport/ViewportState.cs`.
- Mutable MVVM types are usually `sealed partial class` types derived from `ObservableObject`, for example `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.
## Code Style
- No repo-level formatter config was detected: no `.editorconfig`, `.ruleset`, `stylecop.json`, or analyzer package configuration was found in the root or project files.
- Follow standard modern C# formatting already present in `src/` and `tests/`: 4-space indentation, braces on new lines, and file-scoped namespaces such as `namespace AsterGraph.Editor.ViewModels;` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- Nullable reference types and implicit usings are enabled via `Directory.Build.props`, and repeated in the test projects `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Collection expressions and modern object syntax are used freely, for example `[]`, `[.. inputPorts]`, and `with` expressions in `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs` and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.
- No dedicated linting tool or analyzer ruleset is committed in this repository.
- The effective quality gate is the compiler plus test coverage from `tests/AsterGraph.Editor.Tests/` and `tests/AsterGraph.Serialization.Tests/`.
- Production projects generate XML docs by default through `Directory.Build.props`; tests explicitly disable doc generation in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
## Import Organization
- Not applicable. The codebase uses normal C# namespaces and project references, for example the references in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- No `global using` files were detected in `src/` or `tests/`.
## Error Handling
- Validate public inputs immediately with `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrWhiteSpace`, as seen in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs`, and `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`.
- Throw explicit exceptions for invariant violations instead of silently correcting bad state, for example duplicate port/parameter keys and invalid default dimensions in `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`.
- Use typed result objects or boolean success paths when validation is part of normal workflow, for example `NodeParameterValueNormalizationResult` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and the export/import methods exercised by `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`.
## Logging
- Runtime state is surfaced through view-model properties and events rather than structured logging, for example `StatusMessage` and command state inside `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- `Console.WriteLine` is used only in tooling and sample host programs such as `tools/AsterGraph.PackageSmoke/Program.cs` and `tools/AsterGraph.HostSample/Program.cs`.
## Comments
- Public or host-facing types usually carry XML documentation comments, often written in Chinese, for example `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Internal code uses short inline comments only where intent is not obvious, for example the serializer stability note in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs` and the clipboard-bridge boundary comment in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Not applicable. Use C# XML documentation comments (`///`) when documenting API surface.
## Function Design
- Pure helpers and contract types stay compact, for example `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs`.
- Coordination-heavy types are allowed to be large when they own a clear orchestration boundary, most notably `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`.
- Prefer explicit parameters over ambient state, and bundle related toggles into option records such as `src/AsterGraph.Editor/Configuration/GraphEditorBehaviorOptions.cs`, `src/AsterGraph.Editor/Configuration/GraphEditorCommandPermissions.cs`, and `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Nullable intent is expressed in signatures, for example optional host seams like `IGraphContextMenuAugmentor?` and `IGraphLocalizationProvider?` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- Return immutable model records for data transformations, for example `ToModel()` methods in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs` and `src/AsterGraph.Editor/ViewModels/ConnectionViewModel.cs`.
- Return `Task` for async host interactions and typed records for normalization/validation paths, for example `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and clipboard methods in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
## Module Design
- Expose types directly from project folders and namespaces; there are no barrel files or re-export modules.
- Internal implementation seams remain `internal` and are opened to tests via `InternalsVisibleTo` in `src/AsterGraph.Editor/Properties/AssemblyInfo.cs`, `src/AsterGraph.Avalonia/Properties/AssemblyInfo.cs`, and `src/AsterGraph.Core/Properties/AssemblyInfo.cs`.
- Not used. Add new types to the owning project/folder namespace rather than introducing aggregate export files.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## Pattern Overview
- Keep contracts and stable identifiers in `src/AsterGraph.Abstractions` so host packages can depend on extension seams without pulling in UI code.
- Keep immutable graph records and serialization in `src/AsterGraph.Core`, then project them into mutable editor state in `src/AsterGraph.Editor`.
- Route nearly all editor behavior through `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, then let `src/AsterGraph.Avalonia` render that state and forward user input back into it.
## Layers
- Purpose: Define the public SDK contracts for node definitions, identifiers, compatibility, catalogs, and style tokens.
- Location: `src/AsterGraph.Abstractions`
- Contains: `Catalog`, `Compatibility`, `Definitions`, `Identifiers`, and `Styling` types such as `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Abstractions/Compatibility/IPortCompatibilityService.cs`, and `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Depends on: BCL only.
- Used by: `src/AsterGraph.Core`, `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, and both test projects.
- Purpose: Model graph documents and own stable JSON persistence plus the default port-compatibility policy.
- Location: `src/AsterGraph.Core`
- Contains: Immutable records in `src/AsterGraph.Core/Models/*.cs`, compatibility policy in `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`, and serializer/compatibility helpers in `src/AsterGraph.Core/Serialization/*.cs`.
- Depends on: `src/AsterGraph.Abstractions`
- Used by: `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, and tests.
- Purpose: Hold mutable editor state, commands, selection, history, menus, workspace/file services, inspector projection, and host extension seams.
- Location: `src/AsterGraph.Editor`
- Contains: The main facade `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, mutable projections such as `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, configuration records in `src/AsterGraph.Editor/Configuration/*.cs`, menu descriptors/builders in `src/AsterGraph.Editor/Menus/*.cs`, and services in `src/AsterGraph.Editor/Services/*.cs`.
- Depends on: `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, and `CommunityToolkit.Mvvm`.
- Used by: `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, `tools/AsterGraph.PackageSmoke`, and tests.
- Purpose: Adapt editor state into Avalonia controls, resources, clipboard integration, host context propagation, and pointer/keyboard interaction.
- Location: `src/AsterGraph.Avalonia`
- Contains: Shell view in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`, canvas interaction in `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, context menu rendering in `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`, style adaptation in `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`, and theme resources in `src/AsterGraph.Avalonia/Themes/AsterGraphTheme.axaml`.
- Depends on: `src/AsterGraph.Core`, `src/AsterGraph.Editor`, Avalonia, and `CommunityToolkit.Mvvm`.
- Used by: `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, `tools/AsterGraph.PackageSmoke`, and UI-focused tests.
- Purpose: Compose catalogs, documents, style/behavior options, and optional host extensions into runnable applications.
- Location: `src/AsterGraph.Demo` and `tools/AsterGraph.HostSample`
- Contains: Avalonia app bootstrap in `src/AsterGraph.Demo/Program.cs` and `src/AsterGraph.Demo/App.axaml.cs`, demo composition in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, sample host API usage in `tools/AsterGraph.HostSample/Program.cs`, and demo-only node/menu content in `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs` and `src/AsterGraph.Demo/Menus/DemoNodeResultsMenuContributor.cs`.
- Depends on: All publishable library projects as needed by the host.
- Used by: Developers validating the package boundary and integration story.
## Data Flow
- Treat `src/AsterGraph.Core/Models/*.cs` as immutable persistence/domain snapshots.
- Treat `src/AsterGraph.Editor/ViewModels/*.cs` as the mutable runtime state used by the UI.
- Use `ObservableCollection` and `ObservableObject` change notifications inside `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Keep undo/redo snapshots in `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`.
- Keep visual-only shell toggles in `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`; do not move them into editor behavior options.
## Key Abstractions
- Purpose: Register compile-time node definitions and resolve them by stable IDs.
- Examples: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Editor/Catalog/NodeCatalog.cs`, `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs`
- Pattern: Host-owned providers feed definitions into an editor-owned catalog.
- Purpose: Concentrate editing commands, events, selection, workspace IO, viewport control, and integration seams in one public entry point.
- Examples: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Pattern: Facade/service-object hybrid; the view binds to one object instead of coordinating many services directly.
- Purpose: Keep persisted graph contracts simple while allowing interactive mutable state in the editor.
- Examples: `src/AsterGraph.Core/Models/GraphNode.cs`, `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, `src/AsterGraph.Editor/ViewModels/ConnectionViewModel.cs`
- Pattern: Convert immutable model records into mutable UI projections, then rebuild snapshots when saving or raising history states.
- Purpose: Let hosts add runtime behavior without forking the editor or Avalonia layers.
- Examples: `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`, `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`, `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`, `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`
- Pattern: Host passes interface implementations into `GraphEditorViewModel`; Avalonia only supplies adapters such as `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs`.
- Purpose: Keep public styling and menu APIs framework-neutral while the Avalonia package renders them.
- Examples: `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`, `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`
- Pattern: Abstractions and Editor emit data-only tokens/descriptors; Avalonia translates them into brushes, layout resources, and controls.
## Entry Points
- Location: `src/AsterGraph.Demo/Program.cs`
- Triggers: `dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj`
- Responsibilities: Configure and start the Avalonia desktop lifetime.
- Location: `src/AsterGraph.Demo/App.axaml.cs`
- Triggers: Avalonia app startup.
- Responsibilities: Create `MainWindow`, assign `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, and disable duplicate validation plugins.
- Location: `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- Triggers: Main window construction.
- Responsibilities: Register demo definitions, create the default document, configure style/behavior options, instantiate `GraphEditorViewModel`, and toggle host permissions.
- Location: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- Triggers: Any host embedding the control.
- Responsibilities: Bind the shell layout, wire keyboard shortcuts, adapt style resources, and connect Avalonia clipboard/host context services to the editor facade.
- Location: `tools/AsterGraph.HostSample/Program.cs`
- Triggers: `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- Responsibilities: Demonstrate package-consumption boundaries, host context, localization, presentation provider, menu augmentation, and `GraphEditorViewChromeMode`.
## Error Handling
- Wrap host extension execution in `try/catch` and fall back to stock behavior in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` when `IGraphContextMenuAugmentor` throws.
- Convert save/load failures into editor status messages in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` instead of crashing the host.
- Reject invalid graph operations through guards and status updates inside `GraphEditorViewModel` such as permission checks, direction checks, and compatibility checks.
- Let leaf services throw on invalid arguments or invalid persisted data, for example `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs` and `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
## Cross-Cutting Concerns
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
