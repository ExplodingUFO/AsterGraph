using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

public sealed record ConnectionStyleOverride(
    ConversionId ConversionId,
    ConnectionStyleOptions Style);
