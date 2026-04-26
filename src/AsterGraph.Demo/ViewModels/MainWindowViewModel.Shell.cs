using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Demo.Shell;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public sealed record RecentWorkspaceEntry(string Path, string DisplayName);

public partial class MainWindowViewModel
{
    private const int RecentWorkspaceLimit = 5;
    private readonly MainWindowShellOptions _shellOptions;
    private readonly DemoMutableWorkspaceService _workspaceService;
    private readonly DemoShellStateStore _shellStateStore;
    private readonly ObservableCollection<string> _recentWorkspacePaths = [];
    private GraphEditorViewportSnapshot? _pendingViewportRestore;

    [ObservableProperty]
    private bool isMiniMapVisible = true;

    [ObservableProperty]
    private string activeWorkspacePath = string.Empty;

    [ObservableProperty]
    private bool hasAutosaveDraft;

    [ObservableProperty]
    private bool dirtyExitPromptPending;

    [ObservableProperty]
    private string dirtyExitPromptMessage = string.Empty;

    [ObservableProperty]
    private bool reopenLastWorkspaceOnLaunch;

    [ObservableProperty]
    private double preferredWindowWidth = 1480;

    [ObservableProperty]
    private double preferredWindowHeight = 900;

    [ObservableProperty]
    private string themeVariantCaption = "Default";

    public IReadOnlyList<string> RecentWorkspacePaths => _recentWorkspacePaths;

    public IReadOnlyList<RecentWorkspaceEntry> RecentWorkspaceEntries
        => _recentWorkspacePaths
            .Select(path => new RecentWorkspaceEntry(path, Path.GetFileName(path)))
            .ToArray();

    public string AutosaveDraftPath => _shellStateStore.AutosaveDraftPath;

    public IReadOnlyList<string> ShellWorkflowLines =>
    [
        T("当前工作区：", "Current workspace: ") + ActiveWorkspacePath,
        T("最近工作区：", "Recent workspaces: ") + RecentWorkspacePaths.Count,
        T("自动恢复上次工作区：", "Restore last workspace on startup: ") + BoolText(ReopenLastWorkspaceOnLaunch),
        T("自动保存草稿：", "Autosave draft available: ") + BoolText(HasAutosaveDraft),
        T("主快捷键映射：", "Primary shortcut mapping: ") + $"{PrimaryShortcutModifier}+… / {PrimaryShortcutModifier}+Shift+P",
        T("IME 输入：检查器与搜索框继续使用 Avalonia 原生 TextBox / ComboBox。", "IME input: inspector and search fields stay on Avalonia stock TextBox / ComboBox controls."),
        T("触控板缩放：支持精度触控板把捏合转换成 Control + 滚轮缩放。", "Trackpad zoom: precision trackpads are supported through Control-modified wheel zoom gestures."),
        T("系统主题同步：", "System theme synchronization: ") + ThemeVariantCaption,
    ];

    public void SaveWorkspaceAs(string? workspacePath = null)
    {
        var resolvedPath = string.IsNullOrWhiteSpace(workspacePath)
            ? ResolveDefaultSaveAsWorkspacePath()
            : workspacePath;

        SetActiveWorkspacePath(resolvedPath);
        Editor.SaveWorkspace();
        RegisterRecentWorkspace(resolvedPath);
        ClearDirtyExitPrompt();
        HasAutosaveDraft = false;
        _shellStateStore.DeleteAutosaveDraft();
        PersistShellState();
    }

    public bool TryOpenWorkspacePath(string workspacePath)
    {
        if (string.IsNullOrWhiteSpace(workspacePath) || !File.Exists(workspacePath))
        {
            return false;
        }

        SetActiveWorkspacePath(workspacePath);
        var loaded = Editor.LoadWorkspace();
        if (!loaded)
        {
            return false;
        }

        RegisterRecentWorkspace(workspacePath);
        ClearDirtyExitPrompt();
        PersistShellState();
        return true;
    }

    public bool ReopenLastWorkspace()
        => TryOpenWorkspacePath(_recentWorkspacePaths.FirstOrDefault() ?? ActiveWorkspacePath);

