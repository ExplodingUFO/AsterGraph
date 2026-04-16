using AsterGraph.Editor.Plugins;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public IReadOnlyList<GraphEditorPluginLoadSnapshot> PluginLoadSnapshots
        => Editor.Session.Queries.GetPluginLoadSnapshots();

    public IReadOnlyList<string> PluginCandidateLines =>
        PluginCandidates.Count == 0
            ?
            [
                "当前未发现插件候选项。",
            ]
            : PluginCandidates.Select(candidate =>
                $"{candidate.Manifest.DisplayName} · {candidate.SourceKind} · {TrustDecisionText(candidate.TrustEvaluation.Decision)} · {candidate.TrustEvaluation.ReasonMessage ?? "No policy note."}")
                .ToArray();

    public IReadOnlyList<string> PluginLoadLines =>
        PluginLoadSnapshots.Count == 0
            ?
            [
                "当前未加载插件。",
            ]
            : PluginLoadSnapshots.Select(snapshot =>
                $"{snapshot.Manifest.DisplayName} · {snapshot.Status} · nodes {snapshot.Contributions.NodeDefinitionProviderCount} · menu {snapshot.Contributions.ContextMenuAugmentorCount} · l10n {snapshot.Contributions.LocalizationProviderCount} · presentation {snapshot.Contributions.NodePresentationProviderCount}")
                .ToArray();

    public IReadOnlyList<string> ConsumerPathLines =>
    [
        "HostSample = 最小 consumer path。",
        "Demo = 功能展示与边界说明壳层。",
        "Canonical host route：AsterGraphEditorFactory.Create(...) + AsterGraphAvaloniaViewFactory.Create(...).",
        "Runtime-only host route：AsterGraphEditorFactory.CreateSession(...).",
        "独立表面通过 AsterGraphCanvasViewFactory / AsterGraphInspectorViewFactory / AsterGraphMiniMapViewFactory 组合。",
    ];

    private static string TrustDecisionText(GraphEditorPluginTrustDecision decision)
        => decision switch
        {
            GraphEditorPluginTrustDecision.Allowed => "Allowed",
            GraphEditorPluginTrustDecision.Blocked => "Blocked",
            _ => decision.ToString(),
        };
}
