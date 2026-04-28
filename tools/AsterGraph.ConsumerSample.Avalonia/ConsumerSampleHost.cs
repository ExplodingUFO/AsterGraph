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
    public static readonly NodeDefinitionId PluginDataDefinitionId = new("consumer.sample.plugin.data");
    public static readonly NodeDefinitionId PluginAiDefinitionId = new("consumer.sample.plugin.ai");
    public static readonly NodeDefinitionId PluginDiagnosticsDefinitionId = new("consumer.sample.plugin.diagnostics");
    public static readonly NodeDefinitionId PluginLayoutDefinitionId = new("consumer.sample.plugin.layout");

    private const string InitialReviewNodeId = "consumer-sample-review-001";
    private const string InitialQueueNodeId = "consumer-sample-queue-001";
    private const string InputPortId = "input";
    private const string ReviewPolicyPortId = "policy";
    private const string OutputPortId = "output";
    private const string ReviewAuditPortId = "audit";
    private const string PluginId = "consumer.sample.audit-plugin";
    public const string PluginCommandId = "consumer.sample.plugin.add-audit-node";
    public const string QueueLaneSnippetId = "consumer.sample.snippet.queue-lane";
    public const string ScenarioId = "content-review-release-lane";
    public const string ScenarioTitle = "Content Review Release Lane";
    private const int WorkbenchRecentLimit = 8;
    internal static readonly GraphSize ReviewNodeDefaultSize = new(320, 220);
    private static readonly IReadOnlyList<ConsumerSampleSnippetDescriptor> SnippetCatalogEntries =
    [
        new(
            QueueLaneSnippetId,
            "Connected Queue Lane",
            "Adds a queue lane connected from the first review node output.",
            "Workflow",
            "Host snippet",
            "Preview: Review output -> Queue input, with one connected queue node.",
            IsFavorite: true,
            SearchKeywords: ["queue", "review", "lane", "connected"]),
        new(
            "consumer.sample.snippet.audit-branch",
            "Audit Branch",
            "Documents the review audit branch shape used by the sample graph.",
            "Diagnostics",
            "Host snippet",
            "Preview: Review audit output -> Audit node input; copy this branch in hosts that own audit plugins.",
            IsFavorite: false,
            SearchKeywords: ["audit", "plugin", "diagnostics", "branch"]),
    ];
    private static readonly IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> WorkbenchFrictionEvidenceEntries =
    [
        new(
            "layout-resume",
            "Hosts need a copyable way to restore the default workbench layout after panel drag or reset.",
            1,
            "AsterGraphHostBuilder.UseDefaultWorkbench + hosted panel-state projection",
            "local synthetic evidence only; no remote sync, WPF parity, runtime route, or GA claim",
            IsSynthetic: true),
        new(
            "node-discovery",
            "Node discovery should show source labels and recent/favorite hints without making users remember catalog ids.",
            1,
            "IGraphEditorSession.Queries.GetRegisteredNodeDefinitions + hosted discovery projection",
            "local synthetic evidence only; no marketplace, macro/query system, runtime route, or GA claim",
            IsSynthetic: true),
        new(
            "command-feedback",
            "Toolbar, context menu, and command palette actions need shared disabled reasons for support triage.",
            2,
            "IGraphEditorSession.Queries.GetCommandDescriptors + hosted command projection",
            "local synthetic evidence only; no new command engine, runtime route, WPF parity, or GA claim",
            IsSynthetic: true),
        new(
            "support-triage",
            "Support intake should keep proof markers, route, version, and friction notes together for bounded beta feedback.",
            2,
            "ConsumerSample.Avalonia -- --proof --support-bundle",
            "local synthetic evidence only; not an external adopter report, telemetry feed, or GA readiness claim",
            IsSynthetic: true),
    ];
    private readonly GraphEditorViewModel _editor;
    private readonly ConsumerSamplePluginAllowlistTrustPolicy _trustPolicy;
    private readonly GraphEditorPluginDiscoveryOptions _pluginDiscoveryOptions;
    private readonly string _storageRootPath;
    private int _nextHostNodeOrdinal = 2;
    private GraphLayoutPlan? _layoutPreview;
    private IReadOnlyList<string> _focusedSubgraphNodeIds = [];
    private IReadOnlyList<string> _dimmedNodeIds = [];
    private IReadOnlyList<string> _alignmentHelperLines = [];
    private IReadOnlyList<string> _lastRuntimeLogExportLines = [];
    private readonly List<ConsumerSampleNavigationHistoryEntry> _navigationHistory = [];
    private readonly List<string> _recentSnippetIds = [];
    private readonly List<string> _favoriteNodeDefinitionIds = [];
    private readonly List<string> _recentNodeDefinitionIds = [];
    private readonly List<string> _favoriteCommandIds = [];
    private readonly List<string> _recentCommandIds = [];
    private readonly List<string> _favoritePluginSourceIds = [];
    private readonly List<string> _recentPluginSourceIds = [];
    private int _navigationHistoryIndex = -1;
    private GraphEditorViewportSnapshot? _pendingViewportRestore;
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

    public IReadOnlyList<ConsumerSampleSnippetDescriptor> SnippetCatalog => SnippetCatalogEntries;

    public IReadOnlyList<string> FavoriteSnippetIds
        => SnippetCatalog
            .Where(snippet => snippet.IsFavorite)
            .Select(snippet => snippet.Id)
            .ToArray();

    public IReadOnlyList<string> RecentSnippetIds => _recentSnippetIds.ToArray();

    public IReadOnlyList<ConsumerSampleRecentsFavoritesEvidence> RecentsFavoritesEvidence
    {
        get
        {
            var definitions = Session.Queries.GetRegisteredNodeDefinitions()
                .ToDictionary(definition => definition.Id.ToString(), StringComparer.Ordinal);
            var commandDescriptors = Session.Queries.GetCommandDescriptors()
                .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

            return
            [
                new(
                    "node",
                    _recentNodeDefinitionIds.ToArray(),
                    _favoriteNodeDefinitionIds.ToArray(),
                    _recentNodeDefinitionIds
                        .Concat(_favoriteNodeDefinitionIds)
                        .Distinct(StringComparer.Ordinal)
                        .Select(id => definitions.TryGetValue(id, out var definition)
                            && definition.Category.Contains("plugin", StringComparison.OrdinalIgnoreCase)
                                ? "Plugin node"
                                : "Host node")
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    WorkbenchRecentLimit,
                    IsHostOwned: true),
                new(
                    "fragment",
                    RecentSnippetIds,
                    FavoriteSnippetIds,
                    SnippetCatalog
                        .Select(snippet => snippet.SourceLabel)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    WorkbenchRecentLimit,
                    IsHostOwned: true),
                new(
                    "command",
                    _recentCommandIds.ToArray(),
                    _favoriteCommandIds.ToArray(),
                    _recentCommandIds
                        .Concat(_favoriteCommandIds)
                        .Distinct(StringComparer.Ordinal)
                        .Select(id => commandDescriptors.TryGetValue(id, out var descriptor)
                            ? descriptor.Source.ToString()
                            : "Host command")
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    WorkbenchRecentLimit,
                    IsHostOwned: true),
                new(
                    "plugin",
                    _recentPluginSourceIds.ToArray(),
                    _favoritePluginSourceIds.ToArray(),
                    LocalPluginGalleryEntries
                        .Select(entry => entry.SourceLabel)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray(),
                    WorkbenchRecentLimit,
                    IsHostOwned: true),
            ];
        }
    }

    public IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> WorkbenchFrictionEvidence
        => WorkbenchFrictionEvidenceEntries;

    public ConsumerSampleWorkbenchAffordancePolish LayoutResumeAffordance
        => new(
            "consumer.sample.workbench.layout-resume",
            "layout-resume",
            "Reset to default workbench layout",
            "AsterGraphWorkbenchOptions.ResetLayout via hosted workbench options",
            "hosted affordance only; no runtime route, WPF parity, remote sync, macro/query system, execution engine, or GA claim",
            UsesExistingHostedRoute: true);

    public AsterGraphWorkbenchOptions ApplyLayoutResumeAffordance(AsterGraphWorkbenchOptions current)
        => current.ResetLayout();

    public IReadOnlyList<string> LastRuntimeLogExportLines => _lastRuntimeLogExportLines;

    public IReadOnlyList<string> RuntimeLogExportLines
        => RuntimeOverlay.RecentLogs
            .Select(log => $"{log.Id} | {log.TimestampUtc:O} | {log.Status} | scope={log.ScopeId ?? "-"} | node={log.NodeId ?? "-"} | connection={log.ConnectionId ?? "-"} | {log.Message}")
            .ToArray();

    public IReadOnlyList<ConsumerSampleNavigationHistoryEntry> NavigationHistory => _navigationHistory;

    public bool CanNavigateBack => _navigationHistoryIndex > 0;

    public bool CanNavigateForward => _navigationHistoryIndex >= 0 && _navigationHistoryIndex < _navigationHistory.Count - 1;

    public bool CanRestoreViewport => _pendingViewportRestore is not null;

    public IReadOnlyList<ConsumerSampleScopeBreadcrumbEntry> ScopeBreadcrumbs
        => Session.Queries.GetScopeNavigationSnapshot().Breadcrumbs
            .Select(breadcrumb => new ConsumerSampleScopeBreadcrumbEntry(
                breadcrumb.ScopeId,
                breadcrumb.Title,
                IsCurrent: string.Equals(
                    breadcrumb.ScopeId,
                    Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId,
                    StringComparison.Ordinal)))
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
    {
        var executed = Session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(PluginCommandId));
        if (executed)
        {
            TrackRecentCommand(PluginCommandId);
            TrackRecentNodeDefinition(PluginAuditDefinitionId);
            TrackRecentPluginSource(PluginId);
        }

        return executed;
    }

    public void FavoriteNodeDefinition(NodeDefinitionId definitionId)
        => AddBoundedFavorite(_favoriteNodeDefinitionIds, definitionId.ToString());

    public void TrackRecentNodeDefinition(NodeDefinitionId definitionId)
        => TrackBoundedRecent(_recentNodeDefinitionIds, definitionId.ToString(), WorkbenchRecentLimit);

    public void FavoriteCommand(string commandId)
        => AddBoundedFavorite(_favoriteCommandIds, commandId);

    public void TrackRecentCommand(string commandId)
        => TrackBoundedRecent(_recentCommandIds, commandId, WorkbenchRecentLimit);

    public void FavoritePluginSource(string pluginId)
        => AddBoundedFavorite(_favoritePluginSourceIds, pluginId);

    public void TrackRecentPluginSource(string pluginId)
        => TrackBoundedRecent(_recentPluginSourceIds, pluginId, WorkbenchRecentLimit);

    public IReadOnlyList<ConsumerSampleSnippetDescriptor> SearchSnippetCatalog(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return SnippetCatalog;
        }

        var normalizedQuery = query.Trim();
        return SnippetCatalog
            .Where(snippet => MatchesSnippet(snippet, normalizedQuery))
            .ToArray();
    }

    public string? GetSnippetPreview(string snippetId)
        => SnippetCatalog
            .FirstOrDefault(snippet => string.Equals(snippet.Id, snippetId, StringComparison.Ordinal))
            ?.PreviewText;

    public bool TryInsertSnippet(string snippetId)
    {
        if (!string.Equals(snippetId, QueueLaneSnippetId, StringComparison.Ordinal)
            || !TryInsertConnectedQueueLaneSnippet())
        {
            return false;
        }

        TrackRecentSnippet(snippetId);
        TrackRecentNodeDefinition(QueueDefinitionId);
        return true;
    }

    public bool ApproveSelection()
        => Session.Commands.TrySetSelectedNodeParameterValue("status", "approved");

    public bool TrySetSelectedOwner(string owner)
        => Session.Commands.TrySetSelectedNodeParameterValue("owner", owner);

    public void FitView()
    {
        Session.Commands.FitToViewport(updateStatus: false);
        TrackRecentCommand("viewport.fit");
    }

    public void SelectNode(string nodeId)
        => Session.Commands.SetSelection([nodeId], nodeId, updateStatus: false);

    public IReadOnlyList<ConsumerSampleGraphSearchResult> SearchGraph(
        string query,
        ConsumerSampleGraphSearchScope scope = ConsumerSampleGraphSearchScope.All)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalizedQuery = query.Trim();
        var document = Session.Queries.CreateDocumentSnapshot();
        var selection = Session.Queries.GetSelectionSnapshot();
        var navigation = Session.Queries.GetScopeNavigationSnapshot();
        var runtimeOverlay = RuntimeOverlay;
        var definitions = Session.Queries.GetRegisteredNodeDefinitions()
            .ToDictionary(definition => definition.Id, definition => definition);
        var scopedNodeIds = ResolveSearchNodeScope(document, selection, scope, navigation.CurrentScopeId);
        var scopedConnectionIds = ResolveSearchConnectionScope(document, selection, scope, navigation.CurrentScopeId, scopedNodeIds);
        var results = new List<ConsumerSampleGraphSearchResult>();

        foreach (var graphScope in document.GraphScopes)
        {
            foreach (var node in graphScope.Nodes.Where(node => scopedNodeIds.Contains(CreateScopedSearchId(graphScope.Id, node.Id))))
            {
                INodeDefinition? definition = null;
                if (node.DefinitionId is { } definitionId)
                {
                    definitions.TryGetValue(definitionId, out definition);
                }

                var runtime = runtimeOverlay.NodeOverlays.FirstOrDefault(overlay =>
                    string.Equals(overlay.NodeId, node.Id, StringComparison.Ordinal));
                var logs = runtimeOverlay.RecentLogs.Where(log =>
                    string.Equals(log.NodeId, node.Id, StringComparison.Ordinal)
                    && IsRuntimeScopeMatch(log.ScopeId, graphScope.Id));
                var searchableFields = EnumerateNodeSearchFields(node, definition, runtime, logs);
                var matchText = FindFirstMatch(searchableFields, normalizedQuery);
                if (matchText is null)
                {
                    continue;
                }

                results.Add(new ConsumerSampleGraphSearchResult(
                    Id: $"node:{graphScope.Id}:{node.Id}",
                    Kind: "Node",
                    Title: node.Title,
                    MatchText: matchText,
                    SourceLabel: "Graph node",
                    NodeId: node.Id,
                    ConnectionId: null,
                    ScopeId: graphScope.Id));
            }

            foreach (var connection in graphScope.Connections.Where(connection => scopedConnectionIds.Contains(CreateScopedSearchId(graphScope.Id, connection.Id))))
            {
                var sourceNode = graphScope.Nodes.FirstOrDefault(node =>
                    string.Equals(node.Id, connection.SourceNodeId, StringComparison.Ordinal));
                var targetNode = graphScope.Nodes.FirstOrDefault(node =>
                    string.Equals(node.Id, connection.TargetNodeId, StringComparison.Ordinal));
                var runtime = runtimeOverlay.ConnectionOverlays.FirstOrDefault(overlay =>
                    string.Equals(overlay.ConnectionId, connection.Id, StringComparison.Ordinal));
                var logs = runtimeOverlay.RecentLogs.Where(log =>
                    string.Equals(log.ConnectionId, connection.Id, StringComparison.Ordinal)
                    && IsRuntimeScopeMatch(log.ScopeId, graphScope.Id));
                var searchableFields = EnumerateConnectionSearchFields(connection, sourceNode, targetNode, runtime, logs);
                var matchText = FindFirstMatch(searchableFields, normalizedQuery);
                if (matchText is null)
                {
                    continue;
                }

                results.Add(new ConsumerSampleGraphSearchResult(
                    Id: $"connection:{graphScope.Id}:{connection.Id}",
                    Kind: "Connection",
                    Title: connection.Label,
                    MatchText: matchText,
                    SourceLabel: "Graph connection",
                    NodeId: null,
                    ConnectionId: connection.Id,
                    ScopeId: graphScope.Id));
            }
        }

        return results
            .OrderBy(result => result.Kind, StringComparer.Ordinal)
            .ThenBy(result => result.Title, StringComparer.Ordinal)
            .ThenBy(result => result.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public bool TryLocateGraphSearchResult(ConsumerSampleGraphSearchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        PushNavigationHistory("Before graph search locate");
        var document = Session.Queries.CreateDocumentSnapshot();
        if (!TryNavigateToGraphSearchScope(document, result.ScopeId))
        {
            return false;
        }

        document = Session.Queries.CreateDocumentSnapshot();
        var activeScopeId = Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId;
        var activeScope = document.GraphScopes.FirstOrDefault(graphScope =>
            string.Equals(graphScope.Id, activeScopeId, StringComparison.Ordinal));
        if (activeScope is null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(result.NodeId)
            && activeScope.Nodes.Any(node => string.Equals(node.Id, result.NodeId, StringComparison.Ordinal)))
        {
            Session.Commands.SetSelection([result.NodeId], result.NodeId, updateStatus: false);
            Session.Commands.CenterViewOnNode(result.NodeId);
            PushNavigationHistory($"Node {result.NodeId}");
            StateChanged?.Invoke(this, EventArgs.Empty);
            return string.Equals(Session.Queries.GetSelectionSnapshot().PrimarySelectedNodeId, result.NodeId, StringComparison.Ordinal);
        }

        if (!string.IsNullOrWhiteSpace(result.ConnectionId)
            && activeScope.Connections.Any(connection => string.Equals(connection.Id, result.ConnectionId, StringComparison.Ordinal)))
        {
            Session.Commands.SetConnectionSelection([result.ConnectionId], result.ConnectionId, updateStatus: false);
            var geometry = Session.Queries.GetConnectionGeometrySnapshots()
                .FirstOrDefault(snapshot => string.Equals(snapshot.ConnectionId, result.ConnectionId, StringComparison.Ordinal));
            if (geometry is not null)
            {
                Session.Commands.CenterViewAt(new GraphPoint(
                    (geometry.Source.Position.X + geometry.Target.Position.X) / 2d,
                    (geometry.Source.Position.Y + geometry.Target.Position.Y) / 2d),
                    updateStatus: false);
            }

            PushNavigationHistory($"Connection {result.ConnectionId}");
            StateChanged?.Invoke(this, EventArgs.Empty);
            return string.Equals(Session.Queries.GetSelectionSnapshot().PrimarySelectedConnectionId, result.ConnectionId, StringComparison.Ordinal);
        }

        return false;
    }

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

        PushNavigationHistory("Before selected subgraph focus");
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
        PushNavigationHistory("Selected subgraph");
        StateChanged?.Invoke(this, EventArgs.Empty);
        return _focusedSubgraphNodeIds.Count > 0 && _dimmedNodeIds.Count > 0;
    }

    public bool FocusCurrentScopeForReview()
    {
        var before = Session.Queries.GetViewportSnapshot();
        _pendingViewportRestore = before;
        PushNavigationHistory("Before current scope focus");
        Session.Commands.FocusCurrentScope(updateStatus: false);
        var after = Session.Queries.GetViewportSnapshot();
        PushNavigationHistory("Current scope");
        StateChanged?.Invoke(this, EventArgs.Empty);
        return !ViewportEquals(before, after);
    }

    public bool RestorePreviousViewport()
    {
        var restore = _pendingViewportRestore;
        if (restore is null)
        {
            return false;
        }

        ApplyViewport(restore);
        _pendingViewportRestore = null;
        PushNavigationHistory("Restored viewport");
        StateChanged?.Invoke(this, EventArgs.Empty);
        return ViewportEquals(Session.Queries.GetViewportSnapshot(), restore);
    }

    public bool TryNavigateBack()
    {
        if (!CanNavigateBack)
        {
            return false;
        }

        _navigationHistoryIndex--;
        var navigated = ApplyNavigationHistoryEntry(_navigationHistory[_navigationHistoryIndex]);
        if (navigated)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        return navigated;
    }

    public bool TryNavigateForward()
    {
        if (!CanNavigateForward)
        {
            return false;
        }

        _navigationHistoryIndex++;
        var navigated = ApplyNavigationHistoryEntry(_navigationHistory[_navigationHistoryIndex]);
        if (navigated)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        return navigated;
    }

    public bool TryNavigateToScopeBreadcrumb(string scopeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeId);

        var document = Session.Queries.CreateDocumentSnapshot();
        PushNavigationHistory("Before scope breadcrumb");
        if (!TryNavigateToGraphSearchScope(document, scopeId))
        {
            return false;
        }

        PushNavigationHistory($"Scope {scopeId}");
        StateChanged?.Invoke(this, EventArgs.Empty);
        return string.Equals(Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId, scopeId, StringComparison.Ordinal);
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

        PushNavigationHistory("Before runtime log locate");
        SelectNode(log.NodeId);
        PushNavigationHistory($"Runtime log {log.Id}");
        return true;
    }

    public bool ExportRuntimeLogs()
    {
        _lastRuntimeLogExportLines = RuntimeLogExportLines;
        StateChanged?.Invoke(this, EventArgs.Empty);
        return _lastRuntimeLogExportLines.Count > 0;
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

    private void PushNavigationHistory(string title)
    {
        var selection = Session.Queries.GetSelectionSnapshot();
        var navigation = Session.Queries.GetScopeNavigationSnapshot();
        var entry = new ConsumerSampleNavigationHistoryEntry(
            title,
            selection.PrimarySelectedNodeId,
            selection.PrimarySelectedConnectionId,
            navigation.CurrentScopeId,
            Session.Queries.GetViewportSnapshot());
        if (_navigationHistoryIndex >= 0
            && _navigationHistoryIndex < _navigationHistory.Count
            && NavigationEntryEquals(_navigationHistory[_navigationHistoryIndex], entry))
        {
            return;
        }

        if (_navigationHistoryIndex < _navigationHistory.Count - 1)
        {
            _navigationHistory.RemoveRange(_navigationHistoryIndex + 1, _navigationHistory.Count - _navigationHistoryIndex - 1);
        }

        _navigationHistory.Add(entry);
        if (_navigationHistory.Count > 24)
        {
            _navigationHistory.RemoveAt(0);
        }

        _navigationHistoryIndex = _navigationHistory.Count - 1;
    }

    private bool ApplyNavigationHistoryEntry(ConsumerSampleNavigationHistoryEntry entry)
    {
        if (!TryNavigateToGraphSearchScope(Session.Queries.CreateDocumentSnapshot(), entry.ScopeId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entry.NodeId))
        {
            Session.Commands.SetSelection([entry.NodeId], entry.NodeId, updateStatus: false);
        }
        else if (!string.IsNullOrWhiteSpace(entry.ConnectionId))
        {
            Session.Commands.SetConnectionSelection([entry.ConnectionId], entry.ConnectionId, updateStatus: false);
        }
        else
        {
            Session.Commands.ClearSelection(updateStatus: false);
        }

        ApplyViewport(entry.Viewport);
        return true;
    }

    private void ApplyViewport(GraphEditorViewportSnapshot target)
    {
        var current = Session.Queries.GetViewportSnapshot();
        if (current.ViewportWidth <= 0 || current.ViewportHeight <= 0)
        {
            Session.Commands.UpdateViewportSize(target.ViewportWidth, target.ViewportHeight);
            current = Session.Queries.GetViewportSnapshot();
        }

        if (!NearlyEqual(current.Zoom, target.Zoom) && current.Zoom > 0)
        {
            Session.Commands.ZoomAt(
                target.Zoom / current.Zoom,
                new GraphPoint(
                    Math.Max(current.ViewportWidth, target.ViewportWidth) / 2d,
                    Math.Max(current.ViewportHeight, target.ViewportHeight) / 2d));
            current = Session.Queries.GetViewportSnapshot();
        }

        Session.Commands.PanBy(target.PanX - current.PanX, target.PanY - current.PanY);
        current = Session.Queries.GetViewportSnapshot();
        if (!NearlyEqual(current.ViewportWidth, target.ViewportWidth) || !NearlyEqual(current.ViewportHeight, target.ViewportHeight))
        {
            Session.Commands.UpdateViewportSize(target.ViewportWidth, target.ViewportHeight);
        }
    }

    private static bool NavigationEntryEquals(ConsumerSampleNavigationHistoryEntry left, ConsumerSampleNavigationHistoryEntry right)
        => string.Equals(left.NodeId, right.NodeId, StringComparison.Ordinal)
        && string.Equals(left.ConnectionId, right.ConnectionId, StringComparison.Ordinal)
        && string.Equals(left.ScopeId, right.ScopeId, StringComparison.Ordinal)
        && ViewportEquals(left.Viewport, right.Viewport);

    private static bool ViewportEquals(GraphEditorViewportSnapshot left, GraphEditorViewportSnapshot right)
        => NearlyEqual(left.Zoom, right.Zoom)
        && NearlyEqual(left.PanX, right.PanX)
        && NearlyEqual(left.PanY, right.PanY)
        && NearlyEqual(left.ViewportWidth, right.ViewportWidth)
        && NearlyEqual(left.ViewportHeight, right.ViewportHeight);

    private static bool NearlyEqual(double left, double right)
        => Math.Abs(left - right) < 0.0001d;

    private bool TryInsertConnectedQueueLaneSnippet()
    {
        var before = Session.Queries.CreateDocumentSnapshot();
        var sourceNode = before.Nodes.FirstOrDefault(node => node.DefinitionId == ReviewDefinitionId);
        if (sourceNode is null)
        {
            return false;
        }

        Session.Commands.StartConnection(sourceNode.Id, OutputPortId);
        var search = Session.Queries.GetCompatibleNodeDefinitionsForPendingConnection();
        var queueInput = search.Results.FirstOrDefault(result =>
            result.DefinitionId == QueueDefinitionId
            && result.TargetKind == GraphConnectionTargetKind.Port
            && string.Equals(result.TargetId, InputPortId, StringComparison.Ordinal));
        if (queueInput is null)
        {
            Session.Commands.CancelPendingConnection();
            return false;
        }

        var queueCount = before.Nodes.Count(node => node.DefinitionId == QueueDefinitionId);
        var connected = Session.Commands.TryCreateConnectedNodeFromPendingConnection(
            queueInput.DefinitionId,
            queueInput.TargetId,
            queueInput.TargetKind,
            new GraphPoint(520 + (queueCount * 300), 420));
        if (!connected)
        {
            Session.Commands.CancelPendingConnection();
            return false;
        }

        var after = Session.Queries.CreateDocumentSnapshot();
        return after.Nodes.Count == before.Nodes.Count + 1
            && after.Connections.Count == before.Connections.Count + 1;
    }

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
                        new GraphPort(InputPortId, "Input", PortDirection.Input, "flow", "#F3B36B", new PortTypeId("flow"), "Flow", MaxConnections: 1),
                        new GraphPort(ReviewPolicyPortId, "Policy", PortDirection.Input, "policy", "#A5B4FC", new PortTypeId("policy"), "Policy", MaxConnections: 1),
                    ],
                    [
                        new GraphPort(OutputPortId, "Output", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"), "Flow"),
                        new GraphPort(ReviewAuditPortId, "Audit", PortDirection.Output, "audit", "#FF8A5B", new PortTypeId("audit"), "Audit"),
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
                        new GraphPort(InputPortId, "Input", PortDirection.Input, "flow", "#F3B36B", new PortTypeId("flow"), "Flow", MaxConnections: 1),
                    ],
                    [
                        new GraphPort(OutputPortId, "Output", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"), "Flow"),
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
                new PortDefinition(InputPortId, "Input", new PortTypeId("flow"), "#F3B36B", "Main review flow input.", "Flow", maxConnections: 1),
                new PortDefinition(ReviewPolicyPortId, "Policy", new PortTypeId("policy"), "#A5B4FC", "Host-owned policy input.", "Policy", maxConnections: 1),
            ],
            [
                new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#6AD5C4", "Approved review flow output.", "Flow"),
                new PortDefinition(ReviewAuditPortId, "Audit", new PortTypeId("audit"), "#FF8A5B", "Audit evidence output.", "Audit"),
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
                new PortDefinition(InputPortId, "Input", new PortTypeId("flow"), "#F3B36B", "Queue flow input.", "Flow", maxConnections: 1),
            ],
            [
                new PortDefinition(OutputPortId, "Output", new PortTypeId("flow"), "#8B7BFF", "Queue flow output.", "Flow"),
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
            description: "Trusted sample plugin pack that adds audit, data, AI, diagnostics, and layout sample nodes plus executable command, presentation, and localization hooks.",
            version: "0.2.0-alpha.3",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.2.0-alpha.3",
                targetFramework: "net8.0",
                runtimeSurface: "Create + AsterGraphAvaloniaViewFactory"),
            capabilitySummary: "Adds five sample node definitions, one executable command, one presentation badge, and one localization override.");

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
                    GraphEditorRuntimeOverlayStatus.Error,
                    ElapsedMilliseconds: 8,
                    OutputPreview: "queue handoff pending",
                    WarningCount: 1,
                    ErrorCount: 1,
                    ErrorMessage: "Queue handoff requires reviewer approval.",
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
                new GraphEditorConnectionRuntimeOverlaySnapshot(
                    "consumer-sample-connection-001:stale",
                    GraphEditorRuntimeOverlayStatus.Error,
                    ValuePreview: "{ status: draft, lane: alpha }",
                    PayloadType: "review",
                    ItemCount: 1,
                    IsStale: true),
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
                new GraphEditorRuntimeLogEntrySnapshot(
                    "consumer-sample-runtime-log-002",
                    SampleTimestamp,
                    GraphEditorRuntimeOverlayStatus.Success,
                    "Review payload approved.",
                    ScopeId: "root",
                    NodeId: InitialReviewNodeId,
                    ConnectionId: "consumer-sample-connection-001"),
                new GraphEditorRuntimeLogEntrySnapshot(
                    "consumer-sample-runtime-log-003",
                    SampleTimestamp,
                    GraphEditorRuntimeOverlayStatus.Error,
                    "Queue handoff requires reviewer approval.",
                    ScopeId: "root",
                    NodeId: InitialQueueNodeId,
                    ConnectionId: "consumer-sample-connection-001:stale"),
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
                new NodeDefinition(
                    PluginDataDefinitionId,
                    "Sample Data Import",
                    "AsterGraph Sample Plugins",
                    "Data",
                    [],
                    [
                        new PortDefinition(OutputPortId, "Rows", new PortTypeId("table"), "#6AD5C4"),
                    ],
                    [
                        new NodeParameterDefinition(
                            "source",
                            "Source",
                            new PortTypeId("string"),
                            ParameterEditorKind.Text,
                            defaultValue: "reviews.csv",
                            groupName: "Input",
                            placeholderText: "dataset.csv",
                            helpText: "Local sample path used by plugin authors to model a data source."),
                        new NodeParameterDefinition(
                            "delimiter",
                            "Delimiter",
                            new PortTypeId("enum"),
                            ParameterEditorKind.Enum,
                            defaultValue: "comma",
                            constraints: new ParameterConstraints(
                                AllowedOptions:
                                [
                                    new ParameterOptionDefinition("comma", "Comma"),
                                    new ParameterOptionDefinition("tab", "Tab"),
                                ]),
                            groupName: "Input",
                            helpText: "Shows enum metadata for plugin-authored import nodes."),
                    ],
                    description: "Official sample plugin data node with metadata-rich import parameters.",
                    accentHex: "#6AD5C4",
                    defaultWidth: 270,
                    defaultHeight: 180),
                new NodeDefinition(
                    PluginAiDefinitionId,
                    "Sample AI Prompt",
                    "AsterGraph Sample Plugins",
                    "AI",
                    [
                        new PortDefinition(InputPortId, "Context", new PortTypeId("table"), "#6AD5C4"),
                    ],
                    [
                        new PortDefinition(OutputPortId, "Prompt", new PortTypeId("prompt"), "#8B7BFF"),
                    ],
                    [
                        new NodeParameterDefinition(
                            "systemPrompt",
                            "System Prompt",
                            new PortTypeId("string"),
                            ParameterEditorKind.Text,
                            defaultValue: "Summarize risky review items.",
                            templateKey: "code",
                            groupName: "Prompt",
                            placeholderText: "system instructions",
                            helpText: "Multiline/code-like prompt metadata for plugin authors."),
                        new NodeParameterDefinition(
                            "temperature",
                            "Temperature",
                            new PortTypeId("float"),
                            ParameterEditorKind.Number,
                            defaultValue: 0.2,
                            constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                            groupName: "Prompt",
                            helpText: "Numeric constraint metadata for AI sample nodes."),
                    ],
                    description: "Official sample plugin AI node with prompt and numeric metadata.",
                    accentHex: "#8B7BFF",
                    defaultWidth: 300,
                    defaultHeight: 210),
                new NodeDefinition(
                    PluginDiagnosticsDefinitionId,
                    "Sample Diagnostics Probe",
                    "AsterGraph Sample Plugins",
                    "Diagnostics",
                    [
                        new PortDefinition(InputPortId, "Payload", new PortTypeId("prompt"), "#8B7BFF"),
                    ],
                    [
                        new PortDefinition(OutputPortId, "Report", new PortTypeId("report"), "#F3B36B"),
                    ],
                    [
                        new NodeParameterDefinition(
                            "level",
                            "Level",
                            new PortTypeId("enum"),
                            ParameterEditorKind.Enum,
                            defaultValue: "warning",
                            constraints: new ParameterConstraints(
                                AllowedOptions:
                                [
                                    new ParameterOptionDefinition("info", "Info"),
                                    new ParameterOptionDefinition("warning", "Warning"),
                                    new ParameterOptionDefinition("error", "Error"),
                                ]),
                            groupName: "Diagnostics",
                            helpText: "Diagnostic severity metadata for sample plugin probes."),
                    ],
                    description: "Official sample plugin diagnostics node.",
                    accentHex: "#F3B36B",
                    defaultWidth: 280,
                    defaultHeight: 170),
                new NodeDefinition(
                    PluginLayoutDefinitionId,
                    "Sample Layout Hint",
                    "AsterGraph Sample Plugins",
                    "Layout",
                    [
                        new PortDefinition(InputPortId, "Graph", new PortTypeId("report"), "#F3B36B"),
                    ],
                    [],
                    [
                        new NodeParameterDefinition(
                            "orientation",
                            "Orientation",
                            new PortTypeId("enum"),
                            ParameterEditorKind.Enum,
                            defaultValue: "left-to-right",
                            constraints: new ParameterConstraints(
                                AllowedOptions:
                                [
                                    new ParameterOptionDefinition("left-to-right", "Left to Right"),
                                    new ParameterOptionDefinition("top-to-bottom", "Top to Bottom"),
                                ]),
                            groupName: "Layout",
                            helpText: "Layout hint metadata without introducing a mandatory layout engine."),
                        new NodeParameterDefinition(
                            "spacing",
                            "Spacing",
                            new PortTypeId("int"),
                            ParameterEditorKind.Number,
                            defaultValue: 240,
                            constraints: new ParameterConstraints(Minimum: 80, Maximum: 640),
                            groupName: "Layout",
                            helpText: "Numeric layout spacing metadata for sample plugins."),
                    ],
                    description: "Official sample plugin layout hint node.",
                    accentHex: "#6B9CFF",
                    defaultWidth: 280,
                    defaultHeight: 180),
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
        var galleryLine = $"source Plugin · {manifest.DisplayName} · trust {candidate.TrustEvaluation.Decision} · load {loadState} · fingerprint {FormatFingerprint(fingerprint)}";

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
            "Plugin",
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

    private static HashSet<string> ResolveSearchNodeScope(
        GraphDocument document,
        GraphEditorSelectionSnapshot selection,
        ConsumerSampleGraphSearchScope scope,
        string currentScopeId)
    {
        if (scope is ConsumerSampleGraphSearchScope.CurrentSelection)
        {
            return document.GraphScopes
                .SelectMany(graphScope => graphScope.Nodes
                    .Where(node => selection.SelectedNodeIds.Contains(node.Id))
                    .Select(node => CreateScopedSearchId(graphScope.Id, node.Id)))
                .ToHashSet(StringComparer.Ordinal);
        }

        return ResolveSearchScopes(document, scope, currentScopeId)
            .SelectMany(graphScope => graphScope.Nodes.Select(node => CreateScopedSearchId(graphScope.Id, node.Id)))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static HashSet<string> ResolveSearchConnectionScope(
        GraphDocument document,
        GraphEditorSelectionSnapshot selection,
        ConsumerSampleGraphSearchScope scope,
        string currentScopeId,
        IReadOnlySet<string> scopedNodeIds)
    {
        if (scope is ConsumerSampleGraphSearchScope.CurrentSelection)
        {
            return document.GraphScopes
                .SelectMany(graphScope => graphScope.Connections
                    .Where(connection => selection.SelectedConnectionIds.Contains(connection.Id))
                    .Select(connection => CreateScopedSearchId(graphScope.Id, connection.Id)))
                .ToHashSet(StringComparer.Ordinal);
        }

        return ResolveSearchScopes(document, scope, currentScopeId)
            .SelectMany(graphScope => graphScope.Connections
                .Where(connection =>
                    scopedNodeIds.Contains(CreateScopedSearchId(graphScope.Id, connection.SourceNodeId))
                    || scopedNodeIds.Contains(CreateScopedSearchId(graphScope.Id, connection.TargetNodeId)))
                .Select(connection => CreateScopedSearchId(graphScope.Id, connection.Id)))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static IReadOnlyList<GraphScope> ResolveSearchScopes(
        GraphDocument document,
        ConsumerSampleGraphSearchScope scope,
        string currentScopeId)
    {
        if (scope is not ConsumerSampleGraphSearchScope.CurrentScope)
        {
            return document.GraphScopes;
        }

        return document.GraphScopes
            .Where(graphScope => string.Equals(graphScope.Id, currentScopeId, StringComparison.Ordinal))
            .ToArray();
    }

    private bool TryNavigateToGraphSearchScope(
        GraphDocument document,
        string scopeId)
    {
        if (string.IsNullOrWhiteSpace(scopeId))
        {
            return false;
        }

        if (string.Equals(Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId, scopeId, StringComparison.Ordinal))
        {
            return true;
        }

        if (!TryCreateCompositePathToScope(document, scopeId, out var compositeNodePath))
        {
            return false;
        }

        while (Session.Queries.GetScopeNavigationSnapshot().CanNavigateToParent)
        {
            if (!Session.Commands.TryReturnToParentGraphScope(updateStatus: false))
            {
                return false;
            }
        }

        foreach (var compositeNodeId in compositeNodePath)
        {
            if (!Session.Commands.TryEnterCompositeChildGraph(compositeNodeId, updateStatus: false))
            {
                return false;
            }
        }

        return string.Equals(Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId, scopeId, StringComparison.Ordinal);
    }

    private static bool TryCreateCompositePathToScope(
        GraphDocument document,
        string scopeId,
        out IReadOnlyList<string> compositeNodePath)
    {
        if (document.GraphScopes.All(graphScope => !string.Equals(graphScope.Id, scopeId, StringComparison.Ordinal)))
        {
            compositeNodePath = [];
            return false;
        }

        var parentsByChildScope = new Dictionary<string, (string ParentScopeId, string CompositeNodeId)>(StringComparer.Ordinal);
        foreach (var graphScope in document.GraphScopes)
        {
            foreach (var node in graphScope.Nodes)
            {
                if (node.Composite is not null)
                {
                    parentsByChildScope.TryAdd(node.Composite.ChildGraphId, (graphScope.Id, node.Id));
                }
            }
        }

        var path = new List<string>();
        var cursor = scopeId;
        while (!string.Equals(cursor, document.RootGraphId, StringComparison.Ordinal))
        {
            if (!parentsByChildScope.TryGetValue(cursor, out var parent))
            {
                compositeNodePath = [];
                return false;
            }

            path.Add(parent.CompositeNodeId);
            cursor = parent.ParentScopeId;
        }

        path.Reverse();
        compositeNodePath = path;
        return true;
    }

    private static string CreateScopedSearchId(string scopeId, string value)
        => $"{scopeId}:{value}";

    private static bool IsRuntimeScopeMatch(string? runtimeScopeId, string graphScopeId)
        => string.IsNullOrWhiteSpace(runtimeScopeId)
            ? string.Equals(graphScopeId, GraphDocument.DefaultRootGraphId, StringComparison.Ordinal)
            : string.Equals(runtimeScopeId, graphScopeId, StringComparison.Ordinal);

    private static IEnumerable<string?> EnumerateNodeSearchFields(
        GraphNode node,
        INodeDefinition? definition,
        GraphEditorNodeRuntimeOverlaySnapshot? runtime,
        IEnumerable<GraphEditorRuntimeLogEntrySnapshot> logs)
    {
        yield return node.Id;
        yield return node.Title;
        yield return node.Category;
        yield return node.Subtitle;
        yield return node.Description;
        yield return node.DefinitionId?.Value;
        yield return definition?.DisplayName;
        yield return definition?.Category;
        yield return definition?.Subtitle;
        yield return definition?.Description;
        yield return runtime?.Status.ToString();
        yield return runtime?.OutputPreview;
        yield return runtime?.ErrorMessage;

        foreach (var port in node.Inputs.Concat(node.Outputs))
        {
            yield return port.Id;
            yield return port.Label;
            yield return port.DataType;
            yield return port.TypeId?.Value;
        }

        foreach (var parameter in node.ParameterValues ?? [])
        {
            yield return parameter.Key;
            yield return parameter.TypeId.Value;
            yield return parameter.Value?.ToString();
        }

        foreach (var parameter in definition?.Parameters ?? [])
        {
            yield return parameter.Key;
            yield return parameter.DisplayName;
            yield return parameter.Description;
            yield return parameter.HelpText;
            yield return parameter.GroupName;
            yield return parameter.PlaceholderText;
        }

        foreach (var log in logs)
        {
            yield return log.Status.ToString();
            yield return log.Message;
        }
    }

    private static IEnumerable<string?> EnumerateConnectionSearchFields(
        GraphConnection connection,
        GraphNode? sourceNode,
        GraphNode? targetNode,
        GraphEditorConnectionRuntimeOverlaySnapshot? runtime,
        IEnumerable<GraphEditorRuntimeLogEntrySnapshot> logs)
    {
        yield return connection.Id;
        yield return connection.Label;
        yield return connection.SourceNodeId;
        yield return connection.SourcePortId;
        yield return connection.TargetNodeId;
        yield return connection.TargetPortId;
        yield return sourceNode?.Title;
        yield return targetNode?.Title;
        yield return runtime?.Status.ToString();
        yield return runtime?.ValuePreview;
        yield return runtime?.PayloadType;

        foreach (var log in logs)
        {
            yield return log.Status.ToString();
            yield return log.Message;
        }
    }

    private static string? FindFirstMatch(IEnumerable<string?> values, string query)
        => values.FirstOrDefault(value =>
            !string.IsNullOrWhiteSpace(value)
            && value.Contains(query, StringComparison.OrdinalIgnoreCase));

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

    private static bool MatchesSnippet(ConsumerSampleSnippetDescriptor snippet, string query)
        => snippet.Id.Contains(query, StringComparison.OrdinalIgnoreCase)
        || snippet.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
        || snippet.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
        || snippet.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
        || snippet.SearchKeywords.Any(keyword => keyword.Contains(query, StringComparison.OrdinalIgnoreCase));

    private void TrackRecentSnippet(string snippetId)
        => TrackBoundedRecent(_recentSnippetIds, snippetId, WorkbenchRecentLimit);

    private static void TrackBoundedRecent(List<string> ids, string id, int limit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        ids.RemoveAll(candidate => string.Equals(candidate, id, StringComparison.Ordinal));
        ids.Insert(0, id);
        if (ids.Count > limit)
        {
            ids.RemoveRange(limit, ids.Count - limit);
        }
    }

    private static void AddBoundedFavorite(List<string> ids, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!ids.Contains(id, StringComparer.Ordinal))
        {
            ids.Add(id);
        }
    }
}

public sealed record ConsumerSampleSnippetDescriptor(
    string Id,
    string Title,
    string Description,
    string Category,
    string SourceLabel,
    string PreviewText,
    bool IsFavorite,
    IReadOnlyList<string> SearchKeywords);

public enum ConsumerSampleGraphSearchScope
{
    All,
    CurrentScope,
    CurrentSelection,
}

public sealed record ConsumerSampleGraphSearchResult(
    string Id,
    string Kind,
    string Title,
    string MatchText,
    string SourceLabel,
    string? NodeId,
    string? ConnectionId,
    string ScopeId);

public sealed record ConsumerSampleRecentsFavoritesEvidence(
    string Surface,
    IReadOnlyList<string> RecentIds,
    IReadOnlyList<string> FavoriteIds,
    IReadOnlyList<string> SourceLabels,
    int RecentLimit,
    bool IsHostOwned);

public sealed record ConsumerSampleWorkbenchFrictionEvidence(
    string Category,
    string Evidence,
    int PriorityRank,
    string Route,
    string ScopeBoundary,
    bool IsSynthetic);

public sealed record ConsumerSampleWorkbenchAffordancePolish(
    string ActionId,
    string FrictionCategory,
    string Title,
    string Route,
    string ScopeBoundary,
    bool UsesExistingHostedRoute);

public sealed record ConsumerSampleNavigationHistoryEntry(
    string Title,
    string? NodeId,
    string? ConnectionId,
    string ScopeId,
    GraphEditorViewportSnapshot Viewport);

public sealed record ConsumerSampleScopeBreadcrumbEntry(
    string ScopeId,
    string Title,
    bool IsCurrent);
