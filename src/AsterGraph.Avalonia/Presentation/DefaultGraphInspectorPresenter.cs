using Avalonia.Controls;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 提供 stock 检查器表面的默认 presenter 实现。
/// </summary>
public sealed class DefaultGraphInspectorPresenter : IGraphInspectorPresenter
{
    /// <inheritdoc />
    public Control Create(GraphEditorViewModel? editor)
        => new GraphInspectorView
        {
            Editor = editor,
        };
}
