using System.Diagnostics;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using Microsoft.Extensions.Logging;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现。
/// </summary>
public sealed class GraphEditorSession : IGraphEditorSession, IGraphEditorCommands, IGraphEditorQueries, IGraphEditorEvents, IGraphEditorDiagnostics
{
    private const int RecentDiagnosticsCapacity = 32;
    private readonly IGraphEditorSessionHost _host;
    private readonly IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private readonly List<GraphEditorDiagnostic> _recentDiagnostics = [];
    private ILogger? _logger;
    private ActivitySource? _activitySource;
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

    /// <summary>
    /// 初始化运行时会话。
    /// </summary>
    /// <param name="editor">底层兼容宿主。</param>
    /// <param name="diagnosticsSink">可选的宿主诊断发布器。</param>
    public GraphEditorSession(ViewModels.GraphEditorViewModel editor, IGraphEditorDiagnosticsSink? diagnosticsSink = null)
        : this(editor.SessionHost, diagnosticsSink)
    {
    }

    internal GraphEditorSession(IGraphEditorSessionHost host, IGraphEditorDiagnosticsSink? diagnosticsSink = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _diagnosticsSink = diagnosticsSink;
        _lastPendingConnectionSnapshot = CreatePendingConnectionSnapshot();
        _host.DocumentChanged += HandleDocumentChanged;
        _host.SelectionChanged += HandleSelectionChanged;
        _host.ViewportChanged += HandleViewportChanged;
        _host.FragmentExported += HandleFragmentExported;
        _host.FragmentImported += HandleFragmentImported;
        _host.PendingConnectionChanged += HandlePendingConnectionChanged;
        _host.RecoverableFailureRaised += HandleRecoverableFailureRaised;
        _host.DiagnosticPublished += HandleDiagnosticPublished;
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
        if (_mutationDepth == 0)
        {
            _batchEntryPendingConnectionSnapshot = _lastPendingConnectionSnapshot;
        }

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
    public event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;

    /// <inheritdoc />
    public event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailure;

    /// <inheritdoc />
    public void Undo()
        => Execute("history.undo", _host.Undo);

    /// <inheritdoc />
    public void Redo()
        => Execute("history.redo", _host.Redo);

    /// <inheritdoc />
    public void ClearSelection(bool updateStatus = false)
        => Execute("selection.clear", () => _host.ClearSelection(updateStatus));

    /// <inheritdoc />
    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId = null, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(nodeIds);
        Execute("selection.set", () => _host.SetSelection(nodeIds, primaryNodeId, updateStatus));
    }

