using Xunit;

namespace AsterGraph.HelloWorld.Avalonia.Tests;

public sealed class HostedHelloWorldProofTests
{
    [Fact]
    public void HostedHelloWorldProof_Run_EmitsMetricsAndCommandSurfaceMarker()
    {
        var result = HostedHelloWorldProof.Run();

        Assert.True(result.IsOk);
        Assert.True(result.CommandSurfaceOk);
        Assert.True(result.StartupMs >= 0);
        Assert.True(result.InspectorProjectionMs >= 0);
        Assert.True(result.PluginScanMs >= 0);
        Assert.True(result.CommandLatencyMs >= 0);
        Assert.Contains(result.MetricLines, line => line.Contains("startup_ms", StringComparison.Ordinal));
        Assert.Contains(result.MetricLines, line => line.Contains("command_latency_ms", StringComparison.Ordinal));
    }
}
