# Plugin Host Recipe

This recipe shows the host-centric path for plugin discovery, trust evaluation, registration, and runtime communication.

Use it when your host loads plugins from disk and needs explicit allow/block policy before any plugin code runs.

For the smaller plugin-author path, see [Plugin And Custom Node Recipe](./plugin-recipe.md). For the v1 manifest and trust-policy contract, see [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md).

## Packages

```powershell
dotnet add package AsterGraph.Editor --prerelease
dotnet add package AsterGraph.Abstractions --prerelease
```

Add `AsterGraph.Avalonia` only if the host also embeds the shipped UI.

## 1. Discovery

Use `AsterGraphEditorFactory.DiscoverPluginCandidates(...)` to enumerate candidates without loading plugin assemblies.

```csharp
var discoveryOptions = new GraphEditorPluginDiscoveryOptions
{
    DirectorySources =
    [
        new GraphEditorPluginDirectoryDiscoverySource(@"C:\MyHost\Plugins")
    ],
    PackageDirectorySources =
    [
        new GraphEditorPluginPackageDiscoverySource(@"C:\MyHost\PluginPackages")
    ],
    TrustPolicy = myHostTrustPolicy, // optional pre-filter
};

var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(discoveryOptions);
```

What you get:

- `GraphEditorPluginCandidateSnapshot` per candidate
- `Manifest` (id, display name, version, compatibility, provenance)
- `ProvenanceEvidence` (package identity, signature status, signer fingerprint)
- `TrustEvaluation` (decision, source, reason code, reason message)
- `PackagePath` when the candidate comes from a package directory

Discovery does not load assemblies. It is safe to run against untrusted directories.

## 2. Trust Evaluation

Implement `IGraphEditorPluginTrustPolicy` to make host-governed decisions.

```csharp
public sealed class MyHostTrustPolicy : IGraphEditorPluginTrustPolicy
{
    public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
    {
        // context.Registration — the candidate registration
        // context.Manifest — the visible manifest
        // context.ProvenanceEvidence — signature and package identity
        // context.PackagePath — absolute local package path, when present

        if (IsInHostAllowlist(context.Manifest, context.ProvenanceEvidence))
        {
            return new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                reasonCode: "myhost.allowlist.allowed",
                reasonMessage: $"Allowed '{context.Manifest.Id}' via host allowlist.");
        }

        return new GraphEditorPluginTrustEvaluation(
            GraphEditorPluginTrustDecision.Blocked,
            GraphEditorPluginTrustEvaluationSource.HostPolicy,
            reasonCode: "myhost.allowlist.blocked",
            reasonMessage: $"Blocked '{context.Manifest.Id}': not in host allowlist.");
    }
}
```

If no policy is configured, the runtime returns `GraphEditorPluginTrustEvaluation.ImplicitAllow()`.

Trust decisions are evaluated before any plugin contribution code executes.

## 3. Allowlist

Keep the allowlist host-owned:

- Persist entries by manifest id + fingerprint (hash of id, display name, package id, version, signer fingerprint, signature status, target framework)
- Support import/export paths so the same allowlist can move between environments
- Keep provenance snapshots alongside allowlist entries for audit

`ConsumerSamplePluginAllowlistTrustPolicy` in `tools/AsterGraph.ConsumerSample.Avalonia` is the bounded sample path. Copy the shape, replace the storage format and persistence policy with your host's own.

## 4. Registration

Turn approved candidates into `GraphEditorPluginRegistration` and pass them to `AsterGraphEditorOptions.PluginRegistrations`.

Direct instance:

```csharp
var registration = GraphEditorPluginRegistration.FromPlugin(
    new MyPlugin(),
    manifest,
    provenanceEvidence);
```

From assembly path:

```csharp
var registration = GraphEditorPluginRegistration.FromAssemblyPath(
    assemblyPath,
    pluginTypeName: null,
    manifest,
    provenanceEvidence);
```

From package path:

```csharp
var registration = GraphEditorPluginRegistration.FromPackagePath(
    packagePath,
    manifest,
    provenanceEvidence);
```

From staged package (after `StagePluginPackage`):

```csharp
var stageResult = AsterGraphEditorFactory.StagePluginPackage(
    new GraphEditorPluginPackageStageRequest(candidate));

if (stageResult.Registration is not null)
{
    // use stageResult.Registration
}
```

Compose the editor:

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    PluginTrustPolicy = myHostTrustPolicy,
    PluginRegistrations = approvedRegistrations,
});
```

## 5. Communication

After the editor is running, inspect plugin outcomes through the session.

```csharp
var session = editor.Session;

// Loaded, trusted, blocked outcomes
var loadSnapshots = session.Queries.GetPluginLoadSnapshots();

// Session diagnostics include plugin load events
var diagnostics = session.Diagnostics.GetRecentDiagnostics();
```

Plugin-contributed commands surface through the same shared command route:

```csharp
var descriptors = session.Queries.GetCommandDescriptors();
// plugin commands are included in the descriptor list
```

Execute through the canonical path:

```csharp
session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
    commandId, parameters));
```

## Important Boundary

Plugin loading is in-process. AsterGraph does not sandbox plugin code or isolate untrusted execution.

For public beta hosts:

- keep plugin directories explicit
- prefer allowlists
- validate provenance with signatures or hashes in host policy
- do not treat plugin loading as an isolation boundary

## Proof Marker Expectations

Run the defended hosted proof:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

Expect:

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `CONSUMER_SAMPLE_OK:True`

## Related Docs

- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)
- [Host Integration](./host-integration.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
