using System.Globalization;
using System.Text.Json;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Parameters;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeParameterValueAdapterTests
{
    [Fact]
    public void NormalizeValue_NumberAcceptsCommaAndDotFormats()
    {
        var definition = CreateNumberDefinition(isInt: false);

        var commaResult = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Threshold",
            new PortTypeId("double"),
            ParameterEditorKind.Number,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "1,5");

        var pointResult = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Threshold",
            new PortTypeId("double"),
            ParameterEditorKind.Number,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "1.5");

        Assert.True(commaResult.IsValid);
        Assert.Equal(1.5d, Assert.IsType<double>(commaResult.Value));
        Assert.True(pointResult.IsValid);
        Assert.Equal(1.5d, Assert.IsType<double>(pointResult.Value));
    }

    [Fact]
    public void NormalizeValue_EnumValidatesAgainstAllowedOptions()
    {
        var definition = CreateDefinition(ParameterEditorKind.Enum, "string");
        var result = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Mode",
            new PortTypeId("string"),
            ParameterEditorKind.Enum,
            isRequired: false,
            allowedOptionValues: ["fast", "precise"],
            candidateValue: "draft");

        Assert.False(result.IsValid);
        Assert.Equal("Mode must be one of the declared options.", result.ValidationError);
    }

    [Fact]
    public void NormalizeValue_BooleanParsesText()
    {
        var definition = CreateDefinition(ParameterEditorKind.Boolean, "bool");
        var result = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Enabled",
            new PortTypeId("bool"),
            ParameterEditorKind.Boolean,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "true");

        Assert.True(result.IsValid);
        Assert.True(Assert.IsType<bool>(result.Value));
    }

    [Fact]
    public void NormalizeValue_RequiredRejectsWhitespace()
    {
        var definition = CreateDefinition(ParameterEditorKind.Text, "string");
        var result = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Title",
            new PortTypeId("string"),
            ParameterEditorKind.Text,
            isRequired: true,
            allowedOptionValues: [],
            candidateValue: "   ");

        Assert.False(result.IsValid);
        Assert.Equal("Title is required.", result.ValidationError);
    }

    [Fact]
    public void NormalizeValue_NumberEnforcesBoundsAndWholeNumbers()
    {
        var definition = CreateNumberDefinition(isInt: true, minimum: 1, maximum: 5);

        var notWhole = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Count",
            new PortTypeId("int"),
            ParameterEditorKind.Number,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "2.5");

        var outOfRange = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Count",
            new PortTypeId("int"),
            ParameterEditorKind.Number,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "10");

        var success = NodeParameterValueAdapter.NormalizeValue(
            definition,
            "Count",
            new PortTypeId("int"),
            ParameterEditorKind.Number,
            isRequired: false,
            allowedOptionValues: [],
            candidateValue: "3");

        Assert.False(notWhole.IsValid);
        Assert.Equal("Count must be a whole number.", notWhole.ValidationError);
        Assert.False(outOfRange.IsValid);
        Assert.Equal("Count must be <= 5.", outOfRange.ValidationError);
        Assert.True(success.IsValid);
        Assert.Equal(3, Assert.IsType<int>(success.Value));
    }

    [Fact]
    public void NormalizeIncomingValue_ReadsJsonAndFormatUsesInvariant()
    {
        using var json = JsonDocument.Parse("""{"number": 1.5}""");
        var normalized = NodeParameterValueAdapter.NormalizeIncomingValue(json.RootElement.GetProperty("number"));
        var formatted = NodeParameterValueAdapter.FormatValueForEditor(normalized);

        Assert.Equal(1.5d, Assert.IsType<double>(normalized));
        Assert.Equal("1.5", formatted);
    }

    private static NodeParameterDefinition CreateDefinition(ParameterEditorKind kind, string valueType)
        => new(
            key: "value",
            displayName: "Value",
            valueType: new PortTypeId(valueType),
            editorKind: kind);

    private static NodeParameterDefinition CreateNumberDefinition(bool isInt, double? minimum = null, double? maximum = null)
        => new(
            key: "value",
            displayName: "Value",
            valueType: new PortTypeId(isInt ? "int" : "double"),
            editorKind: ParameterEditorKind.Number,
            constraints: new ParameterConstraints(Minimum: minimum, Maximum: maximum));
}
