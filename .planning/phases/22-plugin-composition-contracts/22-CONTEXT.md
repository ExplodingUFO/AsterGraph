# Phase 22: Plugin Composition Contracts - Context

**Gathered:** 2026-04-08
**Status:** Ready for planning
**Source:** v1.4 roadmap + Phase 18 readiness proof + direct plan-phase continuation

<domain>
## Phase Boundary

Phase 22 is the contract and composition entry pass for the new plugin-loading milestone.

This phase is about:

- publishing the first public plugin contract surface in the canonical runtime package
- rooting plugin registration/loading in `AsterGraphEditorFactory.CreateSession(...)`, `AsterGraphEditorFactory.Create(...)`, and `AsterGraphEditorOptions`
- adding the first in-process managed plugin-loading baseline for one or more explicit plugin registrations
- making plugin-loader availability visible from the canonical runtime boundary without forcing hosts to inspect implementation details

This phase is not yet about:

- wiring plugin-contributed menus, presentation, localization, diagnostics, or node providers into live runtime behavior
- adding plugin marketplace/discovery UX, trust policy, signing, sandboxing, or out-of-process isolation
- adding the automation runner or macro execution surface planned for Phase 24
- extending `HostSample`, `PackageSmoke`, `ScaleSmoke`, and broader docs into the full proof ring planned for Phase 25

</domain>

<decisions>
## Implementation Decisions

### Contract placement

- **D-01:** Keep the public plugin contract surface in `AsterGraph.Editor`, not `AsterGraph.Avalonia`. Plugins should target the canonical runtime/session boundary and remain usable by non-Avalonia hosts.
- **D-02:** Root plugin composition in `AsterGraphEditorOptions` and `AsterGraphEditorFactory`. Do not introduce a second bootstrap path or a separate host runtime object.

### Loader posture

- **D-03:** The first loader baseline is in-process managed plugin loading only. It should support explicit host registrations and assembly-path loading, but it must not claim security/isolation guarantees.
- **D-04:** Use a custom `AssemblyLoadContext` plus `AssemblyDependencyResolver` for assembly-path loading so plugin-local dependencies resolve intentionally.
- **D-05:** Keep `AsterGraph.*` contract/runtime assemblies shared from the default load context so plugin type identity remains compatible with the host runtime boundary.

### Scope control

- **D-06:** Phase 22 may define stable plugin descriptor/contribution shapes needed by later phases, but it should not yet wire every contribution into live runtime behavior. Actual seam integration belongs to Phase 23.
- **D-07:** Loader availability should become discoverable through canonical feature descriptors and recoverable diagnostics. Detailed loaded-plugin inspection is Phase 23 scope.
- **D-08:** Load failures should become recoverable diagnostics or explicit load-report data, not host-crashing factory exceptions for ordinary plugin errors.

### the agent's Discretion

- The exact names and shapes of the plugin registration, descriptor, and contribution types.
- Whether the first baseline exposes direct plugin instances, assembly-path registrations, or a unified registration record that can represent both.
- Whether the first custom `AssemblyLoadContext` should be collectible; unloadability is not required by this phase.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Milestone scope and active requirements
- `.planning/PROJECT.md` - v1.4 milestone framing and carry-forward constraints
- `.planning/REQUIREMENTS.md` - `PLUG-01`
- `.planning/ROADMAP.md` - Phase 22 goal, dependency, and success criteria
- `.planning/STATE.md` - active project state and carry-forward concerns

### Carry-forward plugin/automation baseline
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-CONTEXT.md` - readiness proof decisions and deferred platform boundaries
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-RESEARCH.md` - existing seam/discoverability research
- `.planning/phases/18-plugin-and-automation-readiness-proof-ring/18-01-PLAN.md` - explicit seam descriptor posture that Phase 22 must build on
- `.planning/milestones/v1.2-ROADMAP.md` - canonical readiness story and deferred plugin follow-on

