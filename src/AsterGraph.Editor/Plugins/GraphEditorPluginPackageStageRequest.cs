using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Describes one explicit plugin-package staging request.
/// </summary>
public sealed record GraphEditorPluginPackageStageRequest
{
    /// <summary>
    /// Initializes a plugin-package staging request.
    /// </summary>
    public GraphEditorPluginPackageStageRequest(
        GraphEditorPluginCandidateSnapshot candidate,
        string? stagingRootPath = null)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        Candidate = candidate;
        StagingRootPath = string.IsNullOrWhiteSpace(stagingRootPath) ? null : Path.GetFullPath(stagingRootPath);
    }

    /// <summary>
    /// Gets the candidate snapshot that should enter staging.
    /// </summary>
    public GraphEditorPluginCandidateSnapshot Candidate { get; }

    /// <summary>
    /// Gets the optional absolute staging-root path supplied by the host.
    /// </summary>
    public string? StagingRootPath { get; }
}
