using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime.Internal;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public bool TryFocusValidationIssue(GraphEditorValidationIssueSnapshot issue, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(issue);

        if (!TryNavigateToValidationScope(issue.ScopeId))
        {
            return false;
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        if (!string.IsNullOrWhiteSpace(issue.ConnectionId))
        {
            if (!TryFocusValidationConnection(issue, document, updateStatus))
            {
                return false;
            }

            PublishCommandExecuted("validation.focus");
            return true;
        }

        if (!string.IsNullOrWhiteSpace(issue.NodeId))
        {
            if (!TryFocusValidationNode(issue, document, updateStatus))
            {
                return false;
            }

            PublishCommandExecuted("validation.focus");
            return true;
        }

        _host.ClearSelection(updateStatus: false);
        _host.FocusCurrentScope(updateStatus);
        PublishCommandExecuted("validation.focus");
        return true;
    }

    private bool TryFocusValidationConnection(
        GraphEditorValidationIssueSnapshot issue,
        GraphDocument activeDocument,
        bool updateStatus)
    {
        if (string.IsNullOrWhiteSpace(issue.ConnectionId)
            || activeDocument.Connections.All(connection => !string.Equals(connection.Id, issue.ConnectionId, StringComparison.Ordinal)))
        {
            return false;
        }

        _host.SetConnectionSelection([issue.ConnectionId], issue.ConnectionId, updateStatus);
        if (TryResolveConnectionFocusPoint(activeDocument, issue.ConnectionId, out var focusPoint))
        {
            _host.CenterViewAt(focusPoint, updateStatus);
        }
        else
        {
            _host.FocusCurrentScope(updateStatus);
        }

        return true;
    }

    private bool TryFocusValidationNode(
        GraphEditorValidationIssueSnapshot issue,
        GraphDocument activeDocument,
        bool updateStatus)
    {
        if (string.IsNullOrWhiteSpace(issue.NodeId)
            || activeDocument.Nodes.All(node => !string.Equals(node.Id, issue.NodeId, StringComparison.Ordinal)))
        {
            return false;
        }

        _host.SetSelection([issue.NodeId], issue.NodeId, updateStatus);
        _host.CenterViewOnNode(issue.NodeId);
        return true;
    }

    private bool TryNavigateToValidationScope(string scopeId)
    {
        var currentNavigation = _host.GetScopeNavigationSnapshot();
        if (string.Equals(currentNavigation.CurrentScopeId, scopeId, StringComparison.Ordinal))
        {
            return true;
        }

        var document = _host.CreateDocumentSnapshot();
        if (document.GraphScopes.All(scope => !string.Equals(scope.Id, scopeId, StringComparison.Ordinal))
            || !TryCreateCompositePathToScope(document, scopeId, out var compositeNodePath))
        {
            return false;
        }

        while (_host.GetScopeNavigationSnapshot().CanNavigateToParent)
        {
            if (!_host.TryReturnToParentGraphScope(updateStatus: false))
            {
                return false;
            }
        }

        foreach (var compositeNodeId in compositeNodePath)
        {
            if (!_host.TryEnterCompositeChildGraph(compositeNodeId, updateStatus: false))
            {
                return false;
            }
        }

        return string.Equals(_host.GetScopeNavigationSnapshot().CurrentScopeId, scopeId, StringComparison.Ordinal);
    }

    private static bool TryCreateCompositePathToScope(
        GraphDocument document,
        string scopeId,
        out IReadOnlyList<string> compositeNodePath)
    {
        var parentsByChildScope = new Dictionary<string, (string ParentScopeId, string CompositeNodeId)>(StringComparer.Ordinal);
        foreach (var scope in document.GraphScopes)
        {
            foreach (var node in scope.Nodes)
            {
                if (node.Composite is null)
                {
                    continue;
                }

                parentsByChildScope.TryAdd(node.Composite.ChildGraphId, (scope.Id, node.Id));
            }
        }

        var path = new List<string>();
        var cursor = scopeId;
        while (!string.Equals(cursor, document.RootGraphId, StringComparison.Ordinal))
        {
            if (!parentsByChildScope.TryGetValue(cursor, out var parent))
            {
                compositeNodePath = [];
                return false;
            }

            path.Add(parent.CompositeNodeId);
            cursor = parent.ParentScopeId;
        }

        path.Reverse();
        compositeNodePath = path;
        return true;
    }

    private bool TryResolveConnectionFocusPoint(
        GraphDocument activeDocument,
        string connectionId,
        out GraphPoint focusPoint)
    {
        var geometry = GraphEditorConnectionGeometryProjector.Create(activeDocument, ResolveNodeDefinition)
            .FirstOrDefault(candidate => string.Equals(candidate.ConnectionId, connectionId, StringComparison.Ordinal));
        if (geometry is null)
        {
            focusPoint = default;
            return false;
        }

        focusPoint = CreateConnectionFocusPoint(geometry);
        return true;
    }

    private static GraphPoint CreateConnectionFocusPoint(GraphEditorConnectionGeometrySnapshot geometry)
    {
        var x = geometry.Source.Position.X + geometry.Target.Position.X;
        var y = geometry.Source.Position.Y + geometry.Target.Position.Y;
        var count = 2;
        foreach (var vertex in geometry.Route.Vertices)
        {
            x += vertex.X;
            y += vertex.Y;
            count++;
        }

        return new GraphPoint(x / count, y / count);
    }
}
