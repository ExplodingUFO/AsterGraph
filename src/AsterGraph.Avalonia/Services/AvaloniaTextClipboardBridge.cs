using Avalonia.Input.Platform;
using AsterGraph.Editor.Services;

namespace AsterGraph.Avalonia.Services;

/// <summary>
/// Avalonia 平台的纯文本剪贴板桥实现。
/// </summary>
internal sealed class AvaloniaTextClipboardBridge : IGraphTextClipboardBridge
{
    private readonly Func<IClipboard?> _clipboardAccessor;

    public AvaloniaTextClipboardBridge(Func<IClipboard?> clipboardAccessor)
    {
        _clipboardAccessor = clipboardAccessor ?? throw new ArgumentNullException(nameof(clipboardAccessor));
    }

    public async Task<string?> ReadTextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var clipboard = _clipboardAccessor();
        return clipboard is null ? null : await clipboard.TryGetTextAsync();
    }

    public async Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        cancellationToken.ThrowIfCancellationRequested();
        var clipboard = _clipboardAccessor();
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
