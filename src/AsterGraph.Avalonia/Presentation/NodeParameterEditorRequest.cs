using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Context passed to Avalonia parameter-editor registries when a surface needs an editor body.
/// </summary>
public sealed record NodeParameterEditorRequest(
    NodeParameterViewModel Parameter,
    string? TemplateKey,
    NodeParameterEditorUsage Usage);
