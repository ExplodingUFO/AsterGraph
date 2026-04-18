using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace AsterGraph.ConsumerSample;

public sealed class ConsumerSampleApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ConsumerSampleWindowFactory.Create();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public static class ConsumerSampleAppBuilder
{
    public static AppBuilder BuildDesktopApp()
        => AppBuilder.Configure<ConsumerSampleApp>()
            .UsePlatformDetect();
}

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Any(static arg => string.Equals(arg, "--proof", StringComparison.OrdinalIgnoreCase)))
        {
            var result = ConsumerSampleProof.Run();

            Console.WriteLine($"CONSUMER_SAMPLE_HOST_ACTION_OK:{result.HostMenuActionOk}");
            Console.WriteLine($"CONSUMER_SAMPLE_PLUGIN_OK:{result.PluginContributionOk}");
            Console.WriteLine($"CONSUMER_SAMPLE_PARAMETER_OK:{result.ParameterEditingOk}");
            Console.WriteLine($"CONSUMER_SAMPLE_WINDOW_OK:{result.WindowCompositionOk}");
            Console.WriteLine($"CONSUMER_SAMPLE_OK:{result.IsOk}");

            if (!result.IsOk)
            {
                throw new InvalidOperationException("Consumer sample proof failed.");
            }

            return;
        }

        ConsumerSampleAppBuilder.BuildDesktopApp().StartWithClassicDesktopLifetime(args);
    }
}
