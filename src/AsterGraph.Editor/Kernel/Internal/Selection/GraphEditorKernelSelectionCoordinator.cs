using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelSelectionCoordinator
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelSelectionCoordinator(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public void ClearSelection(bool updateStatus)
            => SetSelection([], null, updateStatus);

        public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
        {
            var existingIds = _owner.GetActiveGraphScope().Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
            var selectedIds = nodeIds
                .Where(existingIds.Contains)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var nextPrimary = !string.IsNullOrWhiteSpace(primaryNodeId) && selectedIds.Contains(primaryNodeId, StringComparer.Ordinal)
                ? primaryNodeId
                : selectedIds.LastOrDefault();

            if (_owner._selectedNodeIds.SequenceEqual(selectedIds, StringComparer.Ordinal)
                && string.Equals(_owner._primarySelectedNodeId, nextPrimary, StringComparison.Ordinal))
            {
                return;
            }

            _owner._selectedNodeIds = selectedIds;
            _owner._primarySelectedNodeId = nextPrimary;
            if (updateStatus)
            {
                _owner.CurrentStatusMessage = selectedIds.Count == 0
                    ? "Selection cleared."
                    : $"Selected {selectedIds.Count} node{(selectedIds.Count == 1 ? string.Empty : "s")}.";
            }

            _owner.SelectionChanged?.Invoke(_owner, new GraphEditorSelectionChangedEventArgs(_owner._selectedNodeIds.ToList(), _owner._primarySelectedNodeId));
        }

        public GraphEditorSelectionSnapshot GetSelectionSnapshot()
            => new(_owner._selectedNodeIds.ToList(), _owner._primarySelectedNodeId);
    }
}
