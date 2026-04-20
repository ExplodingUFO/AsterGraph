using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Stock Avalonia parameter-editor registry used by shipped inspector and node-surface authoring surfaces.
/// </summary>
public sealed class DefaultNodeParameterEditorRegistry : INodeParameterEditorRegistry
{
    public static DefaultNodeParameterEditorRegistry Shared { get; } = new();

    public Control CreateEditor(NodeParameterEditorRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Parameter);

        var parameter = request.Parameter;
        Control control;
        if (parameter.UsesMultilineTextInput)
        {
            var textBox = new TextBox
            {
                MinHeight = request.Usage == NodeParameterEditorUsage.NodeSurface ? 88 : 96,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Classes = { "astergraph-input" },
            };
            textBox.Bind(TextBox.TextProperty, new Binding(nameof(NodeParameterViewModel.StringValue)) { Mode = BindingMode.TwoWay });
            textBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
            textBox.Bind(TextBox.WatermarkProperty, new Binding(nameof(NodeParameterViewModel.InputWatermark)));
            control = textBox;
        }
        else if (parameter.UsesTextInput)
        {
            var textBox = new TextBox
            {
                MinHeight = 36,
                Classes = { "astergraph-input" },
            };
            textBox.Bind(TextBox.TextProperty, new Binding(nameof(NodeParameterViewModel.StringValue)) { Mode = BindingMode.TwoWay });
            textBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
            textBox.Bind(TextBox.WatermarkProperty, new Binding(nameof(NodeParameterViewModel.InputWatermark)));
            control = textBox;
        }
        else if (parameter.IsBoolean)
        {
            var checkBox = new CheckBox
            {
                Classes = { "astergraph-input" },
            };
            checkBox.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(NodeParameterViewModel.BoolValue)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(ContentControl.ContentProperty, new Binding(nameof(NodeParameterViewModel.BooleanCaption)));
            checkBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
            control = checkBox;
        }
        else if (parameter.IsEnum)
        {
            var comboBox = new ComboBox
            {
                ItemsSource = parameter.Options,
                MinHeight = 36,
                Classes = { "astergraph-input" },
            };
            comboBox.Bind(SelectingItemsControl.SelectedItemProperty, new Binding(nameof(NodeParameterViewModel.SelectedOption)) { Mode = BindingMode.TwoWay });
            comboBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(NodeParameterViewModel.CanEdit)));
            control = comboBox;
        }
        else
        {
            control = new TextBlock
            {
                Text = "No shipped parameter editor is available for this value.",
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
            };
        }

        control.DataContext = parameter;
        return control;
    }
}
