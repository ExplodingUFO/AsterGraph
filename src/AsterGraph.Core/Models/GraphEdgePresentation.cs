namespace AsterGraph.Core.Models;

/// <summary>
/// Optional persisted edge presentation metadata owned by hosts.
/// </summary>
/// <param name="NoteText">Host-visible note text rendered alongside the edge.</param>
public sealed record GraphEdgePresentation(
    string? NoteText = null);
