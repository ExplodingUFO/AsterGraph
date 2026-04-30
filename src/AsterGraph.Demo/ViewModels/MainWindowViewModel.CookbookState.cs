using AsterGraph.Demo.Cookbook;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private DemoCookbookRecipe selectedCookbookRecipe = null!;

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
            LastCookbookNavigationStatus = FormatCookbookSelectionStatus(value);
            RefreshCookbookProjection();
        }
    }

    private string FormatCookbookSelectionStatus(DemoCookbookRecipe recipe)
        => T("已选择 cookbook 配方：", "Selected cookbook recipe: ")
           + recipe.Title + T("。路线状态：", ". Route status: ") + CookbookWorkspace.SelectedRecipe.RouteStatus;
}
