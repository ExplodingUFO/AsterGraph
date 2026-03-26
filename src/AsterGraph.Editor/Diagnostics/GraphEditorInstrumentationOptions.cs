using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 定义宿主可选接入的日志与跟踪配置。
/// </summary>
public sealed record GraphEditorInstrumentationOptions
{
    /// <summary>
    /// 初始化可选的日志与跟踪配置。
    /// </summary>
    /// <param name="loggerFactory">宿主日志工厂。</param>
    /// <param name="activitySource">宿主跟踪源。</param>
    public GraphEditorInstrumentationOptions(
        ILoggerFactory? loggerFactory = null,
        ActivitySource? activitySource = null)
    {
        LoggerFactory = loggerFactory;
        ActivitySource = activitySource;
    }

    /// <summary>
    /// 宿主提供的日志工厂。
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; }

    /// <summary>
    /// 宿主提供的活动跟踪源。
    /// </summary>
    public ActivitySource? ActivitySource { get; }
}
