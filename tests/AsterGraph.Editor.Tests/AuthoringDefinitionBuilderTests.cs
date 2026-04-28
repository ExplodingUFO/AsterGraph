using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AuthoringDefinitionBuilderTests
{
    [Fact]
    public void NodeDefinitionBuilder_BuildsExistingNodeDefinitionShape()
    {
        var definition = NodeDefinitionBuilder
            .Create("tests.builder.node", "Builder Node")
            .Category("Tests")
            .Subtitle("Thin wrapper")
            .Description("Builder proof")
            .Accent("#6AD5C4")
            .Size(320, 180)
            .Input("in", "Input", "string", "#F3B36B")
            .Output(PortDefinitionBuilder
                .Create("out", "Output", "string")
                .Accent("#6AD5C4")
                .Description("Output port")
                .Group("Results")
                .Connections(max: 1)
                .Build())
            .Parameter(NodeParameterDefinitionBuilder
                .Create("mode", "Mode", "enum", ParameterEditorKind.Enum)
                .DefaultValue("fast")
                .Group("Runtime")
                .Placeholder("Choose mode")
                .Template("tests.mode")
                .Help("Controls processing mode.")
                .SortOrder(10)
                .Option("fast", "Fast")
                .Option("safe", "Safe", "Prefer validation")
                .Build())
            .Build();

        Assert.Equal(new NodeDefinitionId("tests.builder.node"), definition.Id);
        Assert.Equal("Builder Node", definition.DisplayName);
        Assert.Equal("Tests", definition.Category);
        Assert.Equal("Thin wrapper", definition.Subtitle);
        Assert.Equal("Builder proof", definition.Description);
        Assert.Equal("#6AD5C4", definition.AccentHex);
        Assert.Equal(320, definition.DefaultWidth);
        Assert.Equal(180, definition.DefaultHeight);
        Assert.Single(definition.InputPorts);
        Assert.Single(definition.OutputPorts);
        Assert.Single(definition.Parameters);
    }

    [Fact]
    public void PortDefinitionBuilder_BuildsExistingPortDefinitionShape()
    {
        var port = PortDefinitionBuilder
            .Create("payload", "Payload", "json")
            .Accent("#DD88FF")
            .Description("Payload input")
            .Group("Data")
            .Connections(min: 1, max: 2)
            .Build();

        var expected = new PortDefinition(
            "payload",
            "Payload",
            new PortTypeId("json"),
            "#DD88FF",
            "Payload input",
            "Data",
            1,
            2);
        Assert.Equal(expected, port);
    }

    [Fact]
    public void NodeParameterDefinitionBuilder_BuildsExistingParameterDefinitionShape()
    {
        var parameter = NodeParameterDefinitionBuilder
            .Create("threshold", "Threshold", "float", ParameterEditorKind.Number)
            .Required()
            .Description("Cutoff")
            .DefaultValue(0.7d)
            .Range(0, 1)
            .Group("Scoring")
            .Placeholder("0.0 - 1.0")
            .Template("tests.threshold")
            .Help("Tune the cutoff.")
            .SortOrder(3)
            .Advanced()
            .Unit("score")
            .ContractVersion(2)
            .Build();

        var expected = new NodeParameterDefinition(
            "threshold",
            "Threshold",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            true,
            "Cutoff",
            0.7d,
            new ParameterConstraints(Minimum: 0, Maximum: 1),
            "Scoring",
            "0.0 - 1.0",
            "tests.threshold",
            "Tune the cutoff.",
            3,
            true,
            "score",
            2);
        Assert.Equal(expected, parameter);
    }

    [Fact]
    public void ImplicitConversionRuleBuilder_BuildsExistingImplicitConversionRuleShape()
    {
        var rule = ImplicitConversionRuleBuilder
            .ImplicitConversion("int", "double")
            .Conversion("tests.int-to-double")
            .Build();

        Assert.Equal(new ImplicitConversionRule(
            new PortTypeId("int"),
            new PortTypeId("double"),
            new ConversionId("tests.int-to-double")), rule);
    }
}