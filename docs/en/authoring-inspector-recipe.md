# Authoring Inspector Recipe

Use this recipe when you want the shipped inspector to carry most of the authoring workload.

## What You Define

Start from `NodeParameterDefinition` metadata:

- `editorKind` chooses the stock editor
- `defaultValue` seeds new nodes and fallback projection
- `constraints` declares validation and read-only behavior
- `groupName` groups parameters into inspector sections
- `placeholderText` provides short input hints for text-oriented editors

Current shipped editor kinds:

- `Text`
- `Number`
- `Boolean`
- `Enum`
- `Color`
- `List`

## What The Shipped Inspector Does

When a host uses the default Avalonia surfaces:

- parameters are projected from the selected node definition
- shared definitions across a multi-selection enable batch editing
- validation errors are surfaced inline
- read-only constraints are shown explicitly
- list parameters use a multiline one-item-per-line editor

## Minimal Definition Example

```csharp
var definition = new NodeDefinition(
    new NodeDefinitionId("sample.authoring.node"),
    "Authoring Node",
    "Samples",
    "Inspector",
    [],
    [],
    parameters:
    [
        new NodeParameterDefinition(
            "threshold",
            "Threshold",
            new PortTypeId("float"),
            ParameterEditorKind.Number,
            defaultValue: 0.5d,
            constraints: new ParameterConstraints(Minimum: 0, Maximum: 1),
            groupName: "Behavior"),
        new NodeParameterDefinition(
            "slug",
            "Slug",
            new PortTypeId("string"),
            ParameterEditorKind.Text,
            defaultValue: "authoring-node",
            constraints: new ParameterConstraints(
                MinimumLength: 3,
                ValidationPattern: "^[a-z-]+$",
                ValidationPatternDescription: "lowercase letters and dashes"),
            groupName: "Metadata",
            placeholderText: "authoring-node"),
        new NodeParameterDefinition(
            "tags",
            "Tags",
            new PortTypeId("string-list"),
            ParameterEditorKind.List,
            defaultValue: new[] { "alpha", "beta" },
            constraints: new ParameterConstraints(MinimumItemCount: 1, MaximumItemCount: 5),
            groupName: "Metadata",
            placeholderText: "one tag per line"),
    ]);
```

## Where To See It Running

- smallest hosted sample: [`tools/AsterGraph.HelloWorld.Avalonia`](../../tools/AsterGraph.HelloWorld.Avalonia/)
- full showcase host: [`src/AsterGraph.Demo`](../../src/AsterGraph.Demo/)
- realistic hosted integration: [Consumer Sample](./consumer-sample.md)

## When To Extend It Yourself

Stay on the shipped inspector when your host only needs common authoring controls and predictable validation.

Build custom presentation on top when you need:

- domain-specific composite editors
- cross-parameter workflows that should not be expressed as one field at a time
- host-owned panels that combine graph editing with business-specific review UI
