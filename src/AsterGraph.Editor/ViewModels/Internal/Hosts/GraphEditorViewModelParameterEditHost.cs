using AsterGraph.Editor.Services;
using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelParameterEditHost : IGraphEditorParameterEditHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelParameterEditHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IReadOnlyList<NodeViewModel> IGraphEditorParameterEditHost.SelectedNodes => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorParameterEditHost.PrimarySelectedNode => _owner.SelectedNode;

        bool IGraphEditorParameterEditHost.CanEditNodeParameters => _owner.CanEditNodeParameters;

        string IGraphEditorParameterEditHost.StatusText(string key, string fallback, params object?[] arguments)
            => _owner.StatusText(key, fallback, arguments);

        void IGraphEditorParameterEditHost.SetStatus(string key, string fallback)
            => _owner.SetStatus(key, fallback);

        void IGraphEditorParameterEditHost.MarkDirty(string status)
            => _owner.MarkDirty(
                status,
                GraphEditorDocumentChangeKind.ParametersChanged,
                nodeIds: _owner.SelectedNodes.Select(node => node.Id).ToList());
    }
}
