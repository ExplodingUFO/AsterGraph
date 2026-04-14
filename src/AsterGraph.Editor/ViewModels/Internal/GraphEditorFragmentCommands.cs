using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal interface IGraphEditorFragmentCommandHost
    {
        GraphEditorCommandPermissions CommandPermissions { get; }

        GraphEditorBehaviorOptions BehaviorOptions { get; }

        IEnumerable<NodeViewModel> SelectedNodes { get; }

        string? SelectedNodeId { get; }

        string? SelectedNodeTitle { get; }

        IEnumerable<ConnectionViewModel> Connections { get; }

        string? SelectedFragmentTemplatePath { get; }

        IGraphTextClipboardBridge? TextClipboardBridge { get; }

        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer { get; }

        IGraphFragmentWorkspaceService FragmentWorkspaceService { get; }

        IGraphFragmentLibraryService FragmentLibraryService { get; }

        void StoreSelectionClipboard(GraphSelectionFragment fragment);

        GraphSelectionFragment? PeekSelectionClipboard();

        GraphPoint GetNextPasteOrigin();

        string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey);

        string CreateConnectionId();

        void ApplyNodePresentation(NodeViewModel node);

        void AddNode(NodeViewModel node);

        void AddConnection(ConnectionViewModel connection);

        void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode);

        void RefreshFragmentTemplates();

        void RaiseComputedPropertyChanges();

        string StatusText(string key, string fallback, params object?[] arguments);

        string SetStatus(string key, string fallback, params object?[] arguments);

        string MarkDirty(string status);

        void PublishRuntimeDiagnostic(string code, string operation, string message, GraphEditorDiagnosticSeverity severity, Exception? exception = null);

        void RaiseFragmentExported(string path, GraphSelectionFragment fragment);

        void RaiseFragmentImported(string path, GraphSelectionFragment fragment);
    }

    internal sealed class GraphEditorFragmentCommands
    {
        private readonly GraphEditorFragmentTransferSupport _transferSupport;
        private readonly GraphEditorFragmentClipboardCommands _clipboardCommands;
        private readonly GraphEditorFragmentWorkspaceCommands _workspaceCommands;
        private readonly GraphEditorFragmentTemplateCommands _templateCommands;

        internal GraphEditorFragmentCommands(IGraphEditorFragmentCommandHost host)
        {
            ArgumentNullException.ThrowIfNull(host);

            _transferSupport = new GraphEditorFragmentTransferSupport(host);
            _clipboardCommands = new GraphEditorFragmentClipboardCommands(host, _transferSupport);
            _workspaceCommands = new GraphEditorFragmentWorkspaceCommands(host, _transferSupport);
            _templateCommands = new GraphEditorFragmentTemplateCommands(host, _transferSupport, _workspaceCommands);
        }

        internal GraphSelectionFragment? CreateSelectionFragment()
            => _transferSupport.CreateSelectionFragment();

        internal string? PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
            => _transferSupport.PasteFragment(fragment, actionPrefix);

        internal async Task CopySelectionAsync()
            => await _clipboardCommands.CopySelectionAsync();

        internal void ExportSelectionFragment()
            => _workspaceCommands.ExportSelectionFragment();

        internal void ExportSelectionAsTemplate()
            => _templateCommands.ExportSelectionAsTemplate();

        internal bool ExportSelectionFragmentTo(string path)
            => _workspaceCommands.ExportSelectionFragmentTo(path);

        internal async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
            => await _clipboardCommands.GetBestAvailableClipboardFragmentAsync();

        internal async Task PasteSelectionAsync()
            => await _clipboardCommands.PasteSelectionAsync();

        internal void ImportFragment()
            => _workspaceCommands.ImportFragment();

        internal void ClearFragment()
            => _workspaceCommands.ClearFragment();

        internal void ImportSelectedTemplate()
            => _templateCommands.ImportSelectedTemplate();

        internal void DeleteSelectedTemplate()
            => _templateCommands.DeleteSelectedTemplate();

        internal bool ImportFragmentFrom(string path)
            => _workspaceCommands.ImportFragmentFrom(path);
    }
}
