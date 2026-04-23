using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.ConsumerSample;

internal static class ConsumerSampleAuthoringSurfaceRecipe
{
    internal const string StatusTemplateKey = "consumer.sample.status-pill";
    internal const string OwnerTemplateKey = "consumer.sample.owner-chip";

    public static AsterGraphPresentationOptions CreatePresentationOptions()
        => new()
        {
            NodeVisualPresenter = new ConsumerSampleNodeVisualPresenter(),
            NodeParameterEditorRegistry = new ConsumerSampleNodeParameterEditorRegistry(),
        };

    public static Control CreateEdgeOverlay(AsterGraph.Editor.Runtime.IGraphEditorSession session)
        => new ConsumerSampleConnectionOverlay(session);

    public static bool UsesCustomNodePresentation(NodeViewModel node)
        => node.DefinitionId == ConsumerSampleHost.ReviewDefinitionId;

    private sealed class ConsumerSampleNodeParameterEditorRegistry : INodeParameterEditorRegistry
    {
        public Control CreateEditor(NodeParameterEditorRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.TemplateKey switch
            {
                StatusTemplateKey => CreateStatusEditor(request.Parameter),
                OwnerTemplateKey => CreateOwnerEditor(request.Parameter),
                _ => DefaultNodeParameterEditorRegistry.Shared.CreateEditor(request),
            };
        }

        private static Control CreateStatusEditor(NodeParameterViewModel parameter)
        {
            var comboBox = new ComboBox
            {
                Name = $"PART_RecipeStatusEditor_{parameter.Key}",
                ItemsSource = parameter.Options,
                MinHeight = 34,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brush.Parse("#162233"),
                Foreground = Brush.Parse("#F8FBFD"),
                BorderBrush = Brush.Parse("#6AD5C4"),
                BorderThickness = new Thickness(1),
            };
            comboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(NodeParameterViewModel.SelectedOption)) { Mode = BindingMode.TwoWay });
            comboBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
            comboBox.DataContext = parameter;
            return comboBox;
        }

        private static Control CreateOwnerEditor(NodeParameterViewModel parameter)
        {
            var chip = new Border
            {
                Background = Brush.Parse("#111B29"),
                BorderBrush = Brush.Parse("#7B93A8"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 6),
                Child = new TextBox
                {
                    Name = $"PART_RecipeOwnerEditor_{parameter.Key}",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = Brush.Parse("#F8FBFD"),
                    Watermark = parameter.InputWatermark,
                },
            };

            if (chip.Child is TextBox textBox)
            {
                textBox.Bind(TextBox.TextProperty, new Binding(nameof(NodeParameterViewModel.StringValue)) { Mode = BindingMode.TwoWay });
                textBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
                textBox.DataContext = parameter;
            }

            chip.DataContext = parameter;
            return chip;
        }
    }
}
