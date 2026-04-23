# Beta Support Bundle

This is the local evidence contract for beta evaluation and support.
Use the support bundle as the support-bundle attachment note on the bounded intake record when available from the defended hosted proof route.
It does not upload anything and it does not imply telemetry or hosted services.
Use it alongside [Beta Evaluation Path](./evaluation-path.md) when you need the local evidence attachment from the defended hosted proof route.

## Canonical Producer

Generate the support bundle from `ConsumerSample.Avalonia`, because that is the defended realistic hosted proof on the current beta route.

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

Expected additional proof markers:

- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

## Local Evidence Only

Copyable local evidence reference:

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle <support-bundle-path> --support-note "what you were trying to validate"
```

Local evidence only means this bundle remains tied to the defended hosted route and does not widen the support boundary. Use the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note on the bounded intake record. If a route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.

## Contract

The support bundle is one local JSON file with these top-level fields:

- `schemaVersion`
- `packageVersion`
- `publicTag`
- `route`
- `generatedAtUtc`
- `proofLines`
- `environment`
- `reproduction`

`proofLines` should include the full marker set emitted by proof mode: `CONSUMER_SAMPLE_HOST_ACTION_OK:True`, `CONSUMER_SAMPLE_PLUGIN_OK:True`, `CONSUMER_SAMPLE_PARAMETER_OK:True`, `CONSUMER_SAMPLE_WINDOW_OK:True`, `CONSUMER_SAMPLE_TRUST_OK:True`, `COMMAND_SURFACE_OK:True`, `HOST_NATIVE_METRIC:startup_ms=...`, `HOST_NATIVE_METRIC:inspector_projection_ms=...`, `HOST_NATIVE_METRIC:plugin_scan_ms=...`, `HOST_NATIVE_METRIC:command_latency_ms=...`, and `CONSUMER_SAMPLE_OK:True`.

`environment` records the local runtime and OS details used for the run.

`reproduction` records the friction note plus:

- the captured command line
- the working directory
- an optional human note

## When To Use It

- attach it when reporting beta support issues
- attach it to the bounded intake record when filing adopter feedback after reaching the realistic hosted proof
- regenerate it after you change package version, route, or environment
- treat it as intake evidence for the [Project Status](./project-status.md) readiness gate, not as automatic proof that the support boundary should widen
- if a route cannot produce one yet, keep the same route/version/proof markers/friction record and note that no support bundle was available

## Related Docs

- [Beta Evaluation Path](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
