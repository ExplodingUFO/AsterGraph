using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.ConsumerSample;

public sealed class ConsumerSampleHost : IDisposable
{
    public static readonly NodeDefinitionId ReviewDefinitionId = new("consumer.sample.review");
    public static readonly NodeDefinitionId QueueDefinitionId = new("consumer.sample.queue");
    public static readonly NodeDefinitionId PluginAuditDefinitionId = new("consumer.sample.plugin.audit");

    private const string InitialReviewNodeId = "consumer-sample-review-001";
    private const string InitialQueueNodeId = "consumer-sample-queue-001";
    private const string InputPortId = "input";
    private const string ReviewPolicyPortId = "policy";
    private const string OutputPortId = "output";
    private const string ReviewAuditPortId = "audit";
    private const string PluginId = "consumer.sample.audit-plugin";
    public const string PluginCommandId = "consumer.sample.plugin.add-audit-node";
    public const string ScenarioId = "content-review-release-lane";
    public const string ScenarioTitle = "Content Review Release Lane";
    internal static readonly GraphSize ReviewNodeDefaultSize = new(320, 220);
    private readonly GraphEditorViewModel _editor;
    private readonly ConsumerSamplePluginAllowlistTrustPolicy _trustPolicy;
    private readonly GraphEditorPluginDiscoveryOptions _pluginDiscoveryOptions;
    private readonly string _storageRootPath;
    private int _nextHostNodeOrdinal = 2;
    private GraphLayoutPlan? _layoutPreview;
    private IReadOnlyList<string> _focusedSubgraphNodeIds = [];
    private IReadOnlyList<string> _dimmedNodeIds = [];
    private IReadOnlyList<string> _alignmentHelperLines = [];
    private int _lastRouteCleanupCount;

    private ConsumerSampleHost(
        GraphEditorViewModel editor,
        ConsumerSamplePluginAllowlistTrustPolicy trustPolicy,
        GraphEditorPluginDiscoveryOptions pluginDiscoveryOptions,
        string storageRootPath)
    {
        _editor = editor;
        _trustPolicy = trustPolicy;
        _pluginDiscoveryOptions = pluginDiscoveryOptions;
        _storageRootPath = storageRootPath;
        _editor.Session.Events.DocumentChanged += HandleStateChanged;
        _editor.Session.Events.SelectionChanged += HandleStateChanged;
        _editor.Session.Events.CommandExecuted += HandleStateChanged;
    }

    public event EventHandler? StateChanged;

    public GraphEditorViewModel Editor => _editor;

    public IGraphEditorSession Session => _editor.Session;

    public IReadOnlyList<GraphEditorPluginLoadSnapshot> PluginLoadSnapshots => Session.Queries.GetPluginLoadSnapshots();

    public IReadOnlyList<GraphEditorPluginCandidateSnapshot> PluginCandidates
        => AsterGraphEditorFactory.DiscoverPluginCandidates(_pluginDiscoveryOptions);

    public IReadOnlyList<ConsumerSamplePluginCandidateEntry> PluginCandidateEntries
        => PluginCandidates.Select(CreatePluginCandidateEntry).ToArray();

    public IReadOnlyList<ConsumerSamplePluginCandidateEntry> LocalPluginGalleryEntries
        => PluginCandidateEntries;

    public GraphEditorRuntimeOverlaySnapshot RuntimeOverlay
        => Session.Queries.GetRuntimeOverlaySnapshot();

    public GraphLayoutPlan? LayoutPreview => _layoutPreview;

    public IReadOnlyList<string> FocusedSubgraphNodeIds => _focusedSubgraphNodeIds;

    public IReadOnlyList<string> DimmedNodeIds => _dimmedNodeIds;

    public IReadOnlyList<string> AlignmentHelperLines => _alignmentHelperLines;

    public int LastRouteCleanupCount => _lastRouteCleanupCount;

    public IReadOnlyList<string> RuntimeLogExportLines
        => RuntimeOverlay.RecentLogs
            .Select(log => $"{log.TimestampUtc:O} [{log.Status}] {log.Message}")
            .ToArray();

