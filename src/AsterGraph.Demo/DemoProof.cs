using System.Diagnostics;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Demo;

public sealed record DemoProofResult(
    bool TrustTransparencyOk,
    bool ShellWorkflowOk,
    bool CommandSurfaceOk,
    bool TieredNodeSurfaceOk,
    bool FixedGroupFrameOk,
    bool NonObscuringEditingOk,
    bool VisualSemanticsOk,
    bool CompositeScopeOk,
    bool EdgeNoteOk,
    bool DisconnectFlowOk,
    double StartupMs,
    double InspectorProjectionMs,
    double PluginScanMs,
    double CommandLatencyMs)
{
    public bool IsOk =>
        TrustTransparencyOk
        && ShellWorkflowOk
        && CommandSurfaceOk
        && TieredNodeSurfaceOk
        && FixedGroupFrameOk
        && NonObscuringEditingOk
        && VisualSemanticsOk
        && CompositeScopeOk
        && EdgeNoteOk
        && DisconnectFlowOk;

    public IReadOnlyList<string> ProofLines => DemoProofContract.CreateProofLines(this);

    public IReadOnlyList<string> MetricLines => DemoProofContract.CreateMetricLines(this);
}

public static class DemoProof
{
    public static DemoProofResult Run(string? storageRootPath = null)
    {
        var storageRoot = string.IsNullOrWhiteSpace(storageRootPath)
            ? Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Proof", Guid.NewGuid().ToString("N"))
            : storageRootPath;
        Directory.CreateDirectory(storageRoot);

        MainWindowViewModel? viewModel = null;
        var startupMs = MeasureMilliseconds(() =>
            viewModel = new MainWindowViewModel(new MainWindowShellOptions(
                StorageRootPath: storageRoot,
                EnableStatePersistence: true,
                RestoreLastWorkspaceOnStartup: false)));
        var shell = viewModel ?? throw new InvalidOperationException("Demo view model was not created.");

        shell.Editor.SelectSingleNode(shell.Editor.Nodes[0], updateStatus: false);
        var inspectorProjectionMs = MeasureMilliseconds(() => shell.Editor.Session.Queries.GetSelectedNodeParameterSnapshots().ToArray());
        var pluginScanMs = MeasureMilliseconds(() => shell.PluginCandidates.ToArray());

        var nodeCountBeforeUndo = shell.Editor.Nodes.Count;
        shell.Editor.Session.Commands.AddNode(shell.Editor.NodeTemplates[0].Definition.Id, new GraphPoint(920, 260));
        var undoAction = AsterGraphHostedActionFactory.CreateCommandActions(shell.Editor.Session, ["history.undo"])
            .Single(action => string.Equals(action.Id, "history.undo", StringComparison.Ordinal));
        var commandLatencyMs = MeasureMilliseconds(() => undoAction.TryExecute());
        var commandSurfaceOk = undoAction.CanExecute && shell.Editor.Nodes.Count == nodeCountBeforeUndo;
        var lightingNode = shell.Editor.FindNode("light")
            ?? throw new InvalidOperationException("Demo proof requires the lighting node.");
        var lightingNodeViewModel = lightingNode as NodeViewModel
            ?? throw new InvalidOperationException("Demo proof requires the lighting node view model.");
        shell.Editor.SelectSingleNode(lightingNode, updateStatus: false);
        var pulsePort = lightingNode.Inputs.Single(port => string.Equals(port.Id, "pulse", StringComparison.Ordinal));
        var rimMaskPort = lightingNode.Inputs.Single(port => string.Equals(port.Id, "rimMask", StringComparison.Ordinal));
        var lightSurface = shell.Editor.Session.Queries.GetNodeSurfaceSnapshots()
            .Single(snapshot => string.Equals(snapshot.NodeId, "light", StringComparison.Ordinal));
        var lightingNodeTier = lightSurface.ActiveTier;
        var terrainGroup = shell.Editor.Session.Queries.GetNodeGroups()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        var terrainGroupSnapshot = shell.Editor.GetNodeGroupSnapshots()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        var gradientNode = shell.Editor.FindNode("gradient")
            ?? throw new InvalidOperationException("Demo proof requires the gradient node.");
        var lightingMeasurement = lightingNodeViewModel.SurfaceMeasurement;
        var tieredNodeSurfaceOk =
            lightingNodeTier.Key == "details"
            && !lightingNodeTier.ShowsSection(NodeSurfaceSectionKeys.InputSummaries)
            && lightingMeasurement.OptionalParameterCount == 3
            && lightingMeasurement.HeightToRevealAdditionalInputs > lightingNodeViewModel.Height
            && lightingMeasurement.WidthToRevealInputEditors > lightingNodeViewModel.Width
            && shell.Editor.TrySetNodeSize(
                lightingNodeViewModel,
                new GraphSize(
                    lightingMeasurement.WidthToRevealInputEditors,
                    lightingMeasurement.HeightToRevealAdditionalInputs),
                updateStatus: false)
            && shell.Editor.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => string.Equals(snapshot.NodeId, "light", StringComparison.Ordinal))
                .ActiveTier.Key == "input-editors"
            && terrainGroup is not null
            && terrainGroup.NodeIds.Count == 2
            && shell.Editor.HasIncomingConnection(lightingNode, pulsePort)
            && !shell.Editor.HasIncomingConnection(lightingNode, rimMaskPort);
        var noiseNode = shell.Editor.FindNode("noise")
            ?? throw new InvalidOperationException("Demo proof requires the noise node.");
        var expectedGroupLeft = Math.Min(gradientNode.X, noiseNode.X);
        var expectedGroupTop = Math.Min(gradientNode.Y, noiseNode.Y);
        var expectedGroupRight = Math.Max(gradientNode.X + gradientNode.Width, noiseNode.X + noiseNode.Width);
        var expectedGroupBottom = Math.Max(gradientNode.Y + gradientNode.Height, noiseNode.Y + noiseNode.Height);
        var expectedGroupPosition = new GraphPoint(expectedGroupLeft, expectedGroupTop);
        var expectedGroupSize = new GraphSize(expectedGroupRight - expectedGroupLeft, expectedGroupBottom - expectedGroupTop);
        var resizedGroupFrameSize = new GraphSize(expectedGroupSize.Width + 84d, expectedGroupSize.Height + 56d);
        if (terrainGroupSnapshot is null)
        {
            throw new InvalidOperationException("Demo proof requires the terrain authoring group snapshot.");
        }

