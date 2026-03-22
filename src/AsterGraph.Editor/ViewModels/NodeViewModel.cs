using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    private const double MinimumBodyHeight = 158;
    private const double BaseChromeHeight = 132;
    private const double DescriptionHeight = 40;
    private const double PortRowHeight = 24;
    private const double PortRowSpacing = 8;
    private readonly Dictionary<string, GraphParameterValue> _parameterValues;

    public NodeViewModel(GraphNode model)
    {
        Id = model.Id;
        DefinitionId = model.DefinitionId;
        Title = model.Title;
        Category = model.Category;
        Subtitle = model.Subtitle;
        Description = model.Description;
        AccentHex = model.AccentHex;
        Width = model.Size.Width;
        X = model.Position.X;
        Y = model.Position.Y;

        Inputs = model.Inputs
            .Select((port, index) => new PortViewModel(port, index, model.Inputs.Count))
            .ToList()
            .AsReadOnly();

        Outputs = model.Outputs
            .Select((port, index) => new PortViewModel(port, index, model.Outputs.Count))
            .ToList()
            .AsReadOnly();

        _parameterValues = (model.ParameterValues ?? [])
            .ToDictionary(parameter => parameter.Key, StringComparer.Ordinal);

        Height = Math.Max(model.Size.Height, CalculateRequiredHeight());
    }

    public string Id { get; }

    public NodeDefinitionId? DefinitionId { get; }

    public string Title { get; }

    public string Category { get; }

    public string Subtitle { get; }

    public string Description { get; }

    public string AccentHex { get; }

    public double Width { get; }

    public double Height { get; }

    public int InputCount => Inputs.Count;

    public int OutputCount => Outputs.Count;

    public IReadOnlyList<PortViewModel> Inputs { get; }

    public IReadOnlyList<PortViewModel> Outputs { get; }

    public IReadOnlyDictionary<string, GraphParameterValue> ParameterValues => _parameterValues;

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private bool isSelected;

    public NodeBounds Bounds => new(X, Y, Width, Height);

    public PortViewModel? GetPort(string portId)
        => Inputs.Concat(Outputs).FirstOrDefault(port => port.Id == portId);

    public GraphPoint GetPortAnchor(PortViewModel port)
        => PortAnchorCalculator.GetAnchor(Bounds, port.Direction, port.Index, port.Total);

    public GraphNode ToModel()
        => new(
            Id,
            Title,
            Category,
            Subtitle,
            Description,
            new GraphPoint(X, Y),
            new GraphSize(Width, Height),
            Inputs.Select(port => port.ToModel()).ToList(),
            Outputs.Select(port => port.ToModel()).ToList(),
            AccentHex,
            DefinitionId,
            _parameterValues.Values.ToList());

    public void MoveBy(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    public object? GetParameterValue(string key)
        => _parameterValues.TryGetValue(key, out var value) ? value.Value : null;

    public void SetParameterValue(string key, PortTypeId typeId, object? value)
    {
        _parameterValues[key] = new GraphParameterValue(key, typeId, value);
        OnPropertyChanged(nameof(ParameterValues));
    }

    private double CalculateRequiredHeight()
    {
        var visiblePortRows = Math.Max(Math.Max(Inputs.Count, Outputs.Count), 1);
        var portsHeight = (visiblePortRows * PortRowHeight)
            + ((visiblePortRows - 1) * PortRowSpacing);

        return Math.Max(
            MinimumBodyHeight,
            BaseChromeHeight + DescriptionHeight + portsHeight);
    }
}
