using System.Reflection;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginContractsTests
{
    [Fact]
    public void AsterGraphEditorOptions_ExposesExplicitPluginRegistrations()
    {
        var property = typeof(AsterGraphEditorOptions).GetProperty(nameof(AsterGraphEditorOptions.PluginRegistrations));

        Assert.NotNull(property);
        Assert.Equal(typeof(IReadOnlyList<GraphEditorPluginRegistration>), property!.PropertyType);

        var options = new AsterGraphEditorOptions();

        Assert.NotNull(options.PluginRegistrations);
        Assert.Empty(options.PluginRegistrations);
    }

    [Fact]
    public void AsterGraphEditorOptions_ExposesCanonicalPluginTrustPolicy()
    {
        var property = typeof(AsterGraphEditorOptions).GetProperty(nameof(AsterGraphEditorOptions.PluginTrustPolicy));

        Assert.NotNull(property);
        Assert.Equal(typeof(IGraphEditorPluginTrustPolicy), property!.PropertyType);

        var options = new AsterGraphEditorOptions();

        Assert.Null(options.PluginTrustPolicy);
    }

    [Fact]
    public void GraphEditorPluginRegistration_SupportsDirectAndAssemblyBasedInputs_WithOptionalManifestMetadata()
    {
        var plugin = new TestPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.plugin.manifest",
            "Tests Plugin Manifest",
            new GraphEditorPluginManifestProvenance(GraphEditorPluginManifestSourceKind.DirectRegistration, "Tests.Plugin"),
            version: "1.2.3",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "1.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "node-definitions, menus, localization");

        var direct = GraphEditorPluginRegistration.FromPlugin(plugin, manifest);
        var assembly = GraphEditorPluginRegistration.FromAssemblyPath(@"C:\plugins\sample\SamplePlugin.dll", "Sample.Plugin", manifest);

        Assert.Same(plugin, direct.Plugin);
        Assert.Null(direct.AssemblyPath);
        Assert.Null(direct.PluginTypeName);
        Assert.Equal(manifest, direct.Manifest);
        Assert.True(direct.IsDirectRegistration);
        Assert.False(direct.IsAssemblyRegistration);

        Assert.Null(assembly.Plugin);
        Assert.Equal(@"C:\plugins\sample\SamplePlugin.dll", assembly.AssemblyPath);
        Assert.Equal("Sample.Plugin", assembly.PluginTypeName);
        Assert.Equal(manifest, assembly.Manifest);
        Assert.False(assembly.IsDirectRegistration);
        Assert.True(assembly.IsAssemblyRegistration);
    }

    [Fact]
    public void GraphEditorPluginManifest_ExposesCanonicalIdentityCompatibilityCapabilityAndProvenanceMetadata()
    {
        var provenance = new GraphEditorPluginManifestProvenance(
            GraphEditorPluginManifestSourceKind.AssemblyPath,
            @"C:\plugins\sample\SamplePlugin.dll",
            publisher: "AsterGraph Tests",
            packageId: "AsterGraph.SamplePlugin",
            packageVersion: "1.2.3");
        var compatibility = new GraphEditorPluginCompatibilityManifest(
            minimumAsterGraphVersion: "1.0.0",
            maximumAsterGraphVersion: "2.0.0",
            targetFramework: "net9.0",
            runtimeSurface: "session-first");
        var manifest = new GraphEditorPluginManifest(
            "tests.plugin.manifest",
            "Tests Plugin Manifest",
            provenance,
            description: "Contract coverage for plugin manifests.",
            version: "1.2.3",
            compatibility: compatibility,
            capabilitySummary: "menus, node-presentations");

        Assert.Equal("tests.plugin.manifest", manifest.Id);
        Assert.Equal("Tests Plugin Manifest", manifest.DisplayName);
        Assert.Equal("Contract coverage for plugin manifests.", manifest.Description);
        Assert.Equal("1.2.3", manifest.Version);
        Assert.Equal(compatibility, manifest.Compatibility);
        Assert.Equal("menus, node-presentations", manifest.CapabilitySummary);
        Assert.Equal(provenance, manifest.Provenance);
        Assert.Equal(GraphEditorPluginManifestSourceKind.AssemblyPath, manifest.Provenance.SourceKind);
        Assert.Equal("session-first", manifest.Compatibility.RuntimeSurface);
    }

    [Fact]
    public void GraphEditorPluginContractSurface_IsRuntimeFirstAndFreeOfAvaloniaAndGraphEditorViewModel()
    {
        var publicTypes = new Type[]
        {
            typeof(IGraphEditorPlugin),
            typeof(GraphEditorPluginDescriptor),
            typeof(GraphEditorPluginManifest),
            typeof(GraphEditorPluginManifestProvenance),
            typeof(GraphEditorPluginManifestSourceKind),
            typeof(GraphEditorPluginCompatibilityManifest),
            typeof(GraphEditorPluginRegistration),
            typeof(GraphEditorPluginBuilder),
            typeof(GraphEditorPluginMenuAugmentationContext),
            typeof(IGraphEditorPluginContextMenuAugmentor),
            typeof(GraphEditorPluginNodePresentationContext),
            typeof(IGraphEditorPluginNodePresentationProvider),
            typeof(IGraphEditorPluginLocalizationProvider),
            typeof(IGraphEditorPluginTrustPolicy),
            typeof(GraphEditorPluginTrustPolicyContext),
            typeof(GraphEditorPluginTrustEvaluation),
            typeof(GraphEditorPluginTrustDecision),
            typeof(GraphEditorPluginTrustEvaluationSource),
        };

        foreach (var type in publicTypes)
        {
            Assert.True(type.IsPublic);
            Assert.DoesNotContain(GetPubliclyReferencedTypes(type), IsDisallowedType);
        }
    }

    [Fact]
    public void GraphEditorPluginBuilder_CollectsRuntimeFacingContributionSlots()
    {
        var builder = new GraphEditorPluginBuilder();
        var definitionProvider = new TestDefinitionProvider();
        var menuAugmentor = new TestMenuAugmentor();
        var nodePresentationProvider = new TestNodePresentationProvider();
        var localizationProvider = new TestLocalizationProvider();

        builder.AddNodeDefinitionProvider(definitionProvider);
        builder.AddContextMenuAugmentor(menuAugmentor);
        builder.AddNodePresentationProvider(nodePresentationProvider);
        builder.AddLocalizationProvider(localizationProvider);

        Assert.Same(definitionProvider, Assert.Single(builder.NodeDefinitionProviders));
        Assert.Same(menuAugmentor, Assert.Single(builder.ContextMenuAugmentors));
        Assert.Same(nodePresentationProvider, Assert.Single(builder.NodePresentationProviders));
        Assert.Same(localizationProvider, Assert.Single(builder.LocalizationProviders));
    }

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

    private sealed class TestPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.plugin", "Tests Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new TestDefinitionProvider());
        }
    }

    private sealed class TestDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            => [new NodeDefinition(new NodeDefinitionId("tests.plugin.node"), "Plugin Node", "Tests", "Plugin", [], [])];
    }

    private sealed class TestMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return context.StockItems;
        }
    }

    private sealed class TestNodePresentationProvider : IGraphEditorPluginNodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return NodePresentationState.Empty;
        }
    }

    private sealed class TestLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => fallback;
    }
}
