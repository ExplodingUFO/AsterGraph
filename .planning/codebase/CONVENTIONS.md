# Coding Conventions

**Analysis Date:** 2026-04-08

## Naming Patterns

- Source files use `PascalCase.cs`, usually matching the primary type, for example `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
- Avalonia views use paired `.axaml` and `.axaml.cs` files such as `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Test files follow `PascalCaseTests.cs`, for example `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.
- Public/internal methods use `PascalCase`; async methods end in `Async`.
- Private fields use `_camelCase`; locals and parameters use `camelCase`.
- Interfaces use the `I` prefix, for example `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` and `src/AsterGraph.Editor/Services/IGraphWorkspaceService.cs`.
- Immutable value objects usually use `sealed record` or `readonly record struct`; mutable MVVM types are commonly `sealed partial class` types derived from `ObservableObject`.

## Code Style

- No repo-level `.editorconfig`, ruleset, or analyzer config is tracked.
- The prevailing C# style is modern SDK-style C# with 4-space indentation, braces on new lines, and file-scoped namespaces.
- Nullable reference types and implicit usings are enabled through `Directory.Build.props`.
- Collection expressions, `with` expressions, and other modern C# constructs are used freely in `src/` and `tests/`.
- Production projects generate XML docs by default; tests disable doc generation in their `*.csproj` files.
- The repo currently suppresses `CS1591` in `Directory.Build.props` while public XML docs continue to be added.

## Import And Module Organization

- Imports follow normal C# ordering: `System.*`, then third-party namespaces, then `AsterGraph.*`.
- No `global using` files were detected.
- Types are exposed directly from their project namespaces; the repo does not use barrel files or re-export modules.
- Internal implementation seams are opened to tests using `InternalsVisibleTo` in project `AssemblyInfo.cs` files.

## Error Handling

- Public APIs validate arguments early with `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrWhiteSpace`.
- Invalid persisted data and invariant violations are rejected explicitly rather than silently normalized.
- Normal validation paths often return typed results or booleans, for example parameter normalization and compatibility-query helpers.
- Recoverable runtime faults are surfaced through diagnostics and status updates instead of only through thrown exceptions.

## Logging And Diagnostics Style

- Historically the editor surface leaned on properties/events such as `StatusMessage`.
- Current host-facing diagnostics live in `src/AsterGraph.Editor/Diagnostics/*.cs`.
- Optional structured logging/tracing is host-supplied through `GraphEditorInstrumentationOptions` rather than hard-wired into the library.
- Tooling and proof programs still use `Console.WriteLine` heavily, especially in `tools/AsterGraph.HostSample/Program.cs`, `tools/AsterGraph.PackageSmoke/Program.cs`, and `tools/AsterGraph.ScaleSmoke/Program.cs`.

## Documentation And Comments

- Public and host-facing APIs commonly carry XML documentation comments, often written in Chinese.
- Inline comments are usually short and reserved for non-obvious compatibility or adapter behavior.
- README and docs files act as part of the public contract story, especially around migration and supported composition paths.

## Function And Type Design

- Pure helpers and small contract types stay compact.
- Larger coordination types are tolerated when they own a clear orchestration boundary, notably `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`, and `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`.
- Related options are bundled into records such as `src/AsterGraph.Editor/Configuration/GraphEditorBehaviorOptions.cs` and `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Nullable intent is expressed explicitly in public signatures for optional host seams.
- The repo is in a staged migration period, so some APIs intentionally carry `[Obsolete(...)]` compatibility members instead of removing them immediately, for example in `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`, `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`, and `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`.

## Test Conventions

- Tests prefer real objects plus handwritten doubles over mocking frameworks.
- Helper factories usually live inside the test file that uses them.
- Headless Avalonia tests live beside runtime tests in `tests/AsterGraph.Editor.Tests`.
- Proof-oriented suites verify migration parity, diagnostics continuity, and package-surface stability in addition to classic unit behavior.

---

*Convention analysis refreshed: 2026-04-08*
