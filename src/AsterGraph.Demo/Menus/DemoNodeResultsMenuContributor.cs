using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Demo.Menus;

/// <summary>
/// 演示宿主如何向节点右键菜单追加业务结果项。
/// </summary>
public sealed class DemoNodeResultsMenuContributor : IGraphContextMenuContributor
{
    private readonly Action<string> _reportStatus;

    /// <summary>
    /// 初始化节点结果菜单扩展器。
    /// </summary>
    /// <param name="reportStatus">用于回写宿主状态栏文本的委托。</param>
    public DemoNodeResultsMenuContributor(Action<string> reportStatus)
    {
        _reportStatus = reportStatus ?? throw new ArgumentNullException(nameof(reportStatus));
    }

    /// <inheritdoc />
    public IReadOnlyList<MenuItemDescriptor> Contribute(GraphContextMenuExtensionContext context)
    {
        if (context.TargetKind != ContextMenuTargetKind.Node || context.ClickedNode is null)
        {
            return [];
        }

        var node = context.ClickedNode;
        return
        [
            new MenuItemDescriptor(
                $"demo-results-{node.Id}",
                "Results",
                children:
                [
                    new MenuItemDescriptor(
                        $"demo-results-preview-{node.Id}",
                        "Preview",
                        new RelayCommand(() => _reportStatus($"Preview requested for {node.Title} [{node.Id}].")),
                        iconKey: "inspect"),
                    new MenuItemDescriptor(
                        $"demo-results-publish-{node.Id}",
                        "Publish",
                        new RelayCommand(() => _reportStatus($"Publish requested for {node.Title} [{node.Id}].")),
                        iconKey: "export"),
                    new MenuItemDescriptor(
                        $"demo-results-compare-{node.Id}",
                        "Create Comparison",
                        new RelayCommand(() => _reportStatus($"Comparison draft created for {node.Title} [{node.Id}].")),
                        iconKey: "copy"),
                ],
                iconKey: "node"),
        ];
    }
}
