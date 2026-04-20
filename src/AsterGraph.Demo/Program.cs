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

            foreach (var line in result.ProofLines)
            {
                Console.WriteLine(line);
            }
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
