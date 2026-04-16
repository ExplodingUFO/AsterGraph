using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private string LocalizeText(string key, string fallback)
        => _presentationLocalizationCoordinator.LocalizeText(key, fallback);

    private string LocalizeFormat(string key, string fallback, params object?[] arguments)
        => _presentationLocalizationCoordinator.LocalizeFormat(key, fallback, arguments);

    private string StatusText(string key, string fallback)
        => _presentationLocalizationCoordinator.StatusText(key, fallback);

    private string StatusText(string key, string fallback, params object?[] arguments)
        => _presentationLocalizationCoordinator.StatusText(key, fallback, arguments);

    private bool TrySetNodePresentationProvider(INodePresentationProvider? provider)
        => SetProperty(ref _nodePresentationProvider, provider, nameof(NodePresentationProvider));

    private bool TrySetLocalizationProvider(IGraphLocalizationProvider? provider)
        => SetProperty(ref _localizationProvider, provider, nameof(LocalizationProvider));

    private void SetStatus(string key, string fallback)
        => StatusMessage = StatusText(key, fallback);

    private void SetStatus(string key, string fallback, params object?[] arguments)
        => StatusMessage = StatusText(key, fallback, arguments);

    private void SetStatus((string Key, string Fallback) status)
        => SetStatus(status.Key, status.Fallback);

    private void SetStatus((string Key, string Fallback, object?[] Arguments) status)
        => SetStatus(status.Key, status.Fallback, status.Arguments);

    private void PublishRecoverableFailure(string code, string operation, string message, Exception? exception = null)
        => RecoverableFailureRaised?.Invoke(
            this,
            new GraphEditorRecoverableFailureEventArgs(code, operation, message, exception));

    private void PublishRuntimeDiagnostic(
        string code,
        string operation,
        string message,
        GraphEditorDiagnosticSeverity severity,
        Exception? exception = null)
        => DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(code, operation, message, severity, exception));

    private string CreateNodeId(string templateKey)
        => CreateUniqueId(Nodes.Select(node => node.Id), $"{templateKey}-");

    private string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
        => CreateNodeId(
            (definitionId?.Value ?? fallbackKey)
            .Replace(".", "-", StringComparison.Ordinal));

    private string CreateConnectionId()
        => CreateUniqueId(Connections.Select(connection => connection.Id), "connection-");

    private bool CanFitView()
        => Nodes.Count > 0 && _viewportWidth > 0 && _viewportHeight > 0;

    private bool CanReplaceIncomingConnection()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

    private bool CanRemoveConnectionsAsSideEffect()
        => CommandPermissions.Connections.AllowDelete || CommandPermissions.Connections.AllowDisconnect;

    private GraphEditorHistoryState CaptureHistoryState()
        => _historyStateCoordinator.CaptureHistoryState();

    private void RestoreHistoryState(GraphEditorHistoryState state, string status)
        => _historyStateCoordinator.RestoreHistoryState(state, status);

    private void PushCurrentHistoryState()
        => _historyStateCoordinator.PushCurrentHistoryState();

    private void PushHistoryState(GraphEditorHistoryState state)
        => _historyStateCoordinator.PushHistoryState(state);

    private GraphDocument CreateViewModelDocumentSnapshot()
        => new(
            Title,
            Description,
            Nodes.Select(node => node.ToModel()).ToList(),
            Connections.Select(connection => connection.ToModel()).ToList());

    private string CreateDocumentSignature()
        => CreateDocumentSignature(CreateViewModelDocumentSnapshot());

    private static string CreateDocumentSignature(GraphDocument document)
        => GraphEditorHistoryStateCoordinator.CreateDocumentSignature(document);

    private static GraphEditorBehaviorOptions ResolveBehaviorOptions(
        GraphEditorBehaviorOptions? behaviorOptions,
        GraphEditorStyleOptions styleOptions)
    {
        if (behaviorOptions is not null)
        {
            return behaviorOptions;
        }

        return GraphEditorBehaviorOptions.Default with
        {
            DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
            {
                EnableGridSnapping = styleOptions.Canvas.EnableGridSnapping,
                EnableAlignmentGuides = styleOptions.Canvas.EnableAlignmentGuides,
                SnapTolerance = styleOptions.Canvas.SnapTolerance,
            },
        };
    }

    private void UpdateDirtyState()
        => _historyStateCoordinator.UpdateDirtyState();

    private void UpdateDirtyState(string currentSignature)
        => _historyStateCoordinator.UpdateDirtyState(currentSignature);

    partial void OnZoomChanged(double value) => RaiseComputedPropertyChanges();

    partial void OnSelectedFragmentTemplateChanged(FragmentTemplateViewModel? value) => RaiseComputedPropertyChanges();

    partial void OnSelectedNodeChanged(NodeViewModel? value)
    {
        if (_suspendSelectionTracking)
        {
            return;
        }

        RefreshSelectionProjection();
    }

    partial void OnPendingSourceNodeChanged(NodeViewModel? value)
    {
        RaiseComputedPropertyChanges();
        NotifyPendingConnectionChanged();
    }

    partial void OnPendingSourcePortChanged(PortViewModel? value)
    {
        RaiseComputedPropertyChanges();
        NotifyPendingConnectionChanged();
    }

    partial void OnIsDirtyChanged(bool value) => RaiseComputedPropertyChanges();

    private void HandleNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _documentCollectionSynchronizer.HandleNodesCollectionChanged(args, HandleNodePropertyChanged);

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _documentCollectionSynchronizer.HandleConnectionsCollectionChanged(args);

    private void HandleSelectedNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _selectionStateSynchronizer.HandleSelectedNodesCollectionChanged();

    private void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
        => _nodePositionDirtyTracker.HandleNodePropertyChanged(sender, args);

    private void RaiseComputedPropertyChanges()
    {
        _commandStateNotifier.NotifyCanExecuteChanged(_computedStateCommands);

        _commandStateNotifier.NotifyPropertyChanged(OnPropertyChanged, ComputedPropertyNames);
    }

    private void RefreshSelectionProjection()
        => _selectionProjectionApplier.RefreshSelectionProjection();

    private void NotifyPendingConnectionChanged()
        => _retainedEventPublisher.PublishPendingConnectionChanged();

    private void NotifyDocumentChanged(
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null,
        string? statusMessage = null)
        => _retainedEventPublisher.PublishDocumentChanged(changeKind, nodeIds, connectionIds, statusMessage);

    private void NotifySelectionChanged()
        => _retainedEventPublisher.PublishSelectionChanged();

    private void NotifyViewportChanged()
    {
        ViewportChanged?.Invoke(
            this,
            new GraphEditorViewportChangedEventArgs(
                Zoom,
                PanX,
                PanY,
                ViewportWidth,
                ViewportHeight));
    }

    private void CoerceSelectionToExistingNodes()
        => _selectionStateSynchronizer.CoerceSelectionToExistingNodes();

    private void RemoveConnections(Func<ConnectionViewModel, bool> predicate, string status)
    {
        var removed = Connections.Where(predicate).ToList();
        if (removed.Count == 0)
        {
            SetStatus("editor.status.connection.remove.noMatches", "No matching connections to remove.");
            return;
        }

        foreach (var connection in removed)
        {
            Connections.Remove(connection);
        }

        MarkDirty(
            status,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            connectionIds: removed.Select(connection => connection.Id).ToList());
    }

    private static string CreateUniqueId(IEnumerable<string> existingIds, string prefix)
    {
        var ids = existingIds.ToHashSet(StringComparer.Ordinal);
        var next = 1;

        foreach (var id in ids)
        {
            if (!id.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var suffix = id[prefix.Length..];
            if (int.TryParse(suffix, out var value))
            {
                next = Math.Max(next, value + 1);
            }
        }

        string candidate;
        do
        {
            candidate = $"{prefix}{next:000}";
            next++;
        }
        while (ids.Contains(candidate));

        return candidate;
    }

    private void ApplyParameterValue(NodeParameterViewModel parameter, object? value)
        => _parameterEditCoordinator.ApplyParameterValue(parameter, value);
}
