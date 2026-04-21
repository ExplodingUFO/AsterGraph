using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    /// <summary>
    /// 获取当前命令权限配置。
    /// </summary>
    public GraphEditorCommandPermissions CommandPermissions => BehaviorOptions.Commands;

    /// <summary>
    /// 获取当前编辑器行为配置。
    /// </summary>
    public GraphEditorBehaviorOptions BehaviorOptions
    {
        get => _behaviorOptions;
        private set
        {
            if (SetProperty(ref _behaviorOptions, value))
            {
                if (!_isInitialized)
                {
                    return;
                }

                OnPropertyChanged(nameof(CommandPermissions));
                ExportSelectionAsTemplateCommand.NotifyCanExecuteChanged();
                RaiseComputedPropertyChanges();
            }
        }
    }

    /// <summary>
    /// 获取或设置宿主右键菜单增强器。
    /// </summary>
    public IGraphContextMenuAugmentor? ContextMenuAugmentor
    {
        get => _contextMenuAugmentor;
        set => SetProperty(ref _contextMenuAugmentor, value);
    }

    /// <summary>
    /// 获取当前宿主上下文信息。
    /// </summary>
    public IGraphHostContext? HostContext => _hostContext;

    /// <summary>
    /// 获取当前节点展示状态提供器。
    /// </summary>
    public INodePresentationProvider? NodePresentationProvider => _nodePresentationProvider;

    /// <summary>
    /// 获取当前图编辑器内置文案本地化提供器。
    /// </summary>
    public IGraphLocalizationProvider? LocalizationProvider => _localizationProvider;

    /// <summary>
    /// 当前工作区快照文件路径。
    /// </summary>
    public string WorkspacePath => _workspaceService.WorkspacePath;

    /// <summary>
    /// 当前默认片段文件路径。
    /// </summary>
    public string FragmentPath => _fragmentWorkspaceService.FragmentPath;

    /// <summary>
    /// 当前片段模板库目录路径。
    /// </summary>
    public string FragmentLibraryPath => _fragmentLibraryService.LibraryPath;

    /// <summary>
    /// 当前视口宽度。
    /// </summary>
    public double ViewportWidth => _viewportWidth;

    /// <summary>
    /// 当前视口高度。
    /// </summary>
    public double ViewportHeight => _viewportHeight;

    /// <summary>
    /// 获取将当前图保存到默认工作区文件的命令。
    /// </summary>
    public IRelayCommand SaveCommand { get; }

    /// <summary>
    /// 获取从默认工作区文件加载图快照的命令。
    /// </summary>
    public IRelayCommand LoadCommand { get; }

    /// <summary>
    /// 获取根据当前节点边界调整视口的命令。
    /// </summary>
    public IRelayCommand FitViewCommand { get; }

    /// <summary>
    /// 获取将缩放和平移恢复为默认视图状态的命令。
    /// </summary>
    public IRelayCommand ResetViewCommand { get; }

    /// <summary>
    /// 获取回退到上一条历史快照的命令。
    /// </summary>
    public IRelayCommand UndoCommand { get; }

    /// <summary>
    /// 获取重新应用下一条历史快照的命令。
    /// </summary>
    public IRelayCommand RedoCommand { get; }

    /// <summary>
    /// 获取删除当前选择内容的命令。
    /// </summary>
    public IRelayCommand DeleteSelectionCommand { get; }

    /// <summary>
    /// 获取将当前选择复制到编辑器剪贴板及宿主文本剪贴板的命令。
    /// </summary>
    public IAsyncRelayCommand CopySelectionCommand { get; }

    /// <summary>
    /// 获取从编辑器剪贴板或宿主文本剪贴板粘贴内容的命令。
    /// </summary>
    public IAsyncRelayCommand PasteCommand { get; }

    /// <summary>
    /// 获取将当前选择导出到默认片段文件的命令。
    /// </summary>
    public IRelayCommand ExportSelectionFragmentCommand { get; }

    /// <summary>
    /// 获取从默认片段文件导入并粘贴内容的命令。
    /// </summary>
    public IRelayCommand ImportFragmentCommand { get; }

    /// <summary>
    /// 获取删除默认片段文件的命令。
    /// </summary>
    public IRelayCommand ClearFragmentCommand { get; }

    /// <summary>
    /// 获取重新加载片段模板库内容的命令。
    /// </summary>
    public IRelayCommand RefreshFragmentTemplatesCommand { get; }

    /// <summary>
    /// 获取将当前选择保存为片段模板的命令。
    /// </summary>
    public IRelayCommand ExportSelectionAsTemplateCommand { get; }

    /// <summary>
    /// 获取导入当前选中片段模板的命令。
    /// </summary>
    public IRelayCommand ImportSelectedTemplateCommand { get; }

    /// <summary>
    /// 获取删除当前选中片段模板的命令。
    /// </summary>
    public IRelayCommand DeleteSelectedTemplateCommand { get; }

    /// <summary>
    /// 获取按左边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignLeftCommand { get; }

    /// <summary>
    /// 获取按水平中心对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignCenterCommand { get; }

    /// <summary>
    /// 获取按右边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignRightCommand { get; }

    /// <summary>
    /// 获取按上边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignTopCommand { get; }

    /// <summary>
    /// 获取按垂直中心对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignMiddleCommand { get; }

    /// <summary>
    /// 获取按下边缘对齐当前选择的命令。
    /// </summary>
    public IRelayCommand AlignBottomCommand { get; }

    /// <summary>
    /// 获取按水平方向均匀分布当前选择的命令。
    /// </summary>
    public IRelayCommand DistributeHorizontallyCommand { get; }

    /// <summary>
    /// 获取按垂直方向均匀分布当前选择的命令。
    /// </summary>
    public IRelayCommand DistributeVerticallyCommand { get; }

    /// <summary>
    /// 获取取消当前连线预览的命令。
    /// </summary>
    public IRelayCommand CancelPendingConnectionCommand { get; }

    /// <summary>
    /// 获取基于给定模板创建节点的命令。
    /// </summary>
    public IRelayCommand<GraphEditorNodeTemplateSnapshot> AddNodeCommand { get; }

    IEnumerable<GraphEditorNodeTemplateSnapshot> IGraphContextMenuHost.NodeTemplates => NodeTemplates;

    IEnumerable<NodeViewModel> IGraphContextMenuHost.Nodes => Nodes;

    IEnumerable<NodeViewModel> IGraphContextMenuHost.SelectedNodes => SelectedNodes;

    GraphEditorCommandPermissions IGraphContextMenuHost.CommandPermissions => CommandPermissions;

    int IGraphContextMenuHost.SelectedNodeCount => SelectedNodes.Count;

    ICommand IGraphContextMenuHost.DeleteSelectionCommand => DeleteSelectionCommand;

    ICommand IGraphContextMenuHost.CopySelectionCommand => CopySelectionCommand;

    ICommand IGraphContextMenuHost.ExportSelectionFragmentCommand => ExportSelectionFragmentCommand;

    ICommand IGraphContextMenuHost.ImportFragmentCommand => ImportFragmentCommand;

    ICommand IGraphContextMenuHost.FitViewCommand => FitViewCommand;

    ICommand IGraphContextMenuHost.ResetViewCommand => ResetViewCommand;

    ICommand IGraphContextMenuHost.SaveCommand => SaveCommand;

    ICommand IGraphContextMenuHost.LoadCommand => LoadCommand;

    ICommand IGraphContextMenuHost.PasteCommand => PasteCommand;

    ICommand IGraphContextMenuHost.AlignLeftCommand => AlignLeftCommand;

    ICommand IGraphContextMenuHost.AlignCenterCommand => AlignCenterCommand;

    ICommand IGraphContextMenuHost.AlignRightCommand => AlignRightCommand;

    ICommand IGraphContextMenuHost.AlignTopCommand => AlignTopCommand;

    ICommand IGraphContextMenuHost.AlignMiddleCommand => AlignMiddleCommand;

    ICommand IGraphContextMenuHost.AlignBottomCommand => AlignBottomCommand;

    ICommand IGraphContextMenuHost.DistributeHorizontallyCommand => DistributeHorizontallyCommand;

    ICommand IGraphContextMenuHost.DistributeVerticallyCommand => DistributeVerticallyCommand;

    ICommand IGraphContextMenuHost.CancelPendingConnectionCommand => CancelPendingConnectionCommand;

    IReadOnlyList<GraphEditorEdgeTemplateSnapshot> IGraphContextMenuHost.GetEdgeTemplateSnapshots(string sourceNodeId, string sourcePortId)
        => Session.Queries.GetEdgeTemplateSnapshots(sourceNodeId, sourcePortId);
}
