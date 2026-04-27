using System.Security.Cryptography;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;

namespace AsterGraph.PluginTool;

internal static class PluginReportBuilder
{
    public static PluginCandidateReport CreateCandidateReport(GraphEditorPluginCandidateSnapshot candidate, string? hostVersion)
    {
        var artifactPath = candidate.PackagePath ?? candidate.AssemblyPath;
        var nodeDefinitions = TryLoadNodeDefinitions(candidate);
        return new PluginCandidateReport(
            candidate,
            EvaluateHostCompatibility(candidate.Manifest.Compatibility, hostVersion) ?? candidate.Compatibility,
            artifactPath is null ? null : ComputeSha256(artifactPath),
            nodeDefinitions);
    }

    public static Dictionary<string, bool> CreateProofMarkers(IReadOnlyList<PluginCandidateReport> reports)
        => new(StringComparer.Ordinal)
        {
            ["PLUGIN_COMPATIBILITY_OK"] = reports.Count > 0
                && reports.All(report => report.HostCompatibility.Status != GraphEditorPluginCompatibilityStatus.Incompatible),
            ["PLUGIN_MANIFEST_OK"] = reports.Count > 0
                && reports.All(report => !string.IsNullOrWhiteSpace(report.Candidate.Manifest.Id)
                    && !string.IsNullOrWhiteSpace(report.Candidate.Manifest.DisplayName)),
            ["PLUGIN_NODE_DEFINITIONS_OK"] = reports.Any(report => report.NodeDefinitions.Count > 0),
            ["PLUGIN_PARAMETER_METADATA_OK"] = reports.Any(report => report.NodeDefinitions.SelectMany(definition => definition.Parameters).Any()),
            ["PLUGIN_TRUST_EVIDENCE_OK"] = reports.Count > 0
                && reports.All(report => report.Sha256 is not null
                    && report.Candidate.ProvenanceEvidence.Signature.Status != GraphEditorPluginSignatureStatus.Unknown),
        };

    public static void WriteProofMarkers(IReadOnlyDictionary<string, bool> markers, TextWriter output)
    {
        foreach (var marker in markers)
        {
            output.WriteLine($"{marker.Key}:{marker.Value}");
        }
    }

    public static void WriteCandidate(PluginCandidateReport report, TextWriter output)
    {
        var candidate = report.Candidate;
        var manifest = candidate.Manifest;
        var compatibility = manifest.Compatibility;
        var evidence = candidate.ProvenanceEvidence;

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
        output.WriteLine($"  host_compatibility: {report.HostCompatibility.Status}:{report.HostCompatibility.ReasonCode ?? "none"}");
        output.WriteLine($"  trust: {candidate.TrustEvaluation.Decision}:{candidate.TrustEvaluation.Source}:{candidate.TrustEvaluation.ReasonCode ?? "none"}");
        output.WriteLine($"  signature: {evidence.Signature.Status}:{evidence.Signature.ReasonCode ?? "none"}");
        output.WriteLine($"  sha256: {report.Sha256 ?? "unavailable"}");
        output.WriteLine($"  node_definitions: {report.NodeDefinitions.Count}");
        output.WriteLine($"  parameter_metadata: {report.NodeDefinitions.SelectMany(definition => definition.Parameters).Count()}");
    }

