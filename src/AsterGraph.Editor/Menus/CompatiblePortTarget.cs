using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// Describes one compatible target port for a pending connection.
/// </summary>
/// <remarks>
/// This MVVM-oriented shape remains as a retained compatibility shim for existing host integrations.
/// New canonical runtime queries should prefer the DTO-based query path in
/// <c>IGraphEditorQueries.GetCompatiblePortTargets(string, string)</c>.
/// The v1.5 migration window keeps this shim, later minor releases may add stronger warnings,
/// and a future major release may remove it.
/// </remarks>
/// <param name="Node">Target node view model.</param>
/// <param name="Port">Target port view model.</param>
/// <param name="Compatibility">Compatibility result for the connection.</param>
[Obsolete("Retained compatibility shim. Use GraphEditorCompatiblePortTargetSnapshot via IGraphEditorQueries.GetCompatiblePortTargets(string, string) for canonical runtime queries. The v1.5 migration window keeps this shim, later minor releases may add stronger warnings, and a future major release may remove it.")]
public sealed record CompatiblePortTarget(
    NodeViewModel Node,
    PortViewModel Port,
    PortCompatibilityResult Compatibility);
