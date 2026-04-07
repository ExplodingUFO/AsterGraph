using CommunityToolkit.Mvvm.Input;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Menus;

internal static class GraphContextMenuCompatibilityAdapter
{
    public static IReadOnlyList<MenuItemDescriptor> Adapt(
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
        IGraphEditorCommands commands)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(commands);

        return descriptors.Select(descriptor => Adapt(descriptor, commands)).ToList();
    }

    private static MenuItemDescriptor Adapt(
        GraphEditorMenuItemDescriptorSnapshot descriptor,
        IGraphEditorCommands commands)
    {
        if (descriptor.IsSeparator)
        {
            return MenuItemDescriptor.Separator(descriptor.Id);
        }

        IRelayCommand? command = null;
        if (descriptor.Command is not null)
        {
            command = new RelayCommand(
                () => commands.TryExecuteCommand(descriptor.Command),
                () => descriptor.IsEnabled);
        }

        return new MenuItemDescriptor(
            descriptor.Id,
            descriptor.Header,
            command,
            children: descriptor.Children.Select(child => Adapt(child, commands)).ToList(),
            iconKey: descriptor.IconKey,
            disabledReason: descriptor.DisabledReason,
            isEnabled: descriptor.IsEnabled,
            isSeparator: descriptor.IsSeparator);
    }
}
