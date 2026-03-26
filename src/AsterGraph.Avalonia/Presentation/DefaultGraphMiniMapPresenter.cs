using Avalonia.Controls;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 提供 stock 缩略图表面的默认 presenter 实现。
/// </summary>
public sealed class DefaultGraphMiniMapPresenter : IGraphMiniMapPresenter
{
    /// <inheritdoc />
    public Control Create(GraphEditorViewModel? editor)
        => new GraphMiniMap
        {
            ViewModel = editor,
        };
}
