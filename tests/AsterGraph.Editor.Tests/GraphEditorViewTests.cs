using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(AsterGraph.Editor.Tests.GraphEditorViewTestsAppBuilder))]

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorViewTests
{
    [AvaloniaFact]
    public void DefaultChromeMode_KeepsAllChromeSectionsVisible()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.Equal(GraphEditorViewChromeMode.Default, view.ChromeMode);
        Assert.True(view.IsHeaderChromeVisible);
        Assert.True(view.IsLibraryChromeVisible);
        Assert.True(view.IsInspectorChromeVisible);
        Assert.True(view.IsStatusChromeVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.True(FindRequiredControl<Grid>(view, "PART_ShellGrid").ColumnSpacing > 0);
        Assert.True(FindRequiredControl<Grid>(view, "PART_ShellGrid").RowSpacing > 0);
    }

    [AvaloniaFact]
    public void DefaultChromeMode_UsesSeparatedHeaderBadgesAndWrappingToolbar()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var header = FindRequiredControl<Border>(view, "PART_HeaderChrome");
        var badgeStack = FindRequiredControl<StackPanel>(view, "PART_HeaderBadges");
        var toolbar = FindRequiredControl<WrapPanel>(view, "PART_HeaderToolbar");

        Assert.Equal(new Thickness(20), header.Padding);
        Assert.Equal(Orientation.Vertical, badgeStack.Orientation);
        Assert.Equal(Orientation.Horizontal, toolbar.Orientation);
        Assert.Equal(40, toolbar.ItemHeight);
        Assert.Equal(120, toolbar.ItemWidth);
        Assert.True(toolbar.Children.Count >= 7);
    }

    [AvaloniaFact]
    public void DefaultChromeMode_ExposesHostedAccessibilityBaselineSemantics()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");
        var stencilSearchBox = FindRequiredControl<TextBox>(view, "PART_StencilSearchBox");
        var paletteToggle = FindRequiredControl<Button>(view, "PART_OpenCommandPaletteButton");
        var paletteSearchBox = FindRequiredControl<TextBox>(view, "PART_CommandPaletteSearchBox");
        var inspector = FindRequiredControl<GraphInspectorView>(view, "PART_InspectorSurface");
        var parameterSearchBox = FindRequiredDescendant<TextBox>(view, "PART_ParameterSearchBox");

        var nodeSurface = canvas.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(control =>
                control.Focusable
                && AutomationProperties.GetName(control)?.EndsWith(" node", StringComparison.Ordinal) == true);

        Assert.True(view.Focusable);
        Assert.Equal("Graph editor host", AutomationProperties.GetName(view));

        Assert.True(canvas.Focusable);
        Assert.Equal("Graph canvas", AutomationProperties.GetName(canvas));

        Assert.Equal("Stencil search", AutomationProperties.GetName(stencilSearchBox));
        Assert.Equal("Open command palette", AutomationProperties.GetName(paletteToggle));
        Assert.Equal("Command palette search", AutomationProperties.GetName(paletteSearchBox));

        Assert.Equal("Graph inspector", AutomationProperties.GetName(inspector));
        Assert.Equal("Inspector parameter search", AutomationProperties.GetName(parameterSearchBox));

        Assert.NotNull(nodeSurface);
    }

    [AvaloniaFact]
    public void HeaderCommandSurface_UsesSharedDescriptorsForToolbarAndPalette()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var saveButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_workspace.save");
        var undoButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_history.undo");
        var fitSelectionButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_viewport.fit-selection");
        var deleteButton = FindRequiredDescendant<Button>(view, "PART_HeaderCommand_selection.delete");
        var paletteToggle = FindRequiredControl<Button>(view, "PART_OpenCommandPaletteButton");
        var registryHeaderIds = editor.Session.Queries.GetCommandRegistry()
            .SelectMany(entry => entry.Placements
                .Where(placement =>
                    placement.SurfaceKind == GraphEditorCommandSurfaceKind.Workbench
                    && string.Equals(placement.SurfaceId, "workbench.header", StringComparison.Ordinal))
                .Select(_ => entry.CommandId))
            .ToHashSet(StringComparer.Ordinal);
        var headerActionIds = FindRequiredControl<WrapPanel>(view, "PART_HeaderToolbar")
            .Children
            .OfType<Button>()
            .Select(button => button.Name!["PART_HeaderCommand_".Length..])
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal("Save Workspace", Assert.IsType<string>(saveButton.Content));
        Assert.Equal("Undo", Assert.IsType<string>(undoButton.Content));
        Assert.Equal("Fit Selection", Assert.IsType<string>(fitSelectionButton.Content));
        Assert.True(registryHeaderIds.SetEquals(headerActionIds));
        Assert.Equal("Save Workspace", AutomationProperties.GetName(saveButton));
        Assert.Equal("Undo", AutomationProperties.GetName(undoButton));
        Assert.Equal("Ctrl+S", ToolTip.GetTip(saveButton));
        Assert.Contains("Select one or more nodes before fitting the selection.", Assert.IsType<string>(ToolTip.GetTip(fitSelectionButton)), StringComparison.Ordinal);
        Assert.Contains("Select one or more nodes before deleting.", Assert.IsType<string>(ToolTip.GetTip(deleteButton)), StringComparison.Ordinal);

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");
        var paletteItems = FindRequiredControl<StackPanel>(view, "PART_CommandPaletteItems");

        Assert.True(paletteChrome.IsVisible);
        var paletteSaveButton = Assert.Single(
            paletteItems.Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteAction_workspace.save", StringComparison.Ordinal));
        var paletteUndoButton = Assert.Single(
            paletteItems.Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteAction_history.undo", StringComparison.Ordinal));
        var paletteFitSelectionButton = Assert.Single(
            paletteItems.Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteAction_viewport.fit-selection", StringComparison.Ordinal));
        var workspaceGroup = Assert.Single(
            paletteItems.Children.OfType<TextBlock>(),
            text => string.Equals(text.Name, "PART_CommandPaletteGroup_workspace", StringComparison.Ordinal));
        var viewportGroup = Assert.Single(
            paletteItems.Children.OfType<TextBlock>(),
            text => string.Equals(text.Name, "PART_CommandPaletteGroup_viewport", StringComparison.Ordinal));

        Assert.Equal("Save Workspace", AutomationProperties.GetName(paletteSaveButton));
        Assert.Equal("Undo", AutomationProperties.GetName(paletteUndoButton));
        Assert.Equal("workspace", workspaceGroup.Text);
        Assert.Equal("viewport", viewportGroup.Text);
        Assert.Equal("Select one or more nodes before fitting the selection.", ToolTip.GetTip(paletteFitSelectionButton));
    }

    [AvaloniaFact]
    public void ValidationFeedbackChrome_ProjectsCanonicalSnapshotAndFocusAction()
    {
        var editor = CreateValidationFeedbackEditor();
        editor.Session.Commands.UpdateViewportSize(800, 600);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var status = FindRequiredControl<TextBlock>(view, "PART_ValidationStatusText");
        var statusBar = FindRequiredControl<TextBlock>(view, "PART_StatusValidationText");
        var feedbackList = FindRequiredControl<StackPanel>(view, "PART_ValidationFeedbackList");
        var problemRow = FindRequiredDescendant<Button>(
            view,
            "PART_ProblemsIssue_connection-001_connection.incompatible-endpoint-types_in");
        var focusButton = FindRequiredDescendant<Button>(
            view,
            "PART_ValidationFocus_connection-001_connection.incompatible-endpoint-types_in");

        Assert.Contains("1 error", status.Text, StringComparison.Ordinal);
        Assert.Contains("1 error", statusBar.Text, StringComparison.Ordinal);
        Assert.Contains(
            feedbackList.GetVisualDescendants().OfType<TextBlock>(),
            text => text.Text?.Contains("connection.incompatible-endpoint-types", StringComparison.Ordinal) == true);
        Assert.Equal(
            "Problem connection.incompatible-endpoint-types",
            AutomationProperties.GetName(problemRow));

        problemRow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var rowSelection = editor.Session.Queries.GetSelectionSnapshot();
        Assert.Equal(["connection-001"], rowSelection.SelectedConnectionIds);
        Assert.Equal("connection-001", rowSelection.PrimarySelectedConnectionId);

        editor.Session.Commands.ClearSelection(updateStatus: false);
        focusButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var selection = editor.Session.Queries.GetSelectionSnapshot();
        Assert.Equal(["connection-001"], selection.SelectedConnectionIds);
        Assert.Equal("connection-001", selection.PrimarySelectedConnectionId);
    }

    [AvaloniaFact]
    public void ProblemsPanel_DoubleClickFocusesNodeAndReusesInspectorSelection()
    {
        var editor = CreateValidationParameterFeedbackEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var problemRow = FindRequiredDescendant<Button>(
            view,
            "PART_ProblemsIssue_parameter-001_node.parameter-invalid_prompt");
        var pointer = new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true);
        var pointerArgs = CreatePointerPressedArgs(problemRow, view, pointer, new Point(0, 0), KeyModifiers.None);

        problemRow.RaiseEvent(new TappedEventArgs(InputElement.DoubleTappedEvent, pointerArgs)
        {
            Source = problemRow,
        });

        var selection = editor.Session.Queries.GetSelectionSnapshot();
        Assert.Equal(["parameter-001"], selection.SelectedNodeIds);
        Assert.Equal("parameter-001", selection.PrimarySelectedNodeId);
        Assert.Equal("Validation Parameters", editor.InspectorTitle);
        Assert.True(view.IsInspectorChromeVisible);
    }

    [AvaloniaFact]
    public void ProblemsPanel_ProjectsSameHelpTargetAsInspector()
    {
        var editor = CreateValidationParameterFeedbackEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var problemRow = FindRequiredDescendant<Button>(
            view,
            "PART_ProblemsIssue_parameter-001_node.parameter-invalid_prompt");
        var issue = editor.Session.Queries.GetValidationSnapshot().Issues.Single(candidate =>
            string.Equals(candidate.ParameterKey, "prompt", StringComparison.Ordinal));

        Assert.NotNull(issue.HelpTarget);
        var rowHelpText = Assert.IsType<string>(ToolTip.GetTip(problemRow));
        Assert.Contains(issue.HelpTarget!.DisplayText, rowHelpText, StringComparison.Ordinal);
        Assert.Equal(issue.HelpTarget.DisplayText, AutomationProperties.GetHelpText(problemRow));

        problemRow.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var inspectorParameter = editor.Session.Queries.GetSelectedNodeParameterSnapshots().Single(parameter =>
            string.Equals(parameter.Definition.Key, issue.HelpTarget.ParameterKey, StringComparison.Ordinal));

        Assert.Equal(issue.HelpTarget.ParameterKey, inspectorParameter.Definition.Key);
        Assert.Equal(issue.HelpTarget.HelpText, inspectorParameter.HelpText);
    }

    [AvaloniaFact]
    public void ProblemsPanel_ExposesRepairPreviewAndApplyAffordance()
    {
        var editor = CreateValidationFeedbackEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var repairStatus = FindRequiredControl<TextBlock>(view, "PART_ProblemsRepairDiscoveryStatusText");
        var problemRow = FindRequiredDescendant<Button>(
            view,
            "PART_ProblemsIssue_connection-001_connection.incompatible-endpoint-types_in");

        Assert.True(repairStatus.IsVisible);
        Assert.Contains("Preview available", repairStatus.Text, StringComparison.Ordinal);

        Assert.NotNull(problemRow.ContextMenu);
        var repairItems = problemRow.ContextMenu.ItemsSource!.OfType<MenuItem>().ToArray();
        Assert.Contains(repairItems, item =>
            string.Equals(item.Header?.ToString(), "Remove invalid connection", StringComparison.Ordinal)
            && item.IsEnabled
            && Assert.IsType<string>(ToolTip.GetTip(item)).Contains("Remove invalid connection connection-001.", StringComparison.Ordinal));

        var removeItem = repairItems.Single(item => string.Equals(item.Name, "PART_ProblemsIssue_connection-001_connection.incompatible-endpoint-types_in_Repair_validation.connection.remove", StringComparison.Ordinal));
        removeItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        Assert.Empty(editor.Session.Queries.CreateDocumentSnapshot().Connections);
    }

    [AvaloniaFact]
    public void ValidationFeedbackChrome_UsesUniqueFocusNamesForIssuesOnSameNode()
    {
        var editor = CreateValidationParameterFeedbackEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var feedbackList = FindRequiredControl<StackPanel>(view, "PART_ValidationFeedbackList");

        var focusButtons = feedbackList.GetVisualDescendants()
            .OfType<Button>()
            .Where(button => button.Name?.StartsWith("PART_ValidationFocus_parameter-001_node.parameter-invalid_", StringComparison.Ordinal) == true)
            .ToList();

        Assert.Equal(2, focusButtons.Count);
        Assert.Equal(focusButtons.Count, focusButtons.Select(button => button.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(focusButtons, button => string.Equals(button.Name, "PART_ValidationFocus_parameter-001_node.parameter-invalid_prompt", StringComparison.Ordinal));
        Assert.Contains(focusButtons, button => string.Equals(button.Name, "PART_ValidationFocus_parameter-001_node.parameter-invalid_system", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void ShortcutHelp_AndKeyboardRouting_ProjectCommandPaletteFromSharedActionSource()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var shortcutHelp = FindRequiredControl<StackPanel>(view, "PART_ShortcutHelpList");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        Assert.Contains(
            shortcutHelp.Children.OfType<Border>()
                .Select(item => item.Child)
                .OfType<TextBlock>(),
            item => item.Text?.Contains("Ctrl+Shift+P", StringComparison.Ordinal) == true
                && item.Text.Contains("Command Palette", StringComparison.Ordinal));

        var args = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
        };

        InvokeViewKeyDown(view, args);

        Assert.True(args.Handled);
        Assert.True(paletteChrome.IsVisible);
    }

    [AvaloniaFact]
    public void CommandShortcutPolicy_OverridesCommandPaletteShortcutAndShortcutHelp()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            CommandShortcutPolicy = new AsterGraphCommandShortcutPolicy
            {
                ShortcutOverrides = new Dictionary<string, string?>
                {
                    ["shell.command-palette"] = "Ctrl+Alt+P",
                },
            },
        });
        var view = (GraphEditorView)window.Content!;
        var shortcutHelp = FindRequiredControl<StackPanel>(view, "PART_ShortcutHelpList");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        Assert.Contains(
            shortcutHelp.Children.OfType<Border>()
                .Select(item => item.Child)
                .OfType<TextBlock>(),
            item => item.Text?.Contains("Ctrl+Alt+P", StringComparison.Ordinal) == true
                && item.Text.Contains("Command Palette", StringComparison.Ordinal));

        var defaultArgs = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
        };

        InvokeViewKeyDown(view, defaultArgs);

        Assert.False(defaultArgs.Handled);
        Assert.False(paletteChrome.IsVisible);

        var overrideArgs = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Alt,
        };

        InvokeViewKeyDown(view, overrideArgs);

        Assert.True(overrideArgs.Handled);
        Assert.True(paletteChrome.IsVisible);
    }

    [AvaloniaFact]
    public void CommandPalette_RestoresFocusToPriorHostedSurface_WhenClosedWithEscape()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var nodeSurface = view.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(control =>
                control.Focusable
                && AutomationProperties.GetName(control)?.EndsWith(" node", StringComparison.Ordinal) == true);
        var stencilSearchBox = FindRequiredControl<TextBox>(view, "PART_StencilSearchBox");
        var parameterSearchBox = FindRequiredDescendant<TextBox>(view, "PART_ParameterSearchBox");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");
        var paletteSearchBox = FindRequiredControl<TextBox>(view, "PART_CommandPaletteSearchBox");

        Assert.NotNull(nodeSurface);

        AssertFocusRoundTrip(view, nodeSurface!, paletteChrome, paletteSearchBox);
        AssertFocusRoundTrip(view, stencilSearchBox, paletteChrome, paletteSearchBox);
        AssertFocusRoundTrip(view, parameterSearchBox, paletteChrome, paletteSearchBox);
    }

    [AvaloniaFact]
    public void CustomNodeBodyInputScope_SuppressesDestructiveShortcutButKeepsCommandPaletteShortcut()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            Presentation = new AsterGraphPresentationOptions
            {
                NodeBodyPresenter = new InputScopeNodeBodyPresenter(),
            },
        });
        var view = (GraphEditorView)window.Content!;
        var customEditor = FindRequiredDescendant<Button>(view, "PART_CustomBodyInputScopeEditor");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        try
        {
            AssertInputScopeShortcutBehavior(view, editor, customEditor, paletteChrome);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CustomNodeVisualInputScope_SuppressesDestructiveShortcutButKeepsCommandPaletteShortcut()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            Presentation = new AsterGraphPresentationOptions
            {
                NodeVisualPresenter = new InputScopeNodeVisualPresenter(),
            },
        });
        var view = (GraphEditorView)window.Content!;
        var customEditor = FindRequiredDescendant<Button>(view, "PART_CustomVisualInputScopeEditor");
        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");

        try
        {
            AssertInputScopeShortcutBehavior(view, editor, customEditor, paletteChrome);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void IndividualChromeVisibility_TogglesEachRegionIndependently()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            IsHeaderChromeVisible = false,
            IsLibraryChromeVisible = true,
            IsInspectorChromeVisible = false,
            IsStatusChromeVisible = true,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.False(view.IsHeaderChromeVisible);
        Assert.True(view.IsLibraryChromeVisible);
        Assert.False(view.IsInspectorChromeVisible);
        Assert.True(view.IsStatusChromeVisible);

        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.True(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.NotNull(FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas"));
    }

    [AvaloniaFact]
    public void CanvasOnlyChromeMode_HidesAllShellChromeWithoutLeavingShellSpacing()
    {
        var editor = CreateEditor();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;
        var shellGrid = FindRequiredControl<Grid>(view, "PART_ShellGrid");

        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
        Assert.Equal(0, shellGrid.ColumnSpacing);
        Assert.Equal(0, shellGrid.RowSpacing);
    }

    [AvaloniaFact]
    public void SwitchingToCanvasOnly_UpdatesVisibilityImmediatelyWithoutRebuildingEditorState()
    {
        var editor = CreateEditor();
        editor.AddNodeToSelection(editor.Nodes[0], updateStatus: false);
        var originalZoom = editor.Zoom;

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;

        Assert.Same(editor, view.Editor);
        Assert.Same(editor.Nodes[0], editor.SelectedNode);
        Assert.Equal(originalZoom, editor.Zoom);
        Assert.False(FindRequiredControl<Border>(view, "PART_HeaderChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_LibraryChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_InspectorChrome").IsVisible);
        Assert.False(FindRequiredControl<Border>(view, "PART_StatusChrome").IsVisible);
    }

    [AvaloniaFact]
    public void CanvasOnly_KeepsMenuChainAndHostContextAvailable()
    {
        var augmentor = new GraphEditorViewHostAwareAugmentor();
        var editor = CreateEditor(augmentor);

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;

        Assert.NotNull(editor.HostContext);
        Assert.True(editor.HostContext!.TryGetOwner<GraphEditorView>(out var owner));
        Assert.Same(view, owner);
        Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));
        Assert.Same(window, topLevel);

        var menu = editor.BuildContextMenu(
            new ContextMenuContext(
                ContextMenuTargetKind.Node,
                new GraphPoint(120, 160),
                selectedNodeId: "tests.view.node-001",
                selectedNodeIds: ["tests.view.node-001"],
                clickedNodeId: "tests.view.node-001",
                hostContext: editor.HostContext));
        var hostItem = Assert.Single(menu, item => item.Id == "tests-view-host-item");

        Assert.Equal("Host Item", hostItem.Header);
        Assert.True(augmentor.ReceivedHostOwner);
    }

    [AvaloniaFact]
    public void DirectGraphEditorView_WithFactoryCreatedEditor_PreservesCompatibilitySurfaceBehavior()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.factory"),
                "Factory View Node",
                "Tests",
                "Exercises direct GraphEditorView with a factory-created editor.",
                [],
                []));
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Factory View Graph",
                "Exercises direct GraphEditorView behavior with a factory-created editor.",
                [
                    new GraphNode(
                        "tests.view.factory-001",
                        "Factory View Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by GraphEditorView factory-created editor tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.factory")),
                ],
                []),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        });
        var view = (GraphEditorView)window.Content!;
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");

        Assert.Same(editor, view.Editor);
        Assert.False(canvas.AttachPlatformSeams);
        Assert.True(canvas.EnableDefaultContextMenu);
        Assert.True(canvas.CommandShortcutPolicy.Enabled);
        Assert.NotNull(editor.HostContext);
        Assert.True(editor.HostContext!.TryGetOwner<GraphEditorView>(out var owner));
        Assert.Same(view, owner);
        Assert.True(editor.HostContext.TryGetTopLevel<Window>(out var topLevel));
        Assert.Same(window, topLevel);
    }

    [AvaloniaFact]
    public void StencilLibrary_UsesSessionStencilItems_WhenRetainedTemplatesAreCleared()
    {
        var editor = CreateEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var stencilCard = FindRequiredDescendant<Button>(view, "PART_StencilCard_tests-view-node");

        Assert.True(stencilCard.IsEnabled);

        stencilCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(2, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [AvaloniaFact]
    public void StencilLibrary_GroupsSessionStencilItems_ByCategory_WhenRetainedTemplatesAreCleared()
    {
        var editor = CreateStencilEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var generatorsSection = FindRequiredDescendant<Expander>(view, "PART_StencilSection_Generators");
        var utilitiesSection = FindRequiredDescendant<Expander>(view, "PART_StencilSection_Utilities");

        Assert.True(generatorsSection.IsExpanded);
        Assert.True(utilitiesSection.IsExpanded);
        Assert.Contains(
            generatorsSection.GetVisualDescendants().OfType<Button>(),
            button => string.Equals(button.Name, "PART_StencilCard_tests-stencil-noise", StringComparison.Ordinal));
        Assert.Contains(
            utilitiesSection.GetVisualDescendants().OfType<Button>(),
            button => string.Equals(button.Name, "PART_StencilCard_tests-stencil-output", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void StencilLibrary_SearchFiltersGroupedStencilItems()
    {
        var editor = CreateStencilEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var searchBox = FindRequiredControl<TextBox>(view, "PART_StencilSearchBox");

        searchBox.Text = "noise";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.NotNull(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-stencil-noise"));
        Assert.Null(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-stencil-output"));
        Assert.NotNull(FindOptionalDescendant<Expander>(view, "PART_StencilSection_Generators"));
        Assert.Null(FindOptionalDescendant<Expander>(view, "PART_StencilSection_Utilities"));
    }

    [AvaloniaFact]
    public void StencilLibrary_SearchUsesCachedDescriptorsAfterInitialRefresh()
    {
        var commandContributor = new CountingViewCommandContributor();
        var definitionProvider = new CountingViewNodeDefinitionProvider();
        var editor = CreateCountingStencilEditor(commandContributor, definitionProvider);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var searchBox = FindRequiredControl<TextBox>(view, "PART_StencilSearchBox");

        commandContributor.Reset();
        definitionProvider.Reset();

        searchBox.Text = "provider";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.Equal(0, commandContributor.DescriptorQueryCount);
        Assert.Equal(0, definitionProvider.QueryCount);
        Assert.NotNull(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-view-provider-noise"));
    }

    [AvaloniaFact]
    public void StencilLibrary_CollapsedSection_HidesCards_AndRetainsInsertionAfterReexpand()
    {
        var editor = CreateStencilEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var generatorsSection = FindRequiredDescendant<Expander>(view, "PART_StencilSection_Generators");

        generatorsSection.IsExpanded = false;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.False(generatorsSection.IsExpanded);

        generatorsSection.IsExpanded = true;
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        var stencilCard = FindRequiredDescendant<Button>(view, "PART_StencilCard_tests-stencil-noise");
        stencilCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(2, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [AvaloniaFact]
    public void StencilLibrary_TracksRecentsFavoritesSourceFilterAndEmptyState()
    {
        var commandContributor = new CountingViewCommandContributor();
        var definitionProvider = new CountingViewNodeDefinitionProvider();
        var editor = CreateCountingStencilEditor(commandContributor, definitionProvider);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var pluginCard = FindRequiredDescendant<Button>(view, "PART_StencilCard_tests-view-provider-noise");
        var favoriteButton = FindRequiredDescendant<Button>(view, "PART_StencilFavorite_tests-view-provider-noise");

        favoriteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        var favoritesSection = FindRequiredDescendant<Expander>(view, "PART_StencilSection_Favorites");
        Assert.Contains(
            favoritesSection.GetVisualDescendants().OfType<Button>(),
            button => string.Equals(button.Name, "PART_StencilCard_tests-view-provider-noise", StringComparison.Ordinal));

        pluginCard = FindRequiredDescendant<Button>(view, "PART_StencilCard_tests-view-provider-noise");
        pluginCard.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        var recentSection = FindRequiredDescendant<Expander>(view, "PART_StencilSection_Recent");
        Assert.Contains(
            recentSection.GetVisualDescendants().OfType<Button>(),
            button => string.Equals(button.Name, "PART_StencilCard_tests-view-provider-noise", StringComparison.Ordinal));

        var sourceFilter = FindRequiredControl<ComboBox>(view, "PART_StencilSourceFilter");
        sourceFilter.SelectedItem = "Plugin";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.NotNull(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-view-provider-noise"));
        Assert.Null(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-view-base-node"));

        sourceFilter.SelectedItem = "Built-in";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.NotNull(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-view-base-node"));
        Assert.Null(FindOptionalDescendant<Button>(view, "PART_StencilCard_tests-view-provider-noise"));

        var searchBox = FindRequiredControl<TextBox>(view, "PART_StencilSearchBox");
        searchBox.Text = "missing-template";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(FindRequiredControl<TextBlock>(view, "PART_StencilEmptyStateText").IsVisible);

        searchBox.Text = string.Empty;
        sourceFilter.SelectedItem = "All";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.NotNull(FindOptionalDescendant<Expander>(view, "PART_StencilSection_Favorites"));
        Assert.NotNull(FindOptionalDescendant<Expander>(view, "PART_StencilSection_Recent"));
    }

    [AvaloniaFact]
    public void CommandPaletteSearch_UsesCachedProjectionAfterPaletteOpens()
    {
        var commandContributor = new CountingViewCommandContributor();
        var editor = CreateCountingConnectionEditor(commandContributor);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var paletteToggle = FindRequiredControl<Button>(view, "PART_OpenCommandPaletteButton");

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        commandContributor.Reset();

        var searchBox = FindRequiredControl<TextBox>(view, "PART_CommandPaletteSearchBox");
        searchBox.Text = "undo";
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.Equal(0, commandContributor.DescriptorQueryCount);
    }

    [AvaloniaFact]
    public void SelectionRefresh_ReusesCommandDescriptorsAcrossCommandAndAuthoringChrome()
    {
        var commandContributor = new CountingViewCommandContributor();
        var editor = CreateCountingConnectionEditor(commandContributor);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        commandContributor.Reset();

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.NotNull(FindOptionalDescendant<Button>(view, "PART_NodeToolDuplicateButton"));
        Assert.Equal(5, commandContributor.DescriptorQueryCount);
    }

    [AvaloniaFact]
    public void NodeCanvasContextMenuSnapshot_UsesSessionDefinitions_WhenRetainedTemplatesAreCleared()
    {
        var editor = CreateEditor();
        editor.NodeTemplates.Clear();
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var canvas = FindRequiredControl<NodeCanvas>(view, "PART_NodeCanvas");

        var snapshot = InvokeNodeCanvasMethod("CreateContextMenuSnapshot", canvas);
        var definitions = Assert.IsAssignableFrom<IReadOnlyList<INodeDefinition>>(
            snapshot.GetType().GetProperty("AvailableNodeDefinitions")!.GetValue(snapshot));

        Assert.Contains(definitions, definition => definition.Id == new NodeDefinitionId("tests.view.node"));
    }

    [AvaloniaFact]
    public void FragmentLibrary_UsesSessionSnapshots_WhenRetainedTemplatesAreCleared()
    {
        var storageRoot = Path.Combine(Path.GetTempPath(), "astergraph-view-fragment-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRoot);
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "View Fragment Graph",
                "Exercises session-backed fragment library UI.",
                [
                    new GraphNode(
                        "tests.view.fragment-node-001",
                        "Fragment View Node",
                        "Tests",
                        "GraphEditorView",
                        "Source node for fragment workflow tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.fragment-node")),
                ],
                []),
            NodeCatalog = CreateFragmentCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRoot,
        });
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var templatePath = editor.Session.Commands.TryExportSelectionAsTemplate("View Fragment Template");
        editor.FragmentTemplates.Clear();

        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var templatePicker = FindRequiredControl<ComboBox>(view, "PART_FragmentTemplatePicker");
        var importButton = FindRequiredDescendant<Button>(view, "PART_FragmentTemplateImportButton");

        Assert.Equal(1, templatePicker.ItemCount);

        templatePicker.SelectedIndex = 0;
        importButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(2, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);
        Assert.Equal(templatePath, Assert.Single(editor.Session.Queries.GetFragmentTemplateSnapshots()).Path);
    }

    [AvaloniaFact]
    public void CompositeWorkflowChrome_WrapSelectionActionCreatesCompositeShell()
    {
        var editor = CreateSelectionEditor();
        editor.Session.Commands.SetSelection(["tests.view.source-001", "tests.view.target-001"], "tests.view.target-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var wrapSelection = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_composites.wrap-selection");

        Assert.Equal("Wrap Selection To Composite", Assert.IsType<string>(wrapSelection.Content));
        Assert.True(wrapSelection.IsEnabled);

        wrapSelection.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var compositeSnapshot = Assert.Single(editor.Session.Queries.GetCompositeNodeSnapshots());
        var selection = editor.Session.Queries.GetSelectionSnapshot();
        var enterScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.enter");

        Assert.Equal([compositeSnapshot.NodeId], selection.SelectedNodeIds);
        Assert.True(enterScope.IsEnabled);
    }

    [AvaloniaFact]
    public void AuthoringToolsChrome_ProjectsSelectionToolProviderActions()
    {
        var editor = CreateSelectionToolProviderEditor();
        editor.Session.Commands.SetSelection(["tests.view.source-001", "tests.view.target-001"], "tests.view.target-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var selectionTool = FindRequiredDescendant<Button>(view, "PART_SelectionTool_tests.view.selection.wrap");

        Assert.Equal("Host Wrap Selection", Assert.IsType<string>(selectionTool.Content));
        Assert.True(selectionTool.IsEnabled);

        selectionTool.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Single(editor.Session.Queries.GetCompositeNodeSnapshots());
    }

    [AvaloniaFact]
    public void AuthoringToolsChrome_ProjectsStockSelectionLayoutActions()
    {
        var editor = CreateSelectionEditor();
        editor.Session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot("tests.view.source-001", new GraphPoint(121, 163)),
                new NodePositionSnapshot("tests.view.target-001", new GraphPoint(523, 187)),
            ],
            updateStatus: false);
        editor.Session.Commands.SetSelection(["tests.view.source-001", "tests.view.target-001"], "tests.view.target-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var alignLeftButton = FindRequiredDescendant<Button>(view, "PART_SelectionToolAlignLeftButton");
        var distributeHorizontalButton = FindRequiredDescendant<Button>(view, "PART_SelectionToolDistributeHorizontalButton");
        var snapGridButton = FindRequiredDescendant<Button>(view, "PART_SelectionToolSnapGridButton");

        Assert.Equal("Align Left", Assert.IsType<string>(alignLeftButton.Content));
        Assert.True(alignLeftButton.IsEnabled);
        Assert.False(distributeHorizontalButton.IsEnabled);
        var distributeTooltip = Assert.IsType<string>(ToolTip.GetTip(distributeHorizontalButton));
        Assert.Contains("Select at least three nodes before distributing.", distributeTooltip, StringComparison.Ordinal);
        Assert.Contains("Select at least three nodes first.", distributeTooltip, StringComparison.Ordinal);
        Assert.Equal("Snap Selection To Grid", AutomationProperties.GetName(snapGridButton));

        snapGridButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        var nodes = editor.Session.Queries.CreateDocumentSnapshot()
            .Nodes
            .ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(120, 160), nodes["tests.view.source-001"].Position);
        Assert.Equal(new GraphPoint(520, 180), nodes["tests.view.target-001"].Position);
    }

    [AvaloniaFact]
    public void CompositeWorkflowChrome_ProjectsBreadcrumbsAndScopeNavigation()
    {
        var editor = CreateScopedEditor();
        editor.Session.Commands.SetSelection(["tests.view.composite-001"], "tests.view.composite-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var enterScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.enter");
        Assert.True(enterScope.IsEnabled);

        enterScope.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal("graph-child-001", editor.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);

        var rootBreadcrumb = FindRequiredDescendant<Button>(view, "PART_ScopeBreadcrumb_graph-root");
        var childBreadcrumb = FindRequiredDescendant<Button>(view, "PART_ScopeBreadcrumb_graph-child-001");
        var exitScope = FindRequiredDescendant<Button>(view, "PART_CompositeWorkflowAction_scopes.exit");

        Assert.True(rootBreadcrumb.IsEnabled);
        Assert.False(childBreadcrumb.IsEnabled);
        Assert.True(exitScope.IsEnabled);

        rootBreadcrumb.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal("graph-root", editor.Session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);
    }

    [AvaloniaFact]
    public void AuthoringToolsChrome_NodeQuickTools_ProjectCanonicalNodeActions()
    {
        var editor = CreateConnectionToolEditor();
        editor.Session.Commands.SetSelection(["tests.view.tools-source-001"], "tests.view.tools-source-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var inspectButton = FindRequiredDescendant<Button>(view, "PART_NodeToolInspectButton");
        var duplicateButton = FindRequiredDescendant<Button>(view, "PART_NodeToolDuplicateButton");
        var deleteButton = FindRequiredDescendant<Button>(view, "PART_NodeToolDeleteButton");
        var disconnectOutgoingButton = FindRequiredDescendant<Button>(view, "PART_NodeToolDisconnectOutgoingButton");

        Assert.True(inspectButton.IsEnabled);
        Assert.True(duplicateButton.IsEnabled);
        Assert.True(deleteButton.IsEnabled);
        Assert.True(disconnectOutgoingButton.IsEnabled);
        Assert.Equal(Assert.IsType<string>(inspectButton.Content), AutomationProperties.GetName(inspectButton));
        Assert.Equal(Assert.IsType<string>(duplicateButton.Content), AutomationProperties.GetName(duplicateButton));
        Assert.Equal(Assert.IsType<string>(deleteButton.Content), AutomationProperties.GetName(deleteButton));
        Assert.Equal(
            Assert.IsType<string>(disconnectOutgoingButton.Content),
            AutomationProperties.GetName(disconnectOutgoingButton));

        duplicateButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(3, editor.Session.Queries.CreateDocumentSnapshot().Nodes.Count);

        disconnectOutgoingButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Empty(editor.Session.Queries.CreateDocumentSnapshot().Connections);
    }

    [AvaloniaFact]
    public void AuthoringToolsChrome_SingleSelectedNode_ProjectsExpansionAndConnectionEditors()
    {
        var editor = CreateConnectionToolEditor();
        editor.Session.Commands.SetSelection(["tests.view.tools-source-001"], "tests.view.tools-source-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;

        var expansionButton = FindRequiredDescendant<Button>(view, "PART_NodeToolToggleExpansionButton");
        var labelEditor = FindRequiredDescendant<TextBox>(view, "PART_ConnectionToolLabelEditor_connection-001");
        var noteEditor = FindRequiredDescendant<TextBox>(view, "PART_ConnectionToolNoteEditor_connection-001");
        var applyButton = FindRequiredDescendant<Button>(view, "PART_ConnectionToolApply_connection-001");
        var clearNoteButton = FindRequiredDescendant<Button>(view, "PART_ConnectionToolClearNote_connection-001");

        Assert.Equal("Expand Node Card", Assert.IsType<string>(expansionButton.Content));
        Assert.Equal("Signal Flow", labelEditor.Text);
        Assert.Equal("Preview branch", noteEditor.Text);
        Assert.True(clearNoteButton.IsEnabled);
        Assert.Equal("Outgoing to Tool Target label editor", AutomationProperties.GetName(labelEditor));
        Assert.Equal("Outgoing to Tool Target note editor", AutomationProperties.GetName(noteEditor));
        Assert.Equal("Apply Edge Text", AutomationProperties.GetName(applyButton));
        Assert.Equal("Clear Connection Note", AutomationProperties.GetName(clearNoteButton));

        expansionButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        Assert.Equal(
            GraphNodeExpansionState.Expanded,
            Assert.Single(editor.Session.Queries.GetNodeSurfaceSnapshots(), surface => surface.NodeId == "tests.view.tools-source-001").ExpansionState);

        clearNoteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(string.IsNullOrWhiteSpace(Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections).Presentation?.NoteText));

        labelEditor.Text = "Refined Flow";
        noteEditor.Text = "Updated branch";
        applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var updatedConnection = Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections);
        Assert.Equal("Refined Flow", updatedConnection.Label);
        Assert.Equal("Updated branch", updatedConnection.Presentation?.NoteText);
    }

    [AvaloniaFact]
    public void AuthoringToolsChrome_ConnectionRouteEditors_InsertMoveAndRemoveVertices()
    {
        var editor = CreateConnectionToolEditor();
        editor.Session.Commands.SetSelection(["tests.view.tools-source-001"], "tests.view.tools-source-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var initialGeometry = Assert.Single(editor.Session.Queries.GetConnectionGeometrySnapshots());
        var expectedInsertedVertex = ConnectionPathBuilder.ResolveSegmentMidpoint(
            initialGeometry.Source.Position,
            GraphConnectionRoute.Empty,
            initialGeometry.Target.Position,
            0);

        var insertButton = FindRequiredDescendant<Button>(view, "PART_ConnectionToolInsertRouteVertex_connection-001_0");
        insertButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.Equal(
            [expectedInsertedVertex],
            Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices);

        var xEditor = FindRequiredDescendant<TextBox>(view, "PART_ConnectionToolRouteVertexXEditor_connection-001_0");
        var yEditor = FindRequiredDescendant<TextBox>(view, "PART_ConnectionToolRouteVertexYEditor_connection-001_0");
        var applyButton = FindRequiredDescendant<Button>(view, "PART_ConnectionToolApplyRouteVertex_connection-001_0");
        Assert.Equal("Outgoing to Tool Target bend 1 X editor", AutomationProperties.GetName(xEditor));
        Assert.Equal("Outgoing to Tool Target bend 1 Y editor", AutomationProperties.GetName(yEditor));
        Assert.Equal("Apply Bend", AutomationProperties.GetName(applyButton));

        xEditor.Text = "420";
        yEditor.Text = "300";
        applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.Equal(
            [new GraphPoint(420d, 300d)],
            Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices);

        var removeButton = FindRequiredDescendant<Button>(view, "PART_ConnectionToolRemoveRouteVertex_connection-001_0");
        Assert.Equal("Remove Bend", AutomationProperties.GetName(removeButton));
        removeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.Empty(
            Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices
            ?? []);
    }

    [AvaloniaFact]
    public void CommandPalette_ProjectsContextualAuthoringActions_FromSharedToolRoute()
    {
        var editor = CreateConnectionToolEditor();
        editor.Session.Commands.SetSelection(["tests.view.tools-source-001"], "tests.view.tools-source-001", updateStatus: false);
        var window = CreateWindow(new GraphEditorView
        {
            Editor = editor,
        });
        var view = (GraphEditorView)window.Content!;
        var paletteToggle = FindRequiredControl<Button>(view, "PART_OpenCommandPaletteButton");

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var paletteButtons = FindRequiredControl<StackPanel>(view, "PART_CommandPaletteItems")
            .Children
            .OfType<Button>()
            .Where(button => !string.IsNullOrWhiteSpace(button.Name))
            .ToDictionary(button => button.Name!, StringComparer.Ordinal);

        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_selection-create-group"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");
        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_selection-snap-grid"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");
        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_node-toggle-surface-expansion"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");
        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_connection-disconnect"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");
        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_connection-clear-note"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");
        Assert.True(
            paletteButtons.ContainsKey("PART_CommandPaletteAction_selection-distribute-horizontal"),
            $"Palette actions: {string.Join(", ", paletteButtons.Keys.OrderBy(static key => key, StringComparer.Ordinal))}");

        var createGroup = paletteButtons["PART_CommandPaletteAction_selection-create-group"];
        var snapGrid = paletteButtons["PART_CommandPaletteAction_selection-snap-grid"];
        var toggleExpansion = paletteButtons["PART_CommandPaletteAction_node-toggle-surface-expansion"];
        var disconnect = paletteButtons["PART_CommandPaletteAction_connection-disconnect"];
        var clearNote = paletteButtons["PART_CommandPaletteAction_connection-clear-note"];
        var distributeHorizontal = paletteButtons["PART_CommandPaletteAction_selection-distribute-horizontal"];

        Assert.Equal("Create Group", Assert.IsType<string>(createGroup.Content));
        Assert.Equal("Snap Selection To Grid", Assert.IsType<string>(snapGrid.Content));
        Assert.Equal("Expand Node Card", Assert.IsType<string>(toggleExpansion.Content));
        Assert.Equal("Disconnect Connection", Assert.IsType<string>(disconnect.Content));
        Assert.Equal("Clear Connection Note", Assert.IsType<string>(clearNote.Content));
        Assert.Equal("Create Group", AutomationProperties.GetName(createGroup));
        Assert.Equal("Snap Selection To Grid", AutomationProperties.GetName(snapGrid));
        Assert.Equal("Expand Node Card", AutomationProperties.GetName(toggleExpansion));
        Assert.Equal("Disconnect Connection", AutomationProperties.GetName(disconnect));
        Assert.Equal("Clear Connection Note", AutomationProperties.GetName(clearNote));
        var distributeTooltip = Assert.IsType<string>(ToolTip.GetTip(distributeHorizontal));
        Assert.Contains("Select at least three nodes before distributing.", distributeTooltip, StringComparison.Ordinal);
        Assert.Contains("Select at least three nodes first.", distributeTooltip, StringComparison.Ordinal);

        toggleExpansion.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        var paletteChrome = FindRequiredControl<Border>(view, "PART_CommandPaletteChrome");
        if (paletteChrome.IsVisible)
        {
            paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        paletteToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        var recentHeading = Assert.Single(
            FindRequiredControl<StackPanel>(view, "PART_CommandPaletteItems").Children.OfType<TextBlock>(),
            text => string.Equals(text.Name, "PART_CommandPaletteRecentActionsHeading", StringComparison.Ordinal));
        var recentClearNote = Assert.Single(
            FindRequiredControl<StackPanel>(view, "PART_CommandPaletteItems").Children.OfType<Button>(),
            button => string.Equals(button.Name, "PART_CommandPaletteRecentAction_node-toggle-surface-expansion", StringComparison.Ordinal));

        Assert.Equal("Recent Actions", recentHeading.Text);
        Assert.Contains("Node Card", AutomationProperties.GetName(recentClearNote), StringComparison.Ordinal);

        clearNote = FindRequiredDescendant<Button>(view, "PART_CommandPaletteAction_connection-clear-note");
        clearNote.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.True(string.IsNullOrWhiteSpace(Assert.Single(editor.Session.Queries.CreateDocumentSnapshot().Connections).Presentation?.NoteText));
    }

    private static Window CreateWindow(GraphEditorView view)
    {
        var window = new Window
        {
            Width = 1440,
            Height = 900,
            Content = view,
        };
        window.Show();
        return window;
    }

    private static T FindRequiredControl<T>(Control root, string name)
        where T : Control
        => root.FindControl<T>(name) ?? throw new Xunit.Sdk.XunitException($"Could not find control '{name}'.");

    private static T FindRequiredDescendant<T>(Control root, string name)
        where T : Control
        => root.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Could not find descendant control '{name}'.");

    private static T? FindOptionalDescendant<T>(Control root, string name)
        where T : Control
        => root.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

    private static void AssertFocusRoundTrip(
        GraphEditorView view,
        Control focusOrigin,
        Border paletteChrome,
        TextBox paletteSearchBox)
    {
        focusOrigin.Focus();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Source = focusOrigin,
        });
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.True(paletteChrome.IsVisible);
        Assert.Same(paletteSearchBox, GetFocusedElement(view));

        InvokeViewKeyDown(view, new KeyEventArgs
        {
            Key = Key.Escape,
        });
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

        Assert.False(paletteChrome.IsVisible);
        Assert.Same(focusOrigin, GetFocusedElement(view));
    }

    private static void AssertInputScopeShortcutBehavior(
        GraphEditorView view,
        GraphEditorViewModel editor,
        Control inputScopeEditor,
        Border paletteChrome)
    {
        var deleteArgs = new KeyEventArgs
        {
            Key = Key.Delete,
            Source = inputScopeEditor,
        };

        InvokeViewKeyDown(view, deleteArgs);

        Assert.False(deleteArgs.Handled);
        Assert.Single(editor.Nodes);
        Assert.Equal("tests.view.node-001", editor.Nodes[0].Id);
        Assert.Equal("tests.view.node-001", editor.SelectedNode?.Id);

        var paletteArgs = new KeyEventArgs
        {
            Key = Key.P,
            KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift,
            Source = inputScopeEditor,
        };

        InvokeViewKeyDown(view, paletteArgs);

        Assert.True(paletteArgs.Handled);
        Assert.True(paletteChrome.IsVisible);
    }

    private static IInputElement? GetFocusedElement(GraphEditorView view)
        => TopLevel.GetTopLevel(view)?.FocusManager?.GetFocusedElement();

    private static void InvokeViewKeyDown(GraphEditorView view, KeyEventArgs args)
    {
        var handler = typeof(GraphEditorView).GetMethod(
            "HandleKeyDown",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handler);
        handler.Invoke(view, [view, args]);
    }

    private static PointerPressedEventArgs CreatePointerPressedArgs(
        InputElement source,
        InputElement root,
        global::Avalonia.Input.Pointer pointer,
        Point position,
        KeyModifiers modifiers)
        => new(
            source,
            pointer,
            root,
            position,
            0UL,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            modifiers,
            1);

    private static object InvokeNodeCanvasMethod(string methodName, NodeCanvas canvas)
    {
        var method = typeof(NodeCanvas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return method!.Invoke(canvas, [])!;
    }

    private static GraphEditorViewModel CreateEditor(IGraphContextMenuAugmentor? augmentor = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.node"),
                "View Test Node",
                "Tests",
                "Exercises GraphEditorView chrome switching.",
                [],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "View Test Graph",
                "Exercises GraphEditorView shell chrome.",
                [
                    new GraphNode(
                        "tests.view.node-001",
                        "View Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by GraphEditorView tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.view.node")),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService(),
            contextMenuAugmentor: augmentor);
    }

    private static GraphEditorViewModel CreateAuthoringEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.authoring-node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Authoring Node",
                "Tests",
                "GraphEditorView authoring focus coverage.",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls the authoring threshold.",
                        defaultValue: 0.5,
                        groupName: "Behavior",
                        sortOrder: 20,
                        unitSuffix: "ms"),
                ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "GraphEditorView Authoring Graph",
                "Exercises GraphEditorView focus round-tripping.",
                [
                    new GraphNode(
                        "tests.view.authoring-node-001",
                        "Authoring Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by GraphEditorView focus tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        definitionId,
                        [
                            new GraphParameterValue("threshold", new PortTypeId("float"), 0.75),
                        ]),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateValidationFeedbackEditor()
    {
        var sourceDefinitionId = new NodeDefinitionId("tests.view.validation-source");
        var targetDefinitionId = new NodeDefinitionId("tests.view.validation-target");
        var flowTypeId = new PortTypeId("flow");
        var textTypeId = new PortTypeId("string");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            sourceDefinitionId,
            "Validation Source",
            "Tests",
            "Validation source.",
            [],
            [new PortDefinition("out", "Out", flowTypeId, "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            targetDefinitionId,
            "Validation Target",
            "Tests",
            "Validation target.",
            [new PortDefinition("in", "In", textTypeId, "#F3B36B")],
            []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Validation Feedback Graph",
                "Exercises hosted validation feedback projection.",
                [
                    new GraphNode(
                        "tests.view.validation-source-001",
                        "Validation Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for validation feedback tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", flowTypeId)],
                        "#6AD5C4",
                        sourceDefinitionId),
                    new GraphNode(
                        "tests.view.validation-target-001",
                        "Validation Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for validation feedback tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "In", PortDirection.Input, "string", "#F3B36B", textTypeId)],
                        [],
                        "#F3B36B",
                        targetDefinitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "tests.view.validation-source-001",
                        "out",
                        "tests.view.validation-target-001",
                        "in",
                        "Invalid Flow",
                        "#6AD5C4"),
                ]),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateValidationParameterFeedbackEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.validation-parameters");
        var textTypeId = new PortTypeId("string");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Validation Parameters",
            "Tests",
            "Validation parameter target.",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "prompt",
                    "Prompt",
                    textTypeId,
                    ParameterEditorKind.Text,
                    isRequired: true,
                    helpText: "Prompt guidance for review automation."),
                new NodeParameterDefinition(
                    "system",
                    "System",
                    textTypeId,
                    ParameterEditorKind.Text,
                    isRequired: true),
            ]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Validation Parameter Feedback Graph",
                "Exercises repeated validation focus target names.",
                [
                    new GraphNode(
                        "parameter-001",
                        "Validation Parameters",
                        "Tests",
                        "GraphEditorView",
                        "Node with multiple invalid parameters.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        definitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateStencilEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.stencil.noise"),
                "Noise Generator",
                "Generators",
                "Creates procedural noise.",
                [],
                []));
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.stencil.output"),
                "Output Preview",
                "Utilities",
                "Shows the current graph output.",
                [],
                []));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Stencil View Graph",
                "Exercises grouped stencil chrome.",
                [
                    new GraphNode(
                        "tests.stencil.node-001",
                        "Existing Node",
                        "Tests",
                        "GraphEditorView",
                        "Used by grouped stencil tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.stencil.noise")),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateCountingStencilEditor(
        CountingViewCommandContributor commandContributor,
        CountingViewNodeDefinitionProvider definitionProvider)
    {
        var baseDefinitionId = new NodeDefinitionId("tests.view.base-node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                baseDefinitionId,
                "Base Node",
                "Base",
                "Base test node.",
                [],
                []));
        catalog.RegisterProvider(definitionProvider);

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Counting Stencil Graph",
                "Exercises cached stencil descriptor refresh.",
                [
                    new GraphNode(
                        "tests.view.base-node-001",
                        "Base Node",
                        "Tests",
                        "GraphEditorView",
                        "Existing node for stencil tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        baseDefinitionId),
                ],
                []),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(new CountingViewCommandPlugin(commandContributor)),
            ],
        });
    }

    private static INodeCatalog CreateFragmentCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.view.fragment-node"),
                "Fragment View Node",
                "Tests",
                "Exercises session-backed fragment template surfaces.",
                [],
                []));
        return catalog;
    }

    private static GraphEditorViewModel CreateSelectionEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.selection");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Selection View Node",
                "Tests",
                "Exercises composite workflow actions.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Selection Workflow Graph",
                "Exercises composite workflow shell actions.",
                [
                    new GraphNode(
                        "tests.view.source-001",
                        "View Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for composite workflow tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.view.target-001",
                        "View Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for composite workflow tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateSelectionToolProviderEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.selection-provider");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Selection Provider Node",
                "Tests",
                "Exercises selection tool provider actions.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Selection Tool Provider Graph",
                "Exercises selection tool provider projection.",
                [
                    new GraphNode(
                        "tests.view.source-001",
                        "View Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for selection tool provider tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.view.target-001",
                        "View Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for selection tool provider tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                []),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            ToolProvider = new GraphEditorViewSelectionToolProvider(),
        });
    }

    private static GraphEditorViewModel CreateScopedEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.scoped");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Scoped View Node",
                "Tests",
                "Exercises scope workflow actions.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = GraphDocument.CreateScoped(
                "Scoped View Graph",
                "Exercises composite scope navigation chrome.",
                "graph-root",
                [
                    new GraphScope(
                        "graph-root",
                        [
                            new GraphNode(
                                "tests.view.composite-001",
                                "Composite View Node",
                                "Tests",
                                "GraphEditorView",
                                "Composite shell node for scope navigation tests.",
                                new GraphPoint(160, 140),
                                new GraphSize(260, 180),
                                [],
                                [],
                                "#A67CF5",
                                null,
                                [],
                                null,
                                new GraphCompositeNode("graph-child-001", [], [])),
                        ],
                        []),
                    new GraphScope(
                        "graph-child-001",
                        [
                            new GraphNode(
                                "tests.view.child-source-001",
                                "Child Source",
                                "Tests",
                                "GraphEditorView",
                                "Child source node.",
                                new GraphPoint(80, 100),
                                new GraphSize(220, 150),
                                [],
                                [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                                "#6AD5C4",
                                definitionId),
                        ],
                        []),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }

    private static GraphEditorViewModel CreateConnectionToolEditor()
    {
        var definitionId = new NodeDefinitionId("tests.view.tools");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Tool View Node",
                "Tests",
                "Exercises node and edge tooling chrome.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Tooling View Graph",
                "Exercises node and edge tooling shell actions.",
                [
                    new GraphNode(
                        "tests.view.tools-source-001",
                        "Tool Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for tooling tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.view.tools-target-001",
                        "Tool Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for tooling tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "tests.view.tools-source-001",
                        "out",
                        "tests.view.tools-target-001",
                        "in",
                        "Signal Flow",
                        "#6AD5C4",
                        null,
                        new GraphEdgePresentation("Preview branch")),
                ]),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private static GraphEditorViewModel CreateCountingConnectionEditor(CountingViewCommandContributor commandContributor)
    {
        var definitionId = new NodeDefinitionId("tests.view.tools");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Tool View Node",
                "Tests",
                "Exercises node and edge tooling chrome.",
                [new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));

        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Counting Tooling View Graph",
                "Exercises command-surface refresh query counts.",
                [
                    new GraphNode(
                        "tests.view.tools-source-001",
                        "Tool Source",
                        "Tests",
                        "GraphEditorView",
                        "Source node for tooling tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        "tests.view.tools-target-001",
                        "Tool Target",
                        "Tests",
                        "GraphEditorView",
                        "Target node for tooling tests.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort("in", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "tests.view.tools-source-001",
                        "out",
                        "tests.view.tools-target-001",
                        "in",
                        "Signal Flow",
                        "#6AD5C4",
                        null,
                        new GraphEdgePresentation("Preview branch")),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(new CountingViewCommandPlugin(commandContributor)),
            ],
        });
    }

    private sealed class GraphEditorViewHostAwareAugmentor : IGraphContextMenuAugmentor
    {
        public bool ReceivedHostOwner { get; private set; }

        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
        {
            ReceivedHostOwner = context.HostContext.TryGetOwner<GraphEditorView>(out _);
            return
            [
                .. stockItems,
                new MenuItemDescriptor("tests-view-host-item", "Host Item", null),
            ];
        }
    }

    private sealed class GraphEditorViewSelectionToolProvider : IGraphEditorToolProvider
    {
        public IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Context.Kind == GraphEditorToolContextKind.Selection
                ? [
                    new GraphEditorToolDescriptorSnapshot(
                        "tests.view.selection.wrap",
                        GraphEditorToolContextKind.Selection,
                        new GraphEditorCommandDescriptorSnapshot(
                            "tests.view.selection.wrap",
                            "Host Wrap Selection",
                            "host.tools",
                            "composite-wrap",
                            null,
                            GraphEditorCommandSourceKind.Host,
                            isEnabled: true),
                        new GraphEditorCommandInvocationSnapshot("composites.wrap-selection"))
                ]
                : [];
        }
    }

    private sealed class CountingViewCommandPlugin(CountingViewCommandContributor contributor) : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.view.counting-command", "Counting Command");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddCommandContributor(contributor);
        }
    }

    private sealed class CountingViewCommandContributor : IGraphEditorPluginCommandContributor
    {
        public int DescriptorQueryCount { get; private set; }

        public void Reset()
            => DescriptorQueryCount = 0;

        public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            DescriptorQueryCount++;
            return
            [
                new GraphEditorCommandDescriptorSnapshot(
                    "tests.view.counting-plugin",
                    "Counting Plugin Command",
                    "plugin",
                    "plugin",
                    null,
                    GraphEditorCommandSourceKind.Plugin,
                    isEnabled: true),
            ];
        }

        public bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return true;
        }
    }

    private sealed class CountingViewNodeDefinitionProvider : INodeDefinitionProvider
    {
        private readonly IReadOnlyList<INodeDefinition> _definitions =
        [
            new NodeDefinition(
                new NodeDefinitionId("tests.view.provider.noise"),
                "Provider Noise",
                "Provider Plugins",
                "Provided node definition for stencil projection count tests.",
                [],
                []),
            new NodeDefinition(
                new NodeDefinitionId("tests.view.provider.output"),
                "Provider Output",
                "Provider Plugins",
                "Second provided node definition for stencil projection count tests.",
                [],
                []),
        ];

        public int QueryCount { get; private set; }

        public void Reset()
            => QueryCount = 0;

        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        {
            QueryCount++;
            return _definitions;
        }
    }

    private sealed class InputScopeNodeVisualPresenter : IGraphNodeVisualPresenter
    {
        public GraphNodeVisual Create(GraphNodeVisualContext context)
        {
            var surface = new InputScopeHost
            {
                Width = context.Node.Width,
                Height = context.Node.Height,
                Child = new Button
                {
                    Name = "PART_CustomVisualInputScopeEditor",
                    Content = "Custom visual editor",
                },
            };

            return new GraphNodeVisual(surface, new Dictionary<string, Control>(StringComparer.Ordinal));
        }

        public void Update(GraphNodeVisual visual, GraphNodeVisualContext context)
        {
            visual.Root.Width = context.Node.Width;
            visual.Root.Height = context.Node.Height;
        }
    }

    private sealed class InputScopeNodeBodyPresenter : IGraphNodeBodyPresenter
    {
        public GraphNodeBodyVisual Create(GraphNodeVisualContext context)
            => new(
                new InputScopeHost
                {
                    Child = new Button
                    {
                        Name = "PART_CustomBodyInputScopeEditor",
                        Content = "Custom body editor",
                    },
                });

        public void Update(GraphNodeBodyVisual visual, GraphNodeVisualContext context)
        {
        }
    }

    private sealed class InputScopeHost : Border, IGraphEditorInputScope
    {
    }
}

public sealed class GraphEditorViewTestsAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<GraphEditorViewTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public sealed class GraphEditorViewTestsApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
