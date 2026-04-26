# Phase 288: Five-Minute Onboarding And ConsumerSample Scenario - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Autonomous context from roadmap, Phase 287 output, ConsumerSample inspection, and quick-start docs

<domain>
## Phase Boundary

Phase 288 owns the onboarding path between the smallest HelloWorld samples and the full Demo. It should make `tools/AsterGraph.ConsumerSample.Avalonia` a realistic copyable host sample with stable proof markers, and make README/Quick Start guidance shorter for new Avalonia adopters. It does not own SDK runtime architecture, a new compatibility layer, marketplace behavior, or the Phase 289 builder/facade API.

</domain>

<decisions>
## Implementation Decisions

- Keep the implementation sample-local: `ConsumerSampleHost`, proof result markers, support-bundle proof lines, sample README, and quick-start docs.
- Reuse the existing review-to-queue graph as the scenario rather than adding another sample app or a second shell.
- Add explicit scenario/onboarding markers to the existing proof result so CI can catch drift in the copyable host path.
- Keep support-bundle evidence as local proof lines inside the existing bundle schema; avoid adding a new bundle format unless required.
- Keep docs direct: identify when to copy Starter, HelloWorld, HelloWorld.Avalonia, ConsumerSample, and Demo.

</decisions>

<code_context>
## Existing Code Insights

- `ConsumerSampleHost.Create(...)` already builds a host-owned review graph, plugin trust policy, plugin registration, and selected-node parameter seam.
- `ConsumerSampleProof.Run()` already exercises host actions, plugin command contribution, parameter editing, trust allow/block/import/export, export breadth, accessibility, and native hosted metrics.
- `ConsumerSampleSupportBundle.WriteProofBundle(...)` persists `ProofLines`, parameter snapshots, graph summary, feature descriptors, diagnostics, and reproduction metadata.
- `tools/AsterGraph.ConsumerSample.Avalonia/README.md`, `docs/en/quick-start.md`, `docs/zh-CN/quick-start.md`, and `docs/*/consumer-sample.md` already describe the route ladder but need a tighter five-minute path and stable onboarding marker list.

</code_context>

<specifics>
## Specific Ideas

- Add public scenario identity and copy-path lines to `ConsumerSampleHost`.
- Add proof booleans for scenario graph load, host-owned action readiness, support-bundle payload readiness, and five-minute onboarding health.
- Emit proof lines such as `CONSUMER_SAMPLE_SCENARIO_GRAPH_OK`, `CONSUMER_SAMPLE_HOST_OWNED_ACTIONS_OK`, `CONSUMER_SAMPLE_SUPPORT_BUNDLE_READY_OK`, and `FIVE_MINUTE_ONBOARDING_OK`.
- Add tests that fail if the scenario graph loses its review/queue/plugin shape or if support bundles no longer carry onboarding proof lines.
- Update quick-start docs with a five-minute checklist and a route-choice table.

</specifics>

<deferred>
## Deferred Ideas

- A `dotnet new` template.
- A host-friendly builder/facade API.
- A plugin validator CLI or marketplace.
- New runtime compatibility shims or fallback surfaces.

</deferred>
