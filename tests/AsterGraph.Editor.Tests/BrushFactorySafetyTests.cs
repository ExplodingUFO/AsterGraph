using AsterGraph.Avalonia.Styling;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class BrushFactorySafetyTests
{
    [Fact]
    public void SolidSafe_UsesFallbackWhenAccentHexIsInvalid()
    {
        var brush = BrushFactory.SolidSafe("invalid-accent", "#2F81F7", opacity: 0.5);
        var expected = BrushFactory.Color("#2F81F7", opacity: 0.5);

        Assert.Equal(expected, brush.Color);
    }
}
