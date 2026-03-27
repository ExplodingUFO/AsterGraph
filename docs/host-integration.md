# Host Integration Guide

This guide shows how to host AsterGraph without over-coupling your application to internal editor details.

## Package Choice

The supported package publish boundary is exactly these four packages:

- `AsterGraph.Abstractions` for node definitions, identifiers, and shared style contracts
- `AsterGraph.Core` for `GraphDocument`, serialization, and compatibility services
- `AsterGraph.Editor` for editor runtime composition, factories, behavior options, and host extension seams
- `AsterGraph.Avalonia` for the default Avalonia view shell and standalone Avalonia surfaces

Recommended entry strategy:

1. Default host integration starts from `AsterGraph.Avalonia` (main component entry).
2. Protocol/contract-first integration starts from `AsterGraph.Abstractions`.
3. Add direct `AsterGraph.Editor` and/or `AsterGraph.Core` references only when host code needs those APIs directly.

For a default hosted UI, the canonical direct-reference set is:

- `AsterGraph.Abstractions`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

Add a direct `AsterGraph.Core` reference when the host also needs direct access to graph models, serialization, or compatibility services outside the editor factories.

`AsterGraph.Demo` is not a consumable package and is not part of the publish set.

## GitHub Packages Feed Setup

Use GitHub Packages for private restore and publish of the four supported SDK packages (`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`).

Add source via CLI:

```powershell
# replace OWNER with your GitHub org/user
dotnet nuget add source "https://nuget.pkg.github.com/OWNER/index.json" `
  --name github-astergraph `
  --username GITHUB_USERNAME `
  --password GITHUB_PAT `
  --store-password-in-clear-text
```

Credential expectations:

- restore requires a token with `read:packages`
- publish requires a token with `write:packages`
- private repo package access may also require repository scopes depending on organization policy

Prefer storing credentials in local user config or CI secrets, not in tracked repository files.

`NuGet.config` source shape (credential-free template):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-astergraph" value="artifacts/packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-astergraph" value="https://nuget.pkg.github.com/OWNER/index.json" />
  </packageSources>
</configuration>
```

Publish sequence (Demo excluded):

```powershell
# pack
dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages
dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages

# push only .nupkg files
dotnet nuget push "artifacts/packages/AsterGraph.Abstractions.*.nupkg" --source github-astergraph --skip-duplicate
dotnet nuget push "artifacts/packages/AsterGraph.Core.*.nupkg" --source github-astergraph --skip-duplicate
dotnet nuget push "artifacts/packages/AsterGraph.Editor.*.nupkg" --source github-astergraph --skip-duplicate
dotnet nuget push "artifacts/packages/AsterGraph.Avalonia.*.nupkg" --source github-astergraph --skip-duplicate
```

If push fails with authentication or permission errors (401/403), refresh source credentials (token with `write:packages`) and retry publish.

## Canonical Host Composition

Phase 2 gives hosts two canonical entry paths. Pick the narrowest surface that matches what you want to own.

Runtime-only or custom-UI host:

1. Build or register your `INodeCatalog`.
2. Create the initial `GraphDocument`.
3. Choose a host-owned `CompatibilityService`.
4. Optionally provide storage overrides, diagnostics, localization, node presentation, or context-menu augmentation.
5. Create the session through `AsterGraphEditorFactory.CreateSession(...)`.

Minimal runtime-first shape:

```csharp
var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    StorageRootPath = Path.Combine(appDataRoot, "MyHost", "Graph"),
    DiagnosticsSink = diagnosticsSink,
});

session.Events.CommandExecuted += (_, args) =>
    Console.WriteLine(args.CommandId);

session.Events.RecoverableFailure += (_, args) =>
    Console.WriteLine($"{args.Code}: {args.Message}");

using (session.BeginMutation("host-bootstrap"))
{
    session.Commands.AddNode(new NodeDefinitionId("host.sample.node"), new GraphPoint(320, 180));
    session.Commands.PanBy(12, 18);
}

var snapshot = session.Queries.CreateDocumentSnapshot();
```

Default full-shell Avalonia host:

1. Build or register your `INodeCatalog`.
2. Create the initial `GraphDocument`.
3. Create `GraphEditorStyleOptions` and `GraphEditorBehaviorOptions` for host-owned configuration.
4. Create optional host providers:
   - `IGraphLocalizationProvider`
   - `INodePresentationProvider`
   - `IGraphContextMenuAugmentor`
5. Create the editor compatibility facade with `AsterGraphEditorFactory.Create(...)`.
6. Create the default Avalonia view with `AsterGraphAvaloniaViewFactory.Create(...)`.

Minimal default-view shape:

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    StyleOptions = style,
    BehaviorOptions = behavior,
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
});

var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
});
```

