namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    /// <summary>
    /// 当前是否存在等待完成的连线预览。
    /// </summary>
    public bool HasPendingConnection => PendingSourceNode is not null && PendingSourcePort is not null;

    /// <summary>
    /// 指示宿主权限当前是否允许保存工作区快照。
    /// </summary>
    public bool CanSaveWorkspace => CommandPermissions.Workspace.AllowSave;

    /// <summary>
    /// 指示宿主权限当前是否允许加载工作区快照。
    /// </summary>
    public bool CanLoadWorkspace => CommandPermissions.Workspace.AllowLoad;

    /// <summary>
    /// 指示撤销命令当前是否可用。
    /// </summary>
    public bool CanUndo
        => BehaviorOptions.History.EnableUndoRedo
           && CommandPermissions.History.AllowUndo
           && (_kernel.GetCapabilitySnapshot().CanUndo || _historyService.CanUndo);

    /// <summary>
    /// 指示重做命令当前是否可用。
    /// </summary>
    public bool CanRedo
        => BehaviorOptions.History.EnableUndoRedo
           && CommandPermissions.History.AllowRedo
           && (_kernel.GetCapabilitySnapshot().CanRedo || _historyService.CanRedo);

    /// <summary>
    /// 指示当前是否至少选中了一个节点。
    /// </summary>
    public bool HasSelection => SelectedNodes.Count > 0;

    /// <summary>
    /// 指示当前是否同时选中了多个节点。
    /// </summary>
    public bool HasMultipleSelection => SelectedNodes.Count > 1;

    /// <summary>
    /// 指示宿主权限当前是否允许创建节点。
    /// </summary>
    public bool CanCreateNodes => CommandPermissions.Nodes.AllowCreate;

    /// <summary>
    /// 指示当前选择是否满足删除条件且宿主允许删除。
    /// </summary>
    public bool CanDeleteSelection => HasSelection && CommandPermissions.Nodes.AllowDelete;

    /// <summary>
    /// 指示当前选择是否满足复制条件且宿主允许复制。
    /// </summary>
    public bool CanCopySelection => HasSelection && CommandPermissions.Clipboard.AllowCopy;

    /// <summary>
    /// 指示当前是否允许将片段内容插入图中。
    /// </summary>
    public bool CanInsertFragmentContent => CommandPermissions.Nodes.AllowCreate;

    /// <summary>
    /// 指示当前是否允许执行粘贴。
    /// </summary>
    public bool CanPaste => CommandPermissions.Clipboard.AllowPaste && CanInsertFragmentContent && (_selectionClipboard.HasContent || _textClipboardBridge is not null);

    /// <summary>
    /// 指示当前选择是否可导出为默认片段文件。
    /// </summary>
    public bool CanExportSelectionFragment => HasSelection && CommandPermissions.Fragments.AllowExport;

    /// <summary>
    /// 指示默认片段文件当前是否存在且允许被导入。
    /// </summary>
    public bool CanImportFragment => CommandPermissions.Fragments.AllowImport && CanInsertFragmentContent && _fragmentWorkspaceService.Exists();

    /// <summary>
    /// 指示默认片段文件当前是否存在且允许被清理。
    /// </summary>
    public bool CanClearFragment => CommandPermissions.Fragments.AllowClearWorkspaceFragment && _fragmentWorkspaceService.Exists();

    /// <summary>
    /// 指示片段模板库当前是否包含任何模板。
    /// </summary>
    public bool HasFragmentTemplates => FragmentTemplates.Count > 0;

    /// <summary>
    /// 指示当前选择是否可导出为片段模板。
    /// </summary>
    public bool CanExportSelectionAsTemplate => CanExportSelectionFragment && CommandPermissions.Fragments.AllowTemplateManagement && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选中的片段模板是否可导入。
    /// </summary>
    public bool CanImportSelectedTemplate => SelectedFragmentTemplate is not null && CommandPermissions.Fragments.AllowImport && CommandPermissions.Fragments.AllowTemplateManagement && CanInsertFragmentContent && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选中的片段模板是否可删除。
    /// </summary>
    public bool CanDeleteSelectedTemplate => SelectedFragmentTemplate is not null && CommandPermissions.Fragments.AllowTemplateManagement && BehaviorOptions.Fragments.EnableFragmentLibrary;

    /// <summary>
    /// 指示当前选择是否满足对齐操作条件。
    /// </summary>
    public bool CanAlignSelection => SelectedNodes.Count >= 2 && CommandPermissions.Layout.AllowAlign;

    /// <summary>
    /// 指示当前选择是否满足分布操作条件。
    /// </summary>
    public bool CanDistributeSelection => SelectedNodes.Count >= 3 && CommandPermissions.Layout.AllowDistribute;

    /// <summary>
    /// 指示当前单选节点是否暴露了可编辑参数。
    /// </summary>
    public bool HasEditableParameters => SelectedNodes.Count == 1 && SelectedNodeParameters.Count > 0;

    /// <summary>
    /// 指示当前多选节点是否存在可批量编辑的共享参数。
    /// </summary>
    public bool HasBatchEditableParameters => SelectedNodes.Count > 1 && SelectedNodeParameters.Count > 0 && CanEditNodeParameters;

    /// <summary>
    /// 指示当前选择投影是否产生了任何参数项。
    /// </summary>
    public bool HasAnyEditableParameters => SelectedNodeParameters.Count > 0;

    /// <summary>
    /// 指示宿主权限当前是否允许编辑节点参数。
    /// </summary>
    public bool CanEditNodeParameters => CommandPermissions.Nodes.AllowEditParameters;

    /// <summary>
    /// 面向宿主和状态栏的图统计文本。
    /// </summary>
    public string StatsCaption => LocalizeFormat(
        "editor.stats.caption",
        "{0} nodes  ·  {1} links  ·  {2:0}% zoom",
        Nodes.Count,
        Connections.Count,
        Zoom * 100);

    /// <summary>
    /// 工作区状态摘要文本。
    /// </summary>
    public string WorkspaceCaption
        => _storageProjectionSupport.GetWorkspaceCaption();

    /// <summary>
    /// 片段工作区状态摘要文本。
    /// </summary>
    public string FragmentCaption
        => _storageProjectionSupport.GetFragmentCaption();

    /// <summary>
    /// 片段文件更新时间摘要文本。
    /// </summary>
    public string FragmentStatusCaption
        => _storageProjectionSupport.GetFragmentStatusCaption();

    /// <summary>
    /// 片段模板库状态摘要文本。
    /// </summary>
    public string FragmentLibraryCaption
        => _storageProjectionSupport.GetFragmentLibraryCaption();

    /// <summary>
    /// 当前编辑模式摘要文本。
    /// </summary>
    public string ModeCaption => HasPendingConnection
        ? LocalizeFormat(
            "editor.mode.connecting",
            "Connecting {0} / {1}  ->  click an input port",
            PendingSourceNode!.Title,
            PendingSourcePort!.Label)
        : HasMultipleSelection
            ? LocalizeFormat(
                "editor.mode.selection.multiple",
                "Selection mode  ·  {0} nodes selected",
                SelectedNodes.Count)
            : LocalizeText(
                "editor.mode.selection.default",
                "Selection mode  ·  click a template to add a node");

    /// <summary>
    /// 获取基于当前选择生成的检查器标题文本。
    /// </summary>
    public string InspectorTitle => SelectedNodes.Count switch
    {
        0 => LocalizeText("editor.inspector.title.none", "Select A Node"),
        1 => SelectedNode?.Title ?? LocalizeText("editor.inspector.title.none", "Select A Node"),
        _ => LocalizeFormat("editor.inspector.title.multiple", "{0} Nodes Selected", SelectedNodes.Count),
    };

    /// <summary>
    /// 获取基于当前选择生成的检查器分类文本。
    /// </summary>
    public string InspectorCategory => SelectedNodes.Count switch
    {
        0 => LocalizeText("editor.inspector.category.none", "Editor"),
        1 => SelectedNode?.Category ?? LocalizeText("editor.inspector.category.none", "Editor"),
        _ => LocalizeText("editor.inspector.category.multiple", "Multi Selection"),
    };

    /// <summary>
    /// 获取基于当前选择生成的检查器描述文本。
    /// </summary>
    public string InspectorDescription => SelectedNodes.Count switch
    {
        0 => LocalizeText(
            "editor.inspector.description.none",
            "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar."),
        1 => SelectedNode?.Description ?? LocalizeText(
            "editor.inspector.description.none",
            "Build the graph from the left library, connect outputs to inputs, and save snapshots from the toolbar."),
        _ => HasBatchEditableParameters
            ? LocalizeFormat(
                "editor.inspector.description.multiple.batch",
                "Editing shared parameters across {0} nodes of the same definition.",
                SelectedNodes.Count)
            : LocalizeText(
                "editor.inspector.description.multiple.default",
                "Delete removes the full selection. Copy and paste preserve internal links between the selected nodes."),
    };

    /// <summary>
    /// 获取当前主选节点的输入端口摘要文本。
    /// </summary>
    public string InspectorInputs => SelectedNode is null
        ? LocalizeText("editor.inspector.inputs.none", "Select a node to inspect its input ports.")
        : _selectionProjection.FormatPorts(SelectedNode.Inputs);

    /// <summary>
    /// 获取当前主选节点的输出端口摘要文本。
    /// </summary>
    public string InspectorOutputs => SelectedNode is null
        ? LocalizeText("editor.inspector.outputs.none", "Select a node to inspect its output ports.")
        : _selectionProjection.FormatPorts(SelectedNode.Outputs);

    /// <summary>
    /// 获取当前主选节点的连线统计摘要文本。
    /// </summary>
    public string InspectorConnections => SelectedNode is null
        ? LocalizeText("editor.inspector.connections.none", "Select a node to inspect its connection summary.")
        : _inspectorConnectionsText;

    /// <summary>
    /// 获取当前主选节点的上游依赖摘要文本。
    /// </summary>
    public string InspectorUpstream => SelectedNode is null
        ? LocalizeText("editor.inspector.upstream.none", "Select a node to see upstream dependencies.")
        : _inspectorUpstreamText;

    /// <summary>
    /// 获取当前主选节点的下游消费者摘要文本。
    /// </summary>
    public string InspectorDownstream => SelectedNode is null
        ? LocalizeText("editor.inspector.downstream.none", "Select a node to see downstream consumers.")
        : _inspectorDownstreamText;

    /// <summary>
    /// 获取面向状态栏和检查器的当前选择摘要文本。
    /// </summary>
    public string SelectionCaption => _selectionCaptionText;
}
