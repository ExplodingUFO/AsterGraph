using System.Reflection;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSessionTests
{
    [Fact]
    public void IGraphEditorSession_ExposesCommandsQueriesAndEventsProperties()
    {
        var sessionType = typeof(IGraphEditorSession);

        Assert.True(sessionType.IsPublic);
        Assert.True(sessionType.IsInterface);

        AssertProperty(sessionType, nameof(IGraphEditorSession.Commands), typeof(IGraphEditorCommands));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Queries), typeof(IGraphEditorQueries));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Events), typeof(IGraphEditorEvents));
    }

    [Fact]
    public void IGraphEditorCommands_DefinesHostFacingEditAndViewportActions()
    {
        var commandsType = typeof(IGraphEditorCommands);

        Assert.True(commandsType.IsPublic);
        Assert.True(commandsType.IsInterface);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.Undo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.Redo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ClearSelection), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.AddNode), typeof(NodeDefinitionId), typeof(GraphPoint?));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteSelection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.PanBy), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ZoomAt), typeof(double), typeof(GraphPoint));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ResetView), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SaveWorkspace));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.LoadWorkspace));
    }

    [Fact]
    public void IGraphEditorQueries_DefinesHostFacingSnapshotAndDiscoveryReads()
    {
        var queriesType = typeof(IGraphEditorQueries);

        Assert.True(queriesType.IsPublic);
        Assert.True(queriesType.IsInterface);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.CreateDocumentSnapshot));
        Assert.Equal(typeof(GraphDocument), queriesType.GetMethod(nameof(IGraphEditorQueries.CreateDocumentSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodePositions));
        Assert.Equal(typeof(IReadOnlyList<NodePositionSnapshot>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodePositions))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatibleTargets), typeof(string), typeof(string));
        Assert.Equal(typeof(IReadOnlyList<CompatiblePortTarget>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets))!.ReturnType);
    }

    [Fact]
    public void IGraphEditorEvents_ReusesExistingTypedEventArgs()
    {
        var eventsType = typeof(IGraphEditorEvents);

        Assert.True(eventsType.IsPublic);
        Assert.True(eventsType.IsInterface);

        AssertEvent(eventsType, nameof(IGraphEditorEvents.DocumentChanged), typeof(GraphEditorDocumentChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.SelectionChanged), typeof(GraphEditorSelectionChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.ViewportChanged), typeof(GraphEditorViewportChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentExported), typeof(GraphEditorFragmentEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentImported), typeof(GraphEditorFragmentEventArgs));
    }

    [Fact(Skip = "Phase 2 Plan 02-02 implements the concrete runtime session factory entry point.")]
    public void AsterGraphEditorFactory_CreateSession_WillProvidePublicRuntimeEntryPoint()
    {
        var method = typeof(AsterGraphEditorFactory)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(candidate =>
                candidate.Name == "CreateSession"
                && candidate.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(AsterGraphEditorOptions));

        Assert.NotNull(method);
        Assert.Equal(typeof(IGraphEditorSession), method!.ReturnType);
    }

    [Fact(Skip = "Phase 2 Plan 02-02 implements the compatibility session bridge.")]
    public void GraphEditorViewModel_Session_WillExposeSharedRuntimeSession()
    {
        var property = typeof(GraphEditorViewModel).GetProperty("Session", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(typeof(IGraphEditorSession), property!.PropertyType);
    }

    private static void AssertProperty(Type declaringType, string propertyName, Type propertyType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property!.PropertyType);
        Assert.Null(property.SetMethod);
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, parameterTypes);

        Assert.NotNull(method);
    }

    private static void AssertEvent(Type declaringType, string eventName, Type eventArgsType)
    {
        var eventInfo = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(eventInfo);

        var handlerType = eventInfo!.EventHandlerType!;
        Assert.True(handlerType.IsGenericType);
        Assert.Equal(typeof(EventHandler<>), handlerType.GetGenericTypeDefinition());
        Assert.Equal(eventArgsType, handlerType.GetGenericArguments()[0]);
    }
}