If the host is embedding the shipped Avalonia controls, `Create(...)` plus `AsterGraphAvaloniaViewFactory` remains the cleanest setup. If the host wants to own UI composition entirely, `CreateSession(...)` is now the narrower public boundary.

Host-managed Avalonia surface composition:

1. Build the editor compatibility facade through `AsterGraphEditorFactory.Create(...)`.
2. Compose the stock Avalonia surfaces you want to keep:
   - `AsterGraphCanvasViewFactory.Create(...)`
   - `AsterGraphInspectorViewFactory.Create(...)`
   - `AsterGraphMiniMapViewFactory.Create(...)`
3. Bind all of them to the same `GraphEditorViewModel`.

Minimal standalone-surface shape:

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
});

var canvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
    EnableDefaultContextMenu = false,
    EnableDefaultCommandShortcuts = false,
});

var inspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = editor,
});

var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = editor,
});
```

Use this path when the host wants to keep the stock graph canvas, inspector, or mini map but does not want the shipped header/library/status shell.

## Runtime Session Surface

`IGraphEditorSession` is the public Phase 2 runtime root in `AsterGraph.Editor`.

- `Commands`
  - host-triggered mutations such as `AddNode`, `DeleteSelection`, `PanBy`, `ZoomAt`, `SaveWorkspace`, and `LoadWorkspace`
- `Queries`
  - `CreateDocumentSnapshot`, selection/viewport/capability snapshots, node positions, and compatibility-target discovery
- `Events`
  - document, selection, viewport, fragment, command, and recoverable-failure notifications
- `BeginMutation(...)`
  - coalesces runtime notifications until the mutation scope closes, which is useful when the host wants one planned batch instead of a stream of intermediate events

`GraphEditorViewModel.Session` exposes the same runtime contract from the retained compatibility facade, so migrating hosts can adopt the new runtime API without discarding existing `GraphEditorViewModel` integrations first.

## Replaceable Services And Diagnostics

The default services are now explicit public seams. Hosts can replace any of these through `AsterGraphEditorOptions`:

- `IGraphWorkspaceService`
- `IGraphFragmentWorkspaceService`
- `IGraphFragmentLibraryService`
- `IGraphClipboardPayloadSerializer`
- `IGraphEditorDiagnosticsSink`
- `GraphEditorInstrumentationOptions`
  - opt-in `ILoggerFactory` and `ActivitySource` wiring for host-standard logging/tracing

If you want default behavior but under a host-owned storage root, set `StorageRootPath` and let `AsterGraph.Editor` create package-neutral defaults through `GraphEditorStorageDefaults`.

The package-neutral defaults resolve to:

- `workspace.json`
- `selection-fragment.json`
- `fragments/`

under a root chosen by either:

- the host-supplied `StorageRootPath`, or
- the built-in local application data fallback when `StorageRootPath` is not supplied

`IGraphEditorDiagnosticsSink` receives recoverable runtime failures such as workspace save/load errors or host augmentor failures. Hosts should treat this as the stable place to forward AsterGraph runtime issues into their own logging, telemetry, or debug panels.

`IGraphEditorSession.Diagnostics` is the canonical inspection surface for both the factory/session path and the retained `GraphEditorViewModel.Session` compatibility path.

It exposes:

- `CaptureInspectionSnapshot()`
  - immutable inspection output for the current document, selection, viewport, capabilities, pending connection, status, node positions, and recent diagnostics
- `GetRecentDiagnostics(...)`
  - bounded recent `GraphEditorDiagnostic` history suitable for host support tooling and machine-readable regression checks

Minimal diagnostics-first runtime usage:

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

Use `StatusMessage` as a compatibility-facing UI convenience only. New host diagnostics and support flows should consume `session.Diagnostics` or the sink instead of parsing view text.

## Staged Migration Compatibility Path

Existing hosts do not need to rewrite immediately. The constructor-based setup remains supported as a compatibility facade while you migrate toward either the new runtime-session path or the factory/options UI path.

Retained compatibility path:

```csharp
var editor = new GraphEditorViewModel(
    document,
    catalog,
    new DefaultPortCompatibilityService(),
    styleOptions: style,
    behaviorOptions: behavior,
    contextMenuAugmentor: menuAugmentor,
    nodePresentationProvider: presentationProvider,
    localizationProvider: localizationProvider);

