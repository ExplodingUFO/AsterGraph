using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.ViewModels;

Console.WriteLine($"Package smoke: {typeof(GraphEditorView).FullName}");
Console.WriteLine($"Identifier smoke: {new NodeDefinitionId("smoke.node").Value}");
Console.WriteLine($"Menu augmentor smoke: {typeof(IGraphContextMenuAugmentor).FullName}");
Console.WriteLine($"Permissions smoke: {GraphEditorCommandPermissions.ReadOnly.Host.AllowContextMenuExtensions}");
Console.WriteLine($"Position smoke: {new NodePositionSnapshot("smoke-node", new GraphPoint(12, 34)).NodeId}");
Console.WriteLine($"ViewModel smoke: {typeof(GraphEditorViewModel).FullName}");
