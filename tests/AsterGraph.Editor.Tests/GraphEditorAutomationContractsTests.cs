using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorAutomationContractsTests
{
    [Fact]
    public void IGraphEditorSession_ExposesAutomationRunnerProperty()
    {
        var sessionType = typeof(IGraphEditorSession);

        Assert.True(sessionType.IsPublic);
        Assert.True(sessionType.IsInterface);

        AssertProperty(sessionType, nameof(IGraphEditorSession.Automation), typeof(IGraphEditorAutomationRunner));
    }

    [Fact]
    public void AutomationContractSurface_IsRuntimeFirstAndFreeOfAvaloniaAndGraphEditorViewModel()
    {
        var publicTypes = new Type[]
        {
            typeof(IGraphEditorAutomationRunner),
            typeof(GraphEditorAutomationRunRequest),
            typeof(GraphEditorAutomationStep),
            typeof(GraphEditorAutomationExecutionSnapshot),
            typeof(GraphEditorAutomationStepExecutionSnapshot),
        };

        foreach (var type in publicTypes)
        {
            Assert.True(type.IsPublic);
            Assert.DoesNotContain(GetPubliclyReferencedTypes(type), IsDisallowedType);
        }
    }

    [Fact]
    public void AutomationContracts_UseCanonicalInvocationAndInspectionTypes()
    {
        AssertProperty(typeof(GraphEditorAutomationStep), nameof(GraphEditorAutomationStep.Command), typeof(GraphEditorCommandInvocationSnapshot));
        AssertProperty(typeof(GraphEditorAutomationExecutionSnapshot), nameof(GraphEditorAutomationExecutionSnapshot.Inspection), typeof(GraphEditorInspectionSnapshot));
        AssertProperty(typeof(GraphEditorAutomationExecutionSnapshot), nameof(GraphEditorAutomationExecutionSnapshot.Steps), typeof(IReadOnlyList<GraphEditorAutomationStepExecutionSnapshot>));
    }

    [Fact]
    public void CreateSession_ExposesAutomationDiscoverabilityAndAutomationCriticalCommandDescriptors()
    {
        var definitionId = new NodeDefinitionId("tests.automation.contracts");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var features = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var commands = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.True(features["surface.automation.runner"].IsAvailable);
        Assert.True(features["event.automation.started"].IsAvailable);
        Assert.True(features["event.automation.progress"].IsAvailable);
        Assert.True(features["event.automation.completed"].IsAvailable);

        Assert.True(commands["selection.set"].IsEnabled);
        Assert.True(commands["nodes.move"].IsEnabled);
        Assert.True(commands["viewport.pan"].IsEnabled);
        Assert.True(commands["viewport.resize"].IsEnabled);
        Assert.Contains("connections.complete", commands.Keys);
        Assert.Contains("viewport.center", commands.Keys);
    }

    [Fact]
    public void Create_And_CreateSession_ExposeEquivalentAutomationDiscoverySurface()
    {
        var definitionId = new NodeDefinitionId("tests.automation.discovery-parity");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var runtimeSession = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        var retainedFeatures = CaptureAutomationFeatureSignature(editor.Session);
        var runtimeFeatures = CaptureAutomationFeatureSignature(runtimeSession);
        var retainedCommands = CaptureAutomationCommandSignature(editor.Session);
        var runtimeCommands = CaptureAutomationCommandSignature(runtimeSession);

        Assert.Equal(retainedFeatures, runtimeFeatures);
        Assert.Equal(retainedCommands, runtimeCommands);
    }

    private static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Automation Contracts Graph",
            "Automation contract coverage graph.",
            [
                new GraphNode(
                    "tests-automation-source",
                    "Automation Source",
                    "Tests",
                    "Automation",
                    "Source node for automation contract tests.",
                    new GraphPoint(120, 180),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    "tests-automation-target",
                    "Automation Target",
                    "Tests",
                    "Automation",
                    "Target node for automation contract tests.",
                    new GraphPoint(420, 180),
                    new GraphSize(220, 140),
                    [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Automation Node",
            "Tests",
            "Automation",
            [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
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

    private static string CaptureAutomationFeatureSignature(IGraphEditorSession session)
        => string.Join(
            "|",
            session.Queries.GetFeatureDescriptors()
                .Where(descriptor => AutomationFeatureIds.Contains(descriptor.Id, StringComparer.Ordinal))
                .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
                .Select(descriptor => $"{descriptor.Id}:{descriptor.IsAvailable}"));

    private static string CaptureAutomationCommandSignature(IGraphEditorSession session)
        => string.Join(
            "|",
            session.Queries.GetCommandDescriptors()
                .Where(descriptor => AutomationCommandIds.Contains(descriptor.Id, StringComparer.Ordinal))
                .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
                .Select(descriptor => $"{descriptor.Id}:{descriptor.IsEnabled}"));

    private static void AssertProperty(Type type, string propertyName, Type expectedPropertyType)
    {
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(expectedPropertyType, property!.PropertyType);
    }

    private static readonly string[] AutomationFeatureIds =
    [
        "surface.automation.runner",
        "event.automation.started",
        "event.automation.progress",
        "event.automation.completed",
    ];

    private static readonly string[] AutomationCommandIds =
    [
        "selection.set",
        "nodes.move",
        "connections.start",
        "connections.complete",
        "viewport.pan",
        "viewport.resize",
        "viewport.center",
    ];
}
