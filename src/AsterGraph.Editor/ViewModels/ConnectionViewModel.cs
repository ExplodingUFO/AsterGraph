using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

public sealed class ConnectionViewModel
{
    public ConnectionViewModel(
        string id,
        string sourceNodeId,
        string sourcePortId,
        string targetNodeId,
        string targetPortId,
        string label,
        string accentHex,
        ConversionId? conversionId = null)
    {
        Id = id;
        SourceNodeId = sourceNodeId;
        SourcePortId = sourcePortId;
        TargetNodeId = targetNodeId;
        TargetPortId = targetPortId;
        Label = label;
        AccentHex = accentHex;
        ConversionId = conversionId;
    }

    public string Id { get; }

    public string SourceNodeId { get; }

    public string SourcePortId { get; }

    public string TargetNodeId { get; }

    public string TargetPortId { get; }

    public string Label { get; }

    public string AccentHex { get; }

    public ConversionId? ConversionId { get; }

    public GraphConnection ToModel()
        => new(Id, SourceNodeId, SourcePortId, TargetNodeId, TargetPortId, Label, AccentHex, ConversionId);
}
