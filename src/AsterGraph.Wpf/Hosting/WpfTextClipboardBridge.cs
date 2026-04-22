using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AsterGraph.Editor.Services;

namespace AsterGraph.Wpf.Hosting;

internal sealed class WpfTextClipboardBridge : IGraphTextClipboardBridge
{
    private readonly Func<Dispatcher?> _dispatcher;

    public WpfTextClipboardBridge(Func<Dispatcher?> dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task<string?> ReadTextAsync(CancellationToken cancellationToken = default)
        => InvokeOnDispatcherAsync(() => Clipboard.ContainsText() ? Clipboard.GetText() : null, cancellationToken);

    public Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
        => InvokeOnDispatcherAsync(() => Clipboard.SetText(text ?? string.Empty), cancellationToken);

    private static void ThrowIfCancellationRequested(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
    }

    private Task<T?> InvokeOnDispatcherAsync<T>(Func<T?> action, CancellationToken cancellationToken)
        where T : class
    {
        ThrowIfCancellationRequested(cancellationToken);
        var dispatcher = _dispatcher();
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            return Task.FromResult<T?>(action());
        }

        return dispatcher
            .InvokeAsync(() => action(), DispatcherPriority.Normal, cancellationToken)
            .Task
            .ContinueWith(
                static task => (T?)task.Result,
                TaskScheduler.Default);
    }

    private Task InvokeOnDispatcherAsync(Action action, CancellationToken cancellationToken)
    {
        ThrowIfCancellationRequested(cancellationToken);
        var dispatcher = _dispatcher();
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
    }
}
