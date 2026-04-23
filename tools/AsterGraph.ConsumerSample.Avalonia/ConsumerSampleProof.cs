using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;

namespace AsterGraph.ConsumerSample;

public sealed record ConsumerSampleProofResult(
    bool HostMenuActionOk,
    bool PluginContributionOk,
    bool ParameterProjectionOk,
    bool MetadataProjectionOk,
    bool NodeSideAuthoringOk,
    bool WindowCompositionOk,
    bool TrustTransparencyOk,
    bool CommandSurfaceOk,
    bool StencilSurfaceOk,
    bool ExportBreadthOk,
    bool NodeQuickToolsOk,
    bool EdgeQuickToolsOk,
    bool HostedAccessibilityBaselineOk,
    bool HostedAccessibilityFocusOk,
    bool HostedAccessibilityCommandSurfaceOk,
    bool HostedAccessibilityAuthoringSurfaceOk,
    IReadOnlyList<ConsumerSampleProofParameterSnapshot> ParameterSnapshots,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs,
    double StencilSearchMs,
    double CommandSurfaceRefreshMs,
    double NodeToolProjectionMs,
    double EdgeToolProjectionMs,
    bool HostedAutomationNavigationOk = true,
    bool HostedAuthoringAutomationDiagnosticsOk = true)
{
    public bool AuthoringSurfaceOk
        => ParameterProjectionOk
        && MetadataProjectionOk
        && NodeSideAuthoringOk
        && CommandSurfaceOk;

    public bool CapabilityBreadthOk
        => StencilSurfaceOk
        && ExportBreadthOk
        && NodeQuickToolsOk
        && EdgeQuickToolsOk;

    public bool WidenedSurfacePerformanceOk
        => CommandSurfaceOk
        && CapabilityBreadthOk
        && StencilSearchMs >= 0
        && CommandSurfaceRefreshMs >= 0
        && NodeToolProjectionMs >= 0
        && EdgeToolProjectionMs >= 0;

    public bool HostedAccessibilityOk
        => HostedAccessibilityBaselineOk
        && HostedAccessibilityFocusOk
        && HostedAutomationNavigationOk
        && HostedAuthoringAutomationDiagnosticsOk
        && HostedAccessibilityCommandSurfaceOk
        && HostedAccessibilityAuthoringSurfaceOk;

    public bool IsOk
        => HostMenuActionOk
        && PluginContributionOk
        && TrustTransparencyOk
        && WindowCompositionOk
        && AuthoringSurfaceOk
        && CapabilityBreadthOk
        && HostedAccessibilityOk
        && WidenedSurfacePerformanceOk;

    public IReadOnlyList<string> MetricLines =>
    [
        FormatMetric("startup_ms", StartupMs),
        FormatMetric("inspector_projection_ms", InspectorProjectionMs),
        FormatMetric("plugin_scan_ms", PluginScanMs),
        FormatMetric("command_latency_ms", CommandLatencyMs),
        FormatMetric("stencil_search_ms", StencilSearchMs),
        FormatMetric("command_surface_refresh_ms", CommandSurfaceRefreshMs),
        FormatMetric("node_tool_projection_ms", NodeToolProjectionMs),
        FormatMetric("edge_tool_projection_ms", EdgeToolProjectionMs),
    ];

    internal IReadOnlyList<string> ProofLines =>
    [
        $"CONSUMER_SAMPLE_HOST_ACTION_OK:{HostMenuActionOk}",
        $"CONSUMER_SAMPLE_PLUGIN_OK:{PluginContributionOk}",
        $"AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:{ParameterProjectionOk}",
        $"AUTHORING_SURFACE_METADATA_PROJECTION_OK:{MetadataProjectionOk}",
        $"AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:{NodeSideAuthoringOk}",
        $"AUTHORING_SURFACE_COMMAND_PROJECTION_OK:{CommandSurfaceOk}",
        $"CONSUMER_SAMPLE_PARAMETER_OK:{ParameterProjectionOk}",
        $"CONSUMER_SAMPLE_METADATA_PROJECTION_OK:{MetadataProjectionOk}",
        $"CONSUMER_SAMPLE_WINDOW_OK:{WindowCompositionOk}",
        $"CONSUMER_SAMPLE_TRUST_OK:{TrustTransparencyOk}",
        $"COMMAND_SURFACE_OK:{CommandSurfaceOk}",
        $"CAPABILITY_BREADTH_STENCIL_OK:{StencilSurfaceOk}",
        $"CAPABILITY_BREADTH_EXPORT_OK:{ExportBreadthOk}",
        $"CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:{NodeQuickToolsOk}",
        $"CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:{EdgeQuickToolsOk}",
        $"CAPABILITY_BREADTH_OK:{CapabilityBreadthOk}",
        $"HOSTED_ACCESSIBILITY_BASELINE_OK:{HostedAccessibilityBaselineOk}",
        $"HOSTED_ACCESSIBILITY_FOCUS_OK:{HostedAccessibilityFocusOk}",
        $"HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:{HostedAutomationNavigationOk}",
        $"HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:{HostedAuthoringAutomationDiagnosticsOk}",
        $"HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:{HostedAccessibilityCommandSurfaceOk}",
        $"HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:{HostedAccessibilityAuthoringSurfaceOk}",
        $"HOSTED_ACCESSIBILITY_OK:{HostedAccessibilityOk}",
        $"WIDENED_SURFACE_PERFORMANCE_OK:{WidenedSurfacePerformanceOk}",
        .. MetricLines,
        $"AUTHORING_SURFACE_OK:{AuthoringSurfaceOk}",
        $"CONSUMER_SAMPLE_OK:{IsOk}",
    ];

    private static string FormatMetric(string name, double value)
        => $"HOST_NATIVE_METRIC:{name}={value.ToString("0.###", CultureInfo.InvariantCulture)}";
}

