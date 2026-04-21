using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Creates host-facing action descriptors from the shared editor command descriptors plus optional host augmentations.
/// </summary>
public static class AsterGraphHostedActionFactory
{
    public static AsterGraphHostedActionProjection CreateProjection(IEnumerable<AsterGraphHostedActionDescriptor> actions)
        => new(actions);

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
        => new(
            descriptor,
            () => session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(descriptor.Id)),
            descriptor.Id);
}
