using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookVisualBaselineTests
{
    [Fact]
    public void CookbookVisualBaseline_DefinesAuditableLayoutContract()
    {
        Assert.Equal(304, NavigationPanelWidth);
        Assert.Equal(320, GraphMinimumHeight);
        Assert.Equal(260, DetailPanelMaximumHeight);
        Assert.Contains("PART_CookbookWorkspaceNavigationPanel", RequiredNamedParts);
        Assert.Contains("PART_MainGraphEditorHost", RequiredNamedParts);
        Assert.Contains("PART_CookbookWorkspaceRecipeContentPanel", RequiredNamedParts);
        Assert.Contains(OwnedFiles, path => path.EndsWith("DemoCookbookVisualBaselineTests.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(OwnedFiles, path => path.Contains("MainWindowViewModel.Showcase", StringComparison.Ordinal));
        Assert.DoesNotContain(OwnedFiles, path => path.Contains("DemoCookbookWorkspaceProjection", StringComparison.Ordinal));
        Assert.All(Risks, risk => Assert.False(string.IsNullOrWhiteSpace(risk)));
    }

    [Fact]
    public void CookbookVisualBaseline_XamlBindsSelectedStateAndBoundedDetailPanel()
    {
        var xaml = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "AsterGraph.Demo",
            "Views",
            "MainWindow.axaml"));

        foreach (var namedPart in RequiredNamedParts)
        {
            Assert.Contains($"x:Name=\"{namedPart}\"", xaml, StringComparison.Ordinal);
        }

        Assert.Contains("Selector=\"Button.cookbook-workspace-recipe.selected\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Classes.selected=\"{Binding IsSelected}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PART_CookbookWorkspaceScenarioCueList", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedItem=\"{Binding SelectedCookbookScenarioPoint, Mode=TwoWay}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PART_CookbookWorkspaceCodeDemoSection", xaml, StringComparison.Ordinal);
        Assert.Contains("PART_CookbookWorkspaceWorkflowSection", xaml, StringComparison.Ordinal);
        Assert.Contains("PART_CookbookWorkspaceProofSupportSection", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedCookbookWorkspaceWorkflowStepLines", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectedCookbookWorkspaceProofSupportLines", xaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"260\"", xaml, StringComparison.Ordinal);
    }

    private const double NavigationPanelWidth = 304;
    private const double GraphMinimumHeight = 320;
    private const double DetailPanelMaximumHeight = 260;

    private static readonly string[] RequiredNamedParts =
    [
        "PART_CookbookWorkspaceNavigationPanel",
        "PART_CookbookWorkspaceNavigationGroups",
        "PART_CookbookWorkspaceContentShell",
        "PART_MainGraphEditorHost",
        "PART_CookbookWorkspaceRecipeContentPanel",
        "PART_CookbookWorkspaceCodeDemoSection",
        "PART_CookbookWorkspaceWorkflowSection",
        "PART_CookbookWorkspaceProofSupportSection",
        "PART_CookbookWorkspaceScenarioCueList",
        "PART_CookbookWorkspaceWorkflowStepLines",
        "PART_CookbookWorkspaceProofSupportLines",
        "PART_CookbookWorkspaceDetailModeSelector",
        "PART_CookbookWorkspaceDetailLines",
    ];

    private static readonly string[] OwnedFiles =
    [
        "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.Cookbook.cs",
        "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.CookbookDetails.cs",
        "src/AsterGraph.Demo/ViewModels/MainWindowViewModel.CookbookState.cs",
        "src/AsterGraph.Demo/Views/MainWindow.axaml",
        "src/AsterGraph.Demo/Views/Resources/MainWindowResources.axaml",
        "tests/AsterGraph.Demo.Tests/DemoCookbookNavigationTests.cs",
        "tests/AsterGraph.Demo.Tests/DemoCookbookNavigationPolishTests.cs",
        "tests/AsterGraph.Demo.Tests/DemoCookbookDetailReadabilityTests.cs",
        "tests/AsterGraph.Demo.Tests/DemoCookbookVisualBaselineTests.cs",
    ];

    private static readonly string[] Risks =
    [
        "Selected grouped navigation must visibly bind projected recipe selection.",
        "The bottom detail panel must stay height-bounded so the graph remains the primary workspace.",
        "Path-heavy code, proof, docs, and support lines must wrap inside local line cards.",
        "Cookbook polish must stay in narrow cookbook files instead of broad shell or showcase aggregation.",
    ];

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }
}
