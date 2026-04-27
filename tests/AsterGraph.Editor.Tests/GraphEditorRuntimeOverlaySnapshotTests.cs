using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorRuntimeOverlaySnapshotTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.runtime-overlay.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.runtime-overlay.target");

    [Fact]
    public void Queries_ReturnEmptyRuntimeOverlaySnapshotWhenHostDoesNotProvideState()
    {
        var session = CreateSession(null);

        var overlay = session.Queries.GetRuntimeOverlaySnapshot();

        Assert.False(overlay.IsAvailable);
        Assert.Empty(overlay.NodeOverlays);
        Assert.Empty(overlay.ConnectionOverlays);
        Assert.Empty(overlay.RecentLogs);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "query.runtime-overlay-snapshot" && descriptor.IsAvailable);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "integration.runtime-overlay-provider" && !descriptor.IsAvailable);
    }

    [Fact]
    public void Queries_ReturnHostOwnedRuntimeOverlaySnapshots()
    {
        var timestamp = new DateTimeOffset(2026, 4, 27, 8, 0, 0, TimeSpan.Zero);
        var provider = new TestRuntimeOverlayProvider(
            [
                new GraphEditorNodeRuntimeOverlaySnapshot(
                    "source-001",
                    GraphEditorRuntimeOverlayStatus.Success,
                    ElapsedMilliseconds: 12.5,
                    OutputPreview: "approved payload",
                    WarningCount: 1,
                    LastRunAtUtc: timestamp),
                new GraphEditorNodeRuntimeOverlaySnapshot(
                    "target-001",
                    GraphEditorRuntimeOverlayStatus.Error,
                    ErrorCount: 1,
                    ErrorMessage: "Parser failed.",
                    LastRunAtUtc: timestamp),
            ],
            [
                new GraphEditorConnectionRuntimeOverlaySnapshot(
                    "connection-001",
                    GraphEditorRuntimeOverlayStatus.Success,
                    ValuePreview: "{ status: approved }",
                    PayloadType: "review",
                    ItemCount: 1,
                    IsStale: false),
            ],
            [
                new GraphEditorRuntimeLogEntrySnapshot(
                    "log-001",
                    timestamp,
                    GraphEditorRuntimeOverlayStatus.Error,
                    "Parser failed.",
                    ScopeId: "root",
                    NodeId: "target-001",
                    ConnectionId: "connection-001"),
            ]);
        var session = CreateSession(provider);

        var overlay = session.Queries.GetRuntimeOverlaySnapshot();

        Assert.True(overlay.IsAvailable);
        Assert.Equal(2, overlay.NodeOverlays.Count);
        Assert.Contains(overlay.NodeOverlays, node =>
            node.NodeId == "source-001"
            && node.Status == GraphEditorRuntimeOverlayStatus.Success
            && node.ElapsedMilliseconds == 12.5
            && node.OutputPreview == "approved payload"
            && node.WarningCount == 1
            && node.LastRunAtUtc == timestamp);
        Assert.Contains(overlay.NodeOverlays, node =>
            node.NodeId == "target-001"
            && node.Status == GraphEditorRuntimeOverlayStatus.Error
            && node.ErrorCount == 1
            && node.ErrorMessage == "Parser failed.");
        var connection = Assert.Single(overlay.ConnectionOverlays);
        Assert.Equal("connection-001", connection.ConnectionId);
        Assert.Equal("review", connection.PayloadType);
        Assert.Equal(1, connection.ItemCount);
        var log = Assert.Single(overlay.RecentLogs);
        Assert.Equal("target-001", log.NodeId);
        Assert.Equal("connection-001", log.ConnectionId);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "integration.runtime-overlay-provider" && descriptor.IsAvailable);
    }

    private static IGraphEditorSession CreateSession(IGraphRuntimeOverlayProvider? runtimeOverlayProvider)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Runtime",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Runtime",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Runtime Overlay",
                "Runtime overlay proof.",
                [
                    new GraphNode(
                        "source-001",
                        "Source",
                        "Tests",
                        "Runtime",
                        "Source node.",
                        new GraphPoint(120, 120),
                        new GraphSize(220, 120),
                        [],
                        [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        "target-001",
                        "Target",
                        "Tests",
                        "Runtime",
                        "Target node.",
                        new GraphPoint(420, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [],
                        "#8B7BFF",
                        TargetDefinitionId),
                ],
                [
                    new GraphConnection("connection-001", "source-001", "out", "target-001", "in", "Source To Target", "#6AD5C4"),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            RuntimeOverlayProvider = runtimeOverlayProvider,
        });
    }

    private sealed class TestRuntimeOverlayProvider(
        IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> nodeOverlays,
        IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> connectionOverlays,
        IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> recentLogs) : IGraphRuntimeOverlayProvider
    {
        public IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> GetNodeOverlays()
            => nodeOverlays;

        public IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> GetConnectionOverlays()
            => connectionOverlays;

        public IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> GetRecentLogs()
            => recentLogs;
    }
}
