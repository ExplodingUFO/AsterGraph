# Beta Evaluation Path

This guide is the hosted route ladder from first install to realistic host proof.
Use it when you are evaluating AsterGraph as an SDK, not when you are maintaining release infrastructure.

## Boundary First

- stay on the shipped Avalonia path or the canonical runtime/session path
- treat `WPF` as validation-only adapter-2 coverage, not a second onboarding path or a parity promise
- treat retained `GraphEditorViewModel` / `GraphEditorView` as migration-only
- keep `Demo` and release harnesses for later visual inspection or maintainer validation, not for first evaluation

## Route Ladder

Follow this ladder in order. Each step answers one question and hands off to the next one.

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.

| Step | Run this | Why it comes now | Proof handoff |
| --- | --- | --- | --- |
| 1 | `AsterGraph.Starter.Avalonia` | confirms the first hosted end-to-end route with the smallest scaffold | once the shell opens, move to the smallest stock sample |
| 2 | `AsterGraph.HelloWorld.Avalonia` | confirms the shipped Avalonia surface without extra host wiring | once this is clear, move to the realistic hosted proof |
| 3 | `AsterGraph.ConsumerSample.Avalonia -- --proof` | proves host-owned actions, trusted-plugin flow, parameter editing, and command projection on the defended route | expect `CONSUMER_SAMPLE_OK:True`, `COMMAND_SURFACE_OK:True`, `HOST_NATIVE_METRIC:*`, and optionally generate a [Beta Support Bundle](./support-bundle.md) |
| 4 | `AsterGraph.HostSample` | validates the proof harness after the realistic host sample is already understood | expect `HOST_SAMPLE_OK:True`; use this only after `ConsumerSample.Avalonia` |

If you are intentionally evaluating the runtime-only path, run `AsterGraph.HelloWorld` after step 1 instead of step 2, then return to the same realistic hosted proof at step 3.

## Commands

```powershell
# first hosted confirmation
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo

# smallest shipped Avalonia surface
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo

# realistic hosted proof on the defended beta path
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof

# proof harness only after the realistic hosted sample
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
```

## What Success Looks Like

- step 3 is the realistic hosted proof gate for most evaluators
- `ConsumerSample.Avalonia` should emit `CONSUMER_SAMPLE_OK:True`
- the same run should emit `COMMAND_SURFACE_OK:True`
- the same run should emit the four `HOST_NATIVE_METRIC:*` lines
- `HostSample` should emit `HOST_SAMPLE_OK:True`

## What Not To Infer

- `WPF` coverage is validation-only; it does not widen the supported hosted adapter boundary
- retained MVVM is migration-only; it is not the recommended evaluator route
- `HostSample` is a proof harness, not the first product-like experience
- `Demo` is useful only after the route and proof expectations are already clear

## Related Docs

- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
- [Stabilization Support Matrix](./stabilization-support-matrix.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
