using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookNavigationPolishTests
{
    [Fact]
    public void CookbookNavigationFeedback_ReportsEmptyFilterWithoutClearingSelection()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectedCookbookCategoryFilter = viewModel.CookbookCategoryFilters.Single(filter =>
            filter.Category == DemoCookbookRecipeCategory.DiagnosticsSupport);
        var selected = viewModel.SelectedCookbookRecipe;
        viewModel.CookbookSearchText = "allowlist";

        Assert.Empty(viewModel.FilteredCookbookRecipes);
        Assert.Same(selected, viewModel.SelectedCookbookRecipe);
        Assert.Equal(selected.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.False(viewModel.HasCookbookFilterResults);
        Assert.True(viewModel.HasCookbookEmptyFilterFeedback);
        Assert.Equal("allowlist", viewModel.CookbookSearchText);
        Assert.Contains(selected.Title, viewModel.CookbookFilterFeedback, StringComparison.Ordinal);
    }

    [Fact]
    public void CookbookNavigationSearchIncludesScenarioDepth()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.CookbookSearchText = "RuntimeCommandTimelineEntry";

        var recipe = Assert.Single(viewModel.FilteredCookbookRecipes);
        Assert.Equal("diagnostics-support-route", recipe.Id);
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.Contains(viewModel.CookbookWorkspace.SelectedRecipe.ScenarioPoints, point =>
            string.Equals(point.Evidence, "RuntimeCommandTimelineEntry", StringComparison.Ordinal));
    }

    [AvaloniaFact]
    public void MainWindow_CookbookEmptyFilterKeepsGraphAndShowsNavigationFeedback()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        viewModel.OpenHostMenuGroupCommand.Execute("cookbook");
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var graphHost = Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost"));
        var graphEditorView = Assert.IsType<GraphEditorView>(graphHost.Content);
        Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("PART_CookbookWorkspaceNavigationFeedbackLines"));
        var emptyFeedback = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceEmptyFilterFeedback"));

        viewModel.SelectedCookbookCategoryFilter = viewModel.CookbookCategoryFilters.Single(filter =>
            filter.Category == DemoCookbookRecipeCategory.DiagnosticsSupport);
        var selected = viewModel.SelectedCookbookRecipe;
        viewModel.CookbookSearchText = "allowlist";
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.True(emptyFeedback.IsVisible);
        Assert.Contains(
            viewModel.CookbookLandingLines,
            line => line.Contains("筛选结果", StringComparison.Ordinal)
                    && line.EndsWith("0", StringComparison.Ordinal));
        Assert.Same(selected, viewModel.SelectedCookbookRecipe);
        Assert.Empty(viewModel.CookbookWorkspace.NavigationGroups);
        Assert.Empty(FindCookbookRecipeButtons(window, selected.Id));
        Assert.Same(graphEditorView, graphHost.Content);
    }

    private static Button[] FindCookbookRecipeButtons(MainWindow window, string recipeId)
        => window.GetVisualDescendants()
            .OfType<Button>()
            .Where(button =>
                button.CommandParameter is DemoCookbookWorkspaceNavigationItem item
                && string.Equals(item.RecipeId, recipeId, StringComparison.Ordinal))
            .ToArray();
}
