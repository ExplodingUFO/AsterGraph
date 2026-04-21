using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable adapter-neutral snapshot of the current editor scene.
/// </summary>
/// <param name="Document">Current active-scope graph document snapshot.</param>
/// <param name="Selection">Current selection snapshot.</param>
/// <param name="Viewport">Current viewport snapshot.</param>
/// <param name="NodeSurfaces">Resolved node-surface snapshots for the scene.</param>
/// <param name="NodeGroups">Resolved node-group snapshots for the scene.</param>
/// <param name="ConnectionGeometries">Resolved committed connection geometry snapshots for the scene.</param>
/// <param name="PendingConnection">Current pending-connection preview snapshot.</param>
public sealed record GraphEditorSceneSnapshot(
    GraphDocument Document,
    GraphEditorSelectionSnapshot Selection,
    GraphEditorViewportSnapshot Viewport,
    IReadOnlyList<GraphEditorNodeSurfaceSnapshot> NodeSurfaces,
    IReadOnlyList<GraphEditorNodeGroupSnapshot> NodeGroups,
    IReadOnlyList<GraphEditorConnectionGeometrySnapshot> ConnectionGeometries,
    GraphEditorPendingConnectionSnapshot PendingConnection);
