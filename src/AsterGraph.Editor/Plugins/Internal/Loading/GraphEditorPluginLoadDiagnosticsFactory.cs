using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginLoadDiagnosticsFactory
{
    public static GraphEditorPluginContributionSummarySnapshot CreateContributionSummary(GraphEditorPluginContributionSet contributions)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        return new GraphEditorPluginContributionSummarySnapshot(
            contributions.NodeDefinitionProviders.Count,
            contributions.CommandContributors.Count,
            contributions.NodePresentationProviders.Count,
            contributions.LocalizationProviders.Count);
    }

    public static GraphEditorDiagnostic CreateFailureDiagnostic(string source, Exception exception)
        => new(
            "plugin.load.failed",
            "plugin.load",
            $"Failed to load plugin from {source}: {exception.Message}",
            GraphEditorDiagnosticSeverity.Error,
            exception);

    public static GraphEditorDiagnostic CreatePackageStagingRequiredDiagnostic(string source)
        => new(
            "plugin.load.package-staging-required",
            "plugin.load",
            $"Refused plugin package registration from package '{source}': {AsterGraphPluginLoader.PackageStagingRequiredMessage}",
            GraphEditorDiagnosticSeverity.Warning);

    public static GraphEditorDiagnostic CreateBlockedDiagnostic(
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginTrustEvaluation trustEvaluation)
        => new(
            "plugin.load.blocked",
            "plugin.trust",
            $"Blocked plugin '{manifest.Id}' from {source}: {trustEvaluation.ReasonMessage ?? trustEvaluation.ReasonCode ?? "Policy blocked the load."}",
            GraphEditorDiagnosticSeverity.Warning);

    public static GraphEditorDiagnostic CreateIncompatibleDiagnostic(
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility)
        => new(
            "plugin.load.incompatible",
            "plugin.compatibility",
            $"Refused plugin '{manifest.Id}' from {source}: {compatibility.ReasonMessage ?? compatibility.ReasonCode ?? "Manifest compatibility was rejected for the current host."}",
            GraphEditorDiagnosticSeverity.Warning);
}