    /// <inheritdoc />
    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);
        Execute("nodes.add", () => _host.AddNode(definitionId, preferredWorldPosition));
    }

    /// <inheritdoc />
    public void DeleteSelection()
        => Execute("selection.delete", _host.DeleteSelection);

    /// <inheritdoc />
    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(positions);
        Execute("nodes.move", () => _host.SetNodePositions(positions, updateStatus));
    }

    /// <inheritdoc />
    [Obsolete("Use StartConnection instead.")]
    public void BeginConnection(string sourceNodeId, string sourcePortId)
        => StartConnection(sourceNodeId, sourcePortId);

    /// <inheritdoc />
    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePortId);

        Execute("connections.begin", () => _host.StartConnection(sourceNodeId, sourcePortId));
    }

    /// <inheritdoc />
    public void CompleteConnection(string targetNodeId, string targetPortId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPortId);
        Execute("connections.complete", () => _host.CompleteConnection(targetNodeId, targetPortId));
    }

    /// <inheritdoc />
    public void CancelPendingConnection()
        => Execute("connections.cancel", _host.CancelPendingConnection);

    /// <inheritdoc />
    public void DeleteConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        Execute("connections.delete", () => _host.DeleteConnection(connectionId));
    }

    /// <inheritdoc />
    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(portId);
        Execute("connections.break", () => _host.BreakConnectionsForPort(nodeId, portId));
    }

    /// <inheritdoc />
    public void PanBy(double deltaX, double deltaY)
        => Execute("viewport.pan", () => _host.PanBy(deltaX, deltaY));

    /// <inheritdoc />
    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => Execute("viewport.zoom", () => _host.ZoomAt(factor, screenAnchor));

    /// <inheritdoc />
    public void UpdateViewportSize(double width, double height)
        => Execute("viewport.resize", () => _host.UpdateViewportSize(width, height));

    /// <inheritdoc />
    public void ResetView(bool updateStatus = true)
        => Execute("viewport.reset", () => _host.ResetView(updateStatus));

    /// <inheritdoc />
    public void FitToViewport(bool updateStatus = true)
        => Execute("viewport.fit", () => _host.FitToViewport(updateStatus));

    /// <inheritdoc />
    public void CenterViewOnNode(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        Execute("viewport.center-node", () => _host.CenterViewOnNode(nodeId));
    }

    /// <inheritdoc />
    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
        => Execute("viewport.center", () => _host.CenterViewAt(worldPoint, updateStatus));

    /// <inheritdoc />
    public void SaveWorkspace()
        => Execute("workspace.save", _host.SaveWorkspace);

    /// <inheritdoc />
    public bool LoadWorkspace()
        => Execute("workspace.load", _host.LoadWorkspace);

    /// <inheritdoc />
    public GraphDocument CreateDocumentSnapshot()
        => _host.CreateDocumentSnapshot();

    /// <inheritdoc />
    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => _host.GetSelectionSnapshot();

    /// <inheritdoc />
    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => _host.GetViewportSnapshot();

    /// <inheritdoc />
    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => _host.GetCapabilitySnapshot();

    /// <inheritdoc />
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _host.GetNodePositions();

    /// <inheritdoc />
    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => CreatePendingConnectionSnapshot();

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

    /// <inheritdoc />
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatibleTargets(sourceNodeId, sourcePortId);

    /// <inheritdoc />
    public GraphEditorInspectionSnapshot CaptureInspectionSnapshot()
    {
        var pendingConnection = CreatePendingConnectionSnapshot();
        return new(
            CreateDocumentSnapshot(),
            GetSelectionSnapshot(),
            GetViewportSnapshot(),
            GetCapabilitySnapshot(),
            pendingConnection,
            new GraphEditorStatusSnapshot(_host.CurrentStatusMessage),
            GetNodePositions().ToList(),
            GetRecentDiagnostics().ToList());
    }

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorDiagnostic> GetRecentDiagnostics(int maxCount = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount);
        if (maxCount == 0 || _recentDiagnostics.Count == 0)
        {
            return [];
        }

        var skip = Math.Max(0, _recentDiagnostics.Count - maxCount);
        return _recentDiagnostics.Skip(skip).ToList();
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
            PublishDiagnostic(diagnostic);
            return;
        }

        PublishDiagnostic(diagnostic);
        RecoverableFailure?.Invoke(this, failure);
    }

    internal void ConfigureInstrumentation(GraphEditorInstrumentationOptions? instrumentation)
    {
        _logger = instrumentation?.LoggerFactory?.CreateLogger(typeof(GraphEditorSession).FullName ?? nameof(GraphEditorSession));
        _activitySource = instrumentation?.ActivitySource;
    }

    internal void PublishDiagnostic(GraphEditorDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        if (_recentDiagnostics.Count == RecentDiagnosticsCapacity)
        {
            _recentDiagnostics.RemoveAt(0);
        }

        _recentDiagnostics.Add(diagnostic);
        _diagnosticsSink?.Publish(diagnostic);
        EmitInstrumentation(diagnostic);
    }

    private bool IsBatching => _mutationDepth > 0;

    private void EmitInstrumentation(GraphEditorDiagnostic diagnostic)
    {
        using var activity = _activitySource?.StartActivity(diagnostic.Operation, ActivityKind.Internal);
        if (activity is not null)
        {
            activity.SetTag("astergraph.diagnostic.code", diagnostic.Code);
            activity.SetTag("astergraph.diagnostic.operation", diagnostic.Operation);
            activity.SetTag("astergraph.diagnostic.severity", diagnostic.Severity.ToString());
            activity.SetTag("astergraph.diagnostic.message", diagnostic.Message);

            if (diagnostic.Exception is not null)
            {
                activity.SetTag("exception.type", diagnostic.Exception.GetType().FullName);
                activity.SetTag("exception.message", diagnostic.Exception.Message);
            }

            activity.SetStatus(
                diagnostic.Severity == GraphEditorDiagnosticSeverity.Error
                    ? ActivityStatusCode.Error
                    : ActivityStatusCode.Ok,
                diagnostic.Exception?.Message ?? diagnostic.Message);
        }

        _logger?.Log(
            ToLogLevel(diagnostic.Severity),
            diagnostic.Exception,
            "AsterGraph diagnostic {Code} ({Operation}): {Message}",
            diagnostic.Code,
            diagnostic.Operation,
            diagnostic.Message);
    }

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

    private static LogLevel ToLogLevel(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => LogLevel.Information,
            GraphEditorDiagnosticSeverity.Warning => LogLevel.Warning,
            GraphEditorDiagnosticSeverity.Error => LogLevel.Error,
            _ => LogLevel.None,
        };

    private void HandleRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
        => PublishRecoverableFailure(args);

    private void HandleDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => PublishDiagnostic(diagnostic);

    private GraphEditorPendingConnectionSnapshot CreatePendingConnectionSnapshot()
        => _host.GetPendingConnectionSnapshot();

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
