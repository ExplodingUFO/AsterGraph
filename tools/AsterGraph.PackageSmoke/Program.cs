using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

const string SourceNodeId = "smoke-source-001";
const string TargetNodeId = "smoke-target-001";
const string SourcePortId = "out";
const string TargetPortId = "in";
var definitionId = new NodeDefinitionId("smoke.node");

SmokeAvaloniaEnvironment.EnsureInitialized();

var catalog = CreateCatalog(definitionId);
var document = CreateDocument(definitionId);
var styleOptions = GraphEditorStyleOptions.Default with
{
    Shell = GraphEditorStyleOptions.Default.Shell with
    {
        HighlightHex = "#F3B36B",
    },
};
var behaviorOptions = GraphEditorBehaviorOptions.Default with
{
    View = GraphEditorBehaviorOptions.Default.View with
    {
        ShowMiniMap = false,
    },
};

var runtimeResult = VerifyRuntimeRoute(document, catalog, styleOptions, behaviorOptions);
var factoryResult = VerifyFactoryRoute(document, catalog, styleOptions, behaviorOptions);
var retainedResult = VerifyRetainedRoute(document, catalog, styleOptions, behaviorOptions);
var allOk = runtimeResult.IsOk && factoryResult.IsOk && retainedResult.IsOk;

Console.WriteLine($"PACKAGE_SMOKE_ROUTE_OK:{runtimeResult.RouteOk}:{factoryResult.RouteOk}:{retainedResult.RouteOk}");
Console.WriteLine($"PACKAGE_SMOKE_RUNTIME_OK:{runtimeResult.IsOk}:{runtimeResult.CompatibleTargetCount}:{runtimeResult.FeatureDescriptorCount}:{runtimeResult.SaveCalls}");
Console.WriteLine($"PACKAGE_SMOKE_HOSTED_UI_OK:{factoryResult.IsOk}:{factoryResult.ConnectionCount}:{factoryResult.FeatureDescriptorCount}:{factoryResult.SaveCalls}:{factoryResult.ChromeMode}:{factoryResult.EnableDefaultContextMenu}:{factoryResult.EnableDefaultCommandShortcuts}");
Console.WriteLine($"PACKAGE_SMOKE_COMPAT_OK:{retainedResult.IsOk}:{retainedResult.ConnectionCount}:{retainedResult.FeatureDescriptorCount}:{retainedResult.SaveCalls}:{retainedResult.ChromeMode}:{retainedResult.EnableDefaultContextMenu}:{retainedResult.EnableDefaultCommandShortcuts}");
Console.WriteLine($"PACKAGE_SMOKE_OK:{allOk}");

if (!allOk)
{
    throw new InvalidOperationException("Package smoke validation failed.");
}

static RuntimeSmokeResult VerifyRuntimeRoute(
    GraphDocument document,
    INodeCatalog catalog,
    GraphEditorStyleOptions styleOptions,
    GraphEditorBehaviorOptions behaviorOptions)
{
    var workspace = new RecordingWorkspaceService("workspace://package-smoke/runtime");
    var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
    {
        Document = document,
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
        StyleOptions = styleOptions,
        BehaviorOptions = behaviorOptions,
    });

    var compatibleTargets = session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
    session.Commands.StartConnection(SourceNodeId, SourcePortId);
    session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
    session.Commands.SaveWorkspace();

    var snapshot = session.Queries.CreateDocumentSnapshot();
    var featureDescriptors = session.Queries.GetFeatureDescriptors();
    var routeOk = true;
    var isOk = compatibleTargets.Any(target => target.NodeId == TargetNodeId && target.PortId == TargetPortId)
        && snapshot.Connections.Count == 1
        && featureDescriptors.Count > 0
        && workspace.SaveCalls == 1
        && workspace.Exists();

    return new RuntimeSmokeResult(routeOk, isOk, compatibleTargets.Count, snapshot.Connections.Count, featureDescriptors.Count, workspace.SaveCalls);
}

