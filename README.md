# AsterGraph

AsterGraph is a modular node-graph editor for .NET with an Avalonia UI shell, a reusable editor state layer, and compile-time extension points for registering custom algorithm nodes.

## Public Alpha

- current package baseline: `0.2.0-alpha.1`
- English docs: [`docs/en/`](./docs/en/)
- 中文文档: [`docs/zh-CN/`](./docs/zh-CN/)
- 中文总览: [`README.zh-CN.md`](./README.zh-CN.md)
- public project status: [`docs/en/project-status.md`](./docs/en/project-status.md)
- current alpha scope, known limitations, and stability notes: [`docs/en/alpha-status.md`](./docs/en/alpha-status.md)
- public launch checklist: [`docs/en/public-launch-checklist.md`](./docs/en/public-launch-checklist.md)
- public repo hygiene gate: `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release`

## Current Scope

Current capabilities:

- draggable, selectable graph nodes
- left-drag marquee selection for multi-node editing
- `Shift` append selection and `Ctrl` toggle selection on node click
- selection-aware right-click menu for batch actions
- mini map overview with viewport recentering
- mini map drag navigation for continuous viewport repositioning
- zoom and pan canvas interaction (optimized for mouse and precision trackpads)
- connection rendering and pending connection preview
- graph save/load
- `Ctrl+Z` / `Ctrl+Y` undo and redo graph edits
- selection deletion with `Delete`
- selection copy/paste with `Ctrl+C` / `Ctrl+V`
- batch alignment and distribution for multi-selection
- same-definition multi-selection can batch-edit shared parameters
- selection fragments can be exported to and imported from a JSON file
- strict type compatibility with a small set of safe implicit conversions
- compile-time node-definition registration through providers
- runtime plugin composition through `AsterGraphEditorOptions.PluginRegistrations`
- host-governed plugin trust policy through `AsterGraphEditorOptions.PluginTrustPolicy`
- local candidate discovery and pre-load evaluation through `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`
- canonical plugin inspection through `IGraphEditorSession.Queries.GetPluginLoadSnapshots()`
- descriptor-first automation execution through `IGraphEditorSession.Automation`

Current non-goals:

- algorithm execution engine
- property editor framework
- plugin marketplace or remote install/update workflows
- plugin unload lifecycle
- process sandboxing or untrusted-code isolation guarantees
- dedicated scripting language or workflow-designer UI for automation authoring

## Solution Structure

- `src/AsterGraph.Abstractions`
  Stable contracts for node definitions, catalogs, compatibility, and identifiers.
- `src/AsterGraph.Core`
  Pure graph models, serialization, and default compatibility rules.
- `src/AsterGraph.Editor`
  Host-facing editor runtime, session contracts, factories, behavior options, service seams, and diagnostics hooks.
- `src/AsterGraph.Avalonia`
  Default Avalonia controls, theme, and input handling.
- `src/AsterGraph.Demo`
  Demo host and sample content. This project is not part of the supported SDK boundary.

## Demo Showcase

`src/AsterGraph.Demo` is the graph-first showcase for the SDK. The window keeps one live session on screen while the in-window host menu groups (`展示`, `视图`, `行为`, `运行时`, `扩展`, `自动化`, `集成`, `证明`) adjust shell chrome, behavior toggles, runtime readouts, plugin/automation surfaces, and proof cues around the same graph. The top host menu also includes a visible language switch between Chinese and English.

Use the demo to verify two things quickly:

- which adjustments are host-owned seams that come from the demo shell and `MainWindowViewModel`
- which readouts come from shared runtime state such as `Editor.Session.Diagnostics`

The demo is intentionally a sample host, not a fifth supported package. Its job is to make the package boundary, host menu story, and live session proof easy to inspect before you wire the SDK into your own application.

## Supported Package Boundary

Only these four libraries are published as host-consumable SDK packages:

| Package | Directly reference when | Notes |
| --- | --- | --- |
| `AsterGraph.Abstractions` | defining nodes, identifiers, catalogs, or style tokens | Stable contract layer with no UI dependency |
| `AsterGraph.Core` | working directly with `GraphDocument`, serialization, or compatibility services | Model and persistence layer |
| `AsterGraph.Editor` | building or extending an editor runtime or runtime session | Standard host-facing runtime package; contains the public session contracts, factory/options API, replaceable services, diagnostics seam, and compatibility facade |
| `AsterGraph.Avalonia` | embedding the shipped Avalonia UI | Recommended main integration entry package for UI hosts; depends on `AsterGraph.Editor` and `AsterGraph.Core` |