    public bool RestoreAutosaveDraft()
    {
        if (!File.Exists(AutosaveDraftPath))
        {
            HasAutosaveDraft = false;
            return false;
        }

        var currentWorkspacePath = ActiveWorkspacePath;
        var currentViewport = Session.Queries.GetViewportSnapshot();

        SetActiveWorkspacePath(AutosaveDraftPath);
        var loaded = Editor.LoadWorkspace();
        SetActiveWorkspacePath(currentWorkspacePath);
        _pendingViewportRestore = currentViewport;
        HasAutosaveDraft = loaded;
        PersistShellState();
        return loaded;
    }

    public void RecordWindowSize(double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        PreferredWindowWidth = Math.Max(1280, width);
        PreferredWindowHeight = Math.Max(760, height);
        PersistShellState();
    }

    public bool HandleWindowClosingRequest()
    {
        if (!Editor.IsDirty)
        {
            return false;
        }

        if (DirtyExitPromptPending)
        {
            DirtyExitPromptPending = false;
            DirtyExitPromptMessage = string.Empty;
            return false;
        }

        PersistAutosaveDraft();
        DirtyExitPromptPending = true;
        DirtyExitPromptMessage = T(
            "当前工作区仍有未保存修改。再次关闭窗口将退出；草稿已写入自动保存文件。",
            "The workspace still has unsaved changes. Close the window again to exit; a crash-safe autosave draft has been written.");
        RefreshRuntimeProjection();
        return true;
    }

    public void RestorePendingViewport()
    {
        if (_pendingViewportRestore is null)
        {
            return;
        }

        var target = _pendingViewportRestore;
        var current = Session.Queries.GetViewportSnapshot();

        if (current.ViewportWidth <= 0 || current.ViewportHeight <= 0)
        {
            Session.Commands.UpdateViewportSize(target.ViewportWidth, target.ViewportHeight);
            current = Session.Queries.GetViewportSnapshot();
        }

        if (Math.Abs(current.Zoom - target.Zoom) > 0.001)
        {
            Session.Commands.ZoomAt(target.Zoom / current.Zoom, new GraphPoint(0, 0));
        }

        var updated = Session.Queries.GetViewportSnapshot();
        Session.Commands.PanBy(target.PanX - updated.PanX, target.PanY - updated.PanY);
        _pendingViewportRestore = null;
        PersistShellState();
    }

    public void UpdateThemeVariant(string? themeVariant)
    {
        ThemeVariantCaption = string.IsNullOrWhiteSpace(themeVariant) ? "Default" : themeVariant;
        PersistShellState();
    }

    private void InitializeShellState()
    {
        var shellState = _shellOptions.EnableStatePersistence
            ? _shellStateStore.Load()
            : DemoShellState.Empty;

        PreferredWindowWidth = shellState.WindowWidth > 0 ? shellState.WindowWidth : PreferredWindowWidth;
        PreferredWindowHeight = shellState.WindowHeight > 0 ? shellState.WindowHeight : PreferredWindowHeight;
        IsHeaderChromeVisible = shellState.IsHeaderChromeVisible;
        IsLibraryChromeVisible = shellState.IsLibraryChromeVisible;
        IsInspectorChromeVisible = shellState.IsInspectorChromeVisible;
        IsStatusChromeVisible = shellState.IsStatusChromeVisible;
        IsMiniMapVisible = shellState.IsMiniMapVisible;
        IsHostPaneOpen = shellState.IsHostPaneOpen;
        var hasInitialScenario = !string.IsNullOrWhiteSpace(_shellOptions.InitialScenario);
        ReopenLastWorkspaceOnLaunch = !hasInitialScenario && (_shellOptions.RestoreLastWorkspaceOnStartup || shellState.ReopenLastWorkspaceOnStartup);
        ThemeVariantCaption = string.IsNullOrWhiteSpace(shellState.ThemeVariant) ? ThemeVariantCaption : shellState.ThemeVariant!;

        foreach (var path in shellState.RecentWorkspacePaths.Where(File.Exists))
        {
            _recentWorkspacePaths.Add(path);
        }

        HasAutosaveDraft = !hasInitialScenario && _shellOptions.EnableStatePersistence && _shellStateStore.HasAutosaveDraft();

        if (ReopenLastWorkspaceOnLaunch && !string.IsNullOrWhiteSpace(shellState.LastWorkspacePath) && File.Exists(shellState.LastWorkspacePath))
        {
            SetActiveWorkspacePath(shellState.LastWorkspacePath);
            Editor.LoadWorkspace();
            RegisterRecentWorkspace(shellState.LastWorkspacePath);
            _pendingViewportRestore = shellState.Viewport;
        }
        else
        {
            ActiveWorkspacePath = _workspaceService.WorkspacePath;
            _pendingViewportRestore = shellState.Viewport;
        }

        RefreshRuntimeProjection();
    }

