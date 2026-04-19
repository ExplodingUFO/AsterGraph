using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
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
    private const string OutputPortId = "output";
    private const string PluginId = "consumer.sample.audit-plugin";
    private const string PluginMenuItemId = "consumer-sample.plugin.menu";
    private readonly GraphEditorViewModel _editor;
    private readonly ConsumerSamplePluginAllowlistTrustPolicy _trustPolicy;
    private readonly GraphEditorPluginDiscoveryOptions _pluginDiscoveryOptions;
    private readonly string _storageRootPath;
    private int _nextHostNodeOrdinal = 2;
    private int _nextPluginNodeOrdinal = 1;

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
    {
        var before = Session.Queries.CreateDocumentSnapshot();
        _editor.Session.Commands.AddNode(
            PluginAuditDefinitionId,
            new GraphPoint(240 + ((_nextPluginNodeOrdinal - 1) * 260), 520));
        _nextPluginNodeOrdinal++;

        return Session.Queries.CreateDocumentSnapshot().Nodes.Count == before.Nodes.Count + 1;
    }

    public bool ApproveSelection()
        => Session.Commands.TrySetSelectedNodeParameterValue("status", "approved");

    public bool TrySetSelectedOwner(string owner)
        => Session.Commands.TrySetSelectedNodeParameterValue("owner", owner);

    public void FitView()
        => Session.Commands.FitToViewport(updateStatus: false);

    public void SelectNode(string nodeId)
        => Session.Commands.SetSelection([nodeId], nodeId, updateStatus: false);

    public IReadOnlyList<GraphEditorNodeParameterSnapshot> GetSelectedParameterSnapshots()
        => Session.Queries.GetSelectedNodeParameterSnapshots();

    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildCanvasContextMenu()
        => Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(
            ContextMenuTargetKind.Canvas,
            new GraphPoint(320, 220)));

    public bool HasPluginMenuContribution()
        => BuildCanvasContextMenu().Any(item => item.Id == PluginMenuItemId);

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
            "Consumer Sample Graph",
            "A realistic hosted-UI sample with host actions, custom nodes, and one trusted plugin.",
            [
                new GraphNode(
                    InitialReviewNodeId,
                    "Content Review",
                    "Consumer Sample",
                    "Review",
                    "Host-owned review node with shared parameter editing.",
                    new GraphPoint(140, 180),
                    new GraphSize(260, 188),
                    [
                        new GraphPort(InputPortId, "Input", PortDirection.Input, "flow", "#F3B36B", new PortTypeId("flow")),
                    ],
                    [
                        new GraphPort(OutputPortId, "Output", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow")),
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
            ],
            [
                new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#6AD5C4"),
            ],
            [
                new NodeParameterDefinition(
                    "status",
                    "Status",
                    new PortTypeId("enum"),
                    ParameterEditorKind.Enum,
                    description: "Approval state managed by the host menu action.",
                    defaultValue: "draft",
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
                    defaultValue: "design-review"),
                new NodeParameterDefinition(
                    "priority",
                    "Priority",
                    new PortTypeId("int"),
                    ParameterEditorKind.Number,
                    description: "Priority bucket for the sample review lane.",
                    defaultValue: 2,
                    constraints: new ParameterConstraints(Minimum: 1, Maximum: 5)),
            ],
            description: "Host-defined review node that demonstrates shared parameter editing from a custom host panel.",
            accentHex: "#6AD5C4",
            defaultWidth: 260,
            defaultHeight: 188);

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
            description: "Trusted sample plugin that adds one audit node plus menu and presentation hooks.",
            version: "0.2.0-alpha.3",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.2.0-alpha.3",
                targetFramework: "net8.0",
                runtimeSurface: "Create + AsterGraphAvaloniaViewFactory"),
            capabilitySummary: "Adds one node definition, one menu augmentation, one presentation badge, and one localization override.");

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
            builder.AddContextMenuAugmentor(new ConsumerAuditContextMenuAugmentor());
            builder.AddNodePresentationProvider(new ConsumerAuditPresentationProvider());
            builder.AddLocalizationProvider(new ConsumerAuditLocalizationProvider());
        }
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

    private sealed class ConsumerAuditContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return
            [
                .. context.StockItems,
                new GraphEditorMenuItemDescriptorSnapshot(
                    PluginMenuItemId,
                    "Plugin Audit Contribution",
                    iconKey: "plugin",
                    isEnabled: false,
                    disabledReason: "This sample keeps the plugin action read-only to show menu augmentation without adding custom command plumbing."),
            ];
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

    private static ConsumerSamplePluginCandidateEntry CreatePluginCandidateEntry(GraphEditorPluginCandidateSnapshot candidate)
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

        return new ConsumerSamplePluginCandidateEntry(
            manifest.Id,
            manifest.DisplayName,
            version,
            targetFramework,
            fingerprint,
            reason,
            isAllowed,
            !isAllowed,
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

    private sealed class ConsumerSampleManifestSource(IReadOnlyList<GraphEditorPluginManifestSourceCandidate> candidates) : IGraphEditorPluginManifestSource
    {
        public IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates()
            => candidates;
    }
}
