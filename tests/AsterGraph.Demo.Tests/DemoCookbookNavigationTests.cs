using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookNavigationTests
{
    [Fact]
    public void MainWindowViewModel_CookbookDefaultsToCatalogLanding()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal(DemoCookbookCatalog.Recipes.Count, viewModel.CookbookRecipes.Count);
        Assert.Equal(DemoCookbookCatalog.Recipes.Count, viewModel.FilteredCookbookRecipes.Count);
        Assert.Equal(DemoCookbookCatalog.RequiredCategories.Count + 1, viewModel.CookbookCategoryFilters.Count);
        Assert.Same(viewModel.CookbookRecipes[0], viewModel.SelectedCookbookRecipe);
        Assert.Contains(
            viewModel.SelectedCookbookRecipeProofLines,
            line => line.Contains(viewModel.SelectedCookbookRecipe.ProofMarkers[0], StringComparison.Ordinal));
        Assert.Contains(
            viewModel.SelectedCookbookRecipe.SupportBoundary,
            viewModel.SelectedCookbookRecipeSupportBoundary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowViewModel_CookbookFilterUsesSearchAndCategory()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.CookbookSearchText = "allowlist";

        var searchMatch = Assert.Single(viewModel.FilteredCookbookRecipes);
        Assert.Equal("plugin-trust-route", searchMatch.Id);

        viewModel.SelectedCookbookCategoryFilter = viewModel.CookbookCategoryFilters.Single(filter =>
            filter.Category == DemoCookbookRecipeCategory.DiagnosticsSupport);

        Assert.Empty(viewModel.FilteredCookbookRecipes);

        viewModel.CookbookSearchText = "support";

        var categoryMatch = Assert.Single(viewModel.FilteredCookbookRecipes);
        Assert.Equal("diagnostics-support-route", categoryMatch.Id);
        Assert.Same(categoryMatch, viewModel.SelectedCookbookRecipe);
    }

    [Fact]
    public void MainWindowViewModel_OpenSelectedCookbookRecipeLandsOnRelatedDemoPanel()
    {
        var viewModel = new MainWindowViewModel();
        var originalEditor = viewModel.Editor;
        var recipe = viewModel.CookbookRecipes.Single(item => item.Id == "plugin-trust-route");

        viewModel.SelectedCookbookRecipe = recipe;
        viewModel.OpenSelectedCookbookRecipe();

        Assert.Same(originalEditor, viewModel.Editor);
        Assert.True(viewModel.IsHostPaneOpen);
        Assert.Equal("扩展", viewModel.SelectedHostMenuGroupTitle);
        Assert.Contains(recipe.DemoAnchors[0].Path, viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);
        Assert.Contains(
            viewModel.SelectedCookbookRecipeCodeLines,
            line => line.Contains(recipe.CodeAnchors[0].Path, StringComparison.Ordinal));
        Assert.Contains(
            viewModel.SelectedCookbookRecipeProofLines,
            line => line.Contains(recipe.DocumentationAnchors[0].Path, StringComparison.Ordinal));
        Assert.Equal(recipe.SupportBoundary, viewModel.SelectedCookbookRecipeSupportBoundary);
    }

    [AvaloniaFact]
    public void MainWindow_CookbookMenuOpensDrawerWithoutReplacingGraphWorkspace()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var graphHost = Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost"));
        var graphEditorView = Assert.IsType<GraphEditorView>(graphHost.Content);
        var cookbookMenu = Assert.IsType<MenuItem>(window.FindControl<MenuItem>("PART_CookbookMenu"));

        var openCookbookItem = Assert.Single(cookbookMenu.Items!.OfType<MenuItem>());
        openCookbookItem.Command!.Execute(openCookbookItem.CommandParameter);

        Assert.True(viewModel.IsCookbookHostGroupSelected);
        Assert.True(viewModel.IsHostPaneOpen);
        Assert.Same(graphEditorView, graphHost.Content);
    }
}
