using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Tests;

internal static class GraphEditorHistoryTestSupport
{
    public const string SourceNodeId = "tests.history.source-001";
    public const string TargetNodeId = "tests.history.target-001";
    public const string SourcePortId = "out";
    public const string TargetPortId = "in";

    public static GraphEditorViewModel CreateEditor(NodeDefinitionId definitionId, IGraphWorkspaceService? workspaceService = null)
        => AsterGraphEditorFactory.Create(CreateOptions(definitionId) with
        {
            WorkspaceService = workspaceService ?? new RecordingWorkspaceService(),
        });

    public static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };

    public static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "History Graph",
            "Focused retained history/save regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "History Source",
                    "Tests",
                    "Runtime",
                    "History source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "History Target",
                    "Tests",
                    "Runtime",
                    "History target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            [
                new GraphConnection(
                    "tests.history.connection-001",
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "History bridge",
                    "#6AD5C4"),
            ]);

    public static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "History Node",
            "Tests",
            "Runtime",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    internal sealed class RecordingWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://history-tests";

        public GraphDocument? SavedDocument { get; private set; }

        public void Save(GraphDocument document)
            => SavedDocument = document;

        public GraphDocument Load()
            => SavedDocument ?? throw new InvalidOperationException("No saved workspace.");

        public bool Exists()
            => SavedDocument is not null;
    }
}
