using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Parameters;
using System.Linq;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeParameterViewModel : ObservableObject
{
    private readonly Action<NodeParameterViewModel, object?> _applyValue;
    private readonly object? _defaultValue;
    private bool _suppressChangeNotifications;
    private bool _hasMixedValues;

    /// <summary>
    /// 为当前选择投影一个可批量编辑的节点参数。
    /// </summary>
    /// <param name="definition">参数定义快照。</param>
    /// <param name="currentValues">当前选择中的参数值集合。</param>
    /// <param name="applyValue">应用规范化参数值的回调。</param>
    /// <param name="isHostReadOnly">宿主是否强制将该参数标记为只读。</param>
    /// <param name="groupDisplayName">检查器分组标题；无需显示分组时可为空。</param>
    /// <param name="isGroupHeaderVisible">是否在当前参数前显示分组标题。</param>
    public NodeParameterViewModel(
        NodeParameterDefinition definition,
        IReadOnlyList<object?> currentValues,
        Action<NodeParameterViewModel, object?> applyValue,
        bool isHostReadOnly = false,
        string? groupDisplayName = null,
        bool isGroupHeaderVisible = false)
    {
        Definition = definition;
        _applyValue = applyValue;
        _defaultValue = NodeParameterValueAdapter.NormalizeIncomingValue(definition.DefaultValue);

        Key = definition.Key;
        DisplayName = definition.DisplayName;
        Description = definition.Description;
        EditorKind = definition.EditorKind;
        TypeId = definition.ValueType;
        IsRequired = definition.IsRequired;
        IsReadOnly = definition.Constraints.IsReadOnly || isHostReadOnly;
        ReadOnlyReason = BuildReadOnlyReason(definition, isHostReadOnly);
        GroupDisplayName = groupDisplayName;
        IsGroupHeaderVisible = isGroupHeaderVisible;
        PlaceholderText = definition.PlaceholderText;
        HelpText = BuildHelpText(definition, _defaultValue);
        Options = definition.Constraints.AllowedOptions
            .Select(option => new NodeParameterOptionViewModel(option.Value, option.Label, option.Description))
            .ToList()
            .AsReadOnly();

        InitializeValues(currentValues.Count == 0 ? [definition.DefaultValue] : currentValues);
    }

    /// <summary>
    /// 参数定义快照。
    /// </summary>
    public NodeParameterDefinition Definition { get; }

    /// <summary>
    /// 参数稳定键。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 参数显示名称。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 参数说明文本。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 参数声明的编辑器种类。
    /// </summary>
    public ParameterEditorKind EditorKind { get; }

    /// <summary>
    /// 参数值的稳定类型标识。
    /// </summary>
    public PortTypeId TypeId { get; }

    /// <summary>
    /// 指示该参数是否为必填项。
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// 指示该参数是否因定义约束或宿主限制而只读。
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// 指示当前参数是否允许编辑。
    /// </summary>
    public bool CanEdit => !IsReadOnly;

    /// <summary>
    /// 宿主 UI 可用的只读原因说明。
    /// </summary>
    public string? ReadOnlyReason { get; }

    /// <summary>
    /// 指示当前是否应显示只读原因说明。
    /// </summary>
    public bool HasReadOnlyReason => !string.IsNullOrWhiteSpace(ReadOnlyReason);

    /// <summary>
    /// 参数约束公开的允许选项集合。
    /// </summary>
    public IReadOnlyList<NodeParameterOptionViewModel> Options { get; }

    /// <summary>
    /// 指示当前参数是否使用文本型编辑器。
    /// </summary>
    public bool IsText => EditorKind is ParameterEditorKind.Text or ParameterEditorKind.Color;

    /// <summary>
    /// 指示当前参数是否使用数字编辑器。
    /// </summary>
    public bool IsNumber => EditorKind == ParameterEditorKind.Number;

    /// <summary>
    /// 指示当前参数是否使用布尔编辑器。
    /// </summary>
    public bool IsBoolean => EditorKind == ParameterEditorKind.Boolean;

    /// <summary>
    /// 指示当前参数是否使用枚举编辑器。
    /// </summary>
    public bool IsEnum => EditorKind == ParameterEditorKind.Enum;

    /// <summary>
    /// 指示当前参数是否使用颜色编辑器。
    /// </summary>
    public bool IsColor => EditorKind == ParameterEditorKind.Color;

    /// <summary>
    /// Indicates the parameter uses a multiline list editor.
    /// </summary>
    public bool IsList => EditorKind == ParameterEditorKind.List;

    /// <summary>
    /// 指示当前参数是否通过文本输入控件编辑。
    /// </summary>
    public bool UsesTextInput => IsText || IsNumber || IsColor;

    /// <summary>
    /// Indicates the parameter uses a multiline text input.
    /// </summary>
    public bool UsesMultilineTextInput => IsList;

    /// <summary>
    /// Optional grouped inspector heading shown before the parameter card.
    /// </summary>
    public string? GroupDisplayName { get; }

    /// <summary>
    /// Whether the grouped inspector heading should be shown before this parameter.
    /// </summary>
    public bool IsGroupHeaderVisible { get; }

    /// <summary>
    /// Optional placeholder text provided by the parameter definition.
    /// </summary>
    public string? PlaceholderText { get; }

    /// <summary>
    /// 可见的参数帮助提示，聚合默认值、格式约束等高信号说明。
    /// </summary>
    public string? HelpText { get; }

    /// <summary>
    /// 指示当前是否存在显式帮助提示。
    /// </summary>
    public bool HasHelpText => !string.IsNullOrWhiteSpace(HelpText);

    /// <summary>
    /// Watermark shown by shipped text-based editors.
    /// </summary>
    public string InputWatermark => HasMixedValues ? MixedValueHint : PlaceholderText ?? string.Empty;

    /// <summary>
    /// 指示当前参数是否存在验证错误。
    /// </summary>
    public bool HasValidationError => !IsValid;

    /// <summary>
    /// 指示当前多选投影是否包含混合值。
    /// </summary>
    public bool HasMixedValues => _hasMixedValues;

    /// <summary>
    /// 混合值提示文本；无混合值时回退到类型标识文本。
    /// </summary>
    public string MixedValueHint => HasMixedValues ? "Multiple values" : TypeId.Value;

    /// <summary>
    /// 布尔编辑器标题文本；混合值时显示占位提示。
    /// </summary>
    public string BooleanCaption => HasMixedValues ? "Multiple values" : "Enabled";

    /// <summary>
    /// 当前已规范化的参数值；混合值投影时为 <see langword="null"/>。
    /// </summary>
    public object? CurrentValue { get; private set; }

    /// <summary>
    /// 指示当前参数是否允许恢复定义默认值。
    /// </summary>
    public bool CanResetToDefault
        => CanEdit
           && (HasMixedValues || !NodeParameterValueAdapter.AreEquivalent(CurrentValue, _defaultValue));

    [ObservableProperty]
    private string stringValue = string.Empty;

    [ObservableProperty]
    private bool? boolValue;

    [ObservableProperty]
    private NodeParameterOptionViewModel? selectedOption;

    [ObservableProperty]
    private bool isValid = true;

    [ObservableProperty]
    private string? validationMessage;

    partial void OnStringValueChanged(string value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        ValidateAndApply(value, commit: true);
    }

    partial void OnBoolValueChanged(bool? value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        if (value is null)
        {
            return;
        }

        ValidateAndApply(value, commit: true);
    }

    partial void OnSelectedOptionChanged(NodeParameterOptionViewModel? value)
    {
        if (_suppressChangeNotifications || IsReadOnly)
        {
            return;
        }

        ValidateAndApply(value?.Value, commit: true);
    }

    /// <summary>
    /// 将当前参数恢复到定义默认值。
    /// </summary>
    public void ResetToDefault()
    {
        if (!CanResetToDefault)
        {
            return;
        }

        SetFromCurrentValue(_defaultValue);
        ValidateAndApply(_defaultValue, commit: true);
    }

    private void InitializeValues(IReadOnlyList<object?> currentValues)
    {
        var normalizedValues = currentValues
            .Select(NodeParameterValueAdapter.NormalizeIncomingValue)
            .ToList();
        var firstValue = normalizedValues[0];
        _hasMixedValues = normalizedValues.Skip(1).Any(value => !NodeParameterValueAdapter.AreEquivalent(value, firstValue));

        if (_hasMixedValues)
        {
            _suppressChangeNotifications = true;
            CurrentValue = null;
            StringValue = string.Empty;
            BoolValue = null;
            SelectedOption = null;
            IsValid = true;
            ValidationMessage = null;
            _suppressChangeNotifications = false;
            RaiseSelectionStatePropertyChanges();
            return;
        }

        SetFromCurrentValue(firstValue);
        ValidateAndApply(CurrentValue, commit: false);
        RaiseSelectionStatePropertyChanges();
    }

    private void SetFromCurrentValue(object? value)
    {
        _suppressChangeNotifications = true;

        CurrentValue = value;
        StringValue = NodeParameterValueAdapter.FormatValueForEditor(value);
        BoolValue = NodeParameterValueAdapter.TryNormalizeBoolean(value, out var boolean) ? boolean : null;
        SelectedOption = Options.FirstOrDefault(option => option.Value.Equals(NodeParameterValueAdapter.FormatValueForEditor(value), StringComparison.Ordinal));

        _suppressChangeNotifications = false;
        OnPropertyChanged(nameof(CanResetToDefault));
    }

    private void ValidateAndApply(object? candidateValue, bool commit)
    {
        var result = NodeParameterValueAdapter.NormalizeValue(Definition, candidateValue);

        IsValid = result.IsValid;
        ValidationMessage = result.ValidationError;
        OnPropertyChanged(nameof(HasValidationError));

        if (!IsValid)
        {
            return;
        }

        CurrentValue = result.Value;
        if (_hasMixedValues)
        {
            _hasMixedValues = false;
            RaiseSelectionStatePropertyChanges();
        }
        else
        {
            OnPropertyChanged(nameof(CanResetToDefault));
        }

        if (commit)
        {
            _applyValue(this, result.Value);
        }
    }

    private void RaiseSelectionStatePropertyChanges()
    {
        OnPropertyChanged(nameof(HasMixedValues));
        OnPropertyChanged(nameof(MixedValueHint));
        OnPropertyChanged(nameof(BooleanCaption));
        OnPropertyChanged(nameof(InputWatermark));
        OnPropertyChanged(nameof(CanResetToDefault));
    }

    private static string? BuildReadOnlyReason(NodeParameterDefinition definition, bool isHostReadOnly)
    {
        if (definition.Constraints.IsReadOnly && isHostReadOnly)
        {
            return "参数定义和宿主策略都将此字段标记为只读。";
        }

        if (definition.Constraints.IsReadOnly)
        {
            return "参数定义将此字段标记为只读。";
        }

        if (isHostReadOnly)
        {
            return "宿主策略当前禁止修改该参数。";
        }

        return null;
    }

    private static string? BuildHelpText(NodeParameterDefinition definition, object? defaultValue)
    {
        var guidance = new List<string>();
        var formattedDefault = NodeParameterValueAdapter.FormatValueForEditor(defaultValue);
        if (!string.IsNullOrWhiteSpace(definition.PlaceholderText))
        {
            guidance.Add($"提示: {definition.PlaceholderText}");
        }

        if (!string.IsNullOrWhiteSpace(formattedDefault))
        {
            guidance.Add($"默认值: {formattedDefault}");
        }

        if (!string.IsNullOrWhiteSpace(definition.Constraints.ValidationPatternDescription))
        {
            guidance.Add($"格式: {definition.Constraints.ValidationPatternDescription}");
        }

        if (definition.Constraints.Minimum is double min && definition.Constraints.Maximum is double max)
        {
            guidance.Add($"范围: {min} - {max}");
        }
        else if (definition.Constraints.Minimum is double minimum)
        {
            guidance.Add($"最小值: {minimum}");
        }
        else if (definition.Constraints.Maximum is double maximum)
        {
            guidance.Add($"最大值: {maximum}");
        }

        if (definition.Constraints.MinimumLength is int minLength && definition.Constraints.MaximumLength is int maxLength)
        {
            guidance.Add($"长度: {minLength} - {maxLength}");
        }
        else if (definition.Constraints.MinimumLength is int minimumLength)
        {
            guidance.Add($"最短长度: {minimumLength}");
        }
        else if (definition.Constraints.MaximumLength is int maximumLength)
        {
            guidance.Add($"最长长度: {maximumLength}");
        }

        if (definition.Constraints.MinimumItemCount is int minItems && definition.Constraints.MaximumItemCount is int maxItems)
        {
            guidance.Add($"项目数: {minItems} - {maxItems}");
        }
        else if (definition.Constraints.MinimumItemCount is int minimumItems)
        {
            guidance.Add($"最少项目: {minimumItems}");
        }
        else if (definition.Constraints.MaximumItemCount is int maximumItems)
        {
            guidance.Add($"最多项目: {maximumItems}");
        }

        return guidance.Count == 0
            ? null
            : string.Join("  ·  ", guidance);
    }

}
