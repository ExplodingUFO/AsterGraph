using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Compatibility;

/// <summary>
/// Evaluates whether an output port type can connect to an input port type.
/// </summary>
public interface IPortCompatibilityService
{
    /// <summary>
    /// 评估源端口类型到目标端口类型的兼容性。
    /// </summary>
    /// <param name="sourceType">源端口类型。</param>
    /// <param name="targetType">目标端口类型。</param>
    /// <returns>兼容性评估结果。</returns>
    PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType);
}

/// <summary>
/// 端口兼容性结果类型。
/// </summary>
public enum PortCompatibilityKind
{
    /// <summary>
    /// 不兼容。
    /// </summary>
    Rejected = 0,
    /// <summary>
    /// 完全匹配。
    /// </summary>
    Exact = 1,
    /// <summary>
    /// 允许通过安全隐式转换连接。
    /// </summary>
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

    /// <summary>
    /// 兼容性类型。
    /// </summary>
    public PortCompatibilityKind Kind { get; }

    /// <summary>
    /// 对应的隐式转换标识；非隐式转换时为空。
    /// </summary>
    public ConversionId? ConversionId { get; }

    /// <summary>
    /// 是否允许连接。
    /// </summary>
    public bool IsCompatible => Kind is PortCompatibilityKind.Exact or PortCompatibilityKind.ImplicitConversion;

    /// <summary>
    /// 创建不兼容结果。
    /// </summary>
    public static PortCompatibilityResult Rejected() => new(PortCompatibilityKind.Rejected, null);

    /// <summary>
    /// 创建完全匹配结果。
    /// </summary>
    public static PortCompatibilityResult Exact() => new(PortCompatibilityKind.Exact, null);

    /// <summary>
    /// 创建带隐式转换的兼容结果。
    /// </summary>
    /// <param name="conversionId">隐式转换标识。</param>
    public static PortCompatibilityResult ImplicitConversion(ConversionId conversionId) =>
        new(PortCompatibilityKind.ImplicitConversion, conversionId);
}
