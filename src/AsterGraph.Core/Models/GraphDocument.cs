using System;
using System.Linq;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted graph document snapshot.
/// Root nodes, connections, and groups remain the compatibility surface.
/// Child graph scopes are stored separately and composed back into <see cref="GraphScopes"/>.
/// </summary>
public sealed record GraphDocument
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private IReadOnlyList<GraphNode> _nodes = [];
    private IReadOnlyList<GraphConnection> _connections = [];
    private IReadOnlyList<GraphNodeGroup>? _groups;
    private string _rootGraphId = DefaultRootGraphId;
    private readonly IReadOnlyList<GraphScope> _childGraphScopes;

    public const string DefaultRootGraphId = "graph-root";

    /// <summary>
    /// Retained constructor shape for legacy hosts and plugins that target the original root-only document model.
    /// </summary>
    public GraphDocument(
        string Title,
        string Description,
        IReadOnlyList<GraphNode> Nodes,
        IReadOnlyList<GraphConnection> Connections,
        IReadOnlyList<GraphNodeGroup>? Groups = null)
        : this(Title, Description, Nodes, Connections, Groups, DefaultRootGraphId, null)
    {
    }

    /// <summary>
    /// Creates a graph document snapshot.
    /// </summary>
    public GraphDocument(
        string title,
        string description,
        IReadOnlyList<GraphNode> nodes,
        IReadOnlyList<GraphConnection> connections,
        IReadOnlyList<GraphNodeGroup>? groups,
        string rootGraphId,
        IReadOnlyList<GraphScope>? graphScopes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);

        Title = title;
        Description = description;
        Nodes = CloneNodes(nodes);
        Connections = CloneConnections(connections);
        Groups = groups is null ? null : CloneGroups(groups);
        RootGraphId = NormalizeRootGraphId(rootGraphId);
        _childGraphScopes = NormalizeChildGraphScopes(graphScopes, RootGraphId);
    }

    /// <summary>
    /// Document title shown to hosts and end users.
    /// </summary>
    public string Title
    {
        get => _title;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _title = value;
        }
    }

    /// <summary>
    /// Optional document description or summary.
    /// </summary>
    public string Description
    {
        get => _description;
        init => _description = value;
    }

    /// <summary>
    /// All nodes contained in the root graph.
    /// </summary>
    public IReadOnlyList<GraphNode> Nodes
    {
        get => _nodes;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _nodes = CloneNodes(value);
        }
    }

    /// <summary>
    /// All connections contained in the root graph.
    /// </summary>
    public IReadOnlyList<GraphConnection> Connections
    {
        get => _connections;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _connections = CloneConnections(value);
        }
    }

    /// <summary>
    /// Optional editor-only node groups persisted with the root graph.
    /// </summary>
    public IReadOnlyList<GraphNodeGroup>? Groups
    {
        get => _groups;
        init => _groups = value is null ? null : CloneGroups(value);
    }

    /// <summary>
    /// Stable identifier for the root graph scope.
    /// </summary>
    public string RootGraphId
    {
        get => _rootGraphId;
        init => _rootGraphId = NormalizeRootGraphId(value);
    }

    /// <summary>
    /// All graph scopes in the document. The root scope is synthesized from the top-level root fields.
    /// </summary>
    public IReadOnlyList<GraphScope> GraphScopes
        => BuildGraphScopesSnapshot();

    public static GraphDocument CreateScoped(
        string title,
        string description,
        string rootGraphId,
        IReadOnlyList<GraphScope> graphScopes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(graphScopes);

        var normalizedRootGraphId = NormalizeRootGraphId(rootGraphId);
        var normalizedScopes = CloneScopes(graphScopes);
        var rootScope = normalizedScopes.FirstOrDefault(scope => string.Equals(scope.Id, normalizedRootGraphId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Scoped graph document requires a root scope with id '{normalizedRootGraphId}'.");

        return new GraphDocument(
            title,
            description,
            rootScope.Nodes,
            rootScope.Connections,
            rootScope.Groups,
            normalizedRootGraphId,
            normalizedScopes);
    }

    public IReadOnlyList<GraphScope> GetGraphScopes()
        => GraphScopes;

    public GraphScope GetRootGraphScope()
        => GraphScopes.First(scope => string.Equals(scope.Id, RootGraphId, StringComparison.Ordinal));

    public GraphDocument WithRootGraphContents(
        IReadOnlyList<GraphNode> nodes,
        IReadOnlyList<GraphConnection> connections,
        IReadOnlyList<GraphNodeGroup>? groups = null)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);

        return new GraphDocument(
            Title,
            Description,
            nodes,
            connections,
            groups ?? Groups,
            RootGraphId,
            GraphScopes);
    }

    /// <summary>
    /// Retained positional deconstruction contract for legacy hosts and plugins.
    /// </summary>
    public void Deconstruct(
        out string title,
        out string description,
        out IReadOnlyList<GraphNode> nodes,
        out IReadOnlyList<GraphConnection> connections,
        out IReadOnlyList<GraphNodeGroup>? groups)
    {
        title = Title;
        description = Description;
        nodes = Nodes;
        connections = Connections;
        groups = Groups;
    }

    private IReadOnlyList<GraphScope> BuildGraphScopesSnapshot()
    {
        var rootScope = new GraphScope(
            RootGraphId,
            CloneNodes(Nodes),
            CloneConnections(Connections),
            Groups is null ? null : CloneGroups(Groups));
        if (_childGraphScopes.Count == 0)
        {
            return [rootScope];
        }

        var childScopes = _childGraphScopes
            .Where(scope => !string.Equals(scope.Id, RootGraphId, StringComparison.Ordinal))
            .ToList();
        return childScopes.Count == 0
            ? [rootScope]
            : [rootScope, .. CloneScopes(childScopes)];
    }

    private static IReadOnlyList<GraphScope> NormalizeChildGraphScopes(
        IReadOnlyList<GraphScope>? graphScopes,
        string rootGraphId)
    {
        if (graphScopes is not { Count: > 0 })
        {
            return [];
        }

        return CloneScopes(
            graphScopes.Where(scope => !string.Equals(scope.Id, rootGraphId, StringComparison.Ordinal)).ToList());
    }

    private static string NormalizeRootGraphId(string? rootGraphId)
        => string.IsNullOrWhiteSpace(rootGraphId)
            ? DefaultRootGraphId
            : rootGraphId;

    private static List<GraphScope> CloneScopes(IReadOnlyList<GraphScope> scopes)
        => scopes
            .Select(CloneScope)
            .ToList();

    private static GraphScope CloneScope(GraphScope scope)
        => new(
            scope.Id,
            CloneNodes(scope.Nodes),
            CloneConnections(scope.Connections),
            scope.Groups is null ? null : CloneGroups(scope.Groups));

    private static List<GraphNode> CloneNodes(IReadOnlyList<GraphNode> nodes)
        => nodes
            .Select(CloneNode)
            .ToList();

    private static GraphNode CloneNode(GraphNode node)
        => new(
            node.Id,
            node.Title,
            node.Category,
            node.Subtitle,
            node.Description,
            node.Position,
            node.Size,
            ClonePorts(node.Inputs),
            ClonePorts(node.Outputs),
            node.AccentHex,
            node.DefinitionId,
            node.ParameterValues is null ? null : CloneParameterValues(node.ParameterValues),
            node.Surface is null ? null : node.Surface with { },
            node.Composite is null ? null : CloneComposite(node.Composite));

    private static List<GraphPort> ClonePorts(IReadOnlyList<GraphPort> ports)
        => ports
            .Select(port => new GraphPort(
                port.Id,
                port.Label,
                port.Direction,
                port.DataType,
                port.AccentHex,
                port.TypeId,
                port.GroupName,
                port.MinConnections,
                port.MaxConnections))
            .ToList();

    private static List<GraphParameterValue> CloneParameterValues(IReadOnlyList<GraphParameterValue> parameterValues)
        => parameterValues
            .Select(parameter => new GraphParameterValue(parameter.Key, parameter.TypeId, parameter.Value))
            .ToList();

    private static List<GraphConnection> CloneConnections(IReadOnlyList<GraphConnection> connections)
        => connections
            .Select(CloneConnection)
            .ToList();

    private static GraphConnection CloneConnection(GraphConnection connection)
        => new(
            connection.Id,
            connection.SourceNodeId,
            connection.SourcePortId,
            connection.TargetNodeId,
            connection.TargetPortId,
            connection.Label,
            connection.AccentHex,
            connection.ConversionId,
            connection.Presentation is null ? null : CloneEdgePresentation(connection.Presentation))
        {
            TargetKind = connection.TargetKind,
        };

    private static GraphEdgePresentation CloneEdgePresentation(GraphEdgePresentation presentation)
        => new(
            presentation.NoteText,
            presentation.Route is null
                ? null
                : new GraphConnectionRoute(presentation.Route.Vertices));

    private static List<GraphNodeGroup> CloneGroups(IReadOnlyList<GraphNodeGroup> groups)
        => groups
            .Select(group => new GraphNodeGroup(
                group.Id,
                group.Title,
                group.Position,
                group.Size,
                group.NodeIds.ToList(),
                group.IsCollapsed,
                group.ExtraPadding))
            .ToList();

    private static GraphCompositeNode CloneComposite(GraphCompositeNode composite)
        => new(
            composite.ChildGraphId,
            composite.Inputs?.Select(CloneBoundaryPort).ToList() ?? [],
            composite.Outputs?.Select(CloneBoundaryPort).ToList() ?? []);

    private static GraphCompositeBoundaryPort CloneBoundaryPort(GraphCompositeBoundaryPort port)
        => new(
            port.Id,
            port.Label,
            port.Direction,
            port.DataType,
            port.AccentHex,
            port.ChildNodeId,
            port.ChildPortId,
            port.TypeId);
}
