# Host Integration Guide

This guide shows how to host AsterGraph without over-coupling your application to internal editor details.

## Package Choice

The supported Phase 1 package boundary is:

- `AsterGraph.Abstractions` for node definitions, identifiers, and shared style contracts
- `AsterGraph.Core` for `GraphDocument`, serialization, and compatibility services
- `AsterGraph.Editor` for editor runtime composition, factories, behavior options, and host extension seams
- `AsterGraph.Avalonia` for the default Avalonia view shell

For a default hosted UI, the canonical direct-reference set is:

- `AsterGraph.Abstractions`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

Add a direct `AsterGraph.Core` reference when the host also needs direct access to graph models, serialization, or compatibility services outside the editor factories.

`AsterGraph.Demo` is not a consumable package. Treat it as a reference application only.

## Canonical Host Composition

Use the factory/options path for new host integration. It is the canonical Phase 1 initialization surface because it keeps defaults, extension seams, and migration guidance in one place.

Typical host composition flow:

1. Build or register your `INodeCatalog`.
2. Create the initial `GraphDocument`.
3. Create `GraphEditorStyleOptions` and `GraphEditorBehaviorOptions` for host-owned configuration.
4. Create optional host providers:
   - `IGraphLocalizationProvider`
   - `INodePresentationProvider`
   - `IGraphContextMenuAugmentor`
5. Create the editor runtime with `AsterGraphEditorFactory`.
6. Create the default Avalonia view with `AsterGraphAvaloniaViewFactory`.

Minimal canonical shape:

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new DefaultPortCompatibilityService(),
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

## Staged Migration Compatibility Path

Existing hosts do not need to rewrite immediately. Phase 1 keeps the previous constructor-based setup supported as a compatibility facade while you migrate toward the factory/options path.

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
2. Move runtime creation to `AsterGraphEditorFactory.Create(...)` once you want a single documented composition point.
3. Move Avalonia view creation to `AsterGraphAvaloniaViewFactory.Create(...)` once the host is ready to standardize the UI entry path too.

The constructor path is supported for compatibility, but new host documentation and samples now assume the factory path first.

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

Reference sample:

- `tools/AsterGraph.HostSample`
- Run with:
  - `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`

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

## Putting It Together

The recommended layering is:

- `AsterGraph.Abstractions`
  node definitions, identifiers, style contracts
- `AsterGraph.Core`
  graph models and compatibility
- `AsterGraph.Editor`
  state orchestration, factories, and host extension seams
- `AsterGraph.Avalonia`
  default Avalonia UI shell

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
