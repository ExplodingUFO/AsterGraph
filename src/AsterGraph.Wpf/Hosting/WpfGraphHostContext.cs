using AsterGraph.Editor.Hosting;
using System.Windows;

namespace AsterGraph.Wpf.Hosting;

internal sealed record WpfGraphHostContext(object Owner, object? TopLevel, IServiceProvider? Services) : IGraphHostContext;
