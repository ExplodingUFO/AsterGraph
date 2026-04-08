namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents a stable command invocation descriptor.
/// </summary>
public sealed record GraphEditorCommandInvocationSnapshot
{
    public GraphEditorCommandInvocationSnapshot(
        string commandId,
        IReadOnlyList<GraphEditorCommandArgumentSnapshot>? arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        CommandId = commandId.Trim();
        Arguments = arguments ?? [];
    }

    public string CommandId { get; }

    public IReadOnlyList<GraphEditorCommandArgumentSnapshot> Arguments { get; }

    public bool TryGetArgument(string name, out string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var argument = Arguments.FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.Ordinal));
        value = argument?.Value;
        return argument is not null;
    }

    public IReadOnlyList<string> GetArguments(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Arguments
            .Where(candidate => string.Equals(candidate.Name, name, StringComparison.Ordinal))
            .Select(candidate => candidate.Value)
            .ToList();
    }
}
