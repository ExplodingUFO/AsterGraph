using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Runtime.Internal;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private sealed class GraphEditorSessionMutationBatchHost : IGraphEditorSessionMutationBatchHost
    {
        private readonly GraphEditorSession _owner;

        public GraphEditorSessionMutationBatchHost(GraphEditorSession owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        string IGraphEditorSessionMutationBatchHost.CurrentStatusMessage => _owner._host.CurrentStatusMessage;

        GraphEditorPendingConnectionSnapshot IGraphEditorSessionMutationBatchHost.CreatePendingConnectionSnapshot()
            => _owner._host.GetPendingConnectionSnapshot();

        void IGraphEditorSessionMutationBatchHost.PublishDiagnostic(GraphEditorDiagnostic diagnostic)
            => _owner.PublishDiagnostic(diagnostic);

        void IGraphEditorSessionMutationBatchHost.RaiseDocumentChanged(GraphEditorDocumentChangedEventArgs args)
            => _owner.DocumentChanged?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseSelectionChanged(GraphEditorSelectionChangedEventArgs args)
            => _owner.SelectionChanged?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseViewportChanged(GraphEditorViewportChangedEventArgs args)
            => _owner.ViewportChanged?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseFragmentExported(GraphEditorFragmentEventArgs args)
            => _owner.FragmentExported?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseFragmentImported(GraphEditorFragmentEventArgs args)
            => _owner.FragmentImported?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaisePendingConnectionChanged(GraphEditorPendingConnectionChangedEventArgs args)
            => _owner.PendingConnectionChanged?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseCommandExecuted(GraphEditorCommandExecutedEventArgs args)
            => _owner.CommandExecuted?.Invoke(_owner, args);

        void IGraphEditorSessionMutationBatchHost.RaiseRecoverableFailure(GraphEditorRecoverableFailureEventArgs args)
            => _owner.RecoverableFailure?.Invoke(_owner, args);
    }
}
