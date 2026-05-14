namespace AsterGraph.Core.Models;

/// <summary>
/// Internal policy gate for future whiteboard primitive persistence.
/// </summary>
internal sealed record GraphWhiteboardPrimitivePersistenceDecision(
    GraphWhiteboardPrimitivePersistenceOwner Owner,
    GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy GraphDocumentSchemaPolicy,
    GraphWhiteboardPrimitivePersistenceRequirement RequirementsBeforeGraphDocumentSchemaChange)
{
    /// <summary>
    /// Current Phase 549 decision: whiteboard primitives are not part of the current graph document schema.
    /// </summary>
    public static GraphWhiteboardPrimitivePersistenceDecision Current { get; } = new(
        GraphWhiteboardPrimitivePersistenceOwner.SeparateAnnotationSurface,
        GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy.ExcludedFromCurrentGraphDocumentSchema,
        GraphWhiteboardPrimitivePersistenceRequirement.MigrationPolicy
            | GraphWhiteboardPrimitivePersistenceRequirement.GraphDocumentCompatibilityTests
            | GraphWhiteboardPrimitivePersistenceRequirement.WorkspacePersistenceBoundaryTests
            | GraphWhiteboardPrimitivePersistenceRequirement.ClipboardFragmentBoundaryTests
            | GraphWhiteboardPrimitivePersistenceRequirement.ScreenshotArtifactBoundaryTests);

    /// <summary>
    /// Whether the active decision stores whiteboard primitives inside GraphDocument payloads.
    /// </summary>
    public bool PersistsWhiteboardPrimitivesInGraphDocument
        => Owner == GraphWhiteboardPrimitivePersistenceOwner.GraphDocumentSchema
            && GraphDocumentSchemaPolicy == GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy.IncludedInGraphDocumentSchema;

    /// <summary>
    /// Whether this implementation slice persists whiteboard primitives at all.
    /// </summary>
    public bool PersistsWhiteboardPrimitivesInCurrentSlice => false;
}

internal enum GraphWhiteboardPrimitivePersistenceOwner
{
    GraphDocumentSchema,
    SeparateAnnotationSurface,
}

internal enum GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy
{
    ExcludedFromCurrentGraphDocumentSchema,
    IncludedInGraphDocumentSchema,
}

[Flags]
internal enum GraphWhiteboardPrimitivePersistenceRequirement
{
    None = 0,
    MigrationPolicy = 1 << 0,
    GraphDocumentCompatibilityTests = 1 << 1,
    WorkspacePersistenceBoundaryTests = 1 << 2,
    ClipboardFragmentBoundaryTests = 1 << 3,
    ScreenshotArtifactBoundaryTests = 1 << 4,
}
