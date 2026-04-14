namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginPackageStagingService
{
    public static GraphEditorPluginStageSnapshot Stage(GraphEditorPluginPackageStageRequest request)
    {
        return GraphEditorPluginPackageStagingCoordinator.Stage(request);
    }
}
