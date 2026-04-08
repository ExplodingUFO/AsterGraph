using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginInspectionContractsTests
{
    [Fact]
    public void IGraphEditorQueries_DefinesCanonicalPluginLoadInspectionRead()
    {
        var method = typeof(IGraphEditorQueries).GetMethod(nameof(IGraphEditorQueries.GetPluginLoadSnapshots));

        Assert.NotNull(method);
        Assert.Equal(typeof(IReadOnlyList<GraphEditorPluginLoadSnapshot>), method!.ReturnType);
    }

    [Fact]
    public void PluginLoadInspectionContractSurface_IsRuntimeFirstAndFreeOfAvaloniaAndGraphEditorViewModel()
    {
        var publicTypes = new Type[]
        {
            typeof(GraphEditorPluginLoadSnapshot),
            typeof(GraphEditorPluginContributionSummarySnapshot),
            typeof(GraphEditorPluginLoadSourceKind),
            typeof(GraphEditorPluginLoadStatus),
        };

        foreach (var type in publicTypes)
        {
            Assert.True(type.IsPublic);
            Assert.DoesNotContain(GetPubliclyReferencedTypes(type), IsDisallowedType);
        }
    }

    [Fact]
    public void CreateSession_WithAssemblyPluginRegistration_ExposesStructuredPluginLoadSnapshot()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath())));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var inspection = session.Diagnostics.CaptureInspectionSnapshot();

        Assert.Equal(GraphEditorPluginLoadSourceKind.Assembly, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
        Assert.Equal(Path.GetFullPath(GetSamplePluginAssemblyPath()), snapshot.Source);
        Assert.Null(snapshot.RequestedPluginTypeName);
        Assert.Equal("AsterGraph.TestPlugins.SamplePlugin", snapshot.ResolvedPluginTypeName);
        Assert.NotNull(snapshot.Descriptor);
        Assert.Equal("tests.sample-plugin", snapshot.Descriptor!.Id);
        Assert.Equal(1, snapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(1, snapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(1, snapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(1, snapshot.Contributions.LocalizationProviderCount);
        Assert.Null(snapshot.FailureMessage);
        Assert.Equal(session.Queries.GetPluginLoadSnapshots(), inspection.PluginLoadSnapshots);
    }

    [Fact]
    public void CreateSession_WithMissingAssemblyPluginRegistration_ExposesStructuredFailureSnapshot()
    {
        var missingAssemblyPath = Path.Combine(Path.GetTempPath(), "astergraph-plugin-tests", Guid.NewGuid().ToString("N"), "missing-plugin.dll");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromAssemblyPath(missingAssemblyPath)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());

        Assert.Equal(GraphEditorPluginLoadSourceKind.Assembly, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Failed, snapshot.Status);
        Assert.Equal(Path.GetFullPath(missingAssemblyPath), snapshot.Source);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.ResolvedPluginTypeName);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("missing-plugin.dll", snapshot.FailureMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, snapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(0, snapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(0, snapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(0, snapshot.Contributions.LocalizationProviderCount);
    }

    [Fact]
    public void Create_And_CreateSession_ExposeEquivalentPluginLoadSnapshots_AndDiscoverability()
    {
        var registration = GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath());
        var editor = AsterGraphEditorFactory.Create(CreateOptions(registration));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(registration));

        var retainedSnapshots = editor.Session.Queries.GetPluginLoadSnapshots()
            .OrderBy(snapshot => snapshot.Source, StringComparer.Ordinal)
            .ToList();
        var runtimeSnapshots = session.Queries.GetPluginLoadSnapshots()
            .OrderBy(snapshot => snapshot.Source, StringComparer.Ordinal)
            .ToList();
        var retainedFeatures = editor.Session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var runtimeFeatures = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.True(retainedFeatures["query.plugin-load-snapshots"].IsAvailable);
        Assert.True(runtimeFeatures["query.plugin-load-snapshots"].IsAvailable);
    }

    private static AsterGraphEditorOptions CreateOptions(params GraphEditorPluginRegistration[] pluginRegistrations)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations = pluginRegistrations,
        };

    private static string GetSamplePluginAssemblyPath()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "AsterGraph.TestPlugins",
            "bin",
            "Debug",
            "net9.0",
            "AsterGraph.TestPlugins.dll"));

    private static IReadOnlyList<Type> GetPubliclyReferencedTypes(Type type)
    {
        var referencedTypes = new List<Type>();

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            switch (member)
            {
                case PropertyInfo property:
                    referencedTypes.Add(property.PropertyType);
                    break;
                case MethodInfo method:
                    referencedTypes.Add(method.ReturnType);
                    referencedTypes.AddRange(method.GetParameters().Select(parameter => parameter.ParameterType));
                    break;
                case ConstructorInfo constructor:
                    referencedTypes.AddRange(constructor.GetParameters().Select(parameter => parameter.ParameterType));
                    break;
                case EventInfo eventInfo when eventInfo.EventHandlerType is not null:
                    referencedTypes.Add(eventInfo.EventHandlerType);
                    break;
            }
        }

        return referencedTypes
            .Where(candidate => candidate != typeof(void))
            .SelectMany(ExpandType)
            .Distinct()
            .ToList();
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var expanded in ExpandType(elementType))
            {
                yield return expanded;
            }

            yield break;
        }

        if (type.IsGenericType)
        {
            yield return type.GetGenericTypeDefinition();

            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var expanded in ExpandType(argument))
                {
                    yield return expanded;
                }
            }

            yield break;
        }

        yield return type;
    }

    private static bool IsDisallowedType(Type type)
    {
        var fullName = type.FullName ?? string.Empty;
        return fullName.StartsWith("Avalonia.", StringComparison.Ordinal)
            || fullName.Contains("GraphEditorViewModel", StringComparison.Ordinal)
            || fullName.Contains("NodeViewModel", StringComparison.Ordinal);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Plugin Inspection Graph",
            "Plugin inspection regression coverage.",
            [
                new GraphNode(
                    "source-node",
                    "Source Node",
                    "Tests",
                    "Plugins",
                    "Source node for plugin inspection tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    new NodeDefinitionId("tests.plugins.source")),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            new NodeDefinitionId("tests.plugins.source"),
            "Plugin Source",
            "Tests",
            "Plugins",
            [],
            [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }
}
