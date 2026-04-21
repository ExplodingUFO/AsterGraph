using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Kernel.Internal.Layout;

internal sealed record NodeSelectionLayoutInput(
    string NodeId,
    GraphPoint Position,
    GraphSize Size);
