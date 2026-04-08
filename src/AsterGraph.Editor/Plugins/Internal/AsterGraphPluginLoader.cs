using System.Reflection;
using System.Runtime.Loader;
using AsterGraph.Editor.Diagnostics;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class AsterGraphPluginLoader
{
    public static GraphEditorPluginLoadResult Load(IReadOnlyList<GraphEditorPluginRegistration>? registrations)
    {
        if (registrations is null || registrations.Count == 0)
        {
            return GraphEditorPluginLoadResult.Empty;
        }

        var aggregateBuilder = new GraphEditorPluginBuilder();
        var descriptors = new List<GraphEditorPluginDescriptor>();
        var diagnostics = new List<GraphEditorDiagnostic>();
        var loadContexts = new List<AssemblyLoadContext>();

        foreach (var registration in registrations)
        {
            TryLoadRegistration(registration, aggregateBuilder, descriptors, diagnostics, loadContexts);
        }

        return new GraphEditorPluginLoadResult(
            descriptors,
            aggregateBuilder.Build(),
            diagnostics,
            loadContexts);
    }

    private static void TryLoadRegistration(
        GraphEditorPluginRegistration registration,
        GraphEditorPluginBuilder aggregateBuilder,
        ICollection<GraphEditorPluginDescriptor> descriptors,
        ICollection<GraphEditorDiagnostic> diagnostics,
        ICollection<AssemblyLoadContext> loadContexts)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(aggregateBuilder);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(loadContexts);

        var source = registration.IsAssemblyRegistration
            ? $"assembly '{registration.AssemblyPath}'"
            : registration.Plugin?.GetType().FullName ?? "direct plugin registration";

        try
        {
            if (registration.IsDirectRegistration)
            {
                LoadPlugin(
                    registration.Plugin!,
                    aggregateBuilder,
                    descriptors,
                    diagnostics,
                    source);
                return;
            }

            if (!registration.IsAssemblyRegistration)
            {
                throw new InvalidOperationException("Plugin registration did not specify a plugin instance or assembly path.");
            }

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
                    diagnostics,
                    source);
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(CreateFailureDiagnostic(source, exception));
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
        ICollection<GraphEditorDiagnostic> diagnostics,
        string source)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(aggregateBuilder);
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var descriptor = plugin.Descriptor
            ?? throw new InvalidOperationException($"Plugin from {source} returned a null descriptor.");
        var builder = new GraphEditorPluginBuilder();
        plugin.Register(builder);
        aggregateBuilder.Merge(builder.Build());
        descriptors.Add(descriptor);
        diagnostics.Add(new GraphEditorDiagnostic(
            "plugin.load.succeeded",
            "plugin.load",
            $"Loaded plugin '{descriptor.Id}' from {source}.",
            GraphEditorDiagnosticSeverity.Info));
    }

    private static GraphEditorDiagnostic CreateFailureDiagnostic(string source, Exception exception)
        => new(
            "plugin.load.failed",
            "plugin.load",
            $"Failed to load plugin from {source}: {exception.Message}",
            GraphEditorDiagnosticSeverity.Error,
            exception);
}

internal sealed record GraphEditorPluginLoadResult(
    IReadOnlyList<GraphEditorPluginDescriptor> Descriptors,
    GraphEditorPluginContributionSet Contributions,
    IReadOnlyList<GraphEditorDiagnostic> Diagnostics,
    IReadOnlyList<AssemblyLoadContext> LoadContexts)
{
    public static GraphEditorPluginLoadResult Empty { get; } = new([], GraphEditorPluginContributionSet.Empty, [], []);
}
