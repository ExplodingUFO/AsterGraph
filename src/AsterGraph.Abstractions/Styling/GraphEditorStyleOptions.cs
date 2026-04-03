namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Root framework-neutral style contract for the shipped graph editor surfaces.
/// </summary>
public sealed record GraphEditorStyleOptions
{
    /// <summary>
    /// Default style token set used when the host does not provide overrides.
    /// </summary>
    public static GraphEditorStyleOptions Default { get; } = new();

    /// <summary>
    /// Shared shell-level tokens.
    /// </summary>
    public ShellStyleOptions Shell { get; init; } = new();

    /// <summary>
    /// Canvas-level tokens.
    /// </summary>
    public CanvasStyleOptions Canvas { get; init; } = new();

    /// <summary>
    /// Node card tokens.
    /// </summary>
    public NodeCardStyleOptions NodeCard { get; init; } = new();

    /// <summary>
    /// Port row tokens.
    /// </summary>
    public PortStyleOptions Port { get; init; } = new();

    /// <summary>
    /// Connection rendering tokens.
    /// </summary>
    public ConnectionStyleOptions Connection { get; init; } = new();

    /// <summary>
    /// Inspector surface tokens.
    /// </summary>
    public InspectorStyleOptions Inspector { get; init; } = new();

    /// <summary>
    /// Context menu tokens.
    /// </summary>
    public ContextMenuStyleOptions ContextMenu { get; init; } = new();

    /// <summary>
    /// Per-node style overrides keyed by node definition.
    /// </summary>
    public IReadOnlyList<NodeStyleOverride> NodeOverrides { get; init; } = [];

    /// <summary>
    /// Per-port style overrides keyed by port type.
    /// </summary>
    public IReadOnlyList<PortStyleOverride> PortOverrides { get; init; } = [];

    /// <summary>
    /// Per-connection style overrides keyed by conversion identifier.
    /// </summary>
    public IReadOnlyList<ConnectionStyleOverride> ConnectionOverrides { get; init; } = [];
}
