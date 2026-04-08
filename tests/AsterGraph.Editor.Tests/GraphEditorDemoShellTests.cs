using System.Windows.Input;
using Avalonia.Headless.XUnit;
using AsterGraph.Demo.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

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
}
