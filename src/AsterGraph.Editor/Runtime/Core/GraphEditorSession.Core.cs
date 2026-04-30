using AsterGraph.Editor;
using AsterGraph.Editor.Runtime.Internal;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 默认的图编辑器运行时会话实现（运行时会话核心）。
/// </summary>
public sealed partial class GraphEditorSession :
    IGraphEditorSession,
    IGraphEditorAutomationRunner,
    IGraphEditorCommands,
    IGraphEditorQueries,
    IGraphEditorEvents,
    IGraphEditorDiagnostics
{
    private readonly IGraphEditorSessionHost _host;
    private readonly IGraphEditorDiagnosticsSink? _diagnosticsSink;
    private readonly GraphEditorSessionDescriptorSupport? _descriptorSupport;
    private readonly GraphEditorSessionStockMenuDescriptorBuilder _stockMenuDescriptorBuilder;
    private readonly GraphEditorSessionStockToolDescriptorBuilder _stockToolDescriptorBuilder;
    private readonly List<GraphEditorViewportBookmarkSnapshot> _viewportBookmarks = [];
    private int _documentRevision;

    public GraphEditorSession(ViewModels.GraphEditorViewModel editor, IGraphEditorDiagnosticsSink? diagnosticsSink = null)
        : this(editor.SessionHost, diagnosticsSink, editor.CreateSessionDescriptorSupport())
    {
    }

    internal GraphEditorSession(
        IGraphEditorSessionHost host,
        IGraphEditorDiagnosticsSink? diagnosticsSink = null,
        GraphEditorSessionDescriptorSupport? descriptorSupport = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _diagnosticsSink = diagnosticsSink;
        _descriptorSupport = descriptorSupport;
        _stockMenuDescriptorBuilder = new GraphEditorSessionStockMenuDescriptorBuilder(
            _host.CreateDocumentSnapshot,
            _host.CreateActiveScopeDocumentSnapshot,
            _host.GetScopeNavigationSnapshot,
            _host.GetSelectionSnapshot,
            _host.GetEdgeTemplateSnapshots,
            Localize,
            () => _descriptorSupport?.Definitions ?? Array.Empty<global::AsterGraph.Abstractions.Definitions.INodeDefinition>());
        _stockToolDescriptorBuilder = new GraphEditorSessionStockToolDescriptorBuilder(
            _host.CreateActiveScopeDocumentSnapshot,
            _host.GetSelectionSnapshot,
            _host.GetNodeSurfaceSnapshots,
            Localize);

        _lastPendingConnectionSnapshot = CreatePendingConnectionSnapshot();

        _host.DocumentChanged += HandleDocumentChanged;
        _host.SelectionChanged += HandleSelectionChanged;
        _host.ViewportChanged += HandleViewportChanged;
        _host.FragmentExported += HandleFragmentExported;
        _host.FragmentImported += HandleFragmentImported;
        _host.PendingConnectionChanged += HandlePendingConnectionChanged;
        _host.RecoverableFailureRaised += HandleRecoverableFailureRaised;
        _host.DiagnosticPublished += HandleDiagnosticPublished;
    }

    public IGraphEditorAutomationRunner Automation => this;

    public IGraphEditorCommands Commands => this;

    public IGraphEditorQueries Queries => this;

    public IGraphEditorEvents Events => this;

    public IGraphEditorDiagnostics Diagnostics => this;

    internal void SetRuntimeOverlayProvider(IGraphRuntimeOverlayProvider? runtimeOverlayProvider)
        => _descriptorSupport?.SetRuntimeOverlayProvider(runtimeOverlayProvider);

    internal void SetLayoutProvider(IGraphLayoutProvider? layoutProvider)
        => _descriptorSupport?.SetLayoutProvider(layoutProvider);

    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;

    public event EventHandler<GraphEditorAutomationStartedEventArgs>? AutomationStarted;

    public event EventHandler<GraphEditorAutomationProgressEventArgs>? AutomationProgress;

    public event EventHandler<GraphEditorAutomationCompletedEventArgs>? AutomationCompleted;

    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;

    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;

    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;

    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;

    public event EventHandler<GraphEditorCommandExecutedEventArgs>? CommandExecuted;

    public event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;

    public event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailure;
}
