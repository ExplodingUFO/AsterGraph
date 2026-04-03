using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// Describes one compatible target port for a pending connection.
/// </summary>
/// <param name="Node">Target node view model.</param>
/// <param name="Port">Target port view model.</param>
/// <param name="Compatibility">Compatibility result for the connection.</param>
public sealed record CompatiblePortTarget(
    NodeViewModel Node,
    PortViewModel Port,
    PortCompatibilityResult Compatibility);
