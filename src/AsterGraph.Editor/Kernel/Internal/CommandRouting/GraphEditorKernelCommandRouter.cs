using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Kernel.Internal.Layout;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using System.Threading;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelCommandRouterHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    GraphDocument Document { get; }

    int SelectedNodeCount { get; }

    bool CanUndo { get; }

    bool CanRedo { get; }

    bool CanCopySelection { get; }

    bool CanPaste { get; }

    bool CanEditSelectedNodeParameters { get; }

    bool CanAlignSelection { get; }

    bool CanDistributeSelection { get; }

    GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    double ViewportWidth { get; }

    double ViewportHeight { get; }

    bool WorkspaceExists { get; }

    bool FragmentWorkspaceExists { get; }

    bool CanExportSceneAsSvg { get; }

    bool CanExportSceneAsImage { get; }

    bool CanNavigateToParentGraphScope { get; }

    void SetStatus(string statusMessage);

    void Undo();

    void Redo();

    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition);

    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void SetConnectionSelection(IReadOnlyList<string> connectionIds, string? primaryConnectionId, bool updateStatus);

    void DeleteNodeById(string nodeId);

    void DuplicateNode(string nodeId);

    void DeleteSelection();

    Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken);

    Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken);

    bool TryExportSelectionFragment(string? path);

    bool TryImportFragment(string? path);

    bool TryClearWorkspaceFragment(string? path);

    string TryExportSelectionAsTemplate(string? name);

    bool TryExportSceneAsSvg(string? path);

    bool TryExportSceneAsImage(GraphEditorSceneImageExportFormat format, string? path, GraphEditorSceneImageExportOptions? options);

    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);

    bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus);

    bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState);

    string TryCreateNodeGroupFromSelection(string title);

    bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed);

    bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus);

    bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus);

    bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus);

    bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus);

    string TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus);

    string TryWrapSelectionToComposite(string? title, bool updateStatus);

    string TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus);

    bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus);

    bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus);

    bool TryReturnToParentGraphScope(bool updateStatus);

    bool TrySetSelectedNodeParameterValue(string parameterKey, object? value);

    bool TryApplyLayoutPlan(GraphLayoutPlan plan, bool updateStatus);

    bool TryApplySelectionLayout(GraphSelectionLayoutOperation operation, bool updateStatus);

    bool TrySnapSelectedNodesToGrid(double gridSize, bool updateStatus);

    bool TrySnapAllNodesToGrid(double gridSize, bool updateStatus);

    void StartConnection(string sourceNodeId, string sourcePortId);

    void CompleteConnection(GraphConnectionTargetRef target);

    void CancelPendingConnection();

    void DeleteConnection(string connectionId);

    bool TryReconnectConnection(string connectionId, bool updateStatus);

    bool TrySetConnectionLabel(string connectionId, string? label, bool updateStatus);

    bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus);

    bool TryInsertConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus);

    bool TryMoveConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus);

    bool TryRemoveConnectionRouteVertex(string connectionId, int vertexIndex, bool updateStatus);

    void DisconnectConnection(string connectionId);

    void BreakConnectionsForPort(string nodeId, string portId);

    void DisconnectIncoming(string nodeId);

    void DisconnectOutgoing(string nodeId);

    void DisconnectAll(string nodeId);

    void FitToViewport(bool updateStatus);

    void FitSelectionToViewport(bool updateStatus);

    void FocusSelection(bool updateStatus);

    void FocusCurrentScope(bool updateStatus);

    void PanBy(double deltaX, double deltaY);

    void UpdateViewportSize(double width, double height);

    void ResetView(bool updateStatus);

    void CenterViewOnNode(string nodeId);

    void CenterViewAt(GraphPoint worldPoint, bool updateStatus);

    void SaveWorkspace();

    bool LoadWorkspace();
}

internal sealed class GraphEditorKernelCommandRouter
{
    private readonly IGraphEditorKernelCommandRouterHost _host;