public static class ConsumerSampleProof
{
    public static ConsumerSampleProofResult Run()
    {
        ConsumerSampleHeadlessEnvironment.EnsureInitialized();

        var storageRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Proof", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        ConsumerSampleHost? createdHost = null;
        var startupMs = MeasureMilliseconds(() => createdHost = ConsumerSampleHost.Create(storageRoot));
        using var host = createdHost ?? throw new InvalidOperationException("Consumer sample host was not created.");
        var window = ConsumerSampleWindowFactory.Create(host);

        GraphEditorNodeParameterSnapshot[] selectedParameterSnapshots = [];
        bool metadataProjectionOk;
        bool nodeSideAuthoringOk;
        bool commandSurfaceOk;
        bool hostMenuActionOk;
        bool pluginContributionOk;
        bool parameterProjectionOk;
        bool trustTransparencyOk;
        bool windowCompositionOk;
        double inspectorProjectionMs;
        double pluginScanMs;
        double commandLatencyMs;
        double stencilSearchMs;
        double commandSurfaceRefreshMs;
        double nodeToolProjectionMs;
        double edgeToolProjectionMs;

        try
        {
            window.Show();
            FlushUi();

            host.SelectNode(host.GetFirstReviewNodeId());
            FlushUi();
            inspectorProjectionMs = MeasureMilliseconds(() => selectedParameterSnapshots = host.GetSelectedParameterSnapshots().ToArray());
            pluginScanMs = MeasureMilliseconds(() => host.PluginCandidates.ToArray());
            metadataProjectionOk = HasMetadataProjection(selectedParameterSnapshots);
            nodeSideAuthoringOk = HasNodeSideAuthoring(window, host, host.GetFirstReviewNodeId());

            var commandBaseline = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
            host.Session.Commands.AddNode(ConsumerSampleHost.ReviewDefinitionId, new GraphPoint(880, 220));
            var undoAction = AsterGraphHostedActionFactory.CreateCommandActions(host.Session, ["history.undo"])
                .Single(action => string.Equals(action.Id, "history.undo", StringComparison.Ordinal));
            commandLatencyMs = MeasureMilliseconds(() => undoAction.TryExecute());
            commandSurfaceOk = undoAction.CanExecute
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == commandBaseline;

            var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
            hostMenuActionOk = host.AddHostReviewNode()
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == initialSnapshot.Nodes.Count + 1;

            var afterHostSnapshot = host.Session.Queries.CreateDocumentSnapshot();
            pluginContributionOk = host.HasPluginNodeDefinition()
                && host.HasPluginCommandContribution()
                && host.AddPluginAuditNode()
                && host.PluginLoadSnapshots.Any(snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin")
                && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == afterHostSnapshot.Nodes.Count + 1;

            host.SelectNode(host.GetFirstReviewNodeId());
            parameterProjectionOk = host.TrySetSelectedOwner("release-owner")
                && host.ApproveSelection()
                && host.GetSelectedParameterSnapshots().Any(snapshot =>
                    snapshot.Definition.Key == "status"
                    && string.Equals(snapshot.CurrentValue?.ToString(), "approved", StringComparison.Ordinal))
                && host.GetSelectedParameterSnapshots().Any(snapshot =>
                    snapshot.Definition.Key == "owner"
                    && string.Equals(snapshot.CurrentValue?.ToString(), "release-owner", StringComparison.Ordinal));

            trustTransparencyOk =
                host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
                && host.ExportPluginAllowlist()
                && File.Exists(host.PluginAllowlistExchangePath)
                && host.BlockPluginCandidate("consumer.sample.audit-plugin")
                && host.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
                && host.ImportPluginAllowlist()
                && host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase));

            stencilSearchMs = MeasureStencilSearchMilliseconds(host.Session, "plugin");
            commandSurfaceRefreshMs = MeasureCommandSurfaceRefreshMilliseconds(host.Session);
            nodeToolProjectionMs = MeasureNodeToolProjectionMilliseconds(host.Session, host.GetFirstReviewNodeId());
            edgeToolProjectionMs = MeasureEdgeToolProjectionMilliseconds(host.Session, host.GetFirstReviewNodeId(), "consumer-sample-connection-001");

            windowCompositionOk =
                FindNamed<Menu>(window, "PART_MainMenu") is not null
                && FindNamed<Button>(window, "PART_AddReviewNodeButton") is not null
                && FindNamed<Button>(window, "PART_AddPluginNodeButton") is not null
                && FindNamed<Button>(window, "PART_ApproveSelectionButton") is not null
                && FindNamed<ItemsControl>(window, "PART_PluginCandidateItems") is not null
                && FindNamed<ItemsControl>(window, "PART_AllowlistItems") is not null
                && FindNamed<TextBlock>(window, "PART_TrustBoundaryText") is not null;
        }
        finally
        {
            window.Close();
        }

