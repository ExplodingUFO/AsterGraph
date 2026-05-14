using System.Text.Json;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class WhiteboardPrimitivePersistenceDecisionContractsTests
{
    [Fact]
    public void WhiteboardPrimitivePersistenceDecision_RecordsSeparateAnnotationSurfaceOutsideGraphDocumentSchema()
    {
        var decision = GraphWhiteboardPrimitivePersistenceDecision.Current;

        Assert.False(typeof(GraphWhiteboardPrimitivePersistenceDecision).IsPublic);
        Assert.Equal(typeof(GraphWhiteboardPrimitive).Assembly, typeof(GraphWhiteboardPrimitivePersistenceDecision).Assembly);
        Assert.Equal(GraphWhiteboardPrimitivePersistenceOwner.SeparateAnnotationSurface, decision.Owner);
        Assert.Equal(
            GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy.ExcludedFromCurrentGraphDocumentSchema,
            decision.GraphDocumentSchemaPolicy);
        Assert.False(decision.PersistsWhiteboardPrimitivesInGraphDocument);
        Assert.False(decision.PersistsWhiteboardPrimitivesInCurrentSlice);
        AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement.MigrationPolicy);
        AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement.GraphDocumentCompatibilityTests);
        AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement.WorkspacePersistenceBoundaryTests);
        AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement.ClipboardFragmentBoundaryTests);
        AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement.ScreenshotArtifactBoundaryTests);

        void AssertRequiredBeforeSchemaChange(GraphWhiteboardPrimitivePersistenceRequirement requirement)
        {
            Assert.True(
                decision.RequirementsBeforeGraphDocumentSchemaChange.HasFlag(requirement),
                $"Expected Phase 549 persistence decision to require {requirement} before any GraphDocument schema change.");
        }
    }

    [Fact]
    public void GraphDocumentSerialization_RemainsWhiteboardFreeUnderPersistenceDecision()
    {
        var decision = GraphWhiteboardPrimitivePersistenceDecision.Current;
        var document = new GraphDocument(
            "Persistence Boundary",
            "Graph document remains graph-scoped.",
            [],
            [],
            []);

        var json = GraphDocumentSerializer.Serialize(document);

        Assert.Equal(GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy.ExcludedFromCurrentGraphDocumentSchema, decision.GraphDocumentSchemaPolicy);
        Assert.False(decision.PersistsWhiteboardPrimitivesInGraphDocument);
        Assert.Equal(6, GraphDocumentCompatibility.CurrentSchemaVersion);
        Assert.DoesNotContain("Whiteboard", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Primitive", json, StringComparison.OrdinalIgnoreCase);
        AssertNoWhiteboardProperty(typeof(GraphDocument));
        AssertNoWhiteboardProperty(typeof(GraphScope));
        AssertNoWhiteboardProperty(typeof(GraphDocumentSerializer.GraphDocumentFilePayload));

        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        Assert.Equal(6, root.GetProperty(nameof(GraphDocumentSerializer.GraphDocumentFilePayload.SchemaVersion)).GetInt32());
        Assert.True(root.TryGetProperty(nameof(GraphDocumentSerializer.GraphDocumentFilePayload.GraphScopes), out _));
        Assert.DoesNotContain(
            root.EnumerateObject(),
            property => property.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("Primitive", StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertNoWhiteboardProperty(Type type)
    {
        Assert.DoesNotContain(
            type.GetProperties(),
            property => property.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase)
                || property.Name.Contains("Primitive", StringComparison.OrdinalIgnoreCase)
                || property.PropertyType.Name.Contains("Whiteboard", StringComparison.OrdinalIgnoreCase)
                || property.PropertyType.Name.Contains("Primitive", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Phase556Decision_DefersSavedWhiteboardStateWhilePreservingBoundaryCoverage()
    {
        var decision = GraphWhiteboardPrimitivePersistenceDecision.Current;

        Assert.Equal(
            GraphWhiteboardPrimitivePersistenceOutcome.DeferredUntilSeparateAnnotationStoreContract,
            decision.CurrentSliceOutcome);
        Assert.False(decision.PersistsWhiteboardPrimitivesInCurrentSlice);
        Assert.False(decision.PersistsWhiteboardPrimitivesInGraphDocument);
        AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary.GraphDocumentCompatibility);
        AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary.WorkspacePersistence);
        AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary.ClipboardFragment);
        AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary.ScreenshotArtifact);
        AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary.SceneExportArtifact);

        void AssertBoundary(GraphWhiteboardPrimitivePersistenceBoundary boundary)
        {
            Assert.True(
                decision.CoveredBoundaries.HasFlag(boundary),
                $"Expected Phase 556 defer decision to cover {boundary}.");
        }
    }

    [Fact]
    public void WorkspaceClipboardAndSceneExportArtifacts_RemainWhiteboardFreeForDeferredDecision()
    {
        var decision = GraphWhiteboardPrimitivePersistenceDecision.Current;
        var document = new GraphDocument(
            "Phase 556 Boundary",
            "Saved graph state remains graph-scoped while annotation state is deferred.",
            [CreateNode("source-node"), CreateInputNode("target-node")],
            [new GraphConnection("connection-001", "source-node", "result", "target-node", "input", "float", "#6AD5C4")]);
        var workspaceDirectory = CreateTempDirectory();
        var workspacePath = Path.Combine(workspaceDirectory, "graph-workspace.json");
        var svgPath = Path.Combine(workspaceDirectory, "scene.svg");
        var workspace = new GraphWorkspaceService(workspacePath);
        var clipboard = new GraphClipboardPayloadSerializer();
        var fragment = new GraphSelectionFragment(
            document.Nodes,
            document.Connections,
            new GraphPoint(120, 160),
            "source-node");
        var scene = new GraphEditorSceneSnapshot(
            document,
            new GraphEditorSelectionSnapshot(["source-node"], "source-node"),
            new GraphEditorViewportSnapshot(1d, 0d, 0d, 960d, 640d),
            [],
            [],
            [],
            GraphEditorPendingConnectionSnapshot.Create(false, null, null));

        try
        {
            workspace.Save(document);
            new GraphSceneSvgExportService(svgPath).Export(scene);
            var workspaceJson = File.ReadAllText(workspacePath);
            var clipboardJson = clipboard.Serialize(fragment);
            var svg = File.ReadAllText(svgPath);

            Assert.Equal(GraphWhiteboardPrimitivePersistenceOutcome.DeferredUntilSeparateAnnotationStoreContract, decision.CurrentSliceOutcome);
            AssertBoundaryPayload("workspace", workspaceJson);
            AssertBoundaryPayload("clipboard", clipboardJson);
            AssertBoundaryPayload("scene-svg", svg);
        }
        finally
        {
            if (Directory.Exists(workspaceDirectory))
            {
                Directory.Delete(workspaceDirectory, recursive: true);
            }
        }

        static void AssertBoundaryPayload(string name, string payload)
        {
            Assert.DoesNotContain("Whiteboard", payload, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Primitive", payload, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(name == "scene-svg" ? "<svg" : "\"", payload, StringComparison.Ordinal);
        }
    }

    private static GraphNode CreateNode(string id)
        => new(
            id,
            "Persistence Source",
            "Tests",
            "Produces a float output",
            "Used by Phase 556 persistence boundary tests.",
            new GraphPoint(120, 160),
            new GraphSize(240, 160),
            [],
            [
                new GraphPort(
                    "result",
                    "Result",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4"),
            ],
            "#6AD5C4");

    private static GraphNode CreateInputNode(string id)
        => new(
            id,
            "Persistence Target",
            "Tests",
            "Consumes a float input",
            "Used by Phase 556 persistence boundary tests.",
            new GraphPoint(420, 160),
            new GraphSize(240, 160),
            [
                new GraphPort(
                    "input",
                    "Input",
                    PortDirection.Input,
                    "float",
                    "#F3B36B"),
            ],
            [],
            "#F3B36B");

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "AsterGraph.Editor.Tests",
            nameof(WhiteboardPrimitivePersistenceDecisionContractsTests),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
