using System.ComponentModel;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Represents one stable command argument.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record GraphEditorCommandArgumentSnapshot
{
    public GraphEditorCommandArgumentSnapshot(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        Name = name.Trim();
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }
}
