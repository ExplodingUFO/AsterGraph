using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// Aggregates retained host platform services that can be projected onto the hosted editor facade.
/// </summary>
public sealed record GraphEditorPlatformServices
{
    /// <summary>
    /// Optional host-provided plain-text clipboard bridge.
    /// </summary>
    public IGraphTextClipboardBridge? TextClipboardBridge { get; init; }

    /// <summary>
    /// Optional host context for menu, owner, and top-level lookups.
    /// </summary>
    public IGraphHostContext? HostContext { get; init; }
}
