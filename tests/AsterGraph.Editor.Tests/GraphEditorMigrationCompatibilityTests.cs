using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorMigrationCompatibilityTests
{
    private const string SkipReason = "Planned for 01-03: staged migration compatibility behaviors are not implemented yet.";

    [Fact(Skip = SkipReason)]
    public void LegacyGraphEditorViewModelConstructor_RemainsSupportedAlongsideNewInitializationApis()
    {
    }

    [Fact(Skip = SkipReason)]
    public void NewInitializationApis_CreateEditorStateEquivalentToLegacyConstructorPath()
    {
    }

    [Fact(Skip = SkipReason)]
    public void GraphEditorView_RemainsCompatibilityFacadeDuringStagedMigration()
    {
    }

    [Fact(Skip = SkipReason)]
    public void StagedMigrationPath_AllowsHostsToAdoptNewApisWithoutImmediateRewrite()
    {
    }
}
