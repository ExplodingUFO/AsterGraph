using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.ConsumerSample;

internal static class ConsumerSampleSupportBundle
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
            SchemaVersion: 5,
            PackageVersion: packageVersion,
            PublicTag: $"v{packageVersion}",
            Route: "ConsumerSample.Avalonia",
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            PersistenceStatus: "written",
            ProofLines: result.ProofLines,
            ParameterSnapshots: result.ParameterSnapshots,
            Environment: new ConsumerSampleSupportEnvironment(
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                RuntimeInformation.OSArchitecture.ToString(),
                RuntimeInformation.ProcessArchitecture.ToString()),
            Reproduction: new ConsumerSampleSupportReproduction(
                command,
                Environment.CurrentDirectory,
                note),
            GraphSummary: new ConsumerSampleSupportGraphSummary(
                result.NodeCount,
                result.ConnectionCount),
            ReadinessStatus: result.SupportReadinessStatus,
            ValidationSummary: result.SupportValidationSummary,
            ValidationFeedback: result.SupportValidationFeedback,
            RepairEvidence: result.RepairEvidence ?? [],
            FeatureDescriptors: result.FeatureDescriptorIds ?? [],
            RecentsFavorites: result.RecentsFavoritesEvidence ?? [],
            WorkbenchFrictionEvidence: result.WorkbenchFrictionEvidence ?? [],
            WorkbenchAffordancePolish: result.WorkbenchAffordancePolish,
            RecentDiagnostics: result.RecentDiagnosticCodes ?? [],
            RuntimeNodeOverlays: result.RuntimeNodeOverlays ?? [],
            RuntimeConnectionOverlays: result.RuntimeConnectionOverlays ?? [],
            RuntimeLogs: result.RuntimeLogs ?? []);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var json = JsonSerializer.Serialize(document, options);
        File.WriteAllText(fullPath, json);

        ValidateBundleSchema(fullPath);
    }

    private static void ValidateBundleSchema(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        var requiredProperties = new[]
        {
            "schemaVersion",
            "packageVersion",
            "publicTag",
            "route",
            "generatedAtUtc",
            "persistenceStatus",
            "proofLines",
            "parameterSnapshots",
            "environment",
            "reproduction",
            "graphSummary",
            "readinessStatus",
            "validationSummary",
            "validationFeedback",
            "repairEvidence",
            "featureDescriptors",
            "recentsFavorites",
            "workbenchFrictionEvidence",
            "workbenchAffordancePolish",
            "recentDiagnostics",
            "runtimeNodeOverlays",
            "runtimeConnectionOverlays",
            "runtimeLogs",
        };

        foreach (var property in requiredProperties)
        {
            if (!root.TryGetProperty(property, out _))
            {
                throw new InvalidOperationException($"Support bundle schema validation failed: missing required property '{property}'.");
            }
        }

        var graphSummary = root.GetProperty("graphSummary");
        if (!graphSummary.TryGetProperty("nodeCount", out _) || !graphSummary.TryGetProperty("connectionCount", out _))
        {
            throw new InvalidOperationException("Support bundle schema validation failed: graphSummary missing nodeCount or connectionCount.");
        }

        var readinessStatus = root.GetProperty("readinessStatus").GetString();
        if (readinessStatus is not ("Ready" or "Warnings" or "Blocked"))
        {
            throw new InvalidOperationException("Support bundle schema validation failed: readinessStatus must be Ready, Warnings, or Blocked.");
        }

        var validationSummary = root.GetProperty("validationSummary");
        foreach (var property in new[] { "totalIssueCount", "errorCount", "warningCount", "invalidConnectionCount", "invalidParameterCount" })
        {
            if (!validationSummary.TryGetProperty(property, out _))
            {
                throw new InvalidOperationException($"Support bundle schema validation failed: validationSummary missing {property}.");
            }
        }

        var validationFeedback = root.GetProperty("validationFeedback");
        if (validationFeedback.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Support bundle schema validation failed: validationFeedback must be an array.");
        }

        if (validationFeedback.GetArrayLength() != validationSummary.GetProperty("totalIssueCount").GetInt32())
        {
            throw new InvalidOperationException("Support bundle schema validation failed: validationFeedback count does not match validationSummary totalIssueCount.");
        }

        foreach (var feedback in validationFeedback.EnumerateArray())
        {
            if (!feedback.TryGetProperty("code", out _)
                || !feedback.TryGetProperty("severity", out _)
                || !feedback.TryGetProperty("message", out _)
                || !feedback.TryGetProperty("focusTarget", out var focusTarget)
                || !focusTarget.TryGetProperty("kind", out _))
            {
                throw new InvalidOperationException("Support bundle schema validation failed: validationFeedback rows require code, severity, message, and focusTarget.kind.");
            }
        }

        var repairEvidence = root.GetProperty("repairEvidence");
        if (repairEvidence.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Support bundle schema validation failed: repairEvidence must be an array.");
        }

        foreach (var evidence in repairEvidence.EnumerateArray())
        {
            if (!evidence.TryGetProperty("issueCode", out _)
                || !evidence.TryGetProperty("target", out _)
                || !evidence.TryGetProperty("action", out _)
                || !evidence.TryGetProperty("result", out _))
            {
                throw new InvalidOperationException("Support bundle schema validation failed: repairEvidence rows require issueCode, target, action, and result.");
            }
        }

        var frictionEvidence = root.GetProperty("workbenchFrictionEvidence");
        if (frictionEvidence.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Support bundle schema validation failed: workbenchFrictionEvidence must be an array.");
        }

        foreach (var evidence in frictionEvidence.EnumerateArray())
        {
            if (!evidence.TryGetProperty("category", out _)
                || !evidence.TryGetProperty("evidence", out _)
                || !evidence.TryGetProperty("priorityRank", out _)
                || !evidence.TryGetProperty("route", out _)
                || !evidence.TryGetProperty("scopeBoundary", out _)
                || !evidence.TryGetProperty("isSynthetic", out _))
            {
                throw new InvalidOperationException("Support bundle schema validation failed: workbenchFrictionEvidence rows require category, evidence, priorityRank, route, scopeBoundary, and isSynthetic.");
            }
        }

        var affordancePolish = root.GetProperty("workbenchAffordancePolish");
        if (affordancePolish.ValueKind == JsonValueKind.Null)
        {
            return;
        }

        if (affordancePolish.ValueKind != JsonValueKind.Object
            || !affordancePolish.TryGetProperty("actionId", out _)
            || !affordancePolish.TryGetProperty("frictionCategory", out _)
            || !affordancePolish.TryGetProperty("route", out _)
            || !affordancePolish.TryGetProperty("scopeBoundary", out _))
        {
            throw new InvalidOperationException("Support bundle schema validation failed: workbenchAffordancePolish requires actionId, frictionCategory, route, and scopeBoundary.");
        }
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
        string PersistenceStatus,
        IReadOnlyList<string> ProofLines,
        IReadOnlyList<ConsumerSampleProofParameterSnapshot> ParameterSnapshots,
        ConsumerSampleSupportEnvironment Environment,
        ConsumerSampleSupportReproduction Reproduction,
        ConsumerSampleSupportGraphSummary GraphSummary,
        string ReadinessStatus,
        ConsumerSampleProofValidationSummary ValidationSummary,
        IReadOnlyList<ConsumerSampleProofValidationFeedback> ValidationFeedback,
        IReadOnlyList<ConsumerSampleRepairEvidence> RepairEvidence,
        IReadOnlyList<string> FeatureDescriptors,
        IReadOnlyList<ConsumerSampleRecentsFavoritesEvidence> RecentsFavorites,
        IReadOnlyList<ConsumerSampleWorkbenchFrictionEvidence> WorkbenchFrictionEvidence,
        ConsumerSampleWorkbenchAffordancePolish? WorkbenchAffordancePolish,
        IReadOnlyList<string> RecentDiagnostics,
        IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> RuntimeNodeOverlays,
        IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> RuntimeConnectionOverlays,
        IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> RuntimeLogs);

    private sealed record ConsumerSampleSupportEnvironment(
        string FrameworkDescription,
        string OsDescription,
        string OsArchitecture,
        string ProcessArchitecture);

    private sealed record ConsumerSampleSupportReproduction(
        string Command,
        string WorkingDirectory,
        string? Note);

    private sealed record ConsumerSampleSupportGraphSummary(
        int NodeCount,
        int ConnectionCount);
}
