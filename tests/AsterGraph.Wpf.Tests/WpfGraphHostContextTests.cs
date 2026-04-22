using AsterGraph.Editor.Hosting;
using Xunit;

namespace AsterGraph.Wpf.Tests;

public sealed class WpfGraphHostContextTests
{
    [Fact]
    public void WpfGraphHostContext_StoresOwnerAndTopLevel()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var owner = new object();
        var topLevel = new object();

        var hostContext = WpfRouteTestHelpers.CreateWpfHostContext(owner, topLevel);

        Assert.Same(owner, hostContext.Owner);
        Assert.Same(topLevel, hostContext.TopLevel);
        Assert.Null(hostContext.Services);
    }
}
