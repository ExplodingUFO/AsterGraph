using Xunit;

namespace AsterGraph.HelloWorld.Tests;

public sealed class RuntimeHelloWorldProofTests
{
    [Fact]
    public void RuntimeHelloWorldProof_Run_UsesCanonicalSessionRoute()
    {
        var result = HelloWorldRuntimeProof.Run();

        Assert.True(result.IsOk);
        Assert.Equal("AsterGraphEditorFactory.CreateSession(...)", result.Route);
        Assert.True(result.ConnectionCount > 0);
        Assert.True(result.FeatureDescriptorCount > 0);
    }
}
