using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal static class NodeSelectionLayoutService
{
    // 批量布局计算放在独立服务中，避免菜单、快捷键和外部 API 各自维护一份实现。
    public static void AlignLeft(IReadOnlyList<NodeViewModel> nodes)
    {
        var left = nodes.Min(node => node.X);
        foreach (var node in nodes)
        {
            node.X = left;
        }
    }

    public static void AlignCenter(IReadOnlyList<NodeViewModel> nodes)
    {
        var center = GetHorizontalCenter(nodes);
        foreach (var node in nodes)
        {
            node.X = center - (node.Width / 2);
        }
    }

    public static void AlignRight(IReadOnlyList<NodeViewModel> nodes)
    {
        var right = nodes.Max(node => node.X + node.Width);
        foreach (var node in nodes)
        {
            node.X = right - node.Width;
        }
    }

    public static void AlignTop(IReadOnlyList<NodeViewModel> nodes)
    {
        var top = nodes.Min(node => node.Y);
        foreach (var node in nodes)
        {
            node.Y = top;
        }
    }

    public static void AlignMiddle(IReadOnlyList<NodeViewModel> nodes)
    {
        var middle = GetVerticalCenter(nodes);
        foreach (var node in nodes)
        {
            node.Y = middle - (node.Height / 2);
        }
    }

    public static void AlignBottom(IReadOnlyList<NodeViewModel> nodes)
    {
        var bottom = nodes.Max(node => node.Y + node.Height);
        foreach (var node in nodes)
        {
            node.Y = bottom - node.Height;
        }
    }

    public static void DistributeHorizontally(IReadOnlyList<NodeViewModel> nodes)
    {
        // 保留当前顺序，只在既有跨度内重新安置内部节点。
        var orderedNodes = nodes
            .OrderBy(node => node.X + (node.Width / 2))
            .ToList();

        var firstCenter = orderedNodes[0].X + (orderedNodes[0].Width / 2);
        var lastCenter = orderedNodes[^1].X + (orderedNodes[^1].Width / 2);
        var step = (lastCenter - firstCenter) / (orderedNodes.Count - 1);

        for (var index = 1; index < orderedNodes.Count - 1; index++)
        {
            var node = orderedNodes[index];
            var center = firstCenter + (step * index);
            node.X = center - (node.Width / 2);
        }
    }

    public static void DistributeVertically(IReadOnlyList<NodeViewModel> nodes)
    {
        // 保留当前顺序，只在既有跨度内重新安置内部节点。
        var orderedNodes = nodes
            .OrderBy(node => node.Y + (node.Height / 2))
            .ToList();

        var firstCenter = orderedNodes[0].Y + (orderedNodes[0].Height / 2);
        var lastCenter = orderedNodes[^1].Y + (orderedNodes[^1].Height / 2);
        var step = (lastCenter - firstCenter) / (orderedNodes.Count - 1);

        for (var index = 1; index < orderedNodes.Count - 1; index++)
        {
            var node = orderedNodes[index];
            var center = firstCenter + (step * index);
            node.Y = center - (node.Height / 2);
        }
    }

    private static double GetHorizontalCenter(IReadOnlyList<NodeViewModel> nodes)
    {
        var left = nodes.Min(node => node.X);
        var right = nodes.Max(node => node.X + node.Width);
        return (left + right) / 2;
    }

    private static double GetVerticalCenter(IReadOnlyList<NodeViewModel> nodes)
    {
        var top = nodes.Min(node => node.Y);
        var bottom = nodes.Max(node => node.Y + node.Height);
        return (top + bottom) / 2;
    }
}
