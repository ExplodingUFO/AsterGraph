using System.Diagnostics;
using System.Globalization;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using Microsoft.Extensions.Logging;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现。
/// </summary>
public sealed class GraphEditorSession : IGraphEditorSession, IGraphEditorAutomationRunner, IGraphEditorCommands, IGraphEditorQueries, IGraphEditorEvents, IGraphEditorDiagnostics
{
    private const int RecentDiagnosticsCapacity = 32;
    private readonly IGraphEditorSessionHost _host;
    private readonly IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private readonly GraphEditorSessionDescriptorSupport? _descriptorSupport;
    private readonly List<GraphEditorDiagnostic> _recentDiagnostics = [];
    private IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> _pluginContextMenuAugmentors = [];
    private IReadOnlyList<GraphEditorPluginLoadSnapshot> _pluginLoadSnapshots = [];
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
        : this(editor.SessionHost, diagnosticsSink, editor.CreateSessionDescriptorSupport())
    {
    }

    internal GraphEditorSession(
        IGraphEditorSessionHost host,
        IGraphEditorDiagnosticsSink? diagnosticsSink = null,
        GraphEditorSessionDescriptorSupport? descriptorSupport = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _diagnosticsSink = diagnosticsSink;
        _descriptorSupport = descriptorSupport;
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
    public IGraphEditorAutomationRunner Automation => this;

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
    public event EventHandler<GraphEditorAutomationStartedEventArgs>? AutomationStarted;

    /// <inheritdoc />
    public event EventHandler<GraphEditorAutomationProgressEventArgs>? AutomationProgress;

    /// <inheritdoc />
    public event EventHandler<GraphEditorAutomationCompletedEventArgs>? AutomationCompleted;

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
    public GraphEditorAutomationExecutionSnapshot Execute(GraphEditorAutomationRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startedArgs = new GraphEditorAutomationStartedEventArgs(
            request.RunId,
            request.Steps.Count,
            request.RunInMutationScope,
            request.MutationLabel);

        PublishDiagnostic(new GraphEditorDiagnostic(
            "automation.run.started",
            "automation.run",
            $"Automation run '{request.RunId}' started with {request.Steps.Count} step(s).",
            GraphEditorDiagnosticSeverity.Info));
        AutomationStarted?.Invoke(this, startedArgs);

        var stepResults = new List<GraphEditorAutomationStepExecutionSnapshot>(request.Steps.Count);
        string? failureCode = null;
        string? failureMessage = null;

        IGraphEditorMutationScope? mutationScope = null;
        try
        {
            if (request.RunInMutationScope)
            {
                mutationScope = BeginMutation(request.MutationLabel);
            }

            for (var stepIndex = 0; stepIndex < request.Steps.Count; stepIndex++)
            {
                var step = request.Steps[stepIndex];
                var stepResult = ExecuteAutomationStep(step, stepIndex);
                stepResults.Add(stepResult);

                if (!stepResult.Succeeded)
                {
                    failureCode ??= stepResult.FailureCode;
                    failureMessage ??= stepResult.FailureMessage;

                    PublishDiagnostic(new GraphEditorDiagnostic(
                        "automation.step.failed",
                        "automation.run",
                        stepResult.FailureMessage
                            ?? $"Automation step '{step.StepId}' ({step.Command.CommandId}) failed.",
                        GraphEditorDiagnosticSeverity.Error));
                }

                AutomationProgress?.Invoke(this, new GraphEditorAutomationProgressEventArgs(
                    request.RunId,
                    stepResults.Count,
                    request.Steps.Count,
                    request.RunInMutationScope,
                    request.MutationLabel,
                    stepResult));

                if (!stepResult.Succeeded && request.StopOnFailure)
                {
                    break;
                }
            }
        }
        finally
        {
            mutationScope?.Dispose();
        }

        var succeeded = failureCode is null;
        PublishDiagnostic(new GraphEditorDiagnostic(
            "automation.run.completed",
            "automation.run",
            succeeded
                ? $"Automation run '{request.RunId}' completed successfully ({stepResults.Count}/{request.Steps.Count} steps)."
                : $"Automation run '{request.RunId}' completed with failure after {stepResults.Count}/{request.Steps.Count} steps: {failureMessage ?? failureCode ?? "Unknown failure."}",
            succeeded ? GraphEditorDiagnosticSeverity.Info : GraphEditorDiagnosticSeverity.Warning));

        var result = new GraphEditorAutomationExecutionSnapshot(
            request.RunId,
            succeeded,
            request.RunInMutationScope,
            request.MutationLabel,
            stepResults.Count,
            request.Steps.Count,
            stepResults,
            CaptureInspectionSnapshot(),
            failureCode,
            failureMessage);

        AutomationCompleted?.Invoke(this, new GraphEditorAutomationCompletedEventArgs(result));
        return result;
    }

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
    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var executed = _host.TryExecuteCommand(command);
        if (!executed)
        {
            return false;
        }

        PublishCommandExecuted(command.CommandId);
        return true;
    }

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
    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
    {
        var capabilities = GetCapabilitySnapshot();
        var descriptors = _host.GetFeatureDescriptors()
            .Concat(
            [
                new GraphEditorFeatureDescriptorSnapshot("capability.undo", "capability", capabilities.CanUndo),
                new GraphEditorFeatureDescriptorSnapshot("capability.redo", "capability", capabilities.CanRedo),
                new GraphEditorFeatureDescriptorSnapshot("capability.copy-selection", "capability", capabilities.CanCopySelection),
                new GraphEditorFeatureDescriptorSnapshot("capability.paste", "capability", capabilities.CanPaste),
                new GraphEditorFeatureDescriptorSnapshot("capability.workspace.save", "capability", capabilities.CanSaveWorkspace),
                new GraphEditorFeatureDescriptorSnapshot("capability.workspace.load", "capability", capabilities.CanLoadWorkspace),
                new GraphEditorFeatureDescriptorSnapshot("capability.selection.set", "capability", capabilities.CanSetSelection),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.move", "capability", capabilities.CanMoveNodes),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.create", "capability", capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.delete", "capability", capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.break", "capability", capabilities.CanBreakConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.update", "capability", capabilities.CanUpdateViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.fit", "capability", capabilities.CanFitToViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.center", "capability", capabilities.CanCenterViewport),
                new GraphEditorFeatureDescriptorSnapshot("query.plugin-load-snapshots", "query", _descriptorSupport?.HasPluginLoader ?? false),
                new GraphEditorFeatureDescriptorSnapshot("surface.automation.runner", "surface", true),
                new GraphEditorFeatureDescriptorSnapshot("service.fragment-workspace", "service", _descriptorSupport?.HasFragmentWorkspaceService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.fragment-library", "service", _descriptorSupport?.HasFragmentLibraryService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.clipboard-payload-serializer", "service", _descriptorSupport?.HasClipboardPayloadSerializer ?? false),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.started", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.progress", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.completed", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("integration.diagnostics-sink", "integration", _diagnosticsSink is not null),
                new GraphEditorFeatureDescriptorSnapshot("integration.plugin-loader", "integration", _descriptorSupport?.HasPluginLoader ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.context-menu-augmentor", "integration", (_descriptorSupport?.HasContextMenuAugmentor ?? false) || _pluginContextMenuAugmentors.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("integration.node-presentation-provider", "integration", _descriptorSupport?.HasNodePresentationProvider ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.localization-provider", "integration", _descriptorSupport?.HasLocalizationProvider ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.instrumentation.logger", "integration", _logger is not null),
                new GraphEditorFeatureDescriptorSnapshot("integration.instrumentation.activity-source", "integration", _activitySource is not null),
            ])
            .GroupBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(descriptor => descriptor.Category, StringComparer.Ordinal)
            .ThenBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        return descriptors;
    }

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => _host.GetCommandDescriptors();

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorPluginLoadSnapshot> GetPluginLoadSnapshots()
        => _pluginLoadSnapshots.ToList();

    /// <inheritdoc />
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildContextMenuDescriptors(ContextMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commands = GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var stockItems = context.TargetKind switch
        {
            ContextMenuTargetKind.Canvas => BuildCanvasMenuDescriptors(context, commands),
            ContextMenuTargetKind.Selection => BuildSelectionMenuDescriptors(context, commands),
            ContextMenuTargetKind.Node => BuildNodeMenuDescriptors(context, commands),
            ContextMenuTargetKind.Port => BuildPortMenuDescriptors(context, commands),
            ContextMenuTargetKind.Connection => BuildConnectionMenuDescriptors(context, commands),
            _ => [],
        };

        if (_pluginContextMenuAugmentors.Count == 0)
        {
            return stockItems;
        }

        var currentItems = stockItems;
        foreach (var augmentor in _pluginContextMenuAugmentors)
        {
            try
            {
                currentItems = augmentor.Augment(new GraphEditorPluginMenuAugmentationContext(this, context, currentItems))
                    ?? throw new InvalidOperationException($"Plugin context menu augmentor '{augmentor.GetType().FullName}' returned null.");
            }
            catch (Exception exception)
            {
                PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                    "plugin.contextmenu.augment.failed",
                    "plugin.contextmenu.augment",
                    $"Plugin context menu augmentor failed: {augmentor.GetType().Name}. Using current menu.",
                    exception));
            }
        }

