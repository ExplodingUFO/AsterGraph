using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorServiceSeamsTests
{
    [Fact]
    public void AsterGraphEditorFactory_UsesExplicitStorageRootForDefaultServices()
    {
        var definitionId = new NodeDefinitionId("tests.services.default");
        var root = Path.Combine(Path.GetTempPath(), "astergraph-service-seams", Guid.NewGuid().ToString("N"));

        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new ExactCompatibilityService(),
            StorageRootPath = root,
        });

        Assert.Equal(GraphEditorStorageDefaults.GetWorkspacePath(root), editor.WorkspacePath);
        Assert.Equal(GraphEditorStorageDefaults.GetFragmentPath(root), editor.FragmentPath);
        Assert.Equal(GraphEditorStorageDefaults.GetFragmentLibraryPath(root), editor.FragmentLibraryPath);
        Assert.DoesNotContain("Demo", editor.WorkspacePath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Demo", editor.FragmentPath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Demo", editor.FragmentLibraryPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AsterGraphEditorFactory_UsesHostSuppliedServicesDiagnosticsAndCompatibilityContinuity()
    {
        var definitionId = new NodeDefinitionId("tests.services.host");
        var workspace = new RecordingWorkspaceService("workspace://custom");
        var fragmentWorkspace = new RecordingFragmentWorkspaceService("fragment://custom");
        var fragmentLibrary = new RecordingFragmentLibraryService("library://custom");
        var serializer = new RecordingClipboardPayloadSerializer();
        var diagnostics = new RecordingDiagnosticsSink();
        var compatibility = new ExactCompatibilityService();
        var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = compatibility,
            WorkspaceService = workspace,
            FragmentWorkspaceService = fragmentWorkspace,
            FragmentLibraryService = fragmentLibrary,
            ClipboardPayloadSerializer = serializer,
            DiagnosticsSink = diagnostics,
            ContextMenuAugmentor = new ThrowingAugmentor(),
        });
        GraphEditorRecoverableFailureEventArgs? failure = null;
        editor.Session.Events.RecoverableFailure += (_, args) => failure = args;

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        await editor.CopySelectionAsync();
        editor.ExportSelectionFragment();
        editor.Session.Commands.SaveWorkspace();
        var compatibleTargets = editor.Session.Queries.GetCompatibleTargets("source-node", "out");
        var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(200, 120)));

        Assert.Equal("workspace://custom", editor.WorkspacePath);
        Assert.Equal("fragment://custom", editor.FragmentPath);
        Assert.Equal("library://custom", editor.FragmentLibraryPath);
        Assert.Equal(1, serializer.SerializeCalls);
        Assert.Equal(1, fragmentWorkspace.SaveCalls);
        Assert.Equal(1, workspace.SaveCalls);
        Assert.Single(compatibleTargets);
        Assert.True(compatibility.EvaluateCalls > 0);
        Assert.NotEmpty(menu);
        Assert.Collection(
            diagnostics.Diagnostics,
            diagnostic =>
            {
                Assert.Equal("fragment.export.succeeded", diagnostic.Code);
                Assert.Equal("fragment.export", diagnostic.Operation);
                Assert.Equal(GraphEditorDiagnosticSeverity.Info, diagnostic.Severity);
            },
            diagnostic =>
            {
                Assert.Equal("workspace.save.succeeded", diagnostic.Code);
                Assert.Equal("workspace.save", diagnostic.Operation);
                Assert.Equal(GraphEditorDiagnosticSeverity.Info, diagnostic.Severity);
            },
            diagnostic =>
            {
                Assert.Equal("contextmenu.augment.failed", diagnostic.Code);
                Assert.Equal("contextmenu.augment", diagnostic.Operation);
                Assert.Equal(GraphEditorDiagnosticSeverity.Error, diagnostic.Severity);
            });
        Assert.NotNull(failure);
        Assert.Equal("contextmenu.augment.failed", failure!.Code);
    }

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Service Graph",
            "Service seam regression coverage.",
            [
                new GraphNode(
                    "source-node",
                    "Source Node",
                    "Tests",
                    "Services",
                    "Source node for compatibility coverage.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort("out", "Output", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    "target-node",
                    "Target Node",
                    "Tests",
                    "Services",
                    "Target node for compatibility coverage.",
                    new GraphPoint(420, 160),
                    new GraphSize(240, 160),
                    [
                        new GraphPort("in", "Input", PortDirection.Input, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Service Node",
            "Tests",
            "Services",
            [
                new PortDefinition("in", "Input", new PortTypeId("float"), "#55D8C1"),
            ],
            [
                new PortDefinition("out", "Output", new PortTypeId("float"), "#55D8C1"),
            ]));
        return catalog;
    }

    private sealed class RecordingWorkspaceService(string workspacePath) : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = workspacePath;
        public int SaveCalls { get; private set; }
        public GraphDocument? LastSaved { get; private set; }

        public void Save(GraphDocument document)
        {
            SaveCalls++;
            LastSaved = document;
        }

        public GraphDocument Load()
            => LastSaved ?? throw new InvalidOperationException("No workspace snapshot.");

        public bool Exists()
            => LastSaved is not null;
    }

    private sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
    {
        public string FragmentPath { get; } = fragmentPath;
        public int SaveCalls { get; private set; }
        public GraphSelectionFragment? LastSaved { get; private set; }

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
            SaveCalls++;
            LastSaved = fragment;
        }

        public GraphSelectionFragment Load(string? path = null)
            => LastSaved ?? throw new InvalidOperationException("No fragment snapshot.");

        public bool Exists(string? path = null)
            => LastSaved is not null;

        public void Delete(string? path = null)
            => LastSaved = null;
    }

    private sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
    {
        public string LibraryPath { get; } = libraryPath;

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => Path.Combine(LibraryPath, $"{name ?? "fragment"}.json");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new NotSupportedException();

        public void DeleteTemplate(string path)
        {
        }
    }

    private sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
    {
        public int SerializeCalls { get; private set; }

        public string Serialize(GraphSelectionFragment fragment)
        {
            SerializeCalls++;
            return "serialized-fragment";
        }

        public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        {
            fragment = null;
            return false;
        }
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public int EvaluateCalls { get; private set; }

        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
        {
            EvaluateCalls++;
            return sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
        }
    }

    private sealed class ThrowingAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => throw new InvalidOperationException("augmentor exploded");
    }
}
