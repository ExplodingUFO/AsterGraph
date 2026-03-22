namespace AsterGraph.Editor.ViewModels;

public sealed class NodeParameterOptionViewModel
{
    public NodeParameterOptionViewModel(string value, string label, string? description = null)
    {
        Value = value;
        Label = label;
        Description = description;
    }

    public string Value { get; }

    public string Label { get; }

    public string? Description { get; }

    public override string ToString() => Label;
}
