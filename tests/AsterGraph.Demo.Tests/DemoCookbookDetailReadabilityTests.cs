using System;
using System.Linq;
using AsterGraph.Demo.ViewModels;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookDetailReadabilityTests
{
    [Fact]
    public void CookbookDetailModesUseReadableLinesWithoutLosingRecipe()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(item => item.Id == "authoring-surface-route");
        viewModel.SelectedCookbookRecipe = recipe;

        Assert.Equal("code", viewModel.SelectedCookbookDetailMode.Key);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceGraphLines, line => line.Contains(recipe.DemoAnchors[0].Path, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.Contains(viewModel.CookbookWorkspace.SelectedRecipe.RouteStatus, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("支持路线：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("包边界：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("Demo 边界：", StringComparison.Ordinal));
        Assert.StartsWith("路径：", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.CodeAnchors[0].Path, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.CodeAnchors[0].Evidence, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "proof");
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.StartsWith("证明标记：", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.ProofMarkers[0], StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "docs");
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.StartsWith("路径：", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.DocumentationAnchors[0].Path, StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.DocumentationAnchors[0].Evidence, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "scenario");
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.StartsWith("当前场景：", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("图线索：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("内容线索：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("图操作：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("节点元数据：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("支持证据：", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.Contains(recipe.ScenarioPoints[0].Evidence, StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "support");
        Assert.Equal(recipe.Id, viewModel.CookbookWorkspace.SelectedRecipe.RecipeId);
        Assert.StartsWith("支持边界：", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(recipe.SupportBoundary, viewModel.SelectedCookbookWorkspaceDetailLines);
    }

    [Fact]
    public void CookbookDetailModesReprojectAcrossLanguageSwitch()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(item => item.Id == "authoring-surface-route");
        viewModel.SelectedCookbookRecipe = recipe;

        viewModel.SelectLanguage("en");

        Assert.Equal(recipe.Id, viewModel.SelectedCookbookRecipe.Id);
        Assert.StartsWith("Path: ", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "support");

        Assert.StartsWith("Support boundary: ", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(recipe.SupportBoundary, viewModel.SelectedCookbookWorkspaceDetailLines);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("Supported route: ", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("Package boundary: ", StringComparison.Ordinal));
        Assert.Contains(viewModel.SelectedCookbookWorkspaceCoverageLines, line => line.StartsWith("Demo boundary: ", StringComparison.Ordinal));

        viewModel.SelectedCookbookDetailMode = viewModel.CookbookDetailModes.Single(mode => mode.Key == "scenario");

        Assert.StartsWith("Selected scenario: ", viewModel.SelectedCookbookWorkspaceDetailLines[0], StringComparison.Ordinal);
        Assert.Contains(viewModel.SelectedCookbookWorkspaceDetailLines, line => line.StartsWith("Graph operations: ", StringComparison.Ordinal));
    }
}
