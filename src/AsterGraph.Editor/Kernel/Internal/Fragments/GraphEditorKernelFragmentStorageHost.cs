using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelFragmentStorageHost : IGraphEditorKernelFragmentStorageHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelFragmentStorageHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        GraphEditorCommandPermissions IGraphEditorKernelFragmentStorageHost.CommandPermissions
            => _owner._behaviorOptions.Commands;

        GraphEditorBehaviorOptions IGraphEditorKernelFragmentStorageHost.BehaviorOptions
            => _owner._behaviorOptions;

        IGraphFragmentWorkspaceService IGraphEditorKernelFragmentStorageHost.FragmentWorkspaceService
            => _owner._fragmentWorkspaceService;

        IGraphFragmentLibraryService IGraphEditorKernelFragmentStorageHost.FragmentLibraryService
            => _owner._fragmentLibraryService;

        string? IGraphEditorKernelFragmentStorageHost.SelectedNodeTitle
            => _owner.GetSelectedNodeTitle();

        void IGraphEditorKernelFragmentStorageHost.SetStatus(string statusMessage)
            => _owner.CurrentStatusMessage = statusMessage;

        void IGraphEditorKernelFragmentStorageHost.RaiseFragmentExported(string path, GraphSelectionFragment fragment)
            => _owner.FragmentExported?.Invoke(_owner, new GraphEditorFragmentEventArgs(path, fragment.Nodes.Count, fragment.Connections.Count));

        void IGraphEditorKernelFragmentStorageHost.RaiseFragmentImported(string path, GraphSelectionFragment fragment)
            => _owner.FragmentImported?.Invoke(_owner, new GraphEditorFragmentEventArgs(path, fragment.Nodes.Count, fragment.Connections.Count));
    }
}
