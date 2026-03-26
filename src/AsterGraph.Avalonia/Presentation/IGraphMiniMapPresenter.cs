using Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义独立缩略图表面的展示层替换契约。
/// </summary>
public interface IGraphMiniMapPresenter
{
    /// <summary>
    /// 基于当前编辑器状态创建缩略图展示控件。
    /// </summary>
    Control Create(GraphEditorViewModel? editor);
}
