using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorDocumentProjectionApplier
{
    private readonly Action<NodeViewModel> _applyNodePresentation;

    public GraphEditorDocumentProjectionApplier(Action<NodeViewModel> applyNodePresentation)
    {
        _applyNodePresentation = applyNodePresentation ?? throw new ArgumentNullException(nameof(applyNodePresentation));
    }

    public GraphEditorDocumentProjectionResult Project(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var nodes = document.Nodes
            .Select(node =>
            {
                var viewModel = new NodeViewModel(node);
                _applyNodePresentation(viewModel);
                return viewModel;
            })
            .ToList();

        var connections = document.Connections
            .Select(connection => new ConnectionViewModel(
                connection.Id,
                connection.SourceNodeId,
                connection.SourcePortId,
                connection.TargetNodeId,
                connection.TargetPortId,
                connection.Label,
                connection.AccentHex,
                connection.ConversionId))
            .ToList();

        return new GraphEditorDocumentProjectionResult(nodes, connections);
    }
}

internal sealed record GraphEditorDocumentProjectionResult(
    IReadOnlyList<NodeViewModel> Nodes,
    IReadOnlyList<ConnectionViewModel> Connections);