        var fixedGroupFrameOk =
            terrainGroupSnapshot.NodeIds.OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(["gradient", "noise"])
            && terrainGroupSnapshot.Position == expectedGroupPosition
            && terrainGroupSnapshot.Size == expectedGroupSize
            && terrainGroupSnapshot.ContentPosition == expectedGroupPosition
            && terrainGroupSnapshot.ContentSize == expectedGroupSize
            && terrainGroupSnapshot.ExtraPadding == default
            && shell.Editor.TrySetNodeGroupSize("terrain-authoring", resizedGroupFrameSize, updateStatus: false);
        var terrainGroupAfterFrameResize = shell.Editor.GetNodeGroupSnapshots()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        fixedGroupFrameOk =
            fixedGroupFrameOk
            && terrainGroupAfterFrameResize is not null
            && terrainGroupAfterFrameResize.Position == terrainGroupSnapshot.Position
            && terrainGroupAfterFrameResize.Size == resizedGroupFrameSize
            && shell.Editor.TrySetNodeSize(noiseNode, new GraphSize(noiseNode.Width + 48d, noiseNode.Height + 36d), updateStatus: false);
        var terrainGroupAfterResize = shell.Editor.GetNodeGroupSnapshots()
            .SingleOrDefault(group => string.Equals(group.Id, "terrain-authoring", StringComparison.Ordinal));
        fixedGroupFrameOk =
            fixedGroupFrameOk
            && terrainGroupAfterResize is not null
            && terrainGroupAfterResize.Position == terrainGroupSnapshot.Position
            && terrainGroupAfterResize.Size == resizedGroupFrameSize
            && terrainGroupAfterResize.NodeIds.OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(["gradient", "noise"]);
        var editableLightingParameter = shell.Editor.Session.Queries.GetSelectedNodeParameterSnapshots()
            .FirstOrDefault(snapshot => snapshot.CanEdit);
        if (editableLightingParameter is not null)
        {
            shell.Editor.SetInspectorEditingParameter(editableLightingParameter.Definition.Key);
        }

