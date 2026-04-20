using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeSurfaceInputRowViewModelTests
{
    [Fact]
    public void Constructor_PortRowWithoutPort_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new NodeSurfaceInputRowViewModel(
            NodeSurfaceInputRowKind.Port,
            "Input",
            new PortTypeId("float")));
    }

    [Fact]
    public void Constructor_ParameterRowWithoutEndpoint_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new NodeSurfaceInputRowViewModel(
            NodeSurfaceInputRowKind.ParameterEndpoint,
            "Gain",
            new PortTypeId("float")));
    }

    [Fact]
    public void Constructor_PortRowRejectsParameterEndpointPayload()
    {
        Assert.Throws<InvalidOperationException>(() => new NodeSurfaceInputRowViewModel(
            NodeSurfaceInputRowKind.Port,
            "Input",
            new PortTypeId("float"),
            port: CreatePort(),
            parameterEndpoint: CreateParameterEndpoint()));
    }

    [Fact]
    public void Constructor_EditorModeRejectsInlineDisplayText()
    {
        Assert.Throws<InvalidOperationException>(() => new NodeSurfaceInputRowViewModel(
            NodeSurfaceInputRowKind.ParameterEndpoint,
            "Gain",
            new PortTypeId("float"),
            parameterEndpoint: CreateParameterEndpoint(),
            inlineContentKind: NodeSurfaceInlineContentKind.Editor,
            inlineDisplayText: "0.5"));
    }

    private static PortViewModel CreatePort()
        => new(
            new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
            index: 0,
            total: 1);

    private static NodeParameterEndpointViewModel CreateParameterEndpoint()
    {
        var definition = new NodeParameterDefinition(
            "gain",
            "Gain",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            defaultValue: 0.5d);
        var parameter = new NodeParameterViewModel(
            definition,
            [definition.DefaultValue],
            (_, _) => { });

        return new NodeParameterEndpointViewModel(
            parameter,
            new GraphConnectionTargetRef("node-001", definition.Key, GraphConnectionTargetKind.Parameter));
    }
}
