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
        Assert.Equal(viewModel.SelectedCookbookRecipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.Equal(DemoCookbookCatalog.RequiredCategories.Count, viewModel.CookbookWorkspace.NavigationGroups.Count);
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
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
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

    [AvaloniaFact]
    public void MainWindow_CookbookModeShowsLeftNavigationBesideGraph()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();

        var navigationPanel = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceNavigationPanel"));
        var contentShell = Assert.IsType<Grid>(window.FindControl<Grid>("PART_CookbookWorkspaceContentShell"));
        var navigationGroups = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("PART_CookbookWorkspaceNavigationGroups"));
        var contentPanel = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceRecipeContentPanel"));
        var editorFrame = Assert.IsType<Border>(window.FindControl<Border>("MainEditorFrame"));
        var graphLines = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("PART_CookbookWorkspaceGraphLines"));
        var detailModeSelector = Assert.IsType<ComboBox>(window.FindControl<ComboBox>("PART_CookbookWorkspaceDetailModeSelector"));
        var detailLines = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("PART_CookbookWorkspaceDetailLines"));
        var recipeList = Assert.IsType<ListBox>(window.FindControl<ListBox>("PART_CookbookWorkspaceRecipeList"));
        var graphHost = Assert.IsType<ContentControl>(window.FindControl<ContentControl>("PART_MainGraphEditorHost"));
        var graphEditorView = Assert.IsType<GraphEditorView>(graphHost.Content);

        Assert.False(navigationPanel.IsVisible);
        Assert.False(contentPanel.IsVisible);

        viewModel.OpenHostMenuGroupCommand.Execute("cookbook");

        Assert.True(navigationPanel.IsVisible);
        Assert.True(contentPanel.IsVisible);
        Assert.Equal(304, navigationPanel.Width);
        Assert.Equal(1, Grid.GetColumn(contentShell));
        Assert.Equal(1, Grid.GetRow(editorFrame));
        Assert.Equal(2, Grid.GetRow(contentPanel));
        var boundGraphLines = Assert.IsAssignableFrom<System.Collections.IEnumerable>(graphLines.ItemsSource)
            .Cast<string>()
            .ToArray();
        Assert.Equal(viewModel.SelectedCookbookWorkspaceGraphLines, boundGraphLines);
        var boundGroups = Assert.IsAssignableFrom<System.Collections.IEnumerable>(navigationGroups.ItemsSource)
            .Cast<DemoCookbookWorkspaceNavigationGroup>()
            .ToArray();
        Assert.Equal(viewModel.CookbookWorkspace.NavigationGroups.Select(group => group.Category), boundGroups.Select(group => group.Category));
        Assert.Same(viewModel.CookbookDetailModes, detailModeSelector.ItemsSource);
        Assert.Same(viewModel.SelectedCookbookDetailMode, detailModeSelector.SelectedItem);
        var boundDetailLines = Assert.IsAssignableFrom<System.Collections.IEnumerable>(detailLines.ItemsSource)
            .Cast<string>()
            .ToArray();
        Assert.Equal(viewModel.SelectedCookbookWorkspaceDetailLines, boundDetailLines);
        Assert.Same(viewModel.FilteredCookbookRecipes, recipeList.ItemsSource);
        Assert.Same(viewModel.SelectedCookbookRecipe, recipeList.SelectedItem);
        Assert.Same(graphEditorView, graphHost.Content);
    }

    [Fact]
    public void MainWindowViewModel_CookbookWorkspaceNavigationCommandSelectsRecipe()
    {
        var viewModel = new MainWindowViewModel();
        var item = viewModel.CookbookWorkspace.NavigationGroups
            .SelectMany(group => group.Recipes)
            .Single(recipe => recipe.RecipeId == "diagnostics-support-route");

        viewModel.SelectCookbookWorkspaceRecipeCommand.Execute(item);

        Assert.Equal("diagnostics-support-route", viewModel.SelectedCookbookRecipe.Id);
        Assert.Equal("diagnostics-support-route", viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
    }

    [Fact]
    public void MainWindowViewModel_CookbookDetailModesSwitchWithoutLosingRecipe()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(item => item.Id == "authoring-surface-route");
        viewModel.SelectedCookbookRecipe = recipe;

        Assert.Equal("code", viewModel.SelectedCookbookDetailMode.Key);
        Assert.Contains(
            viewModel.SelectedCookbookWorkspaceGraphLines,
            line => line.Contains(recipe.DemoAnchors[0].Path, StringComparison.Ordinal));
        Assert.Contains(
            viewModel.SelectedCookbookWorkspaceDetailLines,
            line => line.Contains(recipe.CodeAnchors[0].Path, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "proof");
        Assert.Equal(recipe.Id, viewModel.SelectedCookbookRecipe.Id);
        Assert.Contains(
            viewModel.SelectedCookbookWorkspaceDetailLines,
            line => line.Contains(recipe.ProofMarkers[0], StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "docs");
        Assert.Contains(
            viewModel.SelectedCookbookWorkspaceDetailLines,
            line => line.Contains(recipe.DocumentationAnchors[0].Path, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "support");
        Assert.Equal([recipe.SupportBoundary], viewModel.SelectedCookbookWorkspaceDetailLines);
    }
}
