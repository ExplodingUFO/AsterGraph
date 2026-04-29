using System.ComponentModel;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Compatibility;

/// <summary>
/// Thin fluent wrapper for creating <see cref="ImplicitConversionRule" /> instances.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ImplicitConversionRuleBuilder
{
    private readonly PortTypeId _sourceType;
    private readonly PortTypeId _targetType;
    private ConversionId? _conversionId;

    private ImplicitConversionRuleBuilder(PortTypeId sourceType, PortTypeId targetType)
    {
        _sourceType = sourceType;
        _targetType = targetType;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ImplicitConversionRuleBuilder ImplicitConversion(PortTypeId sourceType, PortTypeId targetType)
        => new(sourceType, targetType);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ImplicitConversionRuleBuilder ImplicitConversion(string sourceType, string targetType)
        => new(new PortTypeId(sourceType), new PortTypeId(targetType));

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ImplicitConversionRuleBuilder Conversion(string conversionId)
    {
        _conversionId = new ConversionId(conversionId);
        return this;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ImplicitConversionRuleBuilder Conversion(ConversionId conversionId)
    {
        _conversionId = conversionId;
        return this;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ImplicitConversionRule Build()
        => new(
            _sourceType,
            _targetType,
            _conversionId ?? new ConversionId($"{_sourceType.Value}-to-{_targetType.Value}"));
}