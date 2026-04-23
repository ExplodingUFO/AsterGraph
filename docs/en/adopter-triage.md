# Adopter Triage Checklist

When a beta report is filed, keep the same bounded intake record fields together:

- `route` or artifact attempted
- package `version` (or public tag)
- `proof markers` observed
- `friction` summary
- `support-bundle attachment note`: `SUPPORT_BUNDLE_PATH:...` when the route produced a bundle, or `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it did not

Use the same fields in:

- the `adoption_feedback.yml` issue template
- public issue templates using the bug report form
- the release-note evidence summary and prerelease review.

This checklist keeps all intake documents on one beta evidence contract, so route selection and support readiness can be triaged consistently without blocking `HelloWorld`, `Demo`, or `ScaleSmoke` feedback when no support bundle exists yet.

Use [Project Status](./project-status.md) as the readiness gate before treating a report as a support-expansion candidate. Reports outside the proven/bounded rows are intake evidence, not automatic scope widening. Reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the support-bundle attachment note when the route can produce a bundle, or record `NO_SUPPORT_BUNDLE:route-cannot-produce-one` when it cannot.

## Parameter Snapshot Classification

Use this bounded evidence record vocabulary when classifying failures: `route`, `version`, `proof markers`, `friction`, `support-bundle attachment note`, and `parameterSnapshots`.

Keep `status`, `owner`, and `priority` visible when you inspect `parameterSnapshots` so the evidence stays tied to the selected node and its host-owned metadata.

| Failed proof marker | Classify as | Inspect in `parameterSnapshots` | Next action |
| --- | --- | --- | --- |
| `CONSUMER_SAMPLE_PARAMETER_OK` | parameter projection/write-path failure | current/default/editable/valid rows | sample/session parameter projection or write-path investigation |
| `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` | metadata projection failure | metadata such as editor kind/default/constraints/options | definition/inspector metadata projection investigation |
| `SUPPORT_BUNDLE_PERSISTENCE_OK` | support-bundle persistence failure | support-bundle path/write failure/environment | persistence/path/environment investigation |

## Related Links

- [Adoption Feedback](./adoption-feedback.md)
- [Beta Support Bundle](./support-bundle.md)
