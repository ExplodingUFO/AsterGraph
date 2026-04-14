using AsterGraph.Editor.Diagnostics;
using System.Runtime.Loader;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginLoader
{
    internal const string PackageStagingRequiredMessage = "Package registrations must be staged through AsterGraphEditorFactory.StagePluginPackage(...) before loading.";

    public static GraphEditorPluginLoadResult Load(
        IReadOnlyList<GraphEditorPluginRegistration>? registrations,
        IGraphEditorPluginTrustPolicy? trustPolicy = null)
    {
        if (registrations is null || registrations.Count == 0)
        {
            return GraphEditorPluginLoadResult.Empty;
        }

        var aggregateBuilder = new GraphEditorPluginBuilder();
        var descriptors = new List<GraphEditorPluginDescriptor>();
        var snapshots = new List<GraphEditorPluginLoadSnapshot>();
        var diagnostics = new List<GraphEditorDiagnostic>();
        var loadContexts = new List<AssemblyLoadContext>();

        foreach (var registration in registrations)
        {
            GraphEditorPluginLoadCoordinator.TryLoadRegistration(
                registration,
                trustPolicy,
                aggregateBuilder,
                descriptors,
                snapshots,
                diagnostics,
                loadContexts);
        }

        return new GraphEditorPluginLoadResult(
            descriptors,
            aggregateBuilder.Build(),
            snapshots,
            diagnostics,
            loadContexts);
    }
}

internal sealed record GraphEditorPluginLoadResult(
    IReadOnlyList<GraphEditorPluginDescriptor> Descriptors,
    GraphEditorPluginContributionSet Contributions,
    IReadOnlyList<GraphEditorPluginLoadSnapshot> Snapshots,
    IReadOnlyList<GraphEditorDiagnostic> Diagnostics,
    IReadOnlyList<AssemblyLoadContext> LoadContexts)
{
    public static GraphEditorPluginLoadResult Empty { get; } = new([], GraphEditorPluginContributionSet.Empty, [], [], []);
}
