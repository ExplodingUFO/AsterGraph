namespace AsterGraph.Core.Models;

/// <summary>
/// Optional persisted edge presentation metadata owned by hosts.
/// </summary>
/// <param name="NoteText">Host-visible note text rendered alongside the edge.</param>
/// <param name="Route">Host-owned bend-point route persisted with the connection.</param>
/// <param name="PathKind">Host-requested path shape for the edge.</param>
/// <param name="IsAnimated">Whether the stock renderer should render the edge as animated.</param>
/// <param name="UsesFloatingEndpoints">Whether endpoints should float on node bounds instead of fixed port anchors.</param>
/// <param name="IsReconnectable">Whether host UI should expose reconnect affordances for this edge.</param>
/// <param name="IsEditable">Whether host UI should expose label/note editing affordances for this edge.</param>
/// <param name="SourceMarker">Optional source endpoint marker.</param>
/// <param name="TargetMarker">Optional target endpoint marker.</param>
public sealed record GraphEdgePresentation(
    string? NoteText = null,
    GraphConnectionRoute? Route = null,
    GraphEdgePathKind PathKind = GraphEdgePathKind.Auto,
    bool IsAnimated = false,
    bool UsesFloatingEndpoints = false,
    bool IsReconnectable = true,
    bool IsEditable = true,
    GraphEdgeMarkerKind SourceMarker = GraphEdgeMarkerKind.None,
    GraphEdgeMarkerKind TargetMarker = GraphEdgeMarkerKind.None);
