using System.Linq;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

internal sealed class GraphInspectorParameterGroupState
{
    public GraphInspectorParameterGroupState(
        string groupName,
        IReadOnlyList<NodeParameterViewModel> parameters,
        bool isCollapsed)
    {
        GroupName = groupName;
        Parameters = parameters;
        IsCollapsed = isCollapsed;
        ValidationIssueCount = parameters.Count(parameter => parameter.HasValidationError);
    }

    public string GroupName { get; }

    public IReadOnlyList<NodeParameterViewModel> Parameters { get; }

    public bool IsCollapsed { get; }

    public bool IsExpanded => !IsCollapsed;

    public bool HasValidationIssues => ValidationIssueCount > 0;

    public string ToggleCaption => IsCollapsed ? "展开" : "收起";

    public string CountCaption => $"{Parameters.Count} 个参数";

    public int ValidationIssueCount { get; }

    public string ValidationCaption => $"{ValidationIssueCount} 个问题";
}