        var nonObscuringEditingOk =
            editableLightingParameter is not null
            && shell.Editor.InteractionFocus.IsNodeInspected(lightingNode.Id)
            && shell.Editor.InteractionFocus.IsNodeEditing(lightingNode.Id)
            && string.Equals(
                shell.Editor.InteractionFocus.EditingParameterKey,
                editableLightingParameter.Definition.Key,
                StringComparison.Ordinal)
            && !shell.Editor.HasPendingConnection
            && shell.StandaloneSurfaceLines.Any(line => line.Contains("AsterGraphInspectorViewFactory", StringComparison.Ordinal));
        shell.Editor.SetInspectorEditingParameter(null);
        var outputNode = shell.Editor.FindNode("output")
            ?? throw new InvalidOperationException("Demo proof requires the output node.");
        var visualSemanticsOk =
            string.Equals(outputNode.Category, "Output", StringComparison.Ordinal)
            && terrainGroupAfterResize is not null
            && string.Equals(terrainGroupAfterResize.Title, "Terrain Authoring", StringComparison.Ordinal)
            && !shell.Editor.InspectorConnectionsTitle.Contains('/', StringComparison.Ordinal)
            && !shell.Editor.InspectorInputsTitle.Contains('/', StringComparison.Ordinal)
            && !shell.Editor.InspectorOutputsTitle.Contains('/', StringComparison.Ordinal)
            && string.Equals(shell.Editor.InspectorParametersTitle, "参数编辑", StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(shell.Editor.InspectorParametersGuidance);

        var exportPath = Path.Combine(storageRoot, "plugin-allowlist-proof.json");
        var trustTransparencyOk =
            shell.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.TrustPluginCandidate("aster.demo.plugin.blocked")
            && shell.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.ExportPluginAllowlist(exportPath)
            && File.Exists(exportPath)
            && shell.BlockPluginCandidate("aster.demo.plugin.blocked")
            && shell.PluginCandidateEntries.Any(entry => entry.IsBlocked && entry.PluginId == "aster.demo.plugin.blocked")
            && shell.ImportPluginAllowlist(exportPath)
            && shell.PluginCandidateEntries.Any(entry => entry.IsAllowed && entry.PluginId == "aster.demo.plugin.blocked");

        var workspacePath = Path.Combine(storageRoot, "demo-proof-workspace.json");
        var baselineNodeCount = shell.Editor.Nodes.Count;
        shell.SaveWorkspaceAs(workspacePath);
        shell.Editor.Session.Commands.AddNode(shell.Editor.NodeTemplates[0].Definition.Id, new GraphPoint(1040, 320));
        var shellWorkflowOk =
            shell.TryOpenWorkspacePath(workspacePath)
            && shell.Editor.Nodes.Count == baselineNodeCount
            && shell.RecentWorkspacePaths.Any(path => string.Equals(path, workspacePath, StringComparison.OrdinalIgnoreCase))
            && shell.ShellWorkflowLines.Any(line => line.Contains("workspace", StringComparison.OrdinalIgnoreCase) || line.Contains("工作区", StringComparison.Ordinal));
        var (compositeScopeOk, edgeNoteOk, disconnectFlowOk) = RunSemanticGraphProof(storageRoot);

        return new DemoProofResult(
            trustTransparencyOk,
            shellWorkflowOk,
            commandSurfaceOk,
            tieredNodeSurfaceOk,
            fixedGroupFrameOk,
            nonObscuringEditingOk,
            visualSemanticsOk,
            compositeScopeOk,
            edgeNoteOk,
            disconnectFlowOk,
            startupMs,
            inspectorProjectionMs,
            pluginScanMs,
            commandLatencyMs);
    }

