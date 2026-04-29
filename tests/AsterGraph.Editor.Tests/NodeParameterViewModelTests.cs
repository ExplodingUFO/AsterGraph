using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeParameterViewModelTests
{
    [Fact]
    public void ResetToDefault_AppliesDefinitionDefaultAndClearsResetState()
    {
        object? appliedValue = null;
        var viewModel = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "slug",
                "Slug",
                new PortTypeId("string"),
                ParameterEditorKind.Text,
                defaultValue: "valid-id"),
            ["custom-id"],
            (_, value) => appliedValue = value);

        Assert.True(viewModel.CanResetToDefault);

        viewModel.ResetToDefault();

        Assert.Equal("valid-id", viewModel.CurrentValue);
        Assert.Equal("valid-id", viewModel.StringValue);
        Assert.Equal("valid-id", appliedValue);
        Assert.False(viewModel.HasMixedValues);
        Assert.False(viewModel.CanResetToDefault);
    }

    [Fact]
    public void ReadOnlyReason_ReflectsDefinitionAndHostPolicies()
    {
        var definitionLocked = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "system-key",
                "System Key",
                new PortTypeId("string"),
                ParameterEditorKind.Text,
                defaultValue: "core",
                constraints: new ParameterConstraints(IsReadOnly: true)),
            ["core"],
            static (_, _) => { });

        var hostLocked = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "host-only",
                "Host Only",
                new PortTypeId("string"),
                ParameterEditorKind.Text,
                defaultValue: "session"),
            ["session"],
            static (_, _) => { },
            isHostReadOnly: true);

        Assert.Equal("参数定义将此字段标记为只读。", definitionLocked.ReadOnlyReason);
        Assert.Equal("宿主策略当前禁止修改该参数。", hostLocked.ReadOnlyReason);
    }

    [Fact]
    public void AdvancedMetadata_AndDefaultOverrideState_ProjectFromDefinition()
    {
        var viewModel = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "threshold",
                "Threshold",
                new PortTypeId("float"),
                ParameterEditorKind.Number,
                defaultValue: 0.5d,
                helpText: "Fine-tunes the visible threshold.",
                sortOrder: 20,
                isAdvanced: true,
                unitSuffix: "ms"),
            [0.9d],
            static (_, _) => { });

        Assert.True(viewModel.IsAdvanced);
        Assert.Equal("ms", viewModel.UnitSuffix);
        Assert.True(viewModel.HasUnitSuffix);
        Assert.True(viewModel.IsOverriddenFromDefault);
        Assert.False(viewModel.IsUsingDefaultValue);
        Assert.Equal("已覆盖", viewModel.ValueStateCaption);
        Assert.Contains("Fine-tunes the visible threshold.", viewModel.HelpText);
        Assert.Contains("恢复默认将还原为: 0.5", viewModel.HelpText);

        viewModel.ResetToDefault();

        Assert.True(viewModel.IsUsingDefaultValue);
        Assert.False(viewModel.IsOverriddenFromDefault);
        Assert.Equal("默认", viewModel.ValueStateCaption);
    }

    [Fact]
    public void SnapshotConstructor_UsesSharedAuthoringMetadataWithoutRebuildingSeparateState()
    {
        object? appliedValue = null;
        var definition = new NodeParameterDefinition(
            "threshold",
            "Threshold",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            defaultValue: 0.5d,
            helpText: "Fine-tunes the visible threshold.",
            groupName: "Behavior");
        var snapshot = new GraphEditorNodeParameterSnapshot(
            definition,
            0.9d,
            HasMixedValues: false,
            CanEdit: true,
            IsValid: true,
            ValidationMessage: null,
            CanResetToDefault: true,
            IsUsingDefaultValue: false,
            ReadOnlyReason: null,
            HelpText: "Fine-tunes the visible threshold.",
            GroupDisplayName: "Behavior",
            IsGroupHeaderVisible: true,
            ValueState: GraphEditorNodeParameterValueState.Overridden,
            ValueDisplayText: "0.9");

        var viewModel = new NodeParameterViewModel(snapshot, (_, value) => appliedValue = value);

        Assert.Equal("Behavior", viewModel.GroupDisplayName);
        Assert.True(viewModel.IsGroupHeaderVisible);
        Assert.Equal("Fine-tunes the visible threshold.", viewModel.HelpText);
        Assert.True(viewModel.IsOverriddenFromDefault);
        Assert.Equal("已覆盖", viewModel.ValueStateCaption);
        Assert.Equal("0.9", viewModel.StringValue);

        viewModel.ResetToDefault();

        Assert.Equal(0.5d, viewModel.CurrentValue);
        Assert.Equal(0.5d, appliedValue);
    }

    [Fact]
    public void ValidationFixAction_RestoresValidDefaultForInvalidCurrentValue()
    {
        object? appliedValue = null;
        var viewModel = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "slug",
                "Slug",
                new PortTypeId("string"),
                ParameterEditorKind.Text,
                defaultValue: "valid-slug",
                constraints: new ParameterConstraints(
                    MinimumLength: 3,
                    ValidationPattern: "^[a-z-]+$",
                    ValidationPatternDescription: "lowercase letters and dashes")),
            ["ab"],
            (_, value) => appliedValue = value);

        Assert.False(viewModel.IsValid);
        Assert.True(viewModel.CanApplyValidationFix);
        Assert.Equal("Restore default", viewModel.ValidationFixActionLabel);
        Assert.Contains("恢复默认将还原为: valid-slug", viewModel.HelpText);
        Assert.Contains("格式: lowercase letters and dashes", viewModel.HelpText);

        viewModel.ApplyValidationFix();

        Assert.True(viewModel.IsValid);
        Assert.Equal("valid-slug", viewModel.CurrentValue);
        Assert.Equal("valid-slug", appliedValue);
        Assert.False(viewModel.CanApplyValidationFix);
    }

    [Fact]
    public void EditorAffordances_ProjectCodeEnumAndNumberHints()
    {
        var script = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "script",
                "Script",
                new PortTypeId("code"),
                ParameterEditorKind.Text,
                defaultValue: "run.step()\nreturn.ok()",
                templateKey: "code"),
            [],
            static (_, _) => { });
        var status = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "status",
                "Status",
                new PortTypeId("enum"),
                ParameterEditorKind.Enum,
                defaultValue: "draft",
                constraints: new ParameterConstraints(
                    AllowedOptions:
                    [
                        new ParameterOptionDefinition("draft", "Draft"),
                    ])),
            [],
            static (_, _) => { });
        var priority = new NodeParameterViewModel(
            new NodeParameterDefinition(
                "priority",
                "Priority",
                new PortTypeId("int"),
                ParameterEditorKind.Number,
                defaultValue: 2,
                constraints: new ParameterConstraints(Minimum: 1, Maximum: 5)),
            [],
            static (_, _) => { });

        Assert.True(script.UsesMultilineTextInput);
        Assert.True(script.IsCodeLikeText);
        Assert.True(status.SupportsEnumSearch);
        Assert.Equal("Slider range: 1 - 5", priority.NumberSliderHint);
        Assert.True(priority.HasNumberSliderHint);
    }
}
