using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Automation;

/// <summary>
/// 表示一个自动化步骤。
/// </summary>
public sealed record GraphEditorAutomationStep
{
    /// <summary>
    /// 初始化自动化步骤。
    /// </summary>
    /// <param name="stepId">稳定步骤标识。</param>
    /// <param name="command">稳定命令调用描述。</param>
    public GraphEditorAutomationStep(string stepId, GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        ArgumentNullException.ThrowIfNull(command);

        StepId = stepId.Trim();
        Command = command;
    }

    /// <summary>
    /// 稳定步骤标识。
    /// </summary>
    public string StepId { get; }

    /// <summary>
    /// 步骤对应的稳定命令调用描述。
    /// </summary>
    public GraphEditorCommandInvocationSnapshot Command { get; }
}
