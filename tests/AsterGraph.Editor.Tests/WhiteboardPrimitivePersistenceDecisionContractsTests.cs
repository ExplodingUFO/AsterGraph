using System.Text.Json;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
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
}
