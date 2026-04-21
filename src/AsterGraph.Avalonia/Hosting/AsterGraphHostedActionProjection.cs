namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Provides a stable host-facing action projection that can be consumed by rails, menus, palettes, and shortcut help.
/// </summary>
public sealed class AsterGraphHostedActionProjection
{
    private readonly Dictionary<string, AsterGraphHostedActionDescriptor> _actionsById;

    public AsterGraphHostedActionProjection(IEnumerable<AsterGraphHostedActionDescriptor> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var orderedActions = new List<AsterGraphHostedActionDescriptor>();
        var indicesById = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var action in actions)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (indicesById.TryGetValue(action.Id, out var index))
            {
                orderedActions[index] = action;
                continue;
            }

            indicesById.Add(action.Id, orderedActions.Count);
            orderedActions.Add(action);
        }

        Actions = orderedActions;
        _actionsById = orderedActions.ToDictionary(action => action.Id, StringComparer.Ordinal);
    }

    public IReadOnlyList<AsterGraphHostedActionDescriptor> Actions { get; }

    public IReadOnlyList<AsterGraphHostedActionDescriptor> Select(IEnumerable<string> actionIds)
    {
        ArgumentNullException.ThrowIfNull(actionIds);

        var selectedActions = new List<AsterGraphHostedActionDescriptor>();
        foreach (var actionId in actionIds)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                continue;
            }

            if (_actionsById.TryGetValue(actionId, out var action))
            {
                selectedActions.Add(action);
            }
        }

        return selectedActions;
    }

    public IReadOnlyList<AsterGraphHostedActionDescriptor> WithShortcuts()
        => Actions
            .Where(action => !string.IsNullOrWhiteSpace(action.DefaultShortcut))
            .ToList();

    public bool TryGet(string actionId, out AsterGraphHostedActionDescriptor action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        return _actionsById.TryGetValue(actionId, out action!);
    }
}
