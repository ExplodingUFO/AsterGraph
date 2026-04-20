using AsterGraph.Editor.Menus;
using AsterGraph.Editor;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Core.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Parameters;
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
        var supportsDefinitionMetadata = _descriptorSupport is not null;
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
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.parameters.edit", "capability", capabilities.CanEditNodeParameters),
                new GraphEditorFeatureDescriptorSnapshot("query.node-surface-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-groups", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-group-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.create", "capability", capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.delete", "capability", capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.break", "capability", capabilities.CanBreakConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.update", "capability", capabilities.CanUpdateViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.fit", "capability", capabilities.CanFitToViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.center", "capability", capabilities.CanCenterViewport),
                new GraphEditorFeatureDescriptorSnapshot("query.plugin-load-snapshots", "query", _descriptorSupport?.HasPluginLoader ?? false),
                new GraphEditorFeatureDescriptorSnapshot("query.registered-node-definitions", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.shared-selection-definition", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.selected-node-parameter-snapshots", "query", supportsDefinitionMetadata),
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

    public IReadOnlyList<INodeDefinition> GetRegisteredNodeDefinitions()
        => _descriptorSupport?.Definitions
            .OrderBy(definition => definition.Category, StringComparer.Ordinal)
            .ThenBy(definition => definition.DisplayName, StringComparer.Ordinal)
            .ToList()
            ?? [];

    public INodeDefinition? GetSharedSelectionDefinition()
    {
        ResolveSelectedNodesAndSharedDefinition(out _, out var definition);
        return definition;
    }

    public IReadOnlyList<GraphEditorNodeParameterSnapshot> GetSelectedNodeParameterSnapshots()
    {
        if (!ResolveSelectedNodesAndSharedDefinition(out var selectedNodes, out var definition)
            || definition is null)
        {
            return [];
        }

        var canEditParameters = GetCapabilitySnapshot().CanEditNodeParameters;
        return definition.Parameters
            .Select(parameter =>
            {
                var values = selectedNodes
                    .Select(node => ResolveNodeParameterValue(node, parameter))
                    .ToList();
                var firstValue = values[0];
                var hasMixedValues = values.Skip(1).Any(value => !NodeParameterValueAdapter.AreEquivalent(value, firstValue));
                var validation = NodeParameterValueAdapter.NormalizeValue(parameter, firstValue);

                return new GraphEditorNodeParameterSnapshot(
                    parameter,
                    hasMixedValues ? null : firstValue,
                    hasMixedValues,
                    canEditParameters && !parameter.Constraints.IsReadOnly,
                    validation.IsValid,
                    validation.ValidationError);
            })
            .ToList();
    }

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => _host.GetCommandDescriptors();

    public IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots()
        => _host.GetNodeSurfaceSnapshots();

    public IReadOnlyList<GraphEditorCompositeNodeSnapshot> GetCompositeNodeSnapshots()
        => _host.GetCompositeNodeSnapshots();

    public GraphEditorScopeNavigationSnapshot GetScopeNavigationSnapshot()
        => _host.GetScopeNavigationSnapshot();

    public IReadOnlyList<GraphNodeGroup> GetNodeGroups()
        => _host.GetNodeGroups();

    public IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots()
        => _host.GetNodeGroupSnapshots();

    public IReadOnlyList<GraphEditorPluginLoadSnapshot> GetPluginLoadSnapshots()
        => _pluginLoadSnapshots.ToList();

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _host.GetNodePositions();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => CreatePendingConnectionSnapshot();

    public IReadOnlyList<GraphEditorCompatibleConnectionTargetSnapshot> GetCompatibleConnectionTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatibleConnectionTargets(sourceNodeId, sourcePortId);

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
    {
        var compatibleTargets = _host.GetCompatiblePortTargets(sourceNodeId, sourcePortId);
        if (compatibleTargets.Count == 0)
        {
            return [];
        }

        var nodesById = _host.CreateActiveScopeDocumentSnapshot()
            .Nodes
            .ToDictionary(node => node.Id, StringComparer.Ordinal);

        return compatibleTargets
            .Select(target => CreateCompatibilityShimTarget(target, nodesById))
            .Where(target => target is not null)
            .Select(target => target!)
            .ToList();
    }
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

    private bool ResolveSelectedNodesAndSharedDefinition(
        out IReadOnlyList<GraphNode> selectedNodes,
        out INodeDefinition? definition)
    {
        selectedNodes = [];
        definition = null;

        if (_descriptorSupport is null)
        {
            return false;
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        var selection = _host.GetSelectionSnapshot();
        if (selection.SelectedNodeIds.Count == 0)
        {
            return false;
        }

        var nodesById = document.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var resolvedNodes = selection.SelectedNodeIds
            .Where(nodeId => nodesById.ContainsKey(nodeId))
            .Select(nodeId => nodesById[nodeId])
            .ToList();
        if (resolvedNodes.Count == 0)
        {
            return false;
        }

        var sharedDefinitionId = resolvedNodes[0].DefinitionId;
        if (sharedDefinitionId is null || resolvedNodes.Any(node => node.DefinitionId != sharedDefinitionId))
        {
            return false;
        }

        if (!_descriptorSupport.NodeCatalog.TryGetDefinition(sharedDefinitionId, out definition) || definition is null)
        {
            return false;
        }

        selectedNodes = resolvedNodes;
        return true;
    }

    private static object? ResolveNodeParameterValue(GraphNode node, NodeParameterDefinition parameter)
        => NodeParameterValueAdapter.NormalizeIncomingValue(
            node.ParameterValues?.FirstOrDefault(candidate => string.Equals(candidate.Key, parameter.Key, StringComparison.Ordinal))?.Value
            ?? parameter.DefaultValue);

#pragma warning disable CS0618
    private static CompatiblePortTarget? CreateCompatibilityShimTarget(
        GraphEditorCompatiblePortTargetSnapshot target,
        IReadOnlyDictionary<string, GraphNode> nodesById)
    {
        if (!nodesById.TryGetValue(target.NodeId, out var nodeModel))
        {
            return null;
        }

        var node = new NodeViewModel(nodeModel);
        var port = node.GetPort(target.PortId);
        return port is null
            ? null
            : new CompatiblePortTarget(node, port, target.Compatibility);
    }
#pragma warning restore CS0618
}
