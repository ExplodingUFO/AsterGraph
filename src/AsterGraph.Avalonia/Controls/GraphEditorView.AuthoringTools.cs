using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class GraphEditorView
{
    private Border? _authoringToolsChrome;
    private TextBlock? _authoringToolsCaptionText;
    private WrapPanel? _nodeToolToolbar;
    private StackPanel? _connectionToolList;

    private void InitializeAuthoringToolControls()
    {
        _authoringToolsChrome = this.FindControl<Border>("PART_AuthoringToolsChrome");
        _authoringToolsCaptionText = this.FindControl<TextBlock>("PART_AuthoringToolsCaptionText");
        _nodeToolToolbar = this.FindControl<WrapPanel>("PART_NodeToolToolbar");
        _connectionToolList = this.FindControl<StackPanel>("PART_ConnectionToolList");
    }

    private void RefreshAuthoringToolSurface()
    {
        if (_authoringToolsChrome is null
            || _authoringToolsCaptionText is null
            || _nodeToolToolbar is null
            || _connectionToolList is null)
        {
            return;
        }

        _nodeToolToolbar.Children.Clear();
        _connectionToolList.Children.Clear();

        if (Editor is null)
        {
            _authoringToolsChrome.IsVisible = true;
            _authoringToolsCaptionText.Text = "Attach an editor to inspect node and edge authoring tools.";
            return;
        }

        if (Editor.SelectedNodes.Count != 1 || Editor.SelectedNode is null)
        {
            _authoringToolsChrome.IsVisible = true;
            _authoringToolsCaptionText.Text = "Select one node to project node and edge tooling surfaces.";
            return;
        }

        var selectedNode = Editor.SelectedNode;
        var commandDescriptors = Editor.Session.Queries.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var nodeSurface = Editor.Session.Queries.GetNodeSurfaceSnapshots()
            .FirstOrDefault(snapshot => string.Equals(snapshot.NodeId, selectedNode.Id, StringComparison.Ordinal));

        BuildNodeToolToolbar(selectedNode, nodeSurface, commandDescriptors);
        BuildConnectionToolList(selectedNode, commandDescriptors);

        _authoringToolsChrome.IsVisible = true;
        _authoringToolsCaptionText.Text = _connectionToolList.Children.Count == 0
            ? $"Node tools for {selectedNode.Title}."
            : $"Node tools for {selectedNode.Title} plus {_connectionToolList.Children.Count} incident connection editor(s).";
    }

    private void BuildNodeToolToolbar(
        NodeViewModel selectedNode,
        GraphEditorNodeSurfaceSnapshot? nodeSurface,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commandDescriptors)
    {
        if (_nodeToolToolbar is null)
        {
            return;
        }

        commandDescriptors.TryGetValue("nodes.surface.expand", out var toggleDescriptor);
        var nextExpansionState = nodeSurface?.ExpansionState == GraphNodeExpansionState.Expanded
            ? GraphNodeExpansionState.Collapsed
            : GraphNodeExpansionState.Expanded;
        var button = new Button
        {
            Name = "PART_NodeToolToggleExpansionButton",
            Content = nextExpansionState == GraphNodeExpansionState.Expanded
                ? "Expand Node Card"
                : "Collapse Node Card",
            IsEnabled = toggleDescriptor?.CanExecute ?? true,
        };
        button.Classes.Add("astergraph-toolbar-action");
        button.Click += (_, args) =>
        {
            Editor?.Session.Commands.TrySetNodeExpansionState(selectedNode.Id, nextExpansionState);
            args.Handled = true;
        };
        _nodeToolToolbar.Children.Add(button);
    }

    private void BuildConnectionToolList(
        NodeViewModel selectedNode,
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
        var documentConnectionsById = Editor.Session.Queries.CreateDocumentSnapshot()
            .Connections
            .ToDictionary(connection => connection.Id, StringComparer.Ordinal);
        var geometryByConnectionId = Editor.Session.Queries.GetConnectionGeometrySnapshots()
            .ToDictionary(geometry => geometry.ConnectionId, StringComparer.Ordinal);

        if (connections.Count == 0)
        {
            return;
        }

        commandDescriptors.TryGetValue("connections.label.set", out var labelDescriptor);
        commandDescriptors.TryGetValue("connections.note.set", out var noteDescriptor);
        commandDescriptors.TryGetValue("connections.route-vertex.insert", out var insertRouteDescriptor);
        commandDescriptors.TryGetValue("connections.route-vertex.move", out var moveRouteDescriptor);
        commandDescriptors.TryGetValue("connections.route-vertex.remove", out var removeRouteDescriptor);
        commandDescriptors.TryGetValue("connections.reconnect", out var reconnectDescriptor);
        commandDescriptors.TryGetValue("connections.disconnect", out var disconnectDescriptor);

        foreach (var connection in connections)
        {
            documentConnectionsById.TryGetValue(connection.Id, out var modelConnection);
            geometryByConnectionId.TryGetValue(connection.Id, out var geometry);
            var labelEditor = new TextBox
            {
                Name = $"PART_ConnectionToolLabelEditor_{connection.Id}",
                Text = connection.Label,
                Watermark = "Connection label",
            };
            labelEditor.Classes.Add("astergraph-input");

            var noteEditor = new TextBox
            {
                Name = $"PART_ConnectionToolNoteEditor_{connection.Id}",
                Text = connection.NoteText ?? string.Empty,
                Watermark = "Display note",
            };
            noteEditor.Classes.Add("astergraph-input");

            var actionBar = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 36,
                ItemWidth = double.NaN,
            };

            var applyButton = CreateToolActionButton(
                $"PART_ConnectionToolApply_{connection.Id}",
                "Apply Edge Text",
                (labelDescriptor?.CanExecute ?? true) && (noteDescriptor?.CanExecute ?? true),
                () =>
                {
                    Editor.Session.Commands.TrySetConnectionLabel(connection.Id, labelEditor.Text, updateStatus: true);
                    Editor.Session.Commands.TrySetConnectionNoteText(connection.Id, noteEditor.Text, updateStatus: true);
                    return true;
                });
            actionBar.Children.Add(applyButton);

            actionBar.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolReconnect_{connection.Id}",
                "Reconnect",
                reconnectDescriptor?.CanExecute ?? true,
                () => Editor.Session.Commands.TryReconnectConnection(connection.Id, updateStatus: true)));

            actionBar.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolDisconnect_{connection.Id}",
                "Disconnect",
                disconnectDescriptor?.CanExecute ?? true,
                () =>
                {
                    return Editor.Session.Commands.TryExecuteCommand(
                        new GraphEditorCommandInvocationSnapshot(
                            "connections.disconnect",
                            [new GraphEditorCommandArgumentSnapshot("connectionId", connection.Id)]));
                }));

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
                            Text = CreateConnectionToolTitle(selectedNode, connection),
                            FontWeight = FontWeight.SemiBold,
                        },
                        labelEditor,
                        noteEditor,
                        actionBar,
                        CreateConnectionRouteSection(
                            connection.Id,
                            modelConnection?.Presentation?.Route ?? GraphConnectionRoute.Empty,
                            geometry,
                            insertRouteDescriptor?.CanExecute ?? true,
                            moveRouteDescriptor?.CanExecute ?? true,
                            removeRouteDescriptor?.CanExecute ?? true),
                    },
                },
            };
            _connectionToolList.Children.Add(card);
        }
    }

    private Control CreateConnectionRouteSection(
        string connectionId,
        GraphConnectionRoute route,
        GraphEditorConnectionGeometrySnapshot? geometry,
        bool canInsert,
        bool canMove,
        bool canRemove)
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
                    canInsert,
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
                        return Editor.Session.Commands.TryInsertConnectionRouteVertex(
                            connectionId,
                            capturedSegmentIndex,
                            midpoint,
                            updateStatus: true);
                    }));
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
            var xEditor = new TextBox
            {
                Name = $"PART_ConnectionToolRouteVertexXEditor_{connectionId}_{capturedVertexIndex}",
                Text = vertex.X.ToString(CultureInfo.InvariantCulture),
                Watermark = "X",
            };
            xEditor.Classes.Add("astergraph-input");

            var yEditor = new TextBox
            {
                Name = $"PART_ConnectionToolRouteVertexYEditor_{connectionId}_{capturedVertexIndex}",
                Text = vertex.Y.ToString(CultureInfo.InvariantCulture),
                Watermark = "Y",
            };
            yEditor.Classes.Add("astergraph-input");

            var vertexActions = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 36,
                ItemWidth = double.NaN,
            };
            vertexActions.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolApplyRouteVertex_{connectionId}_{capturedVertexIndex}",
                "Apply Bend",
                canMove,
                () => TryApplyRouteVertexPosition(connectionId, capturedVertexIndex, xEditor.Text, yEditor.Text)));
            vertexActions.Children.Add(CreateToolActionButton(
                $"PART_ConnectionToolRemoveRouteVertex_{connectionId}_{capturedVertexIndex}",
                "Remove Bend",
                canRemove,
                () => Editor is not null
                    && Editor.Session.Commands.TryRemoveConnectionRouteVertex(
                        connectionId,
                        capturedVertexIndex,
                        updateStatus: true)));

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

    private Button CreateToolActionButton(string name, string content, bool isEnabled, Func<bool> execute)
    {
        var button = new Button
        {
            Name = name,
            Content = content,
            IsEnabled = isEnabled,
        };
        button.Classes.Add("astergraph-toolbar-action");
        button.Click += (_, args) =>
        {
            execute();
            args.Handled = true;
        };
        return button;
    }

    private bool TryApplyRouteVertexPosition(string connectionId, int vertexIndex, string? rawX, string? rawY)
    {
        if (Editor is null
            || !double.TryParse(rawX, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(rawY, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return false;
        }

        return Editor.Session.Commands.TryMoveConnectionRouteVertex(
            connectionId,
            vertexIndex,
            new GraphPoint(x, y),
            updateStatus: true);
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
}
