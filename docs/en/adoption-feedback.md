# Adoption Feedback Loop

This page defines the public beta bounded intake record and keeps the same bounded intake vocabulary used by the issue template while the prerelease intake loop is still collecting enough real external reports.

## Intake Format

Each beta intake record should stay on one bounded intake vocabulary: route, version, proof markers, friction, and support-bundle attachment note.

Each feedback entry should capture this bounded schema:

- route (`HelloWorld`, `AsterGraph.Starter.Avalonia`, `HelloWorld.Avalonia`, `ConsumerSample.Avalonia`, `HostSample`, `PackageSmoke`, `ScaleSmoke`, `Demo`)
- version
- proof markers or screenshot reference when available
- friction
- support-bundle attachment note: `SUPPORT_BUNDLE_PATH:...` when the route produced a bundle, or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it did not

Use the GitHub issue template `Adoption feedback` for public reports.
When you can reach `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`, attach the local bundle described in [Beta Support Bundle](./support-bundle.md) and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note. If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.

## Seeded Trial Synthesis

Until the repo collects enough real external issues, the current recommendation is based on four structured pre-public adoption rehearsals that exercise distinct entry routes. The `Persona` and `Requested next capability` columns below are maintainer-derived synthesis, not raw public intake fields.

| Persona | Route tried | Main friction | Requested next capability |
| --- | --- | --- | --- |
| Avalonia host integrator | `AsterGraph.Starter.Avalonia` -> `HelloWorld.Avalonia` -> `ConsumerSample.Avalonia` | first-run path was clear, but the jump from the smallest sample to realistic host wiring was still large before the medium sample existed | more copyable hosted-UI templates and recipes |
| SDK evaluator with plugin needs | `ConsumerSample.Avalonia` | trust boundary was understandable only after reading deeper docs | more prominent trust-policy examples and plugin-host recipes |
| Existing retained host maintainer | `Host Integration` + retained migration docs | route choice and migration staging still need careful reading | more migration recipes and route comparison examples |
| Performance-conscious evaluator | `ScaleSmoke` baseline | proof markers were present, but budget interpretation required a separate doc | keep publishing defended baseline numbers before expanding large-tier commitments |

## Current Recommendation

The next 0.xx alpha/beta line should stay on copyable host-owned parameter/metadata polish first; only widen toward defended large-tier performance or broader parameter/metadata editing when 3-5 real external entries cluster on that bounded risk:

1. **continue copyable host-owned parameter/metadata polish first**
2. **do not treat defended large-tier performance as the next widening target before 3-5 real external entries cluster on that bounded risk**
3. **do not treat broader parameter and metadata editing as the next widening target before 3-5 real external entries cluster on that bounded risk**

Until that threshold is met, keep this seeded recommendation in place instead of widening the next beta line ad hoc.

## Related Docs

- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
