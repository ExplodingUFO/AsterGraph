using System;
using System.Collections.Generic;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelKernelProjectionHost : IGraphEditorKernelProjectionHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelKernelProjectionHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        void IGraphEditorKernelProjectionHost.RunInKernelProjectionScope(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            _owner._isApplyingKernelProjection = true;
            try
            {
                action();
            }
            finally
            {
                _owner._isApplyingKernelProjection = false;
            }
        }

        void IGraphEditorKernelProjectionHost.ApplyKernelDocumentCore(GraphDocument document, string status, bool markClean)
            => _owner.LoadDocument(document, status, markClean);

        void IGraphEditorKernelProjectionHost.ApplyKernelSelectionCore(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode)
            => _owner.SetSelectionCore(nodes, primaryNode);

        void IGraphEditorKernelProjectionHost.ApplyKernelViewportCore(GraphEditorViewportSnapshot snapshot)
        {
            _owner._viewportWidth = snapshot.ViewportWidth;
            _owner._viewportHeight = snapshot.ViewportHeight;
            _owner.Zoom = snapshot.Zoom;
            _owner.PanX = snapshot.PanX;
            _owner.PanY = snapshot.PanY;
            _owner.FitViewCommand.NotifyCanExecuteChanged();
            _owner.RaiseComputedPropertyChanges();
            _owner.NotifyViewportChanged();
        }

        void IGraphEditorKernelProjectionHost.ApplyKernelPendingConnectionCore(NodeViewModel? pendingNode, PortViewModel? pendingPort)
        {
            _owner.PendingSourceNode = pendingNode;
            _owner.PendingSourcePort = pendingPort;
        }

        NodeViewModel? IGraphEditorKernelProjectionHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);
    }
}
