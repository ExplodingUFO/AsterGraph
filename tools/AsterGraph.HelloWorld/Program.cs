var result = HelloWorldRuntimeProof.Run();

if (!result.IsOk)
{
    throw new InvalidOperationException("HelloWorld sample expected one connection after the runtime-only command flow.");
}

Console.WriteLine("AsterGraph HelloWorld");
Console.WriteLine($"Route: {result.Route}");
Console.WriteLine($"Nodes: {result.NodeCount}");
Console.WriteLine($"Connections: {result.ConnectionCount}");
Console.WriteLine($"Feature descriptors: {result.FeatureDescriptorCount}");
Console.WriteLine("HELLOWORLD_OK:True");
