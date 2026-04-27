using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorValidationSnapshotTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.validation.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.validation.target");
    private static readonly NodeDefinitionId ParameterDefinitionId = new("tests.validation.parameter");
    private static readonly PortTypeId FlowTypeId = new("flow");
    private static readonly PortTypeId TextTypeId = new("string");

    [Fact]
    public void Queries_ReturnReadySnapshotForCleanGraph()
    {
        var session = CreateSession(CreateCleanDocument(), CreateConnectionCatalog());

        var snapshot = session.Queries.GetValidationSnapshot();
        var inspection = session.Diagnostics.CaptureInspectionSnapshot();

        Assert.True(snapshot.IsReady);
        Assert.Equal(0, snapshot.ErrorCount);
        Assert.Equal(0, snapshot.WarningCount);
        Assert.Empty(snapshot.Issues);
        Assert.Equal(snapshot.IsReady, inspection.ValidationSnapshot.IsReady);
        Assert.Equal(snapshot.ErrorCount, inspection.ValidationSnapshot.ErrorCount);
        Assert.Equal(snapshot.WarningCount, inspection.ValidationSnapshot.WarningCount);
        Assert.Equal(snapshot.Issues.Count, inspection.ValidationSnapshot.Issues.Count);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "query.validation-snapshot" && descriptor.IsAvailable);
    }

    [Fact]
    public void Queries_ReportMissingConnectionEndpointAsReadinessError()
    {
        var document = CreateCleanDocument() with
        {
            Connections =
            [
                new GraphConnection(
                    "connection-missing-target-input",
                    "source-001",
                    "out",
                    "target-001",
                    "missing",
                    "Broken",
                    "#6AD5C4"),
            ],
        };
        var session = CreateSession(document, CreateConnectionCatalog());

        var snapshot = session.Queries.GetValidationSnapshot();

        Assert.False(snapshot.IsReady);
        Assert.Equal(1, snapshot.ErrorCount);
        var issue = Assert.Single(snapshot.Issues);
        Assert.Equal("connection.target-input-missing", issue.Code);
        Assert.Equal(GraphEditorValidationIssueSeverity.Error, issue.Severity);
        Assert.Equal("connection-missing-target-input", issue.ConnectionId);
        Assert.Equal("target-001", issue.NodeId);
        Assert.Equal("missing", issue.EndpointId);
        Assert.Equal(GraphConnectionTargetKind.Port, issue.TargetKind);
    }

    [Fact]
    public void Queries_ReportIncompatibleConnectionTypesAsReadinessError()
    {
        var document = CreateCleanDocument() with
        {
            Nodes =
            [
                CreateSourceNode(),
                new GraphNode(
                    "target-001",
                    "Target",
                    "Tests",
                    "Validation",
                    "Consumes text.",
                    new GraphPoint(420, 120),
                    new GraphSize(220, 120),
                    [new GraphPort("in", "In", PortDirection.Input, "string", "#F3B36B", TextTypeId)],
                    [],
                    "#F3B36B",
                    TargetDefinitionId),
            ],
        };
        var session = CreateSession(document, CreateConnectionCatalog(targetTypeId: TextTypeId));

        var snapshot = session.Queries.GetValidationSnapshot();

        Assert.False(snapshot.IsReady);
        var issue = Assert.Single(snapshot.Issues);
        Assert.Equal("connection.incompatible-endpoint-types", issue.Code);
        Assert.Equal(GraphEditorValidationIssueSeverity.Error, issue.Severity);
        Assert.Contains("flow -> string", issue.Message, StringComparison.Ordinal);
        Assert.Equal("connection-001", issue.ConnectionId);
    }

    [Fact]
    public void Queries_ReportInvalidRequiredParametersAsReadinessError()
    {
        var session = CreateSession(
            new GraphDocument(
                "Parameter Validation",
                "Required parameter coverage.",
                [
                    new GraphNode(
                        "parameter-001",
                        "Parameter Node",
                        "Tests",
                        "Validation",
                        "Requires a prompt.",
                        new GraphPoint(120, 120),
                        new GraphSize(220, 120),
                        [],
                        [],
                        "#8B7BFF",
                        ParameterDefinitionId),
                ],
                []),
            CreateParameterCatalog());

        var snapshot = session.Queries.GetValidationSnapshot();

        Assert.False(snapshot.IsReady);
        var issue = Assert.Single(snapshot.Issues);
        Assert.Equal("node.parameter-invalid", issue.Code);
        Assert.Equal("parameter-001", issue.NodeId);
        Assert.Equal("prompt", issue.ParameterKey);
        Assert.Contains("Prompt is required.", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Queries_ReportUnresolvedDefinitionAsNonBlockingWarning()
    {
        var session = CreateSession(
            new GraphDocument(
                "Unresolved Definition",
                "Unresolved definition coverage.",
                [
                    new GraphNode(
                        "unresolved-001",
                        "Unresolved",
                        "Tests",
                        "Validation",
                        "References a missing definition.",
                        new GraphPoint(120, 120),
                        new GraphSize(220, 120),
                        [],
                        [],
                        "#8B7BFF",
                        new NodeDefinitionId("tests.validation.missing")),
                ],
                []),
            new NodeCatalog());

        var snapshot = session.Queries.GetValidationSnapshot();

        Assert.True(snapshot.IsReady);
        Assert.Equal(0, snapshot.ErrorCount);
        Assert.Equal(1, snapshot.WarningCount);
        var issue = Assert.Single(snapshot.Issues);
        Assert.Equal("node.definition-unresolved", issue.Code);
        Assert.Equal(GraphEditorValidationIssueSeverity.Warning, issue.Severity);
        Assert.Equal("unresolved-001", issue.NodeId);
    }

    private static IGraphEditorSession CreateSession(GraphDocument document, INodeCatalog catalog)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = document,
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateCleanDocument()
        => new(
            "Validation Graph",
            "Document-level validation coverage.",
            [
                CreateSourceNode(),
                new GraphNode(
                    "target-001",
                    "Target",
                    "Tests",
                    "Validation",
                    "Consumes flow.",
                    new GraphPoint(420, 120),
                    new GraphSize(220, 120),
                    [new GraphPort("in", "In", PortDirection.Input, "flow", "#F3B36B", FlowTypeId)],
                    [],
                    "#F3B36B",
                    TargetDefinitionId),
            ],
            [
                new GraphConnection("connection-001", "source-001", "out", "target-001", "in", "Source to target", "#6AD5C4"),
            ]);

    private static GraphNode CreateSourceNode()
        => new(
            "source-001",
            "Source",
            "Tests",
            "Validation",
            "Produces flow.",
            new GraphPoint(120, 120),
            new GraphSize(220, 120),
            [],
            [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", FlowTypeId)],
            "#6AD5C4",
            SourceDefinitionId);

    private static INodeCatalog CreateConnectionCatalog(PortTypeId? targetTypeId = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Validation",
            [],
            [new PortDefinition("out", "Out", FlowTypeId, "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Validation",
            [new PortDefinition("in", "In", targetTypeId ?? FlowTypeId, "#F3B36B")],
            []));
        return catalog;
    }

    private static INodeCatalog CreateParameterCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            ParameterDefinitionId,
            "Parameter Node",
            "Tests",
            "Validation",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "prompt",
                    "Prompt",
                    TextTypeId,
                    ParameterEditorKind.Text,
                    isRequired: true),
            ]));
        return catalog;
    }
}
