using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorHistoryService
{
    private readonly List<GraphEditorHistoryState> _states = [];
    private int _index = -1;

    public bool CanUndo => _index > 0;

    public bool CanRedo => _index >= 0 && _index < (_states.Count - 1);

    public void Reset(GraphEditorHistoryState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _states.Clear();
        _states.Add(state);
        _index = 0;
    }

    public void Push(GraphEditorHistoryState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (_index >= 0 && string.Equals(_states[_index].Signature, state.Signature, StringComparison.Ordinal))
        {
            _states[_index] = state;
            return;
        }

        if (CanRedo)
        {
            _states.RemoveRange(_index + 1, _states.Count - (_index + 1));
        }

        _states.Add(state);
        _index = _states.Count - 1;
    }

    public void ReplaceCurrent(GraphEditorHistoryState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (_index < 0)
        {
            Reset(state);
            return;
        }

        _states[_index] = state;
    }

    public bool TryUndo(out GraphEditorHistoryState? state)
    {
        if (!CanUndo)
        {
            state = null;
            return false;
        }

        _index--;
        state = _states[_index];
        return true;
    }

    public bool TryRedo(out GraphEditorHistoryState? state)
    {
        if (!CanRedo)
        {
            state = null;
            return false;
        }

        _index++;
        state = _states[_index];
        return true;
    }
}

internal sealed record GraphEditorHistoryState(
    GraphDocument Document,
    IReadOnlyList<string> SelectedNodeIds,
    string? PrimarySelectedNodeId,
    string Signature);
