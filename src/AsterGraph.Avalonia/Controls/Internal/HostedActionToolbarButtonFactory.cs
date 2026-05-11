using Avalonia.Automation;
using Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class HostedActionToolbarButtonFactory
{
    public static Button Create(string name, AsterGraphHostedActionDescriptor action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(action);

        var button = new Button
        {
            Name = name,
            Content = action.Title,
            IsEnabled = action.CanExecute,
        };

        var tooltip = CreateTooltip(action.DisabledReason, action.RecoveryHint);
        if (!string.IsNullOrWhiteSpace(tooltip))
        {
            ToolTip.SetTip(button, tooltip);
        }

        button.Classes.Add("astergraph-toolbar-action");
        AutomationProperties.SetName(button, action.Title);
        button.Click += (_, args) =>
        {
            action.TryExecute();
            args.Handled = true;
        };
        return button;
    }

    private static string? CreateTooltip(string? disabledReason, string? recoveryHint)
    {
        if (string.IsNullOrWhiteSpace(recoveryHint))
        {
            return disabledReason;
        }

        return string.IsNullOrWhiteSpace(disabledReason)
            ? recoveryHint
            : $"{disabledReason}\n{recoveryHint}";
    }
}
