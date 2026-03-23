using AsterGraph.Editor.Services;
using CommunityToolkit.Mvvm.Input;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorCommandStateNotifierTests
{
    [Fact]
    public void NotifyCanExecuteChanged_FansOutAcrossCommands_AndIgnoresNullEntries()
    {
        var notifier = new GraphEditorCommandStateNotifier();
        var firstRaised = 0;
        var secondRaised = 0;
        var first = new RelayCommand(() => { });
        var second = new RelayCommand(() => { });
        first.CanExecuteChanged += (_, _) => firstRaised++;
        second.CanExecuteChanged += (_, _) => secondRaised++;

        notifier.NotifyCanExecuteChanged([first, null!, second]);

        Assert.Equal(1, firstRaised);
        Assert.Equal(1, secondRaised);
    }

    [Fact]
    public void NotifyPropertyChanged_FansOutAcrossProperties_AndSkipsEmptyNames()
    {
        var notifier = new GraphEditorCommandStateNotifier();
        var names = new List<string>();

        notifier.NotifyPropertyChanged(
            names.Add,
            ["Zoom", "", "PanX", "  ", null!, "PanY"]);

        Assert.Equal(["Zoom", "PanX", "PanY"], names);
    }
}