static HostedUiSmokeResult VerifyFactoryRoute(
    GraphDocument document,
    INodeCatalog catalog,
    GraphEditorStyleOptions styleOptions,
    GraphEditorBehaviorOptions behaviorOptions)
{
    var workspace = new RecordingWorkspaceService("workspace://package-smoke/factory");
    var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
    {
        Document = document,
        NodeCatalog = catalog,
        CompatibilityService = new ExactCompatibilityService(),
        WorkspaceService = workspace,
        StyleOptions = styleOptions,
        BehaviorOptions = behaviorOptions,
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
    var featureDescriptors = editor.Session.Queries.GetFeatureDescriptors();
    var routeOk = view.Editor == editor;
    var isOk = routeOk
        && snapshot.Connections.Count == 1
        && featureDescriptors.Count > 0
        && workspace.SaveCalls == 1
        && view.ChromeMode == GraphEditorViewChromeMode.CanvasOnly
        && !view.EnableDefaultContextMenu
        && !view.EnableDefaultCommandShortcuts;

    return new HostedUiSmokeResult(
        routeOk,
        isOk,
        snapshot.Connections.Count,
        featureDescriptors.Count,
        workspace.SaveCalls,
        view.ChromeMode,
        view.EnableDefaultContextMenu,
        view.EnableDefaultCommandShortcuts);
}

static HostedUiSmokeResult VerifyRetainedRoute(
    GraphDocument document,
    INodeCatalog catalog,
    GraphEditorStyleOptions styleOptions,
    GraphEditorBehaviorOptions behaviorOptions)
{
    var workspace = new RecordingWorkspaceService("workspace://package-smoke/compat");
    var editor = new GraphEditorViewModel(
        document,
        catalog,
        new ExactCompatibilityService(),
        workspaceService: workspace,
        styleOptions: styleOptions,
        behaviorOptions: behaviorOptions);
    var view = new GraphEditorView
    {
        Editor = editor,
        ChromeMode = GraphEditorViewChromeMode.CanvasOnly,
        EnableDefaultContextMenu = false,
        EnableDefaultCommandShortcuts = false,
    };

    editor.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
    editor.SaveWorkspace();

    var snapshot = editor.Session.Queries.CreateDocumentSnapshot();
    var featureDescriptors = editor.Session.Queries.GetFeatureDescriptors();
    var routeOk = view.Editor == editor;
    var isOk = routeOk
        && snapshot.Connections.Count == 1
        && featureDescriptors.Count > 0
        && workspace.SaveCalls == 1
        && view.ChromeMode == GraphEditorViewChromeMode.CanvasOnly
        && !view.EnableDefaultContextMenu
        && !view.EnableDefaultCommandShortcuts;

    return new HostedUiSmokeResult(
        routeOk,
        isOk,
        snapshot.Connections.Count,
        featureDescriptors.Count,
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
        "Smoke Node",
        "Smoke",
        "Verification",
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
        "Package Smoke Graph",
        "Validates package-consumption routes without relying on the demo shell.",
        [
            new GraphNode(
                SourceNodeId,
                "Smoke Source",
                "Smoke",
                "Verification",
                "Source node for package smoke verification.",
                new GraphPoint(80, 120),
                new GraphSize(220, 160),
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
                "Smoke Target",
                "Smoke",
                "Verification",
                "Target node for package smoke verification.",
                new GraphPoint(420, 120),
                new GraphSize(220, 160),
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

file readonly record struct RuntimeSmokeResult(
    bool RouteOk,
    bool IsOk,
    int CompatibleTargetCount,
    int ConnectionCount,
    int FeatureDescriptorCount,
    int SaveCalls);

file readonly record struct HostedUiSmokeResult(
    bool RouteOk,
    bool IsOk,
    int ConnectionCount,
    int FeatureDescriptorCount,
    int SaveCalls,
    GraphEditorViewChromeMode ChromeMode,
    bool EnableDefaultContextMenu,
    bool EnableDefaultCommandShortcuts);

file static class SmokeAvaloniaEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        SmokeAppBuilder.BuildAvaloniaApp().SetupWithoutStarting();
        _initialized = true;
    }
}

file sealed class SmokeAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<SmokeApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

file sealed class SmokeApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
