namespace AsterGraph.Core.Models;

/// <summary>
/// Internal contract gate for a future separate whiteboard annotation store.
/// </summary>
internal sealed record GraphWhiteboardAnnotationStoreContract(
    GraphWhiteboardAnnotationStoreOwner Owner,
    GraphWhiteboardAnnotationStoreLifetime Lifetime,
    int CurrentStoreSchemaVersion,
    GraphWhiteboardAnnotationStoreCompatibilityBoundary CompatibilityBoundaries)
{
    /// <summary>
    /// Current Phase 558 contract: define the internal store shape without persisting annotation state.
    /// </summary>
    public static GraphWhiteboardAnnotationStoreContract Current { get; } = new(
        GraphWhiteboardAnnotationStoreOwner.SeparateAnnotationSurface,
        GraphWhiteboardAnnotationStoreLifetime.WorkspaceScoped,
        GraphWhiteboardAnnotationMigrationMetadata.Current.StoreSchemaVersion,
        GraphWhiteboardAnnotationStoreCompatibilityBoundary.GraphDocumentCompatibility
            | GraphWhiteboardAnnotationStoreCompatibilityBoundary.WorkspacePersistence
            | GraphWhiteboardAnnotationStoreCompatibilityBoundary.ClipboardFragment
            | GraphWhiteboardAnnotationStoreCompatibilityBoundary.SceneExportArtifact
            | GraphWhiteboardAnnotationStoreCompatibilityBoundary.ScreenshotArtifact);

    /// <summary>
    /// Whether the contract places annotation data inside GraphDocument payloads.
    /// </summary>
    public bool PersistsInGraphDocument => false;

    /// <summary>
    /// Whether this implementation slice persists annotation data anywhere.
    /// </summary>
    public bool PersistsInCurrentSlice => false;
}

internal sealed record GraphWhiteboardAnnotationStoreMetadata
{
    private string _storeId = string.Empty;

    public GraphWhiteboardAnnotationStoreMetadata(
        string StoreId,
        GraphWhiteboardAnnotationStoreOwner Owner,
        GraphWhiteboardAnnotationStoreLifetime Lifetime,
        GraphWhiteboardAnnotationMigrationMetadata Migration)
    {
        this.StoreId = StoreId;
        this.Owner = Owner;
        this.Lifetime = Lifetime;
        this.Migration = Migration ?? throw new ArgumentNullException(nameof(Migration));
    }

    public string StoreId
    {
        get => _storeId;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _storeId = value;
        }
    }

    public GraphWhiteboardAnnotationStoreOwner Owner { get; init; }

    public GraphWhiteboardAnnotationStoreLifetime Lifetime { get; init; }

    public GraphWhiteboardAnnotationMigrationMetadata Migration { get; init; }
}

internal sealed record GraphWhiteboardAnnotationMigrationMetadata
{
    private string _migrationPolicyKey = string.Empty;
    private int _storeSchemaVersion;

    public GraphWhiteboardAnnotationMigrationMetadata(
        int StoreSchemaVersion,
        string MigrationPolicyKey)
    {
        this.StoreSchemaVersion = StoreSchemaVersion;
        this.MigrationPolicyKey = MigrationPolicyKey;
    }

    public static GraphWhiteboardAnnotationMigrationMetadata Current { get; } = new(
        StoreSchemaVersion: 1,
        MigrationPolicyKey: "phase-558-no-persisted-store");

    public int StoreSchemaVersion
    {
        get => _storeSchemaVersion;
        init
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Store schema version must be positive.");
            }

            _storeSchemaVersion = value;
        }
    }

    public string MigrationPolicyKey
    {
        get => _migrationPolicyKey;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _migrationPolicyKey = value;
        }
    }
}

internal sealed record GraphWhiteboardAnnotationIdentity
{
    private string _annotationId = string.Empty;

    public GraphWhiteboardAnnotationIdentity(string AnnotationId)
    {
        this.AnnotationId = AnnotationId;
    }

    public string AnnotationId
    {
        get => _annotationId;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _annotationId = value;
        }
    }
}

internal sealed record GraphWhiteboardPrimitiveReference
{
    private string _primitiveId = string.Empty;

    public GraphWhiteboardPrimitiveReference(
        string PrimitiveId,
        GraphWhiteboardPrimitiveKind Kind)
    {
        this.PrimitiveId = PrimitiveId;
        this.Kind = Kind;
    }

    public string PrimitiveId
    {
        get => _primitiveId;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _primitiveId = value;
        }
    }

    public GraphWhiteboardPrimitiveKind Kind { get; init; }
}

