using System.Reflection;
using System.Runtime.Loader;

namespace AsterGraph.Editor.Plugins.Internal;

internal sealed class AsterGraphPluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public AsterGraphPluginAssemblyLoadContext(string mainAssemblyPath)
        : base($"AsterGraph.Plugin:{Path.GetFileNameWithoutExtension(mainAssemblyPath)}", isCollectible: false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mainAssemblyPath);
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        ArgumentNullException.ThrowIfNull(assemblyName);

        if (ShouldUseDefaultContext(assemblyName))
        {
            return TryLoadFromDefaultContext(assemblyName);
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return TryLoadFromDefaultContext(assemblyName);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unmanagedDllName);

        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is null
            ? 0
            : LoadUnmanagedDllFromPath(libraryPath);
    }

    private static bool ShouldUseDefaultContext(AssemblyName assemblyName)
        => assemblyName.Name?.StartsWith("AsterGraph.", StringComparison.Ordinal) == true;

    private static Assembly? TryLoadFromDefaultContext(AssemblyName assemblyName)
    {
        try
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
        }
        catch
        {
            return null;
        }
    }
}
