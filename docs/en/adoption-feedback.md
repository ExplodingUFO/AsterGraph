# Adoption Feedback Loop

This page defines the public beta bounded intake record and explicitly distinguishes maintainer-seeded rehearsal evidence from real external reports while the prerelease intake loop is still collecting enough real external reports.

## Intake Format

Each beta intake record should stay on one bounded intake vocabulary: report type, adopter context, route, version, proof markers, friction, support-bundle attachment note, and claim-expansion status.

Current intake proof markers: `ADOPTION_INTAKE_EVIDENCE_OK:True`, `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True`, and `REAL_EXTERNAL_REPORT_GATE_OK:True`.
Current v0.61 refresh markers: `ADOPTER_INTAKE_REFRESH_OK:True`, `ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True`, and `ADOPTER_CLAIM_EXPANSION_GATE_OK:True`.

Each feedback entry should capture this bounded schema:

- report type (`Real external adoption report` or `Maintainer-seeded rehearsal / synthetic dry-run`)
- adopter context (public project, company, personal handle, or `private adopter` plus the host context)
- route (`HelloWorld`, `AsterGraph.Starter.Avalonia`, `HelloWorld.Avalonia`, `ConsumerSample.Avalonia`, `HostSample`, `PackageSmoke`, `ScaleSmoke`, `Demo`)
- version
- proof markers
- friction
- support-bundle attachment note: `SUPPORT_BUNDLE_PATH:...` when the route produced a bundle, or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it did not
- claim-expansion status (`No support/capability expansion requested`, `Candidate support/capability expansion`, or `Unsure / needs maintainer triage`)

Screenshots or command output can be attached as supplemental evidence, but they do not replace proof markers in the bounded intake field.

A real external report must come from someone evaluating or embedding AsterGraph outside maintainer rehearsal. Maintainer-seeded rehearsals and synthetic dry-runs stay useful for checking the intake path, but they do not count toward the 3-5 real external reports required before a support or capability claim can widen. A single report does not widen public claims; it can only become a candidate signal for maintainer triage.

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

The current 0.xx alpha/beta hardening line is `Adoption Readiness / Release Candidate Hygiene`: keep the public recommendation, support boundary, API drift checks, and release proof story aligned before widening any capability claim. The previous `Performance / Export Hardening` work is now defended evidence, not the next public recommendation.

Current proof handoff markers: `ADOPTION_RECOMMENDATION_CURRENT_OK:True` and `CLAIM_HYGIENE_BOUNDARY_OK:True`.
Milestone handoff markers: `ADOPTION_READINESS_HANDOFF_OK:True`, `ADOPTION_SCOPE_BOUNDARY_OK:True`, and `V056_MILESTONE_PROOF_OK:True`.

1. **seeded rehearsals do not count toward the 3-5 gate**
2. **each real external report must keep route, version, proof markers, friction, support-bundle attachment note, and claim-expansion status in the same bounded schema**
3. **keep adoption evidence, API drift, support boundary, and release proof gates visible before any release-candidate, GA, or 1.0 language**
4. **do not widen support or capability claims before 3-5 real external reports cluster on the same bounded risk**

Phase 380 refresh proof: `ADOPTER_INTAKE_REFRESH_OK:True` keeps this page, the GitHub intake template, and adopter triage checklist on the same bounded schema; `ADOPTER_SUPPORT_BUNDLE_ATTACHMENT_OK:True` keeps `SUPPORT_BUNDLE_PATH:...` or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` attached to that same record; `ADOPTER_CLAIM_EXPANSION_GATE_OK:True` keeps claim expansion blocked until 3-5 real external reports cluster on the same bounded risk.

Until that threshold is met, keep this seeded recommendation in place instead of widening the next beta line ad hoc.

## Related Docs

- [Beta Support Bundle](./support-bundle.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
- [Consumer Sample](./consumer-sample.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
