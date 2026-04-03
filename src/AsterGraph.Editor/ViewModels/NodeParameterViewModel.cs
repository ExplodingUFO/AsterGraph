using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Parameters;
using System.Linq;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class NodeParameterViewModel : ObservableObject
{
    private readonly Action<NodeParameterViewModel, object?> _applyValue;
    private bool _suppressChangeNotifications;
    private bool _hasMixedValues;

    /// <summary>
    /// 为当前选择投影一个可批量编辑的节点参数。
    /// </summary>
    /// <param name="definition">参数定义快照。</param>
    /// <param name="currentValues">当前选择中的参数值集合。</param>
    /// <param name="applyValue">应用规范化参数值的回调。</param>
    /// <param name="isHostReadOnly">宿主是否强制将该参数标记为只读。</param>
    public NodeParameterViewModel(
        NodeParameterDefinition definition,
        IReadOnlyList<object?> currentValues,
        Action<NodeParameterViewModel, object?> applyValue,
        bool isHostReadOnly = false)
    {
        Definition = definition;
        _applyValue = applyValue;

        Key = definition.Key;
        DisplayName = definition.DisplayName;
        Description = definition.Description;
        EditorKind = definition.EditorKind;
        TypeId = definition.ValueType;
        IsRequired = definition.IsRequired;
        IsReadOnly = definition.Constraints.IsReadOnly || isHostReadOnly;
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
    /// 指示当前参数是否通过文本输入控件编辑。
    /// </summary>
    public bool UsesTextInput => IsText || IsNumber || IsColor;

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

    private void InitializeValues(IReadOnlyList<object?> currentValues)
    {
        var normalizedValues = currentValues
            .Select(NodeParameterValueAdapter.NormalizeIncomingValue)
            .ToList();
        var firstValue = normalizedValues[0];
        _hasMixedValues = normalizedValues.Skip(1).Any(value => !Equals(value, firstValue));

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
            OnPropertyChanged(nameof(HasMixedValues));
            OnPropertyChanged(nameof(MixedValueHint));
            OnPropertyChanged(nameof(BooleanCaption));
            return;
        }

        SetFromCurrentValue(firstValue);
        ValidateAndApply(CurrentValue, commit: false);
        OnPropertyChanged(nameof(HasMixedValues));
        OnPropertyChanged(nameof(MixedValueHint));
        OnPropertyChanged(nameof(BooleanCaption));
    }

    private void SetFromCurrentValue(object? value)
    {
        _suppressChangeNotifications = true;

        CurrentValue = value;
        StringValue = NodeParameterValueAdapter.FormatValueForEditor(value);
        BoolValue = NodeParameterValueAdapter.TryNormalizeBoolean(value, out var boolean) ? boolean : null;
        SelectedOption = Options.FirstOrDefault(option => option.Value.Equals(NodeParameterValueAdapter.FormatValueForEditor(value), StringComparison.Ordinal));

        _suppressChangeNotifications = false;
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

        if (commit)
        {
            _applyValue(this, result.Value);
        }
    }

}
