using System.Threading;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorFragmentClipboardCommands
{
    private readonly GraphEditorViewModel.IGraphEditorFragmentCommandHost _host;
    private readonly GraphEditorFragmentTransferSupport _transferSupport;

    public GraphEditorFragmentClipboardCommands(
        GraphEditorViewModel.IGraphEditorFragmentCommandHost host,
        GraphEditorFragmentTransferSupport transferSupport)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _transferSupport = transferSupport ?? throw new ArgumentNullException(nameof(transferSupport));
    }

    public async Task CopySelectionAsync()
    {
        if (!_host.CommandPermissions.Clipboard.AllowCopy)
        {
            _host.SetStatus("editor.status.clipboard.copy.disabledByPermissions", "Copy is disabled by host permissions.");
            return;
        }

        var fragment = _transferSupport.CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("editor.status.clipboard.copy.selectNodeFirst", "Select at least one node before copying.");
            return;
        }

        _host.StoreSelectionClipboard(fragment);
        var clipboardJson = _host.ClipboardPayloadSerializer.Serialize(fragment);
        if (_host.TextClipboardBridge is not null)
        {
            await _host.TextClipboardBridge.WriteTextAsync(clipboardJson, CancellationToken.None);
        }

        _host.RaiseComputedPropertyChanges();
        if (fragment.Nodes.Count == 1)
        {
            _host.SetStatus("editor.status.clipboard.copy.single", "Copied {0}.", fragment.Nodes[0].Title);
        }
        else
        {
            _host.SetStatus("editor.status.clipboard.copy.multiple", "Copied {0} nodes.", fragment.Nodes.Count);
        }
    }

    public async Task PasteSelectionAsync()
    {
        if (!_host.CommandPermissions.Clipboard.AllowPaste)
        {
            _host.SetStatus("editor.status.clipboard.paste.disabledByPermissions", "Paste is disabled by host permissions.");
            return;
        }

        var fragment = await GetBestAvailableClipboardFragmentAsync();
        if (fragment is null || fragment.Nodes.Count == 0)
        {
            _host.SetStatus("editor.status.clipboard.paste.nothingCopied", "Nothing copied yet.");
            return;
        }

        _ = _transferSupport.PasteFragment(fragment, "Pasted");
    }

    public async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync()
    {
        if (_host.TextClipboardBridge is not null)
        {
            var clipboardText = await _host.TextClipboardBridge.ReadTextAsync(CancellationToken.None);
            // 优先读取系统剪贴板 JSON，但仍保留进程内剪贴板作为可靠回退。
            if (_host.ClipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
            {
                return systemFragment;
            }
        }

        return _host.PeekSelectionClipboard();
    }
}
