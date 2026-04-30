# Host Recipe Ladder

This is the unified "copy-from-here" ladder for hosts building on AsterGraph.

Follow it in order. Each step adds one bounded seam; nothing later undoes what came before.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

## Step 1: Starter.Avalonia — smallest end-to-end scaffold

Run this first to confirm the first hosted Avalonia route works.

```powershell
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo
```

### Copy This

- `AsterGraphEditorFactory.Create(...)`
- `AsterGraphAvaloniaViewFactory.Create(...)`
- `AsterGraphEditorOptions`
- The document/catalog/editor/view composition flow

### Replace This

- The top-level window and its title/size
- The sample graph/catalog definitions as your host grows

### Proof Handoff

Once the shell opens, move to the smallest stock sample.

## Step 2: HelloWorld.Avalonia — smallest shipped Avalonia surface

Run this to confirm the shipped Avalonia surface without extra host wiring.

```powershell
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo
```

### Copy This

- The same factory composition as Step 1
- Minimal menu and shell chrome if your host needs a stripped-down window

### Replace This

- The sample node family with your own catalog definitions
- Window chrome and narrative text

### Proof Handoff

Once this is clear, move to the realistic hosted proof.

## Step 3: ConsumerSample.Avalonia — realistic hosted proof

Run this to prove host-owned actions, trusted-plugin flow, parameter editing, command projection, and hosted accessibility semantics on the defended route.

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

### Copy This

- Action rail / command projection: query `session.Queries.GetCommandDescriptors()` and project through `AsterGraphHostedActionFactory.CreateCommandActions(...)` and `AsterGraphHostedActionFactory.CreateProjection(...)`
- Plugin trust workflow: `GraphEditorPluginDiscoveryOptions`, `AsterGraphEditorOptions.PluginTrustPolicy`, provenance snapshots, and an explicit host-owned allowlist policy
- Selected-node parameter read/write seam: `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` reads, `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` writes

### Replace This

- Review/audit node family
- Action ids and titles
- Window layout and narrative text
- Proof labels beyond the defended markers

### Proof Handoff

Expect these markers:

- `CONSUMER_SAMPLE_OK:True`
- `CONSUMER_SAMPLE_TRUST_OK:True`
- `CONSUMER_SAMPLE_PARAMETER_OK:True`
- `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:True`
- `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`
- `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- `HOST_NATIVE_METRIC:*`

If `CONSUMER_SAMPLE_PARAMETER_OK` or `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` fail, keep the failed proof-marker lines with the support bundle's `parameterSnapshots` rows on the same bounded intake record.

For the actual intake record, run with `--support-bundle <support-bundle-path>` and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line. See [Beta Support Bundle](./support-bundle.md).

## Step 4: Post-ladder proof harness

Run `HostSample` only after the realistic hosted sample is already understood.

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
```

Expect:

- `HOST_SAMPLE_OK:True`
- `HOST_SAMPLE_AUTOMATION_OK:True`
- `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`
- `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`

## Cross-links by seam

| Seam | Copy from | Detailed recipe |
| --- | --- | --- |
| Factory composition | `Starter.Avalonia` / `HelloWorld.Avalonia` | [Quick Start](./quick-start.md) |
| Plugin discovery/trust | `ConsumerSample.Avalonia` | [Plugin Host Recipe](./plugin-host-recipe.md) |
| Custom node/port/edge | `ConsumerSample.Avalonia` | [Custom Node Host Recipe](./custom-node-host-recipe.md) |
| Parameter metadata | Inspector vocabulary | [Authoring Inspector Recipe](./authoring-inspector-recipe.md) |
| Node surface authoring | `ConsumerSample.Avalonia` | [Authoring Surface Recipe](./authoring-surface-recipe.md) |
| Retained migration | Existing `GraphEditorViewModel` host | [Retained Migration Recipe](./retained-migration-recipe.md) |
| Accessibility handoff | `ConsumerSample.Avalonia` | [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md) |
| Performance budgets | `ScaleSmoke` | [ScaleSmoke Baseline](./scale-baseline.md) |

## Related Docs

- [Quick Start](./quick-start.md)
- [Evaluation Path](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
