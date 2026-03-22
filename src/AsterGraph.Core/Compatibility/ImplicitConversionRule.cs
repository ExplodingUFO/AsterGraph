using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Compatibility;

/// <summary>
/// Declares one safe implicit conversion.
/// Keep this list intentionally small so connections stay predictable.
/// </summary>
public sealed record ImplicitConversionRule(
    PortTypeId SourceType,
    PortTypeId TargetType,
    ConversionId ConversionId);
