using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

public sealed record CompatiblePortTarget(
    NodeViewModel Node,
    PortViewModel Port,
    PortCompatibilityResult Compatibility);
