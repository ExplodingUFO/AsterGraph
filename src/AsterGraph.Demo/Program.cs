using Avalonia;
using System;
using System.Linq;

namespace AsterGraph.Demo;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Any(static arg => string.Equals(arg, "--proof", StringComparison.OrdinalIgnoreCase)))
        {
            var result = DemoProof.Run();

            Console.WriteLine($"DEMO_TRUST_OK:{result.TrustTransparencyOk}");
            Console.WriteLine($"DEMO_SHELL_OK:{result.ShellWorkflowOk}");
            Console.WriteLine($"COMMAND_SURFACE_OK:{result.CommandSurfaceOk}");
            Console.WriteLine($"PROGRESSIVE_NODE_SURFACE_OK:{result.ProgressiveNodeSurfaceOk}");
            foreach (var line in result.MetricLines)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine($"DEMO_OK:{result.IsOk}");
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
