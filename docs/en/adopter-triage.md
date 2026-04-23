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

## Related Links

- [Adoption Feedback](./adoption-feedback.md)
- [Beta Support Bundle](./support-bundle.md)
