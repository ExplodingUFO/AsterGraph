using System.Windows;
using System.Windows.Threading;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Starter.Wpf;
using Xunit;

namespace AsterGraph.Wpf.Tests;

public sealed class StarterAndRicherWpfSampleTests
{
    [Fact]
    public void StarterWpfSample_ComposesSessionOwnershipContract()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var (surface, editor) = WpfRouteTestHelpers.RunInSta(() =>
        {
            var sampleSurface = StarterWpfWindowFactory.CreateRuntimeSurface();
            ConfigureWindowForHeadless(sampleSurface.Window);
            sampleSurface.Window.Show();
            sampleSurface.Window.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);

            return (sampleSurface, sampleSurface.Editor);
        });

        Assert.Same(editor.Session, surface.Session);
        Assert.NotNull(editor.HostContext);
        Assert.Same(editor.HostContext.Owner, surface.View);
    }

    [Fact]
    public void HelloWorldWpfRicherSample_ComposesSessionOwnershipAndProofMarkers()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var proof = HostedHelloWorldProof.Run();
        Assert.True(proof.IsOk);
        Assert.True(proof.CommandSurfaceOk);
        Assert.True(proof.AccessibilityBaselineOk);
        Assert.True(proof.AccessibilityFocusOk);
        Assert.True(proof.AccessibilityCommandSurfaceOk);
        Assert.True(proof.AccessibilityAuthoringSurfaceOk);
        Assert.True(proof.HostedAccessibilityOk);
        Assert.True(proof.Adapter2PerformanceBaselineOk);

        var metricLines = proof.MetricLines;
        Assert.Contains(metricLines, line => line.Contains("startup_ms", StringComparison.Ordinal));
        Assert.Contains(metricLines, line => line.Contains("inspector_projection_ms", StringComparison.Ordinal));
        Assert.Contains(metricLines, line => line.Contains("plugin_scan_ms", StringComparison.Ordinal));
        Assert.Contains(metricLines, line => line.Contains("command_latency_ms", StringComparison.Ordinal));
        Assert.Contains(proof.ProofLines, line => line == "ADAPTER2_PERFORMANCE_BASELINE_OK:True");
        Assert.Contains(proof.ProofLines, line => line == "HOSTED_ACCESSIBILITY_BASELINE_OK:True");
        Assert.Contains(proof.ProofLines, line => line == "HOSTED_ACCESSIBILITY_FOCUS_OK:True");
        Assert.Contains(proof.ProofLines, line => line == "HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True");
        Assert.Contains(proof.ProofLines, line => line == "HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True");
        Assert.Contains(proof.ProofLines, line => line == "HOSTED_ACCESSIBILITY_OK:True");

        var (surface, editor) = WpfRouteTestHelpers.RunInSta(() =>
        {
            var sampleSurface = HostedHelloWorldWindowFactory.CreateRuntimeSurface();
            ConfigureWindowForHeadless(sampleSurface.Window);
            sampleSurface.Window.Show();
            sampleSurface.Window.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);

            return (sampleSurface, sampleSurface.Editor);
        });

        Assert.Same(editor.Session, surface.Session);
        Assert.NotNull(editor.HostContext);
        Assert.Same(editor.HostContext.Owner, surface.View);
    }

    private static void ConfigureWindowForHeadless(Window window)
    {
        window.Width = 200;
        window.Height = 120;
        window.ShowInTaskbar = false;
        window.WindowStyle = WindowStyle.None;
        window.ResizeMode = ResizeMode.NoResize;
    }
}
