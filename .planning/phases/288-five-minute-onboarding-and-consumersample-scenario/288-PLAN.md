# Phase 288: Five-Minute Onboarding And ConsumerSample Scenario - Plan

## Goal

Turn ConsumerSample and the quick-start docs into a realistic copyable host path between HelloWorld and the full Demo.

## Success Criteria

1. A new Avalonia host can follow the five-minute quick start from package install or starter run to first graph load/save and first custom node definition.
2. README and quick-start docs clearly explain when to copy Starter, HelloWorld, HelloWorld.Avalonia, ConsumerSample, or the full Demo.
3. ConsumerSample opens with a scenario graph, host-owned actions, parameter editing, trusted plugin flow, support-bundle proof, and stable onboarding markers.

## Tasks

1. Defend the ConsumerSample scenario identity.
   - Expose scenario id/title/copy-path lines from `ConsumerSampleHost`.
   - Add proof checks for the review-to-queue graph, custom node definitions, plugin definition, command descriptors, and parameter seam.

2. Add stable onboarding proof markers.
   - Extend `ConsumerSampleProofResult` with scenario/onboarding booleans.
   - Emit markers for scenario graph load, host-owned actions, support-bundle readiness, and five-minute onboarding health.
   - Keep existing markers intact.

3. Update onboarding documentation.
   - Add a five-minute checklist to English and Chinese quick-start docs.
   - Clarify route choice across Starter, HelloWorld, HelloWorld.Avalonia, ConsumerSample, and Demo.
   - Mirror the new proof marker list in ConsumerSample README/docs.

4. Verify the route.
   - Add focused ConsumerSample tests for scenario shape, proof lines, and support-bundle proof payload.
   - Run ConsumerSample and quick-start proof tests.

## Verification Commands

```powershell
dotnet test tests/AsterGraph.ConsumerSample.Tests/AsterGraph.ConsumerSample.Tests.csproj -c Release --nologo -v minimal
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj -c Release --nologo -v minimal --filter "FullyQualifiedName~ConsumerSample|FullyQualifiedName~QuickStart|FullyQualifiedName~DemoProofReleaseSurfaceTests"
```
