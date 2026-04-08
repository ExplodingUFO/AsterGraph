namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 定义图编辑器插件的最小公共合同。
/// </summary>
public interface IGraphEditorPlugin
{
    /// <summary>
    /// 插件描述信息。
    /// </summary>
    GraphEditorPluginDescriptor Descriptor { get; }

    /// <summary>
    /// 向组合构建器注册插件贡献。
    /// </summary>
    /// <param name="builder">插件组合构建器。</param>
    void Register(GraphEditorPluginBuilder builder);
}
