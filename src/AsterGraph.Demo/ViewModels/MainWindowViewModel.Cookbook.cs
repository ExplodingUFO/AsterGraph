using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Demo.Cookbook;

namespace AsterGraph.Demo.ViewModels;

public sealed record CookbookCategoryFilter(string Key, string DisplayName, DemoCookbookRecipeCategory? Category);

public partial class MainWindowViewModel
{
    private IReadOnlyList<DemoCookbookRecipe> _cookbookRecipes = [];
    private IReadOnlyList<DemoCookbookRecipe> _filteredCookbookRecipes = [];
    private IReadOnlyList<CookbookCategoryFilter> _cookbookCategoryFilters = [];

    public IReadOnlyList<DemoCookbookRecipe> CookbookRecipes => _cookbookRecipes;

    public IReadOnlyList<DemoCookbookRecipe> FilteredCookbookRecipes => _filteredCookbookRecipes;

    public IReadOnlyList<CookbookCategoryFilter> CookbookCategoryFilters => _cookbookCategoryFilters;

    public DemoCookbookWorkspaceSnapshot CookbookWorkspace
        => DemoCookbookWorkspaceProjection.Create(SelectedCookbookRecipe?.Id, FilteredCookbookRecipes);

    public string CookbookSummary
        => T(
            "Cookbook 左侧工作区是主导航；宿主抽屉保留为次要摘要，中心图工作区保持可见。",
            "The cookbook left workspace is the primary navigation; the host drawer remains a secondary summary while the graph workspace stays visible.");

    public IReadOnlyList<string> CookbookLandingLines =>
    [
        T("配方数量：", "Recipe count: ") + CookbookRecipes.Count,
        T("当前筛选结果：", "Filtered recipes: ") + FilteredCookbookRecipes.Count,
        T("当前配方：", "Selected recipe: ") + SelectedCookbookRecipe.Title,
        T("落地点：", "Landing: ") + ResolveCookbookLandingGroup(SelectedCookbookRecipe),
    ];

    public bool HasCookbookFilterResults => FilteredCookbookRecipes.Count > 0;

    public bool HasCookbookEmptyFilterFeedback => !HasCookbookFilterResults;

    public string CookbookFilterFeedback
        => HasCookbookFilterResults
            ? T("可以从下方分组中选择匹配配方。", "Choose a matching recipe from the grouped navigation below.")
            : T("没有匹配配方，当前配方仍保持可见：", "No matching recipes; the current recipe stays visible: ")
              + SelectedCookbookRecipe.Title;

    public IReadOnlyList<string> SelectedCookbookRecipeCodeLines =>
    [
        .. FormatCookbookAnchors(T("代码入口：", "Code: "), SelectedCookbookRecipe.CodeAnchors),
        .. FormatCookbookAnchors(T("Demo 路径：", "Demo: "), SelectedCookbookRecipe.DemoAnchors),
    ];

    public IReadOnlyList<string> SelectedCookbookRecipeProofLines =>
    [
        .. FormatCookbookAnchors(T("文档：", "Docs: "), SelectedCookbookRecipe.DocumentationAnchors),
        .. SelectedCookbookRecipe.ProofMarkers.Select(marker => T("证明标记：", "Proof marker: ") + marker),
    ];

    public string SelectedCookbookRecipeSupportBoundary => SelectedCookbookRecipe.SupportBoundary;

    [ObservableProperty]
    private string cookbookSearchText = string.Empty;

    [ObservableProperty]
    private CookbookCategoryFilter selectedCookbookCategoryFilter = null!;

    [ObservableProperty]
    private string lastCookbookNavigationStatus = string.Empty;

    [RelayCommand]
    public void SelectCookbookRecipe(DemoCookbookRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        SelectedCookbookRecipe = recipe;
    }

    [RelayCommand]
    public void SelectCookbookWorkspaceRecipe(DemoCookbookWorkspaceNavigationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        SelectedCookbookRecipe = CookbookRecipes.Single(recipe =>
            string.Equals(recipe.Id, item.RecipeId, StringComparison.Ordinal));
    }

    [RelayCommand]
    public void OpenSelectedCookbookRecipe()
    {
        var recipe = SelectedCookbookRecipe;
        var group = ResolveCookbookLandingGroup(recipe);

        SelectedHostMenuGroup = group;
        IsHostPaneOpen = true;
        LastCookbookNavigationStatus = T("已打开配方 Demo 路径：", "Opened recipe demo path: ") + ResolvePrimaryAnchorPath(recipe.DemoAnchors);
        RefreshCookbookProjection();
    }

    private void InitializeCookbook()
    {
        _cookbookRecipes = DemoCookbookCatalog.Recipes.ToArray();
        RebuildCookbookCategoryFilters(null);
        SelectedCookbookRecipe = _cookbookRecipes[0];
        LastCookbookNavigationStatus = T("请选择一个 cookbook 配方。", "Select a cookbook recipe.");
        RefreshFilteredCookbookRecipes();
    }

    private void UpdateCookbookLocalization()
    {
        if (_cookbookRecipes.Count == 0)
        {
            return;
        }

        RebuildCookbookCategoryFilters(SelectedCookbookCategoryFilter?.Category);
        if (string.IsNullOrWhiteSpace(LastCookbookNavigationStatus)
            || LastCookbookNavigationStatus.StartsWith("请选择", StringComparison.Ordinal)
            || LastCookbookNavigationStatus.StartsWith("Select", StringComparison.Ordinal)
            || LastCookbookNavigationStatus.StartsWith("已选择", StringComparison.Ordinal)
            || LastCookbookNavigationStatus.StartsWith("Selected cookbook", StringComparison.Ordinal))
        {
            LastCookbookNavigationStatus = selectedCookbookRecipe is null
                ? T("请选择一个 cookbook 配方。", "Select a cookbook recipe.")
                : FormatCookbookSelectionStatus(selectedCookbookRecipe);
        }

        RefreshFilteredCookbookRecipes();
    }

