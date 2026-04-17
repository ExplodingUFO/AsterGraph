using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;

const string SourceNodeId = "hello-source-001";
const string TargetNodeId = "hello-target-001";
const string SourcePortId = "out";
const string TargetPortId = "in";
var definitionId = new NodeDefinitionId("hello.world.node");

var catalog = CreateCatalog(definitionId);
var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
{
    Document = CreateDocument(definitionId),
    NodeCatalog = catalog,
    CompatibilityService = new ExactCompatibilityService(),
});

session.Commands.StartConnection(SourceNodeId, SourcePortId);
session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

var snapshot = session.Queries.CreateDocumentSnapshot();
var featureDescriptors = session.Queries.GetFeatureDescriptors();

if (snapshot.Connections.Count != 1)
{
    throw new InvalidOperationException("HelloWorld sample expected one connection after the runtime-only command flow.");
}

Console.WriteLine("AsterGraph HelloWorld");
Console.WriteLine("Route: AsterGraphEditorFactory.CreateSession(...)");
Console.WriteLine($"Nodes: {snapshot.Nodes.Count}");
Console.WriteLine($"Connections: {snapshot.Connections.Count}");
Console.WriteLine($"Feature descriptors: {featureDescriptors.Count}");
Console.WriteLine("HELLOWORLD_OK:True");

static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
{
    var catalog = new NodeCatalog();
    catalog.RegisterDefinition(new NodeDefinition(
        definitionId,
        "Hello World Node",
        "Hello",
        "Minimal runtime-only node definition for the first-run sample.",
        [
            new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
        ],
        [
            new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
        ]));
    return catalog;
}

static GraphDocument CreateDocument(NodeDefinitionId definitionId)
    => new(
        "Hello World Graph",
        "Minimal runtime-only sample for first-time AsterGraph adoption.",
        [
            new GraphNode(
                SourceNodeId,
                "Hello Source",
                "Hello",
                "Source",
                "Source node for the minimal runtime-only sample.",
                new GraphPoint(120, 160),
                new GraphSize(220, 140),
                [],
                [
                    new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                "#6AD5C4",
                definitionId),
            new GraphNode(
                TargetNodeId,
                "Hello Target",
                "Hello",
                "Target",
                "Target node for the minimal runtime-only sample.",
                new GraphPoint(420, 160),
                new GraphSize(220, 140),
                [
                    new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [],
                "#F3B36B",
                definitionId),
        ],
        []);

file sealed class ExactCompatibilityService : IPortCompatibilityService
{
    public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        => sourceType == targetType
            ? PortCompatibilityResult.Exact()
            : PortCompatibilityResult.Rejected();
}
