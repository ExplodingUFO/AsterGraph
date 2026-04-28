using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

internal sealed class DemoAiPipelineMockRuntimeProvider : IGraphRuntimeOverlayProvider
{
    private IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> _nodeOverlays = [];
    private IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> _connectionOverlays = [];
    private IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> _recentLogs = [];

    public IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> GetNodeOverlays()
        => _nodeOverlays;

    public IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> GetConnectionOverlays()
        => _connectionOverlays;

    public IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> GetRecentLogs()
        => _recentLogs;

    public void RunSuccess()
    {
        var timestamp = DateTimeOffset.UtcNow;
        _nodeOverlays =
        [
            CreateNode("input", GraphEditorRuntimeOverlayStatus.Success, "Input: release request", 8, timestamp),
            CreateNode("prompt", GraphEditorRuntimeOverlayStatus.Success, "Prompt assembled", 18, timestamp),
            CreateNode("retriever", GraphEditorRuntimeOverlayStatus.Success, "2 trusted snippets", 31, timestamp),
            CreateNode("llm", GraphEditorRuntimeOverlayStatus.Success, "Model response ready", 146, timestamp),
            CreateNode("parser", GraphEditorRuntimeOverlayStatus.Success, "Typed payload", 24, timestamp),
            CreateNode("output", GraphEditorRuntimeOverlayStatus.Success, "Decision: approve", 6, timestamp),
        ];
        _connectionOverlays =
        [
            CreateConnection("input.text->prompt.context", "release request", "text", 1),
            CreateConnection("prompt.prompt->retriever.query", "retrieve policy evidence", "prompt", 1),
            CreateConnection("prompt.prompt->llm.prompt", "assembled prompt", "prompt", 1),
            CreateConnection("retriever.evidence->llm.context", "2 snippets", "evidence", 2),
            CreateConnection("llm.response->parser.response", "raw model response", "llm.response", 1),
            CreateConnection("parser.payload->output.payload", "typed approval payload", "json", 1),
        ];
        _recentLogs =
        [
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-run-started", timestamp, GraphEditorRuntimeOverlayStatus.Running, "AI pipeline mock run started.", ScopeId: "root", NodeId: "input"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-input-ready", timestamp, GraphEditorRuntimeOverlayStatus.Success, "Input payload accepted.", ScopeId: "root", NodeId: "input", ConnectionId: "input.text->prompt.context"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-prompt-ready", timestamp, GraphEditorRuntimeOverlayStatus.Success, "Prompt assembled with release policy context.", ScopeId: "root", NodeId: "prompt", ConnectionId: "prompt.prompt->llm.prompt"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-llm-ready", timestamp, GraphEditorRuntimeOverlayStatus.Success, "LLM returned a typed approval response.", ScopeId: "root", NodeId: "llm", ConnectionId: "llm.response->parser.response"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-run-completed", timestamp, GraphEditorRuntimeOverlayStatus.Success, "AI pipeline mock run completed.", ScopeId: "root", NodeId: "output", ConnectionId: "parser.payload->output.payload"),
        ];
    }

    public void RunError()
    {
        var timestamp = DateTimeOffset.UtcNow;
        _nodeOverlays =
        [
            CreateNode("input", GraphEditorRuntimeOverlayStatus.Success, "Input: release request", 8, timestamp),
            CreateNode("prompt", GraphEditorRuntimeOverlayStatus.Success, "Prompt assembled", 18, timestamp),
            CreateNode("retriever", GraphEditorRuntimeOverlayStatus.Success, "2 trusted snippets", 31, timestamp),
            CreateNode("llm", GraphEditorRuntimeOverlayStatus.Error, null, 96, timestamp, "Mock LLM timeout."),
            CreateNode("parser", GraphEditorRuntimeOverlayStatus.Warning, "Waiting for response", 0, timestamp, warningCount: 1),
            CreateNode("output", GraphEditorRuntimeOverlayStatus.Idle, null, 0, timestamp),
        ];
        _connectionOverlays =
        [
            CreateConnection("input.text->prompt.context", "release request", "text", 1),
            CreateConnection("prompt.prompt->retriever.query", "retrieve policy evidence", "prompt", 1),
            CreateConnection("prompt.prompt->llm.prompt", "assembled prompt", "prompt", 1),
            CreateConnection("retriever.evidence->llm.context", "2 snippets", "evidence", 2),
            new GraphEditorConnectionRuntimeOverlaySnapshot("llm.response->parser.response", GraphEditorRuntimeOverlayStatus.Error, ValuePreview: "timeout before parser", PayloadType: "llm.response", ItemCount: 0, IsStale: true),
            new GraphEditorConnectionRuntimeOverlaySnapshot("parser.payload->output.payload", GraphEditorRuntimeOverlayStatus.Warning, ValuePreview: "stale empty payload", PayloadType: "json", ItemCount: 0, IsStale: true),
        ];
        _recentLogs =
        [
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-run-started", timestamp, GraphEditorRuntimeOverlayStatus.Running, "AI pipeline mock run started.", ScopeId: "root", NodeId: "input"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-llm-running", timestamp, GraphEditorRuntimeOverlayStatus.Running, "LLM call in progress.", ScopeId: "root", NodeId: "llm", ConnectionId: "prompt.prompt->llm.prompt"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-run-error", timestamp, GraphEditorRuntimeOverlayStatus.Error, "Mock LLM timeout.", ScopeId: "root", NodeId: "llm", ConnectionId: "llm.response->parser.response"),
            new GraphEditorRuntimeLogEntrySnapshot("ai-pipeline-parser-stale", timestamp, GraphEditorRuntimeOverlayStatus.Warning, "Parser output is stale because the LLM response failed.", ScopeId: "root", NodeId: "parser", ConnectionId: "parser.payload->output.payload"),
        ];
    }

    private static GraphEditorNodeRuntimeOverlaySnapshot CreateNode(
        string nodeId,
        GraphEditorRuntimeOverlayStatus status,
        string? outputPreview,
        double elapsedMs,
        DateTimeOffset timestamp,
        string? errorMessage = null,
        int warningCount = 0)
        => new(
            nodeId,
            status,
            ElapsedMilliseconds: elapsedMs,
            OutputPreview: outputPreview,
            WarningCount: warningCount,
            ErrorCount: status == GraphEditorRuntimeOverlayStatus.Error ? 1 : 0,
            ErrorMessage: errorMessage,
            LastRunAtUtc: timestamp);

    private static GraphEditorConnectionRuntimeOverlaySnapshot CreateConnection(
        string connectionId,
        string valuePreview,
        string payloadType,
        int itemCount)
        => new(
            connectionId,
            GraphEditorRuntimeOverlayStatus.Success,
            ValuePreview: valuePreview,
            PayloadType: payloadType,
            ItemCount: itemCount);
}
