using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;

Console.WriteLine($"Package smoke: {typeof(GraphEditorView).FullName}");
Console.WriteLine($"Identifier smoke: {new NodeDefinitionId("smoke.node").Value}");
