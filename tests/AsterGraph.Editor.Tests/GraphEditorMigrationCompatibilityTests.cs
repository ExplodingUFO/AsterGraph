using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AsterGraph.Editor.Tests;

[Collection("Avalonia UI")]
public sealed class GraphEditorMigrationCompatibilityTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.migration.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.migration.target");
    private const string SourceNodeId = "tests.migration.source-001";
    private const string TargetNodeId = "tests.migration.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void LegacyGraphEditorViewModelConstructor_RemainsSupportedAlongsideNewInitializationApis()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);

        Assert.Same(harness.StyleOptions, legacyEditor.StyleOptions);
        Assert.Same(harness.BehaviorOptions, legacyEditor.BehaviorOptions);
        Assert.Same(harness.MenuAugmentor, legacyEditor.ContextMenuAugmentor);
        Assert.Same(harness.PresentationProvider, legacyEditor.NodePresentationProvider);
        Assert.Same(harness.LocalizationProvider, legacyEditor.LocalizationProvider);
        Assert.Equal(harness.WorkspaceService.WorkspacePath, legacyEditor.WorkspacePath);
        Assert.Equal(harness.FragmentWorkspaceService.FragmentPath, legacyEditor.FragmentPath);
        Assert.Equal(harness.FragmentLibraryService.LibraryPath, legacyEditor.FragmentLibraryPath);
        Assert.IsAssignableFrom<IGraphEditorSession>(legacyEditor.Session);

        var snapshot = CaptureSnapshot(legacyEditor);
        Assert.Equal("Migration subtitle", snapshot.NodeSubtitle);
        Assert.Equal("Migration description", snapshot.NodeDescription);
        Assert.Equal("Localized stats 2/0/88", snapshot.StatsCaption);
        Assert.Contains("tests.migration.host-action", snapshot.MenuSignature);
        Assert.Equal(1, snapshot.CompatibleTargetCount);
    }

    [Fact]
    public void NewInitializationApis_CreateEditorStateEquivalentToLegacyConstructorPath()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        Assert.Equal(CaptureSnapshot(legacyEditor), CaptureSnapshot(factoryEditor));
    }

    [Fact]
    public void LegacyAndFactoryEditorSessions_RemainBehaviorallyAlignedAfterAdapterBackedSessionCommands()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var legacyHost = legacyEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(legacyEditor.Session);
        var factoryHost = factoryEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(factoryEditor.Session);

        legacyEditor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        factoryEditor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);

        Assert.NotNull(legacyHost);
        Assert.NotNull(factoryHost);
        Assert.IsNotType<GraphEditorViewModel>(legacyHost);
        Assert.IsNotType<GraphEditorViewModel>(factoryHost);
        Assert.Equal(
            legacyEditor.Session.Queries.GetSelectionSnapshot().SelectedNodeIds,
            factoryEditor.Session.Queries.GetSelectionSnapshot().SelectedNodeIds);
        Assert.Equal(legacyEditor.SelectedNodes.Count, factoryEditor.SelectedNodes.Count);
        Assert.Equal(legacyEditor.SelectedNode?.Id, factoryEditor.SelectedNode?.Id);
    }

    [Fact]
    public void LegacyAndFactoryEditorSessions_ExposeEquivalentFeatureDescriptors()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyDescriptors = legacyEditor.Session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var factoryDescriptors = factoryEditor.Session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(factoryDescriptors, legacyDescriptors);
    }

    [Fact]
    public void LegacyAndFactoryEditorSessions_ExposeEquivalentCommandDescriptors()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyDescriptors = legacyEditor.Session.Queries.GetCommandDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var factoryDescriptors = factoryEditor.Session.Queries.GetCommandDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(factoryDescriptors, legacyDescriptors);
    }

    [Fact]
    public void LegacyAndFactoryEditorSessions_ExposeEquivalentCanvasMenuDescriptors()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var context = CreateMenuContext();

        var legacyDescriptors = legacyEditor.Session.Queries.BuildContextMenuDescriptors(context)
            .Select(item => item.Id)
            .ToList();
        var factoryDescriptors = factoryEditor.Session.Queries.BuildContextMenuDescriptors(context)
            .Select(item => item.Id)
            .ToList();

        Assert.Equal(factoryDescriptors, legacyDescriptors);
    }

    [Fact]
#pragma warning disable CS0618
    public void LegacyCompatibleTargets_ReturnRetainedNodeAndPortInstances()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);

        var compatibleTargets = legacyEditor.GetCompatibleTargets(SourceNodeId, SourcePortId);

        var target = Assert.Single(compatibleTargets);
        var retainedNode = Assert.IsType<NodeViewModel>(legacyEditor.FindNode(TargetNodeId));
        var retainedPort = Assert.IsType<PortViewModel>(retainedNode.GetPort(TargetPortId));
        Assert.Same(retainedNode, target.Node);
        Assert.Same(retainedPort, target.Port);
    }
