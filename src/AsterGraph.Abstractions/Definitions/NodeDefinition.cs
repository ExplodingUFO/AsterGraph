using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Definitions;

/// <summary>
/// Immutable contract for a node type that can be instantiated in a graph.
/// </summary>
public interface INodeDefinition
{
    NodeDefinitionId Id { get; }

    string DisplayName { get; }

    string Category { get; }

    string Subtitle { get; }

    string? Description { get; }

    string AccentHex { get; }

    double DefaultWidth { get; }

    double DefaultHeight { get; }

    IReadOnlyList<PortDefinition> InputPorts { get; }

    IReadOnlyList<PortDefinition> OutputPorts { get; }

    IReadOnlyList<NodeParameterDefinition> Parameters { get; }
}

public sealed record NodeDefinition : INodeDefinition
{
    public NodeDefinition(
        NodeDefinitionId id,
        string displayName,
        string category,
        string subtitle,
        IReadOnlyList<PortDefinition> inputPorts,
        IReadOnlyList<PortDefinition> outputPorts,
        IReadOnlyList<NodeParameterDefinition>? parameters = null,
        string? description = null,
        string accentHex = "#FFFFFF",
        double defaultWidth = 220d,
        double defaultHeight = 140d)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(subtitle);
        ArgumentNullException.ThrowIfNull(inputPorts);
        ArgumentNullException.ThrowIfNull(outputPorts);

        EnsureUniqueKeys(inputPorts.Select(port => port.Key), "input port");
        EnsureUniqueKeys(outputPorts.Select(port => port.Key), "output port");
        EnsureUniqueKeys(inputPorts.Select(port => port.Key).Concat(outputPorts.Select(port => port.Key)), "port");

        if (defaultWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultWidth), "Default width must be positive.");
        }

        if (defaultHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultHeight), "Default height must be positive.");
        }

        Id = id;
        DisplayName = displayName.Trim();
        Category = category.Trim();
        Subtitle = subtitle.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        AccentHex = accentHex;
        DefaultWidth = defaultWidth;
        DefaultHeight = defaultHeight;
        InputPorts = [.. inputPorts];
        OutputPorts = [.. outputPorts];
        Parameters = parameters is null ? [] : [.. parameters];

        EnsureUniqueKeys(Parameters.Select(parameter => parameter.Key), "parameter");
    }

    public NodeDefinitionId Id { get; }

    public string DisplayName { get; }

    public string Category { get; }

    public string Subtitle { get; }

    public string? Description { get; }

    public string AccentHex { get; }

    public double DefaultWidth { get; }

    public double DefaultHeight { get; }

    public IReadOnlyList<PortDefinition> InputPorts { get; }

    public IReadOnlyList<PortDefinition> OutputPorts { get; }

    public IReadOnlyList<NodeParameterDefinition> Parameters { get; }

    private static void EnsureUniqueKeys(IEnumerable<string> keys, string scope)
    {
        var duplicates = keys
            .GroupBy(key => key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ArgumentException(
                $"Duplicate {scope} key(s) are not allowed: {string.Join(", ", duplicates)}.");
        }
    }
}
