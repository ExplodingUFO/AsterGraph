# Adapter-2 Accessibility Recipe

Use this recipe only after the defended Avalonia hosted accessibility proof is already green. It defines one bounded validation-only follow-up on `WPF` as adapter 2; it does not widen support promises, and it does not claim Avalonia/WPF parity.

## Handoff Order

- Start on the defended Avalonia hosted route with `AsterGraph.ConsumerSample.Avalonia -- --proof` and keep `HOSTED_ACCESSIBILITY_OK:True` plus the hosted accessibility diagnostics on that record first.
- Reuse the same bounded intake record and support-bundle attachment note from the defended Avalonia route; `WPF` does not open a second intake flow.
- Run `AsterGraph.HelloWorld.Wpf -- --proof` only as validation-only adapter-2 follow-up.
- Keep `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, and `HELLOWORLD_WPF_OK:True` on that same local record.
- Treat every `WPF` proof line as validation-only evidence; none of these markers widen public support promises.

## Commands

```powershell
# defended Avalonia accessibility proof first
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof

# bounded adapter-2 accessibility validation only after the defended Avalonia proof
dotnet run --project tools/AsterGraph.HelloWorld.Wpf/AsterGraph.HelloWorld.Wpf.csproj --nologo -- --proof
```

## What Success Looks Like

- the defended Avalonia route stays the first proof gate
- `AsterGraph.HelloWorld.Wpf -- --proof` emits `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- the same `WPF` proof emits `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- the same `WPF` proof emits `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- the same `WPF` proof emits `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- the same `WPF` proof emits `HOSTED_ACCESSIBILITY_OK:True`
- the same `WPF` proof emits `HELLOWORLD_WPF_OK:True`

## What Not To Infer

- `WPF` remains validation-only adapter-2 coverage
- these markers do not create a new onboarding path
- these markers do not claim Avalonia/WPF parity
- these markers do not widen public WPF support

## Related Docs

- [Evaluation Path](./evaluation-path.md)
- [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
