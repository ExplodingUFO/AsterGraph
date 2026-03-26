# Phase 5: Diagnostics & Integration Inspection - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-26
**Phase:** 05-diagnostics-integration-inspection
**Areas discussed:** diagnostics contract evolution, inspection snapshot scope, logging/tracing integration, compatibility and adoption path

---

## Diagnostics Contract Evolution

| Option | Description | Selected |
|--------|-------------|----------|
| Extend the existing machine-readable diagnostics surface | Keep `GraphEditorDiagnostic`, `IGraphEditorDiagnosticsSink`, and recoverable-failure events as the base and expand them into a fuller diagnostics contract | ✓ |
| Replace diagnostics with log-only output | Treat logging as the only host-visible diagnostics surface | |
| Keep status text as the main contract | Continue relying on `StatusMessage` and UI strings for integration troubleshooting | |

**User's choice:** `[auto]` Selected the recommended path that extends the existing machine-readable diagnostics surface.
**Notes:** Chosen because Phase 2 already established a public diagnostics sink and recoverable-failure path; Phase 5 should deepen that contract instead of discarding it.

---

## Inspection Snapshot Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Medium-grain session inspection snapshot | Expose immutable host-readable inspection snapshots that aggregate current document, selection, viewport, pending connection, capability, and runtime status state | ✓ |
| Fine-grain live object graph inspection | Expose mutable internals or raw service instances directly for debugger-like traversal | |
| Minimal diagnostics-only view | Only expose emitted diagnostics and no current-state inspection surface | |

**User's choice:** `[auto]` Selected the recommended medium-grain session inspection snapshot path.
**Notes:** Chosen because the product goal is host troubleshooting and secondary development, not exposing internal mutable implementation details.

---

## Logging And Tracing Integration

| Option | Description | Selected |
|--------|-------------|----------|
| Host-standard vendor-neutral integration | Add public logging/tracing seams that can plug into .NET host-standard tooling without forcing one telemetry vendor | ✓ |
| Custom AsterGraph logging framework | Introduce a new library-owned logging abstraction for every diagnostics scenario | |
| No logging/tracing, diagnostics records only | Stop at records and snapshots without runtime instrumentation hooks | |

**User's choice:** `[auto]` Selected the recommended host-standard vendor-neutral integration path.
**Notes:** Chosen because DIAG-03 explicitly asks for logging or tracing sinks, and earlier architecture research already points at standard .NET host tooling rather than a custom framework.

---

## Compatibility And Adoption Path

| Option | Description | Selected |
|--------|-------------|----------|
| Add diagnostics through runtime/session and retained compatibility facades | New diagnostics contracts flow through `AsterGraph.Editor` and remain reachable from both factory-created and retained compatibility hosts | ✓ |
| Diagnostics only through a new replacement host shell | Require hosts to adopt a new UI path before they can use diagnostics | |
| Diagnostics only through Avalonia controls | Bind diagnostics access to the default UI stack instead of the editor runtime | |

**User's choice:** `[auto]` Selected the recommended runtime-first compatibility path.
**Notes:** Chosen because the project boundary keeps diagnostics in `AsterGraph.Editor`, and existing hosts should not need a full UI rewrite to gain observability.

---

## the agent's Discretion

- Exact contract names and namespace placement for inspection snapshots and diagnostics publishers
- Whether inspection is modeled as one aggregate snapshot, a small set of focused snapshots, or both
- Whether host-standard logging/tracing integration is expressed through direct .NET abstractions, adapter interfaces, or options objects

## Deferred Ideas

- Full diagnostics workbench UI
- Timeline/replay tooling for gesture-level interaction debugging
- Non-.NET logging ecosystems or non-Avalonia diagnostics UIs
