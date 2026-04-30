using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Demo.Views;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookScenarioCueTests
{
    [Fact]
    public void MainWindowViewModel_CookbookScenarioSelectionUpdatesGraphAndContentCues()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(item => item.Id == "authoring-surface-route");
        viewModel.SelectedCookbookRecipe = recipe;

        var initialScenario = viewModel.SelectedCookbookScenarioPoint;
        var nextScenario = viewModel.CookbookWorkspace.SelectedRecipe.ScenarioPoints
            .First(point => !string.Equals(point.Key, initialScenario.Key, StringComparison.Ordinal));

        viewModel.SelectCookbookScenarioPointCommand.Execute(nextScenario);

        Assert.Equal(nextScenario.Key, viewModel.SelectedCookbookScenarioPoint.Key);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceGraphLines, line => line.Contains(nextScenario.GraphCueTarget, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.Contains(nextScenario.ContentCue, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "scenario");

        Assert.Contains(nextScenario.Label, viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(nextScenario.GraphCueTarget, StringComparison.Ordinal));

        viewModel.SelectedCookbookRecipe = viewModel.CookbookRecipes.Single(item => item.Id == "plugin-trust-route");

        Assert.StartsWith("plugin-trust-route:scenario-", viewModel.SelectedCookbookScenarioPoint.Key, StringComparison.Ordinal);
    }

    [AvaloniaFact]
    public void MainWindow_CookbookScenarioCueListSelectsBoundedScenarioState()
    {
        var viewModel = new MainWindowViewModel();
        var window = new MainWindow
        {
            DataContext = viewModel,
        };

        window.Show();
        window.UpdateLayout();
        viewModel.OpenHostMenuGroupCommand.Execute("cookbook");
        window.UpdateLayout();

        var scenarioCueList = Assert.IsType<ListBox>(window.FindControl<ListBox>("PART_CookbookWorkspaceScenarioCueList"));
        var nextScenario = viewModel.CookbookWorkspace.SelectedRecipe.ScenarioPoints[1];

        viewModel.SelectCookbookScenarioPointCommand.Execute(nextScenario);
        window.UpdateLayout();

        Assert.Equal(nextScenario.Key, viewModel.SelectedCookbookScenarioPoint.Key);
        Assert.Equal(viewModel.SelectedCookbookScenarioPoint, scenarioCueList.SelectedItem);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceGraphLines, line => line.Contains(nextScenario.GraphCueTarget, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.Contains(nextScenario.ContentCue, StringComparison.Ordinal));
    }
}