`AsterGraph.Demo` remains a sample application only. Do not consume it as a package dependency.

Recommended package entry order:

1. UI host integration starts from `AsterGraph.Avalonia`.
2. Protocol/contract integration starts from `AsterGraph.Abstractions`.
3. Add direct `AsterGraph.Editor` and/or `AsterGraph.Core` references only when the host needs their APIs directly (runtime-only composition, direct `GraphDocument`/serialization work, custom compatibility tooling).


## Supported Target Frameworks

All four publishable packages currently target `net8.0` and `net9.0`.

- `net8.0` is the conservative downstream-host baseline because all four publishable packages, `AsterGraph.Serialization.Tests`, and the maintained proof tools run there.
- `net9.0` is also supported for hosts already targeting the newer runtime; the main editor/demo regression suites and the demo app run there.
- the packed consumer proof now also runs `HostSample` under `net10.0` during the release gate; treat that as a compatibility proof for downstream hosts rather than as a claim that the published packages target `net10.0`
- `src/AsterGraph.Demo` targets `net9.0` only because it is a sample app, not a supported package.

## Initialization And Migration Story

Start with the [canonical adoption path](./docs/en/quick-start.md#canonical-adoption-path). The short source of truth is:

- runtime-only or custom UI host:
  - `AsterGraphEditorFactory.CreateSession(...)`
- shipped Avalonia UI host:
  - `AsterGraphEditorFactory.Create(...)`
  - `AsterGraphAvaloniaViewFactory.Create(...)`
- retained migration for existing hosts:
  - `new GraphEditorViewModel(...)`
  - `new GraphEditorView { Editor = editor }`

If you want to own Avalonia layout while still reusing the stock canvas, inspector, or mini map, stay on the `Create(...)` family and treat those surface factories as advanced hosted-UI composition detail rather than a fourth canonical entry path. The constructor/view path remains supported so hosts can migrate in planned batches instead of rewriting in one shot.

For the compact package/route/verification matrix, see [Quick Start](./docs/en/quick-start.md#canonical-adoption-path).
For the public repo posture and near-term public-facing priorities, see [Project Status](./docs/en/project-status.md).
For the current alpha scope and known limitations, see [Alpha Status](./docs/en/alpha-status.md).
For the remaining maintainer-only public-opening steps, see [Public Launch Checklist](./docs/en/public-launch-checklist.md).
For the explicit history/save/dirty behavior contract, see [State Contract](./docs/en/state-contracts.md).
For stability tiers, compatibility retirement, extension precedence, and lane ownership, see [Extension Contracts](./docs/en/extension-contracts.md).

## Runtime Session And Services

`AsterGraph.Editor` now exposes one public runtime contract rooted at `IGraphEditorSession`:

- `Commands`
  - host-triggered mutations such as `SetSelection`, `SetNodePositions`, connection lifecycle commands, viewport centering, `AddNode`, `PanBy`, `SaveWorkspace`, and `LoadWorkspace`
- `Queries`
  - document snapshots, selection/viewport/capability snapshots, node positions, pending connection state, and DTO-based compatibility queries
- `Events`
  - document, selection, viewport, pending-connection, fragment, command, and recoverable-failure notifications
- `BeginMutation(...)`
  - lightweight batching so hosts can coalesce event delivery around planned mutation groups

Hosts can replace the default editor services through `AsterGraphEditorOptions`:

- `IGraphWorkspaceService`
- `IGraphFragmentWorkspaceService`
- `IGraphFragmentLibraryService`
- `IGraphClipboardPayloadSerializer`
- `IGraphEditorDiagnosticsSink`
- `Instrumentation`
  - optional `GraphEditorInstrumentationOptions` for host-standard `ILoggerFactory` and `ActivitySource` wiring

If you do not provide explicit storage services, `AsterGraph.Editor` uses package-neutral defaults resolved through `GraphEditorStorageDefaults`. You can redirect those defaults without replacing every service by setting `StorageRootPath` in `AsterGraphEditorOptions`.

## Diagnostics And Inspection

Phase 5 keeps diagnostics in `AsterGraph.Editor`, not in Avalonia controls.

Use `IGraphEditorSession.Diagnostics` when the host needs machine-readable troubleshooting data:

- `CaptureInspectionSnapshot()`
  - returns a medium-grain immutable snapshot of the current document, selection, viewport, capabilities, pending connection, status, node positions, and recent diagnostics
- `GetRecentDiagnostics(...)`
  - returns bounded recent `GraphEditorDiagnostic` history for support tooling, logs, and host debug panels
- `IGraphEditorDiagnosticsSink`
  - receives the same machine-readable diagnostics stream without requiring hosts to parse `StatusMessage`
- `GraphEditorInstrumentationOptions`
  - opt-in logger/tracing bridge for standard .NET `ILoggerFactory` and `ActivitySource`

`StatusMessage` remains a compatibility UX surface for existing UI hosts. New troubleshooting integrations should prefer `IGraphEditorSession.Diagnostics` and the diagnostics sink.

Minimal runtime-first diagnostics shape:

```csharp
var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    DiagnosticsSink = diagnosticsSink,
    Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
});

var inspection = session.Diagnostics.CaptureInspectionSnapshot();
var recent = session.Diagnostics.GetRecentDiagnostics(20);
```

Core runtime-first host interactions now include:

- selection ownership through `Commands.SetSelection(...)`
- node movement through `Commands.SetNodePositions(...)`
- connection lifecycle through `Commands.StartConnection(...)`, `CompleteConnection(...)`, and `CancelPendingConnection()`
- viewport ownership through `Commands.UpdateViewportSize(...)`, `CenterViewOnNode(...)`, and `CenterViewAt(...)`
- pending connection observation through `Queries.GetPendingConnectionSnapshot()` and `Events.PendingConnectionChanged`
- MVVM-free compatibility discovery through `Queries.GetCompatiblePortTargets(...)`

`Queries.GetCompatibleTargets(...)` remains available only as a compatibility-oriented bridge for existing host code that still depends on view-model objects.

## Plugin Loading, Trust, And Automation

v1.5 ships the first public host-governed plugin trust and distribution-policy baseline on top of the existing plugin-loading and automation surface in `AsterGraph.Editor`.

Current boundary:

- hosts can provide manifest metadata, discover local candidates, evaluate trust before activation, and inspect trusted vs blocked outcomes through public runtime DTOs
- plugin loading still runs in-process
- the trust policy is host-governed allow/block logic, not a sandbox
- marketplace or remote distribution UX is still out of scope

Canonical host flow:

- optionally discover local candidates through `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`
- declare plugin registrations through `AsterGraphEditorOptions.PluginRegistrations`
- optionally provide `AsterGraphEditorOptions.PluginTrustPolicy`
- create the runtime through `AsterGraphEditorFactory.CreateSession(...)` or the hosted UI through `AsterGraphEditorFactory.Create(...)`
- inspect candidate trust/compatibility before load and inspect current plugin load state after creation through `Queries.GetPluginLoadSnapshots()` plus `Diagnostics`
- execute automation through `IGraphEditorSession.Automation.Execute(...)`

Minimal runtime-first shape:

```csharp
var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
{
    DirectorySources =
    [
        new GraphEditorPluginDirectoryDiscoverySource(pluginDirectory),
    ],
    TrustPolicy = trustPolicy,
});

var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    PluginTrustPolicy = trustPolicy,
    PluginRegistrations =
    [
        GraphEditorPluginRegistration.FromPlugin(new MyPlugin()),
        GraphEditorPluginRegistration.FromAssemblyPath(pluginAssemblyPath),
    ],
});

var pluginLoads = session.Queries.GetPluginLoadSnapshots();
var run = session.Automation.Execute(new GraphEditorAutomationRunRequest(
    "host-proof",
    [
        new GraphEditorAutomationStep(
            "select-source",
            new GraphEditorCommandInvocationSnapshot(
                "selection.set",
                [new GraphEditorCommandArgumentSnapshot("nodeId", "source-001")])),
    ]));
```

The official proof ring for the shipped surface is:

- official scripted gates:
  - `eng/ci.ps1 -Lane hygiene`
  - `eng/ci.ps1 -Lane release`
  - `eng/ci.ps1 -Lane contract`
  - `eng/ci.ps1 -Lane maintenance`
- minimal consumer host sample:
  - `tools/AsterGraph.HostSample`
- live package/runtime proof:
  - `tools/AsterGraph.PackageSmoke`
  - `tools/AsterGraph.ScaleSmoke`
- core SDK regression lane:
  - `tests/AsterGraph.Editor.Tests`
  - `tests/AsterGraph.Serialization.Tests`
- demo/sample regression lane:
  - `tests/AsterGraph.Demo.Tests` (sample-host and interaction-path checks)
- runnable proof entry points:
  - `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
    - proves the minimal canonical host path from the consumer side
    - supports packed-package restore with `-p:UsePackedAsterGraphPackages=true`
    - release validation also runs the packed sample under `.NET 10` and emits `HOST_SAMPLE_NET10_OK:True`
  - `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
    - proves packaged consumption across the runtime-first, hosted-UI, and retained compatibility routes
  - `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo`
    - keeps the larger-session readiness, history/save, and automation proof path credible
  - `dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo`
    - remains the interactive visual sample for manual shell inspection, not the minimal consumer path

## Quick Start

For first-time onboarding (package choice, package-source options, install commands, and the minimum Avalonia host path), start with the [canonical Quick Start route](./docs/en/quick-start.md#canonical-adoption-path).

Build:

```powershell
dotnet build avalonia-node-map.sln
```

Run the demo:

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj
```

Pack the publishable libraries:

```powershell
dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages
```

Release verification before publish:

```powershell
# preferred release gate (runs focused contract proof, packs packages, runs HostSample + PackageSmoke + ScaleSmoke against packed packages, collects coverage, and runs package validation)
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

Before a public visibility flip or the first public prerelease tag, follow the [Public Launch Checklist](./docs/en/public-launch-checklist.md).

For a focused consumer/contract proof before the full release gate:

```powershell
# focused consumer/proof gate: HostSample + contract suites + history/save proof
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

For a targeted maintenance gate during hotspot refactors:

```powershell
# targeted maintenance gate: focused hotspot editor regressions + ScaleSmoke
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
```

For broader local feedback before the full release gate:

```powershell
# framework matrix build/test lane without contract/release-only smoke/report steps
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
```

To validate regression lanes independently:

```powershell
# core SDK lane
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj --nologo -v minimal
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --nologo -v minimal

# demo/sample lane
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --nologo -v minimal
```

Run the smoke tools separately only when you need their raw proof markers while debugging:

```powershell
# execute the minimal canonical host sample
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
# prove the same sample through packed packages after pack/release validation
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo
# prove packed-package consumption from a .NET 10 host after pack/release validation
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -f net10.0 -p:EnableNet10ConsumerProof=true -p:UsePackedAsterGraphPackages=true --nologo
# execute PackageSmoke against local packages (different restore path than the CI build step above)
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true --nologo
# execute ScaleSmoke against current build outputs
dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo
```

Use `-t:Rebuild` rather than a plain incremental `dotnet build` when validating XML documentation warning cleanup. Incremental builds can reuse prior outputs and falsely appear warning-free.

If local Avalonia validation starts failing with resource-resolution errors such as `AVLN2000` or `Unable to resolve !AvaloniaResources`, treat that as a stale local build-state problem first, not as a confirmed XAML regression. Clean `src/AsterGraph.Avalonia` and `src/AsterGraph.Demo`, then rerun the failing command sequentially:

```powershell
dotnet clean src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -v minimal
dotnet clean src/AsterGraph.Demo/AsterGraph.Demo.csproj -v minimal
dotnet build src/AsterGraph.Demo/AsterGraph.Demo.csproj -v minimal
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj -v minimal
```

Avoid running overlapping `dotnet build` / `dotnet test` commands against the same worktree while debugging Avalonia resource issues; concurrent builds can produce file-lock noise and misleading XAML compiler failures.

Sample local feed config:

```powershell
copy NuGet.config.sample NuGet.config
```

Package-source and publish-channel details are documented in [`docs/host-integration.md`](./docs/host-integration.md#package-feed-options).

For the detailed host setup and migration guidance, see [`docs/host-integration.md`](./docs/host-integration.md).

## Extension Model

Custom nodes are added at compile time by implementing `INodeDefinitionProvider` from `AsterGraph.Abstractions`.

Typical flow:

1. Create a provider that returns one or more `NodeDefinition` values.
2. Register that provider into an `INodeCatalog`.
3. Build the editor runtime through `AsterGraphEditorFactory.CreateSession(...)`, `AsterGraphEditorFactory.Create(...)`, or the retained `GraphEditorViewModel` constructor.
4. Host either the default full shell through `AsterGraphAvaloniaViewFactory`, or host-managed Avalonia composition through `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, and `AsterGraphMiniMapViewFactory`.

`GraphEditorView` also exposes a formal view-layer API through `GraphEditorView.ChromeMode`:

- `Default`: keep the full shell
- `CanvasOnly`: keep only the central canvas without rebuilding the current `GraphEditorViewModel`

Standalone Avalonia surfaces are now supported in `AsterGraph.Avalonia`:

- `NodeCanvas`
  - create through `AsterGraphCanvasViewFactory`
  - defaults to stock context menu and stock command shortcuts
  - hosts can explicitly opt out with `EnableDefaultContextMenu` and `EnableDefaultCommandShortcuts`
- `GraphInspectorView`
  - pure inspector surface for selection, connections, and parameters
- `GraphMiniMap`
  - narrow overview-plus-navigation surface

The stock Avalonia `GraphContextMenuPresenter` is also public for hosts that want to reuse the shipped menu presenter without forking the canvas implementation.

Phase 4 now adds opt-in presenter replacement in `AsterGraph.Avalonia`:

- `AsterGraphPresentationOptions`
  - shared per-surface presenter configuration for full-shell and standalone composition
- `IGraphNodeVisualPresenter`
  - replace node visuals while `NodeCanvas` keeps selection, drag, connection, marquee, viewport, and anchor resolution behavior
- `IGraphContextMenuPresenter`
  - replace Avalonia menu presentation while `GraphEditorViewModel.BuildContextMenu(...)` and `MenuItemDescriptor` stay the source of truth
- `IGraphInspectorPresenter`
  - replace inspector UI while reusing the existing editor-owned selection, connection, and parameter projections
- `IGraphMiniMapPresenter`
  - replace mini-map UI while reusing editor-owned overview and viewport navigation APIs

Stock presenters remain the zero-configuration default. Hosts only provide a presenter when they want to replace that surface.

For host extension seams, prefer the newer stable context shapes over the raw MVVM graph when available:

- menu augmentation should prefer `GraphContextMenuAugmentationContext`
- node presentation should prefer `NodePresentationContext`

The older `GraphEditorViewModel` / `NodeViewModel` seam roots remain only as compatibility bridges during migration.

Minimal full-shell replacement shape:

```csharp
var presentation = new AsterGraphPresentationOptions
{
    NodeVisualPresenter = customNodePresenter,
    ContextMenuPresenter = customMenuPresenter,
    InspectorPresenter = customInspectorPresenter,
    MiniMapPresenter = customMiniMapPresenter,
};

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
    Presentation = presentation,
});
```

Minimal standalone replacement shape:

```csharp
var canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
    Presentation = new AsterGraphPresentationOptions
    {
        NodeVisualPresenter = customNodePresenter,
        ContextMenuPresenter = customMenuPresenter,
    },
});

var inspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = editor,
    Presentation = new AsterGraphPresentationOptions
    {
        InspectorPresenter = customInspectorPresenter,
    },
});
```

For host-side layout persistence without saving a full graph snapshot, `GraphEditorViewModel` also exposes:

- `GetNodePositions()`
- `TryGetNodePosition(nodeId, out snapshot)`
- `TrySetNodePosition(nodeId, position)`
- `SetNodePositions(snapshots)`

For host-side integration hooks, `GraphEditorViewModel` also exposes event subscriptions such as:

- `DocumentChanged`
- `SelectionChanged`
- `ViewportChanged`
- `FragmentExported`
- `FragmentImported`

For host-side right-click menu augmentation, hosts can pass an `IGraphContextMenuAugmentor` into `GraphEditorViewModel`.
The augmentor receives the current `GraphEditorViewModel`, the current `ContextMenuContext`, and the stock `MenuItemDescriptor` list, then returns the final menu.
This keeps menu customization in the editor layer and supports nested host menus such as `Results -> Preview / Publish / Create Comparison` without replacing `NodeCanvas`.

For host-side permission control, `GraphEditorBehaviorOptions` now also carries a grouped `GraphEditorCommandPermissions` object.
Hosts can start from `GraphEditorCommandPermissions.Default` or `GraphEditorCommandPermissions.ReadOnly`, then selectively override workspace, node, connection, clipboard, fragment, layout, history, and host-extension permissions.

## Host Integration

For a deeper host-composition walkthrough, see:

- [`docs/host-integration.md`](./docs/host-integration.md)
- [`docs/node-presentation-guidelines.md`](./docs/node-presentation-guidelines.md)
- [`docs/interactions-and-shortcuts.md`](./docs/interactions-and-shortcuts.md)

That guide covers:

- the supported four-package boundary and target-framework story
- the runtime-session path in `AsterGraph.Editor`
- the canonical `Create(...)` plus `AsterGraphAvaloniaViewFactory` composition path for the default UI
- the retained constructor-based migration path for existing hosts
- how to wire localization, node presentation, style options, host menu context, replaceable services, diagnostics, inspection, and optional instrumentation together
- how to use the typed host-context helper extensions safely
- how to switch `GraphEditorView.ChromeMode` at runtime without rebuilding editor state
- how to compose standalone canvas, inspector, and mini map surfaces against the same editor state
- how to opt out of stock standalone-canvas menu and shortcut behavior
- how to replace node/menu/inspector/mini-map presenters per surface without moving behavior into the host

Interactive demo host:

- `src/AsterGraph.Demo`
- Run with:
  - `dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo`
- Use the demo when you want the visual/default host-composition reference, host menu seams, chrome toggles, and live shell behavior.

Minimal consumer host sample:

- `tools/AsterGraph.HostSample`
- Run with:
  - `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo`
  - `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj -p:UsePackedAsterGraphPackages=true --nologo`
- The sample keeps the code path narrow:
  - canonical runtime-first creation via `AsterGraphEditorFactory.CreateSession(...)`
  - canonical hosted-UI creation via `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
  - optional packed-package restore without depending on the demo shell

Package consumption smoke:

- `tools/AsterGraph.PackageSmoke`
- Run with:
  - `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj --nologo`
- The tool emits stable markers for:
  - runtime-first session creation
  - canonical hosted-UI composition via `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
  - retained compatibility composition via `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = ... }`
  - packaged restore / consumption viability without depending on the demo shell

Repeatable scale validation:

- `tools/AsterGraph.ScaleSmoke`
- Run with:
  - `dotnet run --project tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj --nologo`
- The tool emits stable `SCALE_*` markers for:
  - large-graph setup size
  - bulk selection
  - connection delete/recreate flow
  - drag/history/save/undo/redo dirty-state continuity
  - viewport fitting
  - inspection snapshot continuity

## Type Compatibility

AsterGraph uses:

- exact type matches by default
- a narrow set of explicit safe implicit conversions

The built-in implicit conversions currently include:

- `int -> float`
- `int -> double`
- `float -> double`

Rejected conversions stay explicit and visible rather than guessing.

## Serialization

Graph documents are serialized in `AsterGraph.Core` and can persist connection-level conversion metadata. The stable contract identifiers in `AsterGraph.Abstractions` are intended to survive UI and host changes.

Current serialized payloads are versioned:

- graph document JSON now emits `SchemaVersion`
- clipboard payload JSON now emits `SchemaVersion`
- readers remain backward-compatible with the earlier unversioned document and clipboard shapes
- schema branching is centralized inside dedicated compatibility helpers instead of being spread across serializer entry points

This keeps future contract evolution explicit instead of relying on implicit JSON shape guesses.

## Selection And Clipboard

AsterGraph keeps selection state in the editor layer:

- single selection still drives the inspector
- marquee selection can select multiple nodes
- right-click on the current multi-selection opens batch tools
- `Delete` removes the full current selection
- `Ctrl+C` copies the selected nodes plus only the internal links between them
- `Ctrl+V` pastes a new offset fragment near the current viewport center
- copy writes a versioned AsterGraph JSON payload to the system clipboard when the host provides clipboard access
- paste prefers the system clipboard payload and falls back to the in-memory fragment clipboard

Batch selection tools currently include:

- align left, center, right
- align top, middle, bottom
- distribute horizontally or vertically

Hosts can also opt into drag-assist behavior through `CanvasStyleOptions`:

- `EnableGridSnapping`
- `EnableAlignmentGuides`
- `SnapTolerance`

For runtime behavior switching, hosts should prefer `GraphEditorBehaviorOptions` plus `GraphEditorViewModel.UpdateBehaviorOptions(...)`.
The drag assistant now evaluates snapping from the drag start position instead of accumulating per-frame snap deltas, which avoids pointer drift during grid snapping.

## Fragments

In addition to clipboard-based fragment copy/paste, AsterGraph now supports explicit fragment file workflows:

- `Export Fragment` saves the current selection as JSON
- `Import Fragment` loads the saved fragment file and pastes it near the current viewport center
- the demo host uses the default fragment path exposed through the editor workspace services
- hosts can also call `ExportSelectionFragmentTo(path)` and `ImportFragmentFrom(path)` for custom fragment libraries
- hosts can inject a custom `GraphFragmentWorkspaceService` into `GraphEditorViewModel`

## Style Configuration

Hosts can provide a framework-neutral style configuration through `GraphEditorStyleOptions` in `AsterGraph.Abstractions.Styling`.

Recommended flow:

1. Create a `GraphEditorStyleOptions` value in the host
2. Pass it into `GraphEditorViewModel`
3. Let `AsterGraph.Avalonia` adapt those tokens into brushes, radii, spacing, menu rendering, and component visuals

This keeps the public styling surface stable without leaking Avalonia-specific types into the SDK contract.

The current style surface is organized by concern:

- `ShellStyleOptions` for shell colors, typography, and host-panel widths
- `InspectorStyleOptions` for inspector typography and section geometry
- `NodeCardStyleOptions` / `PortStyleOptions` / `ConnectionStyleOptions` for graph-scene visuals
- `ContextMenuStyleOptions` for right-click menu background, hover, foreground, separators, and item sizing

`AsterGraph.Avalonia` now keeps menu rendering behind a dedicated presenter, so hosts can keep driving behavior from editor-layer menu descriptors while styling stays in the Avalonia layer.

## Behavior Configuration

Hosts can also provide explicit editor behavior settings through `GraphEditorBehaviorOptions` in `AsterGraph.Editor.Configuration`.

Recommended flow:

1. Create a `GraphEditorBehaviorOptions` value in the host
2. Pass it into `GraphEditorViewModel`
3. Keep behavior toggles separate from visual tokens

Current behavior sections include:

- `HistoryBehaviorOptions`
- `SelectionBehaviorOptions`
- `DragAssistBehaviorOptions`
- `FragmentBehaviorOptions`
- `ViewBehaviorOptions`
- `GraphEditorCommandPermissions`

The demo host also exposes live toggles for:

- grid snapping
- alignment guides
- read-only mode
- workspace commands
- fragment commands
- host menu extensions

## Command Permissions

Hosts can centrally govern editability through `GraphEditorCommandPermissions`:

- `WorkspaceCommandPermissions`
- `HistoryCommandPermissions`
- `NodeCommandPermissions`
- `ConnectionCommandPermissions`
- `ClipboardCommandPermissions`
- `LayoutCommandPermissions`
- `FragmentCommandPermissions`
- `HostCommandPermissions`

The editor applies these permissions in one place:

- command `CanExecute`
- toolbar enabled state
- keyboard shortcuts
- parameter editor read-only state
- built-in right-click menus
- host context-menu contribution visibility

`ReadOnly` is not a second implementation path. It is a grouped permission preset that keeps navigation and inspection available while denying graph mutations.

## Context Menu Extension

Hosts that need business-specific node actions can use the public context-menu augmentor API:

1. Implement `IGraphContextMenuAugmentor`
2. Inspect `ContextMenuContext.TargetKind`
3. Use `ContextMenuContext.ClickedNodeId`, `SelectedNodeIds`, and `SelectedConnectionIds` as needed
4. Start from the provided stock `MenuItemDescriptor` list and append your own items
5. Pass the augmentor into `GraphEditorViewModel`

The demo host now includes a sample node contribution:

- `Results`
- `Preview`
- `Publish`
- `Create Comparison`

This is appended legally through the public editor API rather than by replacing the Avalonia menu renderer.

## Roadmap

- move more sample-only styling and content out of shared projects where appropriate
- add richer graph definition metadata and parameter editing
- explore richer provenance and remote distribution workflows only after the current local manifest/trust proof ring proves sufficient
- explore richer automation authoring only after the shipped session-first runner proves sufficient
- improve automated UI coverage for pointer-based graph gestures

## License

MIT. See [LICENSE.md](./LICENSE.md).
