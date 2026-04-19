using System.Text.Json;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

/// <summary>
/// 管理图文档 JSON 契约版本与兼容读取。
/// </summary>
internal static class GraphDocumentCompatibility
{
    public const int CurrentSchemaVersion = 3;

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

        var normalizedNodes = document.Nodes
            .Select(node => node with { Surface = node.Surface ?? GraphNodeSurfaceState.Default })
            .ToList();

        return document with
        {
            Nodes = normalizedNodes,
            Groups = NormalizeGroups(document.Groups, normalizedNodes, schemaVersion),
        };
    }

    private static List<GraphNodeGroup> NormalizeGroups(
        IReadOnlyList<GraphNodeGroup>? groups,
        IReadOnlyList<GraphNode> nodes,
        int? schemaVersion)
    {
        var normalizedGroups = (groups ?? [])
            .Select(group => NormalizeGroup(group, nodes, schemaVersion))
            .ToList();
        return normalizedGroups;
    }

    private static GraphNodeGroup NormalizeGroup(
        GraphNodeGroup group,
        IReadOnlyList<GraphNode> nodes,
        int? schemaVersion)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(nodes);

        var nodeBounds = nodes
            .Where(node => group.NodeIds.Contains(node.Id, StringComparer.Ordinal))
            .ToList();
        if (nodeBounds.Count == 0)
        {
            return group with
            {
                NodeIds = group.NodeIds.ToList(),
                ExtraPadding = group.ExtraPadding.ClampNonNegative(),
            };
        }

        var contentLeft = nodeBounds.Min(node => node.Position.X);
        var contentTop = nodeBounds.Min(node => node.Position.Y);
        var contentRight = nodeBounds.Max(node => node.Position.X + node.Size.Width);
        var contentBottom = nodeBounds.Max(node => node.Position.Y + node.Size.Height);

        var padding = schemaVersion is null or < 3
            ? new GraphPadding(
                contentLeft - group.Position.X,
                contentTop - group.Position.Y,
                (group.Position.X + group.Size.Width) - contentRight,
                (group.Position.Y + group.Size.Height) - contentBottom).ClampNonNegative()
            : group.ExtraPadding.ClampNonNegative();

        var position = new GraphPoint(contentLeft - padding.Left, contentTop - padding.Top);
        var size = new GraphSize(
            (contentRight - contentLeft) + padding.Left + padding.Right,
            (contentBottom - contentTop) + padding.Top + padding.Bottom);

        return group with
        {
            Position = position,
            Size = size,
            NodeIds = group.NodeIds.ToList(),
            ExtraPadding = padding,
        };
    }
}
