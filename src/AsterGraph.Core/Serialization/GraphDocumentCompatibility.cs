using System.Text.Json;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

/// <summary>
/// 管理图文档 JSON 契约版本与兼容读取。
/// </summary>
internal static class GraphDocumentCompatibility
{
    public const int CurrentSchemaVersion = 5;

    public static GraphDocumentSerializer.GraphDocumentFilePayload CreatePayload(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var graphScopes = document.GetGraphScopes();
        return new GraphDocumentSerializer.GraphDocumentFilePayload(
            CurrentSchemaVersion,
            document.Title,
            document.Description,
            document.RootGraphId,
            graphScopes);
    }

    public static GraphDocument Deserialize(string json, JsonSerializerOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(options);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty(nameof(GraphDocumentSerializer.GraphDocumentFilePayload.SchemaVersion), out var versionElement))
        {
            var legacyDocumentPayload = JsonSerializer.Deserialize<LegacyUnversionedGraphDocumentPayload>(json, options);
            return NormalizeDocument(
                legacyDocumentPayload?.ToDocument() ?? throw new InvalidOperationException("Failed to deserialize legacy graph document."),
                null);
        }

        var schemaVersion = versionElement.GetInt32();
        if (schemaVersion is < 1 or > CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported graph document schema version '{schemaVersion}'. Current version is '{CurrentSchemaVersion}'.");
        }

        if (schemaVersion == CurrentSchemaVersion)
        {
            var payload = JsonSerializer.Deserialize<GraphDocumentSerializer.GraphDocumentFilePayload>(json, options)
                ?? throw new InvalidOperationException("Failed to deserialize scoped graph document.");
            return NormalizeDocument(payload.ToDocument(), schemaVersion);
        }

        var legacyPayload = JsonSerializer.Deserialize<LegacyGraphDocumentFilePayload>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize versioned graph document.");
        return NormalizeDocument(legacyPayload.ToDocument(), schemaVersion);
    }

    private static GraphDocument NormalizeDocument(GraphDocument document, int? schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(document);

        var rootGraphId = string.IsNullOrWhiteSpace(document.RootGraphId)
            ? GraphDocument.DefaultRootGraphId
            : document.RootGraphId;
        var normalizedScopes = document.GetGraphScopes()
            .Select(scope => NormalizeScope(scope, schemaVersion))
            .ToList();

        return GraphDocument.CreateScoped(
            document.Title,
            document.Description,
            rootGraphId,
            normalizedScopes);
    }

    private static GraphScope NormalizeScope(GraphScope scope, int? schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var groups = scope.Groups ?? [];
        var validGroupIds = groups
            .Select(group => group.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        var fallbackMembershipByNodeId = groups
            .SelectMany(group => group.NodeIds
                .Where(nodeId => !string.IsNullOrWhiteSpace(nodeId))
                .Select(nodeId => new KeyValuePair<string, string>(nodeId, group.Id)))
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

        var normalizedNodes = scope.Nodes
            .Select(node => node with { Surface = node.Surface ?? GraphNodeSurfaceState.Default })
            .Select(node =>
            {
                var currentGroupId = node.Surface?.GroupId;
                if (!string.IsNullOrWhiteSpace(currentGroupId) && validGroupIds.Contains(currentGroupId))
                {
                    return node;
                }

                if (fallbackMembershipByNodeId.TryGetValue(node.Id, out var fallbackGroupId)
                    && validGroupIds.Contains(fallbackGroupId))
                {
                    return node with
                    {
                        Surface = (node.Surface ?? GraphNodeSurfaceState.Default) with { GroupId = fallbackGroupId },
                    };
                }

                return node with
                {
                    Surface = (node.Surface ?? GraphNodeSurfaceState.Default) with { GroupId = null },
                };
            })
            .ToList();

        return new GraphScope(
            scope.Id,
            normalizedNodes,
            scope.Connections.ToList(),
            NormalizeGroups(groups, normalizedNodes, schemaVersion));
    }

    private static List<GraphNodeGroup> NormalizeGroups(
        IReadOnlyList<GraphNodeGroup> groups,
        IReadOnlyList<GraphNode> nodes,
        int? schemaVersion)
        => groups
            .Select(group => NormalizeGroup(group, nodes, schemaVersion))
            .ToList();

    private static GraphNodeGroup NormalizeGroup(
        GraphNodeGroup group,
        IReadOnlyList<GraphNode> nodes,
        int? schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(nodes);

        var padding = schemaVersion is null or < 3
            ? new GraphPadding(24d, 44d, 24d, 28d)
            : group.ExtraPadding.ClampNonNegative();
        var nodeIds = nodes
            .Where(node => string.Equals(node.Surface?.GroupId, group.Id, StringComparison.Ordinal))
            .Select(node => node.Id)
            .ToList();

        return group with
        {
            Position = group.Position,
            Size = group.Size,
            NodeIds = nodeIds,
            ExtraPadding = padding,
        };
    }

    private sealed record LegacyGraphDocumentFilePayload(
        int SchemaVersion,
        string Title,
        string Description,
        IReadOnlyList<GraphNode> Nodes,
        IReadOnlyList<GraphConnection> Connections,
        IReadOnlyList<GraphNodeGroup>? Groups = null)
    {
        public GraphDocument ToDocument()
            => new(Title, Description, Nodes, Connections, Groups);
    }

    private sealed record LegacyUnversionedGraphDocumentPayload(
        string Title,
        string Description,
        IReadOnlyList<GraphNode> Nodes,
        IReadOnlyList<GraphConnection> Connections,
        IReadOnlyList<GraphNodeGroup>? Groups = null)
    {
        public GraphDocument ToDocument()
            => new(Title, Description, Nodes, Connections, Groups);
    }
}
