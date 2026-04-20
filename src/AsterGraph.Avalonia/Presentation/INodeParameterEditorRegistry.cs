using Avalonia.Controls;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// Creates Avalonia controls for parameter-editor bodies without owning tier or validation policy.
/// </summary>
public interface INodeParameterEditorRegistry
{
    Control CreateEditor(NodeParameterEditorRequest request);
}