    public static object CreateJsonCandidate(PluginCandidateReport report)
    {
        var candidate = report.Candidate;
        var manifest = candidate.Manifest;
        return new
        {
            id = manifest.Id,
            displayName = manifest.DisplayName,
            version = manifest.Version,
            sourceKind = candidate.SourceKind.ToString(),
            source = candidate.Source,
            assembly = candidate.AssemblyPath,
            package = candidate.PackagePath,
            pluginType = candidate.PluginTypeName,
            manifest = new
            {
                description = manifest.Description,
                capabilitySummary = manifest.CapabilitySummary,
                provenance = new
                {
                    sourceKind = manifest.Provenance.SourceKind.ToString(),
                    source = manifest.Provenance.Source,
                    publisher = manifest.Provenance.Publisher,
                    packageId = manifest.Provenance.PackageId,
                    packageVersion = manifest.Provenance.PackageVersion,
                },
                compatibility = new
                {
                    minimumAsterGraphVersion = manifest.Compatibility.MinimumAsterGraphVersion,
                    maximumAsterGraphVersion = manifest.Compatibility.MaximumAsterGraphVersion,
                    targetFramework = manifest.Compatibility.TargetFramework,
                    runtimeSurface = manifest.Compatibility.RuntimeSurface,
                },
            },
            compatibility = CreateJsonCompatibility(candidate.Compatibility),
            hostCompatibility = CreateJsonCompatibility(report.HostCompatibility),
            trust = new
            {
                decision = candidate.TrustEvaluation.Decision.ToString(),
                source = candidate.TrustEvaluation.Source.ToString(),
                reasonCode = candidate.TrustEvaluation.ReasonCode,
                reasonMessage = candidate.TrustEvaluation.ReasonMessage,
            },
            signature = new
            {
                status = candidate.ProvenanceEvidence.Signature.Status.ToString(),
                reasonCode = candidate.ProvenanceEvidence.Signature.ReasonCode,
                reasonMessage = candidate.ProvenanceEvidence.Signature.ReasonMessage,
            },
            sha256 = report.Sha256,
            nodeDefinitions = report.NodeDefinitions.Select(definition => new
            {
                id = definition.Id.Value,
                displayName = definition.DisplayName,
                category = definition.Category,
                subtitle = definition.Subtitle,
                parameterMetadata = definition.Parameters.Select(parameter => new
                {
                    key = parameter.Key,
                    displayName = parameter.DisplayName,
                    editorKind = parameter.EditorKind.ToString(),
                    valueType = parameter.ValueType.Value,
                    groupName = parameter.GroupName,
                    helpText = parameter.HelpText,
                    placeholderText = parameter.PlaceholderText,
                    hasConstraints = parameter.Constraints.AllowedOptions.Count > 0
                        || parameter.Constraints.Minimum is not null
                        || parameter.Constraints.Maximum is not null,
                }),
            }),
        };
    }

    public static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static IReadOnlyList<INodeDefinition> TryLoadNodeDefinitions(GraphEditorPluginCandidateSnapshot candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate.AssemblyPath))
        {
            return [];
        }

        try
        {
            var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
            {
                Document = new GraphDocument("PluginTool inspection", "Temporary local plugin inspection document.", [], []),
                NodeCatalog = new NodeCatalog(),
                CompatibilityService = new DefaultPortCompatibilityService(),
                PluginRegistrations =
                [
                    GraphEditorPluginRegistration.FromAssemblyPath(
                        candidate.AssemblyPath,
                        candidate.PluginTypeName,
                        candidate.Manifest,
                        candidate.ProvenanceEvidence),
                ],
            });
            return session.Queries.GetRegisteredNodeDefinitions();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException or BadImageFormatException)
        {
            return [];
        }
    }

    private static GraphEditorPluginCompatibilityEvaluation? EvaluateHostCompatibility(
        GraphEditorPluginCompatibilityManifest compatibility,
        string? hostVersion)
    {
        if (string.IsNullOrWhiteSpace(hostVersion) || !Version.TryParse(StripPrerelease(hostVersion), out var host))
        {
            return null;
        }

        var minimum = TryParseVersion(compatibility.MinimumAsterGraphVersion);
        if (minimum is not null && host < minimum)
        {
            return new GraphEditorPluginCompatibilityEvaluation(
                GraphEditorPluginCompatibilityStatus.Incompatible,
                "compatibility.host-version.minimum",
                $"Host version '{hostVersion}' is below plugin minimum '{compatibility.MinimumAsterGraphVersion}'.");
        }

        var maximum = TryParseVersion(compatibility.MaximumAsterGraphVersion);
        if (maximum is not null && host > maximum)
        {
            return new GraphEditorPluginCompatibilityEvaluation(
                GraphEditorPluginCompatibilityStatus.Incompatible,
                "compatibility.host-version.maximum",
                $"Host version '{hostVersion}' is above plugin maximum '{compatibility.MaximumAsterGraphVersion}'.");
        }

        return new GraphEditorPluginCompatibilityEvaluation(
            GraphEditorPluginCompatibilityStatus.Compatible,
            "compatibility.host-version.accepted",
            $"Plugin compatibility metadata accepts host version '{hostVersion}'.");
    }

    private static Version? TryParseVersion(string? value)
        => Version.TryParse(StripPrerelease(value), out var version) ? version : null;

    private static string? StripPrerelease(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        var separatorIndex = normalized.IndexOfAny(['-', '+']);
        return separatorIndex >= 0 ? normalized[..separatorIndex] : normalized;
    }

    private static object CreateJsonCompatibility(GraphEditorPluginCompatibilityEvaluation compatibility)
        => new
        {
            status = compatibility.Status.ToString(),
            reasonCode = compatibility.ReasonCode,
            reasonMessage = compatibility.ReasonMessage,
        };
}

internal sealed record PluginCandidateReport(
    GraphEditorPluginCandidateSnapshot Candidate,
    GraphEditorPluginCompatibilityEvaluation HostCompatibility,
    string? Sha256,
    IReadOnlyList<INodeDefinition> NodeDefinitions);
