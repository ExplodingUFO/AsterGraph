using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace AsterGraph.Avalonia.Controls.Internal.Automation;

internal sealed class NodeCanvasAutomationPeer : ControlAutomationPeer
{
    public NodeCanvasAutomationPeer(Control owner)
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Group;

    protected override bool IsContentElementCore()
        => true;

    protected override bool IsControlElementCore()
        => true;
}
