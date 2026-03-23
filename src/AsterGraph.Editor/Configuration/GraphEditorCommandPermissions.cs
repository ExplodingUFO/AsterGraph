namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 图编辑器命令权限入口。
/// </summary>
public sealed record GraphEditorCommandPermissions
{
    /// <summary>
    /// 默认权限配置。
    /// </summary>
    public static GraphEditorCommandPermissions Default { get; } = new();

    /// <summary>
    /// 只读预设。保留查看、选择、缩放和平移能力，禁用所有会修改图数据的命令。
    /// </summary>
    public static GraphEditorCommandPermissions ReadOnly { get; } = new()
    {
        Workspace = new WorkspaceCommandPermissions
        {
            AllowSave = false,
            AllowLoad = false,
        },
        History = new HistoryCommandPermissions
        {
            AllowUndo = false,
            AllowRedo = false,
        },
        Nodes = new NodeCommandPermissions
        {
            AllowCreate = false,
            AllowDelete = false,
            AllowMove = false,
            AllowDuplicate = false,
            AllowEditParameters = false,
        },
        Connections = new ConnectionCommandPermissions
        {
            AllowCreate = false,
            AllowDelete = false,
            AllowDisconnect = false,
        },
        Clipboard = new ClipboardCommandPermissions
        {
            AllowCopy = false,
            AllowPaste = false,
        },
        Layout = new LayoutCommandPermissions
        {
            AllowAlign = false,
            AllowDistribute = false,
        },
        Fragments = new FragmentCommandPermissions
        {
            AllowImport = false,
            AllowExport = false,
            AllowClearWorkspaceFragment = false,
            AllowTemplateManagement = false,
        },
        Host = new HostCommandPermissions
        {
            AllowContextMenuExtensions = true,
        },
    };

    /// <summary>
    /// 工作区相关权限。
    /// </summary>
    public WorkspaceCommandPermissions Workspace { get; init; } = new();

    /// <summary>
    /// 历史记录相关权限。
    /// </summary>
    public HistoryCommandPermissions History { get; init; } = new();

    /// <summary>
    /// 节点相关权限。
    /// </summary>
    public NodeCommandPermissions Nodes { get; init; } = new();

    /// <summary>
    /// 连线相关权限。
    /// </summary>
    public ConnectionCommandPermissions Connections { get; init; } = new();

    /// <summary>
    /// 剪贴板相关权限。
    /// </summary>
    public ClipboardCommandPermissions Clipboard { get; init; } = new();

    /// <summary>
    /// 布局相关权限。
    /// </summary>
    public LayoutCommandPermissions Layout { get; init; } = new();

    /// <summary>
    /// 片段相关权限。
    /// </summary>
    public FragmentCommandPermissions Fragments { get; init; } = new();

    /// <summary>
    /// 宿主扩展相关权限。
    /// </summary>
    public HostCommandPermissions Host { get; init; } = new();
}
