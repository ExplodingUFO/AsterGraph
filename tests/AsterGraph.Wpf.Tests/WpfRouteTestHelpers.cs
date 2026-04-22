using System.Reflection;
using System.Threading;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Hosting;

namespace AsterGraph.Wpf.Tests;

internal static class WpfRouteTestHelpers
{
    private static readonly string[] CandidateAssemblyNames =
    [
        "AsterGraph.Wpf",
        "AsterGraph.Starter.Wpf",
        "AsterGraph.Wpf.Starter",
    ];

    private static readonly string[] HostContextTypeNames =
    [
        "AsterGraph.Wpf.Hosting.WpfGraphHostContext",
        "AsterGraph.Wpf.WpfGraphHostContext",
        "AsterGraph.Wpf.Wpf.WpfGraphHostContext",
    ];

    private static readonly string[] WpfViewFactoryTypeNames =
    [
        "AsterGraph.Wpf.Hosting.AsterGraphWpfViewFactory",
    ];

    private static readonly string[] WpfViewOptionsTypeNames =
    [
        "AsterGraph.Wpf.Hosting.AsterGraphWpfViewOptions",
    ];

    public static IGraphHostContext CreateWpfHostContext(object owner, object? topLevel)
    {
        var hostContextType = ResolveType(
            "WPF host context",
            HostContextTypeNames,
            FilterHostContextType);

        var ctor = hostContextType.GetConstructor(
            [typeof(object), typeof(object), typeof(IServiceProvider)])
            ?? hostContextType.GetConstructor([typeof(object), typeof(object)])
            ?? hostContextType.GetConstructor([typeof(object)])
            ?? throw new InvalidOperationException(
                $"WPF host context type '{hostContextType.FullName}' does not have a usable constructor for tests.");

        var instance = ctor.GetParameters().Length switch
        {
            3 => ctor.Invoke([owner, topLevel, null]),
            2 => ctor.Invoke([owner, topLevel]),
            _ => ctor.Invoke([owner]),
        };

        return AssertAssignable<IGraphHostContext>(instance, "WPF host context instance");
    }

    public static GraphEditorViewModel CreateHelloWorldEditor()
    {
        var document = new GraphDocument(
            "Hello WPF Graph",
            "Minimal WPF hosted-shell smoke graph.",
            [
                new GraphNode(
                    "wpf-hello-node",
                    "WPF Hello Node",
                    "Hello",
                    "Host",
                    "Minimal hosted-shell composition node.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [],
                    "#6AD5C4",
                    new NodeDefinitionId("hello.wpf.node")),
            ],
            []);

        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("hello.wpf.node"),
                "WPF Hello Node",
                "Hello",
                "Host",
                [],
                []));

        return AsterGraphEditorFactory.Create(
            new AsterGraphEditorOptions
            {
                Document = document,
                NodeCatalog = catalog,
                CompatibilityService = new DefaultPortCompatibilityService(),
            });
    }

    public static object CreateHelloWorldHostedView(GraphEditorViewModel editor)
    {
        var editorPropertyName = "Editor";
        var applyHostServicesPropertyName = "ApplyHostServices";
        var factoryType = ResolveType("WPF view factory", WpfViewFactoryTypeNames, _ => true);
        var method = factoryType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"Expected public static Create(...) method on '{factoryType.FullName}'.");
        var optionsType = ResolveType("WPF view options", WpfViewOptionsTypeNames, type =>
            type.GetProperty(editorPropertyName) is not null && type.GetProperty(applyHostServicesPropertyName) is not null);

        var options = AssertAssignable<object>(Activator.CreateInstance(optionsType)!, "WPF view options instance");
        SetProperty(options, editorPropertyName, editor);
        SetProperty(options, applyHostServicesPropertyName, true);

        var result = method.Invoke(null, [options]);
        if (result is null)
        {
            throw new InvalidOperationException(
                $"'{factoryType.FullName}.{method.Name}(options)' returned null.");
        }

        return result;
    }

    public static TValue GetProperty<TValue>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
        {
            throw new InvalidOperationException(
                $"Expected public property '{propertyName}' on '{instance.GetType().FullName}'.");
        }

        var value = property.GetValue(instance);
        if (value is null && default(TValue) is not null)
        {
            throw new InvalidOperationException(
                $"Expected non-null '{propertyName}' on '{instance.GetType().FullName}'.");
        }

        if (value is TValue casted)
        {
            return casted;
        }

        throw new InvalidOperationException(
            $"Property '{propertyName}' on '{instance.GetType().FullName}' is '{value?.GetType().FullName ?? "(null)"}', expected '{typeof(TValue).FullName}'.");
    }

    public static TValue AssertAssignable<TValue>(object value, string description)
        where TValue : class
    {
        if (value is TValue typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"{description} was '{value.GetType().FullName}', expected '{typeof(TValue).FullName}'.");
    }

    public static TValue RunInSta<TValue>(Func<TValue> action)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return action();
        }

        var exception = default(Exception);
        var value = default(TValue);
        var started = new ManualResetEventSlim(false);
        var completed = new ManualResetEventSlim(false);

        var thread = new Thread(() =>
        {
            started.Set();
            try
            {
                value = action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        started.Wait();
        completed.Wait();
        thread.Join();

        if (exception is not null)
        {
            throw new InvalidOperationException("STA invocation failed.", exception);
        }

        return value!;
    }

    public static void SetProperty(object instance, string propertyName, object? value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
        {
            throw new InvalidOperationException(
                $"Expected public property '{propertyName}' on '{instance.GetType().FullName}'.");
        }

        property.SetValue(instance, value);
    }

    private static Type ResolveType(string description, string[] candidateTypeNames, Func<Type, bool> predicate)
    {
        foreach (var assembly in ResolveTypeCandidates())
        {
            foreach (var candidate in candidateTypeNames)
            {
                var type = assembly.GetType(candidate);
                if (type is not null && predicate(type))
                {
                    return type;
                }
            }
        }

        throw new InvalidOperationException($"Could not resolve {description} from known type names.");
    }

    private static bool FilterHostContextType(Type type)
        => typeof(IGraphHostContext).IsAssignableFrom(type)
            && type.Name.Contains("HostContext", StringComparison.Ordinal)
            && !type.IsAbstract;

    private static List<Assembly> ResolveTypeCandidates()
    {
        var candidates = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());

        foreach (var assemblyName in CandidateAssemblyNames)
        {
            if (candidates.Any(assembly => string.Equals(
                    assembly.GetName().Name,
                    assemblyName,
                    StringComparison.Ordinal)))
            {
                continue;
            }

            try
            {
                var loaded = Assembly.Load(assemblyName);
                candidates.Add(loaded);
            }
            catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
            {
                // WPF adapter/project may not be available in this test execution environment.
            }
        }

        // Narrow to AsterGraph assemblies to avoid selecting unrelated host types.
        return candidates
            .Where(assembly =>
            {
                var assemblyName = assembly.GetName().Name;
                return assemblyName is not null
                    && assemblyName.StartsWith("AsterGraph.", StringComparison.Ordinal);
            })
            .ToList();
    }
}
