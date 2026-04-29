using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Documentation;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeDocumentationProviderTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.docs.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.docs.target");

    [Fact]
    public void Provider_ProjectsNodeAndPortDocumentationFromDefinitionMetadata()
    {
        var provider = new NodeDocumentationProvider(CreateCatalog());

        var node = provider.GetNodeDocumentation(TargetDefinitionId);
        var port = provider.GetPortDocumentation(TargetDefinitionId, "in", PortDirection.Input);

        Assert.NotNull(node);
        Assert.Equal("Documentation Target", node!.DisplayName);
        Assert.Contains("Receives reviewed payloads.", node.HelpText);

        Assert.NotNull(port);
        Assert.Equal("Review Input", port!.DisplayName);
        Assert.Contains("Payload accepted by the reviewer.", port.HelpText);
        Assert.Contains("Flow", port.HelpText);
        Assert.Contains("Connections: 1 - 2.", port.HelpText);
    }

    [Fact]
    public void Provider_ProjectsInspectorHelpWithResetExampleAndValidationRules()
    {
        var provider = new NodeDocumentationProvider(CreateCatalog());

        var parameter = provider.GetParameterDocumentation(TargetDefinitionId, "slug");

        Assert.NotNull(parameter);
        Assert.Contains("Used in exports.", parameter!.InspectorHelpText);
        Assert.Contains("Reset restores default value valid-id.", parameter.InspectorHelpText);
        Assert.Contains("Example: lowercase-id.", parameter.InspectorHelpText);
        Assert.Contains("Must match lowercase letters and dashes.", parameter.InspectorHelpText);
    }

    [Fact]
    public void Provider_ProjectsEdgeDocumentationFromEndpointMetadata()
    {
        var provider = new NodeDocumentationProvider(CreateCatalog());
        var document = CreateDocument();

        var documentation = provider.GetEdgeDocumentation(document.Connections[0], document);

        Assert.NotNull(documentation);
        Assert.Equal("Source.Output -> Target.Review Input", documentation!.HelpText?.Split("  ·  ")[0]);
        Assert.Contains("Payload emitted by the source.", documentation.HelpText);
        Assert.Contains("Payload accepted by the reviewer.", documentation.HelpText);
    }

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Documentation Source",
            "Tests",
            "Source node",
            [],
            [
                new PortDefinition(
                    "out",
                    "Output",
                    new PortTypeId("payload"),
                    "#55D8C1",
                    description: "Payload emitted by the source.",
                    groupName: "Flow"),
            ],
            description: "Produces payloads."));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Documentation Target",
            "Tests",
            "Target node",
            [
                new PortDefinition(
                    "in",
                    "Review Input",
                    new PortTypeId("payload"),
                    "#F3B36B",
                    description: "Payload accepted by the reviewer.",
                    groupName: "Flow",
                    minConnections: 1,
                    maxConnections: 2),
            ],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "slug",
                    "Slug",
                    new PortTypeId("string"),
                    ParameterEditorKind.Text,
                    defaultValue: "valid-id",
                    constraints: new ParameterConstraints(
                        MinimumLength: 3,
                        ValidationPattern: "^[a-z-]+$",
                        ValidationPatternDescription: "lowercase letters and dashes"),
                    placeholderText: "lowercase-id",
                    helpText: "Used in exports."),
            ],
            description: "Receives reviewed payloads."));
        return catalog;
    }

    private static GraphDocument CreateDocument()
        => new(
            "Documentation graph",
            "Projects endpoint documentation.",
            [
                new GraphNode(
                    "source",
                    "Source",
                    "Tests",
                    "Source node",
                    "Source instance.",
                    new GraphPoint(0, 0),
                    new GraphSize(220, 160),
                    [],
                    [new GraphPort("out", "Output", PortDirection.Output, "payload", "#55D8C1", new PortTypeId("payload"))],
                    "#55D8C1",
                    SourceDefinitionId),
                new GraphNode(
                    "target",
                    "Target",
                    "Tests",
                    "Target node",
                    "Target instance.",
                    new GraphPoint(320, 0),
                    new GraphSize(220, 160),
                    [new GraphPort("in", "Review Input", PortDirection.Input, "payload", "#F3B36B", new PortTypeId("payload"))],
                    [],
                    "#F3B36B",
                    TargetDefinitionId),
            ],
            [new GraphConnection("edge-001", "source", "out", "target", "in", "payload", "#55D8C1")]);
}

