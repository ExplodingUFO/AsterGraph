using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可调用的图编辑器命令入口。
/// </summary>
public interface IGraphEditorCommands
{
    /// <summary>
    /// 撤销最近一次可撤销操作。
    /// </summary>
    void Undo();

    /// <summary>
    /// 重做最近一次已撤销操作。
    /// </summary>
    void Redo();

    /// <summary>
    /// 清空当前选择。
    /// </summary>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void ClearSelection(bool updateStatus = false);

    /// <summary>
    /// 基于节点定义标识添加一个节点实例。
    /// </summary>
    /// <param name="definitionId">节点定义标识。</param>
    /// <param name="preferredWorldPosition">可选的世界坐标位置。</param>
    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null);

    /// <summary>
    /// 删除当前选择。
    /// </summary>
    void DeleteSelection();

    /// <summary>
    /// 平移视口。
    /// </summary>
    /// <param name="deltaX">水平位移。</param>
    /// <param name="deltaY">垂直位移。</param>
    void PanBy(double deltaX, double deltaY);

    /// <summary>
    /// 以屏幕锚点为中心缩放视口。
    /// </summary>
    /// <param name="factor">缩放因子。</param>
    /// <param name="screenAnchor">屏幕锚点。</param>
    void ZoomAt(double factor, GraphPoint screenAnchor);

    /// <summary>
    /// 重置视口。
    /// </summary>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void ResetView(bool updateStatus = true);

    /// <summary>
    /// 保存当前工作区。
    /// </summary>
    void SaveWorkspace();

    /// <summary>
    /// 加载当前工作区。
    /// </summary>
    /// <returns>加载成功时返回 <see langword="true"/>。</returns>
    bool LoadWorkspace();
}
