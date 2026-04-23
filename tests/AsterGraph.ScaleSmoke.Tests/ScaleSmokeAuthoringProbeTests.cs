using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Editor.Hosting;
using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeAuthoringProbeTests
{
    [Fact]
    public void LargeTierProbe_CoversCanonicalAuthoringInteractions()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var definitionId = new NodeDefinitionId("scale.node");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = ScaleSmokeScenarioFactory.CreateDocument(tier, definitionId),
            NodeCatalog = ScaleSmokeScenarioFactory.CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var result = ScaleSmokeAuthoringProbe.Run(
            session,
            stencilFilter: "authoring",
            nodeId: "scale-node-500",
            connectionId: "scale-connection-500");

        Assert.True(result.StencilFilterOk);
        Assert.True(result.CommandSurfaceRefreshOk);
        Assert.True(result.QuickToolProjectionOk);
        Assert.True(result.QuickToolExecutionOk);
        Assert.True(result.IsOk);
    }
}
