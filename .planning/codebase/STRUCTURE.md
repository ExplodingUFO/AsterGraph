# Codebase Structure

**Analysis Date:** 2026-04-14

## Repository Layout

```text
[project-root]/
|- src/                 # publishable libraries plus demo app
|- tests/               # xUnit regression projects
|- tools/               # proof tools and package-consumption utilities
|- eng/                # scripted validation entrypoint for build/test lanes
|- docs/                # host-facing guides
|- .planning/           # GSD planning, roadmap, and codebase map artifacts
|- .github/             # checked-in workflow definitions
|  `- workflows/        # CI configuration and branch validation
|- artifacts/           # local pack outputs
|- .worktrees/          # local worktree storage
|- Directory.Build.props
|- Directory.Packages.props
|- NuGet.config
|- NuGet.config.sample
|- avalonia-node-map.sln # canonical solution entrypoint
|- avalonia-node-map.slnx  # placeholder, not maintained as canonical entrypoint
|- eng/ci.ps1           # script-first CI/build/test orchestration
|- .github/workflows/ci.yml # checked-in CI workflow invoking `eng/ci.ps1`
|- README.md
|- AGENTS.md
`- CLAUDE.md
```

## Source Projects

### `src/AsterGraph.Abstractions`

- Purpose: stable host-facing contracts and tokens.
- Main folders: `Catalog`, `Compatibility`, `Definitions`, `Identifiers`, `Styling`.
- Anchor files: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Abstractions/Definitions/NodeDefinition.cs`, `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.

### `src/AsterGraph.Core`

- Purpose: immutable graph models and serialization/compatibility logic.
- Main folders: `Compatibility`, `Models`, `Serialization`, `Properties`.
- Anchor files: `src/AsterGraph.Core/Models/GraphDocument.cs`, `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`, `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.

### `src/AsterGraph.Editor`

- Purpose: editor runtime, compatibility facade, services, and host seams.
- Main folders: `Catalog`, `Configuration`, `Diagnostics`, `Events`, `Geometry`, `Hosting`, `Kernel`, `Localization`, `Menus`, `Models`, `Parameters`, `Presentation`, `Runtime`, `Services`, `ViewModels`, `Viewport`.
- Anchor files: `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`, `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`, `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`, `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.

### `src/AsterGraph.Avalonia`

- Purpose: shipped Avalonia controls, themes, and adapter factories.
- Main folders: `Controls`, `Hosting`, `Menus`, `Presentation`, `Services`, `Styling`, `Themes`, `Properties`.
- Anchor files: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml`, `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs`, `src/AsterGraph.Avalonia/Services/AvaloniaTextClipboardBridge.cs`.

### `src/AsterGraph.Demo`

- Purpose: runnable demo host, not part of the supported SDK boundary.
- Main folders: `Definitions`, `Menus`, `ViewModels`, `Views`, plus app bootstrap files.
- Anchor files: `src/AsterGraph.Demo/Program.cs`, `src/AsterGraph.Demo/App.axaml.cs`, `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`.

## Tests

### `tests/AsterGraph.Editor.Tests`

- Purpose: runtime, Avalonia, diagnostics, migration, proof-ring, and surface-composition coverage.
- Notable suites: `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorDiagnosticsInspectionTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorSurfaceCompositionTests.cs`.

### `tests/AsterGraph.Serialization.Tests`

- Purpose: graph-document and clipboard compatibility regression tests.
- Anchor file: `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

### `tests/AsterGraph.Demo.Tests`

- Purpose: demo and host shell proof lanes for sample-first composition and sample integration behavior.
- Notable suites: `tests/AsterGraph.Demo.Tests/DemoMainWindowTests.cs`, `tests/AsterGraph.Demo.Tests/DemoHostMenuControlTests.cs`, `tests/AsterGraph.Demo.Tests/GraphEditorDemoShellTests.cs`, `tests/AsterGraph.Demo.Tests/GraphEditorLocalizationDemoTests.cs`.

## Tools And Proofs

### `tools/AsterGraph.PackageSmoke`

- Purpose: validate public package boundaries and packed-package consumption.
- Anchor file: `tools/AsterGraph.PackageSmoke/Program.cs`.

### `tools/AsterGraph.ScaleSmoke`

- Purpose: emit repeatable `SCALE_*` markers for large-graph and session-state proof flows.
- Anchor file: `tools/AsterGraph.ScaleSmoke/Program.cs`.

## Documentation And Planning

- Public integration docs live in `docs/host-integration.md`, `docs/quick-start.md`, `docs/interactions-and-shortcuts.md`, and `docs/node-presentation-guidelines.md`.
- GSD state and planning live under `.planning/PROJECT.md`, `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md`, `.planning/phases/`, `.planning/research/`, and `.planning/codebase/`.
- `.planning/STATE.md` currently tracks the phase posture as **Phase 28 complete** with **Phase 29 next**.
- `.planning/ROADMAP.md` currently tracks the active v1.5 milestone and the phase sequence around this transition.

## Key File Locations

### Composition Roots

- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphAvaloniaViewFactory.cs`
- `src/AsterGraph.Demo/Program.cs`
- `tools/AsterGraph.PackageSmoke/Program.cs`
- `tools/AsterGraph.ScaleSmoke/Program.cs`

### Runtime Hotspots

- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Editor/Kernel/GraphEditorKernel.cs`
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`

### Storage And Serialization

- `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`
- `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs`
- `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`
- `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`

### Diagnostics

- `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs`
- `src/AsterGraph.Editor/Diagnostics/GraphEditorInstrumentationOptions.cs`
- `src/AsterGraph.Editor/Diagnostics/IGraphEditorDiagnosticsSink.cs`

## Naming And Placement Patterns

- Project folders under `src/`, `tests/`, and `tools/` follow the `AsterGraph.*` naming pattern.
- Most C# files use one primary type per `PascalCase.cs` file.
- Avalonia views are paired as `.axaml` plus `.axaml.cs`.
- Tests are grouped by behavior/surface rather than mirrored one-to-one with production folders.
- New public contracts generally belong in `src/AsterGraph.Abstractions`; new runtime logic belongs in `src/AsterGraph.Core` or `src/AsterGraph.Editor`; new UI adapters belong in `src/AsterGraph.Avalonia`.

## Special Directories

- `artifacts/` contains local package outputs and should stay out of source compilation.
- `.worktrees/` holds local worktree state for parallel development.
- `.planning/codebase/` is generated reference material and is committed.
- `.vs/` is local IDE state and not part of the product surface.

---

*Structure analysis refreshed: 2026-04-14*
