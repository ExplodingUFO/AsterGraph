using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 纯检查器视图，负责展示当前选择的节点摘要、连线信息与参数编辑区。
/// </summary>
public partial class GraphInspectorView : UserControl
{
    private object? _stockContent;

    /// <summary>
    /// 编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> EditorProperty =
        AvaloniaProperty.Register<GraphInspectorView, GraphEditorViewModel?>(nameof(Editor));

    /// <summary>
    /// 可选的检查器展示器依赖属性。
    /// </summary>
    public static readonly StyledProperty<IGraphInspectorPresenter?> InspectorPresenterProperty =
        AvaloniaProperty.Register<GraphInspectorView, IGraphInspectorPresenter?>(nameof(InspectorPresenter));

    /// <summary>
    /// 初始化独立检查器视图。
    /// </summary>
    public GraphInspectorView()
    {
        InitializeComponent();
        _stockContent = Content;
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor
    {
        get => GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    /// <summary>
    /// 当前检查器展示器。
    /// </summary>
    public IGraphInspectorPresenter? InspectorPresenter
    {
        get => GetValue(InspectorPresenterProperty);
        set => SetValue(InspectorPresenterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EditorProperty || change.Property == InspectorPresenterProperty)
        {
            ApplyInspectorPresenter();
        }
    }

    private void InitializeComponent()
        => AvaloniaXamlLoader.Load(this);

    private void ApplyInspectorPresenter()
    {
        if (InspectorPresenter is null)
        {
            if (_stockContent is not null && !ReferenceEquals(Content, _stockContent))
            {
                Content = _stockContent;
            }

            return;
        }

        Content = InspectorPresenter.Create(Editor);
    }
}