var view = new GraphEditorView
{
    Editor = editor,
};
```

Recommended migration stages:

1. Keep `GraphEditorViewModel` and `GraphEditorView` if the host already uses them directly.
2. Start consuming `GraphEditorViewModel.Session` if you want to move host-side commands, queries, batching, diagnostics, and inspection onto the new runtime contract first.
3. Move runtime creation to `AsterGraphEditorFactory.CreateSession(...)` or `AsterGraphEditorFactory.Create(...)` once you want a single documented composition point.
4. Move Avalonia view creation to `AsterGraphAvaloniaViewFactory.Create(...)` once the host is ready to standardize the UI entry path too.

The constructor path is supported for compatibility, but new host documentation and samples now assume the runtime-session or factory path first.

## View Chrome Mode

`GraphEditorView.ChromeMode` is a formal view-layer API. It does not belong to `GraphEditorBehaviorOptions`, and changing it does not mutate `GraphEditorViewModel` state.

- `GraphEditorViewChromeMode.Default` keeps the full shell.
- `GraphEditorViewChromeMode.CanvasOnly` hides the header, library, inspector, and status chrome.
- You can switch it at runtime without rebuilding the current `GraphEditorView` or `GraphEditorViewModel`.

Minimal usage:

```csharp
view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;
```

This keeps the central `NodeCanvas`, context menus, host-context flow, shortcuts, and node presentation data while removing the surrounding shell panels.

## Embeddable Avalonia Surfaces

Phase 3 makes three standalone Avalonia surfaces part of the supported host story:

- `NodeCanvas`
  - canonical entry: `AsterGraphCanvasViewFactory.Create(...)`
  - defaults to stock context menu and stock command shortcuts
  - hosts can explicitly disable those defaults through `EnableDefaultContextMenu` and `EnableDefaultCommandShortcuts`
- `GraphInspectorView`
  - canonical entry: `AsterGraphInspectorViewFactory.Create(...)`
  - pure inspector surface only: selection summary, connections, and parameters
  - does not include workspace, fragment, shortcut-help, or mini-map shell blocks
- `GraphMiniMap`
  - canonical entry: `AsterGraphMiniMapViewFactory.Create(...)`
  - stays a narrow overview-plus-navigation control

`GraphContextMenuPresenter` is now a public stock Avalonia presenter for hosts that want to reuse the shipped menu rendering path.

## Replaceable Presentation Kit

Phase 4 keeps the Phase 3 surface boundary but adds opt-in presenter replacement in `AsterGraph.Avalonia`.

Shared configuration surface:

```csharp
var presentation = new AsterGraphPresentationOptions
{
    NodeVisualPresenter = customNodePresenter,
    ContextMenuPresenter = customMenuPresenter,
    InspectorPresenter = customInspectorPresenter,
    MiniMapPresenter = customMiniMapPresenter,
};
```

Full-shell usage:

```csharp
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
    ChromeMode = GraphEditorViewChromeMode.Default,
    Presentation = presentation,
});
```

Standalone usage:

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

var miniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = editor,
    Presentation = new AsterGraphPresentationOptions
    {
        MiniMapPresenter = customMiniMapPresenter,
    },
});
```

Important contract boundaries:

- `AsterGraph.Editor` still owns state, commands, queries, diagnostics, menu intent, and node presentation data.
- `NodeCanvas` still owns selection, drag, marquee selection, pending connection preview, connection creation, viewport interaction, and port-anchor resolution.
- `GraphEditorViewModel.BuildContextMenu(...)` and `MenuItemDescriptor` remain the only source of menu intent.
- `GraphInspectorView` custom presenters still bind to the existing editor-owned inspector projections and `NodeParameterViewModel` editing flow.
- `GraphMiniMap` custom presenters still use editor-owned overview and `CenterViewAt(...)` navigation rather than introducing shell-only responsibilities.

Stock defaults remain active when `Presentation` is omitted. Replacement is per surface, not all-or-nothing.

Header, library, and status chrome remain shell-only in Phase 3. If a host wants to omit them, the supported approach is direct standalone-surface composition, not a larger `GraphEditorViewChromeMode` matrix.

Reference sample:

