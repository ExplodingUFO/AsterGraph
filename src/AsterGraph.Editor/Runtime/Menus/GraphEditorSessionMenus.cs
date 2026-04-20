using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Plugins;
using System;
using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private IReadOnlyList<GraphEditorPluginLoadSnapshot> _pluginLoadSnapshots = [];
    private bool _hasPluginTrustPolicy;

    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> BuildContextMenuDescriptors(ContextMenuContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commands = GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        return _stockMenuDescriptorBuilder.Build(context, commands);
    }

    internal void SetPluginLoadSnapshots(IReadOnlyList<GraphEditorPluginLoadSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _pluginLoadSnapshots = snapshots.ToList();
    }

    internal void SetPluginTrustPolicyConfigured(bool isConfigured)
        => _hasPluginTrustPolicy = isConfigured;

    private string Localize(string key, string fallback)
        => _descriptorSupport?.Localize(key, fallback) ?? fallback;
}
