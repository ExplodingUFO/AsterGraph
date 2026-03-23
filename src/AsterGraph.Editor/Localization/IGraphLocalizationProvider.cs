namespace AsterGraph.Editor.Localization;

/// <summary>
/// 提供图编辑器内置文本的本地化查询能力。
/// </summary>
public interface IGraphLocalizationProvider
{
    /// <summary>
    /// 按键获取本地化文本，未命中时应返回回退文本。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="fallback">默认回退文本。</param>
    /// <returns>本地化结果或回退文本。</returns>
    string GetString(string key, string fallback);
}
