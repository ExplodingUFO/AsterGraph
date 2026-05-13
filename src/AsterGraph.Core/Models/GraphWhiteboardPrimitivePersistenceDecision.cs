namespace AsterGraph.Core.Models;

/// <summary>
/// Internal policy gate for future whiteboard primitive persistence.
/// </summary>
internal sealed record GraphWhiteboardPrimitivePersistenceDecision(
    GraphWhiteboardPrimitivePersistenceOwner Owner,
    GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy GraphDocumentSchemaPolicy,
    GraphWhiteboardPrimitivePersistenceRequirement RequirementsBeforeGraphDocumentSchemaChange,
    GraphWhiteboardPrimitivePersistenceOutcome CurrentSliceOutcome,
    GraphWhiteboardPrimitivePersistenceBoundary CoveredBoundaries)
{
    /// <summary>
    /// Current Phase 556 decision: saved whiteboard state is deferred until a separate annotation store contract exists.
    /// </summary>
    public static GraphWhiteboardPrimitivePersistenceDecision Current { get; } = new(
        GraphWhiteboardPrimitivePersistenceOwner.SeparateAnnotationSurface,
        GraphWhiteboardPrimitiveGraphDocumentSchemaPolicy.ExcludedFromCurrentGraphDocumentSchema,
        GraphWhiteboardPrimitivePersistenceRequirement.MigrationPolicy
            | GraphWhiteboardPrimitivePersistenceRequirement.GraphDocumentCompatibilityTests
            | GraphWhiteboardPrimitivePersistenceRequirement.WorkspacePersistenceBoundaryTests
            | GraphWhiteboardPrimitivePersistenceRequirement.ClipboardFragmentBoundaryTests
            | GraphWhiteboardPrimitivePersistenceRequirement.ScreenshotArtifactBoundaryTests,
        GraphWhiteboardPrimitivePersistenceOutcome.DeferredUntilSeparateAnnotationStoreContract,
        GraphWhiteboardPrimitivePersistenceBoundary.GraphDocumentCompatibility
            | GraphWhiteboardPrimitivePersistenceBoundary.WorkspacePersistence
            | GraphWhiteboardPrimitivePersistenceBoundary.ClipboardFragment
            | GraphWhiteboardPrimitivePersistenceBoundary.ScreenshotArtifact
            | GraphWhiteboardPrimitivePersistenceBoundary.SceneExportArtifact);

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

internal enum GraphWhiteboardPrimitivePersistenceOutcome
{
    DeferredUntilSeparateAnnotationStoreContract,
    ImplementedInSeparateAnnotationStore,
    ImplementedInGraphDocumentSchema,
}

[Flags]
internal enum GraphWhiteboardPrimitivePersistenceBoundary
{
    None = 0,
    GraphDocumentCompatibility = 1 << 0,
    WorkspacePersistence = 1 << 1,
    ClipboardFragment = 1 << 2,
    ScreenshotArtifact = 1 << 3,
    SceneExportArtifact = 1 << 4,
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
