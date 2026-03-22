using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

public sealed record PortStyleOverride(
    PortTypeId TypeId,
    PortStyleOptions Style);
