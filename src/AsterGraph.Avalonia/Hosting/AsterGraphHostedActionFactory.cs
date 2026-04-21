using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Creates host-facing action descriptors from the shared editor command descriptors plus optional host augmentations.
/// </summary>
public static class AsterGraphHostedActionFactory
{
    public static AsterGraphHostedActionProjection CreateProjection(IEnumerable<AsterGraphHostedActionDescriptor> actions)
        => new(actions);

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> ApplyCommandShortcutPolicy(
        IEnumerable<AsterGraphHostedActionDescriptor> actions,
        AsterGraphCommandShortcutPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(policy);

        return actions
            .Select(action =>
            {
                ArgumentNullException.ThrowIfNull(action);

                var effectiveShortcut = policy.ResolveShortcut(action.Id, action.DefaultShortcut);
                if (string.Equals(effectiveShortcut, action.DefaultShortcut, StringComparison.Ordinal))
                {
                    return action;
                }

                var descriptor = new GraphEditorCommandDescriptorSnapshot(
                    action.Id,
                    action.Title,
                    action.Group,
                    action.IconKey,
                    effectiveShortcut,
                    action.CommandSource,
                    action.CanExecute,
                    action.DisabledReason);
                return new AsterGraphHostedActionDescriptor(descriptor, action.TryExecute, action.CommandId);
            })
            .ToList();
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateCommandActions(
        IGraphEditorSession session,
        IEnumerable<string> commandIds)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(commandIds);

        var actionMap = CreateCommandActionMap(session);
        var orderedActions = new List<AsterGraphHostedActionDescriptor>();
        foreach (var commandId in commandIds)
        {
            if (string.IsNullOrWhiteSpace(commandId))
            {
                continue;
            }

            if (actionMap.TryGetValue(commandId, out var action))
            {
                orderedActions.Add(action);
            }
        }

        return orderedActions;
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateCommandActions(IGraphEditorSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.Queries.GetCommandDescriptors()
            .OrderBy(descriptor => descriptor.Group, StringComparer.Ordinal)
            .ThenBy(descriptor => descriptor.Title, StringComparer.Ordinal)
            .Select(descriptor => CreateCommandAction(session, descriptor))
            .ToList();
    }

    public static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateToolActions(
        IGraphEditorSession session,
        IEnumerable<GraphEditorToolDescriptorSnapshot> tools)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(tools);

        return tools
            .Select(tool =>
            {
                ArgumentNullException.ThrowIfNull(tool);
                return CreateInvocationAction(
                    session,
                    new GraphEditorCommandDescriptorSnapshot(
                        tool.Id,
                        tool.Title,
                        tool.Group,
                        tool.IconKey,
                        tool.DefaultShortcut,
                        tool.Source,
                        tool.CanExecute,
                        tool.DisabledReason),
                    tool.Invocation);
            })
            .ToList();
    }

    public static AsterGraphHostedActionDescriptor CreateHostAction(
        GraphEditorCommandDescriptorSnapshot descriptor,
        Func<bool> execute)
        => new(descriptor, execute);

    private static Dictionary<string, AsterGraphHostedActionDescriptor> CreateCommandActionMap(IGraphEditorSession session)
        => session.Queries.GetCommandDescriptors()
            .GroupBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToDictionary(
                descriptor => descriptor.Id,
                descriptor => CreateCommandAction(session, descriptor),
                StringComparer.Ordinal);

    private static AsterGraphHostedActionDescriptor CreateCommandAction(
        IGraphEditorSession session,
        GraphEditorCommandDescriptorSnapshot descriptor)
        => CreateInvocationAction(
            session,
            descriptor,
            new GraphEditorCommandInvocationSnapshot(descriptor.Id));

    private static AsterGraphHostedActionDescriptor CreateInvocationAction(
        IGraphEditorSession session,
        GraphEditorCommandDescriptorSnapshot descriptor,
        GraphEditorCommandInvocationSnapshot invocation)
        => new(
            descriptor,
            () => session.Commands.TryExecuteCommand(invocation),
            invocation.CommandId);
}
