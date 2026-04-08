# Phase 22 Research: Plugin Composition Contracts

**Date:** 2026-04-08
**Phase:** 22-plugin-composition-contracts

## Research Questions

1. What is the smallest public plugin-loading surface that fits the shipped `CreateSession(...)` / `Create(...)` composition story?
2. What current runtime surfaces already exist for canonical discoverability and recoverable failure reporting?
3. What does official .NET guidance imply for assembly loading, shared contracts, and scope limits?

## Findings

### 1. The composition choke point already exists

`AsterGraphEditorOptions` and `AsterGraphEditorFactory` are already the only public composition roots for:

- runtime-first `CreateSession(...)`
- retained compatibility `Create(...)`
- default service construction
- runtime/session instrumentation and diagnostics wiring

Implication:

- Phase 22 should add plugin registration/loading to this same options/factory path
- a separate plugin bootstrap API would fracture the canonical host story that v1.2 established

### 2. Canonical discoverability is already descriptor-first

The project already has a stable host-facing path for discoverability and proof:

- `IGraphEditorQueries.GetFeatureDescriptors()`
- `GraphEditorInspectionSnapshot.FeatureDescriptors`
- `IGraphEditorDiagnostics` plus recent diagnostics

Implication:

- loader availability should be surfaced through these existing descriptor/diagnostic channels
- Phase 22 does not need a UI-specific or MVVM-specific discovery path

### 3. Official .NET plugin guidance favors custom AssemblyLoadContext + AssemblyDependencyResolver

Microsoft’s plugin tutorial and loader docs point to the same baseline:

- use a custom `AssemblyLoadContext` for plugin loading
- use `AssemblyDependencyResolver` so plugin dependencies resolve from the plugin’s own dependency graph
- keep shared contract assemblies stable between host and plugin so type identity remains compatible

Implication for AsterGraph:

- the first assembly-path loader should be based on `AssemblyLoadContext` + `AssemblyDependencyResolver`
- `AsterGraph.*` runtime/contract assemblies should stay shared from the default load context instead of being duplicated into plugin-local identity islands

### 4. In-process plugins are not a security boundary

Official .NET documentation is explicit that untrusted code cannot be safely loaded into a trusted process through `AssemblyLoadContext` alone.

Implication:

- Phase 22 should make no trust, signing, sandbox, or isolation claims
- the first loader baseline should stay honest: in-process managed plugins only
- security and isolation policy remain deferred milestone work

### 5. Unloadability is real complexity, not baseline value

The official guidance around `AssemblyLoadContext` unloadability is cooperative and easy to invalidate with lingering references.

Implication:

- Phase 22 should not require collectible/unloadable plugin contexts to be considered complete
- the first baseline can be non-collectible or explicitly “no unload guarantee”
- this keeps the phase focused on public composition contracts instead of lifecycle edge cases

### 6. The missing test asset is a real planning gap

Current tests validate runtime descriptors, canonical command/query/event surfaces, and service seam continuity, but they do not prove:

- loading a plugin assembly from a public factory/options path
- preserving `AsterGraph.*` type identity across a plugin load boundary
- surfacing plugin-loader availability or plugin-load failures through canonical descriptors/diagnostics

Implication:

- Phase 22 should create a focused plugin test fixture assembly and dedicated loading tests
- broader proof-ring integration with `HostSample` / `PackageSmoke` / `ScaleSmoke` can remain Phase 25 work

## Recommended Planning Posture

### Wave 1: Public plugin contracts

Define the public plugin interface, descriptor, registration, and builder/contribution shape in `AsterGraph.Editor`, then extend `AsterGraphEditorOptions` to accept plugin registrations without any Avalonia or MVVM dependency.

### Wave 2: Factory loading baseline

Implement the first in-process loader in the canonical factory path using a custom `AssemblyLoadContext` plus `AssemblyDependencyResolver`, then expose loader availability and recoverable failures through existing descriptor/diagnostic channels.

### Wave 3: Focused proof for the contract boundary

Add a fixture plugin assembly and targeted tests that prove:

- plugin registrations load through `CreateSession(...)` and `Create(...)`
- shared `AsterGraph.*` contract identity survives the load boundary
- canonical feature descriptors/diagnostics reflect the loader baseline

## Risks And Guardrails

- Do not let Phase 22 drift into plugin contribution wiring; that belongs to Phase 23.
- Do not claim marketplace, trust, signing, or sandbox behavior from an in-process loader.
- Do not introduce a plugin surface that depends on Avalonia controls or `GraphEditorViewModel`.
- Do not hide plugin failures behind silent best-effort behavior; emit recoverable diagnostics and test them.

## External References

- Microsoft Learn: [Create a .NET Core application with plugins](https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support)
- Microsoft Learn: [About System.Runtime.Loader.AssemblyLoadContext](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- Microsoft Learn: [AssemblyDependencyResolver Class](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblydependencyresolver?view=net-9.0)

---

*Research complete: 2026-04-08*
