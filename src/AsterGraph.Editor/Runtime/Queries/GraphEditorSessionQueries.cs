using AsterGraph.Editor.Menus;
using AsterGraph.Editor;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public AsterGraph.Core.Models.GraphDocument CreateDocumentSnapshot()
        => _host.CreateDocumentSnapshot();

    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => _host.GetSelectionSnapshot();

    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => _host.GetViewportSnapshot();

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => _host.GetCapabilitySnapshot();

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

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => _host.GetCommandDescriptors();

    public IReadOnlyList<GraphEditorPluginLoadSnapshot> GetPluginLoadSnapshots()
        => _pluginLoadSnapshots.ToList();

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _host.GetNodePositions();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => CreatePendingConnectionSnapshot();

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatibleTargets(sourceNodeId, sourcePortId);
#pragma warning restore CS0618

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

    private GraphEditorPendingConnectionSnapshot CreatePendingConnectionSnapshot()
        => _host.GetPendingConnectionSnapshot();
}
