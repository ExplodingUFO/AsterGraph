# Adoption Feedback Loop

This page defines the public beta intake record and records the seeded adopter-trial synthesis used while the prerelease intake loop is still collecting enough real external reports.

## Intake Format

Each beta intake record should stay on one bounded schema: route, version, proof markers, friction, and support-bundle attachment.

Each feedback entry should capture this bounded schema:

- route (`HelloWorld`, `AsterGraph.Starter.Avalonia`, `HelloWorld.Avalonia`, `ConsumerSample.Avalonia`, `HostSample`, `PackageSmoke`, `ScaleSmoke`, `Demo`)
- version
- proof markers or screenshot reference when available
- friction
- support-bundle attachment when available

Use the GitHub issue template `Adoption feedback` for public reports.
When you can reach `ConsumerSample.Avalonia -- --proof`, attach the local bundle described in [Beta Support Bundle](./support-bundle.md) as the support-bundle attachment.

## Seeded Trial Synthesis

Until the repo collects enough real external issues, the current recommendation is based on four structured pre-public adoption rehearsals that exercise distinct entry routes.

| Persona | Route tried | Main friction | Requested next capability |
| --- | --- | --- | --- |
| Avalonia host integrator | `AsterGraph.Starter.Avalonia` -> `HelloWorld.Avalonia` -> `ConsumerSample.Avalonia` | first-run path was clear, but the jump from the smallest sample to realistic host wiring was still large before the medium sample existed | more copyable hosted-UI templates and recipes |
| SDK evaluator with plugin needs | `ConsumerSample.Avalonia` | trust boundary was understandable only after reading deeper docs | more prominent trust-policy examples and plugin-host recipes |
| Existing retained host maintainer | `Host Integration` + retained migration docs | route choice and migration staging still need careful reading | more migration recipes and route comparison examples |
| Performance-conscious evaluator | `ScaleSmoke` baseline | proof markers were present, but budget interpretation required a separate doc | keep publishing defended baseline numbers before expanding large-tier commitments |

## Current Recommendation

The next 0.xx alpha/beta line should stay on usability and sample polish first unless evidence volume/risk says otherwise:

1. **continue usability and sample polish first**
2. **defer defended large-tier performance work until real adopter demand clusters there**
3. **expand richer parameter and metadata editing only after the next adoption round confirms that demand**

This seeded recommendation should be replaced as soon as 3-5 real external entries cluster on one maintained route or bounded risk.

## Related Docs

- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
