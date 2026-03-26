using System.Diagnostics;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorDiagnosticsInstrumentationTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.instrumentation.source");
    private const string SourceNodeId = "instrumentation-source-001";
    private const string SourcePortId = "out";

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_WithInstrumentation_LogsAndTracesWorkspaceDiagnostics()
    {
        using var loggerFactory = new RecordingLoggerFactory();
        using var activitySource = new ActivitySource("AsterGraph.Tests.Instrumentation");
        var activities = new List<string>();
        using var listener = CreateListener(activitySource.Name, activities);

        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            WorkspaceService = new RecordingWorkspaceService("workspace://instrumented"),
            DiagnosticsSink = new RecordingDiagnosticsSink(),
            Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
        });

        Assert.False(session.Commands.LoadWorkspace());
        session.Commands.SaveWorkspace();

        Assert.Contains(loggerFactory.Entries, entry => entry.Level == LogLevel.Warning && entry.Message.Contains("workspace.load.missing", StringComparison.Ordinal));
        Assert.Contains(loggerFactory.Entries, entry => entry.Level == LogLevel.Information && entry.Message.Contains("workspace.save.succeeded", StringComparison.Ordinal));
        Assert.Contains("workspace.load", activities);
        Assert.Contains("workspace.save", activities);
    }

    [Fact]
    public void GraphEditorViewModel_SessionInstrumentation_LogsAndTracesHostSeamFailure()
    {
        using var loggerFactory = new RecordingLoggerFactory();
        using var activitySource = new ActivitySource("AsterGraph.Tests.HostSeamInstrumentation");
        var activities = new List<string>();
        using var listener = CreateListener(activitySource.Name, activities);

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            ContextMenuAugmentor = new ThrowingAugmentor(),
            DiagnosticsSink = new RecordingDiagnosticsSink(),
            Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
        });

        _ = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(120, 80)));

        Assert.Contains(loggerFactory.Entries, entry => entry.Level == LogLevel.Error && entry.Message.Contains("contextmenu.augment.failed", StringComparison.Ordinal));
        Assert.Contains("contextmenu.augment", activities);
    }

    [Fact]
    public void AsterGraphEditorFactory_Create_WithoutInstrumentation_PreservesDiagnosticsSinkWithoutLogsOrTraces()
    {
        using var loggerFactory = new RecordingLoggerFactory();
        using var activitySource = new ActivitySource("AsterGraph.Tests.NoInstrumentation");
        var activities = new List<string>();
        using var listener = CreateListener(activitySource.Name, activities);
        var diagnostics = new RecordingDiagnosticsSink();

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new ExactCompatibilityService(),
            ContextMenuAugmentor = new ThrowingAugmentor(),
            DiagnosticsSink = diagnostics,
        });

        _ = loggerFactory;
        _ = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(120, 80)));

        Assert.Single(diagnostics.Diagnostics);
        Assert.Equal("contextmenu.augment.failed", diagnostics.Diagnostics[0].Code);
        Assert.Empty(loggerFactory.Entries);
        Assert.Empty(activities);
    }

    private static ActivityListener CreateListener(string sourceName, List<string> activities)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => activities.Add(activity.OperationName),
        };

        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static GraphDocument CreateDocument()
        => new(
            "Instrumentation Graph",
            "Phase 5 instrumentation regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Instrumentation Source",
                    "Tests",
                    "Diagnostics",
                    "Produces a float output.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    "#55D8C1",
                    SourceDefinitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Instrumentation Source",
            "Tests",
            "Diagnostics",
            [],
            [
                new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#55D8C1"),
            ]));
        return catalog;
    }

    private sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = workspacePath;

        public GraphDocument? LastSaved { get; private set; }

        public void Save(GraphDocument document)
            => LastSaved = document;

        public GraphDocument Load()
            => LastSaved ?? throw new InvalidOperationException("No saved workspace.");

        public bool Exists()
            => LastSaved is not null;
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
            => sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
    }

    private sealed class ThrowingAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => throw new InvalidOperationException("augmentor exploded");
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName)
            => new RecordingLogger(categoryName, Entries);

        public void AddProvider(ILoggerProvider provider)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger(string categoryName, List<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            entries.Add(new LogEntry(categoryName, logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
