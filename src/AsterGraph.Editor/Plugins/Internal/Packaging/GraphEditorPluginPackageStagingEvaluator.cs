namespace AsterGraph.Editor.Plugins.Internal;

internal static class GraphEditorPluginPackageStagingEvaluator
{
    public static GraphEditorPluginStageSnapshot? TryGetTrustRefusal(
        GraphEditorPluginCandidateSnapshot candidate,
        string packagePath,
        GraphEditorPluginPackageIdentity packageIdentity)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(packageIdentity);

        if (candidate.TrustEvaluation.Decision != GraphEditorPluginTrustDecision.Blocked)
        {
            return null;
        }

        return new GraphEditorPluginStageSnapshot(
            GraphEditorPluginStageOutcome.Refused,
            packagePath,
            packageIdentity,
            pluginTypeName: candidate.PluginTypeName,
            usedCache: false,
            reasonCode: candidate.TrustEvaluation.ReasonCode ?? "stage.trust.blocked",
            reasonMessage: candidate.TrustEvaluation.ReasonMessage ?? "Plugin trust policy blocked verified package staging.");
    }

    public static GraphEditorPluginStageSnapshot? TryGetSignatureRefusal(
        GraphEditorPluginCandidateSnapshot candidate,
        string packagePath,
        GraphEditorPluginPackageIdentity packageIdentity)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(packageIdentity);

        var signature = candidate.ProvenanceEvidence.Signature;
        if (signature.Status == GraphEditorPluginSignatureStatus.Valid)
        {
            return null;
        }

        var reasonCode = signature.Status switch
        {
            GraphEditorPluginSignatureStatus.NotProvided => "stage.signature.not-provided",
            GraphEditorPluginSignatureStatus.Unknown => "stage.signature.unknown",
            GraphEditorPluginSignatureStatus.Unsigned => "stage.signature.unsigned",
            GraphEditorPluginSignatureStatus.Invalid => "stage.signature.invalid",
            _ => "stage.signature.not-verified",
        };
        var reasonMessage = signature.ReasonMessage
            ?? signature.Status switch
            {
                GraphEditorPluginSignatureStatus.NotProvided => "Plugin package candidate did not provide signature evidence required for verified staging.",
                GraphEditorPluginSignatureStatus.Unknown => "Plugin package signature could not be verified for staging.",
                GraphEditorPluginSignatureStatus.Unsigned => "Plugin package is unsigned and cannot be staged.",
                GraphEditorPluginSignatureStatus.Invalid => "Plugin package signature is invalid and cannot be staged.",
                _ => "Plugin package signature is not in a verified state for staging.",
            };

        return new GraphEditorPluginStageSnapshot(
            GraphEditorPluginStageOutcome.Refused,
            packagePath,
            packageIdentity,
            pluginTypeName: candidate.PluginTypeName,
            usedCache: false,
            reasonCode: signature.ReasonCode ?? reasonCode,
            reasonMessage: reasonMessage);
    }
}