- `tools/AsterGraph.HostSample`
- Run with:
  - `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
- The sample demonstrates both the convenience full shell and standalone canvas/inspector/mini map composition against the same editor state.
- It also prints human-readable diagnostics/inspection evidence plus opt-in logger/activity instrumentation markers for Phase 5 validation.

## Localization

Use `IGraphLocalizationProvider` when the host wants to override built-in editor text without forking the editor layer.

Typical usage:

```csharp
internal sealed class HostLocalizationProvider : IGraphLocalizationProvider
{
    private static readonly IReadOnlyDictionary<string, string> Values =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.inspector.title.none"] = "请选择宿主节点",
            ["editor.menu.canvas.addNode"] = "添加节点",
        };

    public string GetString(string key, string fallback)
        => Values.TryGetValue(key, out var value) ? value : fallback;
}
```

Use localization for:

- stock context-menu labels
- inspector titles and descriptions
- stock status messages

Do not use it for host-only business copy that never belongs to the shared editor.

Localization continues to work unchanged whether the host consumes the runtime through `GraphEditorViewModel`, `AsterGraphEditorFactory.Create(...)`, or `AsterGraphEditorFactory.CreateSession(...)`.

## Node Presentation

Use `INodePresentationProvider` when the host needs runtime-owned display state on top of static node definitions.

Typical usage:

```csharp
internal sealed class HostPresentationProvider : INodePresentationProvider
{
    public NodePresentationState GetNodePresentation(NodeViewModel node)
        => new(
            SubtitleOverride: "Runtime annotated",
            TopRightBadges:
            [
                new NodeAdornmentDescriptor("Ready", "#6AD5C4", "Host reports the node is ready."),
            ],
            StatusBar: new NodeStatusBarDescriptor(
                "Preview available",
                "#6AD5C4",
                "Host marks this node as preview-ready."));
}
```

Use presentation state for:

- runtime subtitle or description overrides
- small status badges
- bottom status bars

Do not encode host business behavior into the editor layer itself. Keep the semantics in the host and only pass the display snapshot into AsterGraph.

This seam stays additive in Phase 2. The runtime-session additions do not replace or narrow the presentation-provider path.

For visual usage guidance, see:

- [`docs/node-presentation-guidelines.md`](./node-presentation-guidelines.md)

## Style Options

Use `GraphEditorStyleOptions` when the host wants to provide framework-neutral tokens without pushing Avalonia types into the host-editor contract.

Typical usage:

```csharp
var style = GraphEditorStyleOptions.Default with
{
    Shell = GraphEditorStyleOptions.Default.Shell with
    {
        HighlightHex = "#F3B36B",
        LibraryPanelWidth = 312,
    },
    ContextMenu = GraphEditorStyleOptions.Default.ContextMenu with
    {
        BackgroundHex = "#102332",
    },
};
```

Recommended split:

- host owns semantic color and spacing choices
- `AsterGraph.Avalonia` adapts those tokens into rendered controls
- behavior switches remain in `GraphEditorBehaviorOptions`, not in style tokens

## Host Menu Context

`ContextMenuContext.HostContext` carries host runtime objects into the menu augmentation path.

The editor contract stays framework-neutral, so hosts should avoid raw casts when possible and prefer the helper extensions from `AsterGraph.Editor.Hosting`:

```csharp
if (context.TryGetOwner<MyHostOwner>(out var owner))
{
    // safe typed owner access
}

if (context.TryGetTopLevel<MyShellWindow>(out var shell))
{
    // safe typed top-level access
}
```

These helpers:

- keep host code shorter
- avoid repeated `is` checks
- return `false` safely when the requested type does not match
- stay portable across Avalonia and future non-Avalonia hosts

The context-menu augmentation seam and the typed host-context helpers continue to work unchanged in Phase 2. The new runtime-session APIs are additive and do not require hosts to replace their existing menu augmentors.

## Putting It Together

The recommended layering is:

- `AsterGraph.Abstractions`
  node definitions, identifiers, style contracts
- `AsterGraph.Core`
  graph models and compatibility
- `AsterGraph.Editor`
  runtime session contracts, state orchestration, factories, service seams, diagnostics, and host extension seams
- `AsterGraph.Avalonia`
  default Avalonia UI shell plus opt-in presenter replacement seams for node visuals, menus, inspector, and mini map

The host should own:

- business localization content
- runtime node state
- style token choices
- business-specific context menu actions

AsterGraph should own:

- shared graph editing behavior
- shared menu intent structure
- shared rendering adaptation in the Avalonia layer

This split keeps the public surface stable while still allowing deep host customization.
