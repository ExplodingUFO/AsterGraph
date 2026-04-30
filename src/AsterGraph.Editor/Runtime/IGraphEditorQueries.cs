using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可见的图编辑器查询入口。
/// </summary>
public interface IGraphEditorQueries
{
    /// <summary>
    /// 生成当前图文档快照。
    /// </summary>
    /// <returns>当前不可变图文档。</returns>
    GraphDocument CreateDocumentSnapshot();

    /// <summary>
    /// 获取当前选择快照。
    /// </summary>
    /// <returns>当前选择状态。</returns>
    GraphEditorSelectionSnapshot GetSelectionSnapshot();

    IReadOnlyList<string> GetSelectedNodeConnectionIds()
        => [];

    GraphEditorSelectionTransformSnapshot GetSelectionTransformSnapshot(GraphEditorSelectionTransformQuery? query = null)
        => throw new NotSupportedException();

    GraphEditorSnapGuideSnapshot GetSnapGuideSnapshot(GraphEditorSnapGuideQuery? query = null)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前视口快照。
    /// </summary>
    /// <returns>当前视口状态。</returns>
    GraphEditorViewportSnapshot GetViewportSnapshot();

    /// <summary>
    /// Gets an adapter-neutral scene snapshot that combines document, viewport, selection, and persisted scene metadata.
    /// </summary>
    /// <returns>Current scene snapshot.</returns>
    GraphEditorSceneSnapshot GetSceneSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前能力快照。
    /// </summary>
    /// <returns>当前能力状态。</returns>
    GraphEditorCapabilitySnapshot GetCapabilitySnapshot();

    /// <summary>
    /// Gets canonical fragment workspace and template-library storage state for the active session.
    /// </summary>
    /// <returns>Stable storage snapshot for fragment-hosting UIs.</returns>
    GraphEditorFragmentStorageSnapshot GetFragmentStorageSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前运行时能力、服务和集成特性的显式描述集合。
    /// </summary>
    /// <returns>稳定的特性描述集合。</returns>
    IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets host-owned runtime overlay state for nodes, connections, and recent logs.
    /// </summary>
    /// <returns>Runtime overlay snapshot. Empty when the host did not provide runtime feedback.</returns>
    GraphEditorRuntimeOverlaySnapshot GetRuntimeOverlaySnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets document-level validation feedback and readiness state.
    /// </summary>
    /// <returns>Validation snapshot for all graph scopes in the current document.</returns>
    GraphEditorValidationSnapshot GetValidationSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets previewable quick repair actions for one current validation issue.
    /// </summary>
    /// <param name="issue">Validation issue projected by <see cref="GetValidationSnapshot"/>.</param>
    /// <returns>Actions that can be proven against the current graph state.</returns>
    IReadOnlyList<GraphEditorValidationRepairActionSnapshot> GetValidationIssueRepairActions(GraphEditorValidationIssueSnapshot issue)
        => [];

    /// <summary>
    /// Creates a host-owned graph layout plan without mutating the current document.
    /// </summary>
    /// <param name="request">The requested layout scope, orientation, spacing, and pinned node hints.</param>
    /// <returns>A layout plan, or an unavailable plan when no host provider is configured.</returns>
    GraphLayoutPlan CreateLayoutPlan(GraphLayoutRequest request)
        => throw new NotSupportedException();

    /// <summary>
    /// Gets canonical fragment template snapshots projected from the active template library service.
    /// </summary>
    /// <returns>Stable template metadata collection for host-side fragment library UIs.</returns>
    IReadOnlyList<GraphEditorFragmentTemplateSnapshot> GetFragmentTemplateSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets the currently registered node definitions exposed by the active node catalog.
    /// </summary>
    /// <returns>A stable read-only definition collection for host discovery scenarios.</returns>
    IReadOnlyList<INodeDefinition> GetRegisteredNodeDefinitions()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets canonical node template snapshots projected from the active node catalog.
    /// </summary>
    /// <returns>Stable read-only template metadata suitable for host-side insertion UIs.</returns>
    IReadOnlyList<GraphEditorNodeTemplateSnapshot> GetNodeTemplateSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets a searchable and filterable palette projection for node and fragment templates.
    /// </summary>
    /// <param name="query">Optional search and filter request. Omit for the full deterministic palette.</param>
    /// <returns>Stable template palette projection for host-side palette UIs.</returns>
    GraphEditorTemplatePaletteSnapshot GetTemplatePaletteSnapshot(GraphEditorTemplatePaletteQuery? query = null)
        => throw new NotSupportedException();

    /// <summary>
    /// Gets the shared definition for the current selection when every selected node resolves to the same catalog definition.
    /// </summary>
    /// <returns>The shared node definition, or <see langword="null"/> when the selection is empty or heterogeneous.</returns>
    INodeDefinition? GetSharedSelectionDefinition()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets parameter snapshots projected from the current selection and shared node definition.
    /// </summary>
    /// <returns>A stable snapshot collection suitable for host-side property editors and inspection UIs.</returns>
    IReadOnlyList<GraphEditorNodeParameterSnapshot> GetSelectedNodeParameterSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets parameter snapshots projected for one node-local authoring surface.
    /// </summary>
    /// <param name="nodeId">Target node identifier in the active scope.</param>
    /// <returns>A stable snapshot collection suitable for hosted node-side authoring UIs.</returns>
    IReadOnlyList<GraphEditorNodeParameterSnapshot> GetNodeParameterSnapshots(string nodeId)
        => throw new NotSupportedException();

