# Adopter Triage Checklist

When a beta report is filed, keep the same bounded intake record fields together:

- `report type`: real external adoption report or maintainer-seeded rehearsal / synthetic dry-run
- `adopter context`: public project, company, personal handle, or `private adopter` plus host context
- `route` or artifact attempted
- package `version` (or public tag)
- `proof markers` observed
- `friction` summary
- `support-bundle attachment note`: `SUPPORT_BUNDLE_PATH:...` when the route produced a bundle, or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it did not
- `claim-expansion status`: no expansion requested, candidate expansion, or maintainer triage needed

Current triage proof markers: `ADOPTION_INTAKE_EVIDENCE_OK:True`, `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True`, and `REAL_EXTERNAL_REPORT_GATE_OK:True`.

Use the same fields in:

- the `adoption_feedback.yml` issue template
- public issue templates using the bug report form
- the release-note evidence summary and prerelease review.

This checklist keeps all intake documents on one beta evidence contract, so route selection and support readiness can be triaged consistently without blocking `HelloWorld`, `Demo`, or `ScaleSmoke` feedback when no support bundle exists yet.

Use [Project Status](./project-status.md) as the readiness gate before treating a report as a support-expansion candidate. Reports outside the proven/bounded rows are intake evidence, not automatic scope widening. A single report does not widen public claims; support or capability expansion needs 3-5 real external reports clustered on the same bounded risk. Reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note when the route can produce a bundle, or record `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it cannot.

## Intake Criteria Checklist

Before a report is accepted into the triage queue, verify it contains at minimum:

- [ ] **Route identified**: which sample, route, or artifact was tried
- [ ] **Report type and adopter context captured**: real external or rehearsal, with the adopter/host context stated
- [ ] **Version stated**: NuGet package version or public tag
- [ ] **Proof markers included**: at least the `CONSUMER_SAMPLE_OK` or relevant route-specific marker
- [ ] **Friction described**: what slowed down or felt unclear
- [ ] **Support bundle note**: `SUPPORT_BUNDLE_PATH:...` or `NO_SUPPORT_BUNDLE:reason`
- [ ] **Claim-expansion status captured**: no expansion, candidate expansion, or maintainer triage needed

Reports missing ≥2 of these fields should be bounced back with a request to complete the intake template.

## Triage Routing

Classify every complete report into one of three buckets:

| Bucket | Criteria | Owner | Resolution target |
|--------|----------|-------|-------------------|
| **docs-fix** | friction is "I could not follow the doc/step" or proof marker failure is caused by outdated docs/sample | docs | next patch release |
| **sample-fix** | friction is "the sample did not run as documented" or proof marker failure reproduces on clean checkout | samples / CI | next patch release |
| **capability-gap** | friction is "I need feature X to proceed" or proof marker failure indicates missing SDK capability | product / roadmap | next milestone planning |

A report moves from **docs-fix** → **sample-fix** when the doc is verified correct but the sample still fails. A report moves from **sample-fix** → **capability-gap** when the sample is verified correct but the SDK behavior does not match the contract.

## Parameter Snapshot Classification

Use this bounded evidence record vocabulary when classifying failures: `route`, `version`, `proof markers`, `friction`, `support-bundle attachment note`, and `parameterSnapshots`.

Keep `status`, `owner`, and `priority` visible when you inspect `parameterSnapshots` so the evidence stays tied to the selected node and its host-owned metadata.

| Failed proof marker | Classify as | Inspect in `parameterSnapshots` | Next action |
| --- | --- | --- | --- |
| `CONSUMER_SAMPLE_PARAMETER_OK` | parameter projection/write-path failure | current/default/editable/valid rows | sample/session parameter projection or write-path investigation |
| `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` | metadata projection failure | metadata such as editor kind/default/constraints/options | definition/inspector metadata projection investigation |
| `SUPPORT_BUNDLE_PERSISTENCE_OK` | support-bundle persistence failure | support-bundle path/write failure/environment | persistence/path/environment investigation |
| `HOSTED_ACCESSIBILITY_OK` | accessibility surface failure | automation navigation, focus, command surface | adapter or host integration investigation |
| `WIDENED_SURFACE_PERFORMANCE_OK` | performance regression | host native metrics (startup, latency, search) | environment or recent change investigation |

## Related Links

- [Adoption Feedback](./adoption-feedback.md)
- [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md)
- [Beta Support Bundle](./support-bundle.md)
