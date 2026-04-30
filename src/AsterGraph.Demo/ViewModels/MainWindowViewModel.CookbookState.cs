using AsterGraph.Demo.Cookbook;
using CommunityToolkit.Mvvm.Input;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private DemoCookbookRecipe selectedCookbookRecipe = null!;
    private string selectedCookbookScenarioPointKey = string.Empty;

    public DemoCookbookRecipe SelectedCookbookRecipe
    {
        get => selectedCookbookRecipe;
        set
        {
            if (value is null || ReferenceEquals(selectedCookbookRecipe, value))
            {
                return;
            }

            SetProperty(ref selectedCookbookRecipe, value);
            selectedCookbookScenarioPointKey = string.Empty;
            LastCookbookNavigationStatus = FormatCookbookSelectionStatus(value);
            RefreshCookbookProjection();
        }
    }

    public DemoCookbookWorkspaceScenarioPoint SelectedCookbookScenarioPoint
    {
        get => ResolveSelectedCookbookScenarioPoint();
        set
        {
            if (value is null || string.Equals(selectedCookbookScenarioPointKey, value.Key, StringComparison.Ordinal))
            {
                return;
            }

            selectedCookbookScenarioPointKey = value.Key;
            RefreshCookbookScenarioProjection();
        }
    }

    [RelayCommand]
    public void SelectCookbookScenarioPoint(DemoCookbookWorkspaceScenarioPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        SelectedCookbookScenarioPoint = point;
    }

    private string FormatCookbookSelectionStatus(DemoCookbookRecipe recipe)
        => T("已选择 cookbook 配方：", "Selected cookbook recipe: ")
           + recipe.Title + T("。路线状态：", ". Route status: ") + CookbookWorkspace.SelectedRecipe.RouteStatus;

    private DemoCookbookWorkspaceScenarioPoint ResolveSelectedCookbookScenarioPoint()
    {
        var scenarioPoints = CookbookWorkspace.SelectedRecipe.ScenarioPoints;
        var selected = scenarioPoints.SingleOrDefault(point =>
            string.Equals(point.Key, selectedCookbookScenarioPointKey, StringComparison.Ordinal));

        return selected ?? scenarioPoints[0];
    }
}
