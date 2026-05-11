using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Standalone viewport controls surface for zoom, fit, and reset actions.
/// </summary>
public sealed class AsterGraphControls : UserControl
{
    /// <summary>
    /// Bound canonical editor session.
    /// </summary>
    public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
        AvaloniaProperty.Register<AsterGraphControls, IGraphEditorSession?>(nameof(Session));

    private static readonly string[] CommandIds =
    [
        "viewport.zoom-in",
        "viewport.zoom-out",
        "viewport.fit",
        "viewport.reset",
    ];

    private readonly WrapPanel _itemsHost = new()
    {
        Orientation = Orientation.Vertical,
        ItemHeight = 36,
        ItemWidth = double.NaN,
    };
    private IGraphEditorSession? _observedSession;

    /// <summary>
    /// Initializes a standalone viewport controls surface.
    /// </summary>
    public AsterGraphControls()
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
            _observedSession.Events.ViewportChanged -= HandleSessionChanged;
        }

        _observedSession = session;
        if (_observedSession is not null)
        {
            _observedSession.Events.DocumentChanged += HandleSessionChanged;
            _observedSession.Events.SelectionChanged += HandleSessionChanged;
            _observedSession.Events.ViewportChanged += HandleSessionChanged;
        }
    }

    private void HandleSessionChanged(object? sender, EventArgs args)
        => RefreshActions();

    private void RefreshActions()
    {
        _itemsHost.Children.Clear();
        if (Session is null)
        {
            return;
        }

        foreach (var action in AsterGraphHostedActionFactory.CreateCommandActions(Session, CommandIds))
        {
            _itemsHost.Children.Add(HostedActionToolbarButtonFactory.Create(ResolveButtonName(action), action));
        }
    }

    private static string ResolveButtonName(AsterGraphHostedActionDescriptor action)
        => action.CommandId switch
        {
            "viewport.zoom-in" => "PART_AsterGraphControlsZoomInButton",
            "viewport.zoom-out" => "PART_AsterGraphControlsZoomOutButton",
            "viewport.fit" => "PART_AsterGraphControlsFitViewButton",
            "viewport.reset" => "PART_AsterGraphControlsResetViewButton",
            _ => $"PART_AsterGraphControls_{action.Id}",
        };
}