    public IReadOnlyList<string> PluginAllowlistLines
    {
        get
        {
            var entries = _trustPolicy.Entries;
            var lines = new List<string>
            {
                "Allowlist decisions: " + entries.Count,
                "Allowlist export path: " + PluginAllowlistExchangePath,
            };

            if (entries.Count == 0)
            {
                lines.Add("No persisted allowlist decisions.");
            }
            else
            {
                lines.AddRange(entries.Select(entry =>
                    $"{entry.DisplayName} · {entry.PackageId ?? entry.PluginId}@{entry.PackageVersion ?? entry.Version ?? "?"} · fingerprint {FormatFingerprint(entry.TrustFingerprint)}"));
            }

            return lines;
        }
    }

    public string TrustBoundaryText =>
        "Plugins run in-process. This sample surfaces provenance, trusted/blocked reasons, and persisted allowlist import/export. It does not sandbox plugin code.";

    public IReadOnlyList<string> OnboardingCopyPathLines =>
    [
        "Starter.Avalonia: copy the document/catalog/editor/view composition.",
        "HelloWorld.Avalonia: confirm the smallest hosted shell.",
        "ConsumerSample.Avalonia: copy host-owned actions, selected-parameter editing, plugin trust, and proof markers.",
        "Demo: inspect the full showcase after the host seam is already clear.",
    ];

    public IReadOnlyList<string> RouteBoundaryLines =>
    [
        "Hosted UI route: use AsterGraphEditorFactory.Create(...) with AsterGraphAvaloniaViewFactory.Create(...) for the stock Avalonia shell.",
        "Runtime-only route: use AsterGraphEditorFactory.CreateSession(...) when the host owns the UI or native shell.",
        "Plugin route: discover candidates with AsterGraphEditorFactory.DiscoverPluginCandidates(...) and decide trust through PluginTrustPolicy before loading.",
        "Migration route: keep GraphEditorViewModel and GraphEditorView as retained migration surfaces only.",
    ];

    public string PluginAllowlistExchangePath
        => Path.Combine(_storageRootPath, "plugin-allowlist-export.json");

