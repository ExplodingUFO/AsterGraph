using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Plugins;
using System;
using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> _pluginContextMenuAugmentors = [];
    private IReadOnlyList<GraphEditorPluginLoadSnapshot> _pluginLoadSnapshots = [];
    private bool _hasPluginTrustPolicy;

    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildContextMenuDescriptors(ContextMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commands = GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var stockItems = _stockMenuDescriptorBuilder.Build(context, commands);

        if (_pluginContextMenuAugmentors.Count == 0)
        {
            return stockItems;
        }

        var currentItems = stockItems;
        foreach (var augmentor in _pluginContextMenuAugmentors)
        {
            try
            {
                currentItems = augmentor.Augment(
                    new GraphEditorPluginMenuAugmentationContext(
                        this,
                        context,
                        currentItems))
                    ?? throw new InvalidOperationException(
                        $"Plugin context menu augmentor '{augmentor.GetType().FullName}' returned null.");
            }
            catch (Exception exception)
            {
                PublishRecoverableFailure(new GraphEditorRecoverableFailureEventArgs(
                    "plugin.contextmenu.augment.failed",
                    "plugin.contextmenu.augment",
                    $"Plugin context menu augmentor failed: {augmentor.GetType().Name}. Using current menu.",
                    exception));
            }
        }

        return currentItems;
    }

    internal void SetPluginLoadSnapshots(IReadOnlyList<GraphEditorPluginLoadSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _pluginLoadSnapshots = snapshots.ToList();
    }

    internal void SetPluginContextMenuAugmentors(IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> augmentors)
    {
        ArgumentNullException.ThrowIfNull(augmentors);
        _pluginContextMenuAugmentors = augmentors.ToList();
    }

    internal void SetPluginTrustPolicyConfigured(bool isConfigured)
        => _hasPluginTrustPolicy = isConfigured;

    private string Localize(string key, string fallback)
        => _descriptorSupport?.Localize(key, fallback) ?? fallback;
}