        var exportBreadthOk = HasExportBreadth(host, storageRoot);
        var capabilityWindow = CreateCapabilityBreadthWindow(host);
        bool stencilSurfaceOk;
        bool edgeQuickToolsOk;
        bool nodeQuickToolsOk;
        bool hostedAccessibilityBaselineOk;
        bool hostedAccessibilityFocusOk;
        bool hostedAutomationNavigationOk;
        bool hostedAuthoringAutomationDiagnosticsOk;
        bool hostedAccessibilityCommandSurfaceOk;
        bool hostedAccessibilityAuthoringSurfaceOk;
        try
        {
            capabilityWindow.Show();
            FlushUi();

            var reviewNodeId = host.GetFirstReviewNodeId();
            host.SelectNode(reviewNodeId);
            FlushUi();

            hostedAccessibilityBaselineOk = HasHostedAccessibilityBaselines(capabilityWindow);
            hostedAccessibilityAuthoringSurfaceOk = HasHostedAccessibilityAuthoringSurface(capabilityWindow);
            hostedAccessibilityFocusOk = HasHostedAccessibilityFocus(capabilityWindow);
            hostedAutomationNavigationOk = HasHostedAutomationNavigation(capabilityWindow, host, reviewNodeId);
            hostedAuthoringAutomationDiagnosticsOk = HasHostedAuthoringAutomationDiagnostics(host, reviewNodeId);
            hostedAccessibilityCommandSurfaceOk = HasHostedAccessibilityCommandSurface(capabilityWindow);
            host.SelectNode(reviewNodeId);
            FlushUi();

            stencilSurfaceOk = HasStencilSurface(capabilityWindow, host);
            host.SelectNode(reviewNodeId);
            FlushUi();
            edgeQuickToolsOk = HasEdgeQuickTools(capabilityWindow, host);

            host.SelectNode(reviewNodeId);
            FlushUi();
            nodeQuickToolsOk = HasNodeQuickTools(capabilityWindow, host, reviewNodeId);
        }
        finally
        {
            capabilityWindow.Close();
        }

