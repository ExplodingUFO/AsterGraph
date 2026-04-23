using System.Linq;
using System.Text.Json;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.ConsumerSample;
using Xunit;

namespace AsterGraph.ConsumerSample.Tests;

public sealed class ConsumerSampleProofTests
{
    [Fact]
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

    [Fact]
    public void ConsumerSampleProof_Run_EmitsGreenMarkers()
    {
        var result = ConsumerSampleProof.Run();

        Assert.True(result.IsOk);
        Assert.True(result.HostMenuActionOk);
        Assert.True(result.PluginContributionOk);
        Assert.True(result.ParameterEditingOk);
        Assert.True(result.TrustTransparencyOk);
        Assert.True(result.CommandSurfaceOk);
        Assert.True(result.StartupMs >= 0);
        Assert.True(result.InspectorProjectionMs >= 0);
        Assert.True(result.PluginScanMs >= 0);
        Assert.True(result.CommandLatencyMs >= 0);
        Assert.Contains(result.MetricLines, line => line.Contains("startup_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_latency_ms", StringComparison.Ordinal));
    }

    [Fact]
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

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("ConsumerSample.Avalonia", root.GetProperty("route").GetString());
        Assert.False(string.IsNullOrWhiteSpace(packageVersion));
        Assert.Equal($"v{packageVersion}", publicTag);
        Assert.True(
            DateTimeOffset.TryParse(generatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _),
            "Support bundle generatedAtUtc should be a parseable round-trip UTC timestamp.");
        var proofLines = root.GetProperty("proofLines").EnumerateArray().Select(static item => item.GetString()).ToArray();
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_HOST_ACTION_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PLUGIN_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_PARAMETER_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_WINDOW_OK:True");
        Assert.Contains(proofLines, line => line == "CONSUMER_SAMPLE_TRUST_OK:True");
        Assert.Contains(proofLines, line => line == "COMMAND_SURFACE_OK:True");
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:startup_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:inspector_projection_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:plugin_scan_ms=", StringComparison.Ordinal));
        Assert.Contains(proofLines, line => line is not null && line.StartsWith("HOST_NATIVE_METRIC:command_latency_ms=", StringComparison.Ordinal));
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
    }

    [Fact]
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
        Assert.Contains("SUPPORT_BUNDLE_PATH:", lines, StringComparison.Ordinal);
        Assert.True(File.Exists(bundlePath));
    }

    [Fact]
    public void Program_SupportBundleOption_RequiresExplicitPath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Program.Main(["--support-bundle", "--support-note", "cli-note"]));

        Assert.Contains("--support-bundle requires a file path.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
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

    [Fact]
    public void ConsumerSampleWindow_RendersHostedEditorAndIntegrationPanels()
    {
        ConsumerSampleHeadlessEnvironment.EnsureInitialized();
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

    [Fact]
    public void ConsumerSampleWindow_CommandRailUsesSharedSessionActionPath()
    {
        ConsumerSampleHeadlessEnvironment.EnsureInitialized();
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

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
}
