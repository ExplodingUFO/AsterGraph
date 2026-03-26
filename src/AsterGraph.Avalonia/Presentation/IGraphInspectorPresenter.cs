using Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义独立检查器表面的展示层替换契约。
/// </summary>
public interface IGraphInspectorPresenter
{
    /// <summary>
    /// 基于当前编辑器状态创建检查器展示控件。
    /// </summary>
    Control Create(GraphEditorViewModel? editor);
}
