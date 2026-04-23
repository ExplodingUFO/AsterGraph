using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

internal static class AsterGraphCompositeWorkflowActionFactory
{
    private static readonly IReadOnlyList<string> WorkflowCommandIds =
    [
        "composites.wrap-selection",
        "scopes.enter",
        "scopes.exit",
    ];

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateWorkflowActions(IGraphEditorSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var descriptors = session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var selection = session.Queries.GetSelectionSnapshot();
        var composites = session.Queries.GetCompositeNodeSnapshots()
            .ToDictionary(snapshot => snapshot.NodeId, StringComparer.Ordinal);
        return CreateWorkflowActions(session, descriptors, selection, composites);
    }

    internal static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateWorkflowActions(
        IGraphEditorSession session,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> descriptors,
        GraphEditorSelectionSnapshot selection,
        IReadOnlyDictionary<string, GraphEditorCompositeNodeSnapshot> composites)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(composites);

        var selectedCompositeId = selection.SelectedNodeIds
            .FirstOrDefault(nodeId => composites.ContainsKey(nodeId));
        var actions = new List<AsterGraphHostedActionDescriptor>();

        foreach (var commandId in WorkflowCommandIds)
        {
            if (!descriptors.TryGetValue(commandId, out var descriptor))
            {
                continue;
            }

            switch (commandId)
            {
                case "composites.wrap-selection":
                    actions.Add(AsterGraphHostedActionFactory.CreateHostAction(
                        descriptor,
                        () => !string.IsNullOrWhiteSpace(session.Commands.TryWrapSelectionToComposite(updateStatus: true))));
                    break;

                case "scopes.enter":
                    actions.Add(AsterGraphHostedActionFactory.CreateHostAction(
                        CreateContextualDescriptor(
                            descriptor,
                            !string.IsNullOrWhiteSpace(selectedCompositeId) && descriptor.CanExecute,
                            string.IsNullOrWhiteSpace(selectedCompositeId)
                                ? "Select a composite node to enter its child graph."
                                : descriptor.DisabledReason),
                        () => !string.IsNullOrWhiteSpace(selectedCompositeId)
                            && session.Commands.TryEnterCompositeChildGraph(selectedCompositeId, updateStatus: true)));
                    break;

                case "scopes.exit":
                    actions.Add(AsterGraphHostedActionFactory.CreateHostAction(
                        descriptor,
                        () => session.Commands.TryReturnToParentGraphScope(updateStatus: true)));
                    break;
            }
        }

        return actions;
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateBreadcrumbActions(IGraphEditorSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var navigation = session.Queries.GetScopeNavigationSnapshot();
        return navigation.Breadcrumbs
            .Select(breadcrumb => AsterGraphHostedActionFactory.CreateHostAction(
                new GraphEditorCommandDescriptorSnapshot(
                    $"breadcrumb.{breadcrumb.ScopeId}",
                    breadcrumb.Title,
                    "scopes",
                    "scope-breadcrumb",
                    null,
                    GraphEditorCommandSourceKind.Host,
                    isEnabled: !string.Equals(breadcrumb.ScopeId, navigation.CurrentScopeId, StringComparison.Ordinal),
                    disabledReason: string.Equals(breadcrumb.ScopeId, navigation.CurrentScopeId, StringComparison.Ordinal)
                        ? "Already viewing this graph scope."
                        : null),
                () => TryNavigateToScope(session, breadcrumb.ScopeId)))
            .ToList();
    }

    private static bool TryNavigateToScope(IGraphEditorSession session, string targetScopeId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetScopeId);

        while (true)
        {
            var navigation = session.Queries.GetScopeNavigationSnapshot();
            if (string.Equals(navigation.CurrentScopeId, targetScopeId, StringComparison.Ordinal))
            {
                return true;
            }

            if (!navigation.CanNavigateToParent)
            {
                return false;
            }

            if (!session.Commands.TryReturnToParentGraphScope(updateStatus: true))
            {
                return false;
            }
        }
    }

    private static GraphEditorCommandDescriptorSnapshot CreateContextualDescriptor(
        GraphEditorCommandDescriptorSnapshot descriptor,
        bool isEnabled,
        string? disabledReason)
        => new(
            descriptor.Id,
            descriptor.Title,
            descriptor.Group,
            descriptor.IconKey,
            descriptor.DefaultShortcut,
            descriptor.Source,
            isEnabled,
            disabledReason);
}
