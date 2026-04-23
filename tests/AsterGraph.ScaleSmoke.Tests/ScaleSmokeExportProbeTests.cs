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

        Assert.True(result.SvgExportOk);
        Assert.True(result.PngExportOk);
        Assert.True(result.JpegExportOk);
        Assert.True(result.ReloadOk);
        Assert.True(result.IsOk);
    }
}
