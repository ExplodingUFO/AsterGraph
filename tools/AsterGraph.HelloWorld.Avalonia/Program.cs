using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;

HostedHelloWorldAppBuilder.BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

file sealed class HostedHelloWorldApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = HostedHelloWorldWindowFactory.Create();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

file static class HostedHelloWorldAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HostedHelloWorldApp>()
            .UsePlatformDetect();
}

file static class HostedHelloWorldWindowFactory
{
    private const string SourceNodeId = "hello-ui-source-001";
    private const string TargetNodeId = "hello-ui-target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    public static Window Create()
    {
        var definitionId = new NodeDefinitionId("hello.ui.node");
        INodeCatalog catalog = CreateCatalog(definitionId);
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
        editor.Session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = editor,
            ChromeMode = GraphEditorViewChromeMode.Default,
        });

        return new Window
        {
            Title = "AsterGraph HelloWorld (Avalonia)",
            Width = 1280,
            Height = 900,
            Content = view,
        };
    }

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
                "Hello World Node",
                "Hello",
                "Minimal hosted-UI node definition for the first shipped-shell sample with authoring metadata.",
                [
                    new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
                ],
                [
                    new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
                ],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls when the demo source emits a stronger pulse.",
                        defaultValue: 0.5d,
                        constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "mode",
                        "Mode",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        description: "Chooses how the sample node responds to the threshold.",
                        defaultValue: "steady",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("steady", "Steady"),
                                new ParameterOptionDefinition("pulse", "Pulse"),
                            ]),
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "slug",
                        "Slug",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Stable lowercase identifier shown by the hosted inspector.",
                        defaultValue: "hello-source",
                        constraints: new ParameterConstraints(
                            MinimumLength: 3,
                            ValidationPattern: "^[a-z-]+$",
                            ValidationPatternDescription: "lowercase letters and dashes"),
                        groupName: "Metadata",
                        placeholderText: "hello-source"),
                    new NodeParameterDefinition(
                        "tags",
                        "Tags",
                        new PortTypeId("string-list"),
                        ParameterEditorKind.List,
                        description: "One tag per line to demonstrate the shipped multiline list editor.",
                        defaultValue: new[] { "hello", "avalonia" },
                        constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
                        groupName: "Metadata",
                        placeholderText: "one tag per line"),
                ]));
        return catalog;
    }

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Hello World UI Graph",
            "Minimal shipped-UI sample for first-time AsterGraph adoption.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Hello UI Source",
                    "Hello",
                    "Source",
                    "Source node for the minimal hosted-UI sample.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [
                        new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    definitionId,
                    [
                        new GraphParameterValue("threshold", new PortTypeId("float"), 0.65d),
                        new GraphParameterValue("mode", new PortTypeId("enum"), "pulse"),
                        new GraphParameterValue("slug", new PortTypeId("string"), "hello-source"),
                        new GraphParameterValue("tags", new PortTypeId("string-list"), new[] { "hello", "authoring" }),
                    ]),
                new GraphNode(
                    TargetNodeId,
                    "Hello UI Target",
                    "Hello",
                    "Target",
                    "Target node for the minimal hosted-UI sample.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [
                        new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                    ],
                    [],
                    "#F3B36B",
                    definitionId,
                    [
                        new GraphParameterValue("threshold", new PortTypeId("float"), 0.4d),
                        new GraphParameterValue("mode", new PortTypeId("enum"), "steady"),
                        new GraphParameterValue("slug", new PortTypeId("string"), "hello-target"),
                        new GraphParameterValue("tags", new PortTypeId("string-list"), new[] { "target" }),
                    ]),
            ],
            [
                new GraphConnection(
                    "hello-ui-connection-001",
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Hello UI Connection",
                    "#6AD5C4"),
            ]);
}
