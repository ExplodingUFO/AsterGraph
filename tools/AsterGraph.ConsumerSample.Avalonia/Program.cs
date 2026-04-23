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
        var supportBundleRequested = HasOption(args, "--support-bundle");
        var supportNoteRequested = HasOption(args, "--support-note");
        var supportBundlePath = TryGetOptionValue(args, "--support-bundle");
        var supportNote = TryGetOptionValue(args, "--support-note");
        var proofRequested = args.Any(static arg => string.Equals(arg, "--proof", StringComparison.OrdinalIgnoreCase))
            || !string.IsNullOrWhiteSpace(supportBundlePath);

        if (supportBundleRequested && string.IsNullOrWhiteSpace(supportBundlePath))
        {
            throw new ArgumentException("--support-bundle requires a file path.");
        }

        if (supportNoteRequested && string.IsNullOrWhiteSpace(supportNote))
        {
            throw new ArgumentException("--support-note requires a note value.");
        }

        if (proofRequested)
        {
            var result = ConsumerSampleProof.Run();

            foreach (var line in result.ProofLines)
            {
                Console.WriteLine(line);
            }

            if (!string.IsNullOrWhiteSpace(supportBundlePath))
            {
                try
                {
                    ConsumerSampleSupportBundle.WriteProofBundle(
                        supportBundlePath,
                        result,
                        GetCapturedCommandLine(),
                        supportNote);
                    Console.WriteLine("SUPPORT_BUNDLE_PERSISTENCE_OK:True");
                }
                catch
                {
                    Console.WriteLine("SUPPORT_BUNDLE_PERSISTENCE_OK:False");
                    throw;
                }

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

    private static bool HasOption(IEnumerable<string> args, string optionName)
        => args.Any(arg => string.Equals(arg, optionName, StringComparison.OrdinalIgnoreCase));

    private static string? TryGetOptionValue(IReadOnlyList<string> args, string optionName)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                var candidate = args[index + 1];
                return candidate.StartsWith("--", StringComparison.Ordinal)
                    ? null
                    : candidate;
            }
        }

        return null;
    }

    private static string GetCapturedCommandLine()
        => string.Join(" ", Environment.GetCommandLineArgs().Select(EscapeArgument));

    private static string EscapeArgument(string value)
        => value.Contains(' ', StringComparison.Ordinal)
            ? $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : value;
}
