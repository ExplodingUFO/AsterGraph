using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace AsterGraph.Avalonia.Controls.Internal.Automation;

internal sealed class GraphNodeBorder : Border
{
    protected override AutomationPeer OnCreateAutomationPeer()
        => new GraphNodeAutomationPeer(this);
}
