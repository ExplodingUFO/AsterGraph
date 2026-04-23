using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
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
    IReadOnlyList<ConsumerSampleProofParameterSnapshot> ParameterSnapshots,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
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

    public bool IsOk
        => HostMenuActionOk
        && PluginContributionOk
        && TrustTransparencyOk
        && WindowCompositionOk
        && AuthoringSurfaceOk
        && CapabilityBreadthOk;

    public IReadOnlyList<string> MetricLines =>
    [
        FormatMetric("startup_ms", StartupMs),
        FormatMetric("inspector_projection_ms", InspectorProjectionMs),
        FormatMetric("plugin_scan_ms", PluginScanMs),
        FormatMetric("command_latency_ms", CommandLatencyMs),
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
        try
        {
            capabilityWindow.Show();
            FlushUi();

            stencilSurfaceOk = HasStencilSurface(capabilityWindow, host);

            var reviewNodeId = host.GetFirstReviewNodeId();
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
            ParameterSnapshots: CreateParameterSnapshots(host.GetSelectedParameterSnapshots().ToArray()),
            StartupMs: startupMs,
            InspectorProjectionMs: inspectorProjectionMs,
            PluginScanMs: pluginScanMs,
            CommandLatencyMs: commandLatencyMs);
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

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string? Value)[] arguments)
        => new(
            commandId,
            arguments
                .Where(argument => argument.Value is not null)
                .Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value!))
                .ToArray());

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

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
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
