using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Demo;

public sealed record DemoScenarioPreset(string Id, string Title, string Description);

public static class DemoGraphFactory
{
    public const string TerrainShaderScenario = "terrain-shader";
    public const string AiPipelineScenario = "ai-pipeline";
    public const string MinimapWorkbenchScenario = "minimap-workbench";
    public const string BackgroundGridDensityScenario = "background-grid-density";
    public const string HostedControlsPanelScenario = "hosted-controls-panel";

    public static IReadOnlyList<DemoScenarioPreset> ScenarioPresets { get; } =
    [
        new(
            TerrainShaderScenario,
            "Terrain Shader Graph",
            "The default terrain shader authoring graph with grouped noise and palette nodes."),
        new(
            AiPipelineScenario,
            "AI Workflow / Agent Pipeline",
            "A prebuilt scenario showing input, prompt assembly, trusted tool context, LLM execution, parsing, and output."),
        new(
            MinimapWorkbenchScenario,
            "MiniMap Workbench Surface",
            "A wide graph fixture that exercises minimap-scale workbench projection and spatial overview cues."),
        new(
            BackgroundGridDensityScenario,
            "Background Grid Density",
            "A snapped-grid graph fixture that makes background spacing, edge routing, and viewport density visible."),
        new(
            HostedControlsPanelScenario,
            "Hosted Controls Panel Composition",
            "A composed graph fixture for hosted panel controls, command recovery, and support-review surfaces."),
    ];

    public static GraphDocument CreateStartupDocument(INodeCatalog catalog, string? scenarioName)
        => string.IsNullOrWhiteSpace(scenarioName)
            ? CreateDefault(catalog)
            : CreateScenario(catalog, scenarioName);

    public static bool IsKnownScenario(string scenarioName)
        => TryGetScenarioPreset(scenarioName, out _);

    public static bool TryGetScenarioPreset(string scenarioName, out DemoScenarioPreset? preset)
    {
        preset = ScenarioPresets.SingleOrDefault(candidate =>
            string.Equals(candidate.Id, scenarioName, StringComparison.OrdinalIgnoreCase));
        return preset is not null;
    }

    public static GraphDocument CreateScenario(INodeCatalog catalog, string scenarioName)
    {
        if (string.Equals(scenarioName, TerrainShaderScenario, StringComparison.OrdinalIgnoreCase))
        {
            return CreateDefault(catalog);
        }

        if (string.Equals(scenarioName, AiPipelineScenario, StringComparison.OrdinalIgnoreCase))
        {
            return CreateAiPipeline(catalog);
        }

        if (string.Equals(scenarioName, MinimapWorkbenchScenario, StringComparison.OrdinalIgnoreCase))
        {
            return CreateMinimapWorkbench(catalog);
        }

        if (string.Equals(scenarioName, BackgroundGridDensityScenario, StringComparison.OrdinalIgnoreCase))
        {
            return CreateBackgroundGridDensity(catalog);
        }

        if (string.Equals(scenarioName, HostedControlsPanelScenario, StringComparison.OrdinalIgnoreCase))
        {
            return CreateHostedControlsPanel(catalog);
        }

        throw new ArgumentException($"Unknown demo scenario '{scenarioName}'.", nameof(scenarioName));
    }

