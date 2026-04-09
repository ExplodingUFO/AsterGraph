using System.IO;
using System.Reflection;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginPreloadEvaluator
{
    public static AsterGraphPluginPreloadEvaluation EvaluateRegistration(
        GraphEditorPluginRegistration registration,
        IGraphEditorPluginTrustPolicy? trustPolicy)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var manifest = ResolveManifest(registration);
        var compatibility = EvaluateCompatibility(manifest);
        var trustEvaluation = EvaluateTrustPolicy(trustPolicy, registration, manifest);

        return new AsterGraphPluginPreloadEvaluation(
            manifest,
            compatibility,
            trustEvaluation);
    }

    public static GraphEditorPluginManifest ResolveManifest(GraphEditorPluginRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        if (registration.Manifest is not null)
        {
            return registration.Manifest;
        }

        if (registration.IsDirectRegistration)
        {
            var descriptor = registration.Plugin?.Descriptor
                ?? throw new InvalidOperationException("Direct plugin registration did not expose a descriptor.");
            var source = registration.Plugin.GetType().FullName ?? "direct plugin registration";
            return descriptor.ToManifest(new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                source));
        }

        if (registration.IsAssemblyRegistration && registration.AssemblyPath is not null)
        {
            return CreateAssemblyFallbackManifest(registration.AssemblyPath, registration.PluginTypeName);
        }

        return new GraphEditorPluginManifest(
            "unknown-plugin",
            "unknown-plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.Manifest,
                "unknown-plugin"));
    }

    public static GraphEditorPluginTrustEvaluation EvaluateTrustPolicy(
        IGraphEditorPluginTrustPolicy? trustPolicy,
        GraphEditorPluginRegistration registration,
        GraphEditorPluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(manifest);

        if (trustPolicy is null)
        {
            return GraphEditorPluginTrustEvaluation.ImplicitAllow();
        }

        return trustPolicy.Evaluate(new GraphEditorPluginTrustPolicyContext(registration, manifest))
            ?? throw new InvalidOperationException("Plugin trust policy returned a null evaluation.");
    }

    public static GraphEditorPluginCompatibilityEvaluation EvaluateCompatibility(GraphEditorPluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var compatibility = manifest.Compatibility;
        if (!HasDeclaredCompatibility(compatibility))
        {
            return new GraphEditorPluginCompatibilityEvaluation(
                GraphEditorPluginCompatibilityStatus.Unknown,
                "compatibility.not-declared",
                "Plugin manifest did not declare compatibility metadata.");
        }

        var currentVersion = GetCurrentAsterGraphVersion();
        var minimumVersion = ParseVersion(compatibility.MinimumAsterGraphVersion);
        var maximumVersion = ParseVersion(compatibility.MaximumAsterGraphVersion);

        if (minimumVersion is not null && currentVersion is not null && currentVersion < minimumVersion)
        {
            return new GraphEditorPluginCompatibilityEvaluation(
                GraphEditorPluginCompatibilityStatus.Incompatible,
                "compatibility.astergraph.minimum-version",
                $"Current AsterGraph version '{currentVersion}' is below the declared minimum '{minimumVersion}'.");
        }

        if (maximumVersion is not null && currentVersion is not null && currentVersion > maximumVersion)
        {
            return new GraphEditorPluginCompatibilityEvaluation(
                GraphEditorPluginCompatibilityStatus.Incompatible,
                "compatibility.astergraph.maximum-version",
                $"Current AsterGraph version '{currentVersion}' is above the declared maximum '{maximumVersion}'.");
        }

        return new GraphEditorPluginCompatibilityEvaluation(
            GraphEditorPluginCompatibilityStatus.Compatible,
            "compatibility.accepted",
            "Manifest compatibility metadata was accepted for the current host.");
    }

    private static bool HasDeclaredCompatibility(GraphEditorPluginCompatibilityManifest compatibility)
        => !string.IsNullOrWhiteSpace(compatibility.MinimumAsterGraphVersion)
            || !string.IsNullOrWhiteSpace(compatibility.MaximumAsterGraphVersion)
            || !string.IsNullOrWhiteSpace(compatibility.TargetFramework)
            || !string.IsNullOrWhiteSpace(compatibility.RuntimeSurface);

    private static Version? GetCurrentAsterGraphVersion()
    {
        var assembly = typeof(GraphEditorPluginManifest).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return ParseVersion(informational) ?? assembly.GetName().Version;
    }

    private static Version? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        var separatorIndex = normalized.IndexOfAny(['-', '+']);
        if (separatorIndex >= 0)
        {
            normalized = normalized[..separatorIndex];
        }

        return Version.TryParse(normalized, out var version)
            ? version
            : null;
    }

    private static GraphEditorPluginManifest CreateAssemblyFallbackManifest(string assemblyPath, string? pluginTypeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        var fullPath = Path.GetFullPath(assemblyPath);
        var fileName = Path.GetFileNameWithoutExtension(fullPath);
        var id = string.IsNullOrWhiteSpace(pluginTypeName) ? fileName : pluginTypeName.Trim();
        var displayName = string.IsNullOrWhiteSpace(pluginTypeName)
            ? fileName
            : pluginTypeName.Split('.').Last();
        string? version = null;

        try
        {
            var assemblyName = AssemblyName.GetAssemblyName(fullPath);
            version = assemblyName.Version?.ToString();
            if (string.IsNullOrWhiteSpace(pluginTypeName) && !string.IsNullOrWhiteSpace(assemblyName.Name))
            {
                id = assemblyName.Name!;
                displayName = assemblyName.Name!;
            }
        }
        catch
        {
            // Missing or unreadable files still need a stable manifest shell for pre-load inspection.
        }

        return new GraphEditorPluginManifest(
            id,
            displayName,
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.AssemblyPath,
                fullPath),
            version: version);
    }
}
