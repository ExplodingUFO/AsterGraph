using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Services;

const string SourceNodeId = "host-sample-source-001";
const string TargetNodeId = "host-sample-target-001";
const string SourcePortId = "out";
const string TargetPortId = "in";
var definitionId = new NodeDefinitionId("host.sample.node");

HostSampleAvaloniaEnvironment.EnsureInitialized();

var catalog = CreateCatalog(definitionId);
var runtimeResult = VerifyRuntimeOnlyRoute(catalog, definitionId);
var hostedUiResult = VerifyHostedUiRoute(catalog, definitionId);
var allOk = runtimeResult.IsOk && hostedUiResult.IsOk;

Console.WriteLine("HOST_SAMPLE_PATHS:CreateSession:Create:AsterGraphAvaloniaViewFactory");
Console.WriteLine($"HOST_SAMPLE_RUNTIME_OK:{runtimeResult.IsOk}:{runtimeResult.FeatureDescriptorCount}:{runtimeResult.SaveCalls}:{runtimeResult.ConnectionCount}");
Console.WriteLine($"HOST_SAMPLE_HOSTED_UI_OK:{hostedUiResult.IsOk}:{hostedUiResult.ChromeMode}:{hostedUiResult.EnableDefaultContextMenu}:{hostedUiResult.EnableDefaultCommandShortcuts}:{hostedUiResult.ConnectionCount}");
Console.WriteLine($"HOST_SAMPLE_OK:{allOk}");

if (!allOk)
{
    throw new InvalidOperationException("Host sample validation failed.");
}

static RouteResult VerifyRuntimeOnlyRoute(INodeCatalog catalog, NodeDefinitionId definitionId)
{
    var workspace = new RecordingWorkspaceService("workspace://host-sample/runtime");
    var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
    {
        Document = CreateDocument(definitionId),
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
    });

    session.Commands.StartConnection(SourceNodeId, SourcePortId);
    session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
    session.Commands.SaveWorkspace();

    var snapshot = session.Queries.CreateDocumentSnapshot();
    var featureDescriptors = session.Queries.GetFeatureDescriptors();
    var isOk = snapshot.Connections.Count == 1
        && featureDescriptors.Count > 0
        && workspace.SaveCalls == 1
        && workspace.Exists();

    return new RouteResult(
        isOk,
        snapshot.Connections.Count,
        featureDescriptors.Count,
        workspace.SaveCalls,
        GraphEditorViewChromeMode.Default,
        EnableDefaultContextMenu: true,
        EnableDefaultCommandShortcuts: true);
}

static RouteResult VerifyHostedUiRoute(INodeCatalog catalog, NodeDefinitionId definitionId)
{
    var workspace = new RecordingWorkspaceService("workspace://host-sample/hosted-ui");
    var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
    {
        Document = CreateDocument(definitionId),
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
    });
    var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
    {
        Editor = editor,
        ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        EnableDefaultContextMenu = false,
        EnableDefaultCommandShortcuts = false,
    });

    editor.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
    editor.SaveWorkspace();

    var snapshot = editor.Session.Queries.CreateDocumentSnapshot();
    var isOk = view.Editor == editor
        && snapshot.Connections.Count == 1
        && workspace.SaveCalls == 1
        && workspace.Exists()
        && view.ChromeMode == GraphEditorViewChromeMode.CanvasOnly
        && !view.EnableDefaultContextMenu
        && !view.EnableDefaultCommandShortcuts;

    return new RouteResult(
        isOk,
        snapshot.Connections.Count,
        editor.Session.Queries.GetFeatureDescriptors().Count,
        workspace.SaveCalls,
        view.ChromeMode,
        view.EnableDefaultContextMenu,
        view.EnableDefaultCommandShortcuts);
}

static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
{
    var catalog = new NodeCatalog();
    catalog.RegisterDefinition(new NodeDefinition(
        definitionId,
        "Host Sample Node",
        "Host Sample",
        "Minimal host sample node definition.",
        [
            new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B"),
        ],
        [
            new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4"),
        ]));
    return catalog;
}

static GraphDocument CreateDocument(NodeDefinitionId definitionId)
    => new(
        "Host Sample Graph",
        "Minimal canonical host-integration sample.",
        [
            new GraphNode(
                SourceNodeId,
                "Host Sample Source",
                "Host Sample",
                "Canonical Source",
                "Source node for canonical host integration.",
                new GraphPoint(120, 160),
                new GraphSize(240, 160),
                [
                    new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [
                    new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                "#6AD5C4",
                definitionId),
            new GraphNode(
                TargetNodeId,
                "Host Sample Target",
                "Host Sample",
                "Canonical Target",
                "Target node for canonical host integration.",
                new GraphPoint(460, 160),
                new GraphSize(240, 160),
                [
                    new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [
                    new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                "#F3B36B",
                definitionId),
        ],
        []);

file sealed class ExactCompatibilityService : IPortCompatibilityService
{
    public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        => sourceType == targetType
            ? PortCompatibilityResult.Exact()
            : PortCompatibilityResult.Rejected();
}

file sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
{
    public string WorkspacePath { get; } = workspacePath;

    public int SaveCalls { get; private set; }

    public GraphDocument? SavedDocument { get; private set; }

    public void Save(GraphDocument document)
    {
        SaveCalls++;
        SavedDocument = document;
    }

    public GraphDocument Load()
        => SavedDocument ?? throw new InvalidOperationException("No saved workspace.");

    public bool Exists()
        => SavedDocument is not null;
}

file readonly record struct RouteResult(
    bool IsOk,
    int ConnectionCount,
    int FeatureDescriptorCount,
    int SaveCalls,
    GraphEditorViewChromeMode ChromeMode,
    bool EnableDefaultContextMenu,
    bool EnableDefaultCommandShortcuts);

file static class HostSampleAvaloniaEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        HostSampleAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
        _initialized = true;
    }
}

file sealed class HostSampleAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<HostSampleApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

file sealed class HostSampleApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
