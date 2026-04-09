using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginLoader
{
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
            TryLoadRegistration(registration, trustPolicy, aggregateBuilder, descriptors, snapshots, diagnostics, loadContexts);
        }

        return new GraphEditorPluginLoadResult(
            descriptors,
            aggregateBuilder.Build(),
            snapshots,
            diagnostics,
            loadContexts);
    }

    private static void TryLoadRegistration(
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
        var source = registration.AssemblyPath
            ?? registration.PackagePath
            ?? registration.Plugin?.GetType().FullName
            ?? "direct plugin registration";
        var diagnosticSource = sourceKind switch
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
                diagnostics.Add(CreateBlockedDiagnostic(diagnosticSource, manifest, trustEvaluation));
                snapshots.Add(new GraphEditorPluginLoadSnapshot(
                    sourceKind,
                    source,
                    GraphEditorPluginLoadStatus.Blocked,
                    GraphEditorPluginContributionSummarySnapshot.Empty,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName,
                    packagePath: registration.PackagePath));
                return;
            }

            if (preload.IsCompatibilityBlocked)
            {
                diagnostics.Add(CreateIncompatibleDiagnostic(diagnosticSource, manifest, compatibility));
                snapshots.Add(new GraphEditorPluginLoadSnapshot(
                    sourceKind,
                    source,
                    GraphEditorPluginLoadStatus.Blocked,
                    GraphEditorPluginContributionSummarySnapshot.Empty,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName,
                    packagePath: registration.PackagePath));
                return;
            }

            if (registration.IsDirectRegistration)
            {
                var plugin = registration.Plugin!;
                activationAttempted = true;
                LoadPlugin(
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
                    diagnosticSource);
                return;
            }

            if (registration.IsPackageRegistration)
            {
                const string message = "Package registrations are not supported until verified staging is implemented.";
                diagnostics.Add(CreatePackageRegistrationNotSupportedDiagnostic(source));
                snapshots.Add(new GraphEditorPluginLoadSnapshot(
                    sourceKind,
                    source,
                    GraphEditorPluginLoadStatus.Failed,
                    GraphEditorPluginContributionSummarySnapshot.Empty,
                    manifest,
                    compatibility,
                    trustEvaluation,
                    provenanceEvidence,
                    activationAttempted: false,
                    requestedPluginTypeName: registration.PluginTypeName,
                    failureMessage: message,
                    packagePath: registration.PackagePath));
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
            var pluginTypes = ResolvePluginTypes(assembly, registration.PluginTypeName);

            foreach (var pluginType in pluginTypes)
            {
                LoadPlugin(
                    CreatePlugin(pluginType),
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
                    registration.PluginTypeName);
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(CreateFailureDiagnostic(diagnosticSource, exception));
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
                packagePath: registration.PackagePath));
        }
    }

    private static IReadOnlyList<Type> ResolvePluginTypes(Assembly assembly, string? pluginTypeName)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (pluginTypeName is not null)
        {
            return [ResolvePluginType(assembly, pluginTypeName)];
        }

        var pluginTypes = GetLoadableTypes(assembly)
            .Where(CanCreatePlugin)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();
        if (pluginTypes.Count == 0)
        {
            throw new InvalidOperationException($"No {nameof(IGraphEditorPlugin)} implementations were found in '{assembly.Location}'.");
        }

        return pluginTypes;
    }

    private static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types
                .Where(type => type is not null)
                .Cast<Type>()
                .ToList();
        }
    }

    private static Type ResolvePluginType(Assembly assembly, string pluginTypeName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginTypeName);

        var pluginType = assembly.GetType(pluginTypeName, throwOnError: false, ignoreCase: false);
        if (pluginType is null)
        {
            throw new InvalidOperationException($"Plugin type '{pluginTypeName}' was not found in '{assembly.Location}'.");
        }

        if (!CanCreatePlugin(pluginType))
        {
            throw new InvalidOperationException($"Plugin type '{pluginTypeName}' cannot be activated as an {nameof(IGraphEditorPlugin)}.");
        }

        return pluginType;
    }

    private static bool CanCreatePlugin(Type pluginType)
        => pluginType is
        {
            IsAbstract: false,
            IsInterface: false,
        }
        && typeof(IGraphEditorPlugin).IsAssignableFrom(pluginType)
        && pluginType.GetConstructor(Type.EmptyTypes) is not null;

    private static IGraphEditorPlugin CreatePlugin(Type pluginType)
        => Activator.CreateInstance(pluginType) as IGraphEditorPlugin
            ?? throw new InvalidOperationException($"Plugin type '{pluginType.FullName}' could not be instantiated.");

    private static void LoadPlugin(
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
        string? requestedPluginTypeName = null)
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
            CreateContributionSummary(contributions),
            manifest,
            compatibility,
            trustEvaluation,
            provenanceEvidence,
            activationAttempted: true,
            descriptor,
            requestedPluginTypeName,
            resolvedPluginTypeName));
        diagnostics.Add(new GraphEditorDiagnostic(
            "plugin.load.succeeded",
            "plugin.load",
            $"Loaded plugin '{descriptor.Id}' from {diagnosticSource}.",
            GraphEditorDiagnosticSeverity.Info));
    }

    private static GraphEditorPluginContributionSummarySnapshot CreateContributionSummary(GraphEditorPluginContributionSet contributions)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        return new GraphEditorPluginContributionSummarySnapshot(
            contributions.NodeDefinitionProviders.Count,
            contributions.ContextMenuAugmentors.Count,
            contributions.NodePresentationProviders.Count,
            contributions.LocalizationProviders.Count);
    }

    private static GraphEditorDiagnostic CreateFailureDiagnostic(string source, Exception exception)
        => new(
            "plugin.load.failed",
            "plugin.load",
            $"Failed to load plugin from {source}: {exception.Message}",
            GraphEditorDiagnosticSeverity.Error,
            exception);

    private static GraphEditorDiagnostic CreatePackageRegistrationNotSupportedDiagnostic(string source)
        => new(
            "plugin.load.package-registration-not-supported",
            "plugin.load",
            $"Refused plugin package registration from package '{source}': Package registrations are not supported until verified staging is implemented.",
            GraphEditorDiagnosticSeverity.Warning);

    private static GraphEditorDiagnostic CreateBlockedDiagnostic(
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginTrustEvaluation trustEvaluation)
        => new(
            "plugin.load.blocked",
            "plugin.trust",
            $"Blocked plugin '{manifest.Id}' from {source}: {trustEvaluation.ReasonMessage ?? trustEvaluation.ReasonCode ?? "Policy blocked the load."}",
            GraphEditorDiagnosticSeverity.Warning);

    private static GraphEditorDiagnostic CreateIncompatibleDiagnostic(
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility)
        => new(
            "plugin.load.incompatible",
            "plugin.compatibility",
            $"Refused plugin '{manifest.Id}' from {source}: {compatibility.ReasonMessage ?? compatibility.ReasonCode ?? "Manifest compatibility was rejected for the current host."}",
            GraphEditorDiagnosticSeverity.Warning);
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
