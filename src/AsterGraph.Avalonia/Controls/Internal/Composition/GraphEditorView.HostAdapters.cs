using Avalonia.Controls;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class GraphEditorView
{
    private sealed class GraphEditorViewCompositionHost : IGraphEditorViewCompositionHost
    {
        private readonly GraphEditorView _owner;

        public GraphEditorViewCompositionHost(GraphEditorView owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? Editor => _owner.Editor;

        public GraphEditorViewChromeMode ChromeMode => _owner.ChromeMode;

        public bool IsHeaderChromeVisible => _owner.IsHeaderChromeVisible;

        public bool IsLibraryChromeVisible => _owner.IsLibraryChromeVisible;

        public bool IsInspectorChromeVisible => _owner.IsInspectorChromeVisible;

        public bool IsStatusChromeVisible => _owner.IsStatusChromeVisible;

        public bool EnableDefaultContextMenu => _owner.EnableDefaultContextMenu;

        public bool EnableDefaultCommandShortcuts => _owner.EnableDefaultCommandShortcuts;

        public bool EnableDefaultWheelViewportGestures => _owner.EnableDefaultWheelViewportGestures;

        public bool EnableAltLeftDragPanning => _owner.EnableAltLeftDragPanning;

        public NodeCanvas? NodeCanvas => _owner._nodeCanvas;

        public GraphInspectorView? InspectorSurface => _owner._inspectorSurface;

        public GraphMiniMap? MiniMapSurface => _owner._miniMapSurface;

        public Grid? ShellGrid => _owner._shellGrid;

        public Border? HeaderChrome => _owner._headerChrome;

        public Border? LibraryChrome => _owner._libraryChrome;

        public Border? InspectorChrome => _owner._inspectorChrome;

        public Border? StatusChrome => _owner._statusChrome;

        public double DefaultShellRowSpacing => _owner._defaultShellRowSpacing;

        public double DefaultShellColumnSpacing => _owner._defaultShellColumnSpacing;

        public IResourceDictionary Resources => _owner.Resources;
    }
}
