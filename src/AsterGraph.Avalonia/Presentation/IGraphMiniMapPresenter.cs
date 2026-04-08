using Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义独立缩略图表面的展示层替换契约。
/// </summary>
/// <remarks>
/// This presenter still receives the retained <see cref="GraphEditorViewModel"/> facade
/// during the migration window. Phase 16 keeps the surface supported, but it should be
/// treated as a compatibility-oriented UI seam rather than the canonical runtime boundary.
/// </remarks>
public interface IGraphMiniMapPresenter
{
    /// <summary>
    /// 基于当前编辑器状态创建缩略图展示控件。
    /// </summary>
    Control Create(GraphEditorViewModel? editor);
}
