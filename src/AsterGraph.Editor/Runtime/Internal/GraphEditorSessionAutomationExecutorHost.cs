using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime.Internal;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private sealed class GraphEditorSessionAutomationExecutorHost : IGraphEditorSessionAutomationExecutorHost
    {
        private readonly GraphEditorSession _owner;

        public GraphEditorSessionAutomationExecutorHost(GraphEditorSession owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public IGraphEditorMutationScope BeginMutation(string? label)
            => _owner.BeginMutation(label);

        public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
            => _owner.GetCommandDescriptors();

        public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
            => _owner.Commands.TryExecuteCommand(command);

        public GraphEditorInspectionSnapshot CaptureInspectionSnapshot()
            => _owner.CaptureInspectionSnapshot();

        public void PublishDiagnostic(GraphEditorDiagnostic diagnostic)
            => _owner.PublishDiagnostic(diagnostic);

        public void PublishAutomationStarted(GraphEditorAutomationStartedEventArgs args)
            => _owner.AutomationStarted?.Invoke(_owner, args);

        public void PublishAutomationProgress(GraphEditorAutomationProgressEventArgs args)
            => _owner.AutomationProgress?.Invoke(_owner, args);

        public void PublishAutomationCompleted(GraphEditorAutomationCompletedEventArgs args)
            => _owner.AutomationCompleted?.Invoke(_owner, args);
    }
}
