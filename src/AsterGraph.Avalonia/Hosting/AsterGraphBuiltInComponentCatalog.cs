using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Lists the built-in Avalonia component entry points that hosts can compose or track during Phase 3.
/// </summary>
public static class AsterGraphBuiltInComponentCatalog
{
    /// <summary>
    /// Stable identifier for the standalone node canvas entry.
    /// </summary>
    public const string Canvas = "canvas";

    /// <summary>
    /// Stable identifier for the standalone MiniMap entry.
    /// </summary>
    public const string MiniMap = "minimap";

    /// <summary>
    /// Stable identifier for the background/grid entry.
    /// </summary>
    public const string BackgroundGrid = "background-grid";

    /// <summary>
    /// Stable identifier for the standalone inspector entry.
    /// </summary>
    public const string Inspector = "inspector";

    /// <summary>
    /// Stable identifier for the hosted workbench controls/panel entry.
    /// </summary>
    public const string ControlsPanel = "controls-panel";

    /// <summary>
    /// Stable identifier for command and tool action projection.
    /// </summary>
    public const string CommandToolProjection = "command-tool-projection";

    /// <summary>
    /// Stable identifier for the node toolbar surface.
    /// </summary>
    public const string NodeToolbar = "node-toolbar";

    /// <summary>
    /// Stable identifier for the edge toolbar surface.
    /// </summary>
    public const string EdgeToolbar = "edge-toolbar";

    /// <summary>
    /// Stable identifier for the node resizer surface.
    /// </summary>
    public const string NodeResizer = "node-resizer";

    /// <summary>
    /// Gets the current built-in component descriptors.
    /// </summary>
    public static IReadOnlyList<AsterGraphBuiltInComponentDescriptor> Components { get; } =
    [
        new(
            Canvas,
            "Canvas",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(NodeCanvas).FullName!,
            typeof(AsterGraphCanvasViewFactory).FullName!,
            "AsterGraphCanvasViewFactory.Create(...)",
            "builtin-hosted-controls-route",
            "docs/en/demo-cookbook.md",
            "Standalone graph canvas for hosts that compose their own chrome."),
        new(
            MiniMap,
            "MiniMap",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(GraphMiniMap).FullName!,
            typeof(AsterGraphMiniMapViewFactory).FullName!,
            "AsterGraphMiniMapViewFactory.Create(...)",
            "builtin-minimap-workbench-route",
            "docs/en/demo-cookbook.md",
            "Standalone MiniMap component with stock and custom presenter routes."),
        new(
            BackgroundGrid,
            "Background/Grid",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(GridBackground).FullName!,
            null,
            "GridBackground",
            "builtin-background-grid-route",
            "docs/en/demo-cookbook.md",
            "Reusable grid background control and theme-backed canvas style option."),
        new(
            Inspector,
            "Inspector",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(GraphInspectorView).FullName!,
            typeof(AsterGraphInspectorViewFactory).FullName!,
            "AsterGraphInspectorViewFactory.Create(...)",
            "authoring-inspector-route",
            "docs/en/authoring-inspector-recipe.md",
            "Standalone inspector component for selected node parameters."),
        new(
            ControlsPanel,
            "Controls/Panel Workbench",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(GraphEditorView).FullName!,
            typeof(AsterGraphHostBuilder).FullName!,
            "AsterGraphHostBuilder.Create().UseDefaultWorkbench()",
            "builtin-hosted-controls-route",
            "docs/en/demo-cookbook.md",
            "Default hosted workbench chrome, command controls, panels, and shell composition."),
        new(
            CommandToolProjection,
            "Command/Tool Projection",
            AsterGraphBuiltInComponentStatus.Public,
            typeof(AsterGraphHostedActionProjection).FullName!,
            typeof(AsterGraphAuthoringToolActionFactory).FullName!,
            "AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(...)",
            "v077-authoring-platform-route",
            "docs/en/authoring-surface-recipe.md",
            "Host-consumable action descriptors for toolbar, menu, rail, and palette surfaces."),
        new(
            NodeToolbar,
            "NodeToolbar",
            AsterGraphBuiltInComponentStatus.Planned,
            null,
            typeof(AsterGraphAuthoringToolActionFactory).FullName!,
            "AsterGraphAuthoringToolActionFactory.CreateNodeActions(...)",
            "builtin-hosted-controls-route",
            "docs/en/demo-cookbook.md",
            "Node tool actions are projected today; an independent NodeToolbar control is a later Phase 3 slice."),
        new(
            EdgeToolbar,
            "EdgeToolbar",
            AsterGraphBuiltInComponentStatus.Planned,
            null,
            typeof(AsterGraphAuthoringToolActionFactory).FullName!,
            "AsterGraphAuthoringToolActionFactory.CreateConnectionActions(...)",
            "v078-custom-node-edge-route",
            "docs/en/demo-cookbook.md",
            "Edge tool actions are projected today; an independent EdgeToolbar control is a later Phase 3 slice."),
        new(
            NodeResizer,
            "NodeResizer",
            AsterGraphBuiltInComponentStatus.InternalWorkbench,
            null,
            null,
            "NodeCanvas resize feedback and persisted node surface commands",
            "v078-custom-node-edge-route",
            "docs/en/custom-node-host-recipe.md",
            "Resize behavior is currently part of NodeCanvas workbench interaction rather than a standalone public control."),
    ];

    /// <summary>
    /// Finds a descriptor by stable id.
    /// </summary>
    /// <param name="id">Stable component id.</param>
    /// <param name="descriptor">Resolved descriptor when found.</param>
    /// <returns><see langword="true" /> when the catalog contains <paramref name="id" />.</returns>
    public static bool TryGet(string id, out AsterGraphBuiltInComponentDescriptor? descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        foreach (var component in Components)
        {
            if (string.Equals(component.Id, id, StringComparison.Ordinal))
            {
                descriptor = component;
                return true;
            }
        }

        descriptor = null;
        return false;
    }
}

/// <summary>
/// Describes one built-in Avalonia component or built-in component track.
/// </summary>
/// <param name="Id">Stable catalog id.</param>
/// <param name="Title">Human-readable component name.</param>
/// <param name="Status">Current public availability status.</param>
/// <param name="SurfaceTypeName">Public component or projection type name when one is available.</param>
/// <param name="FactoryTypeName">Public factory or projection factory type name when one is available.</param>
/// <param name="EntryPoint">Copyable entry point or current bounded route.</param>
/// <param name="CookbookRouteId">Cookbook route that demonstrates or tracks the component.</param>
/// <param name="DocumentationPath">Primary documentation anchor.</param>
/// <param name="Boundary">Scope boundary for the current component status.</param>
public sealed record AsterGraphBuiltInComponentDescriptor(
    string Id,
    string Title,
    AsterGraphBuiltInComponentStatus Status,
    string? SurfaceTypeName,
    string? FactoryTypeName,
    string EntryPoint,
    string CookbookRouteId,
    string DocumentationPath,
    string Boundary);

/// <summary>
/// Classifies whether a built-in component has an independent public entry point today.
/// </summary>
public enum AsterGraphBuiltInComponentStatus
{
    /// <summary>
    /// The component or projection has a public, documented host entry point.
    /// </summary>
    Public,

    /// <summary>
    /// The behavior is implemented inside the hosted workbench but is not yet a standalone public component.
    /// </summary>
    InternalWorkbench,

    /// <summary>
    /// The React Flow-style built-in is planned and tracked, but Phase 3a does not expose a fake public control for it.
    /// </summary>
    Planned,
}
