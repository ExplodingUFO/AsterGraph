using System.Text.Json;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

/// <summary>
/// 管理图文档 JSON 契约版本与兼容读取。
/// </summary>
internal static class GraphDocumentCompatibility
{
    public const int CurrentSchemaVersion = 4;

    public static GraphDocumentSerializer.GraphDocumentFilePayload CreatePayload(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new GraphDocumentSerializer.GraphDocumentFilePayload(
            CurrentSchemaVersion,
            document.Title,
            document.Description,
            document.Nodes,
            document.Connections,
            document.Groups);
    }

    public static GraphDocument Deserialize(string json, JsonSerializerOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(options);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty(nameof(GraphDocumentSerializer.GraphDocumentFilePayload.SchemaVersion), out var versionElement))
        {
            var legacy = JsonSerializer.Deserialize<GraphDocument>(json, options);
            return NormalizeDocument(legacy ?? throw new InvalidOperationException("Failed to deserialize legacy graph document."), null);
        }

        var schemaVersion = versionElement.GetInt32();
        if (schemaVersion is < 1 or > CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported graph document schema version '{schemaVersion}'. Current version is '{CurrentSchemaVersion}'.");
        }

        var payload = JsonSerializer.Deserialize<GraphDocumentSerializer.GraphDocumentFilePayload>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize versioned graph document.");
        return NormalizeDocument(payload.ToDocument(), schemaVersion);
    }

    private static GraphDocument NormalizeDocument(GraphDocument document, int? schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(document);

        var groups = document.Groups ?? [];
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

        var normalizedNodes = document.Nodes
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

        return document with
        {
            Nodes = normalizedNodes,
            Groups = NormalizeGroups(groups, normalizedNodes, schemaVersion),
        };
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
}
