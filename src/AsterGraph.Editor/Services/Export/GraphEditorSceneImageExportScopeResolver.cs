using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Services;

internal static class GraphEditorSceneImageExportScopeResolver
{
    public static GraphEditorSceneSnapshot Resolve(
        GraphEditorSceneSnapshot scene,
        GraphEditorSceneImageExportOptions? options)
    {
        ArgumentNullException.ThrowIfNull(scene);

        return (options?.Scope ?? GraphEditorSceneImageExportScope.FullScene) switch
        {
            GraphEditorSceneImageExportScope.FullScene => scene,
            GraphEditorSceneImageExportScope.SelectedNodes => CreateSelectedNodeScene(scene),
            _ => throw new ArgumentOutOfRangeException(nameof(options), "Unsupported image export scope."),
        };
    }

    private static GraphEditorSceneSnapshot CreateSelectedNodeScene(GraphEditorSceneSnapshot scene)
    {
        var selectedNodeIds = scene.Selection.SelectedNodeIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        if (selectedNodeIds.Count == 0)
        {
            throw new InvalidOperationException("Selected-node image export requires at least one selected node.");
        }

        var nodes = scene.Document.Nodes
            .Where(node => selectedNodeIds.Contains(node.Id))
            .ToList();
        if (nodes.Count == 0)
        {
            throw new InvalidOperationException("Selected-node image export could not resolve selected nodes.");
        }

        var connections = scene.Document.Connections
            .Where(connection => selectedNodeIds.Contains(connection.SourceNodeId)
                && selectedNodeIds.Contains(connection.TargetNodeId))
            .ToList();
        var connectionIds = connections
            .Select(connection => connection.Id)
            .ToHashSet(StringComparer.Ordinal);

        var groups = scene.Document.Groups?
            .Where(group => group.NodeIds.Any(selectedNodeIds.Contains))
            .Select(group => group with
            {
                NodeIds = group.NodeIds.Where(selectedNodeIds.Contains).ToList(),
            })
            .ToList();

        var document = scene.Document.WithRootGraphContents(nodes, connections, groups);
        return scene with
        {
            Document = document,
            Selection = new GraphEditorSelectionSnapshot(
                nodes.Select(node => node.Id).ToList(),
                selectedNodeIds.Contains(scene.Selection.PrimarySelectedNodeId ?? string.Empty)
                    ? scene.Selection.PrimarySelectedNodeId
                    : nodes[0].Id),
            NodeSurfaces = scene.NodeSurfaces
                .Where(surface => selectedNodeIds.Contains(surface.NodeId))
                .ToList(),
            NodeGroups = scene.NodeGroups
                .Where(group => group.NodeIds.Any(selectedNodeIds.Contains))
                .Select(group => group with
                {
                    NodeIds = group.NodeIds.Where(selectedNodeIds.Contains).ToList(),
                })
                .ToList(),
            ConnectionGeometries = scene.ConnectionGeometries
                .Where(geometry => connectionIds.Contains(geometry.ConnectionId))
                .ToList(),
            PendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null),
        };
    }
}
