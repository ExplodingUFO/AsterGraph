using System.Diagnostics;
using System.Globalization;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Demo.ViewModels;

namespace AsterGraph.Demo;

public sealed record DemoProofResult(
    bool TrustTransparencyOk,
    bool ShellWorkflowOk,
    bool CommandSurfaceOk,
    bool ProgressiveNodeSurfaceOk,
    bool AdaptiveGroupBoundsOk,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
{
    public bool IsOk => TrustTransparencyOk && ShellWorkflowOk && CommandSurfaceOk && ProgressiveNodeSurfaceOk && AdaptiveGroupBoundsOk;

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

public static class DemoProof
{
    public static DemoProofResult Run(string? storageRootPath = null)
    {
        var storageRoot = string.IsNullOrWhiteSpace(storageRootPath)
            ? Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Proof", Guid.NewGuid().ToString("N"))
            : storageRootPath;
        Directory.CreateDirectory(storageRoot);

        MainWindowViewModel? viewModel = null;
        var startupMs = MeasureMilliseconds(() =>
            viewModel = new MainWindowViewModel(new MainWindowShellOptions(
                StorageRootPath: storageRoot,
                EnableStatePersistence: true,
                RestoreLastWorkspaceOnStartup: false)));
        var shell = viewModel ?? throw new InvalidOperationException("Demo view model was not created.");

        shell.Editor.SelectSingleNode(shell.Editor.Nodes[0], updateStatus: false);
        var inspectorProjectionMs = MeasureMilliseconds(() => shell.Editor.Session.Queries.GetSelectedNodeParameterSnapshots().ToArray());
        var pluginScanMs = MeasureMilliseconds(() => shell.PluginCandidates.ToArray());

        var nodeCountBeforeUndo = shell.Editor.Nodes.Count;
        shell.Editor.Session.Commands.AddNode(shell.Editor.NodeTemplates[0].Definition.Id, new GraphPoint(920, 260));
        var undoAction = AsterGraphHostedActionFactory.CreateCommandActions(shell.Editor.Session, ["history.undo"])
            .Single(action => string.Equals(action.Id, "history.undo", StringComparison.Ordinal));
        var commandLatencyMs = MeasureMilliseconds(() => undoAction.TryExecute());
        var commandSurfaceOk = undoAction.CanExecute && shell.Editor.Nodes.Count == nodeCountBeforeUndo;
        var lightingNode = shell.Editor.FindNode("light")
            ?? throw new InvalidOperationException("Demo proof requires the lighting node.");
        shell.Editor.SelectSingleNode(lightingNode, updateStatus: false);
        var pulsePort = lightingNode.Inputs.Single(port => string.Equals(port.Id, "pulse", StringComparison.Ordinal));
        var rimMaskPort = lightingNode.Inputs.Single(port => string.Equals(port.Id, "rimMask", StringComparison.Ordinal));
        var lightSurface = shell.Editor.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => string.Equals(snapshot.NodeId, "light", StringComparison.Ordinal));
        var terrainGroup = shell.Editor.Session.Queries.GetNodeGroups()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        var terrainGroupSnapshot = shell.Editor.GetNodeGroupSnapshots()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        var progressiveNodeSurfaceOk =
            lightSurface.ExpansionState == GraphNodeExpansionState.Expanded
            && terrainGroup is not null
            && terrainGroup.NodeIds.Count == 2
            && shell.Editor.HasIncomingConnection(lightingNode, pulsePort)
            && !shell.Editor.HasIncomingConnection(lightingNode, rimMaskPort)
            && shell.Editor.ResolveInlineParameter(lightingNode, pulsePort) is not null
            && shell.Editor.ResolveInlineParameter(lightingNode, rimMaskPort)?.Key == "rimMask";
        var noiseNode = shell.Editor.FindNode("noise")
            ?? throw new InvalidOperationException("Demo proof requires the noise node.");
        var adaptivePadding = new GraphPadding(52, 40, 44, 36);
        var adaptiveBoundsOk =
            terrainGroupSnapshot is not null
            && shell.Editor.TrySetNodeGroupExtraPadding("terrain-authoring", adaptivePadding, updateStatus: false)
            && shell.Editor.TrySetNodeWidth(noiseNode, noiseNode.Width + 48d, updateStatus: false);
        var terrainGroupAfterResize = shell.Editor.GetNodeGroupSnapshots()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        var adaptiveGroupBoundsOk =
            adaptiveBoundsOk
            && terrainGroupAfterResize is not null
            && terrainGroupAfterResize.ExtraPadding == adaptivePadding
            && terrainGroupAfterResize.Position.X <= shell.Editor.FindNode("gradient")!.X
            && terrainGroupAfterResize.Position.Y <= Math.Min(shell.Editor.FindNode("gradient")!.Y, shell.Editor.FindNode("noise")!.Y)
            && terrainGroupAfterResize.Size.Width > terrainGroupSnapshot!.Size.Width
            && terrainGroupAfterResize.Size.Height >= terrainGroupSnapshot.Size.Height;

        var exportPath = Path.Combine(storageRoot, "plugin-allowlist-proof.json");
        var trustTransparencyOk =
            shell.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.TrustPluginCandidate("aster.demo.plugin.blocked")
            && shell.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.ExportPluginAllowlist(exportPath)
            && File.Exists(exportPath)
            && shell.BlockPluginCandidate("aster.demo.plugin.blocked")
            && shell.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.ImportPluginAllowlist(exportPath)
            && shell.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.PluginId == "aster.demo.plugin.blocked");

        var workspacePath = Path.Combine(storageRoot, "demo-proof-workspace.json");
        var baselineNodeCount = shell.Editor.Nodes.Count;
        shell.SaveWorkspaceAs(workspacePath);
        shell.Editor.Session.Commands.AddNode(shell.Editor.NodeTemplates[0].Definition.Id, new GraphPoint(1040, 320));
        var shellWorkflowOk =
            shell.TryOpenWorkspacePath(workspacePath)
            && shell.Editor.Nodes.Count == baselineNodeCount
            && shell.RecentWorkspacePaths.Any(path => string.Equals(path, workspacePath, StringComparison.OrdinalIgnoreCase))
            && shell.ShellWorkflowLines.Any(line => line.Contains("workspace", StringComparison.OrdinalIgnoreCase) || line.Contains("工作区", StringComparison.Ordinal));

        return new DemoProofResult(
            trustTransparencyOk,
            shellWorkflowOk,
            commandSurfaceOk,
            progressiveNodeSurfaceOk,
            adaptiveGroupBoundsOk,
            startupMs,
            inspectorProjectionMs,
            pluginScanMs,
            commandLatencyMs);
    }

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }
}