    private static double MeasureMilliseconds(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static (bool CompositeScopeOk, bool EdgeNoteOk, bool DisconnectFlowOk) RunSemanticGraphProof(string storageRoot)
    {
        var catalog = CreateProofCatalog();
        var compositeEditor = CreateProofEditor(
            CreateCompositeProofDocument(catalog),
            catalog,
            Path.Combine(storageRoot, "semantic-composite"));
        var edgeEditor = CreateProofEditor(
            CreateEdgeProofDocument(catalog),
            catalog,
            Path.Combine(storageRoot, "semantic-edge"));

        var compositeScopeOk = RunCompositeScopeProof(compositeEditor);
        var (edgeNoteOk, disconnectFlowOk) = RunEdgeSemanticsProof(edgeEditor);
        return (compositeScopeOk, edgeNoteOk, disconnectFlowOk);
    }

    private static bool RunCompositeScopeProof(GraphEditorViewModel editor)
    {
        var rootGraphId = editor.CreateDocumentSnapshot().RootGraphId;
        editor.Session.Commands.SetSelection(["noise", "gradient"], "gradient", updateStatus: false);

        var groupId = editor.Session.Commands.TryCreateNodeGroupFromSelection("Semantic Composite");
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return false;
        }

        var compositeNodeId = editor.Session.Commands.TryPromoteNodeGroupToComposite(groupId, "Semantic Composite", updateStatus: false);
        if (string.IsNullOrWhiteSpace(compositeNodeId))
        {
            return false;
        }

        var inputBoundaryPortId = editor.Session.Commands.TryExposeCompositePort(
            compositeNodeId,
            "noise",
            "phase",
            "Composite Phase",
            updateStatus: false);
        var outputBoundaryPortId = editor.Session.Commands.TryExposeCompositePort(
            compositeNodeId,
            "gradient",
            "tint",
            "Composite Tint",
            updateStatus: false);
        if (string.IsNullOrWhiteSpace(inputBoundaryPortId) || string.IsNullOrWhiteSpace(outputBoundaryPortId))
        {
            return false;
        }

        var compositeSnapshot = editor.Session.Queries.GetCompositeNodeSnapshots()
            .SingleOrDefault(snapshot => string.Equals(snapshot.NodeId, compositeNodeId, StringComparison.Ordinal));
        if (compositeSnapshot is null)
        {
            return false;
        }

        var inputPort = compositeSnapshot.Inputs.SingleOrDefault(port => string.Equals(port.Id, inputBoundaryPortId, StringComparison.Ordinal));
        var outputPort = compositeSnapshot.Outputs.SingleOrDefault(port => string.Equals(port.Id, outputBoundaryPortId, StringComparison.Ordinal));
        if (inputPort is null
            || outputPort is null
            || !string.Equals(inputPort.Label, "Composite Phase", StringComparison.Ordinal)
            || !string.Equals(outputPort.Label, "Composite Tint", StringComparison.Ordinal)
            || !string.Equals(inputPort.ChildNodeId, "noise", StringComparison.Ordinal)
            || !string.Equals(inputPort.ChildPortId, "phase", StringComparison.Ordinal)
            || !string.Equals(outputPort.ChildNodeId, "gradient", StringComparison.Ordinal)
            || !string.Equals(outputPort.ChildPortId, "tint", StringComparison.Ordinal))
        {
            return false;
        }

        if (!editor.Session.Commands.TryEnterCompositeChildGraph(compositeNodeId, updateStatus: false))
        {
            return false;
        }

        var childScope = editor.Session.Queries.GetScopeNavigationSnapshot();
        var childNodeIds = editor.Nodes
            .Select(node => node.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        if (!childScope.CanNavigateToParent
            || !string.Equals(childScope.ParentScopeId, rootGraphId, StringComparison.Ordinal)
            || !string.Equals(childScope.CurrentScopeId, compositeSnapshot.ChildGraphId, StringComparison.Ordinal)
            || childScope.Breadcrumbs.Count != 2
            || !string.Equals(childScope.Breadcrumbs[0].ScopeId, rootGraphId, StringComparison.Ordinal)
            || !string.Equals(childScope.Breadcrumbs[0].Title, "Semantic Composite Proof", StringComparison.Ordinal)
            || !string.Equals(childScope.Breadcrumbs[1].ScopeId, compositeSnapshot.ChildGraphId, StringComparison.Ordinal)
            || !string.Equals(childScope.Breadcrumbs[1].Title, "Semantic Composite", StringComparison.Ordinal)
            || !childNodeIds.SequenceEqual(["gradient", "noise"], StringComparer.Ordinal))
        {
            return false;
        }

        if (!editor.Session.Commands.TryReturnToParentGraphScope(updateStatus: false))
        {
            return false;
        }

        var rootScope = editor.Session.Queries.GetScopeNavigationSnapshot();
        var rootNodeIds = editor.Nodes
            .Select(node => node.Id)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();
        return !rootScope.CanNavigateToParent
            && string.Equals(rootScope.CurrentScopeId, rootGraphId, StringComparison.Ordinal)
            && rootScope.Breadcrumbs.Count == 1
            && string.Equals(rootScope.Breadcrumbs[0].Title, "Semantic Composite Proof", StringComparison.Ordinal)
            && rootNodeIds.SequenceEqual([compositeNodeId], StringComparer.Ordinal);
    }

    private static (bool EdgeNoteOk, bool DisconnectFlowOk) RunEdgeSemanticsProof(GraphEditorViewModel editor)
    {
        const string reconnectSourceNodeId = "time";
        const string reconnectSourcePortId = "pulse";
        var lightNode = editor.FindNode("light");
        if (lightNode is null)
        {
            return (false, false);
        }

        var pulsePort = lightNode.Inputs.SingleOrDefault(port => string.Equals(port.Id, "pulse", StringComparison.Ordinal));
        var connection = editor.CreateDocumentSnapshot().Connections.SingleOrDefault();
        if (pulsePort is null || connection is null)
        {
            return (false, false);
        }

        editor.SelectSingleNode(lightNode, updateStatus: false);
        var edgeNoteOk =
            editor.Session.Commands.TryExecuteCommand(
                new GraphEditorCommandInvocationSnapshot(
                    "connections.note.set",
                    [
                        new GraphEditorCommandArgumentSnapshot("connectionId", connection.Id),
                        new GraphEditorCommandArgumentSnapshot("text", "Preview branch"),
                        new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
                    ]))
            && string.Equals(
                editor.CreateDocumentSnapshot().Connections.Single().Presentation?.NoteText,
                "Preview branch",
                StringComparison.Ordinal);

        var disconnectFlowOk =
            editor.Session.Commands.TryExecuteCommand(
                new GraphEditorCommandInvocationSnapshot(
                    "connections.reconnect",
                    [
                        new GraphEditorCommandArgumentSnapshot("connectionId", connection.Id),
                        new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
                    ]));
        var updatedLightNode = editor.FindNode("light");
        var updatedPulsePort = updatedLightNode?.Inputs.SingleOrDefault(port => string.Equals(port.Id, "pulse", StringComparison.Ordinal));
        var pending = editor.Session.Queries.GetPendingConnectionSnapshot();
        disconnectFlowOk =
            disconnectFlowOk
            && editor.CreateDocumentSnapshot().Connections.Count == 0
            && pending.HasPendingConnection
            && string.Equals(pending.SourceNodeId, reconnectSourceNodeId, StringComparison.Ordinal)
            && string.Equals(pending.SourcePortId, reconnectSourcePortId, StringComparison.Ordinal)
            && updatedLightNode is not null
            && updatedPulsePort is not null
            && !editor.HasIncomingConnection(updatedLightNode, updatedPulsePort);

        return (edgeNoteOk, disconnectFlowOk);
    }

    private static NodeCatalog CreateProofCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new DemoNodeDefinitionProvider());
        return catalog;
    }

    private static GraphEditorViewModel CreateProofEditor(GraphDocument document, INodeCatalog catalog, string storageRootPath)
    {
        Directory.CreateDirectory(storageRootPath);
        return AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = document,
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRootPath,
        });
    }

    private static GraphDocument CreateCompositeProofDocument(INodeCatalog catalog)
        => new(
            "Semantic Composite Proof",
            "Promotes a grouped authoring cluster into a scoped composite and returns to the root graph.",
            [
                CreateProofNode(catalog, "noise", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(120, 80)),
                CreateProofNode(catalog, "gradient", new NodeDefinitionId("aster.demo.palette-ramp"), new GraphPoint(420, 120)),
            ],
            [],
            []);

    private static GraphDocument CreateEdgeProofDocument(INodeCatalog catalog)
        => new(
            "Edge Semantics Proof",
            "Verifies command-routed edge annotations and canonical reconnect behavior.",
            [
                CreateProofNode(catalog, "time", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(80, 120)),
                CreateProofNode(catalog, "light", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(420, 90)),
            ],
            [
                CreateProofConnection("time", "pulse", "light", "pulse", "animated rim", "#78F0E5"),
            ],
            []);

    private static GraphNode CreateProofNode(
        INodeCatalog catalog,
        string instanceId,
        NodeDefinitionId definitionId,
        GraphPoint position,
        string? groupId = null,
        GraphNodeSurfaceState? surface = null)
    {
        if (!catalog.TryGetDefinition(definitionId, out var definition) || definition is null)
        {
            throw new InvalidOperationException($"Missing demo node definition '{definitionId}'.");
        }

        return new GraphNode(
            instanceId,
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description ?? string.Empty,
            position,
            new GraphSize(definition.DefaultWidth, definition.DefaultHeight),
            definition.InputPorts.Select(port => CreateProofPort(port, PortDirection.Input)).ToList(),
            definition.OutputPorts.Select(port => CreateProofPort(port, PortDirection.Output)).ToList(),
            definition.AccentHex,
            definition.Id,
            definition.Parameters
                .Select(parameter => new GraphParameterValue(parameter.Key, parameter.ValueType, parameter.DefaultValue))
                .ToList(),
            surface ?? new GraphNodeSurfaceState(GraphNodeExpansionState.Collapsed, groupId));
    }

    private static GraphPort CreateProofPort(PortDefinition definition, PortDirection direction)
        => new(
            definition.Key,
            definition.DisplayName,
            direction,
            definition.TypeId.Value,
            definition.AccentHex,
            definition.TypeId);

    private static GraphConnection CreateProofConnection(
        string sourceNodeId,
        string sourcePortId,
        string targetNodeId,
        string targetPortId,
        string label,
        string accentHex)
        => new(
            $"{sourceNodeId}.{sourcePortId}->{targetNodeId}.{targetPortId}",
            sourceNodeId,
            sourcePortId,
            targetNodeId,
            targetPortId,
            label,
            accentHex);
}
