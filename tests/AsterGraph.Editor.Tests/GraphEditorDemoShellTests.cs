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
}
