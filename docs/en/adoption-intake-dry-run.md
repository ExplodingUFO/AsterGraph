# Synthetic Adoption Intake Dry-Run Fixtures

These synthetic dry-run rehearsal records keep the bounded intake fields together: route, version, proof markers, friction, and support-bundle attachment note. They are fixtures only and are not external validation or adopter evidence.

## Record 1: Parameter Projection Failure

- route: `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`
- version: `v0.0.0-dry-run`
- proof markers: `CONSUMER_SAMPLE_PARAMETER_OK:False`
- friction: parameter projection did not surface the expected host-owned values on the selected node
- support-bundle attachment note: `SUPPORT_BUNDLE_PATH:synthetic/parameter-projection.bundle`

parameterSnapshots

| key | value |
| --- | --- |
| status | blocked |
| owner | host-seeded |
| priority | P2 |

## Record 2: Metadata Projection Failure

- route: `ConsumerSample.Avalonia -- --proof --support-bundle <support-bundle-path>`
- version: `v0.0.0-dry-run`
- proof markers: `CONSUMER_SAMPLE_METADATA_PROJECTION_OK:False`
- friction: metadata projection did not expose the expected inspector fields for the selected node
- support-bundle attachment note: `SUPPORT_BUNDLE_PATH:synthetic/metadata-projection.bundle`

parameterSnapshots

| key | value |
| --- | --- |
| status | draft |
| owner | maintainer-seeded |
| priority | P1 |

## Record 3: Support-Bundle Persistence Failure

- route: `ConsumerSample.Avalonia -- --proof`
- version: `v0.0.0-dry-run`
- proof markers: `SUPPORT_BUNDLE_PERSISTENCE_OK:False`
- friction: the dry-run stopped before a bundle could be written and reused
- support-bundle attachment note: `NO_SUPPORT_BUNDLE:route-cannot-produce-one`

This third synthetic rehearsal record intentionally has no `parameterSnapshots` section because no bundle was persisted.
