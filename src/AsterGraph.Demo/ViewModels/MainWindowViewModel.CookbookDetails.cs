namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public string SelectedCookbookRecipeCodeSample
        => SelectedCookbookRecipe?.CodeSample ?? string.Empty;
}
