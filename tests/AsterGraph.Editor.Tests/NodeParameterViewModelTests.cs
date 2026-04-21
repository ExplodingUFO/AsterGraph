using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
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

        viewModel.ResetToDefault();

        Assert.True(viewModel.IsUsingDefaultValue);
        Assert.False(viewModel.IsOverriddenFromDefault);
        Assert.Equal("默认", viewModel.ValueStateCaption);
    }
}
