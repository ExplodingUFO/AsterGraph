using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

public sealed class PortViewModel
{
    public PortViewModel(GraphPort model, int index, int total)
    {
        Id = model.Id;
        Label = model.Label;
        Direction = model.Direction;
        DataType = model.DataType;
        TypeId = model.TypeId ?? new PortTypeId(model.DataType);
        AccentHex = model.AccentHex;
        Index = index;
        Total = total;
    }

    public string Id { get; }

    public string Label { get; }

    public PortDirection Direction { get; }

    public string DataType { get; }

    public PortTypeId TypeId { get; }

    public string AccentHex { get; }

    public int Index { get; }

    public int Total { get; }

    public GraphPort ToModel()
        => new(Id, Label, Direction, DataType, AccentHex, TypeId);
}
