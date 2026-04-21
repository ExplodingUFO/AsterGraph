using AsterGraph.Editor.Plugins;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public IReadOnlyList<GraphEditorPluginLoadSnapshot> PluginLoadSnapshots
        => Session.Queries.GetPluginLoadSnapshots();

    public IReadOnlyList<string> PluginCandidateLines =>
        PluginCandidates.Count == 0
            ?
            [
                T("当前未发现插件候选项。", "No plugin candidates discovered."),
            ]
            : PluginCandidateEntries.Select(entry =>
                $"{entry.DisplayName} · {entry.Source} · {entry.Version} · {entry.TrustLine}")
                .ToArray();

    public IReadOnlyList<string> PluginLoadLines =>
        PluginLoadSnapshots.Count == 0
            ?
            [
                T("当前未加载插件。", "No plugins loaded."),
            ]
            : PluginLoadSnapshots.Select(snapshot =>
                $"{snapshot.Manifest.DisplayName} · {snapshot.Status} · nodes {snapshot.Contributions.NodeDefinitionProviderCount} · commands {snapshot.Contributions.CommandContributorCount} · l10n {snapshot.Contributions.LocalizationProviderCount} · presentation {snapshot.Contributions.NodePresentationProviderCount}")
                .ToArray();

    public IReadOnlyList<string> ConsumerPathLines =>
    [
        T("HostSample = 最小 consumer path。", "HostSample = minimal consumer path."),
        T("Demo = 功能展示与边界说明壳层。", "Demo = showcase shell for capability and boundary proof."),
        "Hosted-UI composition: AsterGraphEditorFactory.Create(...) + AsterGraphAvaloniaViewFactory.Create(...).",
        "Shared runtime owner: Session (IGraphEditorSession).",
        "Runtime-only host route: AsterGraphEditorFactory.CreateSession(...).",
        T("独立表面通过 ", "Standalone surfaces compose through ")
        + "AsterGraphCanvasViewFactory / AsterGraphInspectorViewFactory / AsterGraphMiniMapViewFactory"
        + T(" 组合。", "."),
    ];

    private static string TrustDecisionText(GraphEditorPluginTrustDecision decision)
        => decision switch
        {
            GraphEditorPluginTrustDecision.Allowed => "Allowed",
            GraphEditorPluginTrustDecision.Blocked => "Blocked",
            _ => decision.ToString(),
        };
}