### Canonical runtime and composition code
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs` - current host composition root
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs` - current canonical `Create(...)` / `CreateSession(...)` paths
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs` - canonical runtime root
- `src/AsterGraph.Editor/Runtime/IGraphEditorQueries.cs` - canonical descriptor/query surface
- `src/AsterGraph.Editor/Runtime/GraphEditorSession.cs` - runtime feature descriptor and diagnostics projection
- `src/AsterGraph.Editor/Runtime/GraphEditorSessionDescriptorSupport.cs` - current seam-discovery helper
- `src/AsterGraph.Editor/Runtime/GraphEditorFeatureDescriptorSnapshot.cs` - canonical discoverability DTO
- `src/AsterGraph.Editor/Diagnostics/GraphEditorInspectionSnapshot.cs` - inspection snapshot boundary
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModelKernelAdapter.cs` - retained-path runtime adapter

### Existing proof and sample surfaces
- `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` - factory/options contract coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` - runtime descriptor and session-surface coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` - host seam continuity coverage
- `tests/AsterGraph.Editor.Tests/GraphEditorProofRingTests.cs` - retained/runtime/shared proof coverage
- `tools/AsterGraph.HostSample/Program.cs` - current human-readable readiness output
- `tools/AsterGraph.PackageSmoke/Program.cs` - current machine-checkable readiness markers
- `tools/AsterGraph.ScaleSmoke/Program.cs` - current large-graph readiness marker

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets

- `AsterGraphEditorOptions` is already the single public host composition record; adding plugin inputs there keeps the public story coherent.
- `AsterGraphEditorFactory.Resolve(...)` is the current choke point for default service construction and is the obvious location to insert plugin-loading/composition.
- `GraphEditorSession.GetFeatureDescriptors()` and `GraphEditorInspectionSnapshot` already give the project one canonical place to expose loader discoverability without UI coupling.
- The current readiness proof already distinguishes runtime-first `CreateSession(...)` from retained `Create(...)`, so new plugin composition must preserve both routes.

### Current Gaps

- There is no public plugin contract, registration type, or assembly-path loader surface.
- Hosts cannot ask the canonical composition path to load plugins without building their own reflection/bootstrap layer.
- Feature descriptors expose readiness seams, but not the availability of a real plugin loader.
- No focused test fixture currently proves that a plugin assembly can be loaded against the shipped `AsterGraph.*` runtime contracts.

### Integration Points

- `AsterGraphEditorFactory.CreateSession(...)` is the canonical runtime route and should become the first plugin-aware path.
- `AsterGraphEditorFactory.Create(...)` must preserve parity by reusing the same plugin-loading/composition result instead of inventing a retained-only variant.
- `GraphEditorSessionDescriptorSupport` and `GraphEditorSession.GetFeatureDescriptors()` are the minimum required integration points for loader availability proof in this phase.
- Existing focused runtime/service tests are the right place to lock public plugin contract shape before the broader proof ring expands in Phase 25.

</code_context>

<specifics>
## Specific Ideas

- Introduce a `AsterGraph.Editor.Plugins` namespace for `IGraphEditorPlugin`, descriptor DTOs, registration records, and contribution/build hooks.
- Represent plugin inputs through a unified registration surface that can describe either a direct plugin instance or an assembly-path load request.
- Add an internal plugin load context that uses `AssemblyDependencyResolver` for plugin-local dependencies while keeping shared `AsterGraph.*` assemblies in the default context.
- Publish high-signal diagnostics such as `plugin.load.succeeded` and `plugin.load.failed`, plus at least one canonical loader feature descriptor such as `integration.plugin-loader`.

</specifics>

<deferred>
## Deferred Ideas

- Applying plugin-contributed services, menus, presentation providers, localization, or node providers to the live runtime
- Detailed plugin inspection snapshots and load-report queries
- Plugin marketplace/discovery, trust policy, signing, or sandbox/isolation work
- Plugin-driven automation or macro execution

</deferred>

---

*Phase: 22-plugin-composition-contracts*
*Context gathered: 2026-04-08*