    public static GraphDocument CreateDefault(INodeCatalog catalog)
        => new(
            "Terrain Shader Graph",
            "An extensible AsterGraph demo with compile-time node definitions and a reusable graph editor shell.",
            [
                CreateNode(catalog, "time", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(60, 80)),
                CreateNode(catalog, "noise", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(360, 40), groupId: "terrain-authoring"),
                CreateNode(catalog, "gradient", new NodeDefinitionId("aster.demo.palette-ramp"), new GraphPoint(340, 300), groupId: "terrain-authoring"),
                CreateNode(catalog, "slope", new NodeDefinitionId("aster.demo.slope-blend"), new GraphPoint(700, 170)),
                CreateNode(catalog, "light", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(1080, 110)),
                CreateNode(catalog, "output", new NodeDefinitionId("aster.demo.viewport-output"), new GraphPoint(1460, 180)),
            ],
            [
                Connect("time", "phase", "noise", "phase", "time to detail", "#78F0E5"),
                Connect("time", "pulse", "gradient", "pulse", "pulse to ramp", "#FFD56A"),
                Connect("noise", "height", "slope", "height", "height field", "#FFD56A"),
                Connect("noise", "mask", "slope", "mask", "breakup mask", "#4AD6FF"),
                Connect("gradient", "tint", "slope", "tint", "palette feed", "#78F0E5"),
                Connect("slope", "albedo", "light", "albedo", "base surface", "#79E28A"),
                Connect("slope", "rough", "light", "rough", "roughness", "#FFD56A"),
                Connect("time", "pulse", "light", "pulse", "animated rim", "#78F0E5"),
                Connect("light", "lit", "output", "surface", "preview", "#FFB866"),
            ],
            [
                new GraphNodeGroup(
                    "terrain-authoring",
                    "Terrain Authoring",
                    new GraphPoint(340, 40),
                    new GraphSize(292, 446),
                    ["gradient", "noise"]),
            ]);

    private static GraphDocument CreateAiPipeline(INodeCatalog catalog)
        => new(
            "AI Workflow / Agent Pipeline",
            "A prebuilt scenario showing input, prompt assembly, trusted tool context, LLM execution, parsing, and output.",
            [
                CreateNode(catalog, "input", new NodeDefinitionId("aster.demo.ai-input"), new GraphPoint(60, 160)),
                CreateNode(catalog, "prompt", new NodeDefinitionId("aster.demo.prompt-builder"), new GraphPoint(360, 80), groupId: "ai-pipeline-run"),
                CreateNode(catalog, "retriever", new NodeDefinitionId("aster.demo.tool-call"), new GraphPoint(680, 250), groupId: "ai-pipeline-run"),
                CreateNode(catalog, "llm", new NodeDefinitionId("aster.demo.llm-call"), new GraphPoint(980, 100), groupId: "ai-pipeline-run"),
                CreateNode(catalog, "parser", new NodeDefinitionId("aster.demo.response-parser"), new GraphPoint(1300, 160), groupId: "ai-pipeline-run"),
                CreateNode(catalog, "output", new NodeDefinitionId("aster.demo.ai-output"), new GraphPoint(1620, 180)),
            ],
            [
                Connect("input", "text", "prompt", "context", "request", "#78F0E5"),
                Connect("prompt", "prompt", "retriever", "query", "tool query", "#FFD56A"),
                Connect("prompt", "prompt", "llm", "prompt", "assembled prompt", "#78F0E5"),
                Connect("retriever", "evidence", "llm", "context", "trusted context", "#4AD6FF"),
                Connect("llm", "response", "parser", "response", "model response", "#FFB866"),
                Connect("parser", "payload", "output", "payload", "typed result", "#79E28A"),
            ],
            [
                new GraphNodeGroup(
                    "ai-pipeline-run",
                    "Agent Pipeline",
                    new GraphPoint(332, 40),
                    new GraphSize(1218, 448),
                    ["prompt", "retriever", "llm", "parser"]),
            ]);

