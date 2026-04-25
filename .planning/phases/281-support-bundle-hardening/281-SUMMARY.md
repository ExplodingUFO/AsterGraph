# Phase 281: Support Bundle Hardening And Issue Template Activation - Summary

**Completed:** 2026-04-25

## Delivered

- Extended `ConsumerSampleProofResult` with `NodeCount`, `ConnectionCount`, `FeatureDescriptorIds`, `RecentDiagnosticCodes`
- Extended `ConsumerSampleSupportBundleDocument` with `GraphSummary`, `FeatureDescriptors`, `RecentDiagnostics`
- `ConsumerSampleProof.Run()` now populates graph/diagnostics context from `host.Session`
- Updated `ConsumerSampleProofTests.CanWriteSupportBundleContract` with new field assertions
- Updated `adoption_feedback.yml` with `.NET SDK version`, `UI adapter`, `Custom nodes/ports/edges` fields
- Updated `bug_report.md` environment section

## Files Changed

- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs`
- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleSupportBundle.cs`
- `tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleProofTests.cs`
- `.github/ISSUE_TEMPLATE/adoption_feedback.yml`
- `.github/ISSUE_TEMPLATE/bug_report.md`

## Key Decisions

- Schema version stays at 1 (additive-only change)
- Optional record parameters preserve backward compatibility with existing test constructors
- Feature descriptors filtered to `IsAvailable`; diagnostics deduplicated and capped at 16

## Next Phase

Phase 282 Triage Workflow And Rapid Docs Fix
