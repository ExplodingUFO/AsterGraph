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
        var supportBundlePath = TryGetOptionValue(args, "--support-bundle");
        var supportNote = TryGetOptionValue(args, "--support-note");
        var proofRequested = args.Any(static arg => string.Equals(arg, "--proof", StringComparison.OrdinalIgnoreCase))
            || !string.IsNullOrWhiteSpace(supportBundlePath);

        if (proofRequested)
        {
            var result = ConsumerSampleProof.Run();

            foreach (var line in result.ProofLines)
            {
                Console.WriteLine(line);
            }

            if (!string.IsNullOrWhiteSpace(supportBundlePath))
            {
                ConsumerSampleSupportBundle.WriteProofBundle(
                    supportBundlePath,
                    result,
                    BuildSupportCommand(args),
                    supportNote);

                Console.WriteLine("SUPPORT_BUNDLE_OK:True");
                Console.WriteLine($"SUPPORT_BUNDLE_PATH:{Path.GetFullPath(supportBundlePath)}");
            }

            if (!result.IsOk)
            {
                throw new InvalidOperationException("Consumer sample proof failed.");
            }

            return;
        }

        ConsumerSampleAppBuilder.BuildDesktopApp().StartWithClassicDesktopLifetime(args);
    }

    private static string? TryGetOptionValue(IReadOnlyList<string> args, string optionName)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static string BuildSupportCommand(IEnumerable<string> args)
    {
        const string prefix = "dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo";
        var arguments = args.ToArray();
        return arguments.Length == 0
            ? prefix
            : $"{prefix} -- {string.Join(" ", arguments.Select(EscapeArgument))}";
    }

    private static string EscapeArgument(string value)
        => value.Contains(' ', StringComparison.Ordinal)
            ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : value;
}