        return new ConsumerSampleProofResult(
            HostMenuActionOk: hostMenuActionOk,
            PluginContributionOk: pluginContributionOk,
            ParameterProjectionOk: parameterProjectionOk,
            MetadataProjectionOk: metadataProjectionOk,
            NodeSideAuthoringOk: nodeSideAuthoringOk,
            WindowCompositionOk: windowCompositionOk,
            TrustTransparencyOk: trustTransparencyOk,
            CommandSurfaceOk: commandSurfaceOk,
            StencilSurfaceOk: stencilSurfaceOk,
            ExportBreadthOk: exportBreadthOk,
            NodeQuickToolsOk: nodeQuickToolsOk,
            EdgeQuickToolsOk: edgeQuickToolsOk,
            HostedAccessibilityBaselineOk: hostedAccessibilityBaselineOk,
            HostedAccessibilityFocusOk: hostedAccessibilityFocusOk,
            HostedAccessibilityCommandSurfaceOk: hostedAccessibilityCommandSurfaceOk,
            HostedAccessibilityAuthoringSurfaceOk: hostedAccessibilityAuthoringSurfaceOk,
            ParameterSnapshots: CreateParameterSnapshots(host.GetSelectedParameterSnapshots().ToArray()),
            StartupMs: startupMs,
            InspectorProjectionMs: inspectorProjectionMs,
            PluginScanMs: pluginScanMs,
            CommandLatencyMs: commandLatencyMs,
            StencilSearchMs: stencilSearchMs,
            CommandSurfaceRefreshMs: commandSurfaceRefreshMs,
            NodeToolProjectionMs: nodeToolProjectionMs,
            EdgeToolProjectionMs: edgeToolProjectionMs,
            HostedAutomationNavigationOk: hostedAutomationNavigationOk,
            HostedAuthoringAutomationDiagnosticsOk: hostedAuthoringAutomationDiagnosticsOk);
    }

    private static Window CreateCapabilityBreadthWindow(ConsumerSampleHost host)
    {
        var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = host.Editor,
            ChromeMode = GraphEditorViewChromeMode.Default,
            EnableDefaultContextMenu = true,
            CommandShortcutPolicy = AsterGraphCommandShortcutPolicy.Default,
            Presentation = ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions(),
        });

        return new Window
        {
            Width = 1440,
            Height = 900,
            Content = view,
        };
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

    private static bool HasNodeSideAuthoring(Window window, ConsumerSampleHost host, string reviewNodeId)
    {
        var initialWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .Size.Width;
        var widenButton = FindNamed<Button>(window, $"PART_RecipeToolbarWiden_{reviewNodeId}");
        if (widenButton is not null)
        {
            widenButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            FlushUi();
        }

        var widenedWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .Size.Width;
        var nodeSnapshots = host.Session.Queries.GetNodeParameterSnapshots(reviewNodeId);
        var statusTemplateOk = nodeSnapshots.Any(snapshot =>
            snapshot.Definition.Key == "status"
            && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey, StringComparison.Ordinal));
        var ownerTemplateOk = nodeSnapshots.Any(snapshot =>
            snapshot.Definition.Key == "owner"
            && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey, StringComparison.Ordinal));

        return FindNamed<Canvas>(window, "PART_AuthoringEdgeOverlay") is not null
            && FindNamed<Border>(window, "PART_AuthoringEdgeBadge_consumer-sample-connection-001") is not null
            && widenButton is not null
            && widenedWidth > initialWidth
            && statusTemplateOk
            && ownerTemplateOk;
    }

    private static bool HasStencilSurface(Window window, ConsumerSampleHost host)
    {
        var searchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        if (searchBox is null)
        {
            return false;
        }

        var templates = host.Session.Queries.GetNodeTemplateSnapshots();
        var pluginTemplate = templates.SingleOrDefault(template =>
            template.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        if (pluginTemplate is null)
        {
            return false;
        }

        var allSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        if (allSections.Count < 2)
        {
            return false;
        }

        searchBox.Text = "plugin";
        FlushUi();

        var filteredSections = window.GetVisualDescendants()
            .OfType<Expander>()
            .Where(section => section.Name?.StartsWith("PART_StencilSection_", StringComparison.Ordinal) == true)
            .ToList();
        var pluginSection = filteredSections.SingleOrDefault(section =>
            string.Equals(section.Tag?.ToString(), pluginTemplate.Category, StringComparison.Ordinal));
        var pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        if (filteredSections.Count != 1 || pluginSection is null || pluginCard is null)
        {
            return false;
        }

        var initialPluginNodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId);
        pluginSection.IsExpanded = false;
        FlushUi();
        pluginSection.IsExpanded = true;
        FlushUi();

        pluginCard = FindNamed<Button>(window, $"PART_StencilCard_{pluginTemplate.Key}");
        if (pluginCard is null || !pluginSection.IsExpanded)
        {
            return false;
        }

        pluginCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return host.Session.Queries.CreateDocumentSnapshot().Nodes.Count(node =>
            node.DefinitionId == ConsumerSampleHost.PluginAuditDefinitionId) == initialPluginNodeCount + 1;
    }

    private static bool HasExportBreadth(ConsumerSampleHost host, string storageRoot)
    {
        var featureDescriptors = host.Session.Queries.GetFeatureDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!featureDescriptors.TryGetValue("capability.export.scene-svg", out var svgFeature)
            || !svgFeature.IsAvailable
            || !featureDescriptors.TryGetValue("capability.export.scene-png", out var pngFeature)
            || !pngFeature.IsAvailable
            || !featureDescriptors.TryGetValue("capability.export.scene-jpeg", out var jpegFeature)
            || !jpegFeature.IsAvailable)
        {
            return false;
        }

        var exportRoot = Path.Combine(storageRoot, "capability-breadth-proof");
        Directory.CreateDirectory(exportRoot);
        var svgPath = Path.Combine(exportRoot, "consumer-sample-proof.svg");
        var pngPath = Path.Combine(exportRoot, "consumer-sample-proof.png");
        var jpegPath = Path.Combine(exportRoot, "consumer-sample-proof.jpg");

        return host.Session.Commands.TryExportSceneAsSvg(svgPath)
            && File.Exists(svgPath)
            && File.ReadAllText(svgPath).Contains("<svg", StringComparison.Ordinal)
            && host.Session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Png, pngPath)
            && File.Exists(pngPath)
            && HasImageSignature(File.ReadAllBytes(pngPath), GraphEditorSceneImageExportFormat.Png)
            && host.Session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Jpeg, jpegPath)
            && File.Exists(jpegPath)
            && HasImageSignature(File.ReadAllBytes(jpegPath), GraphEditorSceneImageExportFormat.Jpeg);
    }

    private static double MeasureStencilSearchMilliseconds(
        IGraphEditorSession session,
        string filter)
        => MeasureMilliseconds(() =>
        {
            var addNodeDescriptor = session.Queries.GetCommandDescriptors()
                .FirstOrDefault(descriptor => string.Equals(descriptor.Id, "nodes.add", StringComparison.Ordinal));
            var sections = session.Queries.GetNodeTemplateSnapshots()
                .Where(snapshot => MatchesStencilFilter(snapshot, filter))
                .GroupBy(snapshot => snapshot.Category, StringComparer.Ordinal)
                .OrderBy(section => section.Key, StringComparer.Ordinal)
                .Select(section => (Category: section.Key, Count: section.Count()))
                .ToArray();
            _ = addNodeDescriptor?.Id;
            _ = sections.Length;
        });

    private static double MeasureCommandSurfaceRefreshMilliseconds(IGraphEditorSession session)
        => MeasureMilliseconds(() =>
        {
            var actions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
                [
                    .. AsterGraphHostedActionFactory.CreateCommandActions(session),
                    .. AsterGraphHostedActionFactory.CreateCommandActions(
                        session,
                        ["composites.wrap-selection", "scopes.enter", "scopes.exit"]),
                    .. AsterGraphAuthoringToolActionFactory.CreateCommandSurfaceActions(session),
                ],
                AsterGraphCommandShortcutPolicy.Default);
            _ = actions.Count;
        });

    private static double MeasureNodeToolProjectionMilliseconds(
        IGraphEditorSession session,
        string nodeId)
        => MeasureMilliseconds(() =>
        {
            var selection = session.Queries.GetSelectionSnapshot();
            var actions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
                session,
                nodeId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            _ = actions.Count;
        });

    private static double MeasureEdgeToolProjectionMilliseconds(
        IGraphEditorSession session,
        string nodeId,
        string connectionId)
        => MeasureMilliseconds(() =>
        {
            var selection = session.Queries.GetSelectionSnapshot();
            var nodeActions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
                session,
                nodeId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            var connectionActions = AsterGraphAuthoringToolActionFactory.CreateConnectionActions(
                session,
                connectionId,
                selection.SelectedNodeIds,
                selection.PrimarySelectedNodeId);
            _ = nodeActions.Count + connectionActions.Count;
        });

    private static bool HasNodeQuickTools(Window window, ConsumerSampleHost host, string reviewNodeId)
    {
        var expansionButton = FindNamed<Button>(window, "PART_NodeToolToggleExpansionButton");
        var duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (expansionButton is null
            || duplicateButton is null
            || !expansionButton.IsEnabled
            || !duplicateButton.IsEnabled)
        {
            return false;
        }

        var initialExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .ExpansionState;
        expansionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var expansionChanged = host.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => snapshot.NodeId == reviewNodeId)
            .ExpansionState != initialExpansionState;

        host.SelectNode(reviewNodeId);
        FlushUi();
        duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (duplicateButton is null || !duplicateButton.IsEnabled)
        {
            return false;
        }

        var nodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
        duplicateButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return expansionChanged
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == nodeCount + 1;
    }

    private static bool HasEdgeQuickTools(Window window, ConsumerSampleHost host)
    {
        const string connectionId = "consumer-sample-connection-001";
        if (!host.Session.Commands.TryExecuteCommand(CreateCommand(
                "connections.note.set",
                ("connectionId", connectionId),
                ("text", "Proof note"),
                ("updateStatus", bool.TrueString))))
        {
            return false;
        }

        FlushUi();

        var clearNoteButton = FindNamed<Button>(window, $"PART_ConnectionToolClearNote_{connectionId}");
        var disconnectButton = FindNamed<Button>(window, $"PART_ConnectionToolDisconnect_{connectionId}");
        if (clearNoteButton is null
            || disconnectButton is null
            || !clearNoteButton.IsEnabled
            || !disconnectButton.IsEnabled)
        {
            return false;
        }

        clearNoteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        var noteCleared = string.IsNullOrWhiteSpace(host.Session.Queries.CreateDocumentSnapshot()
            .Connections
            .Single(connection => connection.Id == connectionId)
            .Presentation?.NoteText);

        host.Session.Commands.TryExecuteCommand(CreateCommand(
            "connections.note.set",
            ("connectionId", connectionId),
            ("text", "Disconnect proof"),
            ("updateStatus", bool.TrueString)));
        FlushUi();

        disconnectButton = FindNamed<Button>(window, $"PART_ConnectionToolDisconnect_{connectionId}");
        if (disconnectButton is null || !disconnectButton.IsEnabled)
        {
            return false;
        }

        disconnectButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();

        return noteCleared
            && host.Session.Queries.CreateDocumentSnapshot().Connections.Count == 0;
    }

    private static bool HasHostedAccessibilityBaselines(Window window)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var canvas = FindNamed<NodeCanvas>(window, "PART_NodeCanvas");
        var stencilSearchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        var inspector = FindNamed<GraphInspectorView>(window, "PART_InspectorSurface");
        var parameterSearchBox = FindNamed<TextBox>(window, "PART_ParameterSearchBox");

        return editorView is not null
            && string.Equals(AutomationProperties.GetName(editorView), "Graph editor host", StringComparison.Ordinal)
            && canvas is not null
            && string.Equals(AutomationProperties.GetName(canvas), "Graph canvas", StringComparison.Ordinal)
            && stencilSearchBox is not null
            && string.Equals(AutomationProperties.GetName(stencilSearchBox), "Stencil search", StringComparison.Ordinal)
            && paletteToggle is not null
            && string.Equals(AutomationProperties.GetName(paletteToggle), "Open command palette", StringComparison.Ordinal)
            && paletteSearchBox is not null
            && string.Equals(AutomationProperties.GetName(paletteSearchBox), "Command palette search", StringComparison.Ordinal)
            && inspector is not null
            && string.Equals(AutomationProperties.GetName(inspector), "Graph inspector", StringComparison.Ordinal)
            && parameterSearchBox is not null
            && string.Equals(AutomationProperties.GetName(parameterSearchBox), "Inspector parameter search", StringComparison.Ordinal);
    }

    private static bool HasHostedAccessibilityFocus(Window window)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var paletteChrome = FindNamed<Border>(window, "PART_CommandPaletteChrome");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        var stencilSearchBox = FindNamed<TextBox>(window, "PART_StencilSearchBox");
        var parameterSearchBox = FindNamed<TextBox>(window, "PART_ParameterSearchBox");
        return editorView is not null
            && paletteChrome is not null
            && paletteSearchBox is not null
            && stencilSearchBox is not null
            && parameterSearchBox is not null
            && HasCommandPaletteFocusRoundTrip(editorView, stencilSearchBox, paletteChrome, paletteSearchBox)
            && HasCommandPaletteFocusRoundTrip(editorView, parameterSearchBox, paletteChrome, paletteSearchBox);
    }

    private static bool HasHostedAutomationNavigation(
        Window window,
        ConsumerSampleHost host,
        string reviewNodeId)
    {
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var paletteSearchBox = FindNamed<TextBox>(window, "PART_CommandPaletteSearchBox");
        if (editorView is null
            || paletteToggle is null
            || paletteSearchBox is null)
        {
            return false;
        }

        var descriptors = host.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!HasEnabledCommand(descriptors, "selection.set")
            || !HasEnabledCommand(descriptors, "viewport.center-node")
            || !HasEnabledCommand(descriptors, "viewport.pan"))
        {
            return false;
        }

        var automationRunIds = new List<string>();
        var automationProgressCommands = new List<string>();
        var automationCompletedStates = new List<bool>();
        host.Session.Events.AutomationStarted += (_, args) => automationRunIds.Add(args.RunId);
        host.Session.Events.AutomationProgress += (_, args) => automationProgressCommands.Add(args.Step.CommandId);
        host.Session.Events.AutomationCompleted += (_, args) => automationCompletedStates.Add(args.Result.Succeeded);

        var initialViewport = host.Session.Queries.GetViewportSnapshot();
        var automationResult = host.Session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "consumer-sample-hosted-navigation",
            [
                CreateAutomationStep("select-review", "selection.set", ("nodeId", reviewNodeId), ("primaryNodeId", reviewNodeId), ("updateStatus", "false")),
                CreateAutomationStep("center-review", "viewport.center-node", ("nodeId", reviewNodeId)),
                CreateAutomationStep("pan-view", "viewport.pan", ("deltaX", "24"), ("deltaY", "18")),
            ]));
        FlushUi();

        var selection = host.Session.Queries.GetSelectionSnapshot();
        var viewport = host.Session.Queries.GetViewportSnapshot();
        var diagnostics = host.Session.Diagnostics.GetRecentDiagnostics(20)
            .Select(diagnostic => diagnostic.Code)
            .ToArray();

        return automationResult.Succeeded
            && automationResult.ExecutedStepCount == 3
            && automationResult.TotalStepCount == 3
            && automationRunIds.Count == 1
            && string.Equals(automationRunIds[0], "consumer-sample-hosted-navigation", StringComparison.Ordinal)
            && automationCompletedStates.Count == 1
            && automationCompletedStates[0]
            && automationProgressCommands.SequenceEqual(
                ["selection.set", "viewport.center-node", "viewport.pan"],
                StringComparer.Ordinal)
            && selection.PrimarySelectedNodeId == reviewNodeId
            && automationResult.Inspection.Selection.PrimarySelectedNodeId == reviewNodeId
            && HasViewportChanged(initialViewport, automationResult.Inspection.Viewport)
            && HasViewportChanged(initialViewport, viewport)
            && diagnostics.Contains("automation.run.started", StringComparer.Ordinal)
            && diagnostics.Contains("automation.run.completed", StringComparer.Ordinal);
    }

    private static bool HasHostedAuthoringAutomationDiagnostics(
        ConsumerSampleHost host,
        string reviewNodeId)
    {
        const string connectionId = "consumer-sample-connection-001";
        var automationResult = host.Session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "consumer-sample-authoring-diagnostics",
            [
                CreateAutomationStep("select-review", "selection.set", ("nodeId", reviewNodeId), ("primaryNodeId", reviewNodeId), ("updateStatus", "false")),
            ]));

        var labelEdited = host.Session.Commands.TrySetConnectionLabel(connectionId, "Automation branch", updateStatus: false);
        var noteEdited = host.Session.Commands.TrySetConnectionNoteText(connectionId, "Automation note", updateStatus: false);
        var selectedParameters = host.Session.Queries.GetSelectedNodeParameterSnapshots();
        var nodeParameters = host.Session.Queries.GetNodeParameterSnapshots(reviewNodeId);
        var nodeSurface = host.Session.Queries.GetNodeSurfaceSnapshots()
            .SingleOrDefault(snapshot => string.Equals(snapshot.NodeId, reviewNodeId, StringComparison.Ordinal));
        var selectionTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForSelection([reviewNodeId], reviewNodeId));
        var nodeTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForNode(reviewNodeId, [reviewNodeId], reviewNodeId));
        var connectionTools = host.Session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForConnection(connectionId, [reviewNodeId], reviewNodeId));
        var inspection = host.Session.Diagnostics.CaptureInspectionSnapshot();
        var connection = inspection.Document.Connections.SingleOrDefault(candidate =>
            string.Equals(candidate.Id, connectionId, StringComparison.Ordinal));

        return automationResult.Succeeded
            && automationResult.ExecutedStepCount == 1
            && labelEdited
            && noteEdited
            && inspection.Selection.PrimarySelectedNodeId == reviewNodeId
            && inspection.RecentDiagnostics.Any(diagnostic => diagnostic.Code == "automation.run.started")
            && inspection.RecentDiagnostics.Any(diagnostic => diagnostic.Code == "automation.run.completed")
            && selectedParameters.Any(snapshot =>
                snapshot.Definition.Key == "status"
                && !string.IsNullOrWhiteSpace(snapshot.Definition.Description)
                && !string.IsNullOrWhiteSpace(snapshot.GroupDisplayName)
                && !string.IsNullOrWhiteSpace(snapshot.ValueDisplayText)
                && snapshot.CanEdit
                && snapshot.IsValid)
            && selectedParameters.Any(snapshot =>
                snapshot.Definition.Key == "owner"
                && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(snapshot.Definition.Description))
            && nodeParameters.Any(snapshot =>
                snapshot.Definition.Key == "status"
                && string.Equals(snapshot.Definition.TemplateKey, ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey, StringComparison.Ordinal))
            && nodeSurface is not null
            && nodeSurface.ActiveTier.VisibleSectionKeys.Count > 0
            && ContainsExecutableTool(selectionTools, "selection-create-group")
            && ContainsExecutableTool(nodeTools, "node-inspect")
            && ContainsExecutableTool(nodeTools, "node-toggle-surface-expansion")
            && ContainsExecutableTool(connectionTools, "connection-disconnect")
            && ContainsExecutableTool(connectionTools, "connection-clear-note")
            && connection is not null
            && string.Equals(connection.Label, "Automation branch", StringComparison.Ordinal)
            && string.Equals(connection.Presentation?.NoteText, "Automation note", StringComparison.Ordinal);
    }

    private static bool HasHostedAccessibilityCommandSurface(Window window)
    {
        var saveButton = FindNamed<Button>(window, "PART_HeaderCommand_workspace.save");
        var paletteToggle = FindNamed<Button>(window, "PART_OpenCommandPaletteButton");
        var duplicateButton = FindNamed<Button>(window, "PART_NodeToolDuplicateButton");
        if (saveButton is null || paletteToggle is null || duplicateButton is null)
        {
            return false;
        }

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        var paletteSaveButton = FindNamed<Button>(window, "PART_CommandPaletteAction_workspace.save");
        var paletteDuplicateButton = FindNamed<Button>(window, "PART_CommandPaletteAction_node-duplicate");
        var result = string.Equals(AutomationProperties.GetName(saveButton), "Save Workspace", StringComparison.Ordinal)
            && string.Equals(AutomationProperties.GetName(duplicateButton), duplicateButton.Content?.ToString(), StringComparison.Ordinal)
            && paletteSaveButton is not null
            && string.Equals(AutomationProperties.GetName(paletteSaveButton), "Save Workspace", StringComparison.Ordinal)
            && paletteDuplicateButton is not null
            && string.Equals(AutomationProperties.GetName(paletteDuplicateButton), paletteDuplicateButton.Content?.ToString(), StringComparison.Ordinal);
        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        FlushUi();
        return result;
    }

    private static bool HasHostedAccessibilityAuthoringSurface(Window window)
    {
        const string connectionId = "consumer-sample-connection-001";
        var editorView = window.GetVisualDescendants().OfType<GraphEditorView>().FirstOrDefault();
        var hasAccessibleParameterMetadata = editorView?.Editor?.SelectedNodeParameters.Any(parameter =>
            !string.IsNullOrWhiteSpace(parameter.DisplayName)
            && (!string.IsNullOrWhiteSpace(parameter.HelpText)
                || !string.IsNullOrWhiteSpace(parameter.Description)
                || !string.IsNullOrWhiteSpace(parameter.ReadOnlyReason)
                || !string.IsNullOrWhiteSpace(parameter.ValueStateCaption))) == true;
        var labelEditor = FindNamed<TextBox>(window, $"PART_ConnectionToolLabelEditor_{connectionId}");
        var noteEditor = FindNamed<TextBox>(window, $"PART_ConnectionToolNoteEditor_{connectionId}");
        if (!hasAccessibleParameterMetadata
            || labelEditor is null
            || noteEditor is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(AutomationProperties.GetName(labelEditor))
            && AutomationProperties.GetName(labelEditor)!.EndsWith("label editor", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(AutomationProperties.GetName(noteEditor))
            && AutomationProperties.GetName(noteEditor)!.EndsWith("note editor", StringComparison.Ordinal);
    }

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string? Value)[] arguments)
        => new(
            commandId,
            arguments
                .Where(argument => argument.Value is not null)
                .Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value!))
                .ToArray());

    private static GraphEditorAutomationStep CreateAutomationStep(
        string stepId,
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            stepId,
            CreateCommand(
                commandId,
                arguments
                    .Select(static argument => (argument.Name, (string?)argument.Value))
                    .ToArray()));

    private static bool HasImageSignature(byte[] bytes, GraphEditorSceneImageExportFormat format)
        => bytes.Length > 3
        && format switch
        {
            GraphEditorSceneImageExportFormat.Png
                => bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47,
            GraphEditorSceneImageExportFormat.Jpeg
                => bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            _ => false,
        };

    private static bool MatchesStencilFilter(GraphEditorNodeTemplateSnapshot snapshot, string filter)
        => string.IsNullOrWhiteSpace(filter)
        || snapshot.Category.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Subtitle.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || snapshot.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static bool HasEnabledCommand(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> descriptors,
        string commandId)
        => descriptors.TryGetValue(commandId, out var descriptor)
        && descriptor.IsEnabled;

    private static bool ContainsExecutableTool(
        IReadOnlyList<GraphEditorToolDescriptorSnapshot> tools,
        string toolId)
        => tools.Any(tool => string.Equals(tool.Id, toolId, StringComparison.Ordinal) && tool.CanExecute);

    private static bool HasViewportChanged(
        GraphEditorViewportSnapshot before,
        GraphEditorViewportSnapshot after)
        => before.Zoom != after.Zoom
        || before.PanX != after.PanX
        || before.PanY != after.PanY;

    private static bool HasCommandPaletteFocusRoundTrip(
        GraphEditorView view,
        Control focusOrigin,
        Border paletteChrome,
        TextBox paletteSearchBox)
    {
        focusOrigin.Focus();
        FlushUi();

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Source = focusOrigin,
        });
        FlushUi();

        if (!paletteChrome.IsVisible
            || !ReferenceEquals(TopLevel.GetTopLevel(view)?.FocusManager?.GetFocusedElement(), paletteSearchBox))
        {
            return false;
        }

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.Escape,
        });
        FlushUi();

        return !paletteChrome.IsVisible
            && ReferenceEquals(TopLevel.GetTopLevel(view)?.FocusManager?.GetFocusedElement(), focusOrigin);
    }

    private static void InvokeViewKeyDown(GraphEditorView view, KeyEventArgs args)
    {
        var handler = typeof(GraphEditorView).GetMethod(
            "HandleKeyDown",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        handler?.Invoke(view, [view, args]);
    }

    private static void FlushUi()
        => Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

    private static bool HasMetadataProjection(IReadOnlyList<GraphEditorNodeParameterSnapshot> snapshots)
        => snapshots.Any(snapshot =>
            snapshot.Definition.Key == "status"
            && snapshot.Definition.ValueType == new PortTypeId("enum")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Enum
            && string.Equals(snapshot.Definition.DefaultValue?.ToString(), "draft", StringComparison.Ordinal)
            && HasAllowedOptions(
                snapshot.Definition.Constraints.AllowedOptions,
                ("draft", "Draft"),
                ("review", "In Review"),
                ("approved", "Approved")))
        && snapshots.Any(snapshot =>
            snapshot.Definition.Key == "owner"
            && snapshot.Definition.ValueType == new PortTypeId("string")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Text
            && string.Equals(snapshot.Definition.DefaultValue?.ToString(), "design-review", StringComparison.Ordinal))
        && snapshots.Any(snapshot =>
            snapshot.Definition.Key == "priority"
            && snapshot.Definition.ValueType == new PortTypeId("int")
            && snapshot.Definition.EditorKind == ParameterEditorKind.Number
            && Equals(snapshot.Definition.DefaultValue, 2)
            && snapshot.Definition.Constraints.Minimum == 1
            && snapshot.Definition.Constraints.Maximum == 5);

    private static bool HasAllowedOptions(
        IReadOnlyList<ParameterOptionDefinition> options,
        params (string Value, string Label)[] expected)
        => options.Count == expected.Length
        && options.Zip(expected, static (actual, wanted) =>
            string.Equals(actual.Value, wanted.Value, StringComparison.Ordinal)
            && string.Equals(actual.Label, wanted.Label, StringComparison.Ordinal))
            .All(static matches => matches);

    private static IReadOnlyList<ConsumerSampleProofParameterSnapshot> CreateParameterSnapshots(
        IReadOnlyList<GraphEditorNodeParameterSnapshot> snapshots)
        => snapshots
            .Select(static snapshot => new ConsumerSampleProofParameterSnapshot(
                Key: snapshot.Definition.Key,
                ValueType: snapshot.Definition.ValueType.Value,
                EditorKind: snapshot.Definition.EditorKind.ToString(),
                CurrentValue: snapshot.CurrentValue,
                DefaultValue: snapshot.Definition.DefaultValue,
                CanEdit: snapshot.CanEdit,
                IsValid: snapshot.IsValid,
                AllowedOptions: snapshot.Definition.Constraints.AllowedOptions
                    .Select(static option => new ConsumerSampleProofParameterOptionSnapshot(option.Value, option.Label))
                    .ToArray(),
                Minimum: snapshot.Definition.Constraints.Minimum,
                Maximum: snapshot.Definition.Constraints.Maximum))
            .ToArray();
}

public sealed record ConsumerSampleProofParameterSnapshot(
    string Key,
    string ValueType,
    string EditorKind,
    object? CurrentValue,
    object? DefaultValue,
    bool CanEdit,
    bool IsValid,
    IReadOnlyList<ConsumerSampleProofParameterOptionSnapshot> AllowedOptions,
    double? Minimum,
    double? Maximum);

public sealed record ConsumerSampleProofParameterOptionSnapshot(
    string Value,
    string Label);

public static class ConsumerSampleHeadlessEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized || Application.Current is not null)
        {
            _initialized = true;
            return;
        }

        AppBuilder.Configure<ConsumerSampleProofApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _initialized = true;
    }
}

internal sealed class ConsumerSampleProofApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
