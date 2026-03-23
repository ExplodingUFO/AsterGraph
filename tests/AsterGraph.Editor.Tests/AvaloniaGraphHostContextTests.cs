using AsterGraph.Avalonia.Hosting;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AvaloniaGraphHostContextTests
{
    [Fact]
    public void Constructor_PreservesNullTopLevel()
    {
        var owner = new object();

        var hostContext = new AvaloniaGraphHostContext(owner, TopLevel: null);

        Assert.Same(owner, hostContext.Owner);
        Assert.Null(hostContext.TopLevel);
        Assert.Null(hostContext.Services);
    }
}
