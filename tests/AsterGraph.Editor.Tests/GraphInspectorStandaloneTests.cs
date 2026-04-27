using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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
using AsterGraph.Editor.Localization;
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

            Assert.Contains(editor.InspectorConnectionsTitle, allText);
            Assert.Contains(editor.InspectorInputsTitle, allText);
            Assert.Contains(editor.InspectorOutputsTitle, allText);
            Assert.Contains(editor.InspectorUpstreamTitle, allText);
            Assert.Contains(editor.InspectorDownstreamTitle, allText);
            Assert.Contains(editor.InspectorParametersTitle, allText);
            Assert.Contains("可编辑", allText);
            Assert.Contains("编辑值", allText);
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
    public void StandaloneInspectorFactory_ShowsExplicitParameterEditingControlsAndCues()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var textInputs = inspector.GetVisualDescendants()
                .OfType<TextBox>()
                .Where(textBox => textBox.Classes.Contains("astergraph-input"))
                .ToList();
            var statePills = inspector.GetVisualDescendants()
                .OfType<Border>()
                .Where(border => border.Classes.Contains("astergraph-parameter-state-pill"))
                .ToList();

            Assert.NotEmpty(textInputs);
            Assert.All(textInputs, input => Assert.True(input.MinHeight >= 36));
            Assert.NotEmpty(statePills);
            Assert.Contains("参数编辑", allText);
            Assert.Contains("以下控件直接编辑当前节点参数。", allText);
            Assert.Contains("详细参数编辑固定在检查器中，避免遮挡节点端口和连线。", allText);
            Assert.Contains("可编辑", allText);
            Assert.Contains("编辑值", allText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_FocusingParameterEditor_UpdatesInteractionFocus()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            RefreshInspectorLayout(window, inspector);

            var thresholdEditorHost = inspector.GetVisualDescendants()
                .OfType<NodeParameterEditorHost>()
                .First(host => string.Equals(host.Parameter?.Key, "threshold", StringComparison.Ordinal));
            var parameterInput = thresholdEditorHost.GetVisualDescendants()
                .OfType<TextBox>()
                .First();
            parameterInput.RaiseEvent(new GotFocusEventArgs
            {
                RoutedEvent = InputElement.GotFocusEvent,
                Source = parameterInput,
            });

            Assert.Equal(editor.SelectedNode!.Id, editor.InteractionFocus.InspectedNodeId);
            Assert.Equal(editor.SelectedNode!.Id, editor.InteractionFocus.EditingNodeId);
            Assert.Equal("threshold", editor.InteractionFocus.EditingParameterKey);

            var searchBox = inspector.FindControl<TextBox>("PART_ParameterSearchBox");
            Assert.NotNull(searchBox);
            searchBox!.RaiseEvent(new GotFocusEventArgs
            {
                RoutedEvent = InputElement.GotFocusEvent,
                Source = searchBox,
            });

            Assert.Equal(editor.SelectedNode!.Id, editor.InteractionFocus.InspectedNodeId);
            Assert.Null(editor.InteractionFocus.EditingNodeId);
            Assert.Null(editor.InteractionFocus.EditingParameterKey);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_ParameterEditors_ProjectAccessibleNames_AndHelpText()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            RefreshInspectorLayout(window, inspector);

            var thresholdEditorHost = inspector.GetVisualDescendants()
                .OfType<NodeParameterEditorHost>()
                .First(host => string.Equals(host.Parameter?.Key, "threshold", StringComparison.Ordinal));
            var systemKeyEditorHost = inspector.GetVisualDescendants()
                .OfType<NodeParameterEditorHost>()
                .First(host => string.Equals(host.Parameter?.Key, "system-key", StringComparison.Ordinal));

            Assert.Equal("Threshold parameter editor", AutomationProperties.GetName(thresholdEditorHost));
            Assert.Contains("默认值: 0.5", AutomationProperties.GetHelpText(thresholdEditorHost));
            Assert.Contains("已覆盖", AutomationProperties.GetHelpText(thresholdEditorHost));

            Assert.Equal("System Key parameter editor", AutomationProperties.GetName(systemKeyEditorHost));
            Assert.Contains("参数定义将此字段标记为只读。", AutomationProperties.GetHelpText(systemKeyEditorHost));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_LocalizationProvider_LocalizesStockSectionChrome()
    {
        var editor = CreateEditor();
        editor.SetLocalizationProvider(new DictionaryLocalizationProvider(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.inspector.section.connections"] = "连接关系",
            ["editor.inspector.section.inputs"] = "输入端口",
            ["editor.inspector.section.outputs"] = "输出端口",
            ["editor.inspector.section.upstream"] = "上游依赖",
            ["editor.inspector.section.downstream"] = "下游依赖",
            ["editor.inspector.section.parameters"] = "参数编辑",
            ["editor.inspector.section.parameters.intro"] = "以下控件直接编辑当前节点参数。",
            ["editor.inspector.section.parameters.surface"] = "详细参数编辑固定在检查器中，避免遮挡节点端口和连线。",
        }));
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Contains("连接关系", allText);
            Assert.Contains("输入端口", allText);
            Assert.Contains("输出端口", allText);
            Assert.Contains("参数编辑", allText);
            Assert.DoesNotContain("Connections", allText, StringComparison.Ordinal);
            Assert.DoesNotContain("Inputs", allText, StringComparison.Ordinal);
            Assert.DoesNotContain("Outputs", allText, StringComparison.Ordinal);
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
            var bridgeEditor = inspector.GetVisualDescendants()
                .OfType<TextBox>()
                .Single(textBox => Equals(textBox.Tag, "custom-inspector-bridge"));
            var applyButton = inspector.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => Equals(button.Tag, "custom-inspector-apply"));

            bridgeEditor.RaiseEvent(new GotFocusEventArgs
            {
                RoutedEvent = InputElement.GotFocusEvent,
                Source = bridgeEditor,
            });
            applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Same(editor, customPresenter.LastEditor);
            Assert.Same(customPresenter, inspector.InspectorPresenter);
            Assert.Contains("CUSTOM INSPECTOR:Inspector Node", allText);
            Assert.Equal(editor.SelectedNode!.Id, editor.InteractionFocus.InspectedNodeId);
            Assert.Equal(editor.SelectedNode!.Id, editor.InteractionFocus.EditingNodeId);
            Assert.Equal("threshold", editor.InteractionFocus.EditingParameterKey);
            Assert.Equal("0.90", editor.SelectedNodeParameters[0].StringValue);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_RendersGroupedSectionsListEditorAndValidationHints()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var multilineInputs = inspector.GetVisualDescendants()
                .OfType<TextBox>()
                .Where(textBox => textBox.AcceptsReturn)
                .ToList();
            var slugInput = inspector.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault(textBox => string.Equals(textBox.Watermark?.ToString(), "lowercase-id", StringComparison.Ordinal));

            Assert.Contains("Behavior", allText);
            Assert.Contains("Metadata", allText);
            Assert.Contains("lowercase letters and dashes", allText);
            Assert.Contains("Tags", allText);
            Assert.NotEmpty(multilineInputs);
            Assert.NotNull(slugInput);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_SupportsParameterSearchAndGroupCollapse()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            var searchBox = inspector.FindControl<TextBox>("PART_ParameterSearchBox");
            Assert.NotNull(searchBox);

            searchBox!.Text = "slug";
            searchBox.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent));
            RefreshInspectorLayout(window, inspector);

            Assert.Equal(["slug"], GetVisibleParameterKeys(inspector));

            searchBox.Text = string.Empty;
            searchBox.RaiseEvent(new TextChangedEventArgs(TextBox.TextChangedEvent));
            RefreshInspectorLayout(window, inspector);

            typeof(GraphInspectorView)
                .GetMethod("HandleGroupToggleClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(inspector, [new Button { Tag = "Behavior" }, new RoutedEventArgs(Button.ClickEvent)]);
            RefreshInspectorLayout(window, inspector);

            Assert.True(GetGroupCollapsedState(inspector, "Behavior"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_ShowsValidationSummaryReadonlyReasonAndResetAffordance()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var validationSummary = inspector.FindControl<TextBlock>("PART_ParameterValidationSummaryText");
            var resetSlugButton = inspector.GetVisualDescendants()
                .OfType<Button>()
                .First(button => button.Classes.Contains("astergraph-reset-parameter-button") && Equals(button.Tag, "slug"));

            Assert.NotNull(validationSummary);
            Assert.Contains("Slug", validationSummary!.Text);
            Assert.Contains("参数定义将此字段标记为只读。", allText);
            Assert.Contains("使用搜索过滤参数、折叠分组，并在需要时恢复默认值。", allText);

            resetSlugButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            RefreshInspectorLayout(window, inspector);

            Assert.Equal("valid-id", editor.SelectedNodeParameters.Single(parameter => parameter.Key == "slug").StringValue);
            Assert.DoesNotContain("Slug", inspector.FindControl<TextBlock>("PART_ParameterValidationSummaryText")!.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void StandaloneInspector_HidesAdvancedParametersUntilExpanded_AndShowsValueStateAndUnitSuffix()
    {
        var editor = CreateAuthoringEditor();
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        var (window, inspector) = CreateInspectorWindow(editor);

        try
        {
            RefreshInspectorLayout(window, inspector);

            Assert.Equal(["enabled", "threshold", "slug", "tags", "system-key"], GetVisibleParameterKeys(inspector));

            var allText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
            var advancedToggle = inspector.FindControl<Button>("PART_AdvancedParametersToggleButton");

            Assert.NotNull(advancedToggle);
            Assert.True(advancedToggle!.IsVisible);
            Assert.Equal("显示高级参数", advancedToggle.Content?.ToString());
            Assert.Contains("已覆盖", allText);
            Assert.Contains("默认", allText);
            Assert.Contains("ms", allText);

            advancedToggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            RefreshInspectorLayout(window, inspector);

            var expandedText = string.Join(
                "\n",
                inspector.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(block => block.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text)));

            Assert.Equal("隐藏高级参数", advancedToggle.Content?.ToString());
            Assert.Contains("debug-bias", GetVisibleParameterKeys(inspector));
            Assert.Contains("Debug Bias", expandedText);
            Assert.Contains("Used only for expert tuning.", expandedText);
            Assert.Contains("高级", expandedText);
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

    private static GraphEditorViewModel CreateAuthoringEditor()
    {
        var definitionId = new NodeDefinitionId("tests.inspector.authoring-node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                definitionId,
                "Authoring Node",
                "Tests",
                "Authoring Inspector",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls the authoring threshold.",
                        defaultValue: 0.5,
                        groupName: "Behavior",
                        sortOrder: 20,
                        unitSuffix: "ms"),
                    new NodeParameterDefinition(
                        "enabled",
                        "Enabled",
                        new PortTypeId("bool"),
                        ParameterEditorKind.Boolean,
                        defaultValue: true,
                        groupName: "Behavior",
                        sortOrder: 10),
                    new NodeParameterDefinition(
                        "slug",
                        "Slug",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Stable lowercase identifier.",
                        defaultValue: "valid-id",
                        constraints: new ParameterConstraints(
                            MinimumLength: 3,
                            ValidationPattern: "^[a-z-]+$",
                            ValidationPatternDescription: "lowercase letters and dashes"),
                        groupName: "Metadata",
                        placeholderText: "lowercase-id",
                        helpText: "Used in filenames and automation labels.",
                        sortOrder: 10),
                    new NodeParameterDefinition(
                        "tags",
                        "Tags",
                        new PortTypeId("string-list"),
                        ParameterEditorKind.List,
                        description: "One tag per line.",
                        defaultValue: new[] { "alpha", "beta" },
                        constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
                        groupName: "Metadata",
                        placeholderText: "one tag per line",
                        sortOrder: 20),
                    new NodeParameterDefinition(
                        "system-key",
                        "System Key",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Managed by the definition and shown for transparency.",
                        defaultValue: "system-core",
                        constraints: new ParameterConstraints(IsReadOnly: true),
                        groupName: "Metadata",
                        placeholderText: "system-core",
                        sortOrder: 30),
                    new NodeParameterDefinition(
                        "debug-bias",
                        "Debug Bias",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Advanced tuning parameter.",
                        defaultValue: 0.1d,
                        groupName: "Advanced",
                        helpText: "Used only for expert tuning.",
                        sortOrder: 90,
                        isAdvanced: true),
                ],
                description: "Used to verify grouped authoring inspector UX."));

        return new GraphEditorViewModel(
            new GraphDocument(
                "Authoring Inspector Graph",
                "Regression coverage for grouped authoring inspector composition.",
                [
                    new GraphNode(
                        "tests.inspector.authoring-node-001",
                        "Authoring Node",
                        "Tests",
                        "Authoring Inspector",
                        "Used to project grouped parameters and validation.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        definitionId,
                        [
                            new GraphParameterValue("threshold", new PortTypeId("float"), 0.75),
                            new GraphParameterValue("enabled", new PortTypeId("bool"), true),
                            new GraphParameterValue("slug", new PortTypeId("string"), "AbC"),
                            new GraphParameterValue("tags", new PortTypeId("string-list"), new[] { "alpha", "beta" }),
                            new GraphParameterValue("system-key", new PortTypeId("string"), "system-lock"),
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

            var bridgeEditor = new TextBox
            {
                Tag = "custom-inspector-bridge",
                Text = "Bridge Focus Field",
            };
            GraphPresentationSemantics.SetInspectorEditingParameterKey(bridgeEditor, "threshold");

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
                    bridgeEditor,
                    applyButton,
                },
            };
        }
    }

    private static void RefreshInspectorLayout(Window window, GraphInspectorView inspector)
    {
        inspector.Measure(new Size(window.Width, window.Height));
        inspector.Arrange(new Rect(0, 0, window.Width, window.Height));
    }

    private sealed class DictionaryLocalizationProvider(IReadOnlyDictionary<string, string> values) : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => values.TryGetValue(key, out var localized) ? localized : fallback;
    }

    private static List<string> GetVisibleParameterKeys(GraphInspectorView inspector)
    {
        var groupsControl = inspector.FindControl<ItemsControl>("PART_ParameterGroups");
        Assert.NotNull(groupsControl);

        return groupsControl!.ItemsSource!
            .Cast<object>()
            .SelectMany(group => (IEnumerable<object>)group.GetType().GetProperty("Parameters")!.GetValue(group)!)
            .Select(parameter => (string)parameter.GetType().GetProperty("Key")!.GetValue(parameter)!)
            .ToList();
    }

    private static bool GetGroupCollapsedState(GraphInspectorView inspector, string groupName)
    {
        var groupsControl = inspector.FindControl<ItemsControl>("PART_ParameterGroups");
        Assert.NotNull(groupsControl);

        var group = groupsControl!.ItemsSource!
            .Cast<object>()
            .Single(candidate => string.Equals(
                candidate.GetType().GetProperty("GroupName")!.GetValue(candidate)?.ToString(),
                groupName,
                StringComparison.Ordinal));

        return (bool)group.GetType().GetProperty("IsCollapsed")!.GetValue(group)!;
    }
}
