using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Demo.Presentation;

internal static class DemoShowcasePresenters
{
    public static AsterGraphPresentationOptions CreateReplacementPreviewOptions()
        => new()
        {
            InspectorPresenter = new DemoInspectorPresenter(),
            MiniMapPresenter = new DemoMiniMapPresenter(),
        };

    private sealed class DemoInspectorPresenter : IGraphInspectorPresenter
    {
        public Control Create(GraphEditorViewModel? editor)
        {
            var title = editor?.Title ?? "No graph";
            var selectedNode = editor?.SelectedNode?.Title ?? "None";

            return new Border
            {
                Padding = new Thickness(12),
                Background = new SolidColorBrush(Color.Parse("#162430")),
                BorderBrush = new SolidColorBrush(Color.Parse("#3A6078")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = new StackPanel
                {
                    Spacing = 6,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Presenter Replacement",
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Color.Parse("#7FE7D7")),
                        },
                        new TextBlock
                        {
                            Text = $"Graph: {title}",
                            Foreground = new SolidColorBrush(Color.Parse("#F4FFFC")),
                        },
                        new TextBlock
                        {
                            Text = $"Selected: {selectedNode}",
                            Foreground = new SolidColorBrush(Color.Parse("#C2D7E4")),
                        },
                    },
                },
            };
        }
    }

    private sealed class DemoMiniMapPresenter : IGraphMiniMapPresenter
    {
        public Control Create(IGraphEditorSession? session)
        {
            var document = session?.Queries.CreateDocumentSnapshot();
            var nodeCount = document?.Nodes.Count ?? 0;
            var connectionCount = document?.Connections.Count ?? 0;

            return new Border
            {
                Padding = new Thickness(12),
                Background = new SolidColorBrush(Color.Parse("#152028")),
                BorderBrush = new SolidColorBrush(Color.Parse("#476A7C")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = new StackPanel
                {
                    Spacing = 6,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Custom Mini Map",
                            FontWeight = FontWeight.Bold,
                            Foreground = new SolidColorBrush(Color.Parse("#7FE7D7")),
                        },
                        new TextBlock
                        {
                            Text = $"Nodes: {nodeCount}",
                            Foreground = new SolidColorBrush(Color.Parse("#F4FFFC")),
                        },
                        new TextBlock
                        {
                            Text = $"Connections: {connectionCount}",
                            Foreground = new SolidColorBrush(Color.Parse("#C2D7E4")),
                        },
                    },
                },
            };
        }
    }
}
