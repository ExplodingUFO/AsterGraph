# Phase 281: Support Bundle Hardening And Issue Template Activation - Plan

**Phase:** 281
**Goal:** Make the support bundle capture enough context for first-line triage; activate the GitHub issue template for structured adopter reports.

## Tasks

### Task 1: Extend ConsumerSampleProofResult with graph and diagnostics context
- Add `NodeCount`, `ConnectionCount`, `FeatureDescriptorIds`, `RecentDiagnosticCodes` optional parameters
- Populate in `ConsumerSampleProof.Run()` from `host.Session.Queries` / `host.Session.Diagnostics`

### Task 2: Extend ConsumerSampleSupportBundle schema
- Add `ConsumerSampleSupportGraphSummary` record
- Add `GraphSummary`, `FeatureDescriptors`, `RecentDiagnostics` to `ConsumerSampleSupportBundleDocument`
- Wire into `WriteProofBundle`

### Task 3: Update tests
- Extend `CanWriteSupportBundleContract` to assert new fields exist and are populated

### Task 4: Update GitHub issue templates
- `adoption_feedback.yml`: add `.NET SDK version`, `UI adapter`, `Custom nodes/ports/edges`
- `bug_report.md`: sync environment section

## Verification Criteria

- [x] Support bundle JSON contains `graphSummary.nodeCount` and `graphSummary.connectionCount`
- [x] Support bundle JSON contains non-empty `featureDescriptors` array
- [x] Support bundle JSON contains `recentDiagnostics` array
- [x] Issue template asks for `.NET SDK`, `UI adapter`, and custom node usage
- [x] All 789 tests pass
