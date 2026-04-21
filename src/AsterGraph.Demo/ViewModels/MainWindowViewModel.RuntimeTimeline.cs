using AsterGraph.Editor.Events;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private const int RuntimeCommandTimelineLimit = 12;
    private readonly List<RuntimeCommandTimelineEntry> _runtimeCommandTimeline = [];

    private IReadOnlyList<RuntimeCommandTimelineEntry> RuntimeCommandTimelineEntries
        => _runtimeCommandTimeline;

    private void TrackCommandExecuted(GraphEditorCommandExecutedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        _runtimeCommandTimeline.Insert(0, new RuntimeCommandTimelineEntry(
            args.CommandId,
            args.MutationLabel,
            args.IsInMutationScope,
            args.StatusMessage));

        if (_runtimeCommandTimeline.Count > RuntimeCommandTimelineLimit)
        {
            _runtimeCommandTimeline.RemoveAt(_runtimeCommandTimeline.Count - 1);
        }
    }

    private sealed record RuntimeCommandTimelineEntry(
        string CommandId,
        string? MutationLabel,
        bool IsInMutationScope,
        string? StatusMessage);
}
