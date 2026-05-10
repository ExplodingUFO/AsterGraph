# Beta Evaluation Path

This guide is the hosted route ladder from first install to realistic host proof. It has one unambiguous intake handoff from defended route to bounded intake record: step 3 hands off to [Beta Support Bundle](./support-bundle.md) for the attachment note, then to [Adoption Feedback Loop](./adoption-feedback.md) for the bounded intake record.
Use it when you are evaluating AsterGraph as an SDK, not when you are maintaining release infrastructure.

If you are evaluating plugin trust, treat `src/AsterGraph.Demo` as the defended hosted trust hop. Read [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md) and [Plugin And Custom Node Recipe](./plugin-recipe.md) before you stop at the route ladder.
If you need reviewable local evidence while you follow the defended route, keep [Beta Support Bundle](./support-bundle.md) close to step 3.

## Boundary First

- stay on the shipped Avalonia path or the canonical runtime/session path
- treat `WPF` as validation-only adapter-2 coverage, not a second onboarding path or a parity promise
- treat retained `GraphEditorViewModel` / `GraphEditorView` as migration-only
- keep `Demo` and release harnesses for later visual inspection or maintainer validation, not for first evaluation

## Route Ladder

Follow this ladder in order. Each step answers one question and hands off to the next one.

The hosted route ladder is `templates/astergraph-avalonia -> src/AsterGraph.Demo -> src/AsterGraph.Demo -- --proof`.

| Step | Run this | Why it comes now | Proof handoff |
| --- | --- | --- | --- |
| 1 | `templates/astergraph-avalonia` | confirms the first hosted end-to-end route with the smallest scaffold | once the shell opens, move to the smallest stock sample |
| 2 | `src/AsterGraph.Demo` | confirms the shipped Avalonia surface without extra host wiring | once this is clear, move to the realistic hosted proof |
| 3 | `src/AsterGraph.Demo -- --proof` | proves host-owned actions, trusted-plugin flow, parameter editing, command projection, and hosted accessibility semantics on the defended route | expect `CONSUMER_SAMPLE_OK:True`, `COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`, and `HOST_NATIVE_METRIC:*`; this is the proof handoff only, and the bounded intake record lives in [Beta Support Bundle](./support-bundle.md), [Hosted Accessibility Recipe](./hosted-accessibility-recipe.md), and [Adoption Feedback Loop](./adoption-feedback.md) |
| 4 | release validation lane | validates downstream consumption, template smoke, scale baselines, and adapter-2 evidence after the realistic host sample is already understood | expect `HOST_SAMPLE_OK:True`, `HOST_SAMPLE_AUTOMATION_OK:True`, `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`, and `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True` from the CI/release proof artifacts; use this only after `src/AsterGraph.Demo -- --proof` |

If you are intentionally evaluating the runtime-only path, run `src/AsterGraph.Demo -- --proof` after step 1 instead of step 2, then return to the same realistic hosted proof at step 3.

If `CONSUMER_SAMPLE_PARAMETER_OK` or `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` fail, keep the failed proof-marker lines together with the support bundle's `parameterSnapshots` rows on the same bounded intake record.
For one screen-reader-ready local evaluation path, keep the step-3 support bundle plus the release-lane proof lines on that same bounded intake record. This remains local evidence only and does not widen support promises.
If you need bounded adapter-2 follow-up after the defended Avalonia proof, use the release validation lane and keep `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `ADAPTER2_PERFORMANCE_BASELINE_OK:True`, `ADAPTER2_PROJECTION_BUDGET_OK:True:none`, `ADAPTER2_COMMAND_BUDGET_OK:True:none`, `ADAPTER2_SCENE_BUDGET_OK:True:none`, `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`, `ADAPTER2_CANONICAL_ROUTE_OK:True`, `ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`, and `HELLOWORLD_WPF_OK:True` on that same local record; see [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md) and [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md).

## Commands

```powershell
# first hosted confirmation
dotnet new astergraph-avalonia -n MyGraphHost

# smallest shipped Avalonia surface
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --scenario ai-pipeline

# realistic hosted proof on the defended beta path
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof

# bounded adapter-2 accessibility validation only after the defended Avalonia proof
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --filter FullyQualifiedName~ReleaseClosureContractTests
```

## What Success Looks Like

- step 3 is the realistic hosted proof gate for most evaluators
- `src/AsterGraph.Demo` should emit `CONSUMER_SAMPLE_OK:True`
- the same run should emit `COMMAND_SURFACE_OK:True`
- the same run should emit `HOSTED_ACCESSIBILITY_OK:True`, `HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`, and `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- the same run should emit the four `HOST_NATIVE_METRIC:*` lines
- release validation should emit `HOST_SAMPLE_OK:True`, `HOST_SAMPLE_AUTOMATION_OK:True`, `HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True`, and `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`
- adapter-2 release evidence should emit `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `ADAPTER2_PERFORMANCE_BASELINE_OK:True`, `ADAPTER2_PROJECTION_BUDGET_OK:True:none`, `ADAPTER2_COMMAND_BUDGET_OK:True:none`, `ADAPTER2_SCENE_BUDGET_OK:True:none`, `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`, `ADAPTER2_CANONICAL_ROUTE_OK:True`, `ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`, and `HELLOWORLD_WPF_OK:True` on the validation-only adapter-2 lane

## What Not To Infer

- `WPF` coverage is validation-only; it does not widen the supported hosted adapter boundary
- retained MVVM is migration-only; it is not the recommended evaluator route
- `src/AsterGraph.Demo -- --proof` is proof mode, not the first product-like experience
- release validation lanes are maintainer evidence, not separate adopter sample routes

## Related Docs

- [Quick Start](./quick-start.md)
- `src/AsterGraph.Demo`
- [Host Integration](./host-integration.md)
- [Host Recipe Ladder](./host-recipe-ladder.md)
- [Stabilization Support Matrix](./stabilization-support-matrix.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)
- [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)
