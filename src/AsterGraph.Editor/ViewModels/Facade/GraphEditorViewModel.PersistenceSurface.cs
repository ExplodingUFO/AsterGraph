using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private GraphSelectionFragment? CreateSelectionFragment()
        => _fragmentCommands.CreateSelectionFragment();

    private string? PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
        => _fragmentCommands.PasteFragment(fragment, actionPrefix);

    private Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
        => _fragmentCommands.GetBestAvailableClipboardFragmentAsync();

    /// <summary>
    /// 复制当前选择，并尽可能同步到系统剪贴板 JSON 文本。
    /// </summary>
    public Task CopySelectionAsync()
        => _fragmentCommands.CopySelectionAsync();

    /// <summary>
    /// 将当前选择导出到默认片段文件。
    /// </summary>
    public void ExportSelectionFragment()
        => _fragmentCommands.ExportSelectionFragment();

    /// <summary>
    /// 将当前选择导出为片段模板。
    /// </summary>
    public void ExportSelectionAsTemplate()
        => _fragmentCommands.ExportSelectionAsTemplate();

    /// <summary>
    /// 将当前选择导出到指定片段文件路径。
    /// </summary>
    /// <param name="path">目标文件路径。</param>
    /// <returns>导出成功时返回 <see langword="true"/>。</returns>
    public bool ExportSelectionFragmentTo(string path)
        => _fragmentCommands.ExportSelectionFragmentTo(path);

    /// <summary>
    /// 从系统剪贴板或进程内剪贴板粘贴当前片段。
    /// </summary>
    public Task PasteSelectionAsync()
        => _fragmentCommands.PasteSelectionAsync();

    /// <summary>
    /// 从默认片段文件导入片段。
    /// </summary>
    public void ImportFragment()
        => _fragmentCommands.ImportFragment();

    /// <summary>
    /// 清理默认片段文件。
    /// </summary>
    public void ClearFragment()
        => _fragmentCommands.ClearFragment();

    /// <summary>
    /// 导入当前选中的片段模板。
    /// </summary>
    public void ImportSelectedTemplate()
        => _fragmentCommands.ImportSelectedTemplate();

    /// <summary>
    /// 删除当前选中的片段模板。
    /// </summary>
    public void DeleteSelectedTemplate()
        => _fragmentCommands.DeleteSelectedTemplate();

    /// <summary>
    /// 从指定片段文件路径导入节点片段。
    /// </summary>
    /// <param name="path">源文件路径。</param>
    /// <returns>导入并粘贴成功时返回 <see langword="true"/>。</returns>
    public bool ImportFragmentFrom(string path)
        => _fragmentCommands.ImportFragmentFrom(path);

    /// <summary>
    /// 将当前图保存到默认工作区文件。
    /// </summary>
    public void SaveWorkspace()
        => _workspaceSaveCoordinator.SaveWorkspace();

    /// <summary>
    /// 从默认工作区文件加载图。
    /// </summary>
    /// <returns>加载成功时返回 <see langword="true"/>。</returns>
    public bool LoadWorkspace()
        => _kernel.LoadWorkspace();

    /// <summary>
    /// 生成当前图文档的不可变快照。
    /// </summary>
    public GraphDocument CreateDocumentSnapshot()
        => _kernel.CreateDocumentSnapshot();

    internal void ApplyKernelDocument(GraphDocument document, string status, bool markClean)
        => _kernelProjectionApplier.ApplyDocument(document, status, markClean);

    internal void ApplyKernelSelection(GraphEditorSelectionSnapshot snapshot)
        => ApplyKernelSelection(snapshot.SelectedNodeIds, snapshot.PrimarySelectedNodeId);

    internal void ApplyKernelSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId)
        => _kernelProjectionApplier.ApplySelection(nodeIds, primaryNodeId);

    internal void ApplyKernelViewport(GraphEditorViewportSnapshot snapshot)
        => _kernelProjectionApplier.ApplyViewport(snapshot);

    internal void ApplyKernelPendingConnection(GraphEditorPendingConnectionSnapshot snapshot)
        => _kernelProjectionApplier.ApplyPendingConnection(snapshot);

    internal void ApplyKernelStatus(string statusMessage)
    {
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            return;
        }

        StatusMessage = statusMessage;
    }

    internal void ApplyKernelDirtyState(bool isDirty)
        => IsDirty = isDirty;

    /// <summary>
    /// 按实例标识查找节点视图模型。
    /// </summary>
    public NodeViewModel? FindNode(string nodeId)
        => _documentProjectionApplier.FindNode(nodeId);

    private void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory = true)
        => _documentLoadCoordinator.LoadDocument(document, status, markClean, resetHistory);

    private void MarkDirty(string status)
        => _historyStateCoordinator.MarkDirty(status);

    private GraphPoint GetViewportCenter()
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return ScreenToWorld(new GraphPoint(820, 440));
        }

        return ScreenToWorld(new GraphPoint(_viewportWidth / 2, _viewportHeight / 2));
    }
}
