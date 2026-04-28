# Adapter-2 Performance Recipe

Use this recipe only after the defended Avalonia hosted proof is already green. It defines one bounded validation-only performance follow-up on `WPF` as adapter 2; it does not widen support promises, and it does not claim Avalonia/WPF parity.

## Handoff Order

- Start on the defended Avalonia hosted route with `AsterGraph.ConsumerSample.Avalonia -- --proof` and keep the shared `HOST_NATIVE_METRIC:*` lines on that record first.
- Reuse the same bounded intake record and support-bundle attachment note from the defended Avalonia route; `WPF` does not open a second intake flow.
- Run `AsterGraph.HelloWorld.Wpf -- --proof` only as validation-only adapter-2 follow-up.
- Keep `ADAPTER2_PERFORMANCE_BASELINE_OK:True`, `ADAPTER2_EXPORT_BREADTH_OK:True`, `ADAPTER2_PROJECTION_BUDGET_OK:True:none`, `ADAPTER2_COMMAND_BUDGET_OK:True:none`, `ADAPTER2_SCENE_BUDGET_OK:True:none`, `ADAPTER2_PROOF_BUDGET_OK:True`, `ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`, `ADAPTER2_RECIPE_ALIGNMENT_OK:True`, and `HELLOWORLD_WPF_OK:True` on that same local record.
- Treat every `WPF` performance proof line as validation-only evidence; none of these markers widen public support promises.

## Commands

```powershell
# defended Avalonia hosted proof first
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof

# bounded adapter-2 performance validation only after the defended Avalonia proof
dotnet run --project tools/AsterGraph.HelloWorld.Wpf/AsterGraph.HelloWorld.Wpf.csproj --nologo -- --proof
```

## What Success Looks Like

- the defended Avalonia route stays the first proof gate
- `AsterGraph.HelloWorld.Wpf -- --proof` emits `ADAPTER2_PERFORMANCE_BASELINE_OK:True`
- the same `WPF` proof emits `ADAPTER2_EXPORT_BREADTH_OK:True`
- the same `WPF` proof emits `ADAPTER2_PROJECTION_BUDGET_OK:True:none`
- the same `WPF` proof emits `ADAPTER2_COMMAND_BUDGET_OK:True:none`
- the same `WPF` proof emits `ADAPTER2_SCENE_BUDGET_OK:True:none`
- the same `WPF` proof emits `ADAPTER2_PROOF_BUDGET_OK:True`
- the same `WPF` proof emits `ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`
- the same `WPF` proof emits `ADAPTER2_RECIPE_ALIGNMENT_OK:True`
- the same `WPF` proof emits `HELLOWORLD_WPF_OK:True`

## What Not To Infer

- `WPF` remains validation-only adapter-2 coverage
- these markers do not create a new onboarding path
- these markers do not claim Avalonia/WPF parity
- these markers do not widen public WPF support

## Related Docs

- [Evaluation Path](./evaluation-path.md)
- [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)
- [Widened Surface Performance Recipe](./widened-surface-performance-recipe.md)
- [Beta Support Bundle](./support-bundle.md)
