using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 图编辑器缩略图控件，用于概览节点分布与当前视口位置。
/// </summary>
public sealed class GraphMiniMap : UserControl
{
    /// <summary>
    /// 绑定的 canonical 编辑器 session 依赖属性。
    /// </summary>
    public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
        AvaloniaProperty.Register<GraphMiniMap, IGraphEditorSession?>(nameof(Session));

    /// <summary>
    /// 可选的样式选项依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorStyleOptions?> StyleOptionsProperty =
        AvaloniaProperty.Register<GraphMiniMap, GraphEditorStyleOptions?>(nameof(StyleOptions));

    /// <summary>
    /// 可选的缩略图展示器依赖属性。
    /// </summary>
    public static readonly StyledProperty<IGraphMiniMapPresenter?> MiniMapPresenterProperty =
        AvaloniaProperty.Register<GraphMiniMap, IGraphMiniMapPresenter?>(nameof(MiniMapPresenter));

    private readonly StockGraphMiniMapSurface _stockSurface = new();

    /// <summary>
    /// 初始化缩略图控件。
    /// </summary>
    public GraphMiniMap()
    {
        Focusable = false;
        Content = _stockSurface;
        ApplyMiniMapPresenter();
    }

    /// <summary>
    /// 当前绑定的编辑器 session。
    /// </summary>
    public IGraphEditorSession? Session
    {
        get => GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    /// <summary>
    /// 当前缩略图样式选项。
    /// </summary>
    public GraphEditorStyleOptions? StyleOptions
    {
        get => GetValue(StyleOptionsProperty);
        set => SetValue(StyleOptionsProperty, value);
    }

    /// <summary>
    /// 当前缩略图展示器。
    /// </summary>
    public IGraphMiniMapPresenter? MiniMapPresenter
    {
        get => GetValue(MiniMapPresenterProperty);
        set => SetValue(MiniMapPresenterProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SessionProperty
            || change.Property == StyleOptionsProperty
            || change.Property == MiniMapPresenterProperty)
        {
            ApplyMiniMapPresenter();
        }
    }

    private void ApplyMiniMapPresenter()
    {
        if (MiniMapPresenter is null)
        {
            _stockSurface.Session = Session;
            _stockSurface.StyleOptions = StyleOptions;
            if (!ReferenceEquals(Content, _stockSurface))
            {
                Content = _stockSurface;
            }

            return;
        }

        _stockSurface.Session = null;
        Content = MiniMapPresenter.Create(Session);
    }

    private void CenterViewportFromMiniMap(IGraphEditorSession session, Point point, bool updateStatus = true)
    {
        CenterViewportFromBounds(session, Bounds, point, updateStatus);
    }

    private static void CenterViewportFromBounds(IGraphEditorSession session, Rect bounds, Point point, bool updateStatus = true)
    {
        if (!TryGetWorldBounds(session.Queries.CreateDocumentSnapshot(), out var worldBounds))
        {
            return;
        }

        var scale = GetMiniMapScale(bounds, worldBounds);
        if (scale <= 0)
        {
            return;
        }

        var offsetX = bounds.X + ((bounds.Width - (worldBounds.Width * scale)) / 2);
        var offsetY = bounds.Y + ((bounds.Height - (worldBounds.Height * scale)) / 2);
        var worldX = worldBounds.X + ((point.X - offsetX) / scale);
        var worldY = worldBounds.Y + ((point.Y - offsetY) / scale);

        session.Commands.CenterViewAt(new GraphPoint(worldX, worldY), updateStatus);
    }

    private static bool TryGetWorldBounds(GraphDocument document, out Rect worldBounds)
    {
        if (document.Nodes.Count == 0)
        {
            worldBounds = default;
            return false;
        }

        var minX = document.Nodes.Min(node => node.Position.X);
        var minY = document.Nodes.Min(node => node.Position.Y);
        var maxX = document.Nodes.Max(node => node.Position.X + node.Size.Width);
        var maxY = document.Nodes.Max(node => node.Position.Y + node.Size.Height);
        const double padding = 80;

        worldBounds = new Rect(
            minX - padding,
            minY - padding,
            Math.Max(1, (maxX - minX) + (padding * 2)),
            Math.Max(1, (maxY - minY) + (padding * 2)));
        return true;
    }

    private static double GetMiniMapScale(Rect bounds, Rect worldBounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || worldBounds.Width <= 0 || worldBounds.Height <= 0)
        {
            return 0;
        }

        return Math.Min(bounds.Width / worldBounds.Width, bounds.Height / worldBounds.Height);
    }

    private static Rect ToMiniMapRect(
        double worldX,
        double worldY,
        double worldWidth,
        double worldHeight,
        Rect worldBounds,
        double scale,
        double offsetX,
        double offsetY)
        => new(
            offsetX + ((worldX - worldBounds.X) * scale),
            offsetY + ((worldY - worldBounds.Y) * scale),
            Math.Max(2, worldWidth * scale),
            Math.Max(2, worldHeight * scale));

    private sealed class StockGraphMiniMapSurface : Control
    {
        public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
            AvaloniaProperty.Register<StockGraphMiniMapSurface, IGraphEditorSession?>(nameof(Session));

