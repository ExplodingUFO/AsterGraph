using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Viewport;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class RendererVirtualizationProofHarnessTests
{
    [Fact]
    public void Capture_EmitsMachineReadableProofMetadataWithoutWideningVirtualizationClaim()
    {
        var document = CreateDocument();
        var firstViewport = new GraphEditorViewportSnapshot(
            Zoom: 1d,
            PanX: 0d,
            PanY: 0d,
            ViewportWidth: 640d,
            ViewportHeight: 420d);
        var secondViewport = new GraphEditorViewportSnapshot(
            Zoom: 1d,
            PanX: -720d,
            PanY: -560d,
            ViewportWidth: 640d,
            ViewportHeight: 420d);

        var artifact = RendererVirtualizationProofHarness.Capture(
            document,
            firstViewport,
            secondViewport,
            overscanWorldUnits: 0d);

        Assert.Equal("514", artifact.Phase);
        Assert.Equal("viewport-budgeted-scene-projection", artifact.ClaimBoundary);
        Assert.False(artifact.AvoidsFullCollectionScan);
        Assert.False(artifact.AvoidsFullSceneRebuild);
        Assert.Equal(3, artifact.GraphSize.TotalNodes);
        Assert.Equal(2, artifact.GraphSize.TotalConnections);
        Assert.Equal(2, artifact.GraphSize.TotalGroups);
        Assert.Equal(640d, artifact.Viewport.Width);
        Assert.Equal(420d, artifact.Viewport.Height);
        Assert.Equal(1d, artifact.Zoom);
        Assert.Equal(0d, artifact.OverscanWorldUnits);
        Assert.Equal(2, artifact.VisibleVisualCounts.Nodes);
        Assert.Equal(1, artifact.VisibleVisualCounts.Connections);
        Assert.Equal(1, artifact.VisibleVisualCounts.Groups);
        Assert.Equal(3, artifact.InvalidationCounts.Nodes);
        Assert.Equal(2, artifact.InvalidationCounts.Connections);
        Assert.Equal(2, artifact.InvalidationCounts.Groups);
        Assert.Equal(7, artifact.InvalidationCounts.Total);
        Assert.True(artifact.MeasuredTimings.ProjectionElapsedTicks >= 0);
        Assert.True(artifact.MeasuredTimings.InvalidationElapsedTicks >= 0);

        var marker = artifact.ToMetadataMarker();

        Assert.StartsWith("RENDERER_VIRTUALIZATION_PROOF_ARTIFACT:phase=514", marker, StringComparison.Ordinal);
        Assert.Contains("graphSize=nodes:3,connections:2,groups:2", marker, StringComparison.Ordinal);
        Assert.Contains("viewport=width:640,height:420,panX:0,panY:0", marker, StringComparison.Ordinal);
        Assert.Contains("zoom=1", marker, StringComparison.Ordinal);
        Assert.Contains("overscan=0", marker, StringComparison.Ordinal);
        Assert.Contains("visibleVisualCounts=nodes:2/3,connections:1/2,groups:1/2", marker, StringComparison.Ordinal);
        Assert.Contains("invalidationCounts=nodes:3,connections:2,groups:2,total:7", marker, StringComparison.Ordinal);
        Assert.Contains("measuredTimings=projectionTicks:", marker, StringComparison.Ordinal);
        Assert.Contains("invalidationTicks:", marker, StringComparison.Ordinal);
        Assert.Contains("avoidsFullCollectionScan=false", marker, StringComparison.Ordinal);
        Assert.Contains("avoidsFullSceneRebuild=false", marker, StringComparison.Ordinal);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Renderer Proof",
            "Renderer virtualization proof harness document.",
            [
                CreateNode("node-a", 40d, 40d),
                CreateNode("node-b", 360d, 180d),
                CreateNode("node-c", 980d, 760d),
            ],
            [
                CreateConnection("connection-visible", "node-a", "node-b"),
                CreateConnection("connection-hidden", "node-c", "node-missing"),
            ],
            [
                new GraphNodeGroup("group-visible", "Visible", new GraphPoint(20d, 20d), new GraphSize(580d, 340d), ["node-a", "node-b"]),
                new GraphNodeGroup("group-hidden", "Hidden", new GraphPoint(900d, 700d), new GraphSize(180d, 140d), ["node-c"]),
            ]);

    private static GraphNode CreateNode(string id, double x, double y)
        => new(
            id,
            id,
            "Tests",
            "Projection",
            "Renderer proof node.",
            new GraphPoint(x, y),
            new GraphSize(180d, 120d),
            [],
            [],
            "#6AD5C4",
            new NodeDefinitionId($"tests.renderer-proof.{id}"));

    private static GraphConnection CreateConnection(string id, string sourceNodeId, string targetNodeId)
        => new(
            id,
            sourceNodeId,
            "out",
            targetNodeId,
            "in",
            id,
            "#6AD5C4");
}
