using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 纯检查器视图，负责展示当前选择的节点摘要、连线信息与参数编辑区。
/// </summary>
public partial class GraphInspectorView : UserControl
{
    private readonly ObservableCollection<GraphInspectorParameterGroupState> _parameterGroups = [];
    private readonly Dictionary<string, bool> _collapsedGroups = new(StringComparer.Ordinal);
    private object? _stockContent;
    private GraphEditorViewModel? _subscribedEditor;
    private TextBox? _parameterSearchBox;
    private Border? _parameterValidationSummary;
    private TextBlock? _parameterValidationSummaryText;
    private Border? _parameterSearchEmptyState;
    private TextBlock? _parameterSearchEmptyStateText;
    private ItemsControl? _parameterGroupsControl;
    private string _parameterSearchText = string.Empty;

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
        InitializeStockControls();
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

        if (change.Property == EditorProperty)
        {
            AttachEditor(change.GetNewValue<GraphEditorViewModel?>());
        }

        if (change.Property == EditorProperty || change.Property == InspectorPresenterProperty)
        {
            ApplyInspectorPresenter();
        }
    }

    private void InitializeComponent()
        => AvaloniaXamlLoader.Load(this);

    private void InitializeStockControls()
    {
        _parameterSearchBox = this.FindControl<TextBox>("PART_ParameterSearchBox");
        _parameterValidationSummary = this.FindControl<Border>("PART_ParameterValidationSummary");
        _parameterValidationSummaryText = this.FindControl<TextBlock>("PART_ParameterValidationSummaryText");
        _parameterSearchEmptyState = this.FindControl<Border>("PART_ParameterSearchEmptyState");
        _parameterSearchEmptyStateText = this.FindControl<TextBlock>("PART_ParameterSearchEmptyStateText");
        _parameterGroupsControl = this.FindControl<ItemsControl>("PART_ParameterGroups");

        if (_parameterGroupsControl is not null)
        {
            _parameterGroupsControl.ItemsSource = _parameterGroups;
        }
    }

    private void ApplyInspectorPresenter()
    {
        if (InspectorPresenter is null)
        {
            if (_stockContent is not null && !ReferenceEquals(Content, _stockContent))
            {
                Content = _stockContent;
            }

            RefreshParameterSurface();
            return;
        }

        Content = InspectorPresenter.Create(Editor);
    }

    private void AttachEditor(GraphEditorViewModel? editor)
    {
        if (ReferenceEquals(_subscribedEditor, editor))
        {
            RefreshParameterSurface();
            return;
        }

        DetachEditor();
        _subscribedEditor = editor;
        if (_subscribedEditor is null)
        {
            RefreshParameterSurface();
            return;
        }

        _subscribedEditor.SelectedNodeParameters.CollectionChanged += HandleSelectedNodeParametersCollectionChanged;
        foreach (var parameter in _subscribedEditor.SelectedNodeParameters)
        {
            parameter.PropertyChanged += HandleParameterPropertyChanged;
        }

        RefreshParameterSurface();
    }

    private void DetachEditor()
    {
        if (_subscribedEditor is null)
        {
            return;
        }

        _subscribedEditor.SelectedNodeParameters.CollectionChanged -= HandleSelectedNodeParametersCollectionChanged;
        foreach (var parameter in _subscribedEditor.SelectedNodeParameters)
        {
            parameter.PropertyChanged -= HandleParameterPropertyChanged;
        }

        _subscribedEditor = null;
    }

    private void HandleSelectedNodeParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<NodeParameterViewModel>())
            {
                item.PropertyChanged -= HandleParameterPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<NodeParameterViewModel>())
            {
                item.PropertyChanged += HandleParameterPropertyChanged;
            }
        }

        RefreshParameterSurface();
    }

    private void HandleParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => RefreshParameterSurface();

    private void HandleParameterSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _parameterSearchText = _parameterSearchBox?.Text?.Trim() ?? string.Empty;
        RefreshParameterSurface();
    }

    private void HandleGroupToggleClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string groupName })
        {
            return;
        }

        _collapsedGroups[groupName] = !_collapsedGroups.GetValueOrDefault(groupName);
        RefreshParameterSurface();
    }

    private void HandleResetParameterClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string parameterKey } || Editor is null)
        {
            return;
        }

        Editor.SelectedNodeParameters
            .FirstOrDefault(parameter => string.Equals(parameter.Key, parameterKey, StringComparison.Ordinal))
            ?.ResetToDefault();

        RefreshParameterSurface();
    }

    private void RefreshParameterSurface()
    {
        var parameters = Editor?.SelectedNodeParameters.ToList() ?? [];
        var filteredGroups = BuildParameterGroups(parameters, _parameterSearchText);

        _parameterGroups.Clear();
        foreach (var group in filteredGroups)
        {
            _parameterGroups.Add(group);
        }

        UpdateValidationSummary(parameters);
        UpdateSearchEmptyState(parameters.Count, filteredGroups.Count);
    }

    private List<GraphInspectorParameterGroupState> BuildParameterGroups(
        IReadOnlyList<NodeParameterViewModel> parameters,
        string searchText)
    {
        if (parameters.Count == 0)
        {
            return [];
        }

        return parameters
            .Where(parameter => MatchesSearch(parameter, searchText))
            .GroupBy(parameter => string.IsNullOrWhiteSpace(parameter.GroupDisplayName) ? "General" : parameter.GroupDisplayName!, StringComparer.Ordinal)
            .Select(group => new GraphInspectorParameterGroupState(
                group.Key,
                group.ToList(),
                _collapsedGroups.GetValueOrDefault(group.Key)))
            .ToList();
    }

    private void UpdateValidationSummary(IReadOnlyList<NodeParameterViewModel> parameters)
    {
        if (_parameterValidationSummary is null || _parameterValidationSummaryText is null)
        {
            return;
        }

        var invalidParameters = parameters
            .Where(parameter => parameter.HasValidationError)
            .Select(parameter => parameter.DisplayName)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (invalidParameters.Count == 0)
        {
            _parameterValidationSummary.IsVisible = false;
            _parameterValidationSummaryText.Text = string.Empty;
            return;
        }

        var previewNames = invalidParameters.Take(3).ToList();
        var suffix = invalidParameters.Count > previewNames.Count ? " 等" : string.Empty;
        _parameterValidationSummaryText.Text = $"{invalidParameters.Count} 个参数存在校验问题: {string.Join("、", previewNames)}{suffix}。";
        _parameterValidationSummary.IsVisible = true;
    }

    private void UpdateSearchEmptyState(int totalParameterCount, int visibleGroupCount)
    {
        if (_parameterSearchEmptyState is null || _parameterSearchEmptyStateText is null)
        {
            return;
        }

        var showEmptyState = totalParameterCount > 0
            && !string.IsNullOrWhiteSpace(_parameterSearchText)
            && visibleGroupCount == 0;
        _parameterSearchEmptyState.IsVisible = showEmptyState;
        _parameterSearchEmptyStateText.Text = showEmptyState
            ? $"没有匹配“{_parameterSearchText}”的参数，请尝试别的关键字。"
            : string.Empty;
    }

    private static bool MatchesSearch(NodeParameterViewModel parameter, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return string.Join(
                "\n",
                parameter.DisplayName,
                parameter.Key,
                parameter.GroupDisplayName,
                parameter.Description,
                parameter.PlaceholderText,
                parameter.HelpText,
                parameter.ReadOnlyReason,
                parameter.ValidationMessage)
            .Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}
