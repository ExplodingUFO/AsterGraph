using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class WhiteboardAnnotationStoreContractTests
{
    [Fact]
    public void AnnotationStoreContract_CapturesInternalStoreMetadataPrimitivePayloadAndNeutralBoundary()
    {
        var contract = GraphWhiteboardAnnotationStoreContract.Current;
        var metadata = new GraphWhiteboardAnnotationStoreMetadata(
            "whiteboard-annotation-store-001",
            contract.Owner,
            contract.Lifetime,
            GraphWhiteboardAnnotationMigrationMetadata.Current);
        var geometry = new GraphWhiteboardPrimitiveGeometry(
            new GraphPoint(12d, 18d),
            new GraphSize(120d, 64d),
            [
                new GraphPoint(12d, 18d),
                new GraphPoint(24d, 32d),
            ]);
        var payload = new GraphWhiteboardAnnotationPrimitivePayload(
            GraphWhiteboardPrimitiveKind.Freehand,
            geometry,
            new GraphWhiteboardPrimitiveStyle(
                FillHex: "#6AD5C4",
                StrokeHex: "#1A1F2E",
                StrokeThickness: 2.5d,
                Opacity: 0.72d),
            ZIndex: 12,
            new GraphWhiteboardPrimitiveEditLifecycle(
                GraphWhiteboardPrimitiveEditState.Committed,
                ActiveHandleKey: "freehand-finalized"));
        var record = new GraphWhiteboardAnnotationRecord(
            new GraphWhiteboardAnnotationIdentity("annotation-001"),
            new GraphWhiteboardPrimitiveReference("whiteboard-primitive-001", GraphWhiteboardPrimitiveKind.Freehand),
            payload);
        var snapshot = new GraphWhiteboardAnnotationStoreSnapshot(metadata, [record]);
        IGraphWhiteboardAnnotationStoreBoundary boundary = new RecordingAnnotationStoreBoundary();

        boundary.WriteSnapshot(snapshot);
        var read = boundary.ReadSnapshot();

        Assert.Equal(GraphWhiteboardAnnotationStoreOwner.SeparateAnnotationSurface, contract.Owner);
        Assert.Equal(GraphWhiteboardAnnotationStoreLifetime.WorkspaceScoped, contract.Lifetime);
        Assert.False(contract.PersistsInGraphDocument);
        Assert.False(contract.PersistsInCurrentSlice);
        Assert.Equal(1, contract.CurrentStoreSchemaVersion);
        AssertBoundary(contract.CompatibilityBoundaries, GraphWhiteboardAnnotationStoreCompatibilityBoundary.GraphDocumentCompatibility);
        AssertBoundary(contract.CompatibilityBoundaries, GraphWhiteboardAnnotationStoreCompatibilityBoundary.WorkspacePersistence);
        AssertBoundary(contract.CompatibilityBoundaries, GraphWhiteboardAnnotationStoreCompatibilityBoundary.ClipboardFragment);
        AssertBoundary(contract.CompatibilityBoundaries, GraphWhiteboardAnnotationStoreCompatibilityBoundary.SceneExportArtifact);
        AssertBoundary(contract.CompatibilityBoundaries, GraphWhiteboardAnnotationStoreCompatibilityBoundary.ScreenshotArtifact);
        Assert.Equal("whiteboard-annotation-store-001", read.Metadata.StoreId);
        Assert.Equal(GraphWhiteboardAnnotationMigrationMetadata.Current, read.Metadata.Migration);
        var readRecord = Assert.Single(read.Records);
        Assert.Equal("annotation-001", readRecord.Identity.AnnotationId);
        Assert.Equal("whiteboard-primitive-001", readRecord.PrimitiveReference.PrimitiveId);
        Assert.Equal(GraphWhiteboardPrimitiveKind.Freehand, readRecord.Payload.Kind);
        Assert.Equal(new GraphPoint(12d, 18d), readRecord.Payload.Geometry.Origin);
        Assert.Equal(new GraphSize(120d, 64d), readRecord.Payload.Geometry.Size);
        Assert.Equal([new GraphPoint(12d, 18d), new GraphPoint(24d, 32d)], readRecord.Payload.Geometry.Points);
        Assert.Equal("#6AD5C4", readRecord.Payload.Style.FillHex);
        Assert.Equal("#1A1F2E", readRecord.Payload.Style.StrokeHex);
        Assert.Equal(2.5d, readRecord.Payload.Style.StrokeThickness);
        Assert.Equal(0.72d, readRecord.Payload.Style.Opacity);
        Assert.Equal(12, readRecord.Payload.ZIndex);
        Assert.Equal(GraphWhiteboardPrimitiveEditState.Committed, readRecord.Payload.EditLifecycle.State);
        Assert.Equal("freehand-finalized", readRecord.Payload.EditLifecycle.ActiveHandleKey);

        static void AssertBoundary(
            GraphWhiteboardAnnotationStoreCompatibilityBoundary boundaries,
            GraphWhiteboardAnnotationStoreCompatibilityBoundary expected)
        {
            Assert.True(boundaries.HasFlag(expected), $"Expected Phase 558 annotation store contract to cover {expected}.");
        }
    }

    [Fact]
    public void AnnotationRecord_RejectsPrimitiveReferencePayloadKindMismatch()
    {
        var payload = new GraphWhiteboardAnnotationPrimitivePayload(
            GraphWhiteboardPrimitiveKind.Freehand,
            new GraphWhiteboardPrimitiveGeometry(
                new GraphPoint(12d, 18d),
                new GraphSize(120d, 64d),
                [
                    new GraphPoint(12d, 18d),
                    new GraphPoint(24d, 32d),
                ]),
            new GraphWhiteboardPrimitiveStyle(
                FillHex: "#6AD5C4",
                StrokeHex: "#1A1F2E",
                StrokeThickness: 2.5d,
                Opacity: 0.72d),
            ZIndex: 12,
            new GraphWhiteboardPrimitiveEditLifecycle(
                GraphWhiteboardPrimitiveEditState.Committed,
                ActiveHandleKey: "freehand-finalized"));

        var exception = Assert.Throws<ArgumentException>(() =>
            new GraphWhiteboardAnnotationRecord(
                new GraphWhiteboardAnnotationIdentity("annotation-001"),
                new GraphWhiteboardPrimitiveReference("whiteboard-primitive-001", GraphWhiteboardPrimitiveKind.Rectangle),
                payload));

        Assert.Equal("PrimitiveReference", exception.ParamName);
        Assert.Contains("must match payload kind", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnnotationStoreContract_StaysInternalAndDoesNotChangeGraphPersistenceRendererPointerOrArtifacts()
    {
        Assert.False(typeof(GraphWhiteboardAnnotationStoreContract).IsPublic);
        Assert.False(typeof(GraphWhiteboardAnnotationStoreMetadata).IsPublic);
        Assert.False(typeof(GraphWhiteboardAnnotationIdentity).IsPublic);
        Assert.False(typeof(GraphWhiteboardPrimitiveReference).IsPublic);
        Assert.False(typeof(GraphWhiteboardAnnotationPrimitivePayload).IsPublic);
        Assert.False(typeof(GraphWhiteboardAnnotationRecord).IsPublic);
        Assert.False(typeof(GraphWhiteboardAnnotationStoreSnapshot).IsPublic);
        Assert.False(typeof(IGraphWhiteboardAnnotationStoreBoundary).IsPublic);
        Assert.Equal(typeof(GraphWhiteboardPrimitive).Assembly, typeof(GraphWhiteboardAnnotationStoreContract).Assembly);
        Assert.DoesNotContain(
            typeof(GraphWhiteboardAnnotationStoreContract).Assembly.GetReferencedAssemblies(),
            assemblyName => assemblyName.Name is "AsterGraph.Editor" or "AsterGraph.Avalonia");
        Assert.DoesNotContain(
            typeof(GraphWhiteboardAnnotationStoreContract).Assembly.GetExportedTypes(),
            type => type.Name.Contains("WhiteboardAnnotation", StringComparison.Ordinal)
                || type.Name.Contains("AnnotationStore", StringComparison.Ordinal));

        var document = new GraphDocument(
            "Phase 558 Boundary",
            "Graph document remains graph-scoped while annotation store contracts stay internal.",
            [CreateNode("source-node")],
            [],
            []);
        var json = GraphDocumentSerializer.Serialize(document);
        var scene = new GraphEditorSceneSnapshot(
            document,
            new GraphEditorSelectionSnapshot(["source-node"], "source-node"),
            new GraphEditorViewportSnapshot(1d, 0d, 0d, 960d, 640d),
            [],
            [],
            [],
            GraphEditorPendingConnectionSnapshot.Create(false, null, null));
        var svg = GraphSceneSvgDocumentBuilder.Build(scene);
        var pointerCoordinator = Type.GetType(
            "AsterGraph.Avalonia.Controls.Internal.NodeCanvasPointerInteractionCoordinator, AsterGraph.Avalonia",
            throwOnError: true)!;

        Assert.Equal(6, GraphDocumentCompatibility.CurrentSchemaVersion);
        AssertBoundaryPayload("graph-document-json", json);
        AssertBoundaryPayload("scene-svg", svg);
        AssertNoAnnotationStoreMember(typeof(GraphDocument));
        AssertNoAnnotationStoreMember(typeof(GraphDocumentSerializer.GraphDocumentFilePayload));
        AssertNoAnnotationStoreMember(typeof(GraphWhiteboardPrimitiveRendererAdapter));
        AssertNoAnnotationStoreMember(typeof(GraphEditorSceneSnapshot));
        AssertNoAnnotationStoreMember(typeof(GraphSceneSvgDocumentBuilder));
        AssertNoAnnotationStoreMember(typeof(GraphSceneImageExportService));
        AssertNoAnnotationStoreMember(pointerCoordinator);
    }

    private static GraphNode CreateNode(string id)
        => new(
            id,
            "Annotation Store Source",
            "Tests",
            "Produces annotation-store boundary evidence",
            "Used by Phase 558 annotation-store contract tests.",
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

    private static void AssertBoundaryPayload(string name, string payload)
    {
        Assert.DoesNotContain("AnnotationStore", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("WhiteboardAnnotation", payload, StringComparison.Ordinal);
        Assert.Contains(name == "scene-svg" ? "<svg" : "\"", payload, StringComparison.Ordinal);
    }

    private static void AssertNoAnnotationStoreMember(Type type)
    {
        Assert.DoesNotContain(
            type.GetMembers(),
            member => member.Name.Contains("AnnotationStore", StringComparison.Ordinal)
                || member.Name.Contains("WhiteboardAnnotation", StringComparison.Ordinal));
    }

    private sealed class RecordingAnnotationStoreBoundary : IGraphWhiteboardAnnotationStoreBoundary
    {
        private GraphWhiteboardAnnotationStoreSnapshot? _snapshot;

        public GraphWhiteboardAnnotationStoreSnapshot ReadSnapshot()
            => _snapshot ?? throw new InvalidOperationException("No annotation store snapshot has been written.");

        public void WriteSnapshot(GraphWhiteboardAnnotationStoreSnapshot snapshot)
            => _snapshot = snapshot;
    }
}
