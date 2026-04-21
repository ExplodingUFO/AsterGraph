using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Core.Models;
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

        if (connections.Count == 0)
        {
            return;
        }

        commandDescriptors.TryGetValue("connections.label.set", out var labelDescriptor);
        commandDescriptors.TryGetValue("connections.note.set", out var noteDescriptor);
        commandDescriptors.TryGetValue("connections.reconnect", out var reconnectDescriptor);
        commandDescriptors.TryGetValue("connections.disconnect", out var disconnectDescriptor);

        foreach (var connection in connections)
        {
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
                    },
                },
            };
            _connectionToolList.Children.Add(card);
        }
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