    public static ConsumerSampleHost Create(string? storageRootPath = null)
    {
        var resolvedStorageRoot = ResolveStorageRootPath(storageRootPath);
        var manifest = CreatePluginManifest();
        var provenance = CreatePluginProvenance();
        var allowlistStore = new ConsumerSamplePluginAllowlistStore(resolvedStorageRoot);
        var trustPolicy = new ConsumerSamplePluginAllowlistTrustPolicy(
            allowlistStore,
            [
                ConsumerSamplePluginAllowlistTrustPolicy.CreateEntry(manifest, provenance),
            ]);
        var discoveryOptions = CreatePluginDiscoveryOptions(manifest, provenance, trustPolicy);
        var options = new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(
                    new ConsumerAuditPlugin(),
                    manifest,
                    provenance),
            ],
            PluginTrustPolicy = trustPolicy,
            RuntimeOverlayProvider = new ConsumerSampleRuntimeOverlayProvider(),
            LayoutProvider = new ConsumerSampleLayoutProvider(),
        };

        var editor = AsterGraphEditorFactory.Create(options);
        var host = new ConsumerSampleHost(editor, trustPolicy, discoveryOptions, resolvedStorageRoot);
        host.SelectNode(InitialReviewNodeId);
        return host;
    }

    public bool AddHostReviewNode()
    {
        var before = Session.Queries.CreateDocumentSnapshot();
        _editor.Session.Commands.AddNode(
            ReviewDefinitionId,
            new GraphPoint(220 + ((_nextHostNodeOrdinal - 1) * 260), 300));
        _nextHostNodeOrdinal++;

        return Session.Queries.CreateDocumentSnapshot().Nodes.Count == before.Nodes.Count + 1;
    }

    public bool AddPluginAuditNode()
        => Session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(PluginCommandId));

    public bool ApproveSelection()
        => Session.Commands.TrySetSelectedNodeParameterValue("status", "approved");

    public bool TrySetSelectedOwner(string owner)
        => Session.Commands.TrySetSelectedNodeParameterValue("owner", owner);

    public void FitView()
        => Session.Commands.FitToViewport(updateStatus: false);

    public void SelectNode(string nodeId)
        => Session.Commands.SetSelection([nodeId], nodeId, updateStatus: false);

    public bool PreviewLayout()
    {
        var plan = Session.Queries.CreateLayoutPlan(new GraphLayoutRequest
        {
            Mode = GraphLayoutRequestMode.All,
            Orientation = GraphLayoutOrientation.LeftToRight,
            HorizontalSpacing = 320,
            VerticalSpacing = 160,
        });
        if (!plan.IsAvailable || plan.NodePositions.Count == 0)
        {
            return false;
        }

        _layoutPreview = plan;
        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool CancelLayoutPreview()
    {
        if (_layoutPreview is null)
        {
            return false;
        }

        _layoutPreview = null;
        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool ApplyLayoutPreview()
    {
        var preview = _layoutPreview;
        if (preview is null || !preview.IsAvailable || preview.NodePositions.Count == 0)
        {
            return false;
        }

        Session.Commands.SetNodePositions(
            preview.NodePositions.Select(node => new NodePositionSnapshot(node.NodeId, node.Position)).ToArray(),
            updateStatus: true);
        _layoutPreview = null;
        StateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool FocusSelectedSubgraph()
    {
        var document = Session.Queries.CreateDocumentSnapshot();
        var selection = Session.Queries.GetSelectionSnapshot();
        if (selection.SelectedNodeIds.Count == 0)
        {
            return false;
        }

        var focused = selection.SelectedNodeIds
            .Concat(document.Connections
                .Where(connection =>
                    selection.SelectedNodeIds.Contains(connection.SourceNodeId, StringComparer.Ordinal)
                    || selection.SelectedNodeIds.Contains(connection.TargetNodeId, StringComparer.Ordinal))
                .SelectMany(connection => new[] { connection.SourceNodeId, connection.TargetNodeId }))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        _focusedSubgraphNodeIds = focused;
        _dimmedNodeIds = document.Nodes
            .Select(node => node.Id)
            .Except(focused, StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        Session.Commands.CenterViewOnNode(selection.PrimarySelectedNodeId ?? focused[0]);
        StateChanged?.Invoke(this, EventArgs.Empty);
        return _focusedSubgraphNodeIds.Count > 0 && _dimmedNodeIds.Count > 0;
    }

    public bool CleanupSelectedConnectionRoutes()
    {
        var selectedConnectionIds = Session.Queries.GetSelectionSnapshot().SelectedConnectionIds;
        if (selectedConnectionIds.Count == 0)
        {
            return false;
        }

        var cleaned = 0;
        foreach (var connectionId in selectedConnectionIds)
        {
            var geometry = Session.Queries.GetConnectionGeometrySnapshots()
                .FirstOrDefault(candidate => string.Equals(candidate.ConnectionId, connectionId, StringComparison.Ordinal));
            if (geometry is null || geometry.Route.Vertices.Count == 0)
            {
                continue;
            }

            for (var index = geometry.Route.Vertices.Count - 1; index >= 0; index--)
            {
                if (Session.Commands.TryRemoveConnectionRouteVertex(connectionId, index, updateStatus: false))
                {
                    cleaned++;
                }
            }
        }

        _lastRouteCleanupCount = cleaned;
        StateChanged?.Invoke(this, EventArgs.Empty);
        return cleaned > 0;
    }

    public bool CaptureAlignmentHelperEvidence()
    {
        var descriptors = Session.Queries.GetCommandDescriptors();
        _alignmentHelperLines =
        [
            FormatLayoutCommand(descriptors, "layout.align-left"),
            FormatLayoutCommand(descriptors, "layout.align-center"),
            FormatLayoutCommand(descriptors, "layout.distribute-horizontal"),
        ];
        StateChanged?.Invoke(this, EventArgs.Empty);
        return _alignmentHelperLines.Count == 3
            && _alignmentHelperLines.Any(line => line.StartsWith("layout.align-left:enabled", StringComparison.Ordinal))
            && _alignmentHelperLines.Any(line => line.StartsWith("layout.align-center:enabled", StringComparison.Ordinal))
            && _alignmentHelperLines.Any(line => line.StartsWith("layout.distribute-horizontal:", StringComparison.Ordinal));
    }

    public bool TryNavigateToRuntimeLog(GraphEditorRuntimeLogEntrySnapshot log)
    {
        ArgumentNullException.ThrowIfNull(log);
        if (string.IsNullOrWhiteSpace(log.NodeId))
        {
            return false;
        }

        var nodeExists = Session.Queries.CreateDocumentSnapshot()
            .Nodes
            .Any(node => string.Equals(node.Id, log.NodeId, StringComparison.Ordinal));
        if (!nodeExists)
        {
            return false;
        }

        SelectNode(log.NodeId);
        return true;
    }

    public IReadOnlyList<GraphEditorNodeParameterSnapshot> GetSelectedParameterSnapshots()
        => Session.Queries.GetSelectedNodeParameterSnapshots();

    public bool HasPluginCommandContribution()
        => Session.Queries.GetCommandDescriptors().Any(descriptor => descriptor.Id == PluginCommandId);

    public bool HasPluginNodeDefinition()
        => Session.Queries.GetRegisteredNodeDefinitions().Any(definition => definition.Id == PluginAuditDefinitionId);

    public bool TrustPluginCandidate(string pluginId)
    {
        var candidate = FindPluginCandidate(pluginId);
        if (candidate is null)
        {
            return false;
        }

        var changed = _trustPolicy.Allow(candidate.Manifest, candidate.ProvenanceEvidence);
        StateChanged?.Invoke(this, EventArgs.Empty);
        return changed;
    }

    public bool BlockPluginCandidate(string pluginId)
    {
        var candidate = FindPluginCandidate(pluginId);
        if (candidate is null)
        {
            return false;
        }

        var changed = _trustPolicy.Block(candidate.Manifest, candidate.ProvenanceEvidence);
        if (changed)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        return changed;
    }

    public bool ExportPluginAllowlist(string? path = null)
        => _trustPolicy.Export(string.IsNullOrWhiteSpace(path) ? PluginAllowlistExchangePath : path);

    public bool ImportPluginAllowlist(string? path = null)
    {
        var changed = _trustPolicy.Import(string.IsNullOrWhiteSpace(path) ? PluginAllowlistExchangePath : path);
        if (changed)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        return changed;
    }

    public string GetFirstReviewNodeId()
        => Session.Queries.CreateDocumentSnapshot().Nodes
            .First(node => node.DefinitionId == ReviewDefinitionId)
            .Id;

    public void Dispose()
    {
        _editor.Session.Events.DocumentChanged -= HandleStateChanged;
        _editor.Session.Events.SelectionChanged -= HandleStateChanged;
        _editor.Session.Events.CommandExecuted -= HandleStateChanged;
    }

    private void HandleStateChanged(object? sender, EventArgs args)
        => StateChanged?.Invoke(this, EventArgs.Empty);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(CreateReviewDefinition());
        catalog.RegisterDefinition(CreateQueueDefinition());
        return catalog;
    }

    private static GraphDocument CreateDocument()
        => new(
            ScenarioTitle,
            "A realistic hosted-UI content-review scenario with host actions, custom nodes, and one trusted audit plugin.",
            [
                new GraphNode(
                    InitialReviewNodeId,
                    "Content Review",
                    "Consumer Sample",
                    "Review",
                    "Host-owned review node with shared parameter editing.",
                    new GraphPoint(140, 180),
                    ReviewNodeDefaultSize,
                    [
                        new GraphPort(InputPortId, "Input", PortDirection.Input, "flow", "#F3B36B", new PortTypeId("flow")),
                        new GraphPort(ReviewPolicyPortId, "Policy", PortDirection.Input, "policy", "#A5B4FC", new PortTypeId("policy")),
                    ],
                    [
                        new GraphPort(OutputPortId, "Output", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow")),
                        new GraphPort(ReviewAuditPortId, "Audit", PortDirection.Output, "audit", "#FF8A5B", new PortTypeId("audit")),
                    ],
                    "#6AD5C4",
                    ReviewDefinitionId,
                    ParameterValues:
                    [
                        new GraphParameterValue("status", new PortTypeId("enum"), "draft"),
                        new GraphParameterValue("owner", new PortTypeId("string"), "design-review"),
                        new GraphParameterValue("priority", new PortTypeId("int"), 2),
                    ]),
                new GraphNode(
                    InitialQueueNodeId,
                    "Ship Queue",
                    "Consumer Sample",
                    "Queue",
                    "Host-owned queue node that consumes approved review work.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 164),
                    [
                        new GraphPort(InputPortId, "Input", PortDirection.Input, "flow", "#F3B36B", new PortTypeId("flow")),
                    ],
                    [
                        new GraphPort(OutputPortId, "Output", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow")),
                    ],
                    "#8B7BFF",
                    QueueDefinitionId,
                    ParameterValues:
                    [
                        new GraphParameterValue("lane", new PortTypeId("string"), "alpha"),
                    ]),
            ],
            [
                new GraphConnection(
                    "consumer-sample-connection-001",
                    InitialReviewNodeId,
                    OutputPortId,
                    InitialQueueNodeId,
                    InputPortId,
                    "Review To Queue",
                    "#6AD5C4"),
            ]);

    private static INodeDefinition CreateReviewDefinition()
        => new NodeDefinition(
            ReviewDefinitionId,
            "Review Node",
            "Consumer Sample",
            "Review",
            [
                new PortDefinition(InputPortId, "Input", new PortTypeId("flow"), "#F3B36B"),
                new PortDefinition(ReviewPolicyPortId, "Policy", new PortTypeId("policy"), "#A5B4FC"),
            ],
            [
                new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#6AD5C4"),
                new PortDefinition(ReviewAuditPortId, "Audit", new PortTypeId("audit"), "#FF8A5B"),
            ],
            [
                new NodeParameterDefinition(
                    "status",
                    "Status",
                    new PortTypeId("enum"),
                    ParameterEditorKind.Enum,
                    description: "Approval state managed by the host menu action.",
                    defaultValue: "draft",
                    templateKey: ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey,
                    constraints: new ParameterConstraints(
                        AllowedOptions:
                        [
                            new ParameterOptionDefinition("draft", "Draft"),
                            new ParameterOptionDefinition("review", "In Review"),
                            new ParameterOptionDefinition("approved", "Approved"),
                        ])),
                new NodeParameterDefinition(
                    "owner",
                    "Owner",
                    new PortTypeId("string"),
                    ParameterEditorKind.Text,
                    description: "Host-assigned owner used by the custom parameter panel.",
                    defaultValue: "design-review",
                    templateKey: ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey),
                new NodeParameterDefinition(
                    "priority",
                    "Priority",
                    new PortTypeId("int"),
                    ParameterEditorKind.Number,
                    description: "Priority bucket for the sample review lane.",
                    defaultValue: 2,
                    constraints: new ParameterConstraints(Minimum: 1, Maximum: 5),
                    groupName: "Routing",
                    helpText: "Use the bounded priority range to order review work."),
                new NodeParameterDefinition(
                    "review-script",
                    "Review Script",
                    new PortTypeId("code"),
                    ParameterEditorKind.Text,
                    description: "Code-like checklist shown as multiline inspector text.",
                    defaultValue: "check.source()\ncheck.policy()\napprove.whenReady()",
                    templateKey: "code",
                    groupName: "Guidance",
                    placeholderText: "one check per line",
                    helpText: "Use a short host-owned script to explain review automation."),
            ],
            description: "Host-defined review node that demonstrates shared parameter editing from a custom host panel.",
            accentHex: "#6AD5C4",
            defaultWidth: ReviewNodeDefaultSize.Width,
            defaultHeight: ReviewNodeDefaultSize.Height);

    private static INodeDefinition CreateQueueDefinition()
        => new NodeDefinition(
            QueueDefinitionId,
            "Queue Node",
            "Consumer Sample",
            "Queue",
            [
                new PortDefinition(InputPortId, "Input", new PortTypeId("flow"), "#F3B36B"),
            ],
            [
                new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#8B7BFF"),
            ],
            [
                new NodeParameterDefinition(
                    "lane",
                    "Lane",
                    new PortTypeId("string"),
                    ParameterEditorKind.Text,
                    defaultValue: "alpha"),
            ],
            description: "Second host-defined node used to show a realistic queue hand-off.",
            accentHex: "#8B7BFF",
            defaultWidth: 240,
            defaultHeight: 164);

    private static GraphEditorPluginManifest CreatePluginManifest()
        => new(
            PluginId,
            "Consumer Audit Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                "consumer.sample.plugin",
                publisher: "AsterGraph Samples",
                packageId: "AsterGraph.ConsumerSample.Plugin",
                packageVersion: "0.2.0-alpha.3"),
            description: "Trusted sample plugin that adds one audit node plus executable command, presentation, and localization hooks.",
            version: "0.2.0-alpha.3",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.2.0-alpha.3",
                targetFramework: "net8.0",
                runtimeSurface: "Create + AsterGraphAvaloniaViewFactory"),
            capabilitySummary: "Adds one node definition, one executable command, one presentation badge, and one localization override.");

    private static GraphEditorPluginProvenanceEvidence CreatePluginProvenance()
        => new(
            new GraphEditorPluginPackageIdentity("AsterGraph.ConsumerSample.Plugin", "0.2.0-alpha.3"),
            new GraphEditorPluginSignatureEvidence(
                GraphEditorPluginSignatureStatus.Valid,
                GraphEditorPluginSignatureKind.Repository,
                new GraphEditorPluginSignerIdentity("AsterGraph Samples", "SAMPLE-TRUST-001"),
                reasonCode: "consumer.sample.signature.valid",
                reasonMessage: "Repository-signed sample plugin used for the consumer adoption sample."));

    private sealed class ConsumerAuditPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            PluginId,
            "Consumer Audit Plugin",
            "Trusted sample plugin for the medium adoption host.",
            "0.2.0-alpha.3");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new ConsumerAuditNodeDefinitionProvider());
            builder.AddCommandContributor(new ConsumerAuditCommandContributor());
            builder.AddNodePresentationProvider(new ConsumerAuditPresentationProvider());
            builder.AddLocalizationProvider(new ConsumerAuditLocalizationProvider());
        }
    }

    private sealed class ConsumerSampleRuntimeOverlayProvider : IGraphRuntimeOverlayProvider
    {
        private static readonly DateTimeOffset SampleTimestamp = new(2026, 4, 27, 8, 0, 0, TimeSpan.Zero);

        public IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> GetNodeOverlays()
            =>
            [
                new GraphEditorNodeRuntimeOverlaySnapshot(
                    InitialReviewNodeId,
                    GraphEditorRuntimeOverlayStatus.Success,
                    ElapsedMilliseconds: 42,
                    OutputPreview: "approved review payload",
                    LastRunAtUtc: SampleTimestamp),
                new GraphEditorNodeRuntimeOverlaySnapshot(
                    InitialQueueNodeId,
                    GraphEditorRuntimeOverlayStatus.Running,
                    ElapsedMilliseconds: 8,
                    OutputPreview: "queue handoff pending",
                    LastRunAtUtc: SampleTimestamp),
            ];

        public IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> GetConnectionOverlays()
            =>
            [
                new GraphEditorConnectionRuntimeOverlaySnapshot(
                    "consumer-sample-connection-001",
                    GraphEditorRuntimeOverlayStatus.Success,
                    ValuePreview: "{ status: approved, lane: alpha }",
                    PayloadType: "review",
                    ItemCount: 1),
            ];

        public IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> GetRecentLogs()
            =>
            [
                new GraphEditorRuntimeLogEntrySnapshot(
                    "consumer-sample-runtime-log-001",
                    SampleTimestamp,
                    GraphEditorRuntimeOverlayStatus.Success,
                    "Review payload reached the ship queue.",
                    ScopeId: "root",
                    NodeId: InitialQueueNodeId,
                    ConnectionId: "consumer-sample-connection-001"),
            ];
    }

    private sealed class ConsumerAuditNodeDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            =>
            [
                new NodeDefinition(
                    PluginAuditDefinitionId,
                    "Plugin Audit Node",
                    "Consumer Sample Plugins",
                    "Audit",
                    [
                        new PortDefinition(InputPortId, "Input", new PortTypeId("flow"), "#F3B36B"),
                    ],
                    [
                        new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#6AD5C4"),
                    ],
                    [
                        new NodeParameterDefinition(
                            "state",
                            "State",
                            new PortTypeId("enum"),
                            ParameterEditorKind.Enum,
                            defaultValue: "observing",
                            constraints: new ParameterConstraints(
                                AllowedOptions:
                                [
                                    new ParameterOptionDefinition("observing", "Observing"),
                                    new ParameterOptionDefinition("ready", "Ready"),
                                ])),
                    ],
                    description: "Trusted plugin node that records audit checkpoints inside the host sample.",
                    accentHex: "#FF8A5B",
                    defaultWidth: 250,
                    defaultHeight: 170),
            ];
    }

    private sealed class ConsumerAuditCommandContributor : IGraphEditorPluginCommandContributor
    {
        public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return
            [
                new GraphEditorCommandDescriptorSnapshot(
                    PluginCommandId,
                    "Add Plugin Audit Node",
                    "consumer",
                    iconKey: "plugin",
                    defaultShortcut: "Ctrl+Shift+A",
                    source: GraphEditorCommandSourceKind.Plugin,
                    isEnabled: true),
            ];
        }

        public bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var pluginNodeCount = context.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node => node.DefinitionId == PluginAuditDefinitionId);
            var beforeCount = context.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
            context.Session.Commands.AddNode(
                PluginAuditDefinitionId,
                new GraphPoint(240 + (pluginNodeCount * 260), 520));
            return context.Session.Queries.CreateDocumentSnapshot().Nodes.Count == beforeCount + 1;
        }
    }

    private sealed class ConsumerAuditPresentationProvider : IGraphEditorPluginNodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Node.DefinitionId == PluginAuditDefinitionId
                ? new NodePresentationState(
                    SubtitleOverride: "Plugin Audit",
                    TopRightBadges:
                    [
                        new NodeAdornmentDescriptor("Trusted", "#FF8A5B"),
                    ])
                : NodePresentationState.Empty;
        }
    }

    private sealed class ConsumerAuditLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.menu.canvas.addNode"
                ? "Plugin Add Node"
                : fallback;
    }

    private GraphEditorPluginCandidateSnapshot? FindPluginCandidate(string pluginId)
        => PluginCandidates.SingleOrDefault(candidate => string.Equals(candidate.Manifest.Id, pluginId, StringComparison.Ordinal));

    private ConsumerSamplePluginCandidateEntry CreatePluginCandidateEntry(GraphEditorPluginCandidateSnapshot candidate)
    {
        var manifest = candidate.Manifest;
        var provenance = candidate.ProvenanceEvidence;
        var fingerprint = ConsumerSamplePluginAllowlistTrustPolicy.BuildTrustFingerprint(manifest, provenance);
        var version = manifest.Version ?? "?";
        var targetFramework = manifest.Compatibility.TargetFramework ?? "?";
        var reason = candidate.TrustEvaluation.ReasonMessage ?? candidate.TrustEvaluation.ReasonCode ?? "No policy note.";
        var packageId = provenance.PackageIdentity?.Id ?? manifest.Provenance.PackageId ?? manifest.Id;
        var packageVersion = provenance.PackageIdentity?.Version ?? manifest.Provenance.PackageVersion ?? version;
        var signerFingerprint = provenance.Signature.Signer?.Fingerprint ?? provenance.Signature.Status.ToString();
        var isAllowed = candidate.TrustEvaluation.Decision == GraphEditorPluginTrustDecision.Allowed;
        var loadSnapshot = PluginLoadSnapshots.FirstOrDefault(snapshot => string.Equals(snapshot.Manifest.Id, manifest.Id, StringComparison.Ordinal));
        var loadState = loadSnapshot?.Status.ToString() ?? "Discovered";
        var manifestLine = $"manifest {manifest.Id} · {manifest.DisplayName} · {version} · tfm {targetFramework}";
        var galleryLine = $"{manifest.DisplayName} · trust {candidate.TrustEvaluation.Decision} · load {loadState} · fingerprint {FormatFingerprint(fingerprint)}";

        return new ConsumerSamplePluginCandidateEntry(
            manifest.Id,
            manifest.DisplayName,
            version,
            targetFramework,
            fingerprint,
            reason,
            isAllowed,
            !isAllowed,
            loadSnapshot is not null,
            loadState,
            manifestLine,
            galleryLine,
            $"{manifest.Id} · {version} · tfm {targetFramework}",
            $"package {packageId}@{packageVersion} · signer {signerFingerprint}",
            $"{candidate.TrustEvaluation.Decision} · fingerprint {FormatFingerprint(fingerprint)} · {reason}");
    }

    private static GraphEditorPluginDiscoveryOptions CreatePluginDiscoveryOptions(
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenance,
        ConsumerSamplePluginAllowlistTrustPolicy trustPolicy)
    {
        var assemblyPath = typeof(ConsumerAuditPlugin).Assembly.Location;
        return new GraphEditorPluginDiscoveryOptions
        {
            ManifestSources =
            [
                new ConsumerSampleManifestSource(
                [
                    new GraphEditorPluginManifestSourceCandidate(
                        "consumer.sample.plugin",
                        assemblyPath,
                        manifest,
                        typeof(ConsumerAuditPlugin).FullName,
                        provenance),
                ]),
            ],
            TrustPolicy = trustPolicy,
        };
    }

    private static string ResolveStorageRootPath(string? storageRootPath)
    {
        if (!string.IsNullOrWhiteSpace(storageRootPath))
        {
            Directory.CreateDirectory(storageRootPath);
            return storageRootPath;
        }

        var defaultRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraph.ConsumerSample");
        Directory.CreateDirectory(defaultRoot);
        return defaultRoot;
    }

    private static string FormatFingerprint(string fingerprint)
        => fingerprint.Length <= 12 ? fingerprint : fingerprint[..12] + "…";

    private static string FormatLayoutCommand(
        IReadOnlyList<GraphEditorCommandDescriptorSnapshot> descriptors,
        string commandId)
    {
        var descriptor = descriptors.FirstOrDefault(candidate => string.Equals(candidate.Id, commandId, StringComparison.Ordinal));
        return descriptor is null
            ? $"{commandId}:missing"
            : $"{commandId}:{(descriptor.IsEnabled ? "enabled" : "disabled")}";
    }

    private sealed class ConsumerSampleManifestSource(IReadOnlyList<GraphEditorPluginManifestSourceCandidate> candidates) : IGraphEditorPluginManifestSource
    {
        public IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates()
            => candidates;
    }

    private sealed class ConsumerSampleLayoutProvider : IGraphLayoutProvider
    {
        public GraphLayoutPlan CreateLayoutPlan(GraphLayoutRequest request)
            => new(
                true,
                request,
                [
                    new GraphLayoutNodePosition(InitialReviewNodeId, new GraphPoint(160, 160)),
                    new GraphLayoutNodePosition(InitialQueueNodeId, new GraphPoint(560, 160)),
                ],
                ResetManualRoutes: true);
    }
}
