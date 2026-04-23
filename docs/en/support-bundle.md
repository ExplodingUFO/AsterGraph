# Beta Support Bundle

This is the local evidence contract for beta evaluation and support.
Use the support bundle as the support-bundle attachment note on the beta intake record when available from the defended hosted proof route.
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

Local evidence only means this bundle remains tied to the defended hosted route and does not widen the support boundary. Use the emitted `SUPPORT_BUNDLE_PATH:...` line as the attachment note on the intake record. If a route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.

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

`proofLines` should include the same marker lines emitted by proof mode, including `COMMAND_SURFACE_OK:True`, `CONSUMER_SAMPLE_OK:True`, and the `HOST_NATIVE_METRIC:*` lines.

`environment` records the local runtime and OS details used for the run.

`reproduction` records the friction note plus:

- the captured command line
- the working directory
- an optional human note

## When To Use It

- attach it when reporting beta support issues
- attach it to the intake record when filing adopter feedback after reaching the realistic hosted proof
- regenerate it after you change package version, route, or environment
- treat it as intake evidence for the [Project Status](./project-status.md) readiness gate, not as automatic proof that the support boundary should widen
- if a route cannot produce one yet, keep the same route/version/proof/friction record and note that no support bundle was available

## Related Docs

- [Beta Evaluation Path](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
