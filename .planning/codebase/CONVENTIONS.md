# Coding Conventions

**Analysis Date:** 2026-03-25

## Naming Patterns

**Files:**
- Use `PascalCase.cs` for source files, usually matching the primary type in the file, for example `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, and `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Avalonia views use paired markup/code-behind files such as `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Test files use `PascalCaseTests.cs` under `tests/`, for example `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

**Functions:**
- Public and internal methods use `PascalCase` verb phrases, for example `NormalizeValue` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`, `ApplyChromeMode` in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`, and `Serialize` in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Async methods end with `Async`, for example `CopySelectionAsync` and `PasteSelectionAsync` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- CommunityToolkit-generated property hooks use `partial void On{Name}Changed`, for example `OnIsGridSnappingEnabledChanged` in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs` and `OnPresentationChanged` in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.

**Variables:**
- Private fields use `_camelCase`, for example `_nodeCatalog` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `_parameterValues` in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Method parameters and locals use `camelCase`, for example `displayName`, `normalizedValue`, and `parsedNumber` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`.
- Constants use `PascalCase`, not screaming snake case, for example `DefaultZoom` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `DemoSnapTolerance` in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

**Types:**
- Interfaces use an `I` prefix, for example `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`, and `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`.
- Immutable contracts and value objects prefer `sealed record` or `readonly record struct`, for example `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Core/Models/GraphDocument.cs`, and `src/AsterGraph.Editor/Viewport/ViewportState.cs`.
- Mutable MVVM types are usually `sealed partial class` types derived from `ObservableObject`, for example `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

## Code Style

**Formatting:**
- No repo-level formatter config was detected: no `.editorconfig`, `.ruleset`, `stylecop.json`, or analyzer package configuration was found in the root or project files.
- Follow standard modern C# formatting already present in `src/` and `tests/`: 4-space indentation, braces on new lines, and file-scoped namespaces such as `namespace AsterGraph.Editor.ViewModels;` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- Nullable reference types and implicit usings are enabled via `Directory.Build.props`, and repeated in the test projects `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Collection expressions and modern object syntax are used freely, for example `[]`, `[.. inputPorts]`, and `with` expressions in `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs` and `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

**Linting:**
- No dedicated linting tool or analyzer ruleset is committed in this repository.
- The effective quality gate is the compiler plus test coverage from `tests/AsterGraph.Editor.Tests/` and `tests/AsterGraph.Serialization.Tests/`.
- Production projects generate XML docs by default through `Directory.Build.props`; tests explicitly disable doc generation in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.

## Import Organization

**Order:**
1. `System.*` namespaces first, for example `using System.Globalization;` and `using System.Text.Json;` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`.
2. Third-party libraries second, for example `using CommunityToolkit.Mvvm.ComponentModel;`, `using Avalonia.Controls;`, and `using Xunit;` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`, and `tests/AsterGraph.Editor.Tests/NodeParameterValueAdapterTests.cs`.
3. Project namespaces last, using `AsterGraph.*` groups such as `using AsterGraph.Core.Models;` and `using AsterGraph.Editor.Services;`.

**Path Aliases:**
- Not applicable. The codebase uses normal C# namespaces and project references, for example the references in `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`.
- No `global using` files were detected in `src/` or `tests/`.

## Error Handling

**Patterns:**
- Validate public inputs immediately with `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrWhiteSpace`, as seen in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs`, and `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`.
- Throw explicit exceptions for invariant violations instead of silently correcting bad state, for example duplicate port/parameter keys and invalid default dimensions in `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`.
- Use typed result objects or boolean success paths when validation is part of normal workflow, for example `NodeParameterValueNormalizationResult` in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and the export/import methods exercised by `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`.

## Logging

**Framework:** None detected in the library projects under `src/`.

**Patterns:**
- Runtime state is surfaced through view-model properties and events rather than structured logging, for example `StatusMessage` and command state inside `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
- `Console.WriteLine` is used only in tooling and sample host programs such as `tools/AsterGraph.PackageSmoke/Program.cs` and `tools/AsterGraph.HostSample/Program.cs`.

## Comments

**When to Comment:**
- Public or host-facing types usually carry XML documentation comments, often written in Chinese, for example `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Internal code uses short inline comments only where intent is not obvious, for example the serializer stability note in `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs` and the clipboard-bridge boundary comment in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.

**JSDoc/TSDoc:**
- Not applicable. Use C# XML documentation comments (`///`) when documenting API surface.

## Function Design

**Size:** 
- Pure helpers and contract types stay compact, for example `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and `src/AsterGraph.Editor/Services/GraphEditorCommandStateNotifier.cs`.
- Coordination-heavy types are allowed to be large when they own a clear orchestration boundary, most notably `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`.

**Parameters:**
- Prefer explicit parameters over ambient state, and bundle related toggles into option records such as `src/AsterGraph.Editor/Configuration/GraphEditorBehaviorOptions.cs`, `src/AsterGraph.Editor/Configuration/GraphEditorCommandPermissions.cs`, and `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Nullable intent is expressed in signatures, for example optional host seams like `IGraphContextMenuAugmentor?` and `IGraphLocalizationProvider?` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.

**Return Values:**
- Return immutable model records for data transformations, for example `ToModel()` methods in `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs` and `src/AsterGraph.Editor/ViewModels/ConnectionViewModel.cs`.
- Return `Task` for async host interactions and typed records for normalization/validation paths, for example `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs` and clipboard methods in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.

## Module Design

**Exports:**
- Expose types directly from project folders and namespaces; there are no barrel files or re-export modules.
- Internal implementation seams remain `internal` and are opened to tests via `InternalsVisibleTo` in `src/AsterGraph.Editor/Properties/AssemblyInfo.cs`, `src/AsterGraph.Avalonia/Properties/AssemblyInfo.cs`, and `src/AsterGraph.Core/Properties/AssemblyInfo.cs`.

**Barrel Files:**
- Not used. Add new types to the owning project/folder namespace rather than introducing aggregate export files.

---

*Convention analysis: 2026-03-25*
