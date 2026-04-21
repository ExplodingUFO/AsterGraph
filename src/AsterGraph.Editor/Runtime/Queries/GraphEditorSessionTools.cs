using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private IGraphEditorToolProvider? _toolProvider;

    internal void SetToolProvider(IGraphEditorToolProvider? toolProvider)
        => _toolProvider = toolProvider;

    public IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolContextSnapshot context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commands = GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var tools = _stockToolDescriptorBuilder.Build(context, commands).ToList();
        tools.AddRange(TryGetHostToolDescriptors(context));

        return tools
            .GroupBy(tool => tool.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(tool => tool.Order)
            .ThenBy(tool => tool.Group, StringComparer.Ordinal)
            .ThenBy(tool => tool.Title, StringComparer.Ordinal)
            .ToList();
    }

    private IReadOnlyList<GraphEditorToolDescriptorSnapshot> TryGetHostToolDescriptors(GraphEditorToolContextSnapshot context)
    {
        if (_toolProvider is null)
        {
            return [];
        }

        try
        {
            var descriptors = _toolProvider.GetToolDescriptors(new GraphEditorToolProviderContext(this, context))
                ?? throw new InvalidOperationException(
                    $"Tool provider '{_toolProvider.GetType().FullName}' returned null.");

            return descriptors
                .Where(descriptor => descriptor.ContextKind == context.Kind)
                .ToList();
        }
        catch (Exception exception)
        {
            PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                "tool.provider.describe.failed",
                "tool.provider",
                $"Tool provider failed to produce descriptors for '{context.Kind}'.",
                exception));
            return [];
        }
    }
}
