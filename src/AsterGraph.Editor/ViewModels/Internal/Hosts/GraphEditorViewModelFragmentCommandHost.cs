using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelFragmentCommandHost : IGraphEditorFragmentCommandHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelFragmentCommandHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorCommandPermissions IGraphEditorFragmentCommandHost.CommandPermissions => _owner.CommandPermissions;

        GraphEditorBehaviorOptions IGraphEditorFragmentCommandHost.BehaviorOptions => _owner.BehaviorOptions;

        IEnumerable<NodeViewModel> IGraphEditorFragmentCommandHost.SelectedNodes => _owner.SelectedNodes;

        string? IGraphEditorFragmentCommandHost.SelectedNodeId => _owner.SelectedNode?.Id;

        string? IGraphEditorFragmentCommandHost.SelectedNodeTitle => _owner.SelectedNode?.Title;

        IEnumerable<ConnectionViewModel> IGraphEditorFragmentCommandHost.Connections => _owner.Connections;

        string? IGraphEditorFragmentCommandHost.SelectedFragmentTemplatePath => _owner.SelectedFragmentTemplate?.Path;

        IGraphTextClipboardBridge? IGraphEditorFragmentCommandHost.TextClipboardBridge => _owner._textClipboardBridge;

        IGraphClipboardPayloadSerializer IGraphEditorFragmentCommandHost.ClipboardPayloadSerializer => _owner._clipboardPayloadSerializer;

        IGraphFragmentWorkspaceService IGraphEditorFragmentCommandHost.FragmentWorkspaceService => _owner._fragmentWorkspaceService;

        IGraphFragmentLibraryService IGraphEditorFragmentCommandHost.FragmentLibraryService => _owner._fragmentLibraryService;

        void IGraphEditorFragmentCommandHost.StoreSelectionClipboard(GraphSelectionFragment fragment)
            => _owner._selectionClipboard.Store(fragment);

        GraphSelectionFragment? IGraphEditorFragmentCommandHost.PeekSelectionClipboard()
            => _owner._selectionClipboard.Peek();

        GraphPoint IGraphEditorFragmentCommandHost.GetNextPasteOrigin()
            => _owner._selectionClipboard.GetNextPasteOrigin(_owner.GetViewportCenter());

        string IGraphEditorFragmentCommandHost.CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
            => _owner.CreateNodeId(definitionId, fallbackKey);

        string IGraphEditorFragmentCommandHost.CreateConnectionId()
            => _owner.CreateConnectionId();

        void IGraphEditorFragmentCommandHost.ApplyNodePresentation(NodeViewModel node)
            => _owner._presentationLocalizationCoordinator.ApplyNodePresentation(node);

        void IGraphEditorFragmentCommandHost.AddNode(NodeViewModel node)
            => _owner.Nodes.Add(node);

        void IGraphEditorFragmentCommandHost.AddConnection(ConnectionViewModel connection)
            => _owner.Connections.Add(connection);

        void IGraphEditorFragmentCommandHost.SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode)
            => _owner.SetSelection(nodes, primaryNode);

        void IGraphEditorFragmentCommandHost.RefreshFragmentTemplates()
            => _owner.RefreshFragmentTemplates();

        void IGraphEditorFragmentCommandHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();

        string IGraphEditorFragmentCommandHost.StatusText(string key, string fallback, params object?[] arguments)
            => _owner.StatusText(key, fallback, arguments);

        string IGraphEditorFragmentCommandHost.SetStatus(string key, string fallback, params object?[] arguments)
        {
            var status = _owner.StatusText(key, fallback, arguments);
            _owner.StatusMessage = status;
            return status;
        }

        string IGraphEditorFragmentCommandHost.MarkDirty(string status)
        {
            _owner.MarkDirty(status);
            return status;
        }

        void IGraphEditorFragmentCommandHost.PublishRuntimeDiagnostic(string code, string operation, string message, GraphEditorDiagnosticSeverity severity, Exception? exception)
            => _owner.PublishRuntimeDiagnostic(code, operation, message, severity, exception);

        void IGraphEditorFragmentCommandHost.RaiseFragmentExported(string path, GraphSelectionFragment fragment)
            => _owner.FragmentExported?.Invoke(
                _owner,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));

        void IGraphEditorFragmentCommandHost.RaiseFragmentImported(string path, GraphSelectionFragment fragment)
            => _owner.FragmentImported?.Invoke(
                _owner,
                new GraphEditorFragmentEventArgs(
                    path,
                    fragment.Nodes.Count,
                    fragment.Connections.Count));
    }
}
