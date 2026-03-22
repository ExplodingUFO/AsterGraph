using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Compatibility;

/// <summary>
/// Evaluates whether an output port type can connect to an input port type.
/// </summary>
public interface IPortCompatibilityService
{
    PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType);
}

public enum PortCompatibilityKind
{
    Rejected = 0,
    Exact = 1,
    ImplicitConversion = 2
}

/// <summary>
/// Value object describing compatibility and optional conversion metadata.
/// </summary>
public readonly record struct PortCompatibilityResult
{
    private PortCompatibilityResult(PortCompatibilityKind kind, ConversionId? conversionId)
    {
        Kind = kind;
        ConversionId = conversionId;
    }

    public PortCompatibilityKind Kind { get; }

    public ConversionId? ConversionId { get; }

    public bool IsCompatible => Kind is PortCompatibilityKind.Exact or PortCompatibilityKind.ImplicitConversion;

    public static PortCompatibilityResult Rejected() => new(PortCompatibilityKind.Rejected, null);

    public static PortCompatibilityResult Exact() => new(PortCompatibilityKind.Exact, null);

    public static PortCompatibilityResult ImplicitConversion(ConversionId conversionId) =>
        new(PortCompatibilityKind.ImplicitConversion, conversionId);
}
