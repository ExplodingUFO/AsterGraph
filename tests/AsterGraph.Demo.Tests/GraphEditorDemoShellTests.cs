using System.Windows.Input;
using System.IO;
using Avalonia.Headless.XUnit;
using AsterGraph.Demo.ViewModels;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class GraphEditorDemoShellTests
{
    [Fact]
    public void MainWindowViewModel_OpensHostPaneWithoutReplacingEditorSession()
    {
        var viewModel = new MainWindowViewModel();
        var originalEditor = viewModel.Editor;

        var command = Assert.IsAssignableFrom<ICommand>(
            viewModel.GetType().GetProperty("OpenHostMenuGroupCommand")?.GetValue(viewModel));

        command.Execute("展示");

        var isHostPaneOpen = Assert.IsType<bool>(
            viewModel.GetType().GetProperty("IsHostPaneOpen")?.GetValue(viewModel));
        var selectedTitle = Assert.IsType<string>(
            viewModel.GetType().GetProperty("SelectedHostMenuGroupTitle")?.GetValue(viewModel));

        Assert.True(isHostPaneOpen);
        Assert.Equal("展示", selectedTitle);
        Assert.Same(originalEditor, viewModel.Editor);
    }

    [Fact]
    public void MainWindowViewModel_KeepsSameEditorAcrossOperationalHostGroups()
    {
        var viewModel = new MainWindowViewModel();
        var originalEditor = viewModel.Editor;

        var command = Assert.IsAssignableFrom<ICommand>(
            viewModel.GetType().GetProperty("OpenHostMenuGroupCommand")?.GetValue(viewModel));

        foreach (var group in new[] { "视图", "行为", "运行时" })
        {
            command.Execute(group);

            var selectedTitle = Assert.IsType<string>(
                viewModel.GetType().GetProperty("SelectedHostMenuGroupTitle")?.GetValue(viewModel));

            Assert.Equal(group, selectedTitle);
            Assert.Same(originalEditor, viewModel.Editor);
        }
    }

    [Fact]
    public void MainWindowViewModel_StartsWithHostPaneClosed()
    {
        var viewModel = new MainWindowViewModel();

        var isHostPaneOpen = Assert.IsType<bool>(
            viewModel.GetType().GetProperty("IsHostPaneOpen")?.GetValue(viewModel));

        Assert.False(isHostPaneOpen);
    }

    [AvaloniaFact]
    public void MainWindowViewModel_ExposesHostMenuProofCaptions()
    {
        var viewModel = new MainWindowViewModel();

        var paneState = Assert.IsType<string>(
            viewModel.GetType().GetProperty("HostPaneStateCaption")?.GetValue(viewModel));
        var sessionCaption = Assert.IsType<string>(
            viewModel.GetType().GetProperty("HostSessionContinuityCaption")?.GetValue(viewModel));

        Assert.False(string.IsNullOrWhiteSpace(paneState));
        Assert.Contains("会话", sessionCaption);
    }

    [Fact]
    public void MainWindowViewModel_ExposesPhase21ProofCueProperties()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal(
            "宿主控制抽屉",
            Assert.IsType<string>(viewModel.GetType().GetProperty("HostDrawerCaption")?.GetValue(viewModel)));
        Assert.Equal(
            "实时 SDK 会话",
            Assert.IsType<string>(viewModel.GetType().GetProperty("LiveSessionTitle")?.GetValue(viewModel)));
        Assert.Equal(
            "宿主控制",
            Assert.IsType<string>(viewModel.GetType().GetProperty("HostOwnershipBadgeText")?.GetValue(viewModel)));
        Assert.Equal(
            "共享运行时",
            Assert.IsType<string>(viewModel.GetType().GetProperty("RuntimeOwnershipBadgeText")?.GetValue(viewModel)));
        Assert.Equal(
            "当前分组 · 展示",
            Assert.IsType<string>(viewModel.GetType().GetProperty("ActiveHostGroupBadgeText")?.GetValue(viewModel)));

        viewModel.OpenHostMenuGroup("证明");

        Assert.Equal(
            "当前分组 · 证明",
            Assert.IsType<string>(viewModel.GetType().GetProperty("ActiveHostGroupBadgeText")?.GetValue(viewModel)));
    }

    [Fact]
    public void MainWindowViewModel_SaveAsTracksRecentWorkspaceAndReopensLastWorkspace()
    {
        var storageRoot = CreateTempDirectory();
        var saveAsPath = Path.Combine(storageRoot, "saved-as.json");
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: storageRoot,
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false));

        var originalNodeCount = viewModel.Editor.Nodes.Count;

        viewModel.SaveWorkspaceAs(saveAsPath);
        viewModel.Editor.Session.Commands.AddNode(viewModel.Editor.NodeTemplates[0].DefinitionId);

        var changedNodeCount = viewModel.Editor.Nodes.Count;
        Assert.True(changedNodeCount > originalNodeCount);

        Assert.True(viewModel.TryOpenWorkspacePath(saveAsPath));
        Assert.Equal(originalNodeCount, viewModel.Editor.Nodes.Count);
        Assert.Equal(saveAsPath, viewModel.ActiveWorkspacePath);
        Assert.Contains(saveAsPath, viewModel.RecentWorkspacePaths);

        viewModel.Editor.Session.Commands.AddNode(viewModel.Editor.NodeTemplates[0].DefinitionId);
        Assert.True(viewModel.ReopenLastWorkspace());
        Assert.Equal(originalNodeCount, viewModel.Editor.Nodes.Count);
    }

    [Fact]
    public void MainWindowViewModel_PersistsCrashSafeAutosaveDraftAcrossRestart()
    {
        var storageRoot = CreateTempDirectory();
        var options = new MainWindowShellOptions(
            StorageRootPath: storageRoot,
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false);

        var initialNodeCount = 0;
        var autosavedNodeCount = 0;
        using (var firstLifetime = new DemoShellViewModelLifetime(options))
        {
            initialNodeCount = firstLifetime.ViewModel.Editor.Nodes.Count;
            firstLifetime.ViewModel.Editor.Session.Commands.AddNode(firstLifetime.ViewModel.Editor.NodeTemplates[0].DefinitionId);
            autosavedNodeCount = firstLifetime.ViewModel.Editor.Nodes.Count;

            Assert.True(firstLifetime.ViewModel.HasAutosaveDraft);
            Assert.True(File.Exists(firstLifetime.ViewModel.AutosaveDraftPath));
        }

        var secondViewModel = new MainWindowViewModel(options);
        Assert.True(secondViewModel.HasAutosaveDraft);
        Assert.Equal(initialNodeCount, secondViewModel.Editor.Nodes.Count);

        Assert.True(secondViewModel.RestoreAutosaveDraft());
        Assert.Equal(autosavedNodeCount, secondViewModel.Editor.Nodes.Count);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class DemoShellViewModelLifetime : IDisposable
    {
        public DemoShellViewModelLifetime(MainWindowShellOptions options)
        {
            ViewModel = new MainWindowViewModel(options);
        }

        public MainWindowViewModel ViewModel { get; }

        public void Dispose()
            => GC.KeepAlive(ViewModel);
    }
}
