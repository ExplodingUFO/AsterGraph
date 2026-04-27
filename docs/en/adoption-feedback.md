# Adoption Feedback Loop

This page defines the public beta bounded intake record and explicitly distinguishes maintainer-seeded rehearsal evidence from real external reports while the prerelease intake loop is still collecting enough real external reports.

## Intake Format

Each beta intake record should stay on one bounded intake vocabulary: route, version, proof markers, friction, and support-bundle attachment note.

Each feedback entry should capture this bounded schema:

- route (`HelloWorld`, `AsterGraph.Starter.Avalonia`, `HelloWorld.Avalonia`, `ConsumerSample.Avalonia`, `HostSample`, `PackageSmoke`, `ScaleSmoke`, `Demo`)
- version
- proof markers
- friction
- support-bundle attachment note: `SUPPORT_BUNDLE_PATH:...` when the route produced a bundle, or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it did not

Screenshots or command output can be attached as supplemental evidence, but they do not replace proof markers in the bounded intake field.

Use the GitHub issue template `Adoption feedback` for public reports.
When you can reach `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`, attach the local bundle described in [Beta Support Bundle](./support-bundle.md) and reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note. If the route cannot produce a bundle, record `NO_SUPPORT_BUNDLE:route-cannot-produce-one`.

## Seeded Trial Synthesis

The current recommendation is based on four structured pre-public adoption rehearsals that exercise distinct entry routes. These are maintainer-seeded rehearsal evidence, not real external intake, and they do not count toward the 3-5 gate.

| Persona | Route tried | Main friction | Requested next capability |
| --- | --- | --- | --- |
| Avalonia host integrator | `AsterGraph.Starter.Avalonia` -> `HelloWorld.Avalonia` -> `ConsumerSample.Avalonia` | first-run path was clear, but the jump from the smallest sample to realistic host wiring was still large before the medium sample existed | more copyable hosted-UI templates and recipes |
| SDK evaluator with plugin needs | `ConsumerSample.Avalonia` | trust boundary was understandable only after reading deeper docs | more prominent trust-policy examples and plugin-host recipes |
| Existing retained host maintainer | `Host Integration` + retained migration docs | route choice and migration staging still need careful reading | more migration recipes and route comparison examples |
| Performance-conscious evaluator | `ScaleSmoke` baseline | proof markers were present, but budget interpretation required a separate doc | keep publishing defended baseline numbers before expanding large-tier commitments |

## Current Recommendation

The next 0.xx alpha/beta line should stay on copyable host-owned parameter/metadata polish first; only widen beyond the promoted 5000-node stress gates or broader parameter/metadata editing when 3-5 real external reports cluster on the same bounded risk:

1. **continue copyable host-owned parameter/metadata polish first**
2. **seeded rehearsals do not count toward the 3-5 gate**
3. **do not widen beyond the promoted 5000-node stress gates before 3-5 real external reports cluster on the same bounded risk**
4. **do not treat broader parameter and metadata editing as the next widening target before 3-5 real external reports cluster on the same bounded risk**

Until that threshold is met, keep this seeded recommendation in place instead of widening the next beta line ad hoc.

## Related Docs

- [Beta Support Bundle](./support-bundle.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
