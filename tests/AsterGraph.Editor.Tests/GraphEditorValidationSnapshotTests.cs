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
    public void Queries_ProjectParameterHelpTargetSharedWithInspectorSnapshot()
    {
        var session = CreateSession(CreateInvalidParameterValueDocument(), CreateParameterCatalog(defaultPrompt: "Describe the change."));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        Assert.NotNull(issue.HelpTarget);
        Assert.Equal("Parameter", issue.HelpTarget!.Kind);
        Assert.Equal("parameter-001", issue.HelpTarget.NodeId);
        Assert.Equal("prompt", issue.HelpTarget.ParameterKey);
        Assert.Contains("Prompt", issue.HelpTarget.Title, StringComparison.Ordinal);
        Assert.Contains("Describe the change.", issue.HelpTarget.HelpText, StringComparison.Ordinal);

        Assert.True(session.Commands.TryFocusValidationIssue(issue));
        var inspectorParameter = Assert.Single(session.Queries.GetSelectedNodeParameterSnapshots());
        Assert.Equal(issue.HelpTarget.ParameterKey, inspectorParameter.Definition.Key);
        Assert.Equal(inspectorParameter.HelpText, issue.HelpTarget.HelpText);
    }

    [Fact]
    public void Queries_ProjectConnectionHelpTargetForProblemsPanelAndInspectorReview()
    {
        var session = CreateSession(CreateRoutedIncompatibleConnectionDocument(), CreateConnectionCatalog(targetTypeId: TextTypeId));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        Assert.Equal("connection.incompatible-endpoint-types", issue.Code);
        Assert.NotNull(issue.HelpTarget);
        Assert.Equal("Connection", issue.HelpTarget!.Kind);
        Assert.Equal("connection-001", issue.HelpTarget.ConnectionId);
        Assert.Equal("target-001", issue.HelpTarget.NodeId);
        Assert.Equal("in", issue.HelpTarget.EndpointId);
        Assert.Equal("Source.Out -> Target.In", issue.HelpTarget.Title);
        Assert.Contains("flow", issue.HelpTarget.HelpText, StringComparison.Ordinal);
        Assert.Contains("string", issue.HelpTarget.HelpText, StringComparison.Ordinal);
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

    [Fact]
    public void Commands_FocusValidationIssue_SelectsAndCentersAffectedNode()
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
        session.Commands.UpdateViewportSize(800, 600);
        var before = session.Queries.GetViewportSnapshot();
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        var focused = session.Commands.TryFocusValidationIssue(issue);

        Assert.True(focused);
        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Equal(["parameter-001"], selection.SelectedNodeIds);
        Assert.Equal("parameter-001", selection.PrimarySelectedNodeId);
        Assert.Empty(selection.SelectedConnectionIds);
        var after = session.Queries.GetViewportSnapshot();
        Assert.NotEqual(before.PanX, after.PanX);
        Assert.NotEqual(before.PanY, after.PanY);
    }

    [Fact]
    public void Commands_FocusValidationIssue_NavigatesToIssueScopeBeforeFocusingNode()
    {
        var session = CreateSession(CreateScopedParameterDocument(), CreateParameterCatalog());
        session.Commands.UpdateViewportSize(800, 600);
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        var focused = session.Commands.TryFocusValidationIssue(issue);

        Assert.True(focused);
        Assert.Equal("graph-child", session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);
        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Equal(["child-parameter-001"], selection.SelectedNodeIds);
        Assert.Equal("child-parameter-001", selection.PrimarySelectedNodeId);
    }

    [Fact]
    public void Commands_FocusValidationIssue_SelectsAndCentersAffectedConnection()
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
        session.Commands.UpdateViewportSize(800, 600);
        var before = session.Queries.GetViewportSnapshot();
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        var focused = session.Commands.TryFocusValidationIssue(issue);

        Assert.True(focused);
        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Empty(selection.SelectedNodeIds);
        Assert.Equal(["connection-001"], selection.SelectedConnectionIds);
        Assert.Equal("connection-001", selection.PrimarySelectedConnectionId);
        var after = session.Queries.GetViewportSnapshot();
        Assert.NotEqual(before.PanX, after.PanX);
        Assert.NotEqual(before.PanY, after.PanY);
    }

    [Fact]
    public void Commands_FocusValidationIssue_ReturnsFalseForStaleConcreteTargets()
    {
        var session = CreateSession(CreateCleanDocument(), CreateConnectionCatalog());
        var staleConnectionIssue = new GraphEditorValidationIssueSnapshot(
            "connection.stale",
            GraphEditorValidationIssueSeverity.Error,
            "Connection no longer exists.",
            GraphDocument.DefaultRootGraphId,
            nodeId: "target-001",
            connectionId: "missing-connection");
        var staleNodeIssue = new GraphEditorValidationIssueSnapshot(
            "node.stale",
            GraphEditorValidationIssueSeverity.Error,
            "Node no longer exists.",
            GraphDocument.DefaultRootGraphId,
            nodeId: "missing-node");

        Assert.False(session.Commands.TryFocusValidationIssue(staleConnectionIssue));
        Assert.False(session.Commands.TryFocusValidationIssue(staleNodeIssue));
        Assert.Empty(session.Queries.GetSelectionSnapshot().SelectedNodeIds);
        Assert.Empty(session.Queries.GetSelectionSnapshot().SelectedConnectionIds);
    }

    [Fact]
    public void Commands_FocusValidationIssue_FocusesCurrentScopeForDocumentScopedIssue()
    {
        var session = CreateSession(CreateCleanDocument(), CreateConnectionCatalog());
        session.Commands.UpdateViewportSize(800, 600);
        var before = session.Queries.GetViewportSnapshot();
        var issue = new GraphEditorValidationIssueSnapshot(
            "document.scope",
            GraphEditorValidationIssueSeverity.Error,
            "Document requires attention.",
            GraphDocument.DefaultRootGraphId);

        var focused = session.Commands.TryFocusValidationIssue(issue);

        Assert.True(focused);
        Assert.Empty(session.Queries.GetSelectionSnapshot().SelectedNodeIds);
        Assert.Empty(session.Queries.GetSelectionSnapshot().SelectedConnectionIds);
        var after = session.Queries.GetViewportSnapshot();
        Assert.NotEqual(before.Zoom, after.Zoom);
        Assert.NotEqual(before.PanX, after.PanX);
        Assert.NotEqual(before.PanY, after.PanY);
    }

    [Fact]
    public void Queries_ProjectDefaultValueRepairOnlyWhenDefaultIsValid()
    {
        var session = CreateSession(CreateInvalidParameterValueDocument(), CreateParameterCatalog(defaultPrompt: "Describe the change."));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        var repairs = session.Queries.GetValidationIssueRepairActions(issue);

        var repair = Assert.Single(repairs);
        Assert.Equal("validation.parameter.reset-default", repair.ActionId);
        Assert.Equal("node.parameter-invalid", repair.IssueCode);
        Assert.Equal("parameter-001", repair.NodeId);
        Assert.Equal("prompt", repair.ParameterKey);
        Assert.Equal("Reset Prompt to its default value.", repair.PreviewText);
    }

    [Fact]
    public void Commands_ApplyDefaultValueRepairUsesHistoryUndo()
    {
        var session = CreateSession(CreateInvalidParameterValueDocument(), CreateParameterCatalog(defaultPrompt: "Describe the change."));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);
        var repair = Assert.Single(session.Queries.GetValidationIssueRepairActions(issue));

        Assert.True(session.Commands.TryApplyValidationRepair(repair));
        Assert.Empty(session.Queries.GetValidationSnapshot().Issues);

        session.Commands.Undo();

        var restoredIssue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);
        Assert.Equal("node.parameter-invalid", restoredIssue.Code);
    }

    [Fact]
    public void Queries_ProjectConnectionRemovalReconnectAndRouteResetForProvableConnectionIssue()
    {
        var session = CreateSession(CreateRoutedIncompatibleConnectionDocument(), CreateConnectionCatalog(targetTypeId: TextTypeId));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);

        var repairs = session.Queries.GetValidationIssueRepairActions(issue);

        Assert.Contains(repairs, repair =>
            repair.ActionId == "validation.connection.remove"
            && repair.ConnectionId == "connection-001"
            && repair.PreviewText == "Remove invalid connection connection-001.");
        Assert.Contains(repairs, repair =>
            repair.ActionId == "validation.connection.reconnect"
            && repair.ConnectionId == "connection-001"
            && repair.PreviewText == "Reconnect connection-001 from Source.Out.");
        Assert.Contains(repairs, repair =>
            repair.ActionId == "validation.connection.route.reset"
            && repair.ConnectionId == "connection-001"
            && repair.PreviewText == "Remove 1 persisted route vertex from connection-001.");
    }

    [Fact]
    public void Commands_ApplyConnectionRepairUsesExistingUndo()
    {
        var session = CreateSession(CreateRoutedIncompatibleConnectionDocument(), CreateConnectionCatalog(targetTypeId: TextTypeId));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);
        var repair = session.Queries.GetValidationIssueRepairActions(issue)
            .Single(candidate => candidate.ActionId == "validation.connection.remove");

        Assert.True(session.Commands.TryApplyValidationRepair(repair));
        Assert.Empty(session.Queries.CreateDocumentSnapshot().Connections);

        session.Commands.Undo();

        Assert.Single(session.Queries.CreateDocumentSnapshot().Connections);
        Assert.Single(session.Queries.GetValidationSnapshot().Issues);
    }

    [Fact]
    public void Commands_ApplyRouteResetRepairUsesExistingUndo()
    {
        var session = CreateSession(CreateRoutedIncompatibleConnectionDocument(), CreateConnectionCatalog(targetTypeId: TextTypeId));
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues);
        var repair = session.Queries.GetValidationIssueRepairActions(issue)
            .Single(candidate => candidate.ActionId == "validation.connection.route.reset");

        Assert.True(session.Commands.TryApplyValidationRepair(repair));
        var resetConnection = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections);
        Assert.True(resetConnection.Presentation?.Route is null || resetConnection.Presentation.Route.IsEmpty);

        session.Commands.Undo();

        var restoredConnection = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections);
        Assert.Equal(1, restoredConnection.Presentation?.Route?.Vertices.Count);
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

    private static INodeCatalog CreateParameterCatalog(string? defaultPrompt = null)
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
                    isRequired: true,
                    defaultValue: defaultPrompt,
                    helpText: "Prompt guidance for review automation."),
            ]));
        return catalog;
    }

    private static GraphDocument CreateInvalidParameterValueDocument()
        => new(
            "Parameter Repair",
            "Invalid parameter repair coverage.",
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
                    ParameterDefinitionId,
                    [new GraphParameterValue("prompt", TextTypeId, string.Empty)]),
            ],
            []);

    private static GraphDocument CreateRoutedIncompatibleConnectionDocument()
        => CreateCleanDocument() with
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
            Connections =
            [
                new GraphConnection(
                    "connection-001",
                    "source-001",
                    "out",
                    "target-001",
                    "in",
                    "Source to target",
                    "#6AD5C4",
                    Presentation: new GraphEdgePresentation(
                        Route: new GraphConnectionRoute([new GraphPoint(300, 220)]))),
            ],
        };

    private static GraphDocument CreateScopedParameterDocument()
        => GraphDocument.CreateScoped(
            "Scoped Parameter Validation",
            "Required parameter coverage in child scopes.",
            GraphDocument.DefaultRootGraphId,
            [
                new GraphScope(
                    GraphDocument.DefaultRootGraphId,
                    [
                        new GraphNode(
                            "composite-001",
                            "Composite",
                            "Tests",
                            "Validation",
                            "Owns a child graph.",
                            new GraphPoint(120, 120),
                            new GraphSize(220, 120),
                            [],
                            [],
                            "#8B7BFF",
                            Composite: new GraphCompositeNode("graph-child", [], [])),
                    ],
                    []),
                new GraphScope(
                    "graph-child",
                    [
                        new GraphNode(
                            "child-parameter-001",
                            "Child Parameter Node",
                            "Tests",
                            "Validation",
                            "Requires a prompt.",
                            new GraphPoint(180, 160),
                            new GraphSize(220, 120),
                            [],
                            [],
                            "#8B7BFF",
                            ParameterDefinitionId),
                    ],
                    []),
            ]);
}
