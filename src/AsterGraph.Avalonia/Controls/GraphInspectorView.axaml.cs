using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 纯检查器视图，负责展示当前选择的节点摘要、连线信息与参数编辑区。
/// </summary>
public partial class GraphInspectorView : UserControl
{
    private readonly Dictionary<string, bool> _collapsedGroups = new(StringComparer.Ordinal);
    private object? _stockContent;
    private GraphEditorViewModel? _subscribedEditor;
    private TextBox? _parameterSearchBox;
    private Border? _parameterValidationSummary;
    private TextBlock? _parameterValidationSummaryText;
    private Border? _parameterSearchEmptyState;
    private TextBlock? _parameterSearchEmptyStateText;
    private ItemsControl? _parameterGroupsControl;
    private Button? _advancedParametersToggleButton;
    private string _parameterSearchText = string.Empty;
    private bool _showAdvancedParameters;

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
    /// Optional registry used by shipped inspector parameter-editor hosts.
    /// </summary>
    public static readonly StyledProperty<INodeParameterEditorRegistry?> NodeParameterEditorRegistryProperty =
        AvaloniaProperty.Register<GraphInspectorView, INodeParameterEditorRegistry?>(nameof(NodeParameterEditorRegistry));

    /// <summary>
    /// 初始化独立检查器视图。
    /// </summary>
    public GraphInspectorView()
    {
        InitializeComponent();
        _stockContent = Content;
        InitializeStockControls();
        AddHandler(
            InputElement.GotFocusEvent,
            HandleInspectorFocusChanged,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
        AddHandler(
            InputElement.LostFocusEvent,
            HandleInspectorFocusChanged,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
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

    /// <summary>
    /// Registry used by shipped inspector parameter editors.
    /// </summary>
    public INodeParameterEditorRegistry? NodeParameterEditorRegistry
    {
        get => GetValue(NodeParameterEditorRegistryProperty);
        set => SetValue(NodeParameterEditorRegistryProperty, value);
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
        _advancedParametersToggleButton = this.FindControl<Button>("PART_AdvancedParametersToggleButton");

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

        _subscribedEditor.SetInspectorEditingParameter(null);
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

    private void HandleAdvancedParametersToggleClick(object? sender, RoutedEventArgs e)
    {
        _showAdvancedParameters = !_showAdvancedParameters;
        RefreshParameterSurface();
    }

    private void HandleInspectorFocusChanged(object? sender, RoutedEventArgs e)
    {
        if (e.RoutedEvent == InputElement.GotFocusEvent)
        {
            UpdateInspectorEditingFocus(e.Source as IInputElement);
            return;
        }

        Dispatcher.UIThread.Post(
            () => UpdateInspectorEditingFocus(),
            DispatcherPriority.Input);
    }

    private void UpdateInspectorEditingFocus(IInputElement? focusSource = null)
        => Editor?.SetInspectorEditingParameter(
            GraphPresentationSemantics.ResolveInspectorEditingParameterKey(
                TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement()
                ?? focusSource));

    private void RefreshParameterSurface()
    {
        var parameters = Editor?.SelectedNodeParameters.ToList() ?? [];
        UpdateAdvancedParametersToggle(parameters);
        var filteredGroups = BuildParameterGroups(parameters, _parameterSearchText);
        if (_parameterGroupsControl is not null)
        {
            _parameterGroupsControl.ItemsSource = filteredGroups;
        }

        UpdateValidationSummary(parameters);
        UpdateSearchEmptyState(parameters.Count, filteredGroups.Count);
        _parameterGroupsControl?.InvalidateMeasure();
        _parameterGroupsControl?.InvalidateArrange();
        InvalidateMeasure();
        InvalidateArrange();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
        if (Bounds.Width > 0 && Bounds.Height > 0)
        {
            Measure(Bounds.Size);
            Arrange(new Rect(Bounds.Size));
        }
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
            .Where(parameter => ShouldDisplayParameter(parameter, searchText))
            .GroupBy(parameter => string.IsNullOrWhiteSpace(parameter.GroupDisplayName) ? "General" : parameter.GroupDisplayName!, StringComparer.Ordinal)
            .Select(group => new GraphInspectorParameterGroupState(
                group.Key,
                group.ToList(),
                _collapsedGroups.GetValueOrDefault(group.Key)))
            .ToList();
    }

    private void UpdateAdvancedParametersToggle(IReadOnlyList<NodeParameterViewModel> parameters)
    {
        if (_advancedParametersToggleButton is null)
        {
            return;
        }

        var hasAdvancedParameters = parameters.Any(parameter => parameter.IsAdvanced);
        if (!hasAdvancedParameters)
        {
            _showAdvancedParameters = false;
        }

        _advancedParametersToggleButton.IsVisible = hasAdvancedParameters;
        _advancedParametersToggleButton.Content = _showAdvancedParameters
            ? "隐藏高级参数"
            : "显示高级参数";
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

    private bool ShouldDisplayParameter(NodeParameterViewModel parameter, string searchText)
    {
        var matchesSearch = MatchesSearch(parameter, searchText);
        if (!matchesSearch)
        {
            return false;
        }

        if (!parameter.IsAdvanced)
        {
            return true;
        }

        return _showAdvancedParameters || !string.IsNullOrWhiteSpace(searchText);
    }
}
