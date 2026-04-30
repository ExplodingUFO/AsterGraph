using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public bool TryFocusGraphNode(string nodeId, string? scopeId = null, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!string.IsNullOrWhiteSpace(scopeId) && !TryNavigateToValidationScope(scopeId))
        {
            return false;
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        if (document.Nodes.All(node => !string.Equals(node.Id, nodeId, StringComparison.Ordinal)))
        {
            return false;
        }

        _host.SetSelection([nodeId], nodeId, updateStatus);
        _host.CenterViewOnNode(nodeId);
        PublishCommandExecuted("viewport.focus-node");
        return true;
    }

    public bool TryFocusGraphItemSearchResult(GraphEditorGraphItemSearchResultSnapshot result, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(result);

        var focused = result.Kind switch
        {
            GraphEditorGraphItemSearchResultKind.Node when !string.IsNullOrWhiteSpace(result.NodeId) =>
                TryFocusGraphNode(result.NodeId, result.ScopeId, updateStatus),
            GraphEditorGraphItemSearchResultKind.Issue =>
                TryFocusMatchingValidationIssue(result, updateStatus),
            GraphEditorGraphItemSearchResultKind.Connection when !string.IsNullOrWhiteSpace(result.ConnectionId) =>
                TryFocusConnection(result.ScopeId, result.ConnectionId, updateStatus),
            GraphEditorGraphItemSearchResultKind.Scope =>
                TryNavigateToValidationScope(result.ScopeId) && FocusCurrentScopeFromSearch(updateStatus),
            GraphEditorGraphItemSearchResultKind.Group when !string.IsNullOrWhiteSpace(result.GroupId) =>
                TryFocusGroup(result.ScopeId, result.GroupId, updateStatus),
            _ => false,
        };
        if (focused)
        {
            PublishCommandExecuted("viewport.focus-search-result");
        }

        return focused;
    }

    public bool TryAddViewportBookmark(string bookmarkId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookmarkId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (_viewportBookmarks.Any(bookmark => string.Equals(bookmark.Id, bookmarkId, StringComparison.Ordinal)))
        {
            return false;
        }

        var viewport = _host.GetViewportSnapshot();
        var scopeId = _host.GetScopeNavigationSnapshot().CurrentScopeId;
        _viewportBookmarks.Add(new GraphEditorViewportBookmarkSnapshot(
            bookmarkId.Trim(),
            title.Trim(),
            scopeId,
            viewport.Zoom,
            viewport.PanX,
            viewport.PanY));
        PublishCommandExecuted("viewport.bookmark.add");
        return true;
    }

    public bool TryRemoveViewportBookmark(string bookmarkId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookmarkId);

        var removed = _viewportBookmarks.RemoveAll(bookmark => string.Equals(bookmark.Id, bookmarkId, StringComparison.Ordinal)) > 0;
        if (removed)
        {
            PublishCommandExecuted("viewport.bookmark.remove");
        }

        return removed;
    }

    public bool TryActivateViewportBookmark(string bookmarkId, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookmarkId);

        var bookmark = _viewportBookmarks.FirstOrDefault(candidate => string.Equals(candidate.Id, bookmarkId, StringComparison.Ordinal));
        if (bookmark is null || !TryNavigateToValidationScope(bookmark.ScopeId))
        {
            return false;
        }

        var current = _host.GetViewportSnapshot();
        if (current.Zoom > 0 && bookmark.Zoom > 0 && Math.Abs(current.Zoom - bookmark.Zoom) > 0.0001d)
        {
            _host.ZoomAt(bookmark.Zoom / current.Zoom, new GraphPoint(0, 0));
        }

        current = _host.GetViewportSnapshot();
        _host.PanBy(bookmark.PanX - current.PanX, bookmark.PanY - current.PanY);
        PublishCommandExecuted("viewport.bookmark.activate");
        return true;
    }

    private bool TryExecuteNavigationCommand(GraphEditorCommandInvocationSnapshot command)
    {
        switch (command.CommandId)
        {
            case "viewport.focus-node":
                return command.TryGetArgument("nodeId", out var nodeId)
                    && !string.IsNullOrWhiteSpace(nodeId)
                    && TryFocusGraphNode(
                        nodeId,
                        command.TryGetArgument("scopeId", out var nodeScopeId) ? nodeScopeId : null,
                        ResolveCommandBool(command, "updateStatus", defaultValue: true));

            case "viewport.focus-issue":
                return TryFocusIssueCommand(command);

            case "viewport.focus-search-result":
                return command.TryGetArgument("resultId", out var resultId)
                    && !string.IsNullOrWhiteSpace(resultId)
                    && TryFocusSearchResultById(
                        resultId,
                        ResolveCommandBool(command, "updateStatus", defaultValue: true));

            case "viewport.bookmark.add":
                return command.TryGetArgument("bookmarkId", out var bookmarkId)
                    && command.TryGetArgument("title", out var title)
                    && !string.IsNullOrWhiteSpace(bookmarkId)
                    && !string.IsNullOrWhiteSpace(title)
                    && TryAddViewportBookmark(bookmarkId, title);

            case "viewport.bookmark.remove":
                return command.TryGetArgument("bookmarkId", out var removeBookmarkId)
                    && !string.IsNullOrWhiteSpace(removeBookmarkId)
                    && TryRemoveViewportBookmark(removeBookmarkId);

            case "viewport.bookmark.activate":
                return command.TryGetArgument("bookmarkId", out var activateBookmarkId)
                    && !string.IsNullOrWhiteSpace(activateBookmarkId)
                    && TryActivateViewportBookmark(
                        activateBookmarkId,
                        ResolveCommandBool(command, "updateStatus", defaultValue: true));

            default:
                return false;
        }
    }

    private bool TryFocusIssueCommand(GraphEditorCommandInvocationSnapshot command)
    {
        if (!command.TryGetArgument("issueCode", out var issueCode) || string.IsNullOrWhiteSpace(issueCode))
        {
            return false;
        }

        var issues = GetValidationSnapshot().Issues
            .Where(issue => string.Equals(issue.Code, issueCode, StringComparison.Ordinal));
        if (command.TryGetArgument("scopeId", out var scopeId) && !string.IsNullOrWhiteSpace(scopeId))
        {
            issues = issues.Where(issue => string.Equals(issue.ScopeId, scopeId, StringComparison.Ordinal));
        }

        if (command.TryGetArgument("nodeId", out var nodeId) && !string.IsNullOrWhiteSpace(nodeId))
        {
            issues = issues.Where(issue => string.Equals(issue.NodeId, nodeId, StringComparison.Ordinal));
        }

        if (command.TryGetArgument("connectionId", out var connectionId) && !string.IsNullOrWhiteSpace(connectionId))
        {
            issues = issues.Where(issue => string.Equals(issue.ConnectionId, connectionId, StringComparison.Ordinal));
        }

        var issue = issues.FirstOrDefault();
        return issue is not null && TryFocusValidationIssue(issue, ResolveCommandBool(command, "updateStatus", defaultValue: true));
    }

    private bool TryFocusSearchResultById(string resultId, bool updateStatus)
    {
        var result = SearchGraphItems().Results.FirstOrDefault(candidate => string.Equals(candidate.Id, resultId, StringComparison.Ordinal));
        return result is not null && TryFocusGraphItemSearchResult(result, updateStatus);
    }

    private bool TryFocusMatchingValidationIssue(GraphEditorGraphItemSearchResultSnapshot result, bool updateStatus)
    {
        var issue = GetValidationSnapshot().Issues.FirstOrDefault(candidate =>
            string.Equals(candidate.Code, result.IssueCode, StringComparison.Ordinal)
            && string.Equals(candidate.ScopeId, result.ScopeId, StringComparison.Ordinal)
            && string.Equals(candidate.NodeId, result.NodeId, StringComparison.Ordinal)
            && string.Equals(candidate.ConnectionId, result.ConnectionId, StringComparison.Ordinal));
        return issue is not null && TryFocusValidationIssue(issue, updateStatus);
    }

    private bool TryFocusConnection(string scopeId, string connectionId, bool updateStatus)
    {
        if (!TryNavigateToValidationScope(scopeId))
        {
            return false;
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        if (document.Connections.All(connection => !string.Equals(connection.Id, connectionId, StringComparison.Ordinal)))
        {
            return false;
        }

        _host.SetConnectionSelection([connectionId], connectionId, updateStatus);
        if (TryResolveConnectionFocusPoint(document, connectionId, out var focusPoint))
        {
            _host.CenterViewAt(focusPoint, updateStatus);
        }

        return true;
    }

    private bool TryFocusGroup(string scopeId, string groupId, bool updateStatus)
    {
        if (!TryNavigateToValidationScope(scopeId))
        {
            return false;
        }

        var group = _host.CreateActiveScopeDocumentSnapshot()
            .Groups?
            .FirstOrDefault(candidate => string.Equals(candidate.Id, groupId, StringComparison.Ordinal));
        if (group is null)
        {
            return false;
        }

        _host.SetSelection(group.NodeIds, group.NodeIds.FirstOrDefault(), updateStatus);
        _host.CenterViewAt(new GraphPoint(group.Position.X + group.Size.Width / 2d, group.Position.Y + group.Size.Height / 2d), updateStatus);
        return true;
    }

    private bool FocusCurrentScopeFromSearch(bool updateStatus)
    {
        _host.FocusCurrentScope(updateStatus);
        return true;
    }

    private static bool ResolveCommandBool(GraphEditorCommandInvocationSnapshot command, string name, bool defaultValue)
        => !command.TryGetArgument(name, out var value)
            ? defaultValue
            : bool.TryParse(value, out var parsed) ? parsed : defaultValue;

}
