using System;
using System.Linq;
using AsterGraph.Demo.Cookbook;
using AsterGraph.Demo.ViewModels;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookDetailReadabilityTests
{
    [Fact]
    public void CookbookRecipes_AllHaveNonEmptyCodeSamples()
    {
        foreach (var recipe in DemoCookbookCatalog.Recipes)
        {
            Assert.False(string.IsNullOrWhiteSpace(recipe.CodeSample),
                $"Recipe '{recipe.Id}' is missing a CodeSample.");
        }
    }

    [Fact]
    public void CookbookCodeSamples_ContainExpectedApiTermsForKeyRecipes()
    {
        var viewModel = new MainWindowViewModel();

        // Starter host recipe should mention host builder
        var starterRecipe = viewModel.CookbookRecipes.Single(r => r.Id == "starter-host-route");
        Assert.Contains("AsterGraphHostBuilder", starterRecipe.CodeSample, StringComparison.Ordinal);

        // Authoring platform should mention session commands
        var authoringRecipe = viewModel.CookbookRecipes.Single(r => r.Id == "v077-authoring-platform-route");
        Assert.Contains("GetCommandRegistry", authoringRecipe.CodeSample, StringComparison.Ordinal);

        // Selection rectangle should mention selection commands
        var selectionRecipe = viewModel.CookbookRecipes.Single(r => r.Id == "v079-selection-rectangle-route");
        Assert.Contains("SelectAll", selectionRecipe.CodeSample, StringComparison.Ordinal);

        // Keyboard navigation should mention move/zoom/pan
        var keyboardRecipe = viewModel.CookbookRecipes.Single(r => r.Id == "v079-keyboard-navigation-route");
        Assert.Contains("TryMoveSelectionBy", keyboardRecipe.CodeSample, StringComparison.Ordinal);

        // Host event should mention event subscription
        var eventRecipe = viewModel.CookbookRecipes.Single(r => r.Id == "v079-host-event-route");
        Assert.Contains("SelectionChanged", eventRecipe.CodeSample, StringComparison.Ordinal);
    }

    [Fact]
    public void MainWindowViewModel_CodeSamplePropertyReturnsSelectedRecipeSample()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(r => r.Id == "plugin-trust-route");

        viewModel.SelectedCookbookRecipe = recipe;

        Assert.Equal(recipe.CodeSample, viewModel.SelectedCookbookRecipeCodeSample);
        Assert.Contains("DiscoverPluginCandidates", viewModel.SelectedCookbookRecipeCodeSample, StringComparison.Ordinal);
    }

    [Fact]
    public void CookbookCodeSamples_ReprojectAcrossLanguageSwitch()
    {
        var viewModel = new MainWindowViewModel();
        var recipe = viewModel.CookbookRecipes.Single(r => r.Id == "v077-authoring-platform-route");
        viewModel.SelectedCookbookRecipe = recipe;

        var originalSample = viewModel.SelectedCookbookRecipeCodeSample;

        viewModel.SelectLanguage("en");

        Assert.Equal(originalSample, viewModel.SelectedCookbookRecipeCodeSample);
        Assert.Contains("GetCommandRegistry", viewModel.SelectedCookbookRecipeCodeSample, StringComparison.Ordinal);
    }
}