        return currentItems;
    }

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
#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatibleTargets(sourceNodeId, sourcePortId);
#pragma warning restore CS0618

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
            GetFeatureDescriptors().ToList(),
            GetRecentDiagnostics().ToList(),
            GetPluginLoadSnapshots().ToList());
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

    internal void SetPluginLoadSnapshots(IReadOnlyList<GraphEditorPluginLoadSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _pluginLoadSnapshots = snapshots.ToList();
    }

    internal void SetPluginContextMenuAugmentors(IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> augmentors)
    {
        ArgumentNullException.ThrowIfNull(augmentors);
        _pluginContextMenuAugmentors = augmentors.ToList();
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

    private GraphEditorAutomationStepExecutionSnapshot ExecuteAutomationStep(GraphEditorAutomationStep step, int stepIndex)
    {
        var descriptors = GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        if (!descriptors.TryGetValue(step.Command.CommandId, out var descriptor))
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.command-unknown",
                $"Automation command '{step.Command.CommandId}' is not discoverable in the current session.");
        }

        if (!descriptor.IsEnabled)
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.command-disabled",
                descriptor.DisabledReason
                    ?? $"Automation command '{step.Command.CommandId}' is disabled in the current session.");
        }

        try
        {
            if (!Commands.TryExecuteCommand(step.Command))
            {
                return CreateAutomationStepFailure(
                    step,
                    stepIndex,
                    "automation.step.dispatch-failed",
                    $"Automation command '{step.Command.CommandId}' could not be executed.");
            }
        }
        catch (Exception exception)
        {
            return CreateAutomationStepFailure(
                step,
                stepIndex,
                "automation.step.exception",
                $"Automation command '{step.Command.CommandId}' threw an exception: {exception.Message}");
        }

        return new GraphEditorAutomationStepExecutionSnapshot(stepIndex, step.StepId, step.Command.CommandId, true);
    }

    private static GraphEditorAutomationStepExecutionSnapshot CreateAutomationStepFailure(
        GraphEditorAutomationStep step,
        int stepIndex,
        string failureCode,
        string failureMessage)
        => new(stepIndex, step.StepId, step.Command.CommandId, false, failureCode, failureMessage);

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

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildCanvasMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var definitions = context.AvailableNodeDefinitions.Count > 0
            ? context.AvailableNodeDefinitions
            : _descriptorSupport?.Definitions ?? [];
        var addNode = GetCommandDescriptor(commands, "nodes.add");
        var fitView = GetCommandDescriptor(commands, "viewport.fit");
        var resetView = GetCommandDescriptor(commands, "viewport.reset");
        var save = GetCommandDescriptor(commands, "workspace.save");
        var load = GetCommandDescriptor(commands, "workspace.load");
        var importFragment = GetCommandDescriptor(commands, "fragments.import");
        var cancelPending = GetCommandDescriptor(commands, "connections.cancel");

        var addNodeGroups = definitions
            .GroupBy(definition => definition.Category)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new GraphEditorMenuItemDescriptorSnapshot(
                $"add-category-{group.Key}",
                group.Key,
                children: group
                    .OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
                    .Select(definition => new GraphEditorMenuItemDescriptorSnapshot(
                        $"add-node-{definition.Id.Value.Replace(".", "-", StringComparison.Ordinal)}",
                        definition.DisplayName,
                        CreateCommand(
                            "nodes.add",
                            ("definitionId", definition.Id.Value),
                            ("worldX", context.WorldPosition.X.ToString(CultureInfo.InvariantCulture)),
                            ("worldY", context.WorldPosition.Y.ToString(CultureInfo.InvariantCulture))),
                        iconKey: "node",
                        isEnabled: addNode.IsEnabled,
                        disabledReason: addNode.DisabledReason))
                    .ToList()))
            .ToList();

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("canvas-add-node", L("editor.menu.canvas.addNode", "Add Node"), children: addNodeGroups, iconKey: "add", isEnabled: addNode.IsEnabled, disabledReason: addNode.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("canvas-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-fit-view", L("editor.menu.canvas.fitView", "Fit View"), CreateCommand("viewport.fit"), iconKey: "fit", isEnabled: fitView.IsEnabled, disabledReason: fitView.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-reset-view", L("editor.menu.canvas.resetView", "Reset View"), CreateCommand("viewport.reset"), iconKey: "reset", isEnabled: resetView.IsEnabled, disabledReason: resetView.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("canvas-sep-2"),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-save", L("editor.menu.canvas.saveSnapshot", "Save Snapshot"), CreateCommand("workspace.save"), iconKey: "save", isEnabled: save.IsEnabled, disabledReason: save.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-load", L("editor.menu.canvas.loadSnapshot", "Load Snapshot"), CreateCommand("workspace.load"), iconKey: "load", isEnabled: load.IsEnabled, disabledReason: load.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-import-fragment", L("editor.menu.canvas.importFragment", "Import Fragment"), CreateCommand("fragments.import"), iconKey: "import", isEnabled: importFragment.IsEnabled, disabledReason: importFragment.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("canvas-cancel-pending", L("editor.menu.canvas.cancelPendingConnection", "Cancel Pending Connection"), CreateCommand("connections.cancel"), iconKey: "cancel", isEnabled: cancelPending.IsEnabled, disabledReason: cancelPending.DisabledReason),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildSelectionMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var selectedCount = context.SelectedNodeIds.Count > 0
            ? context.SelectedNodeIds.Count
            : GetSelectionSnapshot().SelectedNodeIds.Count;
        var delete = GetCommandDescriptor(commands, "selection.delete");
        var export = GetCommandDescriptor(commands, "fragments.export-selection");
        var alignLeft = GetCommandDescriptor(commands, "layout.align-left");
        var alignCenter = GetCommandDescriptor(commands, "layout.align-center");
        var alignRight = GetCommandDescriptor(commands, "layout.align-right");
        var alignTop = GetCommandDescriptor(commands, "layout.align-top");
        var alignMiddle = GetCommandDescriptor(commands, "layout.align-middle");
        var alignBottom = GetCommandDescriptor(commands, "layout.align-bottom");
        var distributeHorizontal = GetCommandDescriptor(commands, "layout.distribute-horizontal");
        var distributeVertical = GetCommandDescriptor(commands, "layout.distribute-vertical");

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-delete",
                selectedCount == 1
                    ? L("editor.menu.selection.delete.single", "Delete Selected Node")
                    : LF("editor.menu.selection.delete.multiple", "Delete {0} Selected Nodes", selectedCount),
                CreateCommand("selection.delete"),
                iconKey: "delete",
                isEnabled: delete.IsEnabled,
                disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-export",
                L("editor.menu.selection.exportFragment", "Export Fragment"),
                CreateCommand("fragments.export-selection"),
                iconKey: "export",
                isEnabled: export.IsEnabled,
                disabledReason: export.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("selection-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-align",
                L("editor.menu.selection.align", "Align"),
                iconKey: "align",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-left", L("editor.menu.selection.align.left", "Left"), CreateCommand("layout.align-left"), iconKey: "align-left", isEnabled: alignLeft.IsEnabled, disabledReason: alignLeft.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-center", L("editor.menu.selection.align.center", "Center"), CreateCommand("layout.align-center"), iconKey: "align-center", isEnabled: alignCenter.IsEnabled, disabledReason: alignCenter.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-right", L("editor.menu.selection.align.right", "Right"), CreateCommand("layout.align-right"), iconKey: "align-right", isEnabled: alignRight.IsEnabled, disabledReason: alignRight.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-top", L("editor.menu.selection.align.top", "Top"), CreateCommand("layout.align-top"), iconKey: "align-top", isEnabled: alignTop.IsEnabled, disabledReason: alignTop.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-middle", L("editor.menu.selection.align.middle", "Middle"), CreateCommand("layout.align-middle"), iconKey: "align-middle", isEnabled: alignMiddle.IsEnabled, disabledReason: alignMiddle.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-align-bottom", L("editor.menu.selection.align.bottom", "Bottom"), CreateCommand("layout.align-bottom"), iconKey: "align-bottom", isEnabled: alignBottom.IsEnabled, disabledReason: alignBottom.DisabledReason),
                ]),
            new GraphEditorMenuItemDescriptorSnapshot(
                "selection-distribute",
                L("editor.menu.selection.distribute", "Distribute"),
                iconKey: "distribute",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("selection-distribute-horizontal", L("editor.menu.selection.distribute.horizontal", "Horizontally"), CreateCommand("layout.distribute-horizontal"), iconKey: "distribute-horizontal", isEnabled: distributeHorizontal.IsEnabled, disabledReason: distributeHorizontal.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("selection-distribute-vertical", L("editor.menu.selection.distribute.vertical", "Vertically"), CreateCommand("layout.distribute-vertical"), iconKey: "distribute-vertical", isEnabled: distributeVertical.IsEnabled, disabledReason: distributeVertical.DisabledReason),
                ]),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildNodeMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedNodeId))
        {
            return [];
        }

        var document = CreateDocumentSnapshot();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedNodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return [];
        }

        var inspect = GetCommandDescriptor(commands, "nodes.inspect");
        var center = GetCommandDescriptor(commands, "viewport.center-node");
        var delete = GetCommandDescriptor(commands, "nodes.delete-by-id");
        var duplicate = GetCommandDescriptor(commands, "nodes.duplicate");
        var disconnectIncoming = GetCommandDescriptor(commands, "connections.disconnect-incoming");
        var disconnectOutgoing = GetCommandDescriptor(commands, "connections.disconnect-outgoing");
        var disconnectAll = GetCommandDescriptor(commands, "connections.disconnect-all");
        var connect = GetCommandDescriptor(commands, "connections.connect");
        var connectMenus = node.Outputs
            .Select(port => new GraphEditorMenuItemDescriptorSnapshot(
                $"node-connect-{node.Id}-{port.Id}",
                port.Label,
                children: BuildCompatibleTargetItems(document, node, port, commands),
                isEnabled: connect.IsEnabled,
                disabledReason: connect.DisabledReason))
            .ToList();

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("node-inspect", LF("editor.menu.node.inspect", "Inspect {0}", node.Title), CreateCommand("nodes.inspect", ("nodeId", node.Id)), iconKey: "inspect", isEnabled: inspect.IsEnabled, disabledReason: inspect.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("node-center", L("editor.menu.node.centerViewHere", "Center View Here"), CreateCommand("viewport.center-node", ("nodeId", node.Id)), iconKey: "center", isEnabled: center.IsEnabled, disabledReason: center.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("node-sep-1"),
            new GraphEditorMenuItemDescriptorSnapshot("node-delete", L("editor.menu.node.deleteNode", "Delete Node"), CreateCommand("nodes.delete-by-id", ("nodeId", node.Id)), iconKey: "delete", isEnabled: delete.IsEnabled, disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("node-duplicate", L("editor.menu.node.duplicateNode", "Duplicate Node"), CreateCommand("nodes.duplicate", ("nodeId", node.Id)), iconKey: "duplicate", isEnabled: duplicate.IsEnabled, disabledReason: duplicate.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot(
                "node-disconnect",
                L("editor.menu.node.disconnect", "Disconnect"),
                iconKey: "disconnect",
                children:
                [
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-in", L("editor.menu.node.disconnect.incoming", "Incoming"), CreateCommand("connections.disconnect-incoming", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectIncoming.IsEnabled, disabledReason: disconnectIncoming.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-out", L("editor.menu.node.disconnect.outgoing", "Outgoing"), CreateCommand("connections.disconnect-outgoing", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectOutgoing.IsEnabled, disabledReason: disconnectOutgoing.DisabledReason),
                    new GraphEditorMenuItemDescriptorSnapshot("node-disconnect-all", L("editor.menu.node.disconnect.all", "All"), CreateCommand("connections.disconnect-all", ("nodeId", node.Id)), iconKey: "disconnect", isEnabled: disconnectAll.IsEnabled, disabledReason: disconnectAll.DisabledReason),
                ]),
            new GraphEditorMenuItemDescriptorSnapshot("node-create-connection", L("editor.menu.node.createConnectionFrom", "Create Connection From"), children: connectMenus, iconKey: "connect", isEnabled: connect.IsEnabled, disabledReason: connect.DisabledReason),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildPortMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedPortNodeId) || string.IsNullOrWhiteSpace(context.ClickedPortId))
        {
            return [];
        }

        var document = CreateDocumentSnapshot();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedPortNodeId, StringComparison.Ordinal));
        var port = node?.Inputs.Concat(node.Outputs).FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedPortId, StringComparison.Ordinal));
        if (node is null || port is null)
        {
            return [];
        }

        if (port.Direction == PortDirection.Output)
        {
            var start = GetCommandDescriptor(commands, "connections.start");
            var connect = GetCommandDescriptor(commands, "connections.connect");
            var compatibleTargets = BuildCompatibleTargetItems(document, node, port, commands);
            return
            [
                new GraphEditorMenuItemDescriptorSnapshot("port-start", L("editor.menu.port.startConnection", "Start Connection"), CreateCommand("connections.start", ("sourceNodeId", node.Id), ("sourcePortId", port.Id)), iconKey: "connect", isEnabled: start.IsEnabled, disabledReason: start.DisabledReason),
                new GraphEditorMenuItemDescriptorSnapshot("port-compatible-targets", L("editor.menu.port.compatibleTargets", "Compatible Targets"), children: compatibleTargets, iconKey: "compatible", isEnabled: connect.IsEnabled && compatibleTargets.Count > 0, disabledReason: connect.DisabledReason),
                GraphEditorMenuItemDescriptorSnapshot.Separator("port-sep-1"),
                new GraphEditorMenuItemDescriptorSnapshot("port-info", LF("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
            ];
        }

        var breakPort = GetCommandDescriptor(commands, "connections.break-port");
        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("port-break-connections", L("editor.menu.port.breakConnections", "Break Connections"), CreateCommand("connections.break-port", ("nodeId", node.Id), ("portId", port.Id)), iconKey: "disconnect", isEnabled: breakPort.IsEnabled, disabledReason: breakPort.DisabledReason),
            GraphEditorMenuItemDescriptorSnapshot.Separator("port-sep-2"),
            new GraphEditorMenuItemDescriptorSnapshot("port-info", LF("editor.menu.port.typeInfo", "Type: {0}", port.TypeId), iconKey: "type", isEnabled: false),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildConnectionMenuDescriptors(
        ContextMenuContext context,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        if (string.IsNullOrWhiteSpace(context.ClickedConnectionId))
        {
            return [];
        }

        var document = CreateDocumentSnapshot();
        var connection = document.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, context.ClickedConnectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return [];
        }

        var delete = GetCommandDescriptor(commands, "connections.delete");
        var conversionLabel = connection.ConversionId is null
            ? L("editor.menu.connection.noImplicitConversion", "No implicit conversion")
            : LF("editor.menu.connection.conversion", "Conversion: {0}", connection.ConversionId.Value);

        return
        [
            new GraphEditorMenuItemDescriptorSnapshot("connection-delete", L("editor.menu.connection.deleteConnection", "Delete Connection"), CreateCommand("connections.delete", ("connectionId", connection.Id)), iconKey: "delete", isEnabled: delete.IsEnabled, disabledReason: delete.DisabledReason),
            new GraphEditorMenuItemDescriptorSnapshot("connection-conversion", conversionLabel, iconKey: "conversion", isEnabled: false),
        ];
    }

    private IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildCompatibleTargetItems(
        GraphDocument document,
        GraphNode sourceNode,
        GraphPort sourcePort,
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands)
    {
        var connect = GetCommandDescriptor(commands, "connections.connect");
        var targets = GetCompatiblePortTargets(sourceNode.Id, sourcePort.Id);
        if (targets.Count == 0)
        {
            return [new GraphEditorMenuItemDescriptorSnapshot("no-compatible-targets", L("editor.menu.compatibility.noTargets", "No Compatible Targets"), iconKey: "info", isEnabled: false)];
        }

        return targets
            .GroupBy(target => target.NodeId)
            .OrderBy(group => group.First().NodeTitle, StringComparer.Ordinal)
            .Select(group => new GraphEditorMenuItemDescriptorSnapshot(
                $"compatible-node-{group.Key}",
                GetNodeMenuHeader(document, group.Key),
                children: group
                    .OrderBy(item => item.PortLabel, StringComparer.Ordinal)
                    .Select(target => new GraphEditorMenuItemDescriptorSnapshot(
                        $"compatible-port-{target.NodeId}-{target.PortId}",
                        target.Compatibility.Kind == AsterGraph.Abstractions.Compatibility.PortCompatibilityKind.ImplicitConversion
                            ? LF("editor.menu.compatibility.implicitTarget", "{0} (implicit: {1})", target.PortLabel, target.Compatibility.ConversionId!.Value)
                            : target.PortLabel,
                        CreateCommand("connections.connect", ("sourceNodeId", sourceNode.Id), ("sourcePortId", sourcePort.Id), ("targetNodeId", target.NodeId), ("targetPortId", target.PortId)),
                        iconKey: target.Compatibility.Kind == AsterGraph.Abstractions.Compatibility.PortCompatibilityKind.ImplicitConversion ? "conversion" : "connect",
                        isEnabled: connect.IsEnabled,
                        disabledReason: connect.DisabledReason))
                    .ToList()))
            .ToList();
    }

    private string GetNodeMenuHeader(GraphDocument document, string nodeId)
    {
        var node = document.Nodes.First(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        var duplicateCount = document.Nodes.Count(candidate => string.Equals(candidate.Title, node.Title, StringComparison.Ordinal));
        return duplicateCount > 1
            ? $"{node.Title} [{node.Id}]"
            : node.Title;
    }

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());

    private GraphEditorCommandDescriptorSnapshot GetCommandDescriptor(
        IReadOnlyDictionary<string, GraphEditorCommandDescriptorSnapshot> commands,
        string commandId)
        => commands.TryGetValue(commandId, out var descriptor)
            ? descriptor
            : new GraphEditorCommandDescriptorSnapshot(commandId, false);

    private string L(string key, string fallback)
        => _descriptorSupport?.Localize(key, fallback) ?? fallback;

    private string LF(string key, string fallback, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, L(key, fallback), arguments);

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
