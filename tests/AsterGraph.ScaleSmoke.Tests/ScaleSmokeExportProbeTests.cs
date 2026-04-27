using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Editor.Hosting;
using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeExportProbeTests
{
    [Fact]
    public void LargeTierProbe_CoversCanonicalExportAndReloadInteractions()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var definitionId = new NodeDefinitionId("scale.node");
        var storageRoot = Path.Combine(Path.GetTempPath(), "AsterGraph.ScaleSmoke.ExportProbe", Guid.NewGuid().ToString("N"));
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = ScaleSmokeScenarioFactory.CreateDocument(tier, definitionId),
            NodeCatalog = ScaleSmokeScenarioFactory.CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRoot,
        });

        var result = ScaleSmokeExportProbe.Run(session, storageRoot);

        var marker = result.ToMarker(tier.Id);
        Assert.True(result.SvgExportOk, marker);
        Assert.True(result.PngExportOk, marker);
        Assert.True(result.JpegExportOk, marker);
        Assert.True(result.ReloadOk, marker);
        Assert.True(result.ProgressOk, marker);
        Assert.True(result.CancelOk, marker);
        Assert.True(result.FullScopeOk, marker);
        Assert.True(result.SelectionScopeOk, marker);
        Assert.True(result.IsOk, marker);
        Assert.Contains("progress=True", marker, StringComparison.Ordinal);
        Assert.Contains("cancel=True", marker, StringComparison.Ordinal);
        Assert.Contains("scope=True", marker, StringComparison.Ordinal);
        Assert.Contains("selection=True", marker, StringComparison.Ordinal);
    }
}
