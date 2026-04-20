using AsterGraph.Editor.Events;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private IReadOnlyList<IGraphEditorPluginCommandContributor> _pluginCommandContributors = [];

    internal void SetPluginCommandContributors(IReadOnlyList<IGraphEditorPluginCommandContributor> contributors)
    {
        ArgumentNullException.ThrowIfNull(contributors);
        _pluginCommandContributors = contributors.ToList();
    }

    private IReadOnlyList<GraphEditorCommandDescriptorSnapshot> CollectPluginCommandDescriptors(IReadOnlyCollection<string> reservedCommandIds)
    {
        ArgumentNullException.ThrowIfNull(reservedCommandIds);

        if (_pluginCommandContributors.Count == 0)
        {
            return [];
        }

        var claimedCommandIds = new HashSet<string>(reservedCommandIds, StringComparer.Ordinal);
        var descriptors = new List<GraphEditorCommandDescriptorSnapshot>();
        foreach (var contributor in _pluginCommandContributors)
        {
            var contributedDescriptors = TryGetPluginCommandDescriptors(contributor);
            if (contributedDescriptors.Count == 0)
            {
                continue;
            }

            foreach (var descriptor in contributedDescriptors)
            {
                if (!claimedCommandIds.Add(descriptor.Id))
                {
                    PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                        "plugin.command.duplicate",
                        "plugin.command",
                        $"Plugin command '{descriptor.Id}' was ignored because that command id is already reserved."));
                    continue;
                }

                descriptors.Add(descriptor);
            }
        }

        return descriptors;
    }

    private bool TryExecutePluginCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_pluginCommandContributors.Count == 0)
        {
            return false;
        }

        var claimedCommandIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var contributor in _pluginCommandContributors)
        {
            var contributedDescriptors = TryGetPluginCommandDescriptors(contributor);
            if (contributedDescriptors.Count == 0)
            {
                continue;
            }

            var ownsCommand = false;
            foreach (var descriptor in contributedDescriptors)
            {
                if (!claimedCommandIds.Add(descriptor.Id))
                {
                    continue;
                }

                if (string.Equals(descriptor.Id, command.CommandId, StringComparison.Ordinal))
                {
                    ownsCommand = true;
                }
            }

            if (!ownsCommand)
            {
                continue;
            }

            try
            {
                return contributor.TryExecuteCommand(new GraphEditorPluginCommandExecutionContext(this, command));
            }
            catch (Exception exception)
            {
                PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                    "plugin.command.execute.failed",
                    "plugin.command",
                    $"Plugin command '{command.CommandId}' failed during execution.",
                    exception));
                return false;
            }
        }

        return false;
    }

    private IReadOnlyList<GraphEditorCommandDescriptorSnapshot> TryGetPluginCommandDescriptors(IGraphEditorPluginCommandContributor contributor)
    {
        try
        {
            var descriptors = contributor.GetCommandDescriptors(new GraphEditorPluginCommandContext(this))
                ?? throw new InvalidOperationException(
                    $"Plugin command contributor '{contributor.GetType().FullName}' returned null.");

            return descriptors
                .Select(NormalizePluginCommandDescriptor)
                .ToList();
        }
        catch (Exception exception)
        {
            PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                "plugin.command.describe.failed",
                "plugin.command",
                $"Plugin command contributor failed to produce descriptors: {contributor.GetType().Name}.",
                exception));
            return [];
        }
    }

    private static GraphEditorCommandDescriptorSnapshot NormalizePluginCommandDescriptor(GraphEditorCommandDescriptorSnapshot descriptor)
        => descriptor.Source == GraphEditorCommandSourceKind.Plugin
            ? descriptor
            : new GraphEditorCommandDescriptorSnapshot(
                descriptor.Id,
                descriptor.Title,
                descriptor.Group,
                descriptor.IconKey,
                descriptor.DefaultShortcut,
                GraphEditorCommandSourceKind.Plugin,
                descriptor.IsEnabled,
                descriptor.DisabledReason);
}
