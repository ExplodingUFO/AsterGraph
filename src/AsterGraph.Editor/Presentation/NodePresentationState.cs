namespace AsterGraph.Editor.Presentation;

/// <summary>
/// 节点右上角徽标描述。
/// </summary>
/// <param name="Text">徽标文本。</param>
/// <param name="AccentHex">徽标强调色。</param>
/// <param name="ToolTip">可选提示文本。</param>
public sealed record NodeAdornmentDescriptor(string Text, string AccentHex, string? ToolTip = null);

/// <summary>
/// 节点底部状态栏描述。
/// </summary>
/// <param name="Text">状态文本。</param>
/// <param name="AccentHex">状态栏强调色。</param>
/// <param name="ToolTip">可选提示文本。</param>
public sealed record NodeStatusBarDescriptor(string Text, string AccentHex, string? ToolTip = null);

/// <summary>
/// 节点展示层状态快照。
/// </summary>
public sealed record NodePresentationState
{
    /// <summary>
    /// 初始化节点展示状态。
    /// </summary>
    /// <param name="SubtitleOverride">可选副标题覆盖文本。</param>
    /// <param name="DescriptionOverride">可选描述覆盖文本。</param>
    /// <param name="TopRightBadges">右上角徽标集合。</param>
    /// <param name="StatusBar">底部状态栏描述。</param>
    public NodePresentationState(
        string? SubtitleOverride = null,
        string? DescriptionOverride = null,
        IReadOnlyList<NodeAdornmentDescriptor>? TopRightBadges = null,
        NodeStatusBarDescriptor? StatusBar = null)
    {
        this.SubtitleOverride = SubtitleOverride;
        this.DescriptionOverride = DescriptionOverride;
        this.TopRightBadges = TopRightBadges is null
            ? []
            : TopRightBadges.ToArray();
        this.StatusBar = StatusBar;
    }

    /// <summary>
    /// 空展示状态。
    /// </summary>
    public static NodePresentationState Empty { get; } = new();

    /// <summary>
    /// 可选副标题覆盖文本。
    /// </summary>
    public string? SubtitleOverride { get; }

    /// <summary>
    /// 可选描述覆盖文本。
    /// </summary>
    public string? DescriptionOverride { get; }

    /// <summary>
    /// 右上角徽标集合。
    /// </summary>
    public IReadOnlyList<NodeAdornmentDescriptor> TopRightBadges { get; }

    /// <summary>
    /// 底部状态栏描述。
    /// </summary>
    public NodeStatusBarDescriptor? StatusBar { get; }
}
