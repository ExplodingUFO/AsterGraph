# Codebase Structure

**Analysis Date:** 2026-03-25

## Directory Layout

```text
[project-root]/
├── src/                         # Publishable library projects and the demo app
│   ├── AsterGraph.Abstractions/ # Public contracts, identifiers, styling tokens
│   ├── AsterGraph.Core/         # Immutable graph models and serialization
│   ├── AsterGraph.Editor/       # Editor facade, commands, services, projections
│   ├── AsterGraph.Avalonia/     # Avalonia controls, themes, adapters
│   └── AsterGraph.Demo/         # Demo Avalonia application
├── tests/                       # xUnit coverage for editor/Avalonia and serialization
├── tools/                       # Host sample and package smoke utilities
├── docs/                        # Integration guides and design/implementation plans
├── .planning/codebase/          # Generated codebase-map documents
├── avalonia-node-map.sln        # Solution entry point
├── Directory.Build.props        # Shared MSBuild properties across projects
└── README.md                    # Product, package-boundary, and integration overview
```

## Directory Purposes

**`src/AsterGraph.Abstractions`:**
- Purpose: Hold the stable SDK surface that host applications and other library projects consume directly.
- Contains: `Catalog`, `Compatibility`, `Definitions`, `Identifiers`, and `Styling` folders.
- Key files: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`

**`src/AsterGraph.Core`:**
- Purpose: Keep the framework-neutral graph domain and persistence layer separate from editor state and UI.
- Contains: `Models`, `Compatibility`, `Serialization`, and `Properties`.
- Key files: `src/AsterGraph.Core/Models/GraphDocument.cs`, `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`

**`src/AsterGraph.Editor`:**
- Purpose: Own the mutable editor runtime and most host-facing behavior seams.
- Contains: `Catalog`, `Configuration`, `Events`, `Geometry`, `Hosting`, `Localization`, `Menus`, `Models`, `Parameters`, `Presentation`, `Services`, `ViewModels`, and `Viewport`.
- Key files: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs`, `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`

**`src/AsterGraph.Avalonia`:**
- Purpose: Implement the default Avalonia shell and canvas on top of the editor layer.
- Contains: `Controls`, `Hosting`, `Menus`, `Services`, `Styling`, `Themes`, and `Properties`.
- Key files: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`

**`src/AsterGraph.Demo`:**
- Purpose: Provide a runnable product demo and the default composition example inside the solution.
- Contains: `Definitions`, `Menus`, `ViewModels`, `Views`, `Assets`, and Avalonia app bootstrap files.
- Key files: `src/AsterGraph.Demo/Program.cs`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, `src/AsterGraph.Demo/DemoGraphFactory.cs`

**`tests/AsterGraph.Editor.Tests`:**
- Purpose: Cover editor logic plus Avalonia rendering/integration behavior.
- Contains: xUnit tests against `AsterGraph.Editor`, `AsterGraph.Avalonia`, and host extension seams.
- Key files: `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`, `tests/AsterGraph.Editor.Tests/GraphContextMenuPresenterTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`

**`tests/AsterGraph.Serialization.Tests`:**
- Purpose: Keep graph and clipboard serialization compatibility stable.
- Contains: Focused xUnit tests around persistence/versioning behavior.
- Key files: `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`

**`tools/AsterGraph.HostSample`:**
- Purpose: Act as a reference host that consumes the published package boundary.
- Contains: Single-file sample composition demonstrating localization, presentation, host context, and menu augmentation.
- Key files: `tools/AsterGraph.HostSample/Program.cs`

**`tools/AsterGraph.PackageSmoke`:**
- Purpose: Provide a cheap compile/runtime smoke check for package-surface types.
- Contains: A single console program that touches representative public APIs.
- Key files: `tools/AsterGraph.PackageSmoke/Program.cs`

**`docs`:**
- Purpose: Hold user-facing integration guidance plus implementation plans and design notes.
- Contains: Guides at `docs/host-integration.md` and `docs/node-presentation-guidelines.md`, with historical plan documents under `docs/plans/`.
- Key files: `docs/host-integration.md`, `docs/node-presentation-guidelines.md`, `docs/plans/2026-03-23-astergraph-host-extensibility.md`

## Key File Locations

**Entry Points:**
- `avalonia-node-map.sln`: Visual Studio and `dotnet` solution entry point.
- `src/AsterGraph.Demo/Program.cs`: Demo application process entry.
- `src/AsterGraph.Demo/App.axaml.cs`: Demo application composition root.
- `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`: Reusable host control entry for the default UI.
- `tools/AsterGraph.HostSample/Program.cs`: Reference host sample entry.
- `tools/AsterGraph.PackageSmoke/Program.cs`: Package smoke entry.

**Configuration:**
- `Directory.Build.props`: Shared nullable, package metadata, version, and documentation settings.
- `src/AsterGraph.Editor/Configuration/GraphEditorBehaviorOptions.cs`: Runtime behavior toggles.
- `src/AsterGraph.Editor/Configuration/GraphEditorCommandPermissions.cs`: Permission presets and grouped command controls.
- `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`: Framework-neutral visual tokens.

**Core Logic:**
- `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`: Node definition contract shape.
- `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`: Whole-document persistence.
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`: Main orchestration object.
- `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs`: Stock context-menu generation.
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`: Canvas rendering and interaction handling.

**Testing:**
- `tests/AsterGraph.Editor.Tests`: Editor and Avalonia behavior tests.
- `tests/AsterGraph.Serialization.Tests`: Serialization compatibility tests.

## Naming Conventions

**Files:**
- Use one type per file with PascalCase names that match the public type, for example `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Use paired Avalonia view files with `.axaml` and `.axaml.cs`, for example `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`.
- Use project-prefixed test files that describe the subject under test, for example `tests/AsterGraph.Editor.Tests/GraphHostContextExtensionsTests.cs`.