internal sealed record GraphWhiteboardAnnotationPrimitivePayload
{
    public GraphWhiteboardAnnotationPrimitivePayload(
        GraphWhiteboardPrimitiveKind Kind,
        GraphWhiteboardPrimitiveGeometry Geometry,
        GraphWhiteboardPrimitiveStyle Style,
        int ZIndex,
        GraphWhiteboardPrimitiveEditLifecycle EditLifecycle)
    {
        this.Kind = Kind;
        this.Geometry = Geometry ?? throw new ArgumentNullException(nameof(Geometry));
        this.Style = Style ?? throw new ArgumentNullException(nameof(Style));
        this.ZIndex = ZIndex;
        this.EditLifecycle = EditLifecycle ?? throw new ArgumentNullException(nameof(EditLifecycle));
    }

    public GraphWhiteboardPrimitiveKind Kind { get; init; }

    public GraphWhiteboardPrimitiveGeometry Geometry { get; init; }

    public GraphWhiteboardPrimitiveStyle Style { get; init; }

    public int ZIndex { get; init; }

    public GraphWhiteboardPrimitiveEditLifecycle EditLifecycle { get; init; }
}

internal sealed record GraphWhiteboardAnnotationRecord
{
    public GraphWhiteboardAnnotationRecord(
        GraphWhiteboardAnnotationIdentity Identity,
        GraphWhiteboardPrimitiveReference PrimitiveReference,
        GraphWhiteboardAnnotationPrimitivePayload Payload)
    {
        this.Identity = Identity ?? throw new ArgumentNullException(nameof(Identity));
        this.PrimitiveReference = PrimitiveReference ?? throw new ArgumentNullException(nameof(PrimitiveReference));
        this.Payload = Payload ?? throw new ArgumentNullException(nameof(Payload));
        if (this.PrimitiveReference.Kind != this.Payload.Kind)
        {
            throw new ArgumentException(
                "Primitive reference kind must match payload kind.",
                nameof(PrimitiveReference));
        }
    }

    public GraphWhiteboardAnnotationIdentity Identity { get; init; }

    public GraphWhiteboardPrimitiveReference PrimitiveReference { get; init; }

    public GraphWhiteboardAnnotationPrimitivePayload Payload { get; init; }
}

internal sealed record GraphWhiteboardAnnotationStoreSnapshot
{
    private IReadOnlyList<GraphWhiteboardAnnotationRecord> _records = [];

    public GraphWhiteboardAnnotationStoreSnapshot(
        GraphWhiteboardAnnotationStoreMetadata Metadata,
        IReadOnlyList<GraphWhiteboardAnnotationRecord>? Records = null)
    {
        this.Metadata = Metadata ?? throw new ArgumentNullException(nameof(Metadata));
        this.Records = Records ?? [];
    }

    public GraphWhiteboardAnnotationStoreMetadata Metadata { get; init; }

    public IReadOnlyList<GraphWhiteboardAnnotationRecord> Records
    {
        get => _records;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _records = value.ToList();
        }
    }
}

internal interface IGraphWhiteboardAnnotationStoreBoundary
{
    GraphWhiteboardAnnotationStoreSnapshot ReadSnapshot();

    void WriteSnapshot(GraphWhiteboardAnnotationStoreSnapshot snapshot);
}

internal sealed class InMemoryGraphWhiteboardAnnotationStoreBoundary : IGraphWhiteboardAnnotationStoreBoundary
{
    private const string DefaultStoreId = "whiteboard-annotation-store-session";
    private GraphWhiteboardAnnotationStoreSnapshot _snapshot = CreateEmptySnapshot();

    public GraphWhiteboardAnnotationStoreSnapshot ReadSnapshot()
        => CloneSnapshot(_snapshot);

    public void WriteSnapshot(GraphWhiteboardAnnotationStoreSnapshot snapshot)
        => _snapshot = CloneSnapshot(snapshot ?? throw new ArgumentNullException(nameof(snapshot)));

    private static GraphWhiteboardAnnotationStoreSnapshot CreateEmptySnapshot()
        => new(
            new GraphWhiteboardAnnotationStoreMetadata(
                DefaultStoreId,
                GraphWhiteboardAnnotationStoreContract.Current.Owner,
                GraphWhiteboardAnnotationStoreContract.Current.Lifetime,
                GraphWhiteboardAnnotationMigrationMetadata.Current));

    private static GraphWhiteboardAnnotationStoreSnapshot CloneSnapshot(GraphWhiteboardAnnotationStoreSnapshot snapshot)
        => new(snapshot.Metadata, snapshot.Records);
}

internal enum GraphWhiteboardAnnotationStoreOwner
{
    SeparateAnnotationSurface,
}

internal enum GraphWhiteboardAnnotationStoreLifetime
{
    WorkspaceScoped,
}

[Flags]
internal enum GraphWhiteboardAnnotationStoreCompatibilityBoundary
{
    None = 0,
    GraphDocumentCompatibility = 1 << 0,
    WorkspacePersistence = 1 << 1,
    ClipboardFragment = 1 << 2,
    SceneExportArtifact = 1 << 3,
    ScreenshotArtifact = 1 << 4,
}
