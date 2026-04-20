using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeViewModel : ObservableObject
{
    private readonly Dictionary<string, GraphParameterValue> _parameterValues;
    private readonly ObservableCollection<NodeParameterEndpointViewModel> _parameterEndpoints = [];

    /// <summary>
    /// 由不可变节点模型快照初始化节点运行时投影。
    /// </summary>
    /// <param name="model">节点模型快照。</param>
    public NodeViewModel(GraphNode model)
    {
        var normalizedSize = GraphEditorNodeSurfaceMetrics.NormalizePersistedSize(
            model.Size,
            model.Inputs.Count,
            model.Outputs.Count);

        Id = model.Id;
        DefinitionId = model.DefinitionId;
        Title = model.Title;
        Category = model.Category;
        Subtitle = model.Subtitle;
        Description = model.Description;
        AccentHex = model.AccentHex;
        Width = normalizedSize.Width;
        Height = normalizedSize.Height;
        Surface = model.Surface ?? GraphNodeSurfaceState.Default;
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

        ActiveSurfaceTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            normalizedSize,
            AsterGraph.Editor.Configuration.GraphEditorBehaviorOptions.Default,
            definition: null);
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
    [ObservableProperty]
    private double width;

    /// <summary>
    /// 当前节点持久化 surface 状态快照。
    /// </summary>
    [ObservableProperty]
    private GraphNodeSurfaceState surface = GraphNodeSurfaceState.Default;

    /// <summary>
    /// 当前节点所属的 editor-only 分组标识；未分组时为空。
    /// </summary>
    public string? GroupId => Surface.GroupId;

    /// <summary>
    /// 当前解析得到的节点表面 tier。
    /// </summary>
    [ObservableProperty]
    private GraphEditorNodeSurfaceTierSnapshot activeSurfaceTier = new(
        "compact",
        0d,
        0d,
        []);

    /// <summary>
    /// Indicates whether the active tier exposes parameter authoring affordances.
    /// </summary>
    public bool AllowsParameterAuthoring
        => ActiveSurfaceTier.ShowsSection(AsterGraph.Abstractions.Definitions.NodeSurfaceSectionKeys.ParameterRail)
           || ActiveSurfaceTier.ShowsSection(AsterGraph.Abstractions.Definitions.NodeSurfaceSectionKeys.ParameterEditors);

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

    /// <summary>
    /// Node-local parameter endpoints rendered by hosted node-side authoring surfaces.
    /// </summary>
    public ObservableCollection<NodeParameterEndpointViewModel> ParameterEndpoints => _parameterEndpoints;

    /// <summary>
    /// Whether the node currently exposes any node-local parameter endpoints.
    /// </summary>
    public bool HasParameterEndpoints => _parameterEndpoints.Count > 0;

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
            new GraphSize(Width, Height),
            Inputs.Select(port => port.ToModel()).ToList(),
            Outputs.Select(port => port.ToModel()).ToList(),
            AccentHex,
            DefinitionId,
            _parameterValues.Values.ToList(),
            Surface);

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
    /// Replaces the node-local parameter endpoint projection.
    /// </summary>
    public void UpdateParameterEndpoints(IReadOnlyList<NodeParameterEndpointViewModel> endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        _parameterEndpoints.Clear();
        foreach (var endpoint in endpoints)
        {
            _parameterEndpoints.Add(endpoint);
        }

        OnPropertyChanged(nameof(HasParameterEndpoints));
    }

    /// <summary>
    /// Updates the persisted surface state snapshot.
    /// </summary>
    /// <param name="state">New surface state.</param>
    public void UpdateSurface(GraphNodeSurfaceState? state)
        => Surface = state ?? GraphNodeSurfaceState.Default;

    /// <summary>
    /// Updates the resolved size-driven tier snapshot used by shipped and custom host surfaces.
    /// </summary>
    public void UpdateActiveSurfaceTier(GraphEditorNodeSurfaceTierSnapshot activeTier)
        => ActiveSurfaceTier = activeTier ?? throw new ArgumentNullException(nameof(activeTier));

    /// <summary>
    /// 更新节点展示状态快照。
    /// </summary>
    /// <param name="state">新的展示状态；为空时回退到默认展示。</param>
    public void UpdatePresentation(NodePresentationState? state)
        => Presentation = state ?? NodePresentationState.Empty;

    partial void OnPresentationChanged(NodePresentationState value)
    {
        OnPropertyChanged(nameof(DisplaySubtitle));
        OnPropertyChanged(nameof(DisplayDescription));
    }

    partial void OnWidthChanged(double value)
        => OnPropertyChanged(nameof(Bounds));

    partial void OnHeightChanged(double value)
        => OnPropertyChanged(nameof(Bounds));

    partial void OnXChanged(double value)
        => OnPropertyChanged(nameof(Bounds));

    partial void OnYChanged(double value)
        => OnPropertyChanged(nameof(Bounds));

    partial void OnSurfaceChanged(GraphNodeSurfaceState value)
    {
        OnPropertyChanged(nameof(GroupId));
    }

    partial void OnActiveSurfaceTierChanged(GraphEditorNodeSurfaceTierSnapshot value)
    {
        OnPropertyChanged(nameof(AllowsParameterAuthoring));
    }
}
