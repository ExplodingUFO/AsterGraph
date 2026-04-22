using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace AsterGraph.ConsumerSample;

public static class ConsumerSampleSupportBundle
{
    public static void WriteProofBundle(
        string outputPath,
        ConsumerSampleProofResult result,
        string command,
        string? note = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        var fullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Environment.CurrentDirectory);

        var packageVersion = GetPackageVersion();
        var document = new ConsumerSampleSupportBundleDocument(
            SchemaVersion: 1,
            PackageVersion: packageVersion,
            PublicTag: $"v{packageVersion}",
            Route: "ConsumerSample.Avalonia",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            ProofLines: result.ProofLines,
            Environment: new ConsumerSampleSupportEnvironment(
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                RuntimeInformation.OSArchitecture.ToString(),
                RuntimeInformation.ProcessArchitecture.ToString()),
            Reproduction: new ConsumerSampleSupportReproduction(
                command,
                Environment.CurrentDirectory,
                note));

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        File.WriteAllText(fullPath, JsonSerializer.Serialize(document, options));
    }

    private static string GetPackageVersion()
    {
        var informationalVersion = typeof(ConsumerSampleSupportBundle).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return typeof(ConsumerSampleSupportBundle).Assembly.GetName().Version?.ToString() ?? "0.0.0-local";
        }

        var buildMetadataIndex = informationalVersion.IndexOf('+');
        return buildMetadataIndex >= 0
            ? informationalVersion[..buildMetadataIndex]
            : informationalVersion;
    }

    private sealed record ConsumerSampleSupportBundleDocument(
        int SchemaVersion,
        string PackageVersion,
        string PublicTag,
        string Route,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<string> ProofLines,
        ConsumerSampleSupportEnvironment Environment,
        ConsumerSampleSupportReproduction Reproduction);

    private sealed record ConsumerSampleSupportEnvironment(
        string FrameworkDescription,
        string OsDescription,
        string OsArchitecture,
        string ProcessArchitecture);

    private sealed record ConsumerSampleSupportReproduction(
        string Command,
        string WorkingDirectory,
        string? Note);
}
