using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Standalone edge toolbar control that renders canonical hosted connection actions for one graph edge.
/// </summary>
public sealed class EdgeToolbar : UserControl
{
    /// <summary>
    /// Bound canonical editor session.
    /// </summary>
    public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
        AvaloniaProperty.Register<EdgeToolbar, IGraphEditorSession?>(nameof(Session));

    /// <summary>
    /// Bound connection identifier.
    /// </summary>
    public static readonly StyledProperty<string?> ConnectionIdProperty =
        AvaloniaProperty.Register<EdgeToolbar, string?>(nameof(ConnectionId));

    /// <summary>
    /// Optional selected node identifiers forwarded to contextual action projection.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<string>?> SelectedNodeIdsProperty =
        AvaloniaProperty.Register<EdgeToolbar, IReadOnlyList<string>?>(nameof(SelectedNodeIds));

    /// <summary>
    /// Optional primary selected node identifier forwarded to contextual action projection.
    /// </summary>
    public static readonly StyledProperty<string?> PrimarySelectedNodeIdProperty =
        AvaloniaProperty.Register<EdgeToolbar, string?>(nameof(PrimarySelectedNodeId));

    private readonly WrapPanel _itemsHost = new()
    {
        Orientation = Orientation.Horizontal,
        ItemHeight = 36,
        ItemWidth = double.NaN,
    };
    private IGraphEditorSession? _observedSession;

    /// <summary>
    /// Initializes a standalone edge toolbar.
    /// </summary>
    public EdgeToolbar()
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
    /// Current connection identifier.
    /// </summary>
    public string? ConnectionId
    {
        get => GetValue(ConnectionIdProperty);
        set => SetValue(ConnectionIdProperty, value);
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
            || change.Property == ConnectionIdProperty
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
        if (Session is null || string.IsNullOrWhiteSpace(ConnectionId))
        {
            return;
        }

        foreach (var action in AsterGraphAuthoringToolActionFactory.CreateConnectionActions(
            Session,
            ConnectionId,
            SelectedNodeIds,
            PrimarySelectedNodeId))
        {
            _itemsHost.Children.Add(HostedActionToolbarButtonFactory.Create(ResolveButtonName(action), action));
        }
    }

    private static string ResolveButtonName(AsterGraphHostedActionDescriptor action)
        => action.Id switch
        {
            "connection-reconnect" => "PART_EdgeToolbarReconnectButton",
            "connection-disconnect" => "PART_EdgeToolbarDisconnectButton",
            "connection-clear-note" => "PART_EdgeToolbarClearNoteButton",
            _ => $"PART_EdgeToolbar_{action.Id}",
        };
}