    private void PersistShellState()
    {
        if (!_shellOptions.EnableStatePersistence)
        {
            return;
        }

        _shellStateStore.Save(new DemoShellState(
            LastWorkspacePath: ActiveWorkspacePath,
            RecentWorkspacePaths: _recentWorkspacePaths.ToArray(),
            ReopenLastWorkspaceOnStartup: ReopenLastWorkspaceOnLaunch,
            IsHeaderChromeVisible: IsHeaderChromeVisible,
            IsLibraryChromeVisible: IsLibraryChromeVisible,
            IsInspectorChromeVisible: IsInspectorChromeVisible,
            IsStatusChromeVisible: IsStatusChromeVisible,
            IsMiniMapVisible: IsMiniMapVisible,
            IsHostPaneOpen: IsHostPaneOpen,
            WindowWidth: PreferredWindowWidth,
            WindowHeight: PreferredWindowHeight,
            Viewport: Session.Queries.GetViewportSnapshot(),
            ThemeVariant: ThemeVariantCaption));
        RefreshRuntimeProjection();
    }

    private void PersistAutosaveDraft()
    {
        if (!_shellOptions.EnableStatePersistence || !Editor.IsDirty)
        {
            return;
        }

        GraphDocumentSerializer.Save(Editor.CreateDocumentSnapshot(), AutosaveDraftPath);
        HasAutosaveDraft = true;
        PersistShellState();
    }

    private void RegisterRecentWorkspace(string workspacePath)
    {
        var existing = _recentWorkspacePaths
            .Where(path => !string.Equals(path, workspacePath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _recentWorkspacePaths.Clear();
        _recentWorkspacePaths.Add(workspacePath);
        foreach (var path in existing.Take(RecentWorkspaceLimit - 1))
        {
            _recentWorkspacePaths.Add(path);
        }

        ActiveWorkspacePath = workspacePath;
        OnPropertyChanged(nameof(RecentWorkspacePaths));
        OnPropertyChanged(nameof(RecentWorkspaceEntries));
    }

    private void SetActiveWorkspacePath(string workspacePath)
    {
        _workspaceService.SetWorkspacePath(workspacePath);
        ActiveWorkspacePath = workspacePath;
    }

    private string ResolveDefaultSaveAsWorkspacePath()
        => !string.IsNullOrWhiteSpace(_shellOptions.SaveAsWorkspacePath)
            ? _shellOptions.SaveAsWorkspacePath
            : Path.Combine(
                _shellStateStore.StorageRootPath,
                $"workspace-{DateTime.Now:yyyyMMdd-HHmmss}.json");

    private string PrimaryShortcutModifier
        => OperatingSystem.IsMacOS() ? "Cmd" : "Ctrl";

    private void ClearDirtyExitPrompt()
    {
        DirtyExitPromptPending = false;
        DirtyExitPromptMessage = string.Empty;
    }

    [RelayCommand]
    public void SaveWorkspaceAsShell()
        => SaveWorkspaceAs();

    [RelayCommand]
    public void ReopenLastWorkspaceShell()
        => ReopenLastWorkspace();

    [RelayCommand]
    public void RestoreAutosaveDraftShell()
        => RestoreAutosaveDraft();

    [RelayCommand]
    public void OpenRecentWorkspace(string workspacePath)
        => TryOpenWorkspacePath(workspacePath);

    partial void OnIsMiniMapVisibleChanged(bool value)
    {
        ApplyHostOptions();
        PersistShellState();
    }

    partial void OnReopenLastWorkspaceOnLaunchChanged(bool value)
        => PersistShellState();
}
