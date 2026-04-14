using System.Diagnostics;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime.Internal;
using Microsoft.Extensions.Logging;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现。
/// </summary>
public sealed partial class GraphEditorSession : IGraphEditorSession, IGraphEditorAutomationRunner, IGraphEditorCommands, IGraphEditorQueries, IGraphEditorEvents, IGraphEditorDiagnostics
{
    private const int RecentDiagnosticsCapacity = 32;
    private readonly IGraphEditorSessionHost _host;
    private readonly IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private readonly GraphEditorSessionDescriptorSupport? _descriptorSupport;
    private readonly GraphEditorSessionStockMenuDescriptorBuilder _stockMenuDescriptorBuilder;
    private readonly GraphEditorSessionMutationBatchHost _mutationBatchHost;
    private readonly GraphEditorSessionMutationBatcher _mutationBatcher;
    private readonly List<GraphEditorDiagnostic> _recentDiagnostics = [];
    private IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> _pluginContextMenuAugmentors = [];
    private IReadOnlyList<GraphEditorPluginLoadSnapshot> _pluginLoadSnapshots = [];
    private bool _hasPluginTrustPolicy;
    private ILogger? _logger;
    private ActivitySource? _activitySource;

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
        _stockMenuDescriptorBuilder = new GraphEditorSessionStockMenuDescriptorBuilder(
            _host.CreateDocumentSnapshot,
            _host.GetSelectionSnapshot,
            _host.GetCompatiblePortTargets,
            Localize,
            () => _descriptorSupport?.Definitions ?? Array.Empty<INodeDefinition>());
        _mutationBatchHost = new GraphEditorSessionMutationBatchHost(this);
        _mutationBatcher = new GraphEditorSessionMutationBatcher(_mutationBatchHost);
        _host.DocumentChanged += _mutationBatcher.HandleDocumentChanged;
        _host.SelectionChanged += _mutationBatcher.HandleSelectionChanged;
        _host.ViewportChanged += _mutationBatcher.HandleViewportChanged;
        _host.FragmentExported += _mutationBatcher.HandleFragmentExported;
        _host.FragmentImported += _mutationBatcher.HandleFragmentImported;
        _host.PendingConnectionChanged += _mutationBatcher.HandlePendingConnectionChanged;
        _host.RecoverableFailureRaised += _mutationBatcher.HandleRecoverableFailureRaised;
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
        => _mutationBatcher.BeginMutation(label);

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

        _mutationBatcher.PublishCommandExecuted(command.CommandId);
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
                new GraphEditorFeatureDescriptorSnapshot("integration.plugin-trust-policy", "integration", (_descriptorSupport?.HasPluginTrustPolicy ?? false) || _hasPluginTrustPolicy),
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
        var stockItems = _stockMenuDescriptorBuilder.Build(context, commands);

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
                _mutationBatcher.PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
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
        => _host.GetPendingConnectionSnapshot();

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
        var pendingConnection = _host.GetPendingConnectionSnapshot();
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
        => _mutationBatcher.PublishRecoverableFailure(failure);

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

    internal void SetPluginTrustPolicyConfigured(bool isConfigured)
        => _hasPluginTrustPolicy = isConfigured;

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

    private void Execute(string commandId, Action action)
    {
        action();
        _mutationBatcher.PublishCommandExecuted(commandId);
    }

    private T Execute<T>(string commandId, Func<T> action)
    {
        var result = action();
        _mutationBatcher.PublishCommandExecuted(commandId);
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

    private static LogLevel ToLogLevel(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => LogLevel.Information,
            GraphEditorDiagnosticSeverity.Warning => LogLevel.Warning,
            GraphEditorDiagnosticSeverity.Error => LogLevel.Error,
            _ => LogLevel.None,
        };

    private void HandleDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => PublishDiagnostic(diagnostic);

    private string Localize(string key, string fallback)
        => _descriptorSupport?.Localize(key, fallback) ?? fallback;
}