    /// <summary>
    /// Gets stable node-surface snapshots for every node in the current document.
    /// </summary>
    /// <returns>One surface snapshot per node.</returns>
    IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets stable committed connection geometry snapshots for the current document.
    /// </summary>
    /// <returns>One geometry snapshot per committed connection.</returns>
    IReadOnlyList<GraphEditorConnectionGeometrySnapshot> GetConnectionGeometrySnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets canonical hierarchy state for the active scope, including group membership, collapsed descendants, and composite ownership.
    /// </summary>
    /// <returns>Stable hierarchy-state snapshot for host-side hierarchy-aware authoring surfaces.</returns>
    GraphEditorHierarchyStateSnapshot GetHierarchyStateSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets a source-backed navigator/outline projection for the active scope.
    /// </summary>
    /// <returns>Stable outline items projected from graph, hierarchy, scope, and selection snapshots.</returns>
    GraphEditorNavigatorOutlineSnapshot GetNavigatorOutlineSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets host-facing composite node snapshots for the current root graph.
    /// </summary>
    /// <returns>A stable read-only composite snapshot collection.</returns>
    IReadOnlyList<GraphEditorCompositeNodeSnapshot> GetCompositeNodeSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets the current active-scope navigation snapshot.
    /// </summary>
    /// <returns>Stable scope navigation metadata for hosted navigation surfaces.</returns>
    GraphEditorScopeNavigationSnapshot GetScopeNavigationSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets persisted editor-only node groups for the current document.
    /// </summary>
    /// <returns>A stable read-only group collection.</returns>
    IReadOnlyList<GraphNodeGroup> GetNodeGroups()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets resolved editor-only node-group boundary snapshots for the current document.
    /// </summary>
    /// <returns>A stable read-only group snapshot collection.</returns>
    IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前稳定命令描述集合。
    /// </summary>
    /// <returns>命令描述集合。</returns>
    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets stock runtime command placement metadata backed by the shared session command route.
    /// </summary>
    /// <returns>Runtime command registry entries keyed by stable session command ids.</returns>
    IReadOnlyList<GraphEditorCommandRegistryEntrySnapshot> GetCommandRegistry()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets stable contextual tool descriptors backed by the shared command route.
    /// </summary>
    /// <param name="context">The requested tool context.</param>
    /// <returns>Stable tool descriptors for the requested context.</returns>
    IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolContextSnapshot context)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取当前插件加载检查快照集合。
    /// </summary>
    /// <returns>插件加载快照集合。</returns>
    IReadOnlyList<GraphEditorPluginLoadSnapshot> GetPluginLoadSnapshots()
        => throw new NotSupportedException();

    /// <summary>
    /// 基于命中上下文生成框架无关的菜单描述集合。
    /// </summary>
    /// <param name="context">当前菜单上下文。</param>
    /// <returns>菜单描述集合。</returns>
    IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildContextMenuDescriptors(ContextMenuContext context)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取全部节点位置快照。
    /// </summary>
    /// <returns>节点位置集合。</returns>
    IReadOnlyList<NodePositionSnapshot> GetNodePositions();

    /// <summary>
    /// 获取当前待完成连线快照。
    /// </summary>
    /// <returns>当前待完成连线状态。</returns>
    GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => throw new NotSupportedException();

    /// <summary>
    /// Gets canonical edge template snapshots for the specified source output port.
    /// </summary>
    /// <param name="sourceNodeId">Source node instance identifier.</param>
    /// <param name="sourcePortId">Source output port identifier.</param>
    /// <returns>Stable edge-template snapshots suitable for host-side edge-authoring UIs.</returns>
    IReadOnlyList<GraphEditorEdgeTemplateSnapshot> GetEdgeTemplateSnapshots(string sourceNodeId, string sourcePortId)
        => throw new NotSupportedException();

    /// <summary>
    /// 获取指定源端口的运行时兼容连接目标。
    /// </summary>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <returns>兼容目标 DTO 集合。</returns>
    IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => throw new NotSupportedException();

    /// <summary>
    /// Gets compatible node definitions that can complete the current pending connection.
    /// </summary>
    /// <returns>Compatible node-definition targets plus an empty-state reason when none are available.</returns>
    GraphEditorCompatibleNodeSearchSnapshot GetCompatibleNodeDefinitionsForPendingConnection()
        => throw new NotSupportedException();

    /// <summary>
    /// 获取指定源端口的兼容连接目标。
    /// </summary>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <remarks>
    /// 此成员保留为兼容 shim，用于依赖 MVVM 运行时对象的旧宿主代码。
    /// 新的 canonical runtime queries 应优先使用 <see cref="GetCompatiblePortTargets(string, string)"/>。
    /// v1.5 迁移窗口内仍保留该 shim；后续 minor 版本可能增加更强的警告，
    /// future major release may remove it。
    /// </remarks>
    /// <returns>兼容目标集合。</returns>
    [Obsolete("Compatibility-only shim. Use GetCompatiblePortTargets(string, string) for canonical runtime queries. The v1.5 migration window keeps this shim, later minor releases may add stronger warnings, and a future major release may remove it.")]
    IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId);
}