    private static GraphDocument CreateMinimapWorkbench(INodeCatalog catalog)
        => new(
            "MiniMap Workbench Surface",
            "A wide graph fixture that exercises minimap-scale workbench projection and spatial overview cues.",
            [
                CreateNode(catalog, "time-hub", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(40, 120)),
                CreateNode(catalog, "noise-near", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(420, 40), groupId: "minimap-surface"),
                CreateNode(catalog, "noise-far", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(1040, 520), groupId: "minimap-surface"),
                CreateNode(catalog, "rim-mask", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(760, 660), groupId: "minimap-surface"),
                CreateNode(catalog, "palette", new NodeDefinitionId("aster.demo.palette-ramp"), new GraphPoint(410, 360), groupId: "minimap-surface"),
                CreateNode(catalog, "slope", new NodeDefinitionId("aster.demo.slope-blend"), new GraphPoint(830, 170), groupId: "minimap-surface"),
                CreateNode(catalog, "lighting", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(1240, 230), groupId: "minimap-surface"),
                CreateNode(catalog, "minimap-output", new NodeDefinitionId("aster.demo.viewport-output"), new GraphPoint(1760, 300)),
            ],
            [
                Connect("time-hub", "phase", "noise-near", "phase", "near detail", "#78F0E5"),
                Connect("time-hub", "phase", "noise-far", "phase", "far terrain", "#4AD6FF"),
                Connect("time-hub", "phase", "rim-mask", "phase", "rim mask seed", "#78F0E5"),
                Connect("time-hub", "pulse", "palette", "pulse", "palette pulse", "#FFD56A"),
                Connect("noise-near", "height", "slope", "height", "primary height", "#FFD56A"),
                Connect("noise-far", "mask", "slope", "mask", "overview mask", "#4AD6FF"),
                Connect("palette", "tint", "slope", "tint", "workbench tint", "#78F0E5"),
                Connect("slope", "albedo", "lighting", "albedo", "lit albedo", "#79E28A"),
                Connect("slope", "rough", "lighting", "rough", "roughness", "#FFD56A"),
                Connect("rim-mask", "mask", "lighting", "rimMask", "rim overview", "#4AD6FF"),
                Connect("lighting", "lit", "minimap-output", "surface", "overview preview", "#FFB866"),
            ],
            [
                new GraphNodeGroup(
                    "minimap-surface",
                    "MiniMap Overview Surface",
                    new GraphPoint(392, 24),
                    new GraphSize(1168, 854),
                    ["noise-near", "noise-far", "rim-mask", "palette", "slope", "lighting"]),
            ]);

    private static GraphDocument CreateBackgroundGridDensity(INodeCatalog catalog)
        => new(
            "Background Grid Density",
            "A snapped-grid graph fixture that makes background spacing, edge routing, and viewport density visible.",
            [
                CreateNode(catalog, "grid-clock", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(40, 280)),
                CreateNode(catalog, "grid-noise-a", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(360, 40), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-noise-b", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(360, 320), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-palette", new NodeDefinitionId("aster.demo.palette-ramp"), new GraphPoint(360, 600), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-slope-a", new NodeDefinitionId("aster.demo.slope-blend"), new GraphPoint(760, 130), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-slope-b", new NodeDefinitionId("aster.demo.slope-blend"), new GraphPoint(760, 460), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-light-a", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(1180, 100), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-light-b", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(1180, 460), groupId: "grid-density-band"),
                CreateNode(catalog, "grid-output", new NodeDefinitionId("aster.demo.viewport-output"), new GraphPoint(1700, 300)),
            ],
            [
                Connect("grid-clock", "phase", "grid-noise-a", "phase", "upper phase", "#78F0E5"),
                Connect("grid-clock", "phase", "grid-noise-b", "phase", "lower phase", "#4AD6FF"),
                Connect("grid-clock", "pulse", "grid-palette", "pulse", "grid pulse", "#FFD56A"),
                Connect("grid-noise-a", "height", "grid-slope-a", "height", "upper height", "#FFD56A"),
                Connect("grid-noise-a", "mask", "grid-slope-a", "mask", "upper mask", "#4AD6FF"),
                Connect("grid-noise-b", "height", "grid-slope-b", "height", "lower height", "#FFD56A"),
                Connect("grid-noise-b", "mask", "grid-slope-b", "mask", "lower mask", "#4AD6FF"),
                Connect("grid-palette", "tint", "grid-slope-a", "tint", "upper tint", "#78F0E5"),
                Connect("grid-palette", "tint", "grid-slope-b", "tint", "lower tint", "#78F0E5"),
                Connect("grid-slope-a", "albedo", "grid-light-a", "albedo", "upper albedo", "#79E28A"),
                Connect("grid-slope-a", "rough", "grid-light-a", "rough", "upper rough", "#FFD56A"),
                Connect("grid-slope-b", "albedo", "grid-light-b", "albedo", "lower albedo", "#79E28A"),
                Connect("grid-slope-b", "rough", "grid-light-b", "rough", "lower rough", "#FFD56A"),
                Connect("grid-light-a", "lit", "grid-output", "surface", "upper preview", "#FFB866"),
                Connect("grid-light-b", "lit", "grid-output", "surface", "lower preview", "#E0AEFF"),
            ],
            [
                new GraphNodeGroup(
                    "grid-density-band",
                    "Grid Density Band",
                    new GraphPoint(332, 24),
                    new GraphSize(1294, 818),
                    ["grid-noise-a", "grid-noise-b", "grid-palette", "grid-slope-a", "grid-slope-b", "grid-light-a", "grid-light-b"]),
            ]);

