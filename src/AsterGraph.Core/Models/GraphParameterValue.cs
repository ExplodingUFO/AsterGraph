using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

public sealed record GraphParameterValue(
    string Key,
    PortTypeId TypeId,
    object? Value);
