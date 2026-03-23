namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 片段与剪贴板相关行为配置。
/// </summary>
public sealed record FragmentBehaviorOptions
{
    /// <summary>
    /// 是否启用系统剪贴板互通。
    /// </summary>
    public bool EnableSystemClipboardInterop { get; init; } = true;

    /// <summary>
    /// 是否启用片段模板库。
    /// </summary>
    public bool EnableFragmentLibrary { get; init; } = true;
}
