using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Defines the stock hosted workbench surfaces composed by the Avalonia route.
/// </summary>
/// <remarks>
/// These options only control the existing <see cref="GraphEditorView" /> chrome. They do not create
/// a new runtime model; editor ownership remains on the session created by the editor factory.
/// </remarks>
public sealed record AsterGraphWorkbenchOptions
{
    /// <summary>
    /// Gets the default hosted workbench composition.
    /// </summary>
    public static AsterGraphWorkbenchOptions Default { get; } = new();

    /// <summary>
    /// Creates one of the stock hosted workbench layout presets.
    /// </summary>
    public static AsterGraphWorkbenchOptions ForPreset(AsterGraphWorkbenchLayoutPreset preset)
        => Default with
        {
            LayoutPreset = preset,
            PanelState = AsterGraphWorkbenchPanelState.ForPreset(preset),
        };

    /// <summary>
    /// Resets hosted layout and panel state to the default workbench composition.
    /// </summary>
    public AsterGraphWorkbenchOptions ResetLayout()
        => Default;

    /// <summary>
    /// Names the host-facing layout preset used to compose stock workbench panels.
    /// </summary>
    public AsterGraphWorkbenchLayoutPreset LayoutPreset { get; init; } = AsterGraphWorkbenchLayoutPreset.Authoring;

    /// <summary>
    /// Captures host-owned panel visibility and sizing preferences for the stock workbench.
    /// </summary>
    public AsterGraphWorkbenchPanelState PanelState { get; init; } = AsterGraphWorkbenchPanelState.Default;

    /// <summary>
    /// Shows the header chrome that hosts the stock toolbar and command-palette entry.
    /// </summary>
    public bool ShowHeaderChrome { get; init; } = true;

    /// <summary>
    /// Shows the node palette / stencil chrome.
    /// </summary>
    public bool ShowNodePalette { get; init; } = true;

    /// <summary>
    /// Shows the inspector chrome, including validation, fragment, authoring, and mini-map sections.
    /// </summary>
    public bool ShowInspector { get; init; } = true;

    /// <summary>
    /// Shows the status chrome used for host diagnostics and validation summaries.
    /// </summary>
    public bool ShowStatus { get; init; } = true;

    /// <summary>
    /// Enables default wheel zoom and pan gestures on the hosted canvas.
    /// </summary>
    public bool EnableDefaultWheelViewportGestures { get; init; } = true;

    /// <summary>
    /// Enables Alt+left-drag panning on the hosted canvas.
    /// </summary>
    public bool EnableAltLeftDragPanning { get; init; } = true;

    /// <summary>
    /// Selects the stock hosted projection policy. This is Avalonia workbench chrome behavior, not a runtime contract.
    /// </summary>
    public AsterGraphWorkbenchPerformanceMode PerformanceMode { get; init; } = AsterGraphWorkbenchPerformanceMode.Balanced;

    /// <summary>
    /// Gets the projection policy derived from <see cref="PerformanceMode" />.
    /// </summary>
    public AsterGraphWorkbenchPerformancePolicy PerformancePolicy
        => AsterGraphWorkbenchPerformancePolicy.FromMode(PerformanceMode);
}

/// <summary>
/// Stock hosted workbench layouts that hosts can persist without changing runtime ownership.
/// </summary>
public enum AsterGraphWorkbenchLayoutPreset
{
    /// <summary>
    /// Full authoring layout with stencil, inspector, mini-map, export, runtime, and plugin panels visible.
    /// </summary>
    Authoring,

    /// <summary>
    /// Debugging layout that emphasizes runtime and diagnostic panels.
    /// </summary>
    Debugging,

    /// <summary>
    /// Compact review layout for narrow shells or read-heavy review sessions.
    /// </summary>
    CompactReview,

    /// <summary>
    /// Plugin inspection layout that emphasizes plugin and trust evidence panels.
    /// </summary>
    PluginInspection,
}

/// <summary>
/// Host-owned persisted state for stock hosted workbench panels.
/// </summary>
public sealed record AsterGraphWorkbenchPanelState(
    bool StencilVisible = true,
    double StencilWidth = 260,
    bool InspectorVisible = true,
    double InspectorWidth = 340,
    bool MiniMapVisible = true,
    bool RuntimePanelVisible = true,
    bool ExportPanelVisible = true,
    bool PluginPanelVisible = true)
{
    /// <summary>
    /// Gets the default panel visibility and sizing state.
    /// </summary>
    public static AsterGraphWorkbenchPanelState Default { get; } = new();

    /// <summary>
    /// Creates panel state for a stock hosted layout preset.
    /// </summary>
    public static AsterGraphWorkbenchPanelState ForPreset(AsterGraphWorkbenchLayoutPreset preset)
        => preset switch
        {
            AsterGraphWorkbenchLayoutPreset.Debugging => Default with
            {
                StencilVisible = false,
                RuntimePanelVisible = true,
                ExportPanelVisible = false,
                PluginPanelVisible = false,
            },
            AsterGraphWorkbenchLayoutPreset.CompactReview => Default with
            {
                StencilVisible = false,
                StencilWidth = 220,
                InspectorWidth = 300,
                MiniMapVisible = false,
                RuntimePanelVisible = false,
                ExportPanelVisible = false,
                PluginPanelVisible = false,
            },
            AsterGraphWorkbenchLayoutPreset.PluginInspection => Default with
            {
                StencilVisible = true,
                RuntimePanelVisible = false,
                ExportPanelVisible = false,
                PluginPanelVisible = true,
            },
            _ => Default,
        };
}
