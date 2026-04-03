using AsterGraph.Avalonia.Styling;
using Avalonia.Headless.XUnit;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class BrushFactorySafetyTests
{
    [AvaloniaFact]
    public void SolidSafe_UsesFallbackWhenAccentHexIsInvalid()
    {
        var brush = BrushFactory.SolidSafe("invalid-accent", "#2F81F7", opacity: 0.5);
        var expected = BrushFactory.Color("#2F81F7", opacity: 0.5);

        Assert.Equal(expected, brush.Color);
    }
}
