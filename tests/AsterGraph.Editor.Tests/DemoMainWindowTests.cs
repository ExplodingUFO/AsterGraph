using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class DemoMainWindowTests
{
    [AvaloniaFact]
    public void MainWindow_RendersLockedThreeColumnShellContract()
    {
        var window = CreateWindow();

        Assert.Equal("能力导航", FindText(window, "能力导航").Text);
        Assert.Equal("完整壳层演示", FindText(window, "完整壳层演示").Text);
        Assert.Equal("架构说明", FindText(window, "架构说明").Text);
        Assert.Equal("运行时与诊断", FindText(window, "运行时与诊断").Text);

        var capabilityNames = new[] { "完整壳层", "独立表面", "可替换呈现", "运行时与诊断" };
        foreach (var capabilityName in capabilityNames)
        {
            Assert.Contains(GetAllText(window), value => value == capabilityName);
        }

        Assert.Single(window.GetVisualDescendants().OfType<GraphEditorView>());

        var layoutGrid = window.FindControl<Grid>("MainShellGrid");
        Assert.NotNull(layoutGrid);
        Assert.Equal(new Thickness(24), layoutGrid!.Margin);
        Assert.Equal(24, layoutGrid.ColumnSpacing);
        Assert.Equal(3, layoutGrid.ColumnDefinitions.Count);
        Assert.Equal(new GridLength(280, GridUnitType.Pixel), layoutGrid.ColumnDefinitions[0].Width);
        Assert.Equal(GridLength.Star, layoutGrid.ColumnDefinitions[1].Width);
        Assert.Equal(new GridLength(360, GridUnitType.Pixel), layoutGrid.ColumnDefinitions[2].Width);
    }

    [AvaloniaFact]
    public void MainWindow_UsesSquaredLowRadiusShellCards()
    {
        var window = CreateWindow();

        var leftScrollViewer = window.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .First(scrollViewer => Grid.GetColumn(scrollViewer) == 0);
        var leftStack = Assert.IsType<StackPanel>(leftScrollViewer.Content);
        var titleCard = Assert.IsType<Border>(leftStack.Children[0]);
        var toggleCard = Assert.IsType<Border>(leftStack.Children[2]);

        Assert.Equal(new CornerRadius(10), titleCard.CornerRadius);
        Assert.Equal(new CornerRadius(8), toggleCard.CornerRadius);

        var centerCard = window.FindControl<Border>("MainEditorCard");
        Assert.NotNull(centerCard);
        Assert.Equal(new CornerRadius(10), centerCard!.CornerRadius);
    }

    [AvaloniaFact]
    public void MainWindow_UsesFullHeightCenterEditorComposition()
    {
        var window = CreateWindow();

        var centerGrid = window.GetVisualDescendants()
            .OfType<Grid>()
            .First(grid => Grid.GetColumn(grid) == 1 && grid.RowDefinitions.Count == 3);

        Assert.Equal(new GridLength(1, GridUnitType.Star), centerGrid.RowDefinitions[1].Height);

        var editorFrame = window.FindControl<Border>("MainEditorFrame");
        Assert.NotNull(editorFrame);
        Assert.True(double.IsNaN(editorFrame!.Height));
        Assert.Equal(0, editorFrame.MinHeight);

        var graphEditorView = window.FindControl<GraphEditorView>("MainGraphEditorView");
        Assert.NotNull(graphEditorView);
        Assert.Equal(VerticalAlignment.Stretch, graphEditorView!.VerticalAlignment);
    }

    private static MainWindow CreateWindow()
    {
        var window = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };

        window.Show();
        return window;
    }

    private static TextBlock FindText(Control root, string text)
        => root.GetVisualDescendants()
            .OfType<TextBlock>()
            .FirstOrDefault(textBlock => textBlock.Text == text)
           ?? throw new Xunit.Sdk.XunitException($"Could not find text '{text}'.");

    private static string[] GetAllText(Control root)
        => root.GetVisualDescendants()
            .OfType<TextBlock>()
            .Select(textBlock => textBlock.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Cast<string>()
            .ToArray();
}
