using AsterGraph.Editor;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
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

    public IGraphEditorMutationScope BeginMutation(string? label = null)
    {
        if (_mutationDepth == 0)
        {
            _batchEntryPendingConnectionSnapshot = _lastPendingConnectionSnapshot;
        }

        _mutationDepth++;
        _currentMutationLabel ??= label;
        return new GraphEditorMutationScope(this, label);
    }

    private bool IsBatching => _mutationDepth > 0;

    private void HandleDocumentChanged(object? sender, GraphEditorDocumentChangedEventArgs args)
    {
        _documentRevision++;

        if (!IsBatching)
        {
            DocumentChanged?.Invoke(this, args);
            return;
        }

        _pendingDocumentChanged = _pendingDocumentChanged is null
            ? args
            : MergeDocumentChanged(_pendingDocumentChanged, args);
    }

    private void HandleSelectionChanged(object? sender, GraphEditorSelectionChangedEventArgs args)
    {
        if (!IsBatching)
        {
            SelectionChanged?.Invoke(this, args);
            return;
        }

        _pendingSelectionChanged = args;
    }

    private void HandleViewportChanged(object? sender, GraphEditorViewportChangedEventArgs args)
    {
        if (!IsBatching)
        {
            ViewportChanged?.Invoke(this, args);
            return;
        }

        _pendingViewportChanged = args;
    }

    private void HandleFragmentExported(object? sender, GraphEditorFragmentEventArgs args)
    {
        if (!IsBatching)
        {
            FragmentExported?.Invoke(this, args);
            return;
        }

        _pendingFragmentExported.Add(args);
    }

    private void HandleFragmentImported(object? sender, GraphEditorFragmentEventArgs args)
    {
        if (!IsBatching)
        {
            FragmentImported?.Invoke(this, args);
            return;
        }

        _pendingFragmentImported.Add(args);
    }

    private void HandlePendingConnectionChanged(object? sender, GraphEditorPendingConnectionChangedEventArgs args)
    {
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

        PendingConnectionChanged?.Invoke(this, args);
    }

    private void PublishCommandExecuted(string commandId)
    {
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

        CommandExecuted?.Invoke(this, args);
    }

    private void FlushPendingEvents()
    {
        if (_pendingDocumentChanged is not null)
        {
            DocumentChanged?.Invoke(this, _pendingDocumentChanged);
            _pendingDocumentChanged = null;
        }

        foreach (var args in _pendingFragmentExported)
        {
            FragmentExported?.Invoke(this, args);
        }
        _pendingFragmentExported.Clear();

        foreach (var args in _pendingFragmentImported)
        {
            FragmentImported?.Invoke(this, args);
        }
        _pendingFragmentImported.Clear();

        if (_pendingSelectionChanged is not null)
        {
            SelectionChanged?.Invoke(this, _pendingSelectionChanged);
            _pendingSelectionChanged = null;
        }

        if (_pendingViewportChanged is not null)
        {
            ViewportChanged?.Invoke(this, _pendingViewportChanged);
            _pendingViewportChanged = null;
        }

        if (_pendingPendingConnectionChanged is not null)
        {
            if (_batchEntryPendingConnectionSnapshot != _pendingPendingConnectionChanged.PendingConnection)
            {
                PendingConnectionChanged?.Invoke(this, _pendingPendingConnectionChanged);
            }

            _pendingPendingConnectionChanged = null;
        }

        _batchEntryPendingConnectionSnapshot = null;

        foreach (var args in _pendingCommandExecuted)
        {
            CommandExecuted?.Invoke(this, args);
        }
        _pendingCommandExecuted.Clear();

        foreach (var args in _pendingRecoverableFailures)
        {
            RecoverableFailure?.Invoke(this, args);
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

    private sealed class GraphEditorMutationScope : IGraphEditorMutationScope
    {
        private readonly GraphEditorSession _owner;
        private bool _disposed;

        public GraphEditorMutationScope(GraphEditorSession owner, string? label)
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
