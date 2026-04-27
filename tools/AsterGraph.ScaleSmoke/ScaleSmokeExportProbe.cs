using System.Diagnostics;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeExportProbeResult(
    bool SvgExportOk,
    bool PngExportOk,
    bool JpegExportOk,
    bool ReloadOk,
    bool ProgressOk,
    bool CancelOk,
    bool FullScopeOk,
    bool SelectionScopeOk,
    ScaleSmokeExportMetrics Metrics)
{
    public bool IsOk
        => SvgExportOk
        && PngExportOk
        && JpegExportOk
        && ReloadOk
        && ProgressOk
        && CancelOk
        && FullScopeOk
        && SelectionScopeOk;

    public string ToMarker(string tierId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tierId);

        return string.Join(
            ':',
            [
                $"SCALE_EXPORT_PROBE_OK:{tierId}",
                IsOk.ToString(),
                $"svg={SvgExportOk}",
                $"png={PngExportOk}",
                $"jpeg={JpegExportOk}",
                $"reload={ReloadOk}",
                $"progress={ProgressOk}",
                $"cancel={CancelOk}",
                $"scope={FullScopeOk}",
                $"selection={SelectionScopeOk}",
            ]);
    }
}

public static class ScaleSmokeExportProbe
{
    public static ScaleSmokeExportProbeResult Run(IGraphEditorSession session, string storageRoot, ScaleSmokeTier? tier = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRoot);

        Directory.CreateDirectory(storageRoot);
        var progressEvents = new List<GraphEditorSceneImageExportProgressSnapshot>();
        var imageScale = tier?.EnforceExportBudgets == false ? 0.2d : 0.5d;
        var imageExportOptions = new GraphEditorSceneImageExportOptions
        {
            Scale = imageScale,
            Quality = 84,
            BackgroundHex = "#101820",
            Progress = new RecordingProgress(progressEvents),
        };

        var svgPath = Path.Combine(storageRoot, "scale-export.svg");
        var pngPath = Path.Combine(storageRoot, "scale-export.png");
        var jpegPath = Path.Combine(storageRoot, "scale-export.jpg");

        var svgExportMs = MeasureMilliseconds(() => session.Commands.TryExportSceneAsSvg(svgPath));
        var svgExportOk = File.Exists(svgPath)
            && File.ReadAllText(svgPath).Contains("<svg", StringComparison.Ordinal);

        var pngExportMs = MeasureMilliseconds(() =>
            session.Commands.TryExportSceneAsImage(
                GraphEditorSceneImageExportFormat.Png,
                pngPath,
                imageExportOptions with { Scope = GraphEditorSceneImageExportScope.FullScene }));
        var pngExportOk = File.Exists(pngPath)
            && HasImageSignature(File.ReadAllBytes(pngPath), GraphEditorSceneImageExportFormat.Png);
        var fullScopeOk = pngExportOk;

        var jpegExportMs = MeasureMilliseconds(() =>
            session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Jpeg, jpegPath, imageExportOptions));
        var jpegExportOk = File.Exists(jpegPath)
            && HasImageSignature(File.ReadAllBytes(jpegPath), GraphEditorSceneImageExportFormat.Jpeg);

        var progressOk = HasCompleteProgress(progressEvents);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelPath = Path.Combine(storageRoot, "scale-export-cancel.png");
        var cancelOk = !session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            cancelPath,
            imageExportOptions with { CancellationToken = cancellation.Token })
            && !File.Exists(cancelPath);

        var initialSnapshot = session.Queries.CreateDocumentSnapshot();
        var selectedNodeIds = initialSnapshot.Nodes
            .Take(2)
            .Select(node => node.Id)
            .ToList();
        if (selectedNodeIds.Count < 2)
        {
            throw new InvalidOperationException("Export scope probe requires at least two nodes.");
        }

        session.Commands.SetSelection(selectedNodeIds, selectedNodeIds[0], updateStatus: false);
        var selectedScopePath = Path.Combine(storageRoot, "scale-export-selection.png");
        var selectionScopeOk = session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            selectedScopePath,
            imageExportOptions with { Scope = GraphEditorSceneImageExportScope.SelectedNodes })
            && File.Exists(selectedScopePath)
            && HasImageSignature(File.ReadAllBytes(selectedScopePath), GraphEditorSceneImageExportFormat.Png);

        var initialNodeCount = initialSnapshot.Nodes.Count;
        var firstDefinitionId = initialSnapshot.Nodes
            .Select(node => node.DefinitionId)
            .FirstOrDefault(definitionId => definitionId is not null)
            ?? throw new InvalidOperationException("Export probe requires a definition-backed node.");

        session.Commands.SaveWorkspace();
        session.Commands.AddNode(firstDefinitionId, new GraphPoint(6400, 6400));
        var mutatedNodeCount = session.Queries.CreateDocumentSnapshot().Nodes.Count;
        var reloadMs = MeasureMilliseconds(() => session.Commands.LoadWorkspace());
        var reloadedSnapshot = session.Queries.CreateDocumentSnapshot();
        var reloadOk = mutatedNodeCount == initialNodeCount + 1
            && reloadedSnapshot.Nodes.Count == initialNodeCount;

        return new ScaleSmokeExportProbeResult(
            SvgExportOk: svgExportOk,
            PngExportOk: pngExportOk,
            JpegExportOk: jpegExportOk,
            ReloadOk: reloadOk,
            ProgressOk: progressOk,
            CancelOk: cancelOk,
            FullScopeOk: fullScopeOk,
            SelectionScopeOk: selectionScopeOk,
            Metrics: new ScaleSmokeExportMetrics(
                SvgExportMs: svgExportMs,
                PngExportMs: pngExportMs,
                JpegExportMs: jpegExportMs,
                ReloadMs: reloadMs));
    }

    private static long MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

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

    private static bool HasCompleteProgress(IReadOnlyList<GraphEditorSceneImageExportProgressSnapshot> events)
        => events.Any(progress => progress.Stage == "preparing" && progress.Fraction == 0d)
        && events.Any(progress => progress.Stage == "rasterizing" && progress.Fraction >= 0.7d)
        && events.Any(progress => progress.Stage == "written" && progress.Fraction == 1d);

    private sealed class RecordingProgress : IProgress<GraphEditorSceneImageExportProgressSnapshot>
    {
        private readonly List<GraphEditorSceneImageExportProgressSnapshot> _events;

        public RecordingProgress(List<GraphEditorSceneImageExportProgressSnapshot> events)
        {
            _events = events;
        }

        public void Report(GraphEditorSceneImageExportProgressSnapshot value)
            => _events.Add(value);
    }
}
