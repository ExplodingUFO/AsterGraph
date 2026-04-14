using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginPackageStagingCoordinator
{
    public static GraphEditorPluginStageSnapshot Stage(GraphEditorPluginPackageStageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var candidate = request.Candidate;
        if (string.IsNullOrWhiteSpace(candidate.PackagePath))
        {
            throw new ArgumentException("Plugin candidate must expose a package path before it can be staged.", nameof(request));
        }

        var packagePath = Path.GetFullPath(candidate.PackagePath);
        var packageIdentity = GraphEditorPluginPackageIdentityResolver.Resolve(candidate);

        try
        {
            var trustRefusal = GraphEditorPluginPackageStagingEvaluator.TryGetTrustRefusal(candidate, packagePath, packageIdentity);
            if (trustRefusal is not null)
            {
                return trustRefusal;
            }

            var signatureRefusal = GraphEditorPluginPackageStagingEvaluator.TryGetSignatureRefusal(candidate, packagePath, packageIdentity);
            if (signatureRefusal is not null)
            {
                return signatureRefusal;
            }

            var inspection = AsterGraphPluginPackageArchiveInspector.Inspect(packagePath);
            if (string.IsNullOrWhiteSpace(inspection.DeclaredPluginAssemblyPath))
            {
                return new GraphEditorPluginStageSnapshot(
                    GraphEditorPluginStageOutcome.Refused,
                    packagePath,
                    packageIdentity,
                    pluginTypeName: null,
                    usedCache: false,
                    reasonCode: inspection.PayloadReasonCode ?? "stage.payload.not-declared",
                    reasonMessage: inspection.PayloadReasonMessage ?? "Package archive did not declare a staged plugin payload.");
            }

            var stagingRoot = string.IsNullOrWhiteSpace(request.StagingRootPath)
                ? GraphEditorStorageDefaults.GetPluginStagingPath()
                : request.StagingRootPath;
            var stagingDirectory = GraphEditorPluginPackageStagingPathPlanner.GetStagingDirectory(
                stagingRoot,
                packageIdentity,
                packagePath,
                candidate);
            var stagedEntries = GraphEditorPluginPackageArchivePlan.FromPackagePayload(packagePath, inspection.DeclaredPluginAssemblyPath);
            var mainAssemblyPath = GraphEditorPluginPackageStagingPathPlanner.GetDestinationPath(stagingDirectory, inspection.DeclaredPluginAssemblyPath);

            if (GraphEditorPluginPackageStagingCacheEvaluator.IsCacheHit(stagingDirectory, stagedEntries))
            {
                return new GraphEditorPluginStageSnapshot(
                    GraphEditorPluginStageOutcome.CacheHit,
                    packagePath,
                    packageIdentity,
                    stagingDirectory,
                    mainAssemblyPath,
                    inspection.DeclaredPluginTypeName,
                    usedCache: true);
            }

            GraphEditorPluginPackageArchiveStager.ExtractEntries(packagePath, stagingDirectory, stagedEntries);

            return new GraphEditorPluginStageSnapshot(
                GraphEditorPluginStageOutcome.Staged,
                packagePath,
                packageIdentity,
                stagingDirectory,
                mainAssemblyPath,
                inspection.DeclaredPluginTypeName,
                usedCache: false);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            return new GraphEditorPluginStageSnapshot(
                GraphEditorPluginStageOutcome.Failed,
                packagePath,
                packageIdentity,
                pluginTypeName: null,
                usedCache: false,
                reasonCode: "stage.failed",
                reasonMessage: $"Failed to stage package '{packagePath}': {exception.Message}");
        }
    }
}
