using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Presentation;

/// <summary>
/// 描述一次节点展示状态计算请求。
/// </summary>
public sealed record NodePresentationContext
{
    /// <summary>
    /// 初始化节点展示上下文。
    /// </summary>
    public NodePresentationContext(
        IGraphEditorSession session,
        string nodeId,
        NodeDefinitionId? definitionId,
        string title,
        string category,
        string subtitle,
        string description,
        string accentHex,
        bool isSelected,
        int inputCount,
        int outputCount,
        IReadOnlyDictionary<string, GraphParameterValue> parameterValues,
        NodeViewModel compatibilityNode)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        NodeId = nodeId ?? throw new ArgumentNullException(nameof(nodeId));
        DefinitionId = definitionId;
        Title = title ?? string.Empty;
        Category = category ?? string.Empty;
        Subtitle = subtitle ?? string.Empty;
        Description = description ?? string.Empty;
        AccentHex = accentHex ?? string.Empty;
        IsSelected = isSelected;
        InputCount = inputCount;
        OutputCount = outputCount;
        ParameterValues = parameterValues ?? throw new ArgumentNullException(nameof(parameterValues));
        CompatibilityNode = compatibilityNode ?? throw new ArgumentNullException(nameof(compatibilityNode));
    }

    public IGraphEditorSession Session { get; }
    public string NodeId { get; }
    public NodeDefinitionId? DefinitionId { get; }
    public string Title { get; }
    public string Category { get; }
    public string Subtitle { get; }
    public string Description { get; }
    public string AccentHex { get; }
    public bool IsSelected { get; }
    public int InputCount { get; }
    public int OutputCount { get; }
    public IReadOnlyDictionary<string, GraphParameterValue> ParameterValues { get; }

    /// <remarks>
    /// New implementations should prefer the stable snapshot properties and <see cref="Session"/>.
    /// This property only exists to adapt legacy providers during the migration window.
    /// </remarks>
    public NodeViewModel CompatibilityNode { get; }
}