    public GraphEditorKernelCommandRouter(IGraphEditorKernelCommandRouterHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
    {
        var hasViewport = _host.ViewportWidth > 0 && _host.ViewportHeight > 0;
        var hasNodes = _host.Document.Nodes.Count > 0;
        var hasSelection = _host.SelectedNodeCount > 0;
        return
        [
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.add",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.set",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.connections.set",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.clear",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection,
                hasSelection ? null : "Select one or more nodes before clearing selection.",
                hasSelection ? null : "Select nodes first, then retry.",
                hasSelection ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.delete",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection && _host.BehaviorOptions.Commands.Nodes.AllowDelete,
                GetSelectionDisabledReason(
                    _host.BehaviorOptions.Commands.Nodes.AllowDelete,
                    hasSelection,
                    "Node deletion is disabled by host permissions.",
                    "Select one or more nodes before deleting."),
                hasSelection || !_host.BehaviorOptions.Commands.Nodes.AllowDelete ? null : "Select nodes first, then retry.",
                hasSelection || !_host.BehaviorOptions.Commands.Nodes.AllowDelete ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "clipboard.copy",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanCopySelection,
                _host.BehaviorOptions.Commands.Clipboard.AllowCopy
                    ? _host.CanCopySelection
                        ? null
                        : "Select at least one node before copying."
                    : "Copy is disabled by host permissions.",
                _host.BehaviorOptions.Commands.Clipboard.AllowCopy && !_host.CanCopySelection
                    ? "Select nodes first, then retry."
                    : null,
                _host.BehaviorOptions.Commands.Clipboard.AllowCopy && !_host.CanCopySelection
                    ? "nodes.add"
                    : null),
            GraphEditorCommandDescriptorCatalog.Create(
                "clipboard.paste",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanPaste),
            GraphEditorCommandDescriptorCatalog.Create(
                "export.scene-svg",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanExportSceneAsSvg),
            GraphEditorCommandDescriptorCatalog.Create(
                "export.scene-image",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanExportSceneAsImage),
            GraphEditorCommandDescriptorCatalog.Create(
                "fragments.export-selection",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Fragments.AllowExport && hasSelection,
                GetSelectionDisabledReason(
                    _host.BehaviorOptions.Commands.Fragments.AllowExport,
                    hasSelection,
                    "Fragment export is disabled by host permissions.",
                    "Select one or more nodes before exporting a fragment."),
                hasSelection ? null : "Select nodes first, then retry.",
                hasSelection ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "fragments.import",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Fragments.AllowImport && _host.BehaviorOptions.Commands.Nodes.AllowCreate && _host.FragmentWorkspaceExists),
            GraphEditorCommandDescriptorCatalog.Create(
                "fragments.clear-workspace",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Fragments.AllowClearWorkspaceFragment && _host.FragmentWorkspaceExists),
            GraphEditorCommandDescriptorCatalog.Create(
                "fragments.export-template",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Fragments.EnableFragmentLibrary
                && _host.BehaviorOptions.Commands.Fragments.AllowTemplateManagement
                && _host.BehaviorOptions.Commands.Fragments.AllowExport
                && hasSelection,
                !_host.BehaviorOptions.Fragments.EnableFragmentLibrary
                    ? "Fragment template library is disabled."
                    : !_host.BehaviorOptions.Commands.Fragments.AllowTemplateManagement || !_host.BehaviorOptions.Commands.Fragments.AllowExport
                        ? "Fragment template export is disabled by host permissions."
                        : hasSelection
                            ? null
                            : "Select one or more nodes before exporting a template."),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.move",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.apply-plan",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove && hasNodes,
                _host.BehaviorOptions.Commands.Nodes.AllowMove
                    ? hasNodes ? null : "Add one or more nodes before applying a layout plan."
                    : "Layout application is disabled by host permissions."),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.snap-selection",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove && hasSelection,
                GetSelectionDisabledReason(
                    _host.BehaviorOptions.Commands.Nodes.AllowMove,
                    hasSelection,
                    "Node snapping is disabled by host permissions.",
                    "Select one or more nodes before snapping.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.snap-all",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove && hasNodes,
                _host.BehaviorOptions.Commands.Nodes.AllowMove
                    ? hasNodes ? null : "Add one or more nodes before snapping."
                    : "Node snapping is disabled by host permissions."),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.resize",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.surface.expand",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Count > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.inspect",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Count > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.delete-by-id",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowDelete),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.duplicate",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowDuplicate),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.parameters.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanEditSelectedNodeParameters,
                _host.CanEditSelectedNodeParameters
                    ? null
                    : "Parameter editing requires node-edit permissions and a shared node definition selection."),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.create",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection && _host.BehaviorOptions.Commands.Nodes.AllowMove,
                GetSelectionDisabledReason(
                    _host.BehaviorOptions.Commands.Nodes.AllowMove,
                    hasSelection,
                    "Group creation is disabled by host permissions.",
                    "Select one or more nodes before creating a group."),
                hasSelection ? null : "Select nodes first, then retry.",
                hasSelection ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.collapse",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.move",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.resize",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.membership.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.promote",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-left",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning."),
                _host.CanAlignSelection ? null : "Select at least two nodes first.",
                _host.CanAlignSelection ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-center",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-right",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-top",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-middle",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-bottom",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowAlign, _host.SelectedNodeCount, 2, "Layout alignment is disabled by host permissions.", "Select at least two nodes before aligning.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.distribute-horizontal",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanDistributeSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowDistribute, _host.SelectedNodeCount, 3, "Layout distribution is disabled by host permissions.", "Select at least three nodes before distributing.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.distribute-vertical",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanDistributeSelection,
                GetSelectionCountDisabledReason(_host.BehaviorOptions.Commands.Layout.AllowDistribute, _host.SelectedNodeCount, 3, "Layout distribution is disabled by host permissions.", "Select at least three nodes before distributing.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.wrap-selection",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection && _host.BehaviorOptions.Commands.Nodes.AllowMove,
                GetSelectionDisabledReason(
                    _host.BehaviorOptions.Commands.Nodes.AllowMove,
                    hasSelection,
                    "Composite wrapping is disabled by host permissions.",
                    "Select one or more nodes before wrapping to a composite."),
                hasSelection ? null : "Select nodes first, then retry.",
                hasSelection ? null : "nodes.add"),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.expose-port",
                GraphEditorCommandSourceKind.Kernel,
                (_host.Document.Nodes.Any(node => node.Composite is not null) || _host.CanNavigateToParentGraphScope)
                && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.unexpose-port",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Any(node =>
                    node.Composite is not null
                    && ((node.Composite.Inputs?.Count ?? 0) > 0 || (node.Composite.Outputs?.Count ?? 0) > 0))
                && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "scopes.enter",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Any(node => node.Composite is not null)),
            GraphEditorCommandDescriptorCatalog.Create(
                "scopes.exit",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanNavigateToParentGraphScope),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.start",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.complete",
                GraphEditorCommandSourceKind.Kernel,
                _host.PendingConnection.HasPendingConnection && _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.connect",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.cancel",
                GraphEditorCommandSourceKind.Kernel,
                _host.PendingConnection.HasPendingConnection),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.delete",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDelete),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.label.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.note.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.route-vertex.insert",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.route-vertex.move",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.route-vertex.remove",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.reconnect",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && _host.BehaviorOptions.Commands.Connections.AllowCreate
                && _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.break-port",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-incoming",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-outgoing",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-all",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "history.undo",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanUndo,
                _host.CanUndo ? null : "Nothing to undo yet.",
                _host.CanUndo ? null : "Perform an action first.",
                null),
            GraphEditorCommandDescriptorCatalog.Create(
                "history.redo",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanRedo,
                _host.CanRedo ? null : "Nothing to redo.",
                _host.CanRedo ? null : "Undo an action first.",
                null),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.fit",
                GraphEditorCommandSourceKind.Kernel,
                hasNodes && hasViewport,
                GetViewportDisabledReason(hasNodes, hasViewport, "Add a node before fitting the view."),
                !hasNodes ? "Add a node first." : !hasViewport ? "Wait for viewport to initialize." : null,
                !hasNodes ? "nodes.add" : null),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.fit-selection",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection && hasViewport,
                GetViewportSelectionDisabledReason(hasSelection, hasViewport, "Select one or more nodes before fitting the selection.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.focus-selection",
                GraphEditorCommandSourceKind.Kernel,
                hasSelection && hasViewport,
                GetViewportSelectionDisabledReason(hasSelection, hasViewport, "Select one or more nodes before focusing the selection.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.focus-current-scope",
                GraphEditorCommandSourceKind.Kernel,
                hasNodes && hasViewport,
                GetViewportDisabledReason(hasNodes, hasViewport, "Add a node before focusing the current scope.")),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.pan",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.resize",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.reset",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.center-node",
                GraphEditorCommandSourceKind.Kernel,
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.center",
                GraphEditorCommandSourceKind.Kernel,
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "workspace.save",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Workspace.AllowSave,
                _host.BehaviorOptions.Commands.Workspace.AllowSave ? null : "Snapshot saving is disabled by host permissions.",
                null,
                null),
            GraphEditorCommandDescriptorCatalog.Create(
                "workspace.load",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Workspace.AllowLoad && _host.WorkspaceExists,
                !_host.BehaviorOptions.Commands.Workspace.AllowLoad
                    ? "Snapshot loading is disabled by host permissions."
                    : _host.WorkspaceExists
                        ? null
                        : "No saved snapshot yet. Save once to create one.",
                !_host.WorkspaceExists && _host.BehaviorOptions.Commands.Workspace.AllowLoad
                    ? "Save a workspace first, then load."
                    : null,
                !_host.WorkspaceExists && _host.BehaviorOptions.Commands.Workspace.AllowLoad
                    ? "workspace.save"
                    : null),
        ];
    }

    private static string? GetSelectionDisabledReason(
        bool isPermitted,
        bool hasSelection,
        string permissionReason,
        string selectionReason)
    {
        if (!isPermitted)
        {
            return permissionReason;
        }

        return hasSelection ? null : selectionReason;
    }

    private static string? GetSelectionCountDisabledReason(
        bool isPermitted,
        int selectedCount,
        int requiredCount,
        string permissionReason,
        string selectionReason)
    {
        if (!isPermitted)
        {
            return permissionReason;
        }

        return selectedCount >= requiredCount ? null : selectionReason;
    }

    private static string? GetViewportDisabledReason(
        bool hasNodes,
        bool hasViewport,
        string emptyReason)
    {
        if (!hasViewport)
        {
            return "Viewport size is not available yet.";
        }

        return hasNodes ? null : emptyReason;
    }

    private static string? GetViewportSelectionDisabledReason(
        bool hasSelection,
        bool hasViewport,
        string selectionReason)
    {
        if (!hasViewport)
        {
            return "Viewport size is not available yet.";
        }

        return hasSelection ? null : selectionReason;
    }

    private bool TryBlockDisabledCommand(GraphEditorCommandInvocationSnapshot command)
    {
        if (!CanExposeDisabledReason(command.CommandId))
        {
            return false;
        }

        var descriptor = GetCommandDescriptors()
            .FirstOrDefault(item => string.Equals(item.Id, command.CommandId, StringComparison.Ordinal));
        if (descriptor is null || descriptor.IsEnabled || string.IsNullOrWhiteSpace(descriptor.DisabledReason))
        {
            return false;
        }

        _host.SetStatus(descriptor.DisabledReason);
        return true;
    }

    private static bool CanExposeDisabledReason(string commandId)
        => commandId switch
        {
            "selection.clear"
                or "selection.delete"
                or "clipboard.copy"
                or "fragments.export-selection"
                or "fragments.export-template"
                or "nodes.parameters.set"
                or "groups.create"
                or "layout.align-left"
                or "layout.align-center"
                or "layout.align-right"
                or "layout.align-top"
                or "layout.align-middle"
                or "layout.align-bottom"
                or "layout.distribute-horizontal"
                or "layout.distribute-vertical"
                or "composites.wrap-selection"
                or "history.undo"
                or "history.redo"
                or "viewport.fit"
                or "viewport.fit-selection"
                or "viewport.focus-selection"
                or "viewport.focus-current-scope"
                or "workspace.save"
                or "workspace.load" => true,
            _ => false,
        };

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (TryBlockDisabledCommand(command))
        {
            return false;
        }

        if (TryResolveSelectionLayoutOperation(command.CommandId, out var layoutOperation))
        {
            return _host.TryApplySelectionLayout(layoutOperation, ResolveOptionalUpdateStatus(command, "updateStatus"));
        }

        switch (command.CommandId)
        {
            case "nodes.add":
                if (!TryGetRequiredArgument(command, "definitionId", out var definitionValue))
                {
                    return false;
                }

                var definitionId = new NodeDefinitionId(definitionValue);
                GraphPoint? worldPosition = null;
                if (command.TryGetArgument("worldX", out var worldX)
                    && command.TryGetArgument("worldY", out var worldY)
                    && double.TryParse(worldX, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedX)
                    && double.TryParse(worldY, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedY))
                {
                    worldPosition = new GraphPoint(parsedX, parsedY);
                }

                _host.AddNode(definitionId, worldPosition);
                return true;

            case "selection.set":
                var nodeIds = command.GetArguments("nodeId")
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
                if (nodeIds.Count == 0)
                {
                    return false;
                }

                command.TryGetArgument("primaryNodeId", out var primaryNodeId);
                _host.SetSelection(nodeIds, primaryNodeId, ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "selection.connections.set":
                var connectionIds = command.GetArguments("connectionId")
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
                if (connectionIds.Count == 0)
                {
                    return false;
                }

                command.TryGetArgument("primaryConnectionId", out var primaryConnectionId);
                _host.SetConnectionSelection(connectionIds, primaryConnectionId, ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "selection.delete":
                _host.DeleteSelection();
                return true;

            case "clipboard.copy":
                _ = _host.TryCopySelectionAsync(CancellationToken.None);
                return true;

            case "clipboard.paste":
                _ = _host.TryPasteSelectionAsync(CancellationToken.None);
                return true;

            case "export.scene-svg":
                return _host.TryExportSceneAsSvg(command.TryGetArgument("path", out var sceneExportPath) ? sceneExportPath : null);

            case "export.scene-image":
                if (!TryGetEnumArgument(command, "format", out GraphEditorSceneImageExportFormat imageFormat))
                {
                    return false;
                }

                GraphEditorSceneImageExportOptions? imageOptions = null;
                if (command.TryGetArgument("scale", out _))
                {
                    if (!TryGetDoubleArgument(command, "scale", out var scale))
                    {
                        return false;
                    }

                    imageOptions = (imageOptions ?? new GraphEditorSceneImageExportOptions()) with
                    {
                        Scale = scale,
                    };
                }

                if (command.TryGetArgument("quality", out _))
                {
                    if (!TryGetIntArgument(command, "quality", out var quality))
                    {
                        return false;
                    }

                    imageOptions = (imageOptions ?? new GraphEditorSceneImageExportOptions()) with
                    {
                        Quality = quality,
                    };
                }

                if (command.TryGetArgument("backgroundHex", out var backgroundHex))
                {
                    imageOptions = (imageOptions ?? new GraphEditorSceneImageExportOptions()) with
                    {
                        BackgroundHex = backgroundHex,
                    };
                }

                return _host.TryExportSceneAsImage(
                    imageFormat,
                    command.TryGetArgument("path", out var sceneImagePath) ? sceneImagePath : null,
                    imageOptions);

            case "fragments.export-selection":
                return _host.TryExportSelectionFragment(command.TryGetArgument("path", out var exportPath) ? exportPath : null);

            case "fragments.import":
                return _host.TryImportFragment(command.TryGetArgument("path", out var importPath) ? importPath : null);

            case "fragments.clear-workspace":
                return _host.TryClearWorkspaceFragment(command.TryGetArgument("path", out var clearPath) ? clearPath : null);

            case "fragments.export-template":
                var name = command.TryGetArgument("name", out var rawName) && !string.IsNullOrWhiteSpace(rawName)
                    ? rawName
                    : null;
                return !string.IsNullOrWhiteSpace(_host.TryExportSelectionAsTemplate(name));

            case "nodes.move":
                var positions = command.GetArguments("position")
                    .Select(ParseNodePosition)
                    .ToList();
                if (positions.Count == 0 || positions.Any(position => position is null))
                {
                    return false;
                }

                _host.SetNodePositions(positions.Select(position => position!).ToList(), ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "layout.apply-plan":
                var planPositions = command.GetArguments("position")
                    .Select(ParseGraphLayoutNodePosition)
                    .ToList();
                if (planPositions.Count == 0 || planPositions.Any(position => position is null))
                {
                    return false;
                }

                return _host.TryApplyLayoutPlan(
                    new GraphLayoutPlan(
                        true,
                        new GraphLayoutRequest(),
                        planPositions.Select(position => position!).ToList(),
                        ResetManualRoutes: ResolveOptionalResetManualRoutes(command)),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "layout.snap-selection":
                return _host.TrySnapSelectedNodesToGrid(
                    ResolveOptionalGridSize(command),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "layout.snap-all":
                return _host.TrySnapAllNodesToGrid(
                    ResolveOptionalGridSize(command),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "nodes.resize":
                if (!TryGetRequiredArgument(command, "nodeId", out var resizeNodeId)
                    || !TryGetDoubleArgument(command, "width", out var nodeWidth)
                    || !TryGetDoubleArgument(command, "height", out var nodeHeight))
                {
                    return false;
                }

                return _host.TrySetNodeSize(
                    resizeNodeId,
                    new GraphSize(nodeWidth, nodeHeight),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "nodes.surface.expand":
                if (!TryGetRequiredArgument(command, "nodeId", out var expansionNodeId)
                    || !TryGetEnumArgument(command, "expansionState", out GraphNodeExpansionState expansionState))
                {
                    return false;
                }

                return _host.TrySetNodeExpansionState(expansionNodeId, expansionState);

            case "nodes.inspect":
                if (!TryGetRequiredArgument(command, "nodeId", out var inspectNodeId))
                {
                    return false;
                }

                _host.SetSelection([inspectNodeId], inspectNodeId, ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "nodes.delete-by-id":
                if (!TryGetRequiredArgument(command, "nodeId", out var deleteNodeId))
                {
                    return false;
                }

                _host.DeleteNodeById(deleteNodeId);
                return true;

            case "nodes.duplicate":
                if (!TryGetRequiredArgument(command, "nodeId", out var duplicateNodeId))
                {
                    return false;
                }

                _host.DuplicateNode(duplicateNodeId);
                return true;

            case "nodes.parameters.set":
                if (!TryGetRequiredArgument(command, "parameterKey", out var parameterKey)
                    || !command.TryGetArgument("value", out var parameterValue))
                {
                    return false;
                }

                return _host.TrySetSelectedNodeParameterValue(parameterKey, parameterValue);

            case "groups.create":
                var title = command.TryGetArgument("title", out var rawTitle) && !string.IsNullOrWhiteSpace(rawTitle)
                    ? rawTitle
                    : "Group";
                return !string.IsNullOrWhiteSpace(_host.TryCreateNodeGroupFromSelection(title));

            case "groups.collapse":
                if (!TryGetRequiredArgument(command, "groupId", out var collapseGroupId)
                    || !TryGetBoolArgument(command, "isCollapsed", out var isCollapsed))
                {
                    return false;
                }

                return _host.TrySetNodeGroupCollapsed(collapseGroupId, isCollapsed);

            case "groups.move":
                if (!TryGetRequiredArgument(command, "groupId", out var moveGroupId)
                    || !TryGetDoubleArgument(command, "worldX", out var groupX)
                    || !TryGetDoubleArgument(command, "worldY", out var groupY))
                {
                    return false;
                }

                var moveMembers = !command.TryGetArgument("moveMemberNodes", out var rawMoveMembers)
                    || !bool.TryParse(rawMoveMembers, out var parsedMoveMembers)
                    || parsedMoveMembers;
                return _host.TrySetNodeGroupPosition(
                    moveGroupId,
                    new GraphPoint(groupX, groupY),
                    moveMembers,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.resize":
                if (!TryGetRequiredArgument(command, "groupId", out var resizeGroupId)
                    || !TryGetDoubleArgument(command, "width", out var resizeWidth)
                    || !TryGetDoubleArgument(command, "height", out var resizeHeight))
                {
                    return false;
                }

                return _host.TrySetNodeGroupSize(
                    resizeGroupId,
                    new GraphSize(resizeWidth, resizeHeight),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.membership.set":
                var membershipChanges = command.GetArguments("membership")
                    .Select(ParseGroupMembershipChange)
                    .ToList();
                if (membershipChanges.Count == 0 || membershipChanges.Any(change => change is null))
                {
                    return false;
                }

                return _host.TrySetNodeGroupMemberships(
                    membershipChanges.Select(change => change!).ToList(),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.promote":
                if (!TryGetRequiredArgument(command, "groupId", out var promoteGroupId))
                {
                    return false;
                }

                var promoteTitle = command.TryGetArgument("title", out var rawPromoteTitle) && !string.IsNullOrWhiteSpace(rawPromoteTitle)
                    ? rawPromoteTitle
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryPromoteNodeGroupToComposite(
                        promoteGroupId,
                        promoteTitle,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.wrap-selection":
                var wrapTitle = command.TryGetArgument("title", out var rawWrapTitle) && !string.IsNullOrWhiteSpace(rawWrapTitle)
                    ? rawWrapTitle
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryWrapSelectionToComposite(
                        wrapTitle,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.expose-port":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var compositeNodeId)
                    || !TryGetRequiredArgument(command, "childNodeId", out var childNodeId)
                    || !TryGetRequiredArgument(command, "childPortId", out var childPortId))
                {
                    return false;
                }

                var label = command.TryGetArgument("label", out var rawLabel) && !string.IsNullOrWhiteSpace(rawLabel)
                    ? rawLabel
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryExposeCompositePort(
                        compositeNodeId,
                        childNodeId,
                        childPortId,
                        label,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.unexpose-port":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var unexposeCompositeNodeId)
                    || !TryGetRequiredArgument(command, "boundaryPortId", out var boundaryPortId))
                {
                    return false;
                }

                return _host.TryUnexposeCompositePort(
                    unexposeCompositeNodeId,
                    boundaryPortId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "scopes.enter":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var scopeCompositeNodeId))
                {
                    return false;
                }

                return _host.TryEnterCompositeChildGraph(
                    scopeCompositeNodeId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "scopes.exit":
                return _host.TryReturnToParentGraphScope(ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.start":
                if (!TryGetRequiredArgument(command, "sourceNodeId", out var sourceNodeId)
                    || !TryGetRequiredArgument(command, "sourcePortId", out var sourcePortId))
                {
                    return false;
                }

                _host.StartConnection(sourceNodeId, sourcePortId);
                return true;

            case "connections.complete":
                if (!TryGetRequiredArgument(command, "targetNodeId", out var completeTargetNodeId)
                    || !TryGetRequiredArgument(command, "targetPortId", out var completeTargetPortId))
                {
                    return false;
                }

                _host.CompleteConnection(CreateConnectionTarget(command, completeTargetNodeId, completeTargetPortId));
                return true;

            case "connections.connect":
                if (!TryGetRequiredArgument(command, "sourceNodeId", out var connectSourceNodeId)
                    || !TryGetRequiredArgument(command, "sourcePortId", out var connectSourcePortId)
                    || !TryGetRequiredArgument(command, "targetNodeId", out var targetNodeId)
                    || !TryGetRequiredArgument(command, "targetPortId", out var targetPortId))
                {
                    return false;
                }

                _host.StartConnection(connectSourceNodeId, connectSourcePortId);
                _host.CompleteConnection(CreateConnectionTarget(command, targetNodeId, targetPortId));
                return true;

            case "connections.cancel":
                _host.CancelPendingConnection();
                return true;

            case "connections.delete":
                if (!TryGetRequiredArgument(command, "connectionId", out var connectionId))
                {
                    return false;
                }

                _host.DeleteConnection(connectionId);
                return true;

            case "connections.disconnect":
                if (!TryGetRequiredArgument(command, "connectionId", out var disconnectConnectionId))
                {
                    return false;
                }

                _host.DisconnectConnection(disconnectConnectionId);
                return true;

            case "connections.label.set":
                if (!TryGetRequiredArgument(command, "connectionId", out var labelConnectionId))
                {
                    return false;
                }

                command.TryGetArgument("label", out var labelText);
                return _host.TrySetConnectionLabel(
                    labelConnectionId,
                    labelText,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.note.set":
                if (!TryGetRequiredArgument(command, "connectionId", out var noteConnectionId))
                {
                    return false;
                }

                command.TryGetArgument("text", out var noteText);
                return _host.TrySetConnectionNoteText(
                    noteConnectionId,
                    noteText,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.route-vertex.insert":
                if (!TryGetRequiredArgument(command, "connectionId", out var insertConnectionId)
                    || !TryGetIntArgument(command, "vertexIndex", out var insertVertexIndex)
                    || !TryGetDoubleArgument(command, "worldX", out var insertWorldX)
                    || !TryGetDoubleArgument(command, "worldY", out var insertWorldY))
                {
                    return false;
                }

                return _host.TryInsertConnectionRouteVertex(
                    insertConnectionId,
                    insertVertexIndex,
                    new GraphPoint(insertWorldX, insertWorldY),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.route-vertex.move":
                if (!TryGetRequiredArgument(command, "connectionId", out var moveConnectionId)
                    || !TryGetIntArgument(command, "vertexIndex", out var moveVertexIndex)
                    || !TryGetDoubleArgument(command, "worldX", out var moveWorldX)
                    || !TryGetDoubleArgument(command, "worldY", out var moveWorldY))
                {
                    return false;
                }

                return _host.TryMoveConnectionRouteVertex(
                    moveConnectionId,
                    moveVertexIndex,
                    new GraphPoint(moveWorldX, moveWorldY),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.route-vertex.remove":
                if (!TryGetRequiredArgument(command, "connectionId", out var removeConnectionId)
                    || !TryGetIntArgument(command, "vertexIndex", out var removeVertexIndex))
                {
                    return false;
                }

                return _host.TryRemoveConnectionRouteVertex(
                    removeConnectionId,
                    removeVertexIndex,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.reconnect":
                if (!TryGetRequiredArgument(command, "connectionId", out var reconnectConnectionId))
                {
                    return false;
                }

                return _host.TryReconnectConnection(
                    reconnectConnectionId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.break-port":
                if (!TryGetRequiredArgument(command, "nodeId", out var nodeId)
                    || !TryGetRequiredArgument(command, "portId", out var portId))
                {
                    return false;
                }

                _host.BreakConnectionsForPort(nodeId, portId);
                return true;

            case "connections.disconnect-incoming":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectIncomingNodeId))
                {
                    return false;
                }

                _host.DisconnectIncoming(disconnectIncomingNodeId);
                return true;

            case "connections.disconnect-outgoing":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectOutgoingNodeId))
                {
                    return false;
                }

                _host.DisconnectOutgoing(disconnectOutgoingNodeId);
                return true;

            case "connections.disconnect-all":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectAllNodeId))
                {
                    return false;
                }

                _host.DisconnectAll(disconnectAllNodeId);
                return true;

            case "history.undo":
                _host.Undo();
                return true;

            case "history.redo":
                _host.Redo();
                return true;

            case "viewport.fit":
                _host.FitToViewport(updateStatus: true);
                return true;

            case "viewport.fit-selection":
                _host.FitSelectionToViewport(updateStatus: true);
                return true;

            case "viewport.focus-selection":
                _host.FocusSelection(updateStatus: true);
                return true;

            case "viewport.focus-current-scope":
                _host.FocusCurrentScope(updateStatus: true);
                return true;

            case "viewport.pan":
                if (!TryGetDoubleArgument(command, "deltaX", out var deltaX)
                    || !TryGetDoubleArgument(command, "deltaY", out var deltaY))
                {
                    return false;
                }

                _host.PanBy(deltaX, deltaY);
                return true;

            case "viewport.resize":
                if (!TryGetDoubleArgument(command, "width", out var width)
                    || !TryGetDoubleArgument(command, "height", out var height))
                {
                    return false;
                }

                _host.UpdateViewportSize(width, height);
                return true;

            case "viewport.reset":
                _host.ResetView(updateStatus: true);
                return true;

            case "viewport.center-node":
                if (!TryGetRequiredArgument(command, "nodeId", out var centerNodeId))
                {
                    return false;
                }

                _host.CenterViewOnNode(centerNodeId);
                return true;

            case "viewport.center":
                if (!TryGetDoubleArgument(command, "worldX", out var centerX)
                    || !TryGetDoubleArgument(command, "worldY", out var centerY))
                {
                    return false;
                }

                _host.CenterViewAt(new GraphPoint(centerX, centerY), ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "workspace.save":
                _host.SaveWorkspace();
                return true;

            case "workspace.load":
                _host.LoadWorkspace();
                return true;

            default:
                return false;
        }
    }

    private static bool ResolveOptionalUpdateStatus(GraphEditorCommandInvocationSnapshot command, string argumentName)
        => !command.TryGetArgument(argumentName, out var updateStatusValue)
            || !bool.TryParse(updateStatusValue, out var parsedUpdateStatus)
            || parsedUpdateStatus;

    private static GraphConnectionTargetRef CreateConnectionTarget(
        GraphEditorCommandInvocationSnapshot command,
        string targetNodeId,
        string targetId)
    {
        if (!command.TryGetArgument("targetKind", out var rawKind)
            || string.IsNullOrWhiteSpace(rawKind)
            || !Enum.TryParse<GraphConnectionTargetKind>(rawKind, ignoreCase: true, out var targetKind))
        {
            targetKind = GraphConnectionTargetKind.Port;
        }

        return new GraphConnectionTargetRef(targetNodeId, targetId, targetKind);
    }

    private static bool TryGetDoubleArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out double value)
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryGetIntArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out int value)
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || !int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryGetBoolArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out bool value)
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || !bool.TryParse(rawValue, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryGetEnumArgument<TEnum>(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out TEnum value)
        where TEnum : struct
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || string.IsNullOrWhiteSpace(rawValue)
            || !Enum.TryParse(rawValue, ignoreCase: true, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryResolveSelectionLayoutOperation(
        string commandId,
        out GraphSelectionLayoutOperation operation)
    {
        switch (commandId)
        {
            case "layout.align-left":
                operation = GraphSelectionLayoutOperation.AlignLeft;
                return true;
            case "layout.align-center":
                operation = GraphSelectionLayoutOperation.AlignCenter;
                return true;
            case "layout.align-right":
                operation = GraphSelectionLayoutOperation.AlignRight;
                return true;
            case "layout.align-top":
                operation = GraphSelectionLayoutOperation.AlignTop;
                return true;
            case "layout.align-middle":
                operation = GraphSelectionLayoutOperation.AlignMiddle;
                return true;
            case "layout.align-bottom":
                operation = GraphSelectionLayoutOperation.AlignBottom;
                return true;
            case "layout.distribute-horizontal":
                operation = GraphSelectionLayoutOperation.DistributeHorizontally;
                return true;
            case "layout.distribute-vertical":
                operation = GraphSelectionLayoutOperation.DistributeVertically;
                return true;
            default:
                operation = default;
                return false;
        }
    }

    private static NodePositionSnapshot? ParseNodePosition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 3
            || string.IsNullOrWhiteSpace(parts[0])
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return null;
        }

        return new NodePositionSnapshot(parts[0], new GraphPoint(x, y));
    }

    private static GraphLayoutNodePosition? ParseGraphLayoutNodePosition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length is not (3 or 4)
            || string.IsNullOrWhiteSpace(parts[0])
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || (parts.Length == 4 && !bool.TryParse(parts[3], out _)))
        {
            return null;
        }

        var isPinned = parts.Length == 4 && bool.Parse(parts[3]);
        return new GraphLayoutNodePosition(parts[0], new GraphPoint(x, y), isPinned);
    }

    private static bool ResolveOptionalResetManualRoutes(GraphEditorCommandInvocationSnapshot command)
        => command.TryGetArgument("resetManualRoutes", out var resetValue)
            && bool.TryParse(resetValue, out var parsedReset)
            && parsedReset;

    private static double ResolveOptionalGridSize(GraphEditorCommandInvocationSnapshot command)
        => TryGetDoubleArgument(command, "gridSize", out var gridSize)
            ? gridSize
            : 20d;

    private static GraphEditorNodeGroupMembershipChange? ParseGroupMembershipChange(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
        {
            return null;
        }

        return new GraphEditorNodeGroupMembershipChange(
            parts[0],
            string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1]);
    }

    private static bool TryGetRequiredArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        [NotNullWhen(true)] out string? value)
    {
        if (!command.TryGetArgument(name, out value) || string.IsNullOrWhiteSpace(value))
        {
            value = null;
            return false;
        }

        return true;
    }
}
