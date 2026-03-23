using CommunityToolkit.Mvvm.Input;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 集中处理编辑器命令与计算属性的通知扇出，作为视图模型与通知机制之间的内部接缝。
/// </summary>
internal sealed class GraphEditorCommandStateNotifier
{
    /// <summary>
    /// 触发一组命令的 <c>CanExecute</c> 重新计算。
    /// </summary>
    public void NotifyCanExecuteChanged(IEnumerable<IRelayCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        foreach (var command in commands)
        {
            command.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// 触发一组属性的变更通知。
    /// </summary>
    public void NotifyPropertyChanged(Action<string> notifyPropertyChanged, IEnumerable<string> propertyNames)
    {
        ArgumentNullException.ThrowIfNull(notifyPropertyChanged);
        ArgumentNullException.ThrowIfNull(propertyNames);

        foreach (var propertyName in propertyNames)
        {
            notifyPropertyChanged(propertyName);
        }
    }
}
