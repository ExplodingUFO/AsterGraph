using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphInspectorStandaloneTests
{
    private static readonly NodeDefinitionId InspectorDefinitionId = new("tests.inspector.parameter-node");
    private const string NodeId = "tests.inspector.node-001";

    [AvaloniaFact]
    public void StandaloneInspectorFactory_BindsEditorAndKeepsPureInspectorBoundary()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            Assert.Same(editor, inspector.Editor);

            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Contains("Connections", allText);
            Assert.Contains("Inputs", allText);
            Assert.Contains("Outputs", allText);
            Assert.Contains("Upstream", allText);
            Assert.Contains("Downstream", allText);
            Assert.Single(editor.SelectedNodeParameters);
            Assert.Equal("Threshold", editor.SelectedNodeParameters[0].DisplayName);

            Assert.DoesNotContain("Workspace", allText);
            Assert.DoesNotContain("Fragments", allText);
            Assert.DoesNotContain("Shortcuts", allText);
            Assert.DoesNotContain("Mini Map", allText);
            Assert.Empty(inspector.GetVisualDescendants().OfType<GraphMiniMap>());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_CustomPresenter_UsesSameEditorAndCanEditParameters()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var customPresenter = new RecordingInspectorPresenter();
        var (window, inspector) = CreateInspectorWindow(
            editor,
            new AsterGraphPresentationOptions
            {
                InspectorPresenter = customPresenter,
            });

        try
        {
            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var applyButton = inspector.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => Equals(button.Tag, "custom-inspector-apply"));

            applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Same(editor, customPresenter.LastEditor);
            Assert.Same(customPresenter, inspector.InspectorPresenter);
            Assert.Contains("CUSTOM INSPECTOR:Inspector Node", allText);
            Assert.Equal("0.90", editor.SelectedNodeParameters[0].StringValue);
        }
        finally
        {
            window.Close();
        }
    }

    private static (Window Window, GraphInspectorView Inspector) CreateInspectorWindow(
        GraphEditorViewModel editor,
        AsterGraphPresentationOptions? presentation = null)
    {
        var inspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
        {
            Editor = editor,
            Presentation = presentation,
        });

        var window = new Window
        {
            Width = 420,
            Height = 900,
            Content = inspector,
        };
        window.Show();
        return (window, inspector);
    }

    private static GraphEditorViewModel CreateEditor()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                InspectorDefinitionId,
                "Inspector Node",
                "Tests",
                "Standalone Inspector",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        defaultValue: 0.5),
                ],
                description: "Used to verify pure standalone inspector content."));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Standalone Inspector Graph",
                "Regression coverage for standalone inspector composition.",
                [
                    new GraphNode(
                        NodeId,
                        "Inspector Node",
                        "Tests",
                        "Standalone Inspector",
                        "Used to project selection, connections, and parameters.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        InspectorDefinitionId,
                        [
                            new GraphParameterValue("threshold", new PortTypeId("float"), 0.75),
                        ]),
                ],
                []),
            catalog,
            new DefaultPortCompatibilityService());
    }

    private sealed class RecordingInspectorPresenter : IGraphInspectorPresenter
    {
        public GraphEditorViewModel? LastEditor { get; private set; }

        public Control Create(GraphEditorViewModel? editor)
        {
            LastEditor = editor;

            var applyButton = new Button
            {
                Tag = "custom-inspector-apply",
                Content = "Apply Custom Inspector Edit",
            };
            applyButton.Click += (_, _) =>
            {
                if (editor?.SelectedNodeParameters.Count > 0)
                {
                    editor.SelectedNodeParameters[0].StringValue = "0.90";
                }
            };

            return new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"CUSTOM INSPECTOR:{editor?.InspectorTitle}",
                    },
                    applyButton,
                },
            };
        }
    }
}
