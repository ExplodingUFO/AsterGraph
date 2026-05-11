using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Standalone node toolbar control that renders canonical hosted node actions for one graph node.
/// </summary>
public sealed class NodeToolbar : UserControl
{
    /// <summary>
    /// Bound canonical editor session.
    /// </summary>
    public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
        AvaloniaProperty.Register<NodeToolbar, IGraphEditorSession?>(nameof(Session));

    /// <summary>
    /// Bound node identifier.
    /// </summary>
    public static readonly StyledProperty<string?> NodeIdProperty =
        AvaloniaProperty.Register<NodeToolbar, string?>(nameof(NodeId));

    /// <summary>
    /// Optional selected node identifiers forwarded to contextual action projection.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<string>?> SelectedNodeIdsProperty =
        AvaloniaProperty.Register<NodeToolbar, IReadOnlyList<string>?>(nameof(SelectedNodeIds));

    /// <summary>
    /// Optional primary selected node identifier forwarded to contextual action projection.
    /// </summary>
    public static readonly StyledProperty<string?> PrimarySelectedNodeIdProperty =
        AvaloniaProperty.Register<NodeToolbar, string?>(nameof(PrimarySelectedNodeId));

    private readonly WrapPanel _itemsHost = new()
    {
        Orientation = Orientation.Horizontal,
        ItemHeight = 36,
        ItemWidth = double.NaN,
    };
    private IGraphEditorSession? _observedSession;

    /// <summary>
    /// Initializes a standalone node toolbar.
    /// </summary>
    public NodeToolbar()
    {
        Focusable = false;
        Content = _itemsHost;
        RefreshActions();
    }

    /// <summary>
    /// Current canonical editor session.
    /// </summary>
    public IGraphEditorSession? Session
    {
        get => GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    /// <summary>
    /// Current node identifier.
    /// </summary>
    public string? NodeId
    {
        get => GetValue(NodeIdProperty);
        set => SetValue(NodeIdProperty, value);
    }

    /// <summary>
    /// Current selected node identifiers used for contextual command descriptors.
    /// </summary>
    public IReadOnlyList<string>? SelectedNodeIds
    {
        get => GetValue(SelectedNodeIdsProperty);
        set => SetValue(SelectedNodeIdsProperty, value);
    }

    /// <summary>
    /// Current primary selected node identifier used for contextual command descriptors.
    /// </summary>
    public string? PrimarySelectedNodeId
    {
        get => GetValue(PrimarySelectedNodeIdProperty);
        set => SetValue(PrimarySelectedNodeIdProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachSession(Session);
        RefreshActions();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        AttachSession(null);
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SessionProperty)
        {
            AttachSession(change.GetNewValue<IGraphEditorSession?>());
        }

        if (change.Property == SessionProperty
            || change.Property == NodeIdProperty
            || change.Property == SelectedNodeIdsProperty
            || change.Property == PrimarySelectedNodeIdProperty)
        {
            RefreshActions();
        }
    }

    private void AttachSession(IGraphEditorSession? session)
    {
        if (ReferenceEquals(_observedSession, session))
        {
            return;
        }

        if (_observedSession is not null)
        {
            _observedSession.Events.DocumentChanged -= HandleSessionChanged;
            _observedSession.Events.SelectionChanged -= HandleSessionChanged;
        }

        _observedSession = session;
        if (_observedSession is not null)
        {
            _observedSession.Events.DocumentChanged += HandleSessionChanged;
            _observedSession.Events.SelectionChanged += HandleSessionChanged;
        }
    }

    private void HandleSessionChanged(object? sender, EventArgs args)
        => RefreshActions();

    private void RefreshActions()
    {
        _itemsHost.Children.Clear();
        if (Session is null || string.IsNullOrWhiteSpace(NodeId))
        {
            return;
        }

        foreach (var action in AsterGraphAuthoringToolActionFactory.CreateNodeActions(
            Session,
            NodeId,
            SelectedNodeIds,
            PrimarySelectedNodeId))
        {
            _itemsHost.Children.Add(HostedActionToolbarButtonFactory.Create(ResolveButtonName(action), action));
        }
    }

    private static string ResolveButtonName(AsterGraphHostedActionDescriptor action)
        => action.Id switch
        {
            "node-inspect" => "PART_NodeToolbarInspectButton",
            "node-center" => "PART_NodeToolbarCenterButton",
            "node-toggle-surface-expansion" => "PART_NodeToolbarToggleExpansionButton",
            "node-delete" => "PART_NodeToolbarDeleteButton",
            "node-duplicate" => "PART_NodeToolbarDuplicateButton",
            "node-disconnect-incoming" => "PART_NodeToolbarDisconnectIncomingButton",
            "node-disconnect-outgoing" => "PART_NodeToolbarDisconnectOutgoingButton",
            "node-disconnect-all" => "PART_NodeToolbarDisconnectAllButton",
            "node-enter-composite-scope" => "PART_NodeToolbarEnterCompositeScopeButton",
            _ => $"PART_NodeToolbar_{action.Id}",
        };
}
