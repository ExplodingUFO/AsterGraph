using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Demo.Definitions;

public sealed class DemoNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
    {
        var palette = new Palette();

        return
        [
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.time-driver"),
                "Time Driver",
                "Inputs",
                "Scene clock",
                [
                ],
                [
                    new PortDefinition("phase", "Phase", new PortTypeId("float"), palette.Signal),
                    new PortDefinition("pulse", "Pulse", new PortTypeId("float"), palette.Highlight),
                ],
                [
                    new NodeParameterDefinition(
                        "tempo",
                        "Tempo",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls how quickly the phase loops.",
                        defaultValue: 1.0,
                        constraints: new ParameterConstraints(Minimum: 0.1, Maximum: 8.0),
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "enabled",
                        "Enabled",
                        new PortTypeId("bool"),
                        ParameterEditorKind.Boolean,
                        defaultValue: true,
                        groupName: "Behavior"),
                ],
                description: "Provides looping phase and eased pulse values used across the graph.",
                accentHex: palette.Input,
                defaultWidth: 240,
                defaultHeight: 158),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.noise-field"),
                "Noise Field",
                "Procedural",
                "Terrain detail",
                [
                    new PortDefinition("phase", "Phase", new PortTypeId("float"), palette.Signal),
                ],
                [
                    new PortDefinition("height", "Height", new PortTypeId("float"), palette.Highlight),
                    new PortDefinition("mask", "Mask", new PortTypeId("float"), palette.Input),
                ],
                [
                    new NodeParameterDefinition(
                        "scale",
                        "Scale",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls the macro noise frequency.",
                        defaultValue: 0.75,
                        constraints: new ParameterConstraints(Minimum: 0.05, Maximum: 8.0),
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "octaves",
                        "Octaves",
                        new PortTypeId("int"),
                        ParameterEditorKind.Number,
                        description: "Number of stacked noise layers.",
                        defaultValue: 4,
                        constraints: new ParameterConstraints(Minimum: 1, Maximum: 12),
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "enabled",
                        "Enabled",
                        new PortTypeId("bool"),
                        ParameterEditorKind.Boolean,
                        defaultValue: true,
                        groupName: "Behavior"),
                    new NodeParameterDefinition(
                        "tags",
                        "Tags",
                        new PortTypeId("string-list"),
                        ParameterEditorKind.List,
                        description: "One tag per line to annotate the generated field.",
                        defaultValue: new[] { "terrain", "detail" },
                        constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
                        groupName: "Metadata",
                        placeholderText: "one tag per line"),
                ],
                description: "Builds a layered noise mask with controllable density and turbulence.",
                accentHex: palette.Procedure,
                defaultWidth: 272,
                defaultHeight: 212),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.palette-ramp"),
                "Palette Ramp",
                "Color",
                "Atmosphere tint",
                [
                    new PortDefinition("pulse", "Pulse", new PortTypeId("float"), palette.Highlight),
                ],
                [
                    new PortDefinition("tint", "Tint", new PortTypeId("color3"), palette.Signal),
                ],
                [
                    new NodeParameterDefinition(
                        "palettePreset",
                        "Palette Preset",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        description: "Selects the base color ramp.",
                        defaultValue: "sunset",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("sunset", "Sunset"),
                                new ParameterOptionDefinition("aurora", "Aurora"),
                                new ParameterOptionDefinition("desert", "Desert"),
                            ]),
                        groupName: "Appearance"),
                    new NodeParameterDefinition(
                        "label",
                        "Label",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Optional lowercase identifier for this palette branch.",
                        defaultValue: "sunset-ramp",
                        constraints: new ParameterConstraints(
                            MinimumLength: 3,
                            ValidationPattern: "^[a-z-]+$",
                            ValidationPatternDescription: "lowercase letters and dashes"),
                        groupName: "Metadata",
                        placeholderText: "palette-id"),
                ],
                description: "Generates a warm-to-cool palette gradient for the final surface tint.",
                accentHex: palette.Color,
                defaultWidth: 248,
                defaultHeight: 186),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.slope-blend"),
                "Slope Blend",
                "Compositing",
                "Surface breakup",
                [
                    new PortDefinition("height", "Height", new PortTypeId("float"), palette.Highlight),
                    new PortDefinition("mask", "Mask", new PortTypeId("float"), palette.Input),
                    new PortDefinition("tint", "Tint", new PortTypeId("color3"), palette.Signal),
                ],
                [
                    new PortDefinition("albedo", "Albedo", new PortTypeId("color3"), palette.Procedure),
                    new PortDefinition("rough", "Roughness", new PortTypeId("float"), palette.Highlight),
                ],
                [
                    new NodeParameterDefinition(
                        "blendMode",
                        "Blend Mode",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        description: "Chooses how tint is applied to the height field.",
                        defaultValue: "overlay",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("overlay", "Overlay"),
                                new ParameterOptionDefinition("multiply", "Multiply"),
                                new ParameterOptionDefinition("screen", "Screen"),
                            ]),
                        groupName: "Compositing"),
                ],
                description: "Combines height data with the palette ramp and a cliff mask to shape the result.",
                accentHex: palette.Composite,
                defaultWidth: 300,
                defaultHeight: 222),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.lighting-mix"),
                "Lighting Mix",
                "Shading",
                "Rim and bounce",
                [
                    new PortDefinition("albedo", "Albedo", new PortTypeId("color3"), palette.Procedure),
                    new PortDefinition("rough", "Roughness", new PortTypeId("float"), palette.Highlight),
                    new PortDefinition("pulse", "Pulse", new PortTypeId("float"), palette.Signal),
                    new PortDefinition("rimMask", "Rim Mask", new PortTypeId("float"), palette.Input),
                ],
                [
                    new PortDefinition("lit", "Lit Color", new PortTypeId("color3"), palette.Color),
                ],
                [
                    new NodeParameterDefinition(
                        "rimStrength",
                        "Rim Strength",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Controls the strength of the animated rim light.",
                        defaultValue: 0.45,
                        constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                        groupName: "Lighting"),
                    new NodeParameterDefinition(
                        "pulseBias",
                        "Pulse Bias",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Parameter fallback value shown when the pulse input is disconnected.",
                        defaultValue: 0.2,
                        constraints: new ParameterConstraints(Minimum: -1, Maximum: 1),
                        groupName: "Inputs",
                        templateKey: "demo.inline.pulse-bias"),
                    new NodeParameterDefinition(
                        "rimMask",
                        "Rim Mask",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        description: "Parameter value shown when the active tier exposes the node-side rail and the rim mask input is disconnected.",
                        defaultValue: 0.68,
                        constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                        groupName: "Inputs",
                        templateKey: "demo.inline.rim-mask"),
                ],
                description: "Adds bounce lighting and a soft rim based on the evolving scene pulse.",
                accentHex: palette.Shading,
                defaultWidth: 430,
                defaultHeight: 260),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.viewport-output"),
                "Viewport Output",
                "Output",
                "Preview target",
                [
                    new PortDefinition("surface", "Surface", new PortTypeId("color3"), palette.Color),
                ],
                [
                ],
                description: "Final material output routed into the preview surface and debug viewport.",
                accentHex: palette.Output,
                defaultWidth: 238,
                defaultHeight: 162),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.ai-input"),
                "Input",
                "AI Pipeline",
                "User request",
                [
                ],
                [
                    new PortDefinition("text", "Text", new PortTypeId("text"), palette.Signal),
                ],
                [
                    new NodeParameterDefinition(
                        "sampleText",
                        "Sample Text",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Seed request used by the scenario graph.",
                        defaultValue: "Summarize open support tickets and propose next actions.",
                        groupName: "Request",
                        placeholderText: "user request"),
                ],
                description: "Represents a host-provided user request or document payload.",
                accentHex: palette.Input,
                defaultWidth: 250,
                defaultHeight: 168),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.prompt-builder"),
                "Prompt",
                "AI Pipeline",
                "Instruction builder",
                [
                    new PortDefinition("context", "Context", new PortTypeId("text"), palette.Signal),
                ],
                [
                    new PortDefinition("prompt", "Prompt", new PortTypeId("text"), palette.Highlight),
                ],
                [
                    new NodeParameterDefinition(
                        "systemPrompt",
                        "System Prompt",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Instruction text merged with the incoming request.",
                        defaultValue: "You are a support operations agent. Return concise, actionable findings.",
                        groupName: "Prompt",
                        placeholderText: "system instruction"),
                    new NodeParameterDefinition(
                        "mode",
                        "Mode",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        defaultValue: "triage",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("triage", "Triage"),
                                new ParameterOptionDefinition("extract", "Extract"),
                                new ParameterOptionDefinition("route", "Route"),
                            ]),
                        groupName: "Prompt"),
                ],
                description: "Builds an instruction payload from host input and selected prompt mode.",
                accentHex: palette.Color,
                defaultWidth: 280,
                defaultHeight: 210),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.tool-call"),
                "Tool",
                "AI Pipeline",
                "Trusted context",
                [
                    new PortDefinition("query", "Query", new PortTypeId("text"), palette.Highlight),
                ],
                [
                    new PortDefinition("evidence", "Evidence", new PortTypeId("text"), palette.Input),
                ],
                [
                    new NodeParameterDefinition(
                        "toolName",
                        "Tool",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        defaultValue: "tickets",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("tickets", "Ticket Search"),
                                new ParameterOptionDefinition("docs", "Docs Search"),
                                new ParameterOptionDefinition("crm", "CRM Lookup"),
                            ]),
                        groupName: "Tool"),
                    new NodeParameterDefinition(
                        "maxResults",
                        "Max Results",
                        new PortTypeId("int"),
                        ParameterEditorKind.Number,
                        defaultValue: 5,
                        constraints: new ParameterConstraints(Minimum: 1, Maximum: 25),
                        groupName: "Tool"),
                ],
                description: "Models a trusted host tool call that supplies bounded context to the model.",
                accentHex: palette.Procedure,
                defaultWidth: 270,
                defaultHeight: 208),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.llm-call"),
                "LLM",
                "AI Pipeline",
                "Model run",
                [
                    new PortDefinition("prompt", "Prompt", new PortTypeId("text"), palette.Highlight),
                    new PortDefinition("context", "Context", new PortTypeId("text"), palette.Input),
                ],
                [
                    new PortDefinition("response", "Response", new PortTypeId("text"), palette.Color),
                ],
                [
                    new NodeParameterDefinition(
                        "model",
                        "Model",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        defaultValue: "fast",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("fast", "Fast"),
                                new ParameterOptionDefinition("balanced", "Balanced"),
                                new ParameterOptionDefinition("accurate", "Accurate"),
                            ]),
                        groupName: "Model"),
                    new NodeParameterDefinition(
                        "temperature",
                        "Temperature",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        defaultValue: 0.2,
                        constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
                        groupName: "Model"),
                ],
                description: "Represents the model invocation fed by prompt instructions and trusted context.",
                accentHex: palette.Shading,
                defaultWidth: 300,
                defaultHeight: 230),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.response-parser"),
                "Parser",
                "AI Pipeline",
                "Typed payload",
                [
                    new PortDefinition("response", "Response", new PortTypeId("text"), palette.Color),
                ],
                [
                    new PortDefinition("payload", "Payload", new PortTypeId("json"), palette.Procedure),
                ],
                [
                    new NodeParameterDefinition(
                        "schema",
                        "Schema",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Expected response shape.",
                        defaultValue: "{ \"summary\": string, \"actions\": string[] }",
                        groupName: "Parser",
                        placeholderText: "json schema"),
                    new NodeParameterDefinition(
                        "strict",
                        "Strict",
                        new PortTypeId("bool"),
                        ParameterEditorKind.Boolean,
                        defaultValue: true,
                        groupName: "Parser"),
                ],
                description: "Turns a natural-language model response into a typed host payload.",
                accentHex: palette.Composite,
                defaultWidth: 285,
                defaultHeight: 218),
            new NodeDefinition(
                new NodeDefinitionId("aster.demo.ai-output"),
                "Output",
                "AI Pipeline",
                "Host result",
                [
                    new PortDefinition("payload", "Payload", new PortTypeId("json"), palette.Procedure),
                ],
                [
                ],
                [
                    new NodeParameterDefinition(
                        "exportFormat",
                        "Export Format",
                        new PortTypeId("enum"),
                        ParameterEditorKind.Enum,
                        defaultValue: "json",
                        constraints: new ParameterConstraints(
                            AllowedOptions:
                            [
                                new ParameterOptionDefinition("json", "JSON"),
                                new ParameterOptionDefinition("markdown", "Markdown"),
                                new ParameterOptionDefinition("ticket", "Ticket Update"),
                            ]),
                        groupName: "Output"),
                ],
                description: "Final typed result ready for export or host automation.",
                accentHex: palette.Output,
                defaultWidth: 250,
                defaultHeight: 168),
        ];
    }

    private sealed class Palette
    {
        public string Input { get; } = "#4AD6FF";
        public string Procedure { get; } = "#79E28A";
        public string Color { get; } = "#FFB866";
        public string Composite { get; } = "#FF7A90";
        public string Shading { get; } = "#E0AEFF";
        public string Output { get; } = "#FFF1AA";
        public string Signal { get; } = "#78F0E5";
        public string Highlight { get; } = "#FFD56A";
    }
}