        public static readonly StyledProperty<GraphEditorStyleOptions?> StyleOptionsProperty =
            AvaloniaProperty.Register<StockGraphMiniMapSurface, GraphEditorStyleOptions?>(nameof(StyleOptions));

        private IGraphEditorSession? _observedSession;
        private bool _isDraggingViewport;

        public StockGraphMiniMapSurface()
        {
            Focusable = false;
            PointerPressed += HandlePointerPressed;
            PointerMoved += HandlePointerMoved;
            PointerReleased += HandlePointerReleased;
        }

        public IGraphEditorSession? Session
        {
            get => GetValue(SessionProperty);
            set => SetValue(SessionProperty, value);
        }

        public GraphEditorStyleOptions? StyleOptions
        {
            get => GetValue(StyleOptionsProperty);
            set => SetValue(StyleOptionsProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SessionProperty)
            {
                AttachSession(change.GetNewValue<IGraphEditorSession?>());
                InvalidateVisual();
                return;
            }

            if (change.Property == StyleOptionsProperty)
            {
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var session = Session;
            if (session is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                return;
            }

            var scene = session.Queries.GetSceneSnapshot();
            var canvasStyle = (StyleOptions ?? GraphEditorStyleOptions.Default).Canvas;
            context.FillRectangle(BrushFactory.Solid(canvasStyle.GridBackgroundHex), Bounds);

            if (!TryGetWorldBounds(scene.Document, out var worldBounds))
            {
                return;
            }

            var scale = GetMiniMapScale(Bounds, worldBounds);
            if (scale <= 0)
            {
                return;
            }

            var offsetX = Bounds.X + ((Bounds.Width - (worldBounds.Width * scale)) / 2);
            var offsetY = Bounds.Y + ((Bounds.Height - (worldBounds.Height * scale)) / 2);
            var selectedNodeIds = scene.Selection.SelectedNodeIds.ToHashSet(StringComparer.Ordinal);

            foreach (var node in scene.Document.Nodes)
            {
                var isSelected = selectedNodeIds.Contains(node.Id);
                var nodeRect = ToMiniMapRect(
                    node.Position.X,
                    node.Position.Y,
                    node.Size.Width,
                    node.Size.Height,
                    worldBounds,
                    scale,
                    offsetX,
                    offsetY);
                context.DrawRectangle(
                    BrushFactory.Solid(isSelected ? node.AccentHex : canvasStyle.PrimaryGridHex, isSelected ? 0.7 : 0.45),
                    new Pen(BrushFactory.Solid(node.AccentHex, 0.8), 1),
                    nodeRect);
            }

            if (scene.Viewport.ViewportWidth <= 0 || scene.Viewport.ViewportHeight <= 0)
            {
                return;
            }

            var viewportState = new ViewportState(scene.Viewport.Zoom, scene.Viewport.PanX, scene.Viewport.PanY);
            var topLeft = ViewportMath.ScreenToWorld(viewportState, new GraphPoint(0, 0));
            var bottomRight = ViewportMath.ScreenToWorld(
                viewportState,
                new GraphPoint(scene.Viewport.ViewportWidth, scene.Viewport.ViewportHeight));
            var viewportRect = ToMiniMapRect(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y,
                worldBounds,
                scale,
                offsetX,
                offsetY);

            context.DrawRectangle(
                null,
                new Pen(BrushFactory.Solid(canvasStyle.GuideHex, canvasStyle.GuideOpacity), 1.2),
                viewportRect);
        }

        private void AttachSession(IGraphEditorSession? current)
        {
            if (ReferenceEquals(_observedSession, current))
            {
                return;
            }

            if (_observedSession is not null)
            {
                _observedSession.Events.DocumentChanged -= HandleSessionChanged;
                _observedSession.Events.SelectionChanged -= HandleSessionChanged;
                _observedSession.Events.ViewportChanged -= HandleSessionChanged;
            }

            _observedSession = current;
            if (_observedSession is null)
            {
                return;
            }

            _observedSession.Events.DocumentChanged += HandleSessionChanged;
            _observedSession.Events.SelectionChanged += HandleSessionChanged;
            _observedSession.Events.ViewportChanged += HandleSessionChanged;
        }

        private void HandleSessionChanged(object? sender, EventArgs args)
            => InvalidateVisual();

        private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
        {
            var session = Session;
            if (session is null || session.Queries.CreateDocumentSnapshot().Nodes.Count == 0 || args.GetCurrentPoint(this).Properties.IsLeftButtonPressed is false)
            {
                return;
            }

            CenterViewportFromBounds(session, Bounds, args.GetPosition(this));
            _isDraggingViewport = true;
            args.Pointer.Capture(this);
            args.Handled = true;
        }

        private void HandlePointerMoved(object? sender, PointerEventArgs args)
        {
            if (!_isDraggingViewport || Session is null)
            {
                return;
            }

            CenterViewportFromBounds(Session, Bounds, args.GetPosition(this), updateStatus: false);
            args.Handled = true;
        }

        private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
        {
            if (!_isDraggingViewport)
            {
                return;
            }

            _isDraggingViewport = false;
            args.Pointer.Capture(null);
            args.Handled = true;
        }
    }
}
