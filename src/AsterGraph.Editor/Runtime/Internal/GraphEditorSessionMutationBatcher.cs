using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Runtime.Internal;

internal interface IGraphEditorSessionMutationBatchHost
{
    string CurrentStatusMessage { get; }

    GraphEditorPendingConnectionSnapshot CreatePendingConnectionSnapshot();

    void PublishDiagnostic(GraphEditorDiagnostic diagnostic);

    void RaiseDocumentChanged(GraphEditorDocumentChangedEventArgs args);

    void RaiseSelectionChanged(GraphEditorSelectionChangedEventArgs args);

    void RaiseViewportChanged(GraphEditorViewportChangedEventArgs args);

    void RaiseFragmentExported(GraphEditorFragmentEventArgs args);

    void RaiseFragmentImported(GraphEditorFragmentEventArgs args);

    void RaisePendingConnectionChanged(GraphEditorPendingConnectionChangedEventArgs args);

    void RaiseCommandExecuted(GraphEditorCommandExecutedEventArgs args);

    void RaiseRecoverableFailure(GraphEditorRecoverableFailureEventArgs args);
}

internal sealed class GraphEditorSessionMutationBatcher
{
    private readonly IGraphEditorSessionMutationBatchHost _host;
    private int _mutationDepth;
    private string? _currentMutationLabel;
    private GraphEditorDocumentChangedEventArgs? _pendingDocumentChanged;
    private GraphEditorSelectionChangedEventArgs? _pendingSelectionChanged;
    private GraphEditorViewportChangedEventArgs? _pendingViewportChanged;
    private GraphEditorPendingConnectionChangedEventArgs? _pendingPendingConnectionChanged;
    private GraphEditorPendingConnectionSnapshot _lastPendingConnectionSnapshot;
    private GraphEditorPendingConnectionSnapshot? _batchEntryPendingConnectionSnapshot;
    private readonly List<GraphEditorFragmentEventArgs> _pendingFragmentExported = [];
    private readonly List<GraphEditorFragmentEventArgs> _pendingFragmentImported = [];
    private readonly List<GraphEditorCommandExecutedEventArgs> _pendingCommandExecuted = [];
    private readonly List<GraphEditorRecoverableFailureEventArgs> _pendingRecoverableFailures = [];

    public GraphEditorSessionMutationBatcher(IGraphEditorSessionMutationBatchHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _lastPendingConnectionSnapshot = _host.CreatePendingConnectionSnapshot();
    }

    public IGraphEditorMutationScope BeginMutation(string? label = null)
    {
        if (_mutationDepth == 0)
        {
            _batchEntryPendingConnectionSnapshot = _lastPendingConnectionSnapshot;
        }

        _mutationDepth++;
        _currentMutationLabel ??= label;
        return new GraphEditorSessionMutationScope(this, label);
    }

    public void PublishRecoverableFailure(GraphEditorRecoverableFailureEventArgs failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        var diagnostic = new GraphEditorDiagnostic(
            failure.Code,
            failure.Operation,
            failure.Message,
            GraphEditorDiagnosticSeverity.Error,
            failure.Exception);

        if (IsBatching)
        {
            _pendingRecoverableFailures.Add(failure);
            _host.PublishDiagnostic(diagnostic);
            return;
        }

        _host.PublishDiagnostic(diagnostic);
        _host.RaiseRecoverableFailure(failure);
    }

    public void HandleDocumentChanged(object? sender, GraphEditorDocumentChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!IsBatching)
        {
            _host.RaiseDocumentChanged(args);
            return;
        }

