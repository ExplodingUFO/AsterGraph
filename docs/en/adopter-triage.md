# Adopter Triage Checklist

When a beta report is filed, include all four evidence dimensions together:

- package `version` (or public tag)
- `route` or artifact attempted
- `proof` markers observed
- `support bundle` JSON path when available from `ConsumerSample.Avalonia -- --proof`; otherwise note that the route could not produce one

Use the same fields in:

- the `adoption_feedback.yml` issue template
- public issue templates using the bug report form
- the release-note evidence summary and prerelease review.

This checklist keeps all intake documents on one beta evidence contract, so route selection and support readiness can be triaged consistently without blocking `HelloWorld`, `Demo`, or `ScaleSmoke` feedback when no support bundle exists yet.

Use [Project Status](./project-status.md) as the readiness gate before treating a report as a support-expansion candidate. Reports outside the proven/bounded rows are intake evidence, not automatic scope widening.

## Related Links

- [Adoption Feedback](./adoption-feedback.md)
- [Beta Support Bundle](./support-bundle.md)
