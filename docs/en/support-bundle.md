# Beta Support Bundle

This is the local evidence contract for beta evaluation and support.
It does not upload anything and it does not imply telemetry or hosted services.

## Canonical Producer

Generate the bundle from `ConsumerSample.Avalonia`, because that is the defended realistic hosted proof on the current beta route.

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle artifacts/consumer-support-bundle.json --support-note "what you were trying to validate"
```

Expected additional proof markers:

- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

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

`reproduction` records:

- the exact command
- the working directory
- an optional human note

## When To Use It

- attach it when reporting beta support issues
- attach it when filing adopter feedback after reaching the realistic hosted proof
- regenerate it after you change package version, route, or environment

## Related Docs

- [Beta Evaluation Path](./evaluation-path.md)
- [Consumer Sample](./consumer-sample.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
