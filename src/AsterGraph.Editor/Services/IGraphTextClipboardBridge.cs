using System.Threading;
using System.Threading.Tasks;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Host-provided bridge for exchanging plain text with the system clipboard.
/// </summary>
public interface IGraphTextClipboardBridge
{
    Task<string?> ReadTextAsync(CancellationToken cancellationToken = default);

    Task WriteTextAsync(string text, CancellationToken cancellationToken = default);
}
