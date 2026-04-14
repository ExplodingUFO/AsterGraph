using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginLoadExecutor
{
    public static void LoadPlugin(
        IGraphEditorPlugin plugin,
        GraphEditorPluginBuilder aggregateBuilder,
        ICollection<GraphEditorPluginDescriptor> descriptors,
        ICollection<GraphEditorPluginLoadSnapshot> snapshots,
        ICollection<GraphEditorDiagnostic> diagnostics,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility,
        GraphEditorPluginTrustEvaluation trustEvaluation,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        GraphEditorPluginLoadSourceKind sourceKind,
        string source,
        string? resolvedPluginTypeName,
        string diagnosticSource,
        string? requestedPluginTypeName = null,
        string? packagePath = null,
        GraphEditorPluginStageSnapshot? stage = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(aggregateBuilder);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(compatibility);
        ArgumentNullException.ThrowIfNull(trustEvaluation);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticSource);

        var descriptor = plugin.Descriptor
            ?? throw new InvalidOperationException($"Plugin from {diagnosticSource} returned a null descriptor.");
        var builder = new GraphEditorPluginBuilder();
        plugin.Register(builder);
        var contributions = builder.Build();
        aggregateBuilder.Merge(contributions);
        descriptors.Add(descriptor);
        snapshots.Add(new GraphEditorPluginLoadSnapshot(
            sourceKind,
            source,
            GraphEditorPluginLoadStatus.Loaded,
            GraphEditorPluginLoadDiagnosticsFactory.CreateContributionSummary(contributions),
            manifest,
            compatibility,
            trustEvaluation,
            provenanceEvidence,
            activationAttempted: true,
            descriptor: descriptor,
            requestedPluginTypeName: requestedPluginTypeName,
            resolvedPluginTypeName: resolvedPluginTypeName,
            failureMessage: null,
            packagePath: packagePath,
            stage: stage));
        diagnostics.Add(new GraphEditorDiagnostic(
            "plugin.load.succeeded",
            "plugin.load",
            $"Loaded plugin '{descriptor.Id}' from {diagnosticSource}.",
            GraphEditorDiagnosticSeverity.Info));
    }

    public static GraphEditorPluginLoadSnapshot CreateBlockedSnapshot(
        GraphEditorPluginLoadSourceKind sourceKind,
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility,
        GraphEditorPluginTrustEvaluation trustEvaluation,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        GraphEditorPluginRegistration registration,
        GraphEditorPluginLoadStatus status,
        bool activationAttempted,
        string? requestedPluginTypeName,
        string? failureMessage = null)
    {
        return new GraphEditorPluginLoadSnapshot(
            sourceKind,
            source,
            status,
            GraphEditorPluginContributionSummarySnapshot.Empty,
            manifest,
            compatibility,
            trustEvaluation,
            provenanceEvidence,
            activationAttempted,
            descriptor: null,
            requestedPluginTypeName: requestedPluginTypeName,
            packagePath: registration.PackagePath,
            stage: registration.Stage,
            failureMessage: failureMessage);
    }
}