**Directories:**
- Use project folders named `AsterGraph.*` directly under `src/`, `tests/`, and `tools/`.
- Inside each project, group files by concern rather than by technical layer leakage, for example `src/AsterGraph.Editor/Services`, `src/AsterGraph.Editor/Menus`, and `src/AsterGraph.Avalonia/Styling`.

## Where to Add New Code

**New Public Contract:**
- Primary code: `src/AsterGraph.Abstractions`
- Use when adding host-visible identifiers, definition contracts, compatibility interfaces, or style tokens.

**New Domain Model Or Persistence Logic:**
- Primary code: `src/AsterGraph.Core`
- Use `src/AsterGraph.Core/Models` for record shapes and `src/AsterGraph.Core/Serialization` for JSON compatibility work.

**New Editor Behavior Or Host Extension Seam:**
- Primary code: `src/AsterGraph.Editor`
- Put the public/editor-facing entry point in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` or a nearby type, then factor helpers into `Services`, `Menus`, `Hosting`, `Presentation`, `Parameters`, or `Configuration` as appropriate.

**New Avalonia Control Or Visual Adaptation:**
- Implementation: `src/AsterGraph.Avalonia/Controls`, `src/AsterGraph.Avalonia/Styling`, or `src/AsterGraph.Avalonia/Menus`
- Keep Avalonia-specific types out of `src/AsterGraph.Abstractions` and `src/AsterGraph.Core`.

**New Demo-Only Example Behavior:**
- Implementation: `src/AsterGraph.Demo`
- Put demo node definitions under `src/AsterGraph.Demo/Definitions`, demo menu examples under `src/AsterGraph.Demo/Menus`, and demo composition toggles under `src/AsterGraph.Demo/ViewModels`.

**New Host Integration Example:**
- Implementation: `tools/AsterGraph.HostSample`
- Use this path when documenting or proving package-consumption guidance without polluting the demo app.

**Utilities:**
- Shared helpers for editor behavior: `src/AsterGraph.Editor/Services`, `src/AsterGraph.Editor/Geometry`, `src/AsterGraph.Editor/Viewport`
- Shared helpers for Avalonia rendering: `src/AsterGraph.Avalonia/Styling`, `src/AsterGraph.Avalonia/Services`, `src/AsterGraph.Avalonia/Controls/Internal`

**Tests:**
- Editor/Avalonia behavior tests: `tests/AsterGraph.Editor.Tests`
- Serialization and compatibility tests: `tests/AsterGraph.Serialization.Tests`

## Special Directories

**`src/*/bin` and `src/*/obj`:**
- Purpose: Standard .NET build outputs.
- Generated: Yes
- Committed: No, ignored by `.gitignore`

**`src/AsterGraph.Abstractions/artifacts`, `src/AsterGraph.Core/artifacts`, `src/AsterGraph.Editor/artifacts`:**
- Purpose: Audit/build-output snapshots currently present inside the source tree.
- Generated: Yes
- Committed: Yes

**`.planning/codebase`:**
- Purpose: Generated repository-mapping documents consumed by GSD planning/execution workflows.
- Generated: Yes
- Committed: Yes

**`docs/plans`:**
- Purpose: Design notes and implementation plans that explain why current seams exist.
- Generated: No
- Committed: Yes

---

*Structure analysis: 2026-03-25*
