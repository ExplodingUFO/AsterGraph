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
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var commaResult = NodeParameterValueAdapter.NormalizeValue(definition, "1,5");
            var pointResult = NodeParameterValueAdapter.NormalizeValue(definition, "1.5");

            Assert.True(commaResult.IsValid);
            Assert.Equal(1.5d, Assert.IsType<double>(commaResult.Value));
            Assert.True(pointResult.IsValid);
            Assert.Equal(1.5d, Assert.IsType<double>(pointResult.Value));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void NormalizeValue_ThousandSeparatorRespectsCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var definition = CreateNumberDefinition(isInt: false);

            var result = NodeParameterValueAdapter.NormalizeValue(definition, "1,000");

            Assert.True(result.IsValid);
            Assert.Equal(1000d, Assert.IsType<double>(result.Value));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void NormalizeValue_EnumValidatesAgainstAllowedOptions()
    {
        var definition = CreateDefinition(
            ParameterEditorKind.Enum,
            "string",
            displayName: "Mode",
            allowedOptions:
            [
                new ParameterOptionDefinition("fast", "Fast"),
                new ParameterOptionDefinition("precise", "Precise"),
            ]);
        var result = NodeParameterValueAdapter.NormalizeValue(definition, "draft");

        Assert.False(result.IsValid);
        Assert.Equal("Mode must be one of the declared options.", result.ValidationError);
    }

    [Fact]
    public void NormalizeValue_BooleanParsesText()
    {
        var definition = CreateDefinition(ParameterEditorKind.Boolean, "bool", displayName: "Enabled");
        var result = NodeParameterValueAdapter.NormalizeValue(definition, "true");

        Assert.True(result.IsValid);
        Assert.True(Assert.IsType<bool>(result.Value));
    }

    [Fact]
    public void NormalizeValue_RequiredRejectsWhitespace()
    {
        var definition = CreateDefinition(ParameterEditorKind.Text, "string", displayName: "Title", isRequired: true);
        var result = NodeParameterValueAdapter.NormalizeValue(definition, "   ");

        Assert.False(result.IsValid);
        Assert.Equal("Title is required.", result.ValidationError);
    }

    [Fact]
    public void NormalizeValue_NumberEnforcesBoundsAndWholeNumbers()
    {
        var definition = CreateNumberDefinition(isInt: true, minimum: 1, maximum: 5, displayName: "Count");

        var notWhole = NodeParameterValueAdapter.NormalizeValue(definition, "2.5");
        var outOfRange = NodeParameterValueAdapter.NormalizeValue(definition, "10");
        var success = NodeParameterValueAdapter.NormalizeValue(definition, "3");

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

    [Fact]
    public void NormalizeValue_GroupingInputRespectsCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var definition = CreateNumberDefinition(isInt: false, displayName: "Amount");

            var result = NodeParameterValueAdapter.NormalizeValue(definition, "1.000");

            Assert.True(result.IsValid);
            Assert.Equal(1000d, Assert.IsType<double>(result.Value));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void NormalizeValue_MixedSeparatorsUseCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var definition = CreateNumberDefinition(isInt: false, displayName: "Amount");

            var result = NodeParameterValueAdapter.NormalizeValue(definition, "1.234,5");

            Assert.True(result.IsValid);
            Assert.Equal(1234.5d, Assert.IsType<double>(result.Value));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    private static NodeParameterDefinition CreateDefinition(
        ParameterEditorKind kind,
        string valueType,
        string displayName = "Value",
        bool isRequired = false,
        IReadOnlyList<ParameterOptionDefinition>? allowedOptions = null)
        => new(
            key: "value",
            displayName: displayName,
            valueType: new PortTypeId(valueType),
            editorKind: kind,
            isRequired: isRequired,
            constraints: new ParameterConstraints(AllowedOptions: allowedOptions));

    private static NodeParameterDefinition CreateNumberDefinition(
        bool isInt,
        double? minimum = null,
        double? maximum = null,
        string displayName = "Value")
        => new(
            key: "value",
            displayName: displayName,
            valueType: new PortTypeId(isInt ? "int" : "double"),
            editorKind: ParameterEditorKind.Number,
            constraints: new ParameterConstraints(Minimum: minimum, Maximum: maximum));
}
