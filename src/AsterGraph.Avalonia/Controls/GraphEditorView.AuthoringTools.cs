using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class GraphEditorView
{
    private Border? _authoringToolsChrome;
    private TextBlock? _authoringToolsCaptionText;
    private WrapPanel? _selectionToolToolbar;
    private WrapPanel? _nodeToolToolbar;
    private StackPanel? _connectionToolList;
    private GraphEditorAuthoringToolSurfaceState? _authoringToolSurfaceState;

    private void InitializeAuthoringToolControls()
    {
        _authoringToolsChrome = this.FindControl<Border>("PART_AuthoringToolsChrome");
        _authoringToolsCaptionText = this.FindControl<TextBlock>("PART_AuthoringToolsCaptionText");
        _selectionToolToolbar = this.FindControl<WrapPanel>("PART_SelectionToolToolbar");
        _nodeToolToolbar = this.FindControl<WrapPanel>("PART_NodeToolToolbar");
        _connectionToolList = this.FindControl<StackPanel>("PART_ConnectionToolList");
    }

    private void RefreshAuthoringToolSurface(IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptors)
    {
        if (_authoringToolsChrome is null
            || _authoringToolsCaptionText is null
            || _selectionToolToolbar is null
            || _nodeToolToolbar is null
            || _connectionToolList is null)
        {
            return;
        }

        _selectionToolToolbar.Children.Clear();
        _nodeToolToolbar.Children.Clear();
        _connectionToolList.Children.Clear();

        if (Editor is null)
        {
            _authoringToolsChrome.IsVisible = true;
            _authoringToolsCaptionText.Text = "Attach an editor to inspect selection, node, and edge authoring tools.";
            return;
        }

        var authoringState = _authoringToolSurfaceState ?? CreateAuthoringToolSurfaceState(Editor.Session.Queries.GetSelectionSnapshot());
        _authoringToolSurfaceState = authoringState;
        BuildSelectionToolToolbar(authoringState.SelectionActions);

        if (Editor.SelectedNodes.Count != 1 || Editor.SelectedNode is null)
        {
            _authoringToolsChrome.IsVisible = true;
            _authoringToolsCaptionText.Text = authoringState.Selection.SelectedNodeIds.Count == 0
                ? "Select nodes to project selection, node, and edge tooling surfaces."
                : $"Selection tools for {authoringState.Selection.SelectedNodeIds.Count} node(s).";
            return;
        }

        var selectedNode = Editor.SelectedNode;
        BuildNodeToolToolbar(authoringState.NodeActions);
        BuildConnectionToolList(selectedNode, authoringState, commandDescriptors);

        _authoringToolsChrome.IsVisible = true;
        _authoringToolsCaptionText.Text = _connectionToolList.Children.Count == 0
            ? $"Selection and node tools for {selectedNode.Title}."
            : $"Selection tools, node tools for {selectedNode.Title}, plus {_connectionToolList.Children.Count} incident connection editor(s).";
    }

    private void BuildSelectionToolToolbar(IReadOnlyList<AsterGraphHostedActionDescriptor> actions)
    {
        if (_selectionToolToolbar is null || actions.Count == 0)
        {
            return;
        }

        foreach (var action in actions)
        {
            _selectionToolToolbar.Children.Add(CreateHostedToolButton(ResolveSelectionToolButtonName(action), action));
        }
    }

    private void BuildNodeToolToolbar(IReadOnlyList<AsterGraphHostedActionDescriptor> actions)
    {
        if (_nodeToolToolbar is null || actions.Count == 0)
        {
            return;
        }

        foreach (var action in actions)
        {
            _nodeToolToolbar.Children.Add(CreateHostedToolButton(ResolveNodeToolButtonName(action), action));
        }
    }

    private void BuildConnectionToolList(
        NodeViewModel selectedNode,
        GraphEditorAuthoringToolSurfaceState authoringState,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptors)
    {
        if (_connectionToolList is null || Editor is null)
        {
            return;
        }

        var connections = Editor.Connections
            .Where(connection =>
                string.Equals(connection.SourceNodeId, selectedNode.Id, StringComparison.Ordinal)
                || string.Equals(connection.TargetNodeId, selectedNode.Id, StringComparison.Ordinal))
            .OrderBy(connection => connection.Id, StringComparer.Ordinal)
            .ToList();

        if (connections.Count == 0)
        {
            return;
        }

        foreach (var connection in connections)
        {
            authoringState.DocumentConnectionsById.TryGetValue(connection.Id, out var modelConnection);
            authoringState.GeometryByConnectionId.TryGetValue(connection.Id, out var geometry);
            var connectionTitle = CreateConnectionToolTitle(selectedNode, connection);
            var labelEditor = new TextBox
            {
                Name = $"PART_ConnectionToolLabelEditor_{connection.Id}",
                Text = connection.Label,
                Watermark = "Connection label",
            };
            labelEditor.Classes.Add("astergraph-input");
            AutomationProperties.SetName(labelEditor, $"{connectionTitle} label editor");

            var noteEditor = new TextBox
            {
                Name = $"PART_ConnectionToolNoteEditor_{connection.Id}",
                Text = connection.NoteText ?? string.Empty,
                Watermark = "Display note",
            };
            noteEditor.Classes.Add("astergraph-input");
            AutomationProperties.SetName(noteEditor, $"{connectionTitle} note editor");

            var actionBar = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 36,
                ItemWidth = double.NaN,
            };

            actionBar.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolApply_{connection.Id}",
                "Apply Edge Text",
                CanApplyConnectionText(commandDescriptors),
                () =>
                    TryExecuteConnectionTextCommands(connection.Id, labelEditor.Text, noteEditor.Text)));

            var actions = authoringState.ConnectionActionsById.TryGetValue(connection.Id, out var cachedActions)
                ? cachedActions
                : [];
            foreach (var action in actions)
            {
                actionBar.Children.Add(CreateHostedToolButton(
                    ResolveConnectionToolButtonName(connection.Id, action),
                    action));
            }

            var card = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = connectionTitle,
                            FontWeight = FontWeight.SemiBold,
                        },
                        labelEditor,
                        noteEditor,
                        actionBar,
                        CreateConnectionRouteSection(
                            connection.Id,
                            connectionTitle,
                            modelConnection?.Presentation?.Route ?? GraphConnectionRoute.Empty,
                            geometry,
                            commandDescriptors),
                    },
                },
            };
            _connectionToolList.Children.Add(card);
        }
    }

    private Control CreateConnectionRouteSection(
        string connectionId,
        string connectionTitle,
        GraphConnectionRoute route,
        GraphEditorConnectionGeometrySnapshot? geometry,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptors)
    {
        var container = new StackPanel
        {
            Spacing = 8,
        };
        container.Children.Add(new TextBlock
        {
            Text = "Route Vertices",
            FontWeight = FontWeight.SemiBold,
        });

        commandDescriptors.TryGetValue("connections.route-vertex.insert", out var insertRouteDescriptor);
        commandDescriptors.TryGetValue("connections.route-vertex.move", out var moveRouteDescriptor);
        commandDescriptors.TryGetValue("connections.route-vertex.remove", out var removeRouteDescriptor);

        if (geometry is not null)
        {
            var segmentBar = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 36,
                ItemWidth = double.NaN,
            };
            for (var segmentIndex = 0; segmentIndex <= route.Vertices.Count; segmentIndex++)
            {
                var capturedSegmentIndex = segmentIndex;
                segmentBar.Children.Add(CreateToolActionButton(
                    $"PART_ConnectionToolInsertRouteVertex_{connectionId}_{capturedSegmentIndex}",
                    $"Insert Bend {capturedSegmentIndex + 1}",
                    insertRouteDescriptor?.CanExecute ?? true,
                    () =>
                    {
                        if (Editor is null)
                        {
                            return false;
                        }

                        var midpoint = ConnectionPathBuilder.ResolveSegmentMidpoint(
                            geometry.Source.Position,
                            route,
                            geometry.Target.Position,
                            capturedSegmentIndex);
                        return TryExecuteCommand(
                            "connections.route-vertex.insert",
                            ("connectionId", connectionId),
                            ("vertexIndex", capturedSegmentIndex.ToString(CultureInfo.InvariantCulture)),
                            ("worldX", midpoint.X.ToString(CultureInfo.InvariantCulture)),
                            ("worldY", midpoint.Y.ToString(CultureInfo.InvariantCulture)),
                            ("updateStatus", bool.TrueString));
                    },
                    CreateActionTooltip(insertRouteDescriptor)));
            }

            container.Children.Add(segmentBar);
        }

        if (route.IsEmpty)
        {
            container.Children.Add(new TextBlock
            {
                Text = "No persisted route vertices yet.",
                Opacity = 0.74d,
            });
            return container;
        }

        for (var vertexIndex = 0; vertexIndex < route.Vertices.Count; vertexIndex++)
        {
            var vertex = route.Vertices[vertexIndex];
            var capturedVertexIndex = vertexIndex;
            var vertexCaption = $"{connectionTitle} bend {capturedVertexIndex + 1}";
            var xEditor = new TextBox
            {
                Name = $"PART_ConnectionToolRouteVertexXEditor_{connectionId}_{capturedVertexIndex}",
                Text = vertex.X.ToString(CultureInfo.InvariantCulture),
                Watermark = "X",
            };
            xEditor.Classes.Add("astergraph-input");
            AutomationProperties.SetName(xEditor, $"{vertexCaption} X editor");

            var yEditor = new TextBox
            {
                Name = $"PART_ConnectionToolRouteVertexYEditor_{connectionId}_{capturedVertexIndex}",
                Text = vertex.Y.ToString(CultureInfo.InvariantCulture),
                Watermark = "Y",
            };
            yEditor.Classes.Add("astergraph-input");
            AutomationProperties.SetName(yEditor, $"{vertexCaption} Y editor");

            var vertexActions = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 36,
                ItemWidth = double.NaN,
            };
            vertexActions.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolApplyRouteVertex_{connectionId}_{capturedVertexIndex}",
                "Apply Bend",
                moveRouteDescriptor?.CanExecute ?? true,
                () => TryApplyRouteVertexPosition(connectionId, capturedVertexIndex, xEditor.Text, yEditor.Text),
                CreateActionTooltip(moveRouteDescriptor)));
            vertexActions.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolRemoveRouteVertex_{connectionId}_{capturedVertexIndex}",
                "Remove Bend",
                removeRouteDescriptor?.CanExecute ?? true,
                () => TryExecuteCommand(
                    "connections.route-vertex.remove",
                    ("connectionId", connectionId),
                    ("vertexIndex", capturedVertexIndex.ToString(CultureInfo.InvariantCulture)),
                    ("updateStatus", bool.TrueString)),
                CreateActionTooltip(removeRouteDescriptor)));

            container.Children.Add(new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Child = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Vertex {capturedVertexIndex + 1}",
                        },
                        xEditor,
                        yEditor,
                        vertexActions,
                    },
                },
            });
        }

        return container;
    }

    private Button CreateHostedToolButton(string name, AsterGraphHostedActionDescriptor action)
        => CreateToolActionButton(name, action.Title, action.CanExecute, action.TryExecute, CreateActionTooltip(action));

    private Button CreateToolActionButton(string name, string content, bool isEnabled, Func<bool> execute, string? disabledReason = null)
    {
        var button = new Button
        {
            Name = name,
            Content = content,
            IsEnabled = isEnabled,
        };
        if (!string.IsNullOrWhiteSpace(disabledReason))
        {
            ToolTip.SetTip(button, disabledReason);
        }

        button.Classes.Add("astergraph-toolbar-action");
        AutomationProperties.SetName(button, content);
        button.Click += (_, args) =>
        {
            execute();
            args.Handled = true;
        };
        return button;
    }

    private static string? CreateActionTooltip(AsterGraphHostedActionDescriptor action)
        => CreateActionTooltip(action.DisabledReason, action.RecoveryHint);

    private static string? CreateActionTooltip(GraphEditorCommandDescriptorSnapshot? descriptor)
        => descriptor is null ? null : CreateActionTooltip(descriptor.DisabledReason, descriptor.RecoveryHint);

    private static string? CreateActionTooltip(string? disabledReason, string? recoveryHint)
    {
        if (string.IsNullOrWhiteSpace(recoveryHint))
        {
            return disabledReason;
        }

        return string.IsNullOrWhiteSpace(disabledReason)
            ? recoveryHint
            : $"{disabledReason}\n{recoveryHint}";
    }

    private bool TryApplyRouteVertexPosition(string connectionId, int vertexIndex, string? rawX, string? rawY)
    {
        if (Editor is null
            || !double.TryParse(rawX, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(rawY, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return false;
        }

        return TryExecuteCommand(
            "connections.route-vertex.move",
            ("connectionId", connectionId),
            ("vertexIndex", vertexIndex.ToString(CultureInfo.InvariantCulture)),
            ("worldX", x.ToString(CultureInfo.InvariantCulture)),
            ("worldY", y.ToString(CultureInfo.InvariantCulture)),
            ("updateStatus", bool.TrueString));
    }

    private bool CanApplyConnectionText(IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptors)
    {
        return commandDescriptors.TryGetValue("connections.label.set", out var labelSet) && labelSet.CanExecute
            || commandDescriptors.TryGetValue("connections.note.set", out var noteSet) && noteSet.CanExecute;
    }

    private bool TryExecuteConnectionTextCommands(string connectionId, string? label, string? noteText)
    {
        var labelApplied = TryExecuteCommand(
            "connections.label.set",
            ("connectionId", connectionId),
            ("label", label),
            ("updateStatus", bool.TrueString));
        var noteApplied = TryExecuteCommand(
            "connections.note.set",
            ("connectionId", connectionId),
            ("text", noteText),
            ("updateStatus", bool.TrueString));
        return labelApplied || noteApplied;
    }

    private GraphEditorAuthoringToolSurfaceState CreateAuthoringToolSurfaceState(GraphEditorSelectionSnapshot selection)
    {
        if (Editor is null)
        {
            return GraphEditorAuthoringToolSurfaceState.Empty;
        }

        var selectionActions = selection.SelectedNodeIds.Count == 0
            ? []
            : CreateToolActions(GraphEditorToolContextSnapshot.ForSelection(selection.SelectedNodeIds, selection.PrimarySelectedNodeId));
        var primaryNodeId = ResolvePrimarySelectedNodeId(selection);
        var nodeActions = string.IsNullOrWhiteSpace(primaryNodeId)
            ? []
            : CreateToolActions(GraphEditorToolContextSnapshot.ForNode(primaryNodeId, selection.SelectedNodeIds, selection.PrimarySelectedNodeId));
        var incidentConnectionIds = string.IsNullOrWhiteSpace(primaryNodeId)
            ? []
            : Editor.Connections
                .Where(connection =>
                    string.Equals(connection.SourceNodeId, primaryNodeId, StringComparison.Ordinal)
                    || string.Equals(connection.TargetNodeId, primaryNodeId, StringComparison.Ordinal))
                .Select(connection => connection.Id)
                .OrderBy(connectionId => connectionId, StringComparer.Ordinal)
                .ToArray();
        var connectionActionsById = incidentConnectionIds.ToDictionary(
            connectionId => connectionId,
            connectionId => (IReadOnlyList<AsterGraphHostedActionDescriptor>)CreateToolActions(
                GraphEditorToolContextSnapshot.ForConnection(connectionId, selection.SelectedNodeIds, selection.PrimarySelectedNodeId)),
            StringComparer.Ordinal);
        var documentConnectionsById = Editor.Session.Queries.CreateDocumentSnapshot()
            .Connections
            .ToDictionary(connection => connection.Id, StringComparer.Ordinal);
        var geometryByConnectionId = Editor.Session.Queries.GetConnectionGeometrySnapshots()
            .ToDictionary(geometry => geometry.ConnectionId, StringComparer.Ordinal);

        return new GraphEditorAuthoringToolSurfaceState(
            selection,
            selectionActions,
            primaryNodeId,
            nodeActions,
            connectionActionsById,
            documentConnectionsById,
            geometryByConnectionId,
            CreateCommandSurfaceActions(selectionActions, nodeActions, connectionActionsById));
    }

    private IReadOnlyList<AsterGraphHostedActionDescriptor> CreateToolActions(GraphEditorToolContextSnapshot context)
    {
        if (Editor is null)
        {
            return [];
        }

        return AsterGraphHostedActionFactory.CreateToolActions(
            Editor.Session,
            Editor.Session.Queries.GetToolDescriptors(context));
    }

    private static string? ResolvePrimarySelectedNodeId(GraphEditorSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!string.IsNullOrWhiteSpace(selection.PrimarySelectedNodeId))
        {
            return selection.PrimarySelectedNodeId;
        }

        return selection.SelectedNodeIds.Count == 1
            ? selection.SelectedNodeIds[0]
            : null;
    }

    private static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateCommandSurfaceActions(
        IReadOnlyList<AsterGraphHostedActionDescriptor> selectionActions,
        IReadOnlyList<AsterGraphHostedActionDescriptor> nodeActions,
        IReadOnlyDictionary<string, IReadOnlyList<AsterGraphHostedActionDescriptor>> connectionActionsById)
    {
        var actions = new List<AsterGraphHostedActionDescriptor>();
        actions.AddRange(selectionActions.Where(action => !IsDuplicateGlobalAction(action.Id)));
        actions.AddRange(nodeActions.Where(action => !IsDuplicateGlobalAction(action.Id)));

        if (connectionActionsById.Count == 1)
        {
            actions.AddRange(connectionActionsById.Values.Single());
        }

        return actions;
    }

    private static bool IsDuplicateGlobalAction(string actionId)
        => actionId is "selection-wrap-composite" or "node-enter-composite-scope";

    private bool TryExecuteCommand(string commandId, params (string Name, string? Value)[] arguments)
    {
        if (Editor is null)
        {
            return false;
        }

        return Editor.Session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                commandId,
                arguments
                    .Where(argument => argument.Value is not null)
                    .Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value!))
                    .ToList()));
    }

    private string CreateConnectionToolTitle(NodeViewModel selectedNode, ConnectionViewModel connection)
    {
        if (Editor is null)
        {
            return connection.Id;
        }

        if (string.Equals(connection.SourceNodeId, selectedNode.Id, StringComparison.Ordinal))
        {
            var targetTitle = Editor.FindNode(connection.TargetNodeId)?.Title ?? connection.TargetNodeId;
            return $"Outgoing to {targetTitle}";
        }

        var sourceTitle = Editor.FindNode(connection.SourceNodeId)?.Title ?? connection.SourceNodeId;
        return $"Incoming from {sourceTitle}";
    }

    private static string ResolveSelectionToolButtonName(AsterGraphHostedActionDescriptor action)
        => action.Id switch
        {
            "selection-create-group" => "PART_SelectionToolCreateGroupButton",
            "selection-wrap-composite" => "PART_SelectionToolWrapCompositeButton",
            "selection-align-left" => "PART_SelectionToolAlignLeftButton",
            "selection-align-center" => "PART_SelectionToolAlignCenterButton",
            "selection-align-right" => "PART_SelectionToolAlignRightButton",
            "selection-align-top" => "PART_SelectionToolAlignTopButton",
            "selection-align-middle" => "PART_SelectionToolAlignMiddleButton",
            "selection-align-bottom" => "PART_SelectionToolAlignBottomButton",
            "selection-distribute-horizontal" => "PART_SelectionToolDistributeHorizontalButton",
            "selection-distribute-vertical" => "PART_SelectionToolDistributeVerticalButton",
            "selection-snap-grid" => "PART_SelectionToolSnapGridButton",
            _ => $"PART_SelectionTool_{action.Id}",
        };

    private static string ResolveNodeToolButtonName(AsterGraphHostedActionDescriptor action)
        => action.Id switch
        {
            "node-inspect" => "PART_NodeToolInspectButton",
            "node-center" => "PART_NodeToolCenterButton",
            "node-toggle-surface-expansion" => "PART_NodeToolToggleExpansionButton",
            "node-delete" => "PART_NodeToolDeleteButton",
            "node-duplicate" => "PART_NodeToolDuplicateButton",
            "node-disconnect-incoming" => "PART_NodeToolDisconnectIncomingButton",
            "node-disconnect-outgoing" => "PART_NodeToolDisconnectOutgoingButton",
            "node-disconnect-all" => "PART_NodeToolDisconnectAllButton",
            "node-enter-composite-scope" => "PART_NodeToolEnterCompositeScopeButton",
            _ => $"PART_NodeTool_{action.Id}",
        };

    private static string ResolveConnectionToolButtonName(string connectionId, AsterGraphHostedActionDescriptor action)
        => action.Id switch
        {
            "connection-reconnect" => $"PART_ConnectionToolReconnect_{connectionId}",
            "connection-disconnect" => $"PART_ConnectionToolDisconnect_{connectionId}",
            "connection-clear-note" => $"PART_ConnectionToolClearNote_{connectionId}",
            _ => $"PART_ConnectionToolAction_{connectionId}_{action.Id}",
        };

}

internal sealed record GraphEditorAuthoringToolSurfaceState(
    GraphEditorSelectionSnapshot Selection,
    IReadOnlyList<AsterGraphHostedActionDescriptor> SelectionActions,
    string? PrimaryNodeId,
    IReadOnlyList<AsterGraphHostedActionDescriptor> NodeActions,
    IReadOnlyDictionary<string, IReadOnlyList<AsterGraphHostedActionDescriptor>> ConnectionActionsById,
    IReadOnlyDictionary<string, GraphConnection> DocumentConnectionsById,
    IReadOnlyDictionary<string, GraphEditorConnectionGeometrySnapshot> GeometryByConnectionId,
    IReadOnlyList<AsterGraphHostedActionDescriptor> CommandSurfaceActions)
{
    public static GraphEditorAuthoringToolSurfaceState Empty { get; } = new(
        new GraphEditorSelectionSnapshot([], null),
        [],
        null,
        [],
        new Dictionary<string, IReadOnlyList<AsterGraphHostedActionDescriptor>>(StringComparer.Ordinal),
        new Dictionary<string, GraphConnection>(StringComparer.Ordinal),
        new Dictionary<string, GraphEditorConnectionGeometrySnapshot>(StringComparer.Ordinal),
        []);
}
