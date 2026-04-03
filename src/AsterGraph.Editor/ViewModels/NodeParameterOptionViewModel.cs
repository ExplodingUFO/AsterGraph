namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 枚举参数允许选项的只读运行时投影。
/// </summary>
public sealed class NodeParameterOptionViewModel
{
    /// <summary>
    /// 初始化参数选项视图模型。
    /// </summary>
    /// <param name="value">选项写回模型时使用的稳定值。</param>
    /// <param name="label">选项显示文本。</param>
    /// <param name="description">选项的附加说明文本。</param>
    public NodeParameterOptionViewModel(string value, string label, string? description = null)
    {
        Value = value;
        Label = label;
        Description = description;
    }

    /// <summary>
    /// 选项写回模型时使用的稳定值。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 选项显示文本。
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// 选项的附加说明文本。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 返回用于界面显示的选项标签。
    /// </summary>
    /// <returns>当前选项的 <see cref="Label"/>。</returns>
    public override string ToString() => Label;
}
