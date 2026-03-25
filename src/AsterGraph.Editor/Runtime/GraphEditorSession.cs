using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现。
/// </summary>
public sealed class GraphEditorSession : IGraphEditorSession, IGraphEditorCommands, IGraphEditorQueries, IGraphEditorEvents
{
    private readonly GraphEditorViewModel _editor;

    /// <summary>
    /// 初始化运行时会话。
    /// </summary>
    /// <param name="editor">底层兼容立面。</param>
    public GraphEditorSession(GraphEditorViewModel editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    /// <inheritdoc />
    public IGraphEditorCommands Commands => this;

    /// <inheritdoc />
    public IGraphEditorQueries Queries => this;

    /// <inheritdoc />
    public IGraphEditorEvents Events => this;

    /// <inheritdoc />
    public IGraphEditorMutationScope BeginMutation(string? label = null)
        => new GraphEditorMutationScope(label);

    /// <inheritdoc />
    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged
    {
        add => _editor.DocumentChanged += value;
        remove => _editor.DocumentChanged -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged
    {
        add => _editor.SelectionChanged += value;
        remove => _editor.SelectionChanged -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged
    {
        add => _editor.ViewportChanged += value;
        remove => _editor.ViewportChanged -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported
    {
        add => _editor.FragmentExported += value;
        remove => _editor.FragmentExported -= value;
    }

    /// <inheritdoc />
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported
    {
        add => _editor.FragmentImported += value;
        remove => _editor.FragmentImported -= value;
    }

    /// <inheritdoc />
    public void Undo()
        => _editor.Undo();

    /// <inheritdoc />
    public void Redo()
        => _editor.Redo();

    /// <inheritdoc />
    public void ClearSelection(bool updateStatus = false)
        => _editor.ClearSelection(updateStatus);

    /// <inheritdoc />
    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);

        var template = _editor.NodeTemplates.FirstOrDefault(candidate => candidate.Definition.Id == definitionId)
            ?? throw new InvalidOperationException($"Node definition '{definitionId}' is not registered in the current editor catalog.");

        _editor.AddNode(template, preferredWorldPosition);
    }

    /// <inheritdoc />
    public void DeleteSelection()
        => _editor.DeleteSelection();

    /// <inheritdoc />
    public void PanBy(double deltaX, double deltaY)
        => _editor.PanBy(deltaX, deltaY);

    /// <inheritdoc />
    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => _editor.ZoomAt(factor, screenAnchor);

    /// <inheritdoc />
    public void ResetView(bool updateStatus = true)
        => _editor.ResetView(updateStatus);

    /// <inheritdoc />
    public void SaveWorkspace()
        => _editor.SaveWorkspace();

    /// <inheritdoc />
    public bool LoadWorkspace()
        => _editor.LoadWorkspace();

    /// <inheritdoc />
    public GraphDocument CreateDocumentSnapshot()
        => _editor.CreateDocumentSnapshot();

    /// <inheritdoc />
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _editor.GetNodePositions();

    /// <inheritdoc />
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _editor.GetCompatibleTargets(sourceNodeId, sourcePortId);

    private sealed class GraphEditorMutationScope : IGraphEditorMutationScope
    {
        public GraphEditorMutationScope(string? label)
        {
            Label = label;
        }

        public string? Label { get; }

        public void Dispose()
        {
        }
    }
}
