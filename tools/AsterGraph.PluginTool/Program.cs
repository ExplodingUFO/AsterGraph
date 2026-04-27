using System.Diagnostics;
using System.Text.Json;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.PluginTool;

public static class Program
{
    public static int Main(string[] args)
        => PluginToolProgram.Run(args, Console.Out, Console.Error);
}

public static class PluginToolProgram
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (args.Length == 0 || IsHelp(args[0]))
        {
            WriteUsage(output);
            return args.Length == 0 ? 1 : 0;
        }

        if (string.Equals(args[0], "validate", StringComparison.OrdinalIgnoreCase))
        {
            return RunValidate(args.Skip(1).ToArray(), output, error);
        }

        if (string.Equals(args[0], "inspect", StringComparison.OrdinalIgnoreCase))
        {
            return RunInspect(args.Skip(1).ToArray(), output, error);
        }

        if (string.Equals(args[0], "hash", StringComparison.OrdinalIgnoreCase))
        {
            return RunHash(args.Skip(1).ToArray(), output, error);
        }

        error.WriteLine($"Unknown command '{args[0]}'.");
        WriteUsage(error);
        return 1;
    }

    private static int RunValidate(string[] args, TextWriter output, TextWriter error)
    {
        if (TryParsePluginReportOptions(args, output, error, "validate", out var options, out var helpExitCode))
        {
            return helpExitCode;
        }

        return RunPluginReport(options, output, error, validateMode: true);
    }

    private static int RunInspect(string[] args, TextWriter output, TextWriter error)
    {
        if (TryParsePluginReportOptions(args, output, error, "inspect", out var options, out var helpExitCode))
        {
            return helpExitCode;
        }

        return RunPluginReport(options, output, error, validateMode: false);
    }

    private static int RunPluginReport(PluginReportOptions options, TextWriter output, TextWriter error, bool validateMode)
    {
        var path = Path.GetFullPath(options.Path);
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            error.WriteLine($"Plugin path was not found: {path}");
            return 2;
        }

        var stopwatch = Stopwatch.StartNew();
        IReadOnlyList<GraphEditorPluginCandidateSnapshot> candidates;
        try
        {
            candidates = DiscoverCandidates(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            error.WriteLine($"Plugin validation failed: {exception.Message}");
            return 2;
        }

        stopwatch.Stop();
        var reports = candidates.Select(candidate => PluginReportBuilder.CreateCandidateReport(candidate, options.HostVersion)).ToArray();
        var markers = PluginReportBuilder.CreateProofMarkers(reports);

        if (options.Json)
        {
            var json = JsonSerializer.Serialize(
                new
                {
                    source = path,
                    hostVersion = options.HostVersion,
                    candidates = reports.Select(PluginReportBuilder.CreateJsonCandidate),
                    proofMarkers = markers,
                },
                new JsonSerializerOptions { WriteIndented = true });
            output.WriteLine(json);
        }
        else
        {
            output.WriteLine(validateMode
                ? $"ASTERGRAPH_PLUGIN_VALIDATE:source={path}"
                : $"ASTERGRAPH_PLUGIN_INSPECT:source={path}");
            output.WriteLine(validateMode
                ? $"ASTERGRAPH_PLUGIN_VALIDATE:candidates={candidates.Count}:elapsed_ms={stopwatch.ElapsedMilliseconds}"
                : $"ASTERGRAPH_PLUGIN_INSPECT:candidates={candidates.Count}:elapsed_ms={stopwatch.ElapsedMilliseconds}");

            foreach (var report in reports)
            {
                PluginReportBuilder.WriteCandidate(report, output);
            }

            PluginReportBuilder.WriteProofMarkers(markers, output);
        }

        var passed = candidates.Count > 0
            && reports.All(report => report.Candidate.Compatibility.Status != GraphEditorPluginCompatibilityStatus.Incompatible)
            && markers["PLUGIN_COMPATIBILITY_OK"];

        if (!options.Json)
        {
            output.WriteLine(validateMode
                ? $"ASTERGRAPH_PLUGIN_VALIDATE_OK:{passed}"
                : $"ASTERGRAPH_PLUGIN_INSPECT_OK:{passed}");
        }

        return passed ? 0 : 3;
    }

    private static IReadOnlyList<GraphEditorPluginCandidateSnapshot> DiscoverCandidates(string path)
    {
        if (Directory.Exists(path))
        {
            return AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
            {
                DirectorySources =
                [
                    new GraphEditorPluginDirectoryDiscoverySource(path, "*.dll", includeSubdirectories: false),
                ],
                PackageDirectorySources =
                [
                    new GraphEditorPluginPackageDiscoverySource(path, "*.nupkg", includeSubdirectories: false),
                ],
            });
        }

        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidDataException($"Plugin path does not have a parent directory: {path}");
        var fileName = Path.GetFileName(path);
        var extension = Path.GetExtension(path);

        if (string.Equals(extension, ".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            return AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
            {
                PackageDirectorySources =
                [
                    new GraphEditorPluginPackageDiscoverySource(directory, fileName, includeSubdirectories: false),
                ],
            });
        }

        if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
        {
            return AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
            {
                DirectorySources =
                [
                    new GraphEditorPluginDirectoryDiscoverySource(directory, fileName, includeSubdirectories: false),
                ],
            });
        }

        throw new InvalidDataException("Plugin validation accepts a directory, .dll, or .nupkg path.");
    }

    private static int RunHash(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length != 1 || IsHelp(args[0]))
        {
            WriteHashUsage(args.Length == 1 && IsHelp(args[0]) ? output : error);
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var path = Path.GetFullPath(args[0]);
        if (!File.Exists(path))
        {
            error.WriteLine($"Plugin artifact was not found: {path}");
            return 2;
        }

        output.WriteLine($"PLUGIN_HASH:source={path}");
        output.WriteLine($"PLUGIN_HASH_SHA256:{PluginReportBuilder.ComputeSha256(path)}");
        output.WriteLine("PLUGIN_TRUST_EVIDENCE_OK:True");
        return 0;
    }

    private static bool TryParsePluginReportOptions(
        string[] args,
        TextWriter output,
        TextWriter error,
        string command,
        out PluginReportOptions options,
        out int exitCode)
    {
        options = new PluginReportOptions(string.Empty, null, false);
        exitCode = 1;

        if (args.Length == 1 && IsHelp(args[0]))
        {
            WriteValidateUsage(output, command);
            exitCode = 0;
            return true;
        }

        string? path = null;
        string? hostVersion = null;
        var json = false;
        for (var index = 0; index < args.Length; index++)
        {
            var value = args[index];
            if (string.Equals(value, "--json", StringComparison.OrdinalIgnoreCase))
            {
                json = true;
                continue;
            }

            if (string.Equals(value, "--host-version", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length)
                {
                    error.WriteLine("--host-version requires a value.");
                    WriteValidateUsage(error, command);
                    return true;
                }

                hostVersion = args[++index];
                continue;
            }

            if (path is null)
            {
                path = value;
                continue;
            }

            error.WriteLine($"Unexpected argument '{value}'.");
            WriteValidateUsage(error, command);
            return true;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            WriteValidateUsage(error, command);
            return true;
        }

        options = new PluginReportOptions(path, hostVersion, json);
        exitCode = 0;
        return false;
    }

    private static bool IsHelp(string value)
        => string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "help", StringComparison.OrdinalIgnoreCase);

    private static void WriteUsage(TextWriter writer)
    {
        writer.WriteLine("AsterGraph.PluginTool");
        writer.WriteLine("Trusted in-process plugin validation utilities.");
        writer.WriteLine();
        writer.WriteLine("Commands:");
        writer.WriteLine("  validate <path> [--host-version <version>] [--json]    Validate a plugin directory, .dll, or .nupkg.");
        writer.WriteLine("  inspect <path> [--host-version <version>] [--json]     Inspect plugin manifest, compatibility, trust, and definitions.");
        writer.WriteLine("  hash <path>                                           Print a local plugin artifact SHA-256.");
        writer.WriteLine();
        writer.WriteLine("Validation evidence:");
        writer.WriteLine("  ASTERGRAPH_PLUGIN_VALIDATE_OK:<bool>");
        writer.WriteLine("  PLUGIN_COMPATIBILITY_OK:<bool>");
        writer.WriteLine("  PLUGIN_MANIFEST_OK:<bool>");
        writer.WriteLine("  PLUGIN_NODE_DEFINITIONS_OK:<bool>");
        writer.WriteLine("  PLUGIN_PARAMETER_METADATA_OK:<bool>");
        writer.WriteLine("  PLUGIN_TRUST_EVIDENCE_OK:<bool>");
        writer.WriteLine("  PLUGIN:<id>");
        writer.WriteLine("  target_framework:, capability_summary:, host_compatibility:, trust:, signature:, sha256:");
        writer.WriteLine();
        writer.WriteLine("Non-goals: marketplace distribution, sandboxing, unload/reload, or untrusted-code isolation.");
    }

    private static void WriteValidateUsage(TextWriter writer, string command)
    {
        writer.WriteLine($"Usage: AsterGraph.PluginTool {command} <plugin-directory|plugin.dll|plugin.nupkg> [--host-version <version>] [--json]");
        writer.WriteLine();
        writer.WriteLine("Accepted inputs:");
        writer.WriteLine("  plugin-directory  Scans top-level .dll and .nupkg plugin artifacts.");
        writer.WriteLine("  plugin.dll        Validates one plugin assembly candidate.");
        writer.WriteLine("  plugin.nupkg      Validates one packaged plugin candidate.");
        writer.WriteLine();
        writer.WriteLine("Expected evidence markers:");
        writer.WriteLine($"  ASTERGRAPH_PLUGIN_{command.ToUpperInvariant()}:source=<path>");
        writer.WriteLine($"  ASTERGRAPH_PLUGIN_{command.ToUpperInvariant()}:candidates=<count>:elapsed_ms=<ms>");
        writer.WriteLine($"  ASTERGRAPH_PLUGIN_{command.ToUpperInvariant()}_OK:<bool>");
        writer.WriteLine("  PLUGIN_COMPATIBILITY_OK:<bool>");
        writer.WriteLine("  PLUGIN_MANIFEST_OK:<bool>");
        writer.WriteLine("  PLUGIN_NODE_DEFINITIONS_OK:<bool>");
        writer.WriteLine("  PLUGIN_PARAMETER_METADATA_OK:<bool>");
        writer.WriteLine("  PLUGIN_TRUST_EVIDENCE_OK:<bool>");
        writer.WriteLine("  PLUGIN:<id>");
        writer.WriteLine("  target_framework:, capability_summary:, host_compatibility:, trust:, signature:, sha256:");
        writer.WriteLine();
        writer.WriteLine("This command reports host trust evidence only; it does not approve marketplace distribution, sandbox code, unload plugins, or isolate untrusted code.");
    }

    private static void WriteHashUsage(TextWriter writer)
    {
        writer.WriteLine("Usage: AsterGraph.PluginTool hash <plugin.dll|plugin.nupkg>");
        writer.WriteLine();
        writer.WriteLine("Expected evidence markers:");
        writer.WriteLine("  PLUGIN_HASH:source=<path>");
        writer.WriteLine("  PLUGIN_HASH_SHA256:<sha256>");
        writer.WriteLine("  PLUGIN_TRUST_EVIDENCE_OK:<bool>");
    }

    private sealed record PluginReportOptions(string Path, string? HostVersion, bool Json);
}
