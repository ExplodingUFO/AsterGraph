using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

public sealed record GraphConnection(
    string Id,
    string SourceNodeId,
    string SourcePortId,
    string TargetNodeId,
    string TargetPortId,
    string Label,
    string AccentHex,
    ConversionId? ConversionId = null);
