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
        => DemoCookbookWorkspaceProjection.Create(SelectedCookbookRecipe?.Id);

    public string CookbookSummary
        => T(
            "Cookbook 把目录、搜索、筛选和选中配方投影到宿主抽屉，中心图工作区保持可见。",
            "The cookbook projects catalog navigation, search, filtering, and the selected recipe into the host drawer while the graph workspace stays visible.");

    public IReadOnlyList<string> CookbookLandingLines =>
    [
        T("配方数量：", "Recipe count: ") + CookbookRecipes.Count,
        T("当前筛选结果：", "Filtered recipes: ") + FilteredCookbookRecipes.Count,
        T("当前配方：", "Selected recipe: ") + SelectedCookbookRecipe.Title,
        T("落地点：", "Landing: ") + ResolveCookbookLandingGroup(SelectedCookbookRecipe),
    ];

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
    private DemoCookbookRecipe selectedCookbookRecipe = null!;

    [ObservableProperty]
    private string lastCookbookNavigationStatus = string.Empty;

    [RelayCommand]
    public void SelectCookbookRecipe(DemoCookbookRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        SelectedCookbookRecipe = recipe;
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
            || LastCookbookNavigationStatus.StartsWith("Select", StringComparison.Ordinal))
        {
            LastCookbookNavigationStatus = T("请选择一个 cookbook 配方。", "Select a cookbook recipe.");
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

    partial void OnSelectedCookbookRecipeChanged(DemoCookbookRecipe value)
        => RefreshCookbookProjection();

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
        OnPropertyChanged(nameof(SelectedCookbookRecipeCodeLines));
        OnPropertyChanged(nameof(SelectedCookbookRecipeProofLines));
        OnPropertyChanged(nameof(SelectedCookbookRecipeSupportBoundary));
    }

    private bool MatchesCookbookSearch(DemoCookbookRecipe recipe, string searchText)
        => Contains(recipe.Id, searchText)
           || Contains(FormatCookbookCategory(recipe.Category), searchText)
           || Contains(recipe.Title, searchText)
           || Contains(recipe.Summary, searchText)
           || ContainsAny(recipe.CodeAnchors, searchText)
           || ContainsAny(recipe.DemoAnchors, searchText)
           || ContainsAny(recipe.DocumentationAnchors, searchText)
           || recipe.ProofMarkers.Any(marker => Contains(marker, searchText))
           || Contains(recipe.SupportBoundary, searchText);

    private static bool Contains(string value, string searchText)
        => value.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(IReadOnlyList<DemoCookbookAnchor> anchors, string searchText)
        => anchors.Any(anchor =>
            Contains(anchor.Label, searchText)
            || Contains(anchor.Path, searchText)
            || Contains(anchor.Evidence, searchText));

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
            DemoCookbookRecipeCategory.PluginTrust => T("Plugin Trust", "Plugin Trust"),
            DemoCookbookRecipeCategory.DiagnosticsSupport => T("Diagnostics Support", "Diagnostics Support"),
            DemoCookbookRecipeCategory.ReviewHelp => T("Review Help", "Review Help"),
            _ => category.ToString(),
        };
}
