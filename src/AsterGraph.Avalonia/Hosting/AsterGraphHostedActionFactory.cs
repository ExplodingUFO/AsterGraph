using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Creates host-facing action descriptors from the shared editor command descriptors plus optional host augmentations.
/// </summary>
public static class AsterGraphHostedActionFactory
{
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

    public static AsterGraphHostedActionDescriptor CreateHostAction(
        string id,
        string title,
        string group,
        Func<bool> execute,
        bool canExecute = true,
        string? iconKey = null,
        string? defaultShortcut = null,
        string? disabledReason = null)
        => new(
            id,
            title,
            group,
            execute,
            canExecute,
            iconKey,
            defaultShortcut,
            disabledReason);

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
        => new(
            descriptor.Id,
            descriptor.Title,
            descriptor.Group,
            () => session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(descriptor.Id)),
            descriptor.CanExecute,
            descriptor.IconKey,
            descriptor.DefaultShortcut,
            descriptor.DisabledReason,
            descriptor.Id,
            descriptor.Source);
}
