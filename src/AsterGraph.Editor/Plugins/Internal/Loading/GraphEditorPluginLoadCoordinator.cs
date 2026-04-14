using AsterGraph.Editor.Diagnostics;
using System.Runtime.Loader;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginLoadCoordinator
{
    public static void TryLoadRegistration(
        GraphEditorPluginRegistration registration,
        IGraphEditorPluginTrustPolicy? trustPolicy,
        GraphEditorPluginBuilder aggregateBuilder,
        ICollection<GraphEditorPluginDescriptor> descriptors,
        ICollection<GraphEditorPluginLoadSnapshot> snapshots,
        ICollection<GraphEditorDiagnostic> diagnostics,
        ICollection<AssemblyLoadContext> loadContexts)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(aggregateBuilder);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(loadContexts);

        var sourceKind = registration.IsPackageRegistration
            ? GraphEditorPluginLoadSourceKind.Package
            : registration.IsAssemblyRegistration
                ? GraphEditorPluginLoadSourceKind.Assembly
                : GraphEditorPluginLoadSourceKind.Direct;
        var source = registration.PackagePath
            ?? registration.AssemblyPath
            ?? registration.Plugin?.GetType().FullName
            ?? "direct plugin registration";
        var diagnosticSource = registration.Stage is not null
            ? $"staged package '{source}'"
            : sourceKind switch
            {
                GraphEditorPluginLoadSourceKind.Package => $"package '{source}'",
                GraphEditorPluginLoadSourceKind.Assembly => $"assembly '{source}'",
                _ => $"plugin '{source}'",
            };
        var preload = AsterGraphPluginPreloadEvaluator.EvaluateRegistration(registration, trustPolicy);
        var manifest = preload.Manifest;
        var compatibility = preload.Compatibility;
        var trustEvaluation = preload.TrustEvaluation;
        var provenanceEvidence = preload.ProvenanceEvidence;
        var activationAttempted = false;

        try
        {
            if (preload.IsTrustBlocked)
            {
                diagnostics.Add(GraphEditorPluginLoadDiagnosticsFactory.CreateBlockedDiagnostic(diagnosticSource, manifest, trustEvaluation));
                snapshots.Add(GraphEditorPluginLoadExecutor.CreateBlockedSnapshot(
                    sourceKind,
                    source,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    registration,
                    GraphEditorPluginLoadStatus.Blocked,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName));
                return;
            }

            if (preload.IsCompatibilityBlocked)
            {
                diagnostics.Add(GraphEditorPluginLoadDiagnosticsFactory.CreateIncompatibleDiagnostic(diagnosticSource, manifest, compatibility));
                snapshots.Add(GraphEditorPluginLoadExecutor.CreateBlockedSnapshot(
                    sourceKind,
                    source,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    registration,
                    GraphEditorPluginLoadStatus.Blocked,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName));
                return;
            }

            if (registration.IsDirectRegistration)
            {
                var plugin = registration.Plugin!;
                activationAttempted = true;
                GraphEditorPluginLoadExecutor.LoadPlugin(
                    plugin,
                    aggregateBuilder,
                    descriptors,
                    snapshots,
                    diagnostics,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    sourceKind,
                    source,
                    plugin.GetType().FullName,
                    diagnosticSource,
                    requestedPluginTypeName: null,
                    packagePath: registration.PackagePath,
                    stage: registration.Stage);
                return;
            }

            if (registration.IsPackageRegistration && !registration.IsAssemblyRegistration)
            {
                diagnostics.Add(GraphEditorPluginLoadDiagnosticsFactory.CreatePackageStagingRequiredDiagnostic(source));
                snapshots.Add(GraphEditorPluginLoadExecutor.CreateBlockedSnapshot(
                    sourceKind,
                    source,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    registration,
                    GraphEditorPluginLoadStatus.Failed,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName,
                    failureMessage: AsterGraphPluginLoader.PackageStagingRequiredMessage));
                return;
            }

            if (!registration.IsAssemblyRegistration)
            {
                throw new InvalidOperationException("Plugin registration did not specify a plugin instance or assembly path.");
            }

            activationAttempted = true;
            var loadContext = new AsterGraphPluginAssemblyLoadContext(registration.AssemblyPath!);
            loadContexts.Add(loadContext);
            var assembly = loadContext.LoadFromAssemblyPath(registration.AssemblyPath!);
            var pluginTypes = GraphEditorPluginLoadTypeResolver.ResolvePluginTypes(assembly, registration.PluginTypeName);

            foreach (var pluginType in pluginTypes)
            {
                GraphEditorPluginLoadExecutor.LoadPlugin(
                    GraphEditorPluginLoadTypeResolver.CreatePlugin(pluginType),
                    aggregateBuilder,
                    descriptors,
                    snapshots,
                    diagnostics,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    sourceKind,
                    source,
                    pluginType.FullName,
                    diagnosticSource,
                    registration.PluginTypeName,
                    registration.PackagePath,
                    registration.Stage);
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(GraphEditorPluginLoadDiagnosticsFactory.CreateFailureDiagnostic(diagnosticSource, exception));
            snapshots.Add(new GraphEditorPluginLoadSnapshot(
                sourceKind,
                source,
                GraphEditorPluginLoadStatus.Failed,
                GraphEditorPluginContributionSummarySnapshot.Empty,
                manifest,
                compatibility,
                trustEvaluation,
                provenanceEvidence,
                activationAttempted,
                requestedPluginTypeName: registration.PluginTypeName,
                failureMessage: exception.Message,
                packagePath: registration.PackagePath,
                stage: registration.Stage));
        }
    }
}
