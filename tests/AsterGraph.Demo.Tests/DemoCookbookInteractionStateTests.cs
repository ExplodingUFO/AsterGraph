using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookInteractionStateTests
{
    [AvaloniaFact]
    public void MainWindow_CookbookEmptyFilterDisablesNavigationAndKeepsStateBounded()
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

        var navigationGroups = Assert.IsType<ItemsControl>(window.FindControl<ItemsControl>("PART_CookbookWorkspaceNavigationGroups"));
        var feedback = Assert.IsType<Border>(window.FindControl<Border>("PART_CookbookWorkspaceNavigationFeedback"));
        Assert.True(navigationGroups.IsEnabled);
        Assert.Equal(132, feedback.MaxHeight);

        viewModel.SelectedCookbookCategoryFilter = viewModel.CookbookCategoryFilters.Single(filter =>
            filter.Category == DemoCookbookRecipeCategory.DiagnosticsSupport);
        var selected = viewModel.SelectedCookbookRecipe;
        viewModel.CookbookSearchText = "allowlist";
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.False(navigationGroups.IsEnabled);
        Assert.Same(selected, viewModel.SelectedCookbookRecipe);
        Assert.Equal("没有匹配配方，当前配方仍保持可见。", viewModel.LastCookbookNavigationStatus);

        viewModel.CookbookSearchText = string.Empty;
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        Assert.True(navigationGroups.IsEnabled);
        Assert.Contains("已选择 cookbook 配方：", viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);
        Assert.DoesNotContain("没有匹配配方", viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);
    }

    [Fact]
    public void CookbookInteractionStatusReprojectsWhenLanguageChanges()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SelectCookbookWorkspaceRecipe(viewModel.CookbookWorkspace.NavigationGroups
            .SelectMany(group => group.Recipes)
            .Single(recipe => recipe.RecipeId == "plugin-trust-route"));

        Assert.Contains("已选择 cookbook 配方：", viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);

        viewModel.SelectLanguage("en");

        Assert.Contains("Selected cookbook recipe: ", viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);
        Assert.DoesNotContain("已选择", viewModel.LastCookbookNavigationStatus, StringComparison.Ordinal);
    }
}
