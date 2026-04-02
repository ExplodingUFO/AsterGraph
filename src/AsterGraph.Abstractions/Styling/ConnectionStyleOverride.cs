using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Overrides connection styling for a specific implicit conversion.
/// </summary>
/// <param name="ConversionId">Stable implicit-conversion identifier to match.</param>
/// <param name="Style">Replacement style tokens for that conversion.</param>
public sealed record ConnectionStyleOverride(
    ConversionId ConversionId,
    ConnectionStyleOptions Style);