    private void RebuildCookbookCategoryFilters(DemoCookbookRecipeCategory? selectedCategory)
    {
        _cookbookCategoryFilters =
        [
            new CookbookCategoryFilter("all", T("全部", "All"), null),
            .. DemoCookbookCatalog.RequiredCategories.Select(category =>
                new CookbookCategoryFilter(category.ToString(), FormatCookbookCategory(category), category)),
        ];

        SelectedCookbookCategoryFilter = _cookbookCategoryFilters.Single(filter => filter.Category == selectedCategory);
    }

    partial void OnCookbookSearchTextChanged(string value)
        => RefreshFilteredCookbookRecipes();

    partial void OnSelectedCookbookCategoryFilterChanged(CookbookCategoryFilter value)
        => RefreshFilteredCookbookRecipes();

    private void RefreshFilteredCookbookRecipes()
    {
        if (_cookbookRecipes.Count == 0 || SelectedCookbookCategoryFilter is null)
        {
            return;
        }

        var searchText = CookbookSearchText.Trim();
        var category = SelectedCookbookCategoryFilter.Category;
        _filteredCookbookRecipes = _cookbookRecipes
            .Where(recipe => category is null || recipe.Category == category)
            .Where(recipe => string.IsNullOrWhiteSpace(searchText) || MatchesCookbookSearch(recipe, searchText))
            .ToArray();

        if (_filteredCookbookRecipes.Count > 0 && !_filteredCookbookRecipes.Contains(SelectedCookbookRecipe))
        {
            SelectedCookbookRecipe = _filteredCookbookRecipes[0];
        }
        else if (_filteredCookbookRecipes.Count > 0)
        {
            LastCookbookNavigationStatus = FormatCookbookSelectionStatus(SelectedCookbookRecipe);
        }
        else if (_filteredCookbookRecipes.Count == 0)
        {
            LastCookbookNavigationStatus = T("没有匹配配方，当前配方仍保持可见。", "No matching recipes; the current recipe remains visible.");
        }

        RefreshCookbookProjection();
    }

    private void RefreshCookbookProjection()
    {
        OnPropertyChanged(nameof(CookbookRecipes));
        OnPropertyChanged(nameof(FilteredCookbookRecipes));
        OnPropertyChanged(nameof(CookbookCategoryFilters));
        OnPropertyChanged(nameof(CookbookWorkspace));
        OnPropertyChanged(nameof(IsCookbookHostGroupSelected));
        OnPropertyChanged(nameof(CookbookSummary));
        OnPropertyChanged(nameof(CookbookLandingLines));
        OnPropertyChanged(nameof(HasCookbookFilterResults));
        OnPropertyChanged(nameof(HasCookbookEmptyFilterFeedback));
        OnPropertyChanged(nameof(CookbookFilterFeedback));
        OnPropertyChanged(nameof(SelectedCookbookRecipeCodeLines));
        OnPropertyChanged(nameof(SelectedCookbookRecipeProofLines));
        OnPropertyChanged(nameof(SelectedCookbookRecipeSupportBoundary));
        OnPropertyChanged(nameof(SelectedCookbookRecipeCodeSample));
        OnPropertyChanged(nameof(SelectedCookbookScenarioPoint));
    }

    private void RefreshCookbookScenarioProjection()
    {
        OnPropertyChanged(nameof(SelectedCookbookScenarioPoint));
    }

    private static IEnumerable<string> FormatCookbookAnchors(string prefix, IReadOnlyList<DemoCookbookAnchor> anchors)
        => anchors.Select(anchor => prefix + anchor.Path + " — " + anchor.Evidence);

    private static string ResolvePrimaryAnchorPath(IReadOnlyList<DemoCookbookAnchor> anchors)
        => anchors.Count == 0 ? string.Empty : anchors[0].Path;
    private string ResolveCookbookLandingGroup(DemoCookbookRecipe recipe)
        => recipe.Category switch
        {
            DemoCookbookRecipeCategory.PluginTrust => DemoHostMenuGroups.Extensions,
            DemoCookbookRecipeCategory.DiagnosticsSupport => DemoHostMenuGroups.Runtime,
            DemoCookbookRecipeCategory.ReviewHelp => DemoHostMenuGroups.Proof,
            DemoCookbookRecipeCategory.Authoring => DemoHostMenuGroups.Tour,
            DemoCookbookRecipeCategory.StarterHost => DemoHostMenuGroups.Integration,
            _ => DemoHostMenuGroups.Cookbook,
        };

    private string FormatCookbookCategory(DemoCookbookRecipeCategory category)
        => category switch
        {
            DemoCookbookRecipeCategory.StarterHost => T("Starter Host", "Starter Host"),
            DemoCookbookRecipeCategory.Authoring => T("Authoring", "Authoring"),
            DemoCookbookRecipeCategory.PerformanceViewport => T("Performance / Viewport", "Performance / Viewport"),
            DemoCookbookRecipeCategory.PluginTrust => T("Plugin Trust", "Plugin Trust"),
            DemoCookbookRecipeCategory.DiagnosticsSupport => T("Diagnostics Support", "Diagnostics Support"),
            DemoCookbookRecipeCategory.ReviewHelp => T("Review Help", "Review Help"),
            _ => category.ToString(),
        };
}