#pragma warning restore CS0618

    [Fact]
#pragma warning disable CS0618
    public void RetainedCompatibilityFacade_RemainsLocalizedWhileSessionHostStaysAdapterBacked()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var legacyHost = legacyEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(legacyEditor.Session);
        var factoryHost = factoryEditor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(factoryEditor.Session);
        var legacyFacadeTarget = Assert.Single(legacyEditor.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var factoryFacadeTarget = Assert.Single(factoryEditor.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var legacySessionTarget = Assert.Single(legacyEditor.Session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var factorySessionTarget = Assert.Single(factoryEditor.Session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId));
        var legacyRetainedNode = Assert.IsType<NodeViewModel>(legacyEditor.FindNode(TargetNodeId));
        var factoryRetainedNode = Assert.IsType<NodeViewModel>(factoryEditor.FindNode(TargetNodeId));
        var legacyRetainedPort = Assert.IsType<PortViewModel>(legacyRetainedNode.GetPort(TargetPortId));
        var factoryRetainedPort = Assert.IsType<PortViewModel>(factoryRetainedNode.GetPort(TargetPortId));

        Assert.NotNull(legacyHost);
        Assert.NotNull(factoryHost);
        Assert.IsType<GraphEditorViewModelKernelAdapter>(legacyHost);
        Assert.IsType<GraphEditorViewModelKernelAdapter>(factoryHost);
        Assert.Null(legacyHost!.GetType().GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(string), typeof(string)]));
        Assert.Null(factoryHost!.GetType().GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(string), typeof(string)]));
        Assert.Same(legacyRetainedNode, legacyFacadeTarget.Node);
        Assert.Same(factoryRetainedNode, factoryFacadeTarget.Node);
        Assert.Same(legacyRetainedPort, legacyFacadeTarget.Port);
        Assert.Same(factoryRetainedPort, factoryFacadeTarget.Port);
        Assert.NotSame(legacyRetainedNode, legacySessionTarget.Node);
        Assert.NotSame(factoryRetainedNode, factorySessionTarget.Node);
        Assert.Equal(legacyFacadeTarget.Node.Id, legacySessionTarget.Node.Id);
        Assert.Equal(factoryFacadeTarget.Node.Id, factorySessionTarget.Node.Id);
        Assert.Equal(legacyFacadeTarget.Port.Id, legacySessionTarget.Port.Id);
        Assert.Equal(factoryFacadeTarget.Port.Id, factorySessionTarget.Port.Id);
    }
