using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Demo.Menus;

/// <summary>
/// 演示宿主如何向节点右键菜单追加业务结果项。
/// </summary>
public sealed class DemoNodeResultsMenuContributor : IGraphContextMenuAugmentor
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
    public IReadOnlyList<MenuItemDescriptor> Augment(
        GraphEditorViewModel editor,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(stockItems);

        if (context.TargetKind != ContextMenuTargetKind.Node || string.IsNullOrWhiteSpace(context.ClickedNodeId))
        {
            return stockItems;
        }

        var node = editor.FindNode(context.ClickedNodeId);
        if (node is null)
        {
            return stockItems;
        }

        var resultItems = new List<MenuItemDescriptor>(stockItems);
        if (resultItems.Count > 0 && !resultItems[^1].IsSeparator)
        {
            resultItems.Add(MenuItemDescriptor.Separator($"demo-results-sep-{node.Id}"));
        }

        resultItems.Add(
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
                iconKey: "node"));
        return resultItems;
    }
}