        _pendingDocumentChanged = _pendingDocumentChanged is null
            ? args
            : MergeDocumentChanged(_pendingDocumentChanged, args);
    }

    public void HandleSelectionChanged(object? sender, GraphEditorSelectionChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!IsBatching)
        {
            _host.RaiseSelectionChanged(args);
            return;
        }

        _pendingSelectionChanged = args;
    }

    public void HandleViewportChanged(object? sender, GraphEditorViewportChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!IsBatching)
        {
            _host.RaiseViewportChanged(args);
            return;
        }

        _pendingViewportChanged = args;
    }

    public void HandleFragmentExported(object? sender, GraphEditorFragmentEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!IsBatching)
        {
            _host.RaiseFragmentExported(args);
            return;
        }

        _pendingFragmentExported.Add(args);
    }

    public void HandleFragmentImported(object? sender, GraphEditorFragmentEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!IsBatching)
        {
            _host.RaiseFragmentImported(args);
            return;
        }

        _pendingFragmentImported.Add(args);
    }

    public void HandlePendingConnectionChanged(object? sender, GraphEditorPendingConnectionChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var current = args.PendingConnection;
        if (_lastPendingConnectionSnapshot == current)
        {
            return;
        }

        if (!_lastPendingConnectionSnapshot.HasPendingConnection && !current.HasPendingConnection)
        {
            _lastPendingConnectionSnapshot = current;
            return;
        }

        _lastPendingConnectionSnapshot = current;

        if (IsBatching)
        {
            _pendingPendingConnectionChanged = args;
            return;
        }

        _host.RaisePendingConnectionChanged(args);
    }

    public void HandleRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
        => PublishRecoverableFailure(args);

    public void PublishCommandExecuted(string commandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        var args = new GraphEditorCommandExecutedEventArgs(
            commandId,
            _currentMutationLabel,
            IsBatching,
            _host.CurrentStatusMessage);

        if (IsBatching)
        {
            _pendingCommandExecuted.Add(args);
            return;
        }

        _host.RaiseCommandExecuted(args);
    }

    private bool IsBatching => _mutationDepth > 0;

    private void FlushPendingEvents()
    {
        if (_pendingDocumentChanged is not null)
        {
            _host.RaiseDocumentChanged(_pendingDocumentChanged);
            _pendingDocumentChanged = null;
        }

        foreach (var args in _pendingFragmentExported)
        {
            _host.RaiseFragmentExported(args);
        }

        _pendingFragmentExported.Clear();

        foreach (var args in _pendingFragmentImported)
        {
            _host.RaiseFragmentImported(args);
        }

        _pendingFragmentImported.Clear();

        if (_pendingSelectionChanged is not null)
        {
            _host.RaiseSelectionChanged(_pendingSelectionChanged);
            _pendingSelectionChanged = null;
        }

        if (_pendingViewportChanged is not null)
        {
            _host.RaiseViewportChanged(_pendingViewportChanged);
            _pendingViewportChanged = null;
        }

        if (_pendingPendingConnectionChanged is not null)
        {
            if (_batchEntryPendingConnectionSnapshot != _pendingPendingConnectionChanged.PendingConnection)
            {
                _host.RaisePendingConnectionChanged(_pendingPendingConnectionChanged);
            }

            _pendingPendingConnectionChanged = null;
        }

        _batchEntryPendingConnectionSnapshot = null;

        foreach (var args in _pendingCommandExecuted)
        {
            _host.RaiseCommandExecuted(args);
        }

        _pendingCommandExecuted.Clear();

        foreach (var args in _pendingRecoverableFailures)
        {
            _host.RaiseRecoverableFailure(args);
        }

        _pendingRecoverableFailures.Clear();
    }

    private static GraphEditorDocumentChangedEventArgs MergeDocumentChanged(
        GraphEditorDocumentChangedEventArgs current,
        GraphEditorDocumentChangedEventArgs next)
    {
        if (current.ChangeKind != next.ChangeKind)
        {
            return next;
        }

        return new GraphEditorDocumentChangedEventArgs(
            current.ChangeKind,
            current.NodeIds.Concat(next.NodeIds).Distinct(StringComparer.Ordinal).ToList(),
            current.ConnectionIds.Concat(next.ConnectionIds).Distinct(StringComparer.Ordinal).ToList(),
            next.StatusMessage ?? current.StatusMessage);
    }

    private sealed class GraphEditorSessionMutationScope : IGraphEditorMutationScope
    {
        private readonly GraphEditorSessionMutationBatcher _owner;
        private bool _disposed;

        public GraphEditorSessionMutationScope(GraphEditorSessionMutationBatcher owner, string? label)
        {
            _owner = owner;
            Label = label;
        }

        public string? Label { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner._mutationDepth--;
            if (_owner._mutationDepth == 0)
            {
                _owner._currentMutationLabel = null;
                _owner.FlushPendingEvents();
            }
        }
    }
}
