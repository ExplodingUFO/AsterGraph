using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Presentation;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    private const double MinimumBodyHeight = 158;
    private const double BaseChromeHeight = 132;
    private const double DescriptionHeight = 40;
    private const double PortRowHeight = 24;
    private const double PortRowSpacing = 8;
    private const double StatusBarHeight = 28;
    private readonly Dictionary<string, GraphParameterValue> _parameterValues;
    private readonly double _baseHeight;

    /// <summary>
    /// 由不可变节点模型快照初始化节点运行时投影。
    /// </summary>
    /// <param name="model">节点模型快照。</param>
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

        _baseHeight = Math.Max(model.Size.Height, CalculateRequiredHeight());
        Height = CalculateRenderedHeight();
    }

    /// <summary>
    /// 节点实例标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 生成该节点的定义标识；未知定义时可为空。
    /// </summary>
    public NodeDefinitionId? DefinitionId { get; }

    /// <summary>
    /// 节点标题。
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 节点分类文本。
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// 节点副标题。
    /// </summary>
    public string Subtitle { get; }

    /// <summary>
    /// 节点描述文本。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 节点当前展示状态快照。
    /// </summary>
    [ObservableProperty]
    private NodePresentationState presentation = NodePresentationState.Empty;

    /// <summary>
    /// 当前用于渲染的副标题文本。
    /// </summary>
    public string DisplaySubtitle => Presentation.SubtitleOverride ?? Subtitle;

    /// <summary>
    /// 当前用于渲染的描述文本。
    /// </summary>
    public string DisplayDescription => Presentation.DescriptionOverride ?? Description;

    /// <summary>
    /// 节点强调色。
    /// </summary>
    public string AccentHex { get; }

    /// <summary>
    /// 节点宽度。
    /// </summary>
    public double Width { get; }

    [ObservableProperty]
    private double height;

    /// <summary>
    /// 输入端口数量。
    /// </summary>
    public int InputCount => Inputs.Count;

    /// <summary>
    /// 输出端口数量。
    /// </summary>
    public int OutputCount => Outputs.Count;

    /// <summary>
    /// 输入端口的运行时投影集合。
    /// </summary>
    public IReadOnlyList<PortViewModel> Inputs { get; }

    /// <summary>
    /// 输出端口的运行时投影集合。
    /// </summary>
    public IReadOnlyList<PortViewModel> Outputs { get; }

    /// <summary>
    /// 当前节点参数值的只读字典视图。
    /// </summary>
    public IReadOnlyDictionary<string, GraphParameterValue> ParameterValues => _parameterValues;

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// 当前节点边界。
    /// </summary>
    public NodeBounds Bounds => new(X, Y, Width, Height);

    /// <summary>
    /// 按端口标识查找输入或输出端口投影。
    /// </summary>
    /// <param name="portId">端口标识。</param>
    /// <returns>匹配的端口投影；未找到时为 <see langword="null"/>。</returns>
    public PortViewModel? GetPort(string portId)
        => Inputs.Concat(Outputs).FirstOrDefault(port => port.Id == portId);

    /// <summary>
    /// 计算指定端口在当前节点边界上的锚点坐标。
    /// </summary>
    /// <param name="port">目标端口投影。</param>
    /// <returns>对应的世界坐标锚点。</returns>
    public GraphPoint GetPortAnchor(PortViewModel port)
        => PortAnchorCalculator.GetAnchor(Bounds, port.Direction, port.Index, port.Total);

    /// <summary>
    /// 转回不可变节点模型快照。
    /// </summary>
    /// <returns>对应的不可变节点模型。</returns>
    public GraphNode ToModel()
        => new(
            Id,
            Title,
            Category,
            Subtitle,
            Description,
            new GraphPoint(X, Y),
            new GraphSize(Width, _baseHeight),
            Inputs.Select(port => port.ToModel()).ToList(),
            Outputs.Select(port => port.ToModel()).ToList(),
            AccentHex,
            DefinitionId,
            _parameterValues.Values.ToList());

    /// <summary>
    /// 按指定偏移量移动节点位置。
    /// </summary>
    /// <param name="deltaX">X 轴偏移量。</param>
    /// <param name="deltaY">Y 轴偏移量。</param>
    public void MoveBy(double deltaX, double deltaY)
    {
        X += deltaX;
        Y += deltaY;
    }

    /// <summary>
    /// 读取指定参数键对应的原始参数值。
    /// </summary>
    /// <param name="key">参数稳定键。</param>
    /// <returns>参数值；不存在时为 <see langword="null"/>。</returns>
    public object? GetParameterValue(string key)
        => _parameterValues.TryGetValue(key, out var value) ? value.Value : null;

    /// <summary>
    /// 更新指定参数键对应的参数值。
    /// </summary>
    /// <param name="key">参数稳定键。</param>
    /// <param name="typeId">参数值类型标识。</param>
    /// <param name="value">新的参数值。</param>
    public void SetParameterValue(string key, PortTypeId typeId, object? value)
    {
        _parameterValues[key] = new GraphParameterValue(key, typeId, value);
        OnPropertyChanged(nameof(ParameterValues));
    }

    /// <summary>
    /// 更新节点展示状态快照。
    /// </summary>
    /// <param name="state">新的展示状态；为空时回退到默认展示。</param>
    public void UpdatePresentation(NodePresentationState? state)
        => Presentation = state ?? NodePresentationState.Empty;

    partial void OnPresentationChanged(NodePresentationState value)
    {
        Height = CalculateRenderedHeight();
        OnPropertyChanged(nameof(DisplaySubtitle));
        OnPropertyChanged(nameof(DisplayDescription));
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

    private double CalculateRenderedHeight()
        => _baseHeight + (Presentation.StatusBar is null ? 0 : StatusBarHeight);
}
