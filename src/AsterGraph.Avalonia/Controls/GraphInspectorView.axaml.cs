using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 纯检查器视图，负责展示当前选择的节点摘要、连线信息与参数编辑区。
/// </summary>
public partial class GraphInspectorView : UserControl
{
    /// <summary>
    /// 编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
        AvaloniaProperty.Register<GraphInspectorView, GraphEditorViewModel?>(nameof(Editor));

    /// <summary>
    /// 初始化独立检查器视图。
    /// </summary>
    public GraphInspectorView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    private void InitializeComponent()
        => AvaloniaXamlLoader.Load(this);
}
