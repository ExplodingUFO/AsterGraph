using System.Reflection;

namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginLoadTypeResolver
{
    public static IReadOnlyList<Type> ResolvePluginTypes(Assembly assembly, string? pluginTypeName)
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

    public static IGraphEditorPlugin CreatePlugin(Type pluginType)
        => Activator.CreateInstance(pluginType) as IGraphEditorPlugin
            ?? throw new InvalidOperationException($"Plugin type '{pluginType.FullName}' could not be instantiated.");

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
}
