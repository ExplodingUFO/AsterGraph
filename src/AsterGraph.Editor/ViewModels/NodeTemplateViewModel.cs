using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

public sealed class NodeTemplateViewModel
{
    public NodeTemplateViewModel(INodeDefinition definition)
    {
        Definition = definition;
        Key = definition.Id.Value.Replace(".", "-", StringComparison.Ordinal);
        Title = definition.DisplayName;
        Category = definition.Category;
        Subtitle = definition.Subtitle;
        Description = definition.Description ?? string.Empty;
        Size = new GraphSize(definition.DefaultWidth, definition.DefaultHeight);
        AccentHex = definition.AccentHex;
    }

    public INodeDefinition Definition { get; }

    public string Key { get; }

    public string Title { get; }

    public string Category { get; }

    public string Subtitle { get; }

    public string Description { get; }

    public GraphSize Size { get; }

    public string AccentHex { get; }

    public int InputCount => Definition.InputPorts.Count;

    public int OutputCount => Definition.OutputPorts.Count;

    public string PortSummary => $"{InputCount} in  ·  {OutputCount} out";

    public NodeViewModel CreateNode(string nodeId, GraphPoint position)
        => new(new GraphNode(
            nodeId,
            Definition.DisplayName,
            Definition.Category,
            Definition.Subtitle,
            Definition.Description ?? string.Empty,
            position,
            Size,
            Definition.InputPorts.Select(port => new GraphPort(
                port.Key,
                port.DisplayName,
                PortDirection.Input,
                port.TypeId.Value,
                port.AccentHex,
                port.TypeId)).ToList(),
            Definition.OutputPorts.Select(port => new GraphPort(
                port.Key,
                port.DisplayName,
                PortDirection.Output,
                port.TypeId.Value,
                port.AccentHex,
                port.TypeId)).ToList(),
            Definition.AccentHex,
            Definition.Id));
}
