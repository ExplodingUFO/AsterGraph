using AsterGraph.Demo.ViewModels;

namespace AsterGraph.Demo;

public sealed record DemoStartupOptions(MainWindowShellOptions ShellOptions, string[] AvaloniaArgs);

public static class DemoStartupOptionsParser
{
    public static DemoStartupOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string? scenarioName = null;
        var avaloniaArgs = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--scenario", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    throw new ArgumentException("The --scenario option requires a scenario name.", nameof(args));
                }

                scenarioName = SetScenarioName(scenarioName, args[++index]);
                continue;
            }

            const string scenarioPrefix = "--scenario=";
            if (arg.StartsWith(scenarioPrefix, StringComparison.OrdinalIgnoreCase))
            {
                scenarioName = SetScenarioName(scenarioName, arg[scenarioPrefix.Length..]);
                continue;
            }

            avaloniaArgs.Add(arg);
        }

        var shellOptions = MainWindowShellOptions.CreatePersistentDefault();
        if (!string.IsNullOrWhiteSpace(scenarioName))
        {
            if (!DemoGraphFactory.IsKnownScenario(scenarioName))
            {
                throw new ArgumentException($"Unknown demo scenario '{scenarioName}'.", nameof(args));
            }

            shellOptions = shellOptions with
            {
                RestoreLastWorkspaceOnStartup = false,
                InitialScenario = DemoGraphFactory.AiPipelineScenario,
            };
        }

        return new DemoStartupOptions(shellOptions, avaloniaArgs.ToArray());
    }

    private static string SetScenarioName(string? current, string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            throw new ArgumentException("The --scenario option requires a scenario name.", nameof(candidate));
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            throw new ArgumentException("Only one --scenario option can be provided.", nameof(candidate));
        }

        return candidate.Trim();
    }
}
