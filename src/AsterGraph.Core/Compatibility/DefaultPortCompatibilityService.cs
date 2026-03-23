using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Compatibility;

/// <summary>
/// Default compatibility policy:
/// exact type matches are allowed, and only a narrow set of explicit safe widening conversions are implicit.
/// </summary>
public sealed class DefaultPortCompatibilityService : IPortCompatibilityService
{
    private static readonly IReadOnlyList<ImplicitConversionRule> Rules =
    [
        new(new PortTypeId("int"), new PortTypeId("float"), new ConversionId("core.int-to-float")),
        new(new PortTypeId("int"), new PortTypeId("double"), new ConversionId("core.int-to-double")),
        new(new PortTypeId("float"), new PortTypeId("double"), new ConversionId("core.float-to-double")),
    ];

    /// <summary>
    /// 评估两个端口类型之间是否可以直接连接，或是否可通过安全隐式转换连接。
    /// </summary>
    /// <param name="sourceType">源端口类型。</param>
    /// <param name="targetType">目标端口类型。</param>
    /// <returns>兼容性评估结果。</returns>
    public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
    {
        if (sourceType.Value.Equals(targetType.Value, StringComparison.Ordinal))
        {
            return PortCompatibilityResult.Exact();
        }

        var rule = Rules.FirstOrDefault(candidate =>
            candidate.SourceType.Value.Equals(sourceType.Value, StringComparison.Ordinal)
            && candidate.TargetType.Value.Equals(targetType.Value, StringComparison.Ordinal));

        if (rule is not null)
        {
            return PortCompatibilityResult.ImplicitConversion(rule.ConversionId);
        }

        return PortCompatibilityResult.Rejected();
    }
}
