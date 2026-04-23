using System.Linq;
using System.Text.Json;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.ConsumerSample;
using Xunit;

namespace AsterGraph.ConsumerSample.Tests;

public sealed class ConsumerSampleProofTests
{
    [AvaloniaFact]
    public void ConsumerSampleHost_CombinesHostNodesPluginContributionsAndParameterEditing()
    {
        using var host = ConsumerSampleHost.Create();

        var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.True(host.PluginLoadSnapshots.Count > 0);
        Assert.Contains(host.PluginLoadSnapshots, snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin");

        host.AddHostReviewNode();
        host.AddPluginAuditNode();

        var updatedSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.Equal(initialSnapshot.Nodes.Count + 2, updatedSnapshot.Nodes.Count);

        var reviewNodeId = host.GetFirstReviewNodeId();
        host.Session.Commands.SetSelection([reviewNodeId], reviewNodeId, updateStatus: false);

        var parameterSnapshots = host.Session.Queries.GetSelectedNodeParameterSnapshots();
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "status");
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "owner");

        Assert.True(host.ApproveSelection());

        var approvedStatus = host.Session.Queries.GetSelectedNodeParameterSnapshots()
            .Single(snapshot => snapshot.Definition.Key == "status");
        Assert.Equal("approved", approvedStatus.CurrentValue);
    }

    [AvaloniaFact]
    public void ConsumerSampleProof_Run_EmitsGreenMarkers()
    {
        var result = ConsumerSampleProof.Run();

        Assert.True(result.IsOk);
        Assert.True(result.CapabilityBreadthOk);
        Assert.True(result.NodeSideAuthoringOk);
        Assert.True(result.HostMenuActionOk);
        Assert.True(result.PluginContributionOk);
        Assert.True(result.ParameterProjectionOk);
        Assert.True(result.MetadataProjectionOk);
        Assert.True(result.TrustTransparencyOk);
        Assert.True(result.CommandSurfaceOk);
        Assert.True(result.StencilSurfaceOk);
        Assert.True(result.ExportBreadthOk);
        Assert.True(result.NodeQuickToolsOk);
        Assert.True(result.EdgeQuickToolsOk);
        Assert.True(result.StartupMs >= 0);
        Assert.True(result.InspectorProjectionMs >= 0);
        Assert.True(result.PluginScanMs >= 0);
        Assert.True(result.CommandLatencyMs >= 0);
        Assert.True(result.StencilSearchMs >= 0);
        Assert.True(result.CommandSurfaceRefreshMs >= 0);
        Assert.True(result.NodeToolProjectionMs >= 0);
        Assert.True(result.EdgeToolProjectionMs >= 0);
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_STENCIL_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_OK:True");
        Assert.Contains(result.ProofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:True");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_OK:True");
        Assert.Contains(result.MetricLines, line => line.Contains("startup_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_latency_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("stencil_search_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_surface_refresh_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("node_tool_projection_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("edge_tool_projection_ms", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_MetadataProjectionMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: false,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: true,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1);

        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "AUTHORING_SURFACE_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProofResult_CapabilityBreadthMarker_FailsOverallProofStatus()
    {
        var result = new ConsumerSampleProofResult(
            HostMenuActionOk: true,
            PluginContributionOk: true,
            ParameterProjectionOk: true,
            MetadataProjectionOk: true,
            NodeSideAuthoringOk: true,
            WindowCompositionOk: true,
            TrustTransparencyOk: true,
            CommandSurfaceOk: true,
            StencilSurfaceOk: true,
            ExportBreadthOk: false,
            NodeQuickToolsOk: true,
            EdgeQuickToolsOk: true,
            ParameterSnapshots: [],
            StartupMs: 1,
            InspectorProjectionMs: 1,
            PluginScanMs: 1,
            CommandLatencyMs: 1,
            StencilSearchMs: 1,
            CommandSurfaceRefreshMs: 1,
            NodeToolProjectionMs: 1,
            EdgeToolProjectionMs: 1);

        Assert.False(result.CapabilityBreadthOk);
        Assert.False(result.IsOk);
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CAPABILITY_BREADTH_OK:False");
        Assert.Contains(result.ProofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:False");
        Assert.Contains(result.ProofLines, line => line == "CONSUMER_SAMPLE_OK:False");
    }

    [AvaloniaFact]
    public void ConsumerSampleProof_CanWriteSupportBundleContract()
    {
        var result = ConsumerSampleProof.Run();
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var bundlePath = Path.Combine(tempRoot, "consumer-support-bundle.json");

        ConsumerSampleSupportBundle.WriteProofBundle(
            bundlePath,
            result,
            "dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle consumer-support-bundle.json",
            "repro-note");

        Assert.True(File.Exists(bundlePath));

        using var document = JsonDocument.Parse(File.ReadAllText(bundlePath));
        var root = document.RootElement;
        var packageVersion = root.GetProperty("packageVersion").GetString();
        var publicTag = root.GetProperty("publicTag").GetString();
        var generatedAtUtc = root.GetProperty("generatedAtUtc").GetString();
        var environment = root.GetProperty("environment");
        var reproduction = root.GetProperty("reproduction");
        var persistenceStatus = root.GetProperty("persistenceStatus").GetString();
        var parameterSnapshots = root.GetProperty("parameterSnapshots").EnumerateArray().ToArray();

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("ConsumerSample.Avalonia", root.GetProperty("route").GetString());
        Assert.False(string.IsNullOrWhiteSpace(packageVersion));
        Assert.Equal($"v{packageVersion}", publicTag);
        Assert.Equal("written", persistenceStatus);
        Assert.Equal(3, parameterSnapshots.Length);
        Assert.True(
            DateTimeOffset.TryParse(generatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _),
            "Support bundle generatedAtUtc should be a parseable round-trip UTC timestamp.");
        var proofLines = root.GetProperty("proofLines").EnumerateArray().Select(static item => item.GetString()).ToArray();
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_HOST_ACTION_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PLUGIN_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_PARAMETER_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_METADATA_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_NODE_SIDE_EDITOR_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_COMMAND_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_STENCIL_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_EXPORT_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_NODE_QUICK_TOOLS_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_EDGE_QUICK_TOOLS_OK:True");
        Assert.Contains(proofLines, line => line == "CAPABILITY_BREADTH_OK:True");
        Assert.Contains(proofLines, line => line == "WIDENED_SURFACE_PERFORMANCE_OK:True");
        Assert.Contains(proofLines, line => line == "AUTHORING_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PARAMETER_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_WINDOW_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_TRUST_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:startup_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:inspector_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:plugin_scan_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_latency_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:stencil_search_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_surface_refresh_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:node_tool_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:edge_tool_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_OK:True");
        Assert.Equal(
            ["frameworkDescription", "osArchitecture", "osDescription", "processArchitecture"],
            environment.EnumerateObject().Select(static property => property.Name).OrderBy(static name => name).ToArray());
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("frameworkDescription").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("osDescription").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("osArchitecture").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(environment.GetProperty("processArchitecture").GetString()));
        Assert.Equal("repro-note", reproduction.GetProperty("note").GetString());
        Assert.Contains("--support-bundle", reproduction.GetProperty("command").GetString(), StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(reproduction.GetProperty("workingDirectory").GetString()));

        var statusSnapshot = parameterSnapshots.Single(snapshot => snapshot.GetProperty("key").GetString() == "status");
        Assert.Equal("enum", statusSnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Enum", statusSnapshot.GetProperty("editorKind").GetString());
        Assert.Equal("approved", statusSnapshot.GetProperty("currentValue").GetString());
        Assert.Equal("draft", statusSnapshot.GetProperty("defaultValue").GetString());
        Assert.True(statusSnapshot.GetProperty("canEdit").GetBoolean());
        Assert.True(statusSnapshot.GetProperty("isValid").GetBoolean());
        var allowedOptions = statusSnapshot.GetProperty("allowedOptions").EnumerateArray().ToArray();
        Assert.Equal(
            ["draft", "review", "approved"],
            allowedOptions.Select(option => option.GetProperty("value").GetString()!).ToArray());
        Assert.Equal(
            ["Draft", "In Review", "Approved"],
            allowedOptions.Select(option => option.GetProperty("label").GetString()!).ToArray());

        var ownerSnapshot = parameterSnapshots.Single(snapshot => snapshot.GetProperty("key").GetString() == "owner");
        Assert.Equal("string", ownerSnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Text", ownerSnapshot.GetProperty("editorKind").GetString());
        Assert.Equal("release-owner", ownerSnapshot.GetProperty("currentValue").GetString());
        Assert.Equal("design-review", ownerSnapshot.GetProperty("defaultValue").GetString());
        Assert.True(ownerSnapshot.GetProperty("canEdit").GetBoolean());
        Assert.True(ownerSnapshot.GetProperty("isValid").GetBoolean());
        Assert.Empty(ownerSnapshot.GetProperty("allowedOptions").EnumerateArray());

        var prioritySnapshot = parameterSnapshots.Single(snapshot => snapshot.GetProperty("key").GetString() == "priority");
        Assert.Equal("int", prioritySnapshot.GetProperty("valueType").GetString());
        Assert.Equal("Number", prioritySnapshot.GetProperty("editorKind").GetString());
        Assert.Equal(2, prioritySnapshot.GetProperty("currentValue").GetInt32());
        Assert.Equal(2, prioritySnapshot.GetProperty("defaultValue").GetInt32());
        Assert.Equal(1, prioritySnapshot.GetProperty("minimum").GetDouble());
        Assert.Equal(5, prioritySnapshot.GetProperty("maximum").GetDouble());
        Assert.Empty(prioritySnapshot.GetProperty("allowedOptions").EnumerateArray());
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_EmitsBundleMarkersAndFile()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var bundlePath = Path.Combine(tempRoot, "program-support-bundle.json");
        var originalWriter = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Program.Main(["--support-bundle", bundlePath, "--support-note", "cli-note"]);
        }
        finally
        {
            Console.SetOut(originalWriter);
        }

        var lines = output.ToString();
        Assert.Contains("SUPPORT_BUNDLE_OK:True", lines, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:True", lines, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:", lines, StringComparison.Ordinal);
        Assert.True(File.Exists(bundlePath));
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_EmitsFalsePersistenceMarkerWhenWriteFails()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var originalWriter = Console.Out;
        using var output = new StringWriter();

        try
        {
            Console.SetOut(output);
            Assert.ThrowsAny<Exception>(() => Program.Main(["--support-bundle", tempRoot, "--support-note", "cli-note"]));
        }
        finally
        {
            Console.SetOut(originalWriter);
        }

        var lines = output.ToString();
        Assert.Contains("SUPPORT_BUNDLE_PERSISTENCE_OK:False", lines, StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK:True", lines, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void Program_SupportBundleOption_RequiresExplicitPath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Program.Main(["--support-bundle", "--support-note", "cli-note"]));

        Assert.Contains("--support-bundle requires a file path.", exception.Message, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void ConsumerSampleHost_PersistsExportsAndImportsPluginAllowlistDecisions()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ConsumerSample.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);

        using var host = ConsumerSampleHost.Create(storageRoot);

        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsAllowed);

        var exportPath = Path.Combine(storageRoot, "consumer-allowlist-export.json");
        Assert.True(host.ExportPluginAllowlist(exportPath));
        Assert.True(File.Exists(exportPath));

        Assert.True(host.BlockPluginCandidate("consumer.sample.audit-plugin"));
        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsBlocked);

        Assert.True(host.ImportPluginAllowlist(exportPath));
        Assert.Contains(host.PluginCandidateEntries, entry => entry.PluginId == "consumer.sample.audit-plugin" && entry.IsAllowed);
        Assert.Contains(host.PluginAllowlistLines, line => line.Contains("Allowlist", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_RendersHostedEditorAndIntegrationPanels()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            Assert.NotNull(FindNamed<Menu>(window, "PART_MainMenu"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddReviewNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddPluginNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_ApproveSelectionButton"));
            Assert.NotNull(FindNamed<GraphEditorView>(window, "PART_EditorView"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_ParameterItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_PluginCandidateItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_PluginSnapshotItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_AllowlistItems"));
            Assert.NotNull(FindNamed<TextBlock>(window, "PART_TrustBoundaryText"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_CommandRailUsesSharedSessionActionPath()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            var initialNodeCount = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
            var addReview = Assert.IsType<Button>(FindNamed<Button>(window, "PART_AddReviewNodeButton"));

            addReview.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(initialNodeCount + 1, host.Session.Queries.CreateDocumentSnapshot().Nodes.Count);

            var undo = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_history.undo"));
            undo.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(initialNodeCount, host.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_HostRailProjectsSharedAuthoringToolActions()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            var createGroup = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_selection-create-group"));
            var toggleExpansion = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_node-toggle-surface-expansion"));
            var disconnect = Assert.IsType<Button>(FindNamed<Button>(window, "PART_Action_connection-disconnect"));
            var reviewNodeId = host.GetFirstReviewNodeId();
            var initialExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .ExpansionState;

            Assert.True(createGroup.IsEnabled);
            Assert.True(toggleExpansion.IsEnabled);
            Assert.True(disconnect.IsEnabled);

            toggleExpansion.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            disconnect.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            var updatedExpansionState = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .ExpansionState;

            Assert.NotEqual(initialExpansionState, updatedExpansionState);
            Assert.Empty(host.Session.Queries.CreateDocumentSnapshot().Connections);
        }
        finally
        {
            window.Close();
        }
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
}
