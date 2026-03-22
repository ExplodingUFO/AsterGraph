using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

public sealed record GraphPort(
    string Id,
    string Label,
    PortDirection Direction,
    string DataType,
    string AccentHex,
    PortTypeId? TypeId = null);
