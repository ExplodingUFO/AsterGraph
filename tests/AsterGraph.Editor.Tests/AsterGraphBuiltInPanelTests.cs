using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AsterGraphBuiltInPanelTests
{
    [Fact]
    public void AsterGraphPanelType_IsPublicContentControlWithPositioningProperties()
    {
        Assert.True(typeof(AsterGraphPanel).IsPublic);
        Assert.True(typeof(ContentControl).IsAssignableFrom(typeof(AsterGraphPanel)));

        Assert.NotNull(typeof(AsterGraphPanel).GetProperty(nameof(AsterGraphPanel.Position)));
        Assert.NotNull(typeof(AsterGraphPanel).GetProperty(nameof(AsterGraphPanel.Offset)));
        Assert.NotNull(typeof(AsterGraphPanel).GetProperty(nameof(AsterGraphPanel.Padding)));
        Assert.NotNull(typeof(AsterGraphPanel).GetProperty(nameof(AsterGraphPanel.CornerRadius)));
    }

    [AvaloniaFact]
    public void AsterGraphPanel_ArrangesContentAtRequestedOverlayPosition()
    {
        var content = new Border
        {
            Width = 120d,
            Height = 48d,
            Background = Brushes.CadetBlue,
        };
        var panel = new AsterGraphPanel
        {
            Position = AsterGraphPanelPosition.BottomRight,
            Offset = new Thickness(20d, 16d),
            Padding = new Thickness(10d),
            Content = content,
        };
        var window = Show(panel);
        try
        {
            panel.Measure(new Size(320d, 200d));
            panel.Arrange(new Rect(0d, 0d, 320d, 200d));

            Assert.Equal(HorizontalAlignment.Stretch, panel.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Stretch, panel.VerticalAlignment);
            Assert.Equal(new Rect(170d, 126d, 120d, 48d), content.Bounds);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AsterGraphPanel_DefaultsToNonFocusableOverlayContainerAndPreservesFocusableContent()
    {
        var action = new Button
        {
            Content = "Overlay action",
        };
        AutomationProperties.SetName(action, "Overlay action");
        var panel = new AsterGraphPanel
        {
            Content = action,
        };
        var window = Show(panel);
        try
        {
            Assert.False(panel.Focusable);
            Assert.True(action.Focusable);
            Assert.Equal("Overlay action", AutomationProperties.GetName(action));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Catalog_ExposesPanelAsStandalonePublicBuiltIn()
    {
        Assert.True(AsterGraphBuiltInComponentCatalog.TryGet(AsterGraphBuiltInComponentCatalog.Panel, out var descriptor));
        Assert.NotNull(descriptor);
        Assert.Equal(AsterGraphBuiltInComponentStatus.Public, descriptor.Status);
        Assert.Equal("AsterGraph.Avalonia.Controls.AsterGraphPanel", descriptor.SurfaceTypeName);
        Assert.Equal("AsterGraphPanel", descriptor.EntryPoint);
    }

    [Fact]
    public void PublicApiDocs_DescribeStandalonePanelSurface()
    {
        var englishInventory = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseInventory = ReadRepoFile("docs/zh-CN/public-api-inventory.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");

        foreach (var contents in new[] { englishInventory, chineseInventory, avaloniaReadme })
        {
            Assert.Contains("AsterGraphPanel", contents, StringComparison.Ordinal);
            Assert.Contains("panel", contents, StringComparison.Ordinal);
        }
    }

    private static Window Show(Control content)
    {
        var window = new Window
        {
            Width = 320d,
            Height = 200d,
            Content = content,
        };
        window.Show();
        return window;
    }

    private static string ReadRepoFile(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../", relativePath));
        return File.ReadAllText(fullPath);
    }
}