    private static GraphDocument CreateHostedControlsPanel(INodeCatalog catalog)
        => new(
            "Hosted Controls Panel Composition",
            "A composed graph fixture for hosted panel controls, command recovery, and support-review surfaces.",
            [
                CreateNode(catalog, "panel-input", new NodeDefinitionId("aster.demo.ai-input"), new GraphPoint(60, 240)),
                CreateNode(catalog, "panel-prompt", new NodeDefinitionId("aster.demo.prompt-builder"), new GraphPoint(380, 110), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-tool", new NodeDefinitionId("aster.demo.tool-call"), new GraphPoint(710, 310), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-llm", new NodeDefinitionId("aster.demo.llm-call"), new GraphPoint(1010, 140), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-parser", new NodeDefinitionId("aster.demo.response-parser"), new GraphPoint(1340, 230), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-output", new NodeDefinitionId("aster.demo.ai-output"), new GraphPoint(1660, 250)),
                CreateNode(catalog, "panel-clock", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(380, 570), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-light", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(920, 560), groupId: "hosted-control-panel"),
                CreateNode(catalog, "panel-preview", new NodeDefinitionId("aster.demo.viewport-output"), new GraphPoint(1420, 620)),
            ],
            [
                Connect("panel-input", "text", "panel-prompt", "context", "request", "#78F0E5"),
                Connect("panel-prompt", "prompt", "panel-tool", "query", "tool query", "#FFD56A"),
                Connect("panel-prompt", "prompt", "panel-llm", "prompt", "host prompt", "#78F0E5"),
                Connect("panel-tool", "evidence", "panel-llm", "context", "panel context", "#4AD6FF"),
                Connect("panel-llm", "response", "panel-parser", "response", "model response", "#FFB866"),
                Connect("panel-parser", "payload", "panel-output", "payload", "typed result", "#79E28A"),
                Connect("panel-clock", "pulse", "panel-light", "pulse", "panel pulse", "#FFD56A"),
                Connect("panel-light", "lit", "panel-preview", "surface", "control preview", "#E0AEFF"),
            ],
            [
                new GraphNodeGroup(
                    "hosted-control-panel",
                    "Hosted Controls Panel",
                    new GraphPoint(352, 80),
                    new GraphSize(1268, 752),
                    ["panel-prompt", "panel-tool", "panel-llm", "panel-parser", "panel-clock", "panel-light"]),
            ]);

    private static GraphNode CreateNode(
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
            definition.InputPorts.Select(port => CreatePort(port, PortDirection.Input)).ToList(),
            definition.OutputPorts.Select(port => CreatePort(port, PortDirection.Output)).ToList(),
            definition.AccentHex,
            definition.Id,
            definition.Parameters
                .Select(parameter => new GraphParameterValue(parameter.Key, parameter.ValueType, parameter.DefaultValue))
                .ToList(),
            surface ?? new GraphNodeSurfaceState(GraphNodeExpansionState.Collapsed, groupId));
    }

    private static GraphPort CreatePort(PortDefinition definition, PortDirection direction)
        => new(
            definition.Key,
            definition.DisplayName,
            direction,
            definition.TypeId.Value,
            definition.AccentHex,
            definition.TypeId);

    private static GraphConnection Connect(
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
