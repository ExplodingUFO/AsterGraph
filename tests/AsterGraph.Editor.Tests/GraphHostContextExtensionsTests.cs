using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphHostContextExtensionsTests
{
    [Fact]
    public void TryGetOwner_ReturnsTypedOwnerWhenTypesMatch()
    {
        var owner = new SampleOwner("owner-001");
        IGraphHostContext hostContext = new SampleHostContext(owner, null);

        var matched = hostContext.TryGetOwner<SampleOwner>(out var typedOwner);

        Assert.True(matched);
        Assert.Same(owner, typedOwner);
    }

    [Fact]
    public void TryGetOwner_ReturnsFalseWhenTypesDoNotMatch()
    {
        IGraphHostContext hostContext = new SampleHostContext(new SampleOwner("owner-001"), null);

        var matched = hostContext.TryGetOwner<string>(out var typedOwner);

        Assert.False(matched);
        Assert.Null(typedOwner);
    }

    [Fact]
    public void TryGetTopLevel_ReturnsTypedTopLevelWhenTypesMatch()
    {
        var topLevel = new SampleTopLevel("shell-001");
        IGraphHostContext hostContext = new SampleHostContext(new SampleOwner("owner-001"), topLevel);

        var matched = hostContext.TryGetTopLevel<SampleTopLevel>(out var typedTopLevel);

        Assert.True(matched);
        Assert.Same(topLevel, typedTopLevel);
    }

    [Fact]
    public void ContextMenuContextHelpers_DelegateThroughHostContext()
    {
        var owner = new SampleOwner("owner-001");
        var topLevel = new SampleTopLevel("shell-001");
        var context = new ContextMenuContext(
            ContextMenuTargetKind.Node,
            new GraphPoint(32, 64),
            hostContext: new SampleHostContext(owner, topLevel));

        var ownerMatched = context.TryGetOwner<SampleOwner>(out var typedOwner);
        var topLevelMatched = context.TryGetTopLevel<SampleTopLevel>(out var typedTopLevel);

        Assert.True(ownerMatched);
        Assert.True(topLevelMatched);
        Assert.Same(owner, typedOwner);
        Assert.Same(topLevel, typedTopLevel);
    }

    [Fact]
    public void NullHostContextPaths_FailSafely()
    {
        IGraphHostContext? hostContext = null;
        var context = new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0));

        var ownerMatched = hostContext.TryGetOwner<SampleOwner>(out var typedOwner);
        var topLevelMatched = hostContext.TryGetTopLevel<SampleTopLevel>(out var typedTopLevel);
        var contextOwnerMatched = context.TryGetOwner<SampleOwner>(out var contextOwner);
        var contextTopLevelMatched = context.TryGetTopLevel<SampleTopLevel>(out var contextTopLevel);

        Assert.False(ownerMatched);
        Assert.False(topLevelMatched);
        Assert.False(contextOwnerMatched);
        Assert.False(contextTopLevelMatched);
        Assert.Null(typedOwner);
        Assert.Null(typedTopLevel);
        Assert.Null(contextOwner);
        Assert.Null(contextTopLevel);
    }

    [Fact]
    public void NullContextMenuContextPaths_FailSafely()
    {
        ContextMenuContext? context = null;

        var ownerMatched = context.TryGetOwner<SampleOwner>(out var owner);
        var topLevelMatched = context.TryGetTopLevel<SampleTopLevel>(out var topLevel);

        Assert.False(ownerMatched);
        Assert.False(topLevelMatched);
        Assert.Null(owner);
        Assert.Null(topLevel);
    }

    private sealed record SampleOwner(string Id);

    private sealed record SampleTopLevel(string Id);

    private sealed record SampleHostContext(object Owner, object? TopLevel) : IGraphHostContext
    {
        public IServiceProvider? Services => null;
    }
}
