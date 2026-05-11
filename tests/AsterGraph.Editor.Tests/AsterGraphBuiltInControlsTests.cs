using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AsterGraphBuiltInControlsTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.controls.node");
    private const string NodeId = "tests.controls.node-001";

    [Fact]
    public void AsterGraphControlsType_IsPublicControlWithSessionProperty()
    {
        Assert.True(typeof(AsterGraphControls).IsPublic);
        Assert.True(typeof(Control).IsAssignableFrom(typeof(AsterGraphControls)));

        Assert.NotNull(typeof(AsterGraphControls).GetProperty(nameof(AsterGraphControls.Session)));
    }

    [AvaloniaFact]
    public void AsterGraphControls_RendersViewportActionsAndExecutesCanonicalCommands()
    {
        var session = CreateSession();
        session.Commands.UpdateViewportSize(640d, 420d);
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);
        var controls = new AsterGraphControls
        {
            Session = session,
        };
        var window = Show(controls);
        try
        {
            var zoomIn = FindButton(controls, "Zoom In");
            var zoomOut = FindButton(controls, "Zoom Out");
            var fitView = FindButton(controls, "Fit View");
            var resetView = FindButton(controls, "Reset View");
            var buttons = new[] { zoomIn, zoomOut, fitView, resetView };

            Assert.False(controls.Focusable);
            Assert.Equal("PART_AsterGraphControlsZoomInButton", zoomIn.Name);
            Assert.Equal("PART_AsterGraphControlsZoomOutButton", zoomOut.Name);
            Assert.Equal("PART_AsterGraphControlsFitViewButton", fitView.Name);
            Assert.Equal("PART_AsterGraphControlsResetViewButton", resetView.Name);
            Assert.All(buttons, button =>
            {
                Assert.True(button.Focusable);
                Assert.True(button.IsEnabled);
            });

            zoomIn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            zoomOut.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            fitView.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            resetView.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(
                ["viewport.zoom-in", "viewport.zoom-out", "viewport.fit", "viewport.reset"],
                executedCommandIds);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AsterGraphControls_DisabledCommandDescriptorsExposeTooltipRecoveryText()
    {
        var controls = new AsterGraphControls
        {
            Session = CreateSession(),
        };
        var window = Show(controls);
        try
        {
            var fitView = FindButton(controls, "Fit View");

            Assert.False(fitView.IsEnabled);
            Assert.Equal(
                "Viewport size is not available yet.\nWait for viewport to initialize.",
                ToolTip.GetTip(fitView));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Catalog_ExposesControlsAsStandalonePublicBuiltIn()
    {
        Assert.True(AsterGraphBuiltInComponentCatalog.TryGet(AsterGraphBuiltInComponentCatalog.Controls, out var descriptor));
        Assert.NotNull(descriptor);
        Assert.Equal(AsterGraphBuiltInComponentStatus.Public, descriptor.Status);
        Assert.Equal("AsterGraph.Avalonia.Controls.AsterGraphControls", descriptor.SurfaceTypeName);
        Assert.Equal("AsterGraphControls", descriptor.EntryPoint);
    }

    [Fact]
    public void PublicApiDocs_DescribeStandaloneControlsSurface()
    {
        var englishInventory = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseInventory = ReadRepoFile("docs/zh-CN/public-api-inventory.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");

        foreach (var contents in new[] { englishInventory, chineseInventory, avaloniaReadme })
        {
            Assert.Contains("AsterGraphControls", contents, StringComparison.Ordinal);
            Assert.Contains("controls", contents, StringComparison.Ordinal);
        }
    }

    private static Window Show(Control content)
    {
        var window = new Window
        {
            Width = 320,
            Height = 160,
            Content = content,
        };
        window.Show();
        return window;
    }

    private static Button FindButton(Control root, string automationName)
        => root.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => string.Equals(
                AutomationProperties.GetName(button),
                automationName,
                StringComparison.Ordinal));

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateDocument()
        => new(
            "Controls Graph",
            "Standalone controls contract coverage.",
            [
                new GraphNode(
                    NodeId,
                    "Controls Node",
                    "Tests",
                    "Controls",
                    "Source node for controls tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [],
                    "#6AD5C4",
                    DefinitionId),
            ],
            []);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Controls Node",
            "Tests",
            "Controls",
            [],
            []));
        return catalog;
    }

    private static string ReadRepoFile(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../", relativePath));
        return File.ReadAllText(fullPath);
    }
}
