using AsterGraph.Editor.Menus;
using AsterGraph.Editor;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Core.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Parameters;
using AsterGraph.Editor.Runtime.Internal;
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

    public IReadOnlyList<string> GetSelectedNodeConnectionIds()
    {
        var selectedNodeIds = GetSelectionSnapshot().SelectedNodeIds.ToHashSet(StringComparer.Ordinal);
        if (selectedNodeIds.Count < 2)
        {
            return [];
        }

        return _host.CreateActiveScopeDocumentSnapshot()
            .Connections
            .Where(connection => selectedNodeIds.Contains(connection.SourceNodeId) && selectedNodeIds.Contains(connection.TargetNodeId))
            .Select(connection => connection.Id)
            .ToList();
    }

    public GraphEditorSelectionTransformSnapshot GetSelectionTransformSnapshot(GraphEditorSelectionTransformQuery? query = null)
    {
        query ??= new GraphEditorSelectionTransformQuery();
        var document = _host.CreateActiveScopeDocumentSnapshot();
        var selection = GetSelectionSnapshot();
        var selectedNodeIds = selection.SelectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var selectedNodes = document.Nodes
            .Where(node => selectedNodeIds.Contains(node.Id))
            .ToList();
        var selectedGroups = _host.GetNodeGroups()
            .Where(group => group.NodeIds.Any(selectedNodeIds.Contains))
            .Select(group => group.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var selectedConnectionIds = document.Connections
            .Where(connection => selectedNodeIds.Contains(connection.SourceNodeId) && selectedNodeIds.Contains(connection.TargetNodeId))
            .Select(connection => connection.Id)
            .Concat(selection.SelectedConnectionIds)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var previewDelta = NormalizeTransformDelta(query.PreviewDelta ?? new GraphPoint(0d, 0d), query.ConstrainPreviewToPrimaryAxis);
        var items = selectedNodes
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .Select(node => new GraphEditorSelectionTransformItemSnapshot(
                node.Id,
                node.Position,
                node.Size,
                node.Position + previewDelta))
            .ToList();
        var (boundsPosition, boundsSize) = CreateBounds(selectedNodes);
        var (rectangleNodeIds, rectangleConnectionIds) = ProjectSelectionRectangle(document, query);
        var emptyReason = selectedNodes.Count == 0
            ? "Select one or more nodes before transforming the selection."
            : null;

        return new GraphEditorSelectionTransformSnapshot(
            selectedNodes.Count > 0,
            selectedNodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal).ToList(),
            selectedConnectionIds,
            selectedGroups,
            boundsPosition,
            boundsSize,
            previewDelta,
            items,
            rectangleNodeIds,
            rectangleConnectionIds,
            emptyReason);
    }

    public GraphEditorSnapGuideSnapshot GetSnapGuideSnapshot(GraphEditorSnapGuideQuery? query = null)
    {
        query ??= new GraphEditorSnapGuideQuery();
        if (query.GridSize <= 0d)
        {
            return new GraphEditorSnapGuideSnapshot(false, query.GridSize, query.SelectionOnly, [], "Grid size must be positive.");
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        var selection = GetSelectionSnapshot();
        var selectedNodeIds = selection.SelectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var nodes = query.SelectionOnly
            ? document.Nodes.Where(node => selectedNodeIds.Contains(node.Id)).ToList()
            : document.Nodes.ToList();
        if (nodes.Count == 0)
        {
            return new GraphEditorSnapGuideSnapshot(
                false,
                query.GridSize,
                query.SelectionOnly,
                [],
                query.SelectionOnly
                    ? "Select one or more nodes before projecting snap guides."
                    : "Add one or more nodes before projecting snap guides.");
        }

        var items = nodes
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .Select(node =>
            {
                var snapped = SnapToGrid(node.Position, query.GridSize);
                return new GraphEditorSnapGuideItemSnapshot(
                    node.Id,
                    node.Position,
                    snapped,
                    snapped - node.Position,
                    query.GridSize);
            })
            .ToList();

        return new GraphEditorSnapGuideSnapshot(true, query.GridSize, query.SelectionOnly, items, null);
    }

    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => _host.GetViewportSnapshot();

    public GraphEditorSceneSnapshot GetSceneSnapshot()
    {
        var document = CreateDocumentSnapshot();
        return new(
            document,
            GetSelectionSnapshot(),
            GetViewportSnapshot(),
            GetNodeSurfaceSnapshots(),
            GetNodeGroupSnapshots(),
            CreateConnectionGeometrySnapshots(document),
            GetPendingConnectionSnapshot());
    }

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => _host.GetCapabilitySnapshot();

    public GraphEditorFragmentStorageSnapshot GetFragmentStorageSnapshot()
        => _host.GetFragmentStorageSnapshot();

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
                new GraphEditorFeatureDescriptorSnapshot("capability.layout.align", "capability", capabilities.CanAlignSelection),
                new GraphEditorFeatureDescriptorSnapshot("capability.layout.distribute", "capability", capabilities.CanDistributeSelection),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.parameters.edit", "capability", capabilities.CanEditNodeParameters),
                new GraphEditorFeatureDescriptorSnapshot("capability.export.scene-svg", "capability", (_descriptorSupport?.HasSceneSvgExportService ?? false) && GetSceneSnapshot().Document.Nodes.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("capability.export.scene-image", "capability", (_descriptorSupport?.HasSceneImageExportService ?? false) && GetSceneSnapshot().Document.Nodes.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("capability.export.scene-png", "capability", (_descriptorSupport?.HasSceneImageExportService ?? false) && GetSceneSnapshot().Document.Nodes.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("capability.export.scene-jpeg", "capability", (_descriptorSupport?.HasSceneImageExportService ?? false) && GetSceneSnapshot().Document.Nodes.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("query.scene-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.runtime-overlay-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.validation-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.layout-plan", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.selected-node-connection-ids", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-surface-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.connection-geometry-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.hierarchy-state-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.navigator-outline-snapshot", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-groups", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.node-group-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.create", "capability", capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.quick-add-connected", "capability", supportsDefinitionMetadata && capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.insert-into-connection", "capability", supportsDefinitionMetadata && capabilities.CanCreateConnections && capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.delete-reconnect", "capability", capabilities.CanCreateConnections && capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.nodes.detach-connections", "capability", capabilities.CanCreateConnections && capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.pending", "capability", capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.complete", "capability", capabilities.CanCreateConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.reconnect", "capability", capabilities.CanCreateConnections && capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.disconnect", "capability", capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.delete", "capability", capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.select", "capability", true),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.multiselect", "capability", true),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.slice", "capability", capabilities.CanDeleteConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.connections.break", "capability", capabilities.CanBreakConnections),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.update", "capability", capabilities.CanUpdateViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.fit", "capability", capabilities.CanFitToViewport),
                new GraphEditorFeatureDescriptorSnapshot("capability.viewport.center", "capability", capabilities.CanCenterViewport),
                new GraphEditorFeatureDescriptorSnapshot("query.plugin-load-snapshots", "query", _descriptorSupport?.HasPluginLoader ?? false),
                new GraphEditorFeatureDescriptorSnapshot("query.fragment-storage-snapshot", "query", _descriptorSupport?.HasFragmentWorkspaceService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("query.fragment-template-snapshots", "query", _descriptorSupport?.HasFragmentLibraryService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("query.registered-node-definitions", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.node-template-snapshots", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.template-palette-snapshot", "query", supportsDefinitionMetadata || (_descriptorSupport?.HasFragmentLibraryService ?? false)),
                new GraphEditorFeatureDescriptorSnapshot("query.edge-template-snapshots", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.compatible-node-definitions", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.command-registry", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.tool-descriptors", "query", true),
                new GraphEditorFeatureDescriptorSnapshot("query.shared-selection-definition", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.selected-node-parameter-snapshots", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("query.node-parameter-snapshots", "query", supportsDefinitionMetadata),
                new GraphEditorFeatureDescriptorSnapshot("surface.automation.runner", "surface", true),
                new GraphEditorFeatureDescriptorSnapshot("service.fragment-workspace", "service", _descriptorSupport?.HasFragmentWorkspaceService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.fragment-library", "service", _descriptorSupport?.HasFragmentLibraryService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.scene-svg-export", "service", _descriptorSupport?.HasSceneSvgExportService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.scene-image-export", "service", _descriptorSupport?.HasSceneImageExportService ?? false),
                new GraphEditorFeatureDescriptorSnapshot("service.clipboard-payload-serializer", "service", _descriptorSupport?.HasClipboardPayloadSerializer ?? false),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.started", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.progress", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("event.automation.completed", "event", true),
                new GraphEditorFeatureDescriptorSnapshot("integration.diagnostics-sink", "integration", _diagnosticsSink is not null),
                new GraphEditorFeatureDescriptorSnapshot("integration.plugin-loader", "integration", _descriptorSupport?.HasPluginLoader ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.plugin-trust-policy", "integration", (_descriptorSupport?.HasPluginTrustPolicy ?? false) || _hasPluginTrustPolicy),
                new GraphEditorFeatureDescriptorSnapshot("integration.command-contributor", "integration", (_descriptorSupport?.HasCommandContributor ?? false) || _pluginCommandContributors.Count > 0),
                new GraphEditorFeatureDescriptorSnapshot("integration.context-menu-augmentor", "integration", _descriptorSupport?.HasContextMenuAugmentor ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.tool-provider", "integration", _descriptorSupport?.HasToolProvider ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.runtime-overlay-provider", "integration", _descriptorSupport?.HasRuntimeOverlayProvider ?? false),
                new GraphEditorFeatureDescriptorSnapshot("integration.layout-provider", "integration", _descriptorSupport?.HasLayoutProvider ?? false),
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

    public IReadOnlyList<GraphEditorFragmentTemplateSnapshot> GetFragmentTemplateSnapshots()
        => _host.GetFragmentTemplateSnapshots();

    public GraphEditorRuntimeOverlaySnapshot GetRuntimeOverlaySnapshot()
    {
        var provider = _descriptorSupport?.RuntimeOverlayProvider;
        return provider is null
            ? GraphEditorRuntimeOverlaySnapshot.Empty
            : new GraphEditorRuntimeOverlaySnapshot(
                true,
                provider.GetNodeOverlays().ToList(),
                provider.GetConnectionOverlays().ToList(),
                provider.GetRecentLogs().ToList());
    }

    public GraphEditorValidationSnapshot GetValidationSnapshot()
        => GraphEditorValidationSnapshotProjector.Project(CreateDocumentSnapshot(), _descriptorSupport);

    public IReadOnlyList<GraphEditorValidationRepairActionSnapshot> GetValidationIssueRepairActions(GraphEditorValidationIssueSnapshot issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        var document = CreateDocumentSnapshot();
        var scope = document.GraphScopes.FirstOrDefault(candidate => string.Equals(candidate.Id, issue.ScopeId, StringComparison.Ordinal));
        if (scope is null)
        {
            return [];
        }

        var repairs = new List<GraphEditorValidationRepairActionSnapshot>();
        if (string.Equals(issue.Code, "node.parameter-invalid", StringComparison.Ordinal))
        {
            AddParameterDefaultRepair(issue, scope, repairs);
        }

        if (!string.IsNullOrWhiteSpace(issue.ConnectionId))
        {
            AddConnectionRepairs(issue, scope, repairs);
        }

        return repairs;
    }

    public GraphLayoutPlan CreateLayoutPlan(GraphLayoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = _descriptorSupport?.LayoutProvider;
        return provider is null
            ? GraphLayoutPlan.Empty(request, "No layout provider is configured.")
            : provider.CreateLayoutPlan(request);
    }

    public IReadOnlyList<INodeDefinition> GetRegisteredNodeDefinitions()
        => _descriptorSupport?.Definitions
            .OrderBy(definition => definition.Category, StringComparer.Ordinal)
            .ThenBy(definition => definition.DisplayName, StringComparer.Ordinal)
            .ToList()
            ?? [];

    public IReadOnlyList<GraphEditorNodeTemplateSnapshot> GetNodeTemplateSnapshots()
        => _descriptorSupport?.Definitions
            .Select(GraphEditorNodeTemplateSnapshot.Create)
            .OrderBy(item => item.Category, StringComparer.Ordinal)
            .ThenBy(item => item.Title, StringComparer.Ordinal)
            .ToList()
            ?? [];

    public GraphEditorTemplatePaletteSnapshot GetTemplatePaletteSnapshot(GraphEditorTemplatePaletteQuery? query = null)
    {
        query ??= new GraphEditorTemplatePaletteQuery();
        var searchText = NormalizeSearchText(query.SearchText);
        var category = string.IsNullOrWhiteSpace(query.Category) ? null : query.Category.Trim();

        var items = GetNodeTemplateSnapshots()
            .Select(CreateNodePaletteItem)
            .Concat(GetFragmentTemplateSnapshots().Select(CreateFragmentPaletteItem))
            .Where(item => query.Kind is null || item.Kind == query.Kind)
            .Where(item => query.SourceKind is null || item.SourceKind == query.SourceKind)
            .Where(item => category is null || string.Equals(item.Category, category, StringComparison.Ordinal))
            .Where(item => string.IsNullOrEmpty(searchText) || MatchesTemplatePaletteSearch(item, searchText))
            .OrderBy(item => item.SourceKind)
            .ThenBy(item => item.Kind)
            .ThenBy(item => item.Category, StringComparer.Ordinal)
            .ThenBy(item => item.Title, StringComparer.Ordinal)
            .ThenBy(item => item.TemplateKey, StringComparer.Ordinal)
            .ToList();

        return new GraphEditorTemplatePaletteSnapshot(searchText, query.Kind, query.SourceKind, category, items);
    }

    public INodeDefinition? GetSharedSelectionDefinition()
    {
        ResolveSelectedNodesAndSharedDefinition(out _, out var definition);
        return definition;
    }

    public IReadOnlyList<GraphEditorNodeParameterSnapshot> GetSelectedNodeParameterSnapshots()
    {
        var descriptorSupport = _descriptorSupport;
        if (descriptorSupport is null)
        {
            return [];
        }

        if (!ResolveSelectedNodesAndSharedDefinition(out var selectedNodes, out var definition)
            || definition is null)
        {
            return [];
        }

        var canEditParameters = descriptorSupport.CanEditNodeParameters;
        return GraphEditorNodeParameterSnapshotProjector.Project(
            definition.Parameters,
            parameter => selectedNodes
                .Select(node => ResolveNodeParameterValue(node, parameter))
                .ToList(),
            parameter => !canEditParameters);
    }

    public IReadOnlyList<GraphEditorNodeParameterSnapshot> GetNodeParameterSnapshots(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var descriptorSupport = _descriptorSupport;
        if (descriptorSupport is null)
        {
            return [];
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        if (node is null || node.DefinitionId is null)
        {
            return [];
        }

        if (!descriptorSupport.NodeCatalog.TryGetDefinition(node.DefinitionId, out var definition) || definition is null)
        {
            return [];
        }

        var canEditParameters = descriptorSupport.CanEditNodeParameters;
        return GraphEditorNodeParameterSnapshotProjector.Project(
            definition.Parameters,
            parameter => [ResolveNodeParameterValue(node, parameter)],
            parameter => !canEditParameters);
    }

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
    {
        var descriptors = _host.GetCommandDescriptors().ToList();
        descriptors.AddRange(CollectPluginCommandDescriptors(descriptors.Select(descriptor => descriptor.Id).ToArray()));
        return descriptors;
    }

    public IReadOnlyList<GraphEditorCommandRegistryEntrySnapshot> GetCommandRegistry()
        => GraphEditorCommandRegistry.Create(GetCommandDescriptors());

    public IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots()
        => _host.GetNodeSurfaceSnapshots();

    public GraphEditorHierarchyStateSnapshot GetHierarchyStateSnapshot()
        => _host.GetHierarchyStateSnapshot();

    public GraphEditorNavigatorOutlineSnapshot GetNavigatorOutlineSnapshot()
        => GraphEditorNavigatorOutlineProjector.Project(
            _host.CreateActiveScopeDocumentSnapshot(),
            GetHierarchyStateSnapshot(),
            GetSelectionSnapshot());

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

    public IReadOnlyList<GraphEditorEdgeTemplateSnapshot> GetEdgeTemplateSnapshots(string sourceNodeId, string sourcePortId)
        => _host.GetEdgeTemplateSnapshots(sourceNodeId, sourcePortId);

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _host.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

    public GraphEditorCompatibleNodeSearchSnapshot GetCompatibleNodeDefinitionsForPendingConnection()
    {
        var pending = GetPendingConnectionSnapshot();
        if (!pending.HasPendingConnection
            || string.IsNullOrWhiteSpace(pending.SourceNodeId)
            || string.IsNullOrWhiteSpace(pending.SourcePortId))
        {
            return new(false, null, null, [], "No pending connection.");
        }

        var descriptorSupport = _descriptorSupport;
        if (descriptorSupport is null)
        {
            return new(true, pending.SourceNodeId, pending.SourcePortId, [], "No node catalog is available.");
        }

        var document = _host.CreateActiveScopeDocumentSnapshot();
        var sourceNode = document.Nodes.FirstOrDefault(node => string.Equals(node.Id, pending.SourceNodeId, StringComparison.Ordinal));
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, pending.SourcePortId, StringComparison.Ordinal));
        if (sourcePort?.TypeId is null)
        {
            return new(true, pending.SourceNodeId, pending.SourcePortId, [], "Pending connection source port was not found.");
        }

        var results = descriptorSupport.Definitions
            .SelectMany(definition => EnumerateCompatibleNodeDefinitionTargets(descriptorSupport, definition, sourcePort.TypeId))
            .OrderBy(result => result.Category, StringComparer.Ordinal)
            .ThenBy(result => result.DisplayName, StringComparer.Ordinal)
            .ThenBy(result => result.TargetLabel, StringComparer.Ordinal)
            .ToList();
        var emptyReason = results.Count == 0
            ? "No compatible node definitions found for the pending connection."
            : null;
        return new(true, pending.SourceNodeId, pending.SourcePortId, results, emptyReason);
    }

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
            GetPluginLoadSnapshots().ToList(),
            GetSelectedNodeParameterSnapshots(),
            GetValidationSnapshot());
    }

    private GraphEditorPendingConnectionSnapshot CreatePendingConnectionSnapshot()
        => _host.GetPendingConnectionSnapshot();

    private void AddParameterDefaultRepair(
        GraphEditorValidationIssueSnapshot issue,
        GraphScope scope,
        List<GraphEditorValidationRepairActionSnapshot> repairs)
    {
        if (string.IsNullOrWhiteSpace(issue.NodeId)
            || string.IsNullOrWhiteSpace(issue.ParameterKey)
            || _descriptorSupport is null)
        {
            return;
        }

        var node = scope.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, issue.NodeId, StringComparison.Ordinal));
        if (node?.DefinitionId is null
            || !_descriptorSupport.NodeCatalog.TryGetDefinition(node.DefinitionId, out var definition)
            || definition is null)
        {
            return;
        }

        var parameter = definition.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Key, issue.ParameterKey, StringComparison.Ordinal));
        if (parameter is null || parameter.Constraints.IsReadOnly)
        {
            return;
        }

        var normalized = NodeParameterValueAdapter.NormalizeValue(parameter, parameter.DefaultValue);
        if (!normalized.IsValid)
        {
            return;
        }

        repairs.Add(new GraphEditorValidationRepairActionSnapshot(
            "validation.parameter.reset-default",
            $"Reset {parameter.DisplayName} to default",
            $"Reset {parameter.DisplayName} to its default value.",
            issue.Code,
            issue.ScopeId,
            node.Id,
            ParameterKey: parameter.Key));
    }

    private static void AddConnectionRepairs(
        GraphEditorValidationIssueSnapshot issue,
        GraphScope scope,
        List<GraphEditorValidationRepairActionSnapshot> repairs)
    {
        var connection = scope.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, issue.ConnectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return;
        }

        repairs.Add(new GraphEditorValidationRepairActionSnapshot(
            "validation.connection.remove",
            "Remove invalid connection",
            $"Remove invalid connection {connection.Id}.",
            issue.Code,
            issue.ScopeId,
            ConnectionId: connection.Id,
            EndpointId: issue.EndpointId));

        var sourceNode = scope.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, connection.SourceNodeId, StringComparison.Ordinal));
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(candidate => string.Equals(candidate.Id, connection.SourcePortId, StringComparison.Ordinal));
        if (sourceNode is not null && sourcePort is not null)
        {
            repairs.Add(new GraphEditorValidationRepairActionSnapshot(
                "validation.connection.reconnect",
                "Reconnect from source",
                $"Reconnect {connection.Id} from {sourceNode.Title}.{sourcePort.Label}.",
                issue.Code,
                issue.ScopeId,
                sourceNode.Id,
                connection.Id,
                sourcePort.Id));
        }

        var routeVertexCount = connection.Presentation?.Route?.Vertices.Count ?? 0;
        if (routeVertexCount == 1)
        {
            repairs.Add(new GraphEditorValidationRepairActionSnapshot(
                "validation.connection.route.reset",
                "Reset route vertex",
                $"Remove 1 persisted route vertex from {connection.Id}.",
                issue.Code,
                issue.ScopeId,
                ConnectionId: connection.Id,
                EndpointId: issue.EndpointId,
                RouteVertexCount: routeVertexCount));
        }
    }

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

    private static GraphEditorTemplatePaletteItemSnapshot CreateNodePaletteItem(GraphEditorNodeTemplateSnapshot template)
        => new(
            $"node:{template.DefinitionId.Value}",
            GraphEditorTemplatePaletteItemKind.NodeTemplate,
            GraphEditorTemplatePaletteSourceKind.NodeCatalog,
            template.DefinitionId.Value,
            template.Key,
            template.Title,
            template.Category,
            template.Subtitle,
            template.Description,
            template.PortSummary,
            template.ActionDescription,
            1,
            0,
            0);

    private static GraphEditorTemplatePaletteItemSnapshot CreateFragmentPaletteItem(GraphEditorFragmentTemplateSnapshot template)
    {
        var category = template.GroupCount > 0 ? "Fragment Groups" : "Fragments";
        var description = template.GroupCount > 0
            ? "Reusable graph fragment with groups."
            : "Reusable graph fragment.";

        return new(
            $"fragment:{template.Path}",
            GraphEditorTemplatePaletteItemKind.FragmentTemplate,
            GraphEditorTemplatePaletteSourceKind.FragmentLibrary,
            template.Path,
            template.Path,
            template.Name,
            category,
            "Fragment template",
            description,
            template.Summary,
            template.ActionDescription,
            template.NodeCount,
            template.ConnectionCount,
            template.GroupCount);
    }

    private static string NormalizeSearchText(string? searchText)
        => string.IsNullOrWhiteSpace(searchText) ? string.Empty : searchText.Trim();

    private static (GraphPoint? Position, GraphSize? Size) CreateBounds(IReadOnlyList<GraphNode> nodes)
    {
        if (nodes.Count == 0)
        {
            return (null, null);
        }

        var left = nodes.Min(node => node.Position.X);
        var top = nodes.Min(node => node.Position.Y);
        var right = nodes.Max(node => node.Position.X + node.Size.Width);
        var bottom = nodes.Max(node => node.Position.Y + node.Size.Height);
        return (new GraphPoint(left, top), new GraphSize(right - left, bottom - top));
    }

    private static (IReadOnlyList<string> NodeIds, IReadOnlyList<string> ConnectionIds) ProjectSelectionRectangle(
        GraphDocument document,
        GraphEditorSelectionTransformQuery query)
    {
        if (query.SelectionRectanglePosition is not { } position || query.SelectionRectangleSize is not { } size)
        {
            return ([], []);
        }

        var left = Math.Min(position.X, position.X + size.Width);
        var top = Math.Min(position.Y, position.Y + size.Height);
        var right = Math.Max(position.X, position.X + size.Width);
        var bottom = Math.Max(position.Y, position.Y + size.Height);
        var nodeIds = document.Nodes
            .Where(node => Intersects(left, top, right, bottom, node.Position, node.Size))
            .Select(node => node.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var nodeIdSet = nodeIds.ToHashSet(StringComparer.Ordinal);
        var connectionIds = document.Connections
            .Where(connection => nodeIdSet.Contains(connection.SourceNodeId) && nodeIdSet.Contains(connection.TargetNodeId))
            .Select(connection => connection.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        return (nodeIds, connectionIds);
    }

    private static bool Intersects(double left, double top, double right, double bottom, GraphPoint position, GraphSize size)
    {
        var nodeRight = position.X + size.Width;
        var nodeBottom = position.Y + size.Height;
        return position.X <= right
               && nodeRight >= left
               && position.Y <= bottom
               && nodeBottom >= top;
    }

    private static GraphPoint SnapToGrid(GraphPoint position, double gridSize)
        => new(
            Math.Round(position.X / gridSize, MidpointRounding.AwayFromZero) * gridSize,
            Math.Round(position.Y / gridSize, MidpointRounding.AwayFromZero) * gridSize);

    private static GraphPoint NormalizeTransformDelta(GraphPoint delta, bool constrainToPrimaryAxis)
    {
        if (!constrainToPrimaryAxis)
        {
            return delta;
        }

        return Math.Abs(delta.X) >= Math.Abs(delta.Y)
            ? new GraphPoint(delta.X, 0d)
            : new GraphPoint(0d, delta.Y);
    }

    private static bool MatchesTemplatePaletteSearch(GraphEditorTemplatePaletteItemSnapshot item, string searchText)
        => Contains(item.Id, searchText)
           || Contains(item.TemplateKey, searchText)
           || Contains(item.Title, searchText)
           || Contains(item.Category, searchText)
           || Contains(item.Subtitle, searchText)
           || Contains(item.Description, searchText)
           || Contains(item.Summary, searchText)
           || Contains(item.SourceKind.ToString(), searchText)
           || Contains(item.Kind.ToString(), searchText);

    private static bool Contains(string value, string searchText)
        => value.Contains(searchText, StringComparison.OrdinalIgnoreCase);

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

    private static IEnumerable<GraphEditorCompatibleNodeDefinitionSnapshot> EnumerateCompatibleNodeDefinitionTargets(
        GraphEditorSessionDescriptorSupport descriptorSupport,
        INodeDefinition definition,
        PortTypeId sourceTypeId)
    {
        foreach (var input in definition.InputPorts)
        {
            var compatibility = descriptorSupport.CompatibilityService.Evaluate(sourceTypeId, input.TypeId);
            if (!compatibility.IsCompatible)
            {
                continue;
            }

            yield return new GraphEditorCompatibleNodeDefinitionSnapshot(
                definition.Id,
                definition.DisplayName,
                definition.Category,
                input.Key,
                input.DisplayName,
                GraphConnectionTargetKind.Port,
                input.TypeId,
                compatibility);
        }

        foreach (var parameter in definition.Parameters)
        {
            var compatibility = descriptorSupport.CompatibilityService.Evaluate(sourceTypeId, parameter.ValueType);
            if (!compatibility.IsCompatible)
            {
                continue;
            }

            yield return new GraphEditorCompatibleNodeDefinitionSnapshot(
                definition.Id,
                definition.DisplayName,
                definition.Category,
                parameter.Key,
                parameter.DisplayName,
                GraphConnectionTargetKind.Parameter,
                parameter.ValueType,
                compatibility);
        }
    }
}
