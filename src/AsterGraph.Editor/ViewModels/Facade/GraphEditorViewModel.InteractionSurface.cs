using System;
using System.Collections.Generic;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    /// <summary>
    /// 为给定上下文构建右键菜单描述。
    /// </summary>
    /// <param name="context">当前菜单上下文。</param>
    /// <returns>供视图层渲染的菜单项集合。</returns>
    public IReadOnlyList<MenuItemDescriptor> BuildContextMenu(ContextMenuContext context)
        => _compatibilityCommands.BuildContextMenu(context);

    /// <summary>
    /// 更新当前视口尺寸。
    /// </summary>
    /// <param name="width">视口宽度。</param>
    /// <param name="height">视口高度。</param>
    public void UpdateViewportSize(double width, double height)
        => _kernel.UpdateViewportSize(width, height);

    /// <summary>
    /// 一次性应用宿主提供的平台服务。
    /// </summary>
    /// <param name="services">新的平台服务聚合；为 <see langword="null"/> 时清空当前服务。</param>
    public void ApplyPlatformServices(GraphEditorPlatformServices? services)
        => ApplyPlatformServicesCore(services?.TextClipboardBridge, services?.HostContext);

    /// <summary>
    /// 配置宿主提供的纯文本剪贴板桥。
    /// </summary>
    /// <param name="bridge">宿主桥实现；为 <see langword="null"/> 时仅保留进程内剪贴板回退。</param>
    public void SetTextClipboardBridge(IGraphTextClipboardBridge? bridge)
        => ApplyPlatformServicesCore(bridge, _hostContext);

    /// <summary>
    /// 设置宿主上下文信息。
    /// </summary>
    /// <param name="hostContext">宿主上下文；为 <see langword="null"/> 时清空当前宿主上下文。</param>
    public void SetHostContext(IGraphHostContext? hostContext)
        => ApplyPlatformServicesCore(_textClipboardBridge, hostContext);

    private void ApplyPlatformServicesCore(
        IGraphTextClipboardBridge? textClipboardBridge,
        IGraphHostContext? hostContext)
    {
        if (!ReferenceEquals(_textClipboardBridge, textClipboardBridge))
        {
            _textClipboardBridge = textClipboardBridge;
            RaiseComputedPropertyChanges();
        }

        SetProperty(ref _hostContext, hostContext, nameof(HostContext));
    }

    /// <summary>
    /// 设置图编辑器内置文案本地化提供器。
    /// </summary>
    /// <param name="provider">本地化提供器；为 <see langword="null"/> 时回退到默认文案。</param>
    public void SetLocalizationProvider(IGraphLocalizationProvider? provider)
        => _presentationLocalizationCoordinator.SetLocalizationProvider(provider);

    /// <summary>
    /// 设置节点展示状态提供器。
    /// </summary>
    /// <param name="provider">新的展示状态提供器。</param>
    /// <param name="refreshImmediately">是否立即刷新当前全部节点展示状态。</param>
    public void SetNodePresentationProvider(INodePresentationProvider? provider, bool refreshImmediately = true)
        => _presentationLocalizationCoordinator.SetNodePresentationProvider(provider, refreshImmediately);

    /// <summary>
    /// 刷新单个节点的展示状态。
    /// </summary>
    /// <param name="nodeId">目标节点标识。</param>
    /// <returns>找到节点并完成刷新时返回 <see langword="true"/>。</returns>
    public bool RefreshNodePresentation(string nodeId)
        => _presentationLocalizationCoordinator.RefreshNodePresentation(nodeId);

    /// <summary>
    /// 刷新当前图中全部节点的展示状态。
    /// </summary>
    /// <returns>刷新节点数量。</returns>
    public int RefreshNodePresentations()
        => _presentationLocalizationCoordinator.RefreshNodePresentations();

    /// <summary>
    /// 重新扫描片段模板库并刷新当前模板列表。
    /// </summary>
    public void RefreshFragmentTemplates()
        => _storageProjectionSupport.RefreshFragmentTemplates();

    /// <summary>
    /// 在运行时替换编辑器行为配置。
    /// </summary>
    /// <param name="behaviorOptions">新的行为配置。</param>
    /// <param name="status">可选状态文本。</param>
    public void UpdateBehaviorOptions(GraphEditorBehaviorOptions behaviorOptions, string? status = null)
    {
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        _kernel.UpdateBehaviorOptions(behaviorOptions);
        BehaviorOptions = behaviorOptions;
        RefreshNodeSurfaceTiers();
        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    /// <summary>
    /// 将一组节点重定位到拖拽起点加偏移后的绝对位置。
    /// </summary>
    /// <param name="originPositions">拖拽开始时记录的节点起始位置。</param>
    /// <param name="deltaX">相对起始位置的水平偏移。</param>
    /// <param name="deltaY">相对起始位置的垂直偏移。</param>
    public void ApplyDragOffset(IReadOnlyDictionary<string, GraphPoint> originPositions, double deltaX, double deltaY)
        => _nodeLayoutCoordinator.ApplyDragOffset(originPositions, deltaX, deltaY);

    /// <summary>
    /// 开始记录一次连续交互的历史基线，通常用于节点拖动这类高频操作。
    /// </summary>
    public void BeginHistoryInteraction()
    {
        _pendingInteractionState ??= CaptureHistoryState();
    }

    /// <summary>
    /// 结束一次连续交互，并在状态发生变化时写入撤销栈。
    /// </summary>
    /// <param name="status">交互完成后显示的状态文本。</param>
    public void CompleteHistoryInteraction(string status)
    {
        if (_pendingInteractionState is null)
        {
            return;
        }

        var previousState = _pendingInteractionState;
        _pendingInteractionState = null;
        var currentState = CaptureHistoryState();
        if (string.Equals(previousState.Signature, currentState.Signature, StringComparison.Ordinal))
        {
            UpdateDirtyState(currentState.Signature);
            RaiseComputedPropertyChanges();
            return;
        }

        CommitRetainedMutation(
            status,
            GraphEditorDocumentChangeKind.LayoutChanged,
            currentState.SelectedNodeIds);
    }

    /// <summary>
    /// 撤销到上一个已记录的图编辑状态。
    /// </summary>
    public void Undo()
    {
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowUndo)
        {
            SetStatus("editor.status.history.undo.disabledByPermissions", "Undo is disabled by host permissions.");
            return;
        }

        if (!_sessionHost.GetCapabilitySnapshot().CanUndo)
        {
            SetStatus("editor.status.history.undo.none", "No more undo steps.");
            return;
        }

        _sessionHost.Undo();
    }

    /// <summary>
    /// 重做到下一个已记录的图编辑状态。
    /// </summary>
    public void Redo()
    {
        if (!BehaviorOptions.History.EnableUndoRedo || !CommandPermissions.History.AllowRedo)
        {
            SetStatus("editor.status.history.redo.disabledByPermissions", "Redo is disabled by host permissions.");
            return;
        }

        if (!_sessionHost.GetCapabilitySnapshot().CanRedo)
        {
            SetStatus("editor.status.history.redo.none", "No more redo steps.");
            return;
        }

        _sessionHost.Redo();
    }

    /// <summary>
    /// 选中指定节点，等价于单选该节点。
    /// </summary>
    /// <param name="node">要选中的节点；为 <see langword="null"/> 时清空选择。</param>
    public void SelectNode(NodeViewModel? node) => _selectionCoordinator.SelectSingleNode(node);

    /// <summary>
    /// 清空当前选择。
    /// </summary>
    /// <param name="updateStatus">是否同步更新状态文本。</param>
    public void ClearSelection(bool updateStatus = false)
        => _selectionCoordinator.ClearSelection(updateStatus);

    /// <summary>
    /// 将选择切换为单个节点。
    /// </summary>
    /// <param name="node">目标节点；为 <see langword="null"/> 时清空选择。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void SelectSingleNode(NodeViewModel? node, bool updateStatus = true)
        => _selectionCoordinator.SelectSingleNode(node, updateStatus);

    /// <summary>
    /// 将节点追加到当前选择集合。
    /// </summary>
    /// <param name="node">要追加的节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void AddNodeToSelection(NodeViewModel node, bool updateStatus = true)
        => _selectionCoordinator.AddNodeToSelection(node, updateStatus);

    /// <summary>
    /// 切换节点在当前选择集合中的状态。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    public void ToggleNodeSelection(NodeViewModel node, bool updateStatus = true)
        => _selectionCoordinator.ToggleNodeSelection(node, updateStatus);

    /// <summary>
    /// 直接设置当前选择集合及主选中节点。
    /// </summary>
    /// <param name="nodes">新的选择集合。</param>
    /// <param name="primaryNode">新的主选中节点。</param>
    /// <param name="status">可选状态文本。</param>
    public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
        => _selectionCoordinator.SetSelection(nodes, primaryNode, status);

    private void SetSelectionCore(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
        => _selectionCoordinator.SetSelectionCore(nodes, primaryNode, status);
}
