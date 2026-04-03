using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Compatibility;

/// <summary>
/// Declares one safe implicit conversion.
/// Keep this list intentionally small so connections stay predictable.
/// </summary>
/// <param name="SourceType">Source port type identifier.</param>
/// <param name="TargetType">Target port type identifier.</param>
/// <param name="ConversionId">Stable conversion identifier emitted into persisted connections.</param>
public sealed record ImplicitConversionRule(
    PortTypeId SourceType,
    PortTypeId TargetType,
    ConversionId ConversionId);
