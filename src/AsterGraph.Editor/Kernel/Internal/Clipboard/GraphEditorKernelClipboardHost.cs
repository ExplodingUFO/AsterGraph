using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelClipboardHost : IGraphEditorKernelClipboardHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelClipboardHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorCommandPermissions IGraphEditorKernelClipboardHost.CommandPermissions => _owner._behaviorOptions.Commands;

        GraphDocument IGraphEditorKernelClipboardHost.ActiveScopeDocument => _owner.CreateActiveScopeDocumentSnapshot();

        IReadOnlyList<string> IGraphEditorKernelClipboardHost.SelectedNodeIds => _owner._selectedNodeIds;

        string? IGraphEditorKernelClipboardHost.PrimarySelectedNodeId => _owner._primarySelectedNodeId;

        IGraphTextClipboardBridge? IGraphEditorKernelClipboardHost.TextClipboardBridge => _owner._textClipboardBridge;

        IGraphClipboardPayloadSerializer IGraphEditorKernelClipboardHost.ClipboardPayloadSerializer => _owner._clipboardPayloadSerializer;

        bool IGraphEditorKernelClipboardHost.HasClipboardContent => _owner._selectionClipboard.HasContent;

        void IGraphEditorKernelClipboardHost.StoreSelectionClipboard(GraphSelectionFragment fragment)
            => _owner.StoreSelectionClipboard(fragment);

        GraphSelectionFragment? IGraphEditorKernelClipboardHost.PeekSelectionClipboard()
            => _owner.PeekSelectionClipboard();

        GraphPoint IGraphEditorKernelClipboardHost.GetNextPasteOrigin()
            => _owner.GetNextPasteOrigin();

        string IGraphEditorKernelClipboardHost.CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey, IEnumerable<string> reservedIds)
            => _owner.CreateNodeId(definitionId, fallbackKey, reservedIds);

        string IGraphEditorKernelClipboardHost.CreateConnectionId()
            => _owner.CreateConnectionId();

        void IGraphEditorKernelClipboardHost.UpdateActiveScopeDocument(GraphDocument document)
            => _owner.ApplyActiveScopeDocument(document);

        void IGraphEditorKernelClipboardHost.SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
            => _owner.SetSelection(nodeIds, primaryNodeId, updateStatus);

        void IGraphEditorKernelClipboardHost.SetStatus(string statusMessage)
            => _owner.CurrentStatusMessage = statusMessage;

        void IGraphEditorKernelClipboardHost.MarkDirty(
            string status,
            GraphEditorDocumentChangeKind changeKind,
            IReadOnlyList<string>? nodeIds,
            IReadOnlyList<string>? connectionIds,
            bool preserveStatus)
            => _owner.MarkDirty(status, changeKind, nodeIds, connectionIds, preserveStatus);
    }
}
