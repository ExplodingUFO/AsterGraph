# Architecture

**Analysis Date:** 2026-03-25

## Pattern Overview

**Overall:** Layered multi-project .NET SDK with a host-facing editor facade and an Avalonia presentation shell.

**Key Characteristics:**
- Keep contracts and stable identifiers in `src/AsterGraph.Abstractions` so host packages can depend on extension seams without pulling in UI code.
- Keep immutable graph records and serialization in `src/AsterGraph.Core`, then project them into mutable editor state in `src/AsterGraph.Editor`.
- Route nearly all editor behavior through `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, then let `src/AsterGraph.Avalonia` render that state and forward user input back into it.

## Layers

**Contract Layer:**
- Purpose: Define the public SDK contracts for node definitions, identifiers, compatibility, catalogs, and style tokens.
- Location: `src/AsterGraph.Abstractions`
- Contains: `Catalog`, `Compatibility`, `Definitions`, `Identifiers`, and `Styling` types such as `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Abstractions/Compatibility/IPortCompatibilityService.cs`, and `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`.
- Depends on: BCL only.
- Used by: `src/AsterGraph.Core`, `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, and both test projects.

**Domain Layer:**
- Purpose: Model graph documents and own stable JSON persistence plus the default port-compatibility policy.
- Location: `src/AsterGraph.Core`
- Contains: Immutable records in `src/AsterGraph.Core/Models/*.cs`, compatibility policy in `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`, and serializer/compatibility helpers in `src/AsterGraph.Core/Serialization/*.cs`.
- Depends on: `src/AsterGraph.Abstractions`
- Used by: `src/AsterGraph.Editor`, `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, and tests.

**Editor Orchestration Layer:**
- Purpose: Hold mutable editor state, commands, selection, history, menus, workspace/file services, inspector projection, and host extension seams.
- Location: `src/AsterGraph.Editor`
- Contains: The main facade `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, mutable projections such as `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, configuration records in `src/AsterGraph.Editor/Configuration/*.cs`, menu descriptors/builders in `src/AsterGraph.Editor/Menus/*.cs`, and services in `src/AsterGraph.Editor/Services/*.cs`.
- Depends on: `src/AsterGraph.Abstractions`, `src/AsterGraph.Core`, and `CommunityToolkit.Mvvm`.
- Used by: `src/AsterGraph.Avalonia`, `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, `tools/AsterGraph.PackageSmoke`, and tests.

**Avalonia Presentation Layer:**
- Purpose: Adapt editor state into Avalonia controls, resources, clipboard integration, host context propagation, and pointer/keyboard interaction.
- Location: `src/AsterGraph.Avalonia`
- Contains: Shell view in `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`, canvas interaction in `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`, context menu rendering in `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`, style adaptation in `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`, and theme resources in `src/AsterGraph.Avalonia/Themes/AsterGraphTheme.axaml`.
- Depends on: `src/AsterGraph.Core`, `src/AsterGraph.Editor`, Avalonia, and `CommunityToolkit.Mvvm`.
- Used by: `src/AsterGraph.Demo`, `tools/AsterGraph.HostSample`, `tools/AsterGraph.PackageSmoke`, and UI-focused tests.

**Host Layer:**
- Purpose: Compose catalogs, documents, style/behavior options, and optional host extensions into runnable applications.
- Location: `src/AsterGraph.Demo` and `tools/AsterGraph.HostSample`
- Contains: Avalonia app bootstrap in `src/AsterGraph.Demo/Program.cs` and `src/AsterGraph.Demo/App.axaml.cs`, demo composition in `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, sample host API usage in `tools/AsterGraph.HostSample/Program.cs`, and demo-only node/menu content in `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs` and `src/AsterGraph.Demo/Menus/DemoNodeResultsMenuContributor.cs`.
- Depends on: All publishable library projects as needed by the host.
- Used by: Developers validating the package boundary and integration story.

## Data Flow

**Host Composition Flow:**

1. A host registers node definitions through `src/AsterGraph.Editor/Catalog/NodeCatalog.cs` and creates a `GraphDocument` from `src/AsterGraph.Core/Models/GraphDocument.cs`.
2. The host constructs `GraphEditorViewModel` from `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`, optionally passing style options, behavior options, localization, menu augmentation, and node-presentation providers.
3. `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs` binds the editor facade to Avalonia controls and injects an Avalonia clipboard bridge plus `IGraphHostContext`.
4. `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs` renders nodes/connections and forwards pointer, wheel, keyboard, and context-menu interactions back into `GraphEditorViewModel`.
5. `GraphEditorViewModel` mutates `NodeViewModel` and `ConnectionViewModel`, runs compatibility checks, updates history, raises editor events, and exposes computed captions/commands for the view.

**Persistence And Fragment Flow:**

1. `GraphEditorViewModel.CreateDocumentSnapshot()` rebuilds immutable `GraphDocument` state from live view models in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`.
2. Whole-document save/load goes through `src/AsterGraph.Editor/Services/GraphWorkspaceService.cs`, which delegates JSON serialization to `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.
3. Selection copy/export uses `src/AsterGraph.Editor/Services/GraphSelectionClipboard.cs` and `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`.
4. Fragment workspace and fragment library persistence go through `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs` and `src/AsterGraph.Editor/Services/GraphFragmentLibraryService.cs`.

**Context Menu Flow:**

1. `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasContextMenuContextFactory.cs` converts the current click target plus selection state into a framework-neutral `ContextMenuContext`.
2. `GraphEditorViewModel.BuildContextMenu(...)` in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` calls `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs` to create stock `MenuItemDescriptor` values.
3. If present, `IGraphContextMenuAugmentor` from `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs` appends host-specific items.
4. `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs` translates descriptors into Avalonia `ContextMenu` and `MenuItem` controls.

**State Management:**
- Treat `src/AsterGraph.Core/Models/*.cs` as immutable persistence/domain snapshots.
- Treat `src/AsterGraph.Editor/ViewModels/*.cs` as the mutable runtime state used by the UI.
- Use `ObservableCollection` and `ObservableObject` change notifications inside `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` and `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`.
- Keep undo/redo snapshots in `src/AsterGraph.Editor/Services/GraphEditorHistoryService.cs`.
- Keep visual-only shell toggles in `src/AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`; do not move them into editor behavior options.

## Key Abstractions

**Node Definition Catalog:**
- Purpose: Register compile-time node definitions and resolve them by stable IDs.
- Examples: `src/AsterGraph.Abstractions/Catalog/INodeCatalog.cs`, `src/AsterGraph.Editor/Catalog/NodeCatalog.cs`, `src/AsterGraph.Demo/Definitions/DemoNodeDefinitionProvider.cs`
- Pattern: Host-owned providers feed definitions into an editor-owned catalog.

**Graph Editor Facade:**
- Purpose: Concentrate editing commands, events, selection, workspace IO, viewport control, and integration seams in one public entry point.
- Examples: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Pattern: Facade/service-object hybrid; the view binds to one object instead of coordinating many services directly.

**View-Model Projection Over Immutable Models:**
- Purpose: Keep persisted graph contracts simple while allowing interactive mutable state in the editor.
- Examples: `src/AsterGraph.Core/Models/GraphNode.cs`, `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`, `src/AsterGraph.Editor/ViewModels/ConnectionViewModel.cs`
- Pattern: Convert immutable model records into mutable UI projections, then rebuild snapshots when saving or raising history states.

**Framework-Neutral Host Extension Seams:**
- Purpose: Let hosts add runtime behavior without forking the editor or Avalonia layers.
- Examples: `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`, `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`, `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`, `src/AsterGraph.Editor/Menus/IGraphContextMenuAugmentor.cs`
- Pattern: Host passes interface implementations into `GraphEditorViewModel`; Avalonia only supplies adapters such as `src/AsterGraph.Avalonia/Hosting/AvaloniaGraphHostContext.cs`.

**Style And Menu Descriptor Adaptation:**
- Purpose: Keep public styling and menu APIs framework-neutral while the Avalonia package renders them.
- Examples: `src/AsterGraph.Abstractions/Styling/GraphEditorStyleOptions.cs`, `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`, `src/AsterGraph.Avalonia/Styling/GraphEditorStyleAdapter.cs`, `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`
- Pattern: Abstractions and Editor emit data-only tokens/descriptors; Avalonia translates them into brushes, layout resources, and controls.

## Entry Points

**Demo Desktop App Bootstrap:**
- Location: `src/AsterGraph.Demo/Program.cs`
- Triggers: `dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj`
- Responsibilities: Configure and start the Avalonia desktop lifetime.

**Demo Application Composition:**
- Location: `src/AsterGraph.Demo/App.axaml.cs`
- Triggers: Avalonia app startup.
- Responsibilities: Create `MainWindow`, assign `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`, and disable duplicate validation plugins.

**Primary Runtime Composition Root:**
- Location: `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- Triggers: Main window construction.
- Responsibilities: Register demo definitions, create the default document, configure style/behavior options, instantiate `GraphEditorViewModel`, and toggle host permissions.

**Reusable UI Entry Point:**
- Location: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml` and `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- Triggers: Any host embedding the control.
- Responsibilities: Bind the shell layout, wire keyboard shortcuts, adapt style resources, and connect Avalonia clipboard/host context services to the editor facade.

**Reference Integration Entry Point:**
- Location: `tools/AsterGraph.HostSample/Program.cs`
- Triggers: `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- Responsibilities: Demonstrate package-consumption boundaries, host context, localization, presentation provider, menu augmentation, and `GraphEditorViewChromeMode`.

## Error Handling

**Strategy:** Fail soft at host/UI boundaries, fail fast for programmer errors in lower-level services.

**Patterns:**
- Wrap host extension execution in `try/catch` and fall back to stock behavior in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` when `IGraphContextMenuAugmentor` throws.
- Convert save/load failures into editor status messages in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs` instead of crashing the host.
- Reject invalid graph operations through guards and status updates inside `GraphEditorViewModel` such as permission checks, direction checks, and compatibility checks.
- Let leaf services throw on invalid arguments or invalid persisted data, for example `src/AsterGraph.Editor/Services/GraphFragmentWorkspaceService.cs` and `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`.

## Cross-Cutting Concerns

**Logging:** Library projects do not provide a logging abstraction. Observable runtime feedback is exposed as `StatusMessage` and editor events in `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`. Sample hosts log with `Console.WriteLine` in `tools/AsterGraph.HostSample/Program.cs`.

**Validation:** Port compatibility is centralized in `src/AsterGraph.Abstractions/Compatibility/IPortCompatibilityService.cs` and `src/AsterGraph.Core/Compatibility/DefaultPortCompatibilityService.cs`. Parameter normalization and validation live in `src/AsterGraph.Editor/Parameters/NodeParameterValueAdapter.cs`.

**Authentication:** Not applicable. The repository is a local desktop/library codebase with no auth subsystem.

---

*Architecture analysis: 2026-03-25*