#pragma warning restore CS0618

    [Fact]
    public void MigrationGuidance_KeepsCompatibilityShimRetirementExplicit()
    {
        var queryMethod = typeof(IGraphEditorQueries).GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(string), typeof(string)]);

        Assert.NotNull(queryMethod);
        var queryAttribute = Assert.Single(
            queryMethod!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
#pragma warning disable CS0618
        var shimAttribute = Assert.Single(
            typeof(CompatiblePortTarget).GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
#pragma warning restore CS0618

        var queryObsolete = Assert.IsType<ObsoleteAttribute>(queryAttribute);
        var shimObsolete = Assert.IsType<ObsoleteAttribute>(shimAttribute);
        Assert.Contains("canonical runtime queries", queryObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("later minor releases may add stronger warnings", queryObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("future major release may remove it", queryObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("Retained compatibility shim", shimObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("future major release may remove it", shimObsolete.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedNodeSurfaceCompatibilityHelpers_AreExplicitlyMarkedObsoleteWithRuntimeReplacements()
    {
        var expansionMethod = typeof(GraphEditorViewModel).GetMethod(
            nameof(GraphEditorViewModel.TrySetNodeExpansionState),
            [typeof(NodeViewModel), typeof(GraphNodeExpansionState)]);
        var groupPaddingMethod = typeof(GraphEditorViewModel).GetMethod(
            nameof(GraphEditorViewModel.TrySetNodeGroupExtraPadding),
            [typeof(string), typeof(GraphPadding), typeof(bool)]);

        Assert.NotNull(expansionMethod);
        Assert.NotNull(groupPaddingMethod);

        var expansionAttribute = Assert.Single(
            expansionMethod!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
        var groupPaddingAttribute = Assert.Single(
            groupPaddingMethod!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);

        var expansionObsolete = Assert.IsType<ObsoleteAttribute>(expansionAttribute);
        var groupPaddingObsolete = Assert.IsType<ObsoleteAttribute>(groupPaddingAttribute);

        Assert.Contains("TrySetNodeSize", expansionObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("GetNodeSurfaceSnapshots", expansionObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("TrySetNodeGroupSize", groupPaddingObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("TrySetNodeGroupPosition", groupPaddingObsolete.Message, StringComparison.Ordinal);
        Assert.Contains("GetNodeGroupSnapshots", groupPaddingObsolete.Message, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void RetainedCompatibilityTypes_AreExplicitlyDemotedFromCanonicalNewWork()
    {
        var editorAttribute = Assert.Single(
            typeof(GraphEditorViewModel).GetCustomAttributes(typeof(EditorBrowsableAttribute), inherit: false),
            attribute => attribute is EditorBrowsableAttribute);
        var viewAttribute = Assert.Single(
            typeof(GraphEditorView).GetCustomAttributes(typeof(EditorBrowsableAttribute), inherit: false),
            attribute => attribute is EditorBrowsableAttribute);

        Assert.Equal(EditorBrowsableState.Advanced, Assert.IsType<EditorBrowsableAttribute>(editorAttribute).State);
        Assert.Equal(EditorBrowsableState.Advanced, Assert.IsType<EditorBrowsableAttribute>(viewAttribute).State);
    }

    [AvaloniaFact]
    public void GraphEditorView_RemainsCompatibilityFacadeDuringStagedMigration()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };

        var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        Assert.Same(legacyEditor, legacyView.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, legacyView.ChromeMode);
        Assert.Same(factoryEditor, factoryView.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, factoryView.ChromeMode);
    }

    [AvaloniaFact]
    public void StagedMigrationPath_AllowsHostsToAdoptNewApisWithoutImmediateRewrite()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);

        var legacyEverywhere = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryEditorOnly = new GraphEditorView
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryViewOnly = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var factoryEverywhere = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });

        AssertViewBindings(legacyEverywhere, legacyEditor);
        AssertViewBindings(factoryEditorOnly, factoryEditor);
        AssertViewBindings(factoryViewOnly, legacyEditor);
        AssertViewBindings(factoryEverywhere, factoryEditor);
    }

    [AvaloniaFact]
    public void StagedMigrationPath_PreservesPresentationReplacementAcrossLegacyAndFactoryViews()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var presentation = new AsterGraphPresentationOptions
        {
            NodeVisualPresenter = new RecordingNodeVisualPresenter(),
            ContextMenuPresenter = new RecordingContextMenuPresenter(),
            InspectorPresenter = new RecordingInspectorPresenter(),
            MiniMapPresenter = new RecordingMiniMapPresenter(),
        };

        var legacyView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = presentation,
        };
        var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
            Presentation = presentation,
        });

        AssertPresentationBindings(legacyView, presentation);
        AssertPresentationBindings(factoryView, presentation);
    }

    [AvaloniaFact]
    public void LegacyAndFactoryStandaloneCanvasPaths_AttachEquivalentPlatformSeams()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var legacyCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = legacyEditor,
        });
        var factoryCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = factoryEditor,
        });
        var legacyWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = legacyCanvas,
        };
        var factoryWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = factoryCanvas,
        };
        legacyWindow.Show();
        factoryWindow.Show();

        try
        {
            Assert.True(legacyEditor.CanPaste);
            Assert.True(factoryEditor.CanPaste);

            Assert.NotNull(legacyEditor.HostContext);
            Assert.NotNull(factoryEditor.HostContext);

            Assert.True(legacyEditor.HostContext!.TryGetOwner<NodeCanvas>(out var legacyOwner));
            Assert.True(factoryEditor.HostContext!.TryGetOwner<NodeCanvas>(out var factoryOwner));
            Assert.Same(legacyCanvas, legacyOwner);
            Assert.Same(factoryCanvas, factoryOwner);

            Assert.True(legacyEditor.HostContext.TryGetTopLevel<Window>(out var legacyTopLevel));
            Assert.True(factoryEditor.HostContext.TryGetTopLevel<Window>(out var factoryTopLevel));
            Assert.Same(legacyWindow, legacyTopLevel);
            Assert.Same(factoryWindow, factoryTopLevel);
        }
        finally
        {
            legacyWindow.Close();
            factoryWindow.Close();
        }
    }

    [AvaloniaFact]
    public void LegacyAndFactoryFullShellPaths_AttachEquivalentPlatformSeams()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var legacyView = new GraphEditorView
        {
            Editor = legacyEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        };
        var factoryView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = factoryEditor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var legacyWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = legacyView,
        };
        var factoryWindow = new Window
        {
            Width = 1440,
            Height = 900,
            Content = factoryView,
        };
        legacyWindow.Show();
        factoryWindow.Show();

        try
        {
            var legacyCanvas = legacyView.FindControl<NodeCanvas>("PART_NodeCanvas");
            var factoryCanvas = factoryView.FindControl<NodeCanvas>("PART_NodeCanvas");

            Assert.NotNull(legacyCanvas);
            Assert.NotNull(factoryCanvas);
            Assert.False(legacyCanvas.AttachPlatformSeams);
            Assert.False(factoryCanvas.AttachPlatformSeams);
            Assert.True(legacyEditor.CanPaste);
            Assert.True(factoryEditor.CanPaste);
            Assert.NotNull(legacyEditor.HostContext);
            Assert.NotNull(factoryEditor.HostContext);
            Assert.True(legacyEditor.HostContext!.TryGetOwner<GraphEditorView>(out var legacyOwner));
            Assert.True(factoryEditor.HostContext!.TryGetOwner<GraphEditorView>(out var factoryOwner));
            Assert.Same(legacyView, legacyOwner);
            Assert.Same(factoryView, factoryOwner);
            Assert.True(legacyEditor.HostContext.TryGetTopLevel<Window>(out var legacyTopLevel));
            Assert.True(factoryEditor.HostContext.TryGetTopLevel<Window>(out var factoryTopLevel));
            Assert.Same(legacyWindow, legacyTopLevel);
            Assert.Same(factoryWindow, factoryTopLevel);
        }
        finally
        {
            legacyWindow.Close();
            factoryWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DirectAndFactoryFullShellRoutes_ExposeEquivalentMigrationViewSnapshots()
    {
        var harness = CreateHarness();
        var legacyDirectSnapshot = CaptureViewRouteSnapshot(new GraphEditorView
        {
            Editor = CreateLegacyEditor(harness),
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var factoryDirectSnapshot = CaptureViewRouteSnapshot(new GraphEditorView
        {
            Editor = CreateFactoryEditor(harness),
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var legacyFactorySnapshot = CaptureViewRouteSnapshot(AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = CreateLegacyEditor(harness),
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        }));
        var factoryFactorySnapshot = CaptureViewRouteSnapshot(AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = CreateFactoryEditor(harness),
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        }));

        Assert.Equal(legacyDirectSnapshot, factoryDirectSnapshot);
        Assert.Equal(legacyDirectSnapshot, legacyFactorySnapshot);
        Assert.Equal(legacyDirectSnapshot, factoryFactorySnapshot);
    }

    [AvaloniaFact]
    public void LegacyAndFactoryStandaloneCanvasRoutes_ExposeEquivalentMigrationSurfaceSnapshots()
    {
        var harness = CreateHarness();
        var legacySnapshot = CaptureStandaloneCanvasSnapshot(AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = CreateLegacyEditor(harness),
        }));
        var factorySnapshot = CaptureStandaloneCanvasSnapshot(AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = CreateFactoryEditor(harness),
        }));

        Assert.Equal(legacySnapshot, factorySnapshot);
    }

    [Fact]
    public async Task RuntimeSession_ServiceSeamsAndCompatibilityService_RemainAvailableAcrossMigrationPaths()
    {
        var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var factorySession = CreateFactorySession(harness);
        var commandIds = new List<string>();

        factorySession.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        SelectSourceNode(legacyEditor);
        SelectSourceNode(factoryEditor);

        await legacyEditor.CopySelectionAsync();
        await factoryEditor.CopySelectionAsync();
        legacyEditor.ExportSelectionFragment();
        factoryEditor.ExportSelectionFragment();
        factorySession.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        factorySession.Commands.SetNodePositions(
            [
                new NodePositionSnapshot(SourceNodeId, new GraphPoint(180, 180)),
                new NodePositionSnapshot(TargetNodeId, new GraphPoint(480, 180)),
            ],
            updateStatus: false);
        factorySession.Commands.StartConnection(SourceNodeId, SourcePortId);
        factorySession.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        factorySession.Commands.CenterViewOnNode(TargetNodeId);
        factorySession.Commands.SaveWorkspace();

        var legacySelection = legacyEditor.Session.Queries.GetSelectionSnapshot();
        var factorySelection = factoryEditor.Session.Queries.GetSelectionSnapshot();
        var legacyCompatible = legacyEditor.Session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
        var factoryCompatible = factoryEditor.Session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
        var sessionCompatible = factorySession.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
        var sessionSelection = factorySession.Queries.GetSelectionSnapshot();

        Assert.IsAssignableFrom<IGraphEditorSession>(legacyEditor.Session);
        Assert.IsAssignableFrom<IGraphEditorSession>(factoryEditor.Session);
        Assert.Equal(legacySelection.SelectedNodeIds, factorySelection.SelectedNodeIds);
        Assert.Single(legacyCompatible);
        Assert.Single(factoryCompatible);
        Assert.Single(sessionCompatible);
        Assert.Equal(TargetNodeId, legacyCompatible[0].NodeId);
        Assert.Equal(TargetNodeId, factoryCompatible[0].NodeId);
        Assert.Equal(TargetNodeId, sessionCompatible[0].NodeId);
        Assert.Equal([SourceNodeId], sessionSelection.SelectedNodeIds);
        Assert.Equal(1, harness.WorkspaceService.SaveCalls);
        Assert.Equal(2, harness.FragmentWorkspaceService.SaveCalls);
        Assert.Equal(2, harness.ClipboardPayloadSerializer.SerializeCalls);
        Assert.Contains("selection.set", commandIds);
        Assert.Contains("nodes.move", commandIds);
        Assert.Contains("connections.begin", commandIds);
        Assert.Contains("connections.complete", commandIds);
        Assert.Contains("viewport.center-node", commandIds);
        Assert.Contains("workspace.save", commandIds);
        Assert.True(harness.CompatibilityService.EvaluateCalls > 0);

        factorySession.Commands.AddNode(TargetDefinitionId, new GraphPoint(640, 220));

        Assert.Contains("nodes.add", commandIds);
        Assert.Single(factorySession.Queries.CreateDocumentSnapshot().Connections);
        Assert.Equal(3, factorySession.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [Fact]
    public void DiagnosticsSurface_RemainsReachableAcrossLegacyFactoryAndSessionMigrationPaths()
    {
        using var harness = CreateHarness();
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var factorySession = CreateFactorySession(harness);

        SelectSourceNode(legacyEditor);
        SelectSourceNode(factoryEditor);
        legacyEditor.StartConnection(SourceNodeId, SourcePortId);
        factoryEditor.StartConnection(SourceNodeId, SourcePortId);
        legacyEditor.ExportSelectionFragment();
        factoryEditor.ExportSelectionFragment();
        legacyEditor.Session.Commands.SaveWorkspace();
        factoryEditor.Session.Commands.SaveWorkspace();
        factorySession.Commands.SaveWorkspace();

        var legacyInspection = legacyEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var factoryInspection = factoryEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        var sessionInspection = factorySession.Diagnostics.CaptureInspectionSnapshot();
        var legacyRecent = legacyEditor.Session.Diagnostics.GetRecentDiagnostics(10);
        var factoryRecent = factoryEditor.Session.Diagnostics.GetRecentDiagnostics(10);
        var sessionRecent = factorySession.Diagnostics.GetRecentDiagnostics(10);

        Assert.Equal(SourceNodeId, legacyInspection.Selection.PrimarySelectedNodeId);
        Assert.Equal(SourceNodeId, factoryInspection.Selection.PrimarySelectedNodeId);
        Assert.Null(sessionInspection.Selection.PrimarySelectedNodeId);
        Assert.True(legacyInspection.PendingConnection.HasPendingConnection);
        Assert.True(factoryInspection.PendingConnection.HasPendingConnection);
        Assert.False(sessionInspection.PendingConnection.HasPendingConnection);
        Assert.Equal("workspace.save.succeeded", legacyInspection.RecentDiagnostics[^1].Code);
        Assert.Equal("workspace.save.succeeded", factoryInspection.RecentDiagnostics[^1].Code);
        Assert.Equal("workspace.save.succeeded", sessionInspection.RecentDiagnostics[^1].Code);
        Assert.Equal("Saved snapshot to workspace://migration.", legacyInspection.Status.Message);
        Assert.Collection(
            legacyRecent,
            diagnostic => Assert.Equal("fragment.export.succeeded", diagnostic.Code),
            diagnostic => Assert.Equal("workspace.save.succeeded", diagnostic.Code));
        Assert.Collection(
            factoryRecent,
            diagnostic => Assert.Equal("fragment.export.succeeded", diagnostic.Code),
            diagnostic => Assert.Equal("workspace.save.succeeded", diagnostic.Code));
        Assert.Collection(
            sessionRecent,
            diagnostic => Assert.Equal("workspace.save.succeeded", diagnostic.Code));
        Assert.Equal(5, harness.DiagnosticsSink.Diagnostics.Count);
        Assert.All(
            harness.DiagnosticsSink.Diagnostics,
            diagnostic => Assert.Equal(GraphEditorDiagnosticSeverity.Info, diagnostic.Severity));
    }

    [Fact]
    public void DiagnosticsInstrumentation_RemainsOptInForCanonicalFactoryAndSessionPaths_WhileLegacyPathsKeepDiagnosticsAccess()
    {
        using var harness = CreateHarness(enableInstrumentation: true);
        var legacyEditor = CreateLegacyEditor(harness);
        var factoryEditor = CreateFactoryEditor(harness);
        var factorySession = CreateFactorySession(harness);

        _ = legacyEditor.Session.Diagnostics.CaptureInspectionSnapshot();
        _ = factoryEditor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(120, 80)));
        factorySession.Commands.SaveWorkspace();

        Assert.Contains(harness.LoggerFactory!.Entries, entry => entry.Level == LogLevel.Error && entry.Message.Contains("contextmenu.augment.failed", StringComparison.Ordinal));
        Assert.Contains(harness.LoggerFactory.Entries, entry => entry.Level == LogLevel.Information && entry.Message.Contains("workspace.save.succeeded", StringComparison.Ordinal));
        Assert.Contains("contextmenu.augment", harness.ActivityOperations ?? []);
        Assert.Contains("workspace.save", harness.ActivityOperations ?? []);
        Assert.Equal(2, harness.DiagnosticsSink.Diagnostics.Count);
        Assert.Equal("workspace.save.succeeded", harness.DiagnosticsSink.Diagnostics[^1].Code);
        Assert.Empty(legacyEditor.Session.Diagnostics.GetRecentDiagnostics());
    }

    private static void AssertViewBindings(GraphEditorView view, GraphEditorViewModel expectedEditor)
    {
        Assert.Same(expectedEditor, view.Editor);
        Assert.Equal(GraphEditorViewChromeMode.CanvasOnly, view.ChromeMode);
    }

    private static void AssertPresentationBindings(GraphEditorView view, AsterGraphPresentationOptions presentation)
    {
        var canvas = view.FindControl<NodeCanvas>("PART_NodeCanvas");
        var inspector = view.FindControl<GraphInspectorView>("PART_InspectorSurface");
        var miniMap = view.FindControl<GraphMiniMap>("PART_MiniMapSurface");

        Assert.NotNull(canvas);
        Assert.NotNull(inspector);
        Assert.NotNull(miniMap);
        Assert.Same(presentation, view.Presentation);
        Assert.Same(presentation.NodeVisualPresenter, canvas.NodeVisualPresenter);
        Assert.Same(presentation.ContextMenuPresenter, canvas.ContextMenuPresenter);
        Assert.Same(presentation.InspectorPresenter, inspector.InspectorPresenter);
        Assert.Same(presentation.MiniMapPresenter, miniMap.MiniMapPresenter);
    }

    private static ViewRouteSnapshot CaptureViewRouteSnapshot(GraphEditorView view)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = view,
        };
        window.Show();

        try
        {
            var canvas = view.FindControl<NodeCanvas>("PART_NodeCanvas");
            var inspector = view.FindControl<GraphInspectorView>("PART_InspectorSurface");
            var miniMap = view.FindControl<GraphMiniMap>("PART_MiniMapSurface");
            var editor = Assert.IsType<GraphEditorViewModel>(view.Editor);

            Assert.NotNull(canvas);
            Assert.NotNull(inspector);
            Assert.NotNull(miniMap);
            Assert.NotNull(editor.HostContext);
            Assert.True(editor.HostContext!.TryGetOwner<GraphEditorView>(out var owner));
            Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));

            return new ViewRouteSnapshot(
                view.ChromeMode,
                view.EnableDefaultContextMenu,
                view.CommandShortcutPolicy.Enabled,
                canvas.EnableDefaultContextMenu,
                canvas.CommandShortcutPolicy.Enabled,
                canvas.AttachPlatformSeams,
                ReferenceEquals(editor, inspector.Editor),
                ReferenceEquals(editor.Session, miniMap.Session),
                owner?.GetType().Name ?? "<none>",
                topLevel?.GetType().Name ?? "<none>");
        }
        finally
        {
            window.Close();
        }
    }

    private static StandaloneCanvasRouteSnapshot CaptureStandaloneCanvasSnapshot(NodeCanvas canvas)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = canvas,
        };
        window.Show();

        try
        {
            var editor = Assert.IsType<GraphEditorViewModel>(canvas.ViewModel);

            Assert.NotNull(editor.HostContext);
            Assert.True(editor.HostContext!.TryGetOwner<NodeCanvas>(out var owner));
            Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));

            return new StandaloneCanvasRouteSnapshot(
                canvas.EnableDefaultContextMenu,
                canvas.CommandShortcutPolicy.Enabled,
                canvas.AttachPlatformSeams,
                ReferenceEquals(editor, canvas.ViewModel),
                owner?.GetType().Name ?? "<none>",
                topLevel?.GetType().Name ?? "<none>");
        }
        finally
        {
            window.Close();
        }
    }

    private static void SelectSourceNode(GraphEditorViewModel editor)
        => editor.SelectSingleNode(Assert.Single(editor.Nodes, node => node.Id == SourceNodeId), updateStatus: false);

    private static EditorParitySnapshot CaptureSnapshot(GraphEditorViewModel editor)
    {
        var menuSignature = string.Join(
            "|",
            editor.BuildContextMenu(CreateMenuContext()).Select(item => item.Id));
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == SourceNodeId);

        return new EditorParitySnapshot(
            editor.Title,
            editor.Description,
            editor.StyleOptions.Shell.HighlightHex,
            editor.BehaviorOptions.View.ShowMiniMap,
            editor.StatsCaption,
            sourceNode.DisplaySubtitle,
            sourceNode.DisplayDescription,
            menuSignature,
            editor.WorkspacePath,
            editor.FragmentPath,
            editor.FragmentLibraryPath,
            editor.Session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId).Count);
    }

    private static ContextMenuContext CreateMenuContext()
        => new(
            ContextMenuTargetKind.Canvas,
            new GraphPoint(320, 180));

    private static GraphEditorViewModel CreateLegacyEditor(MigrationHarness harness)
        => new(
            harness.Document,
            harness.Catalog,
            harness.CompatibilityService,
            harness.WorkspaceService,
            harness.FragmentWorkspaceService,
            harness.StyleOptions,
            harness.BehaviorOptions,
            harness.FragmentLibraryService,
            harness.MenuAugmentor,
            harness.PresentationProvider,
            harness.LocalizationProvider,
            harness.ClipboardPayloadSerializer,
            harness.DiagnosticsSink);

    private static GraphEditorViewModel CreateFactoryEditor(MigrationHarness harness)
        => AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = harness.Document,
            NodeCatalog = harness.Catalog,
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            StyleOptions = harness.StyleOptions,
            BehaviorOptions = harness.BehaviorOptions,
            FragmentLibraryService = harness.FragmentLibraryService,
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            ContextMenuAugmentor = harness.MenuAugmentor,
            NodePresentationProvider = harness.PresentationProvider,
            LocalizationProvider = harness.LocalizationProvider,
            DiagnosticsSink = harness.DiagnosticsSink,
            Instrumentation = harness.Instrumentation,
        });

    private static IGraphEditorSession CreateFactorySession(MigrationHarness harness)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = harness.Document,
            NodeCatalog = harness.Catalog,
            CompatibilityService = harness.CompatibilityService,
            WorkspaceService = harness.WorkspaceService,
            FragmentWorkspaceService = harness.FragmentWorkspaceService,
            StyleOptions = harness.StyleOptions,
            BehaviorOptions = harness.BehaviorOptions,
            FragmentLibraryService = harness.FragmentLibraryService,
            ClipboardPayloadSerializer = harness.ClipboardPayloadSerializer,
            ContextMenuAugmentor = harness.MenuAugmentor,
            NodePresentationProvider = harness.PresentationProvider,
            LocalizationProvider = harness.LocalizationProvider,
            DiagnosticsSink = harness.DiagnosticsSink,
            Instrumentation = harness.Instrumentation,
        });

    private static MigrationHarness CreateHarness(bool enableInstrumentation = false)
    {
        var compatibility = new RecordingCompatibilityService();
        var diagnosticsSink = new RecordingDiagnosticsSink();
        RecordingLoggerFactory? loggerFactory = null;
        ActivitySource? activitySource = null;
        List<string>? activityOperations = null;
        ActivityListener? activityListener = null;
        GraphEditorInstrumentationOptions? instrumentation = null;

        if (enableInstrumentation)
        {
            loggerFactory = new RecordingLoggerFactory();
            activitySource = new ActivitySource("AsterGraph.Tests.MigrationCompatibility");
            activityOperations = [];
            activityListener = CreateListener(activitySource.Name, activityOperations);
            instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource);
        }

        return new MigrationHarness(
            CreateDocument(),
            CreateCatalog(),
            compatibility,
            new RecordingWorkspaceService("workspace://migration"),
            new RecordingFragmentWorkspaceService("fragment://migration"),
            GraphEditorStyleOptions.Default with
            {
                Shell = GraphEditorStyleOptions.Default.Shell with
                {
                    HighlightHex = "#F3B36B",
                },
            },
            GraphEditorBehaviorOptions.Default with
            {
                View = GraphEditorBehaviorOptions.Default.View with
                {
                    ShowMiniMap = false,
                },
            },
            new RecordingFragmentLibraryService("library://migration"),
            new RecordingClipboardPayloadSerializer(),
            diagnosticsSink,
            new MigrationContextMenuAugmentor(enableThrow: enableInstrumentation),
            new MigrationNodePresentationProvider(),
            new MigrationLocalizationProvider(),
            instrumentation,
            loggerFactory,
            activitySource,
            activityOperations,
            activityListener);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Migration Graph",
            "Compatibility regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Migration Source",
                    "Tests",
                    "Compatibility",
                    "Migration parity source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort(
                            SourcePortId,
                            "Output",
                            PortDirection.Output,
                            "float",
                            "#6AD5C4",
                            new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    SourceDefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Migration Target",
                    "Tests",
                    "Compatibility",
                    "Migration parity target node.",
                    new GraphPoint(420, 160),
                    new GraphSize(240, 160),
                    [
                        new GraphPort(
                            TargetPortId,
                            "Input",
                            PortDirection.Input,
                            "float",
                            "#F3B36B",
                            new PortTypeId("float")),
                    ],
                    [],
                    "#F3B36B",
                    TargetDefinitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Migration Source",
            "Tests",
            "Compatibility",
            [],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
            ]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Migration Target",
            "Tests",
            "Compatibility",
            [
                new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
            ],
            []));
        return catalog;
    }

    private sealed record MigrationHarness(
        GraphDocument Document,
        NodeCatalog Catalog,
        RecordingCompatibilityService CompatibilityService,
        RecordingWorkspaceService WorkspaceService,
        RecordingFragmentWorkspaceService FragmentWorkspaceService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions,
        RecordingFragmentLibraryService FragmentLibraryService,
        RecordingClipboardPayloadSerializer ClipboardPayloadSerializer,
        RecordingDiagnosticsSink DiagnosticsSink,
        MigrationContextMenuAugmentor MenuAugmentor,
        MigrationNodePresentationProvider PresentationProvider,
        MigrationLocalizationProvider LocalizationProvider,
        GraphEditorInstrumentationOptions? Instrumentation,
        RecordingLoggerFactory? LoggerFactory,
        ActivitySource? ActivitySource,
        List<string>? ActivityOperations,
        ActivityListener? ActivityListener)
        : IDisposable
    {
        public void Dispose()
        {
            ActivityListener?.Dispose();
            ActivitySource?.Dispose();
            LoggerFactory?.Dispose();
        }
    }


    private sealed record EditorParitySnapshot(
        string Title,
        string Description,
        string HighlightHex,
        bool ShowMiniMap,
        string StatsCaption,
        string NodeSubtitle,
        string NodeDescription,
        string MenuSignature,
        string WorkspacePath,
        string FragmentPath,
        string FragmentLibraryPath,
        int CompatibleTargetCount);

    private sealed record ViewRouteSnapshot(
        GraphEditorViewChromeMode ChromeMode,
        bool ViewDefaultContextMenu,
        bool ViewDefaultCommandShortcuts,
        bool CanvasDefaultContextMenu,
        bool CanvasDefaultCommandShortcuts,
        bool CanvasAttachPlatformSeams,
        bool InspectorBound,
        bool MiniMapBound,
        string HostOwnerType,
        string TopLevelType);

    private sealed record StandaloneCanvasRouteSnapshot(
        bool CanvasDefaultContextMenu,
        bool CanvasDefaultCommandShortcuts,
        bool CanvasAttachPlatformSeams,
        bool ViewModelBound,
        string HostOwnerType,
        string TopLevelType);

    private sealed class MigrationContextMenuAugmentor(bool enableThrow = false) : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
        {
            if (enableThrow)
            {
                throw new InvalidOperationException("migration augmentor exploded");
            }

            return [.. stockItems, new MenuItemDescriptor("tests.migration.host-action", "Host Action")];
        }
    }

    private sealed class MigrationNodePresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => new(
                SubtitleOverride: "Migration subtitle",
                DescriptionOverride: "Migration description");
    }

    private sealed class MigrationLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.stats.caption"
                ? "Localized stats {0}/{1}/{2:0}"
                : fallback;
    }

    private sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = workspacePath;

        public int SaveCalls { get; private set; }

        public GraphDocument? LastSaved { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            LastSaved = document;
        }

        public GraphDocument Load()
            => LastSaved ?? throw new InvalidOperationException("No saved workspace.");

        public bool Exists()
            => LastSaved is not null;
    }

    private sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
    {
        public string FragmentPath { get; } = fragmentPath;

        public int SaveCalls { get; private set; }

        public GraphSelectionFragment? LastSaved { get; private set; }

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
            SaveCalls++;
            LastSaved = fragment;
        }

        public GraphSelectionFragment Load(string? path = null)
            => LastSaved ?? throw new InvalidOperationException("No saved fragment.");

        public bool Exists(string? path = null)
            => LastSaved is not null;

        public void Delete(string? path = null)
            => LastSaved = null;
    }

    private sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
    {
        public string LibraryPath { get; } = libraryPath;

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => Path.Combine(LibraryPath, $"{name ?? "fragment"}.json");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new NotSupportedException();

        public void DeleteTemplate(string path)
        {
        }
    }

    private sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
    {
        public int SerializeCalls { get; private set; }

        public string Serialize(GraphSelectionFragment fragment)
        {
            SerializeCalls++;
            return "serialized-fragment";
        }

        public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        {
            fragment = null;
            return false;
        }
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class RecordingCompatibilityService : IPortCompatibilityService
    {
        public int EvaluateCalls { get; private set; }

        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        {
            EvaluateCalls++;
            return sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
        }
    }

    private sealed class RecordingNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        private readonly DefaultGraphNodeVisualPresenter _stockPresenter = new();

        public GraphNodeVisual Create(GraphNodeVisualContext context)
            => _stockPresenter.Create(context);

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
            => _stockPresenter.Update(visual, context);
    }

    private sealed class RecordingContextMenuPresenter : IGraphContextMenuPresenter
    {
        public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
            => throw new NotSupportedException();
    }

    private static ActivityListener CreateListener(string sourceName, List<string> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => activities.Add(activity.OperationName),
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName)
            => new RecordingLogger(categoryName, Entries);

        public void AddProvider(ILoggerProvider provider)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger(string categoryName, List<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(new LogEntry(categoryName, logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }

    private sealed class RecordingInspectorPresenter : IGraphInspectorPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
            => new TextBlock
            {
                Text = $"MIGRATION INSPECTOR:{editor?.InspectorTitle ?? "<none>"}",
            };
    }

    private sealed class RecordingMiniMapPresenter : IGraphMiniMapPresenter
    {
        public Control Create(IGraphEditorSession? session)
            => new TextBlock
            {
                Text = $"MIGRATION MINIMAP:{session?.Queries.CreateDocumentSnapshot().Title ?? "<none>"}",
            };
    }
}
