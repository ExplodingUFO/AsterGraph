using System.Diagnostics;
using System.Security.Cryptography;
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

        if (!string.Equals(args[0], "validate", StringComparison.OrdinalIgnoreCase))
        {
            error.WriteLine($"Unknown command '{args[0]}'.");
            WriteUsage(error);
            return 1;
        }

        return RunValidate(args.Skip(1).ToArray(), output, error);
    }

    private static int RunValidate(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length != 1 || IsHelp(args[0]))
        {
            WriteValidateUsage(args.Length == 1 && IsHelp(args[0]) ? output : error);
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var path = Path.GetFullPath(args[0]);
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

        output.WriteLine($"ASTERGRAPH_PLUGIN_VALIDATE:source={path}");
        output.WriteLine($"ASTERGRAPH_PLUGIN_VALIDATE:candidates={candidates.Count}:elapsed_ms={stopwatch.ElapsedMilliseconds}");

        foreach (var candidate in candidates)
        {
            WriteCandidate(candidate, output);
        }

        var passed = candidates.Count > 0
            && candidates.All(candidate => candidate.Compatibility.Status != GraphEditorPluginCompatibilityStatus.Incompatible);

        output.WriteLine($"ASTERGRAPH_PLUGIN_VALIDATE_OK:{passed}");
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

    private static void WriteCandidate(GraphEditorPluginCandidateSnapshot candidate, TextWriter output)
    {
        var manifest = candidate.Manifest;
        var compatibility = manifest.Compatibility;
        var evidence = candidate.ProvenanceEvidence;
        var artifactPath = candidate.PackagePath ?? candidate.AssemblyPath;

        output.WriteLine($"PLUGIN:{manifest.Id}");
        output.WriteLine($"  display_name: {manifest.DisplayName}");
        output.WriteLine($"  version: {manifest.Version ?? "unspecified"}");
        output.WriteLine($"  source_kind: {candidate.SourceKind}");
        output.WriteLine($"  source: {candidate.Source}");
        output.WriteLine($"  assembly: {candidate.AssemblyPath ?? "none"}");
        output.WriteLine($"  package: {candidate.PackagePath ?? "none"}");
        output.WriteLine($"  plugin_type: {candidate.PluginTypeName ?? "auto"}");
        output.WriteLine($"  target_framework: {compatibility.TargetFramework ?? "unspecified"}");
        output.WriteLine($"  runtime_surface: {compatibility.RuntimeSurface ?? "unspecified"}");
        output.WriteLine($"  capability_summary: {manifest.CapabilitySummary ?? "unspecified"}");
        output.WriteLine($"  compatibility: {candidate.Compatibility.Status}:{candidate.Compatibility.ReasonCode ?? "none"}");
        output.WriteLine($"  trust: {candidate.TrustEvaluation.Decision}:{candidate.TrustEvaluation.Source}:{candidate.TrustEvaluation.ReasonCode ?? "none"}");
        output.WriteLine($"  signature: {evidence.Signature.Status}:{evidence.Signature.ReasonCode ?? "none"}");
        output.WriteLine($"  sha256: {(artifactPath is null ? "unavailable" : ComputeSha256(artifactPath))}");
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool IsHelp(string value)
        => string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "help", StringComparison.OrdinalIgnoreCase);

    private static void WriteUsage(TextWriter writer)
    {
        writer.WriteLine("AsterGraph.PluginTool");
        writer.WriteLine();
        writer.WriteLine("Commands:");
        writer.WriteLine("  validate <path>    Inspect a plugin directory, .dll, or .nupkg.");
    }

    private static void WriteValidateUsage(TextWriter writer)
        => writer.WriteLine("Usage: AsterGraph.PluginTool validate <plugin-directory|plugin.dll|plugin.nupkg>");
}
