using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;

namespace AsterGraph.ConsumerSample;

public sealed record ConsumerSampleProofResult(
    bool HostMenuActionOk,
    bool PluginContributionOk,
    bool ParameterEditingOk,
    bool WindowCompositionOk,
    bool TrustTransparencyOk,
    bool CommandSurfaceOk,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
{
    public bool IsOk
        => HostMenuActionOk
        && PluginContributionOk
        && ParameterEditingOk
        && WindowCompositionOk
        && TrustTransparencyOk
        && CommandSurfaceOk;

    public IReadOnlyList<string> MetricLines =>
    [
        FormatMetric("startup_ms", StartupMs),
        FormatMetric("inspector_projection_ms", InspectorProjectionMs),
        FormatMetric("plugin_scan_ms", PluginScanMs),
        FormatMetric("command_latency_ms", CommandLatencyMs),
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

        window.Show();

        host.SelectNode(host.GetFirstReviewNodeId());
        var inspectorProjectionMs = MeasureMilliseconds(() => host.GetSelectedParameterSnapshots().ToArray());
        var pluginScanMs = MeasureMilliseconds(() => host.PluginCandidates.ToArray());

        var commandBaseline = host.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
        host.Session.Commands.AddNode(ConsumerSampleHost.ReviewDefinitionId, new GraphPoint(880, 220));
        var undoAction = AsterGraphHostedActionFactory.CreateCommandActions(host.Session, ["history.undo"])
            .Single(action => string.Equals(action.Id, "history.undo", StringComparison.Ordinal));
        var commandLatencyMs = MeasureMilliseconds(() => undoAction.TryExecute());
        var commandSurfaceOk = undoAction.CanExecute
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == commandBaseline;

        var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var hostMenuActionOk = host.AddHostReviewNode()
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == initialSnapshot.Nodes.Count + 1;

        var afterHostSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var pluginContributionOk = host.HasPluginNodeDefinition()
            && host.HasPluginMenuContribution()
            && host.AddPluginAuditNode()
            && host.PluginLoadSnapshots.Any(snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin")
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == afterHostSnapshot.Nodes.Count + 1;

        host.SelectNode(host.GetFirstReviewNodeId());
        var parameterEditingOk = host.TrySetSelectedOwner("release-owner")
            && host.ApproveSelection()
            && host.GetSelectedParameterSnapshots().Any(snapshot =>
                snapshot.Definition.Key == "status"
                && string.Equals(snapshot.CurrentValue?.ToString(), "approved", StringComparison.Ordinal))
            && host.GetSelectedParameterSnapshots().Any(snapshot =>
                snapshot.Definition.Key == "owner"
                && string.Equals(snapshot.CurrentValue?.ToString(), "release-owner", StringComparison.Ordinal));

        var trustTransparencyOk =
            host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
            && host.ExportPluginAllowlist()
            && File.Exists(host.PluginAllowlistExchangePath)
            && host.BlockPluginCandidate("consumer.sample.audit-plugin")
            && host.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase))
            && host.ImportPluginAllowlist()
            && host.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.TrustReason.Contains("allowlist", StringComparison.OrdinalIgnoreCase));

        var windowCompositionOk =
            FindNamed<Menu>(window, "PART_MainMenu") is not null
            && FindNamed<Button>(window, "PART_AddReviewNodeButton") is not null
            && FindNamed<Button>(window, "PART_AddPluginNodeButton") is not null
            && FindNamed<Button>(window, "PART_ApproveSelectionButton") is not null
            && FindNamed<ItemsControl>(window, "PART_PluginCandidateItems") is not null
            && FindNamed<ItemsControl>(window, "PART_AllowlistItems") is not null
            && FindNamed<TextBlock>(window, "PART_TrustBoundaryText") is not null;

        window.Close();

        return new ConsumerSampleProofResult(
            HostMenuActionOk: hostMenuActionOk,
            PluginContributionOk: pluginContributionOk,
            ParameterEditingOk: parameterEditingOk,
            WindowCompositionOk: windowCompositionOk,
            TrustTransparencyOk: trustTransparencyOk,
            CommandSurfaceOk: commandSurfaceOk,
            StartupMs: startupMs,
            InspectorProjectionMs: inspectorProjectionMs,
            PluginScanMs: pluginScanMs,
            CommandLatencyMs: commandLatencyMs);
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}

public static class ConsumerSampleHeadlessEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
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
