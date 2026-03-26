using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现。
/// </summary>
public sealed class GraphEditorSession : IGraphEditorSession, IGraphEditorCommands, IGraphEditorQueries, IGraphEditorEvents, IGraphEditorDiagnostics
{
    private readonly GraphEditorViewModel _editor;
    private readonly IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private int _mutationDepth;
    private string? _currentMutationLabel;
    private GraphEditorDocumentChangedEventArgs? _pendingDocumentChanged;
    private GraphEditorSelectionChangedEventArgs? _pendingSelectionChanged;
    private GraphEditorViewportChangedEventArgs? _pendingViewportChanged;
    private readonly List<GraphEditorFragmentEventArgs> _pendingFragmentExported = [];
    private readonly List<GraphEditorFragmentEventArgs> _pendingFragmentImported = [];
    private readonly List<GraphEditorCommandExecutedEventArgs> _pendingCommandExecuted = [];
    private readonly List<GraphEditorRecoverableFailureEventArgs> _pendingRecoverableFailures = [];

    /// <summary>
    /// 初始化运行时会话。
    /// </summary>
    /// <param name="editor">底层兼容立面。</param>
    public GraphEditorSession(GraphEditorViewModel editor, IGraphEditorDiagnosticsSink? diagnosticsSink = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _diagnosticsSink = diagnosticsSink;
        _editor.DocumentChanged += HandleDocumentChanged;
        _editor.SelectionChanged += HandleSelectionChanged;
        _editor.ViewportChanged += HandleViewportChanged;
        _editor.FragmentExported += HandleFragmentExported;
        _editor.FragmentImported += HandleFragmentImported;
    }

    /// <inheritdoc />
    public IGraphEditorCommands Commands => this;

    /// <inheritdoc />
    public IGraphEditorQueries Queries => this;

    /// <inheritdoc />
    public IGraphEditorEvents Events => this;

    /// <inheritdoc />
    public IGraphEditorDiagnostics Diagnostics => this;

    /// <inheritdoc />
    public IGraphEditorMutationScope BeginMutation(string? label = null)
    {
        _mutationDepth++;
        _currentMutationLabel ??= label;
        return new GraphEditorMutationScope(this, label);
    }

    /// <inheritdoc />
    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;

    /// <inheritdoc />
    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;

    /// <inheritdoc />
    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;

    /// <inheritdoc />
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;

    /// <inheritdoc />
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;

    /// <inheritdoc />
    public event EventHandler<GraphEditorCommandExecutedEventArgs>? CommandExecuted;

    /// <inheritdoc />
    public event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailure;

    /// <inheritdoc />
    public void Undo()
        => Execute("history.undo", _editor.Undo);

    /// <inheritdoc />
    public void Redo()
        => Execute("history.redo", _editor.Redo);

    /// <inheritdoc />
    public void ClearSelection(bool updateStatus = false)
        => Execute("selection.clear", () => _editor.ClearSelection(updateStatus));

    /// <inheritdoc />
    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);

        var template = _editor.NodeTemplates.FirstOrDefault(candidate => candidate.Definition.Id == definitionId)
            ?? throw new InvalidOperationException($"Node definition '{definitionId}' is not registered in the current editor catalog.");

        Execute("nodes.add", () => _editor.AddNode(template, preferredWorldPosition));
    }

    /// <inheritdoc />
    public void DeleteSelection()
        => Execute("selection.delete", _editor.DeleteSelection);

    /// <inheritdoc />
    public void PanBy(double deltaX, double deltaY)
        => Execute("viewport.pan", () => _editor.PanBy(deltaX, deltaY));

    /// <inheritdoc />
    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => Execute("viewport.zoom", () => _editor.ZoomAt(factor, screenAnchor));

    /// <inheritdoc />
    public void ResetView(bool updateStatus = true)
        => Execute("viewport.reset", () => _editor.ResetView(updateStatus));

    /// <inheritdoc />
    public void SaveWorkspace()
        => Execute("workspace.save", _editor.SaveWorkspace);

    /// <inheritdoc />
    public bool LoadWorkspace()
        => Execute("workspace.load", _editor.LoadWorkspace);

    /// <inheritdoc />
    public GraphDocument CreateDocumentSnapshot()
        => _editor.CreateDocumentSnapshot();

    /// <inheritdoc />
    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => new(
            _editor.SelectedNodes.Select(node => node.Id).ToList(),
            _editor.SelectedNode?.Id);

    /// <inheritdoc />
    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => new(
            _editor.Zoom,
            _editor.PanX,
            _editor.PanY,
            _editor.ViewportWidth,
            _editor.ViewportHeight);

    /// <inheritdoc />
    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => new(
            _editor.CanUndo,
            _editor.CanRedo,
            _editor.CanCopySelection,
            _editor.CanPaste,
            _editor.CanSaveWorkspace,
            _editor.CanLoadWorkspace);

    /// <inheritdoc />
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _editor.GetNodePositions();

    /// <inheritdoc />
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _editor.GetCompatibleTargets(sourceNodeId, sourcePortId);

    /// <inheritdoc />
    public GraphEditorInspectionSnapshot CaptureInspectionSnapshot()
        => new(
            CreateDocumentSnapshot(),
            GetSelectionSnapshot(),
            GetViewportSnapshot(),
            GetCapabilitySnapshot(),
            new GraphEditorPendingConnectionSnapshot(
                _editor.HasPendingConnection,
                _editor.PendingSourceNode?.Id,
                _editor.PendingSourcePort?.Id),
            new GraphEditorStatusSnapshot(_editor.StatusMessage),
            GetNodePositions().ToList(),
            GetRecentDiagnostics().ToList());

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorDiagnostic> GetRecentDiagnostics(int maxCount = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount);
        return [];
    }

    internal void PublishRecoverableFailure(GraphEditorRecoverableFailureEventArgs failure)
    {
        var diagnostic = new GraphEditorDiagnostic(
            failure.Code,
            failure.Operation,
            failure.Message,
            GraphEditorDiagnosticSeverity.Error,
            failure.Exception);

        if (IsBatching)
        {
            _pendingRecoverableFailures.Add(failure);
            _diagnosticsSink?.Publish(diagnostic);
            return;
        }

        _diagnosticsSink?.Publish(diagnostic);
        RecoverableFailure?.Invoke(this, failure);
    }

    private bool IsBatching => _mutationDepth > 0;

    private void HandleDocumentChanged(object? sender, GraphEditorDocumentChangedEventArgs args)
    {
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

    private void Execute(string commandId, Action action)
    {
        action();
        PublishCommandExecuted(commandId);
    }

    private T Execute<T>(string commandId, Func<T> action)
    {
        var result = action();
        PublishCommandExecuted(commandId);
        return result;
    }

    private void PublishCommandExecuted(string commandId)
    {
        var args = new GraphEditorCommandExecutedEventArgs(
            commandId,
            _currentMutationLabel,
            IsBatching,
            _editor.StatusMessage);

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
