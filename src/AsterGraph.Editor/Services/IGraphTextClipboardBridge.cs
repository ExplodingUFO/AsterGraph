using System.Threading;
using System.Threading.Tasks;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 由宿主提供的纯文本剪贴板桥，编辑器通过它读写系统剪贴板而不依赖具体 UI 框架。
/// </summary>
public interface IGraphTextClipboardBridge
{
    /// <summary>
    /// 读取当前剪贴板中的文本内容。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>剪贴板文本；如果不可用则返回 <see langword="null"/>。</returns>
    Task<string?> ReadTextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 将指定文本写入剪贴板。
    /// </summary>
    /// <param name="text">要写入的文本。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task WriteTextAsync(string text, CancellationToken cancellationToken = default);
}
