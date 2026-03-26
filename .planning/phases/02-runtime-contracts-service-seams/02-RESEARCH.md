# Phase 2: Runtime Contracts & Service Seams - Research

**Researched:** 2026-03-26
**Domain:** Framework-neutral editor runtime contracts, replaceable editor services, and lightweight batching/transaction seams
**Confidence:** MEDIUM-HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** `GraphEditorViewModel` remains the compatibility facade during Phase 2; the new runtime contracts must be extracted behind or alongside it instead of replacing it outright.
- **D-02:** Phase 2 should reduce the amount of behavior implemented directly inside `GraphEditorViewModel`; it should move toward being a coordinator over smaller runtime contracts and services.
- **D-03:** The main Phase 2 deliverable is a framework-neutral runtime contract layer centered on `session / state / commands / queries / events`.
- **D-04:** Hosts should be able to execute editor behavior through public runtime contracts in `AsterGraph.Editor` without calling Avalonia control internals.
- **D-05:** Queries and events should become first-class public runtime surfaces, not secondary helpers hanging off UI-facing code.
- **D-06:** The first service seams to extract and formalize are `workspace`, `fragment`, `template`, `clipboard`, and `diagnostics`, plus seams directly tied to persistence and serialization flows.
- **D-07:** Existing `localization`, `presentation`, and `context menu augmentor` seams are already relatively mature and are not the primary battle for this phase.
- **D-08:** Default service implementations must keep working without demo-branded storage assumptions.
- **D-09:** Phase 2 should introduce a lightweight public batching/transaction concept so hosts can group operations and receive coherent runtime notifications.
- **D-10:** Do not introduce a full command bus, event bus, or heavy messaging architecture in this phase.

### the agent's Discretion
- Exact contract names and file layout inside `AsterGraph.Editor`
- Whether the batching API is disposable-scope, callback-based, or explicit begin/end

### Deferred Ideas (OUT OF SCOPE)
None.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| API-01 | Host can execute editor commands through stable public APIs instead of invoking Avalonia control internals | Extract a host-facing session/command surface from `GraphEditorViewModel` |
| API-02 | Host can query current selection, viewport, document snapshot, and active capabilities through stable public APIs | Publish a read/query surface over existing editor state and projections |
| API-03 | Host can subscribe to typed events for document changes, selection changes, viewport changes, command execution, and recoverable failures | Regularize current event args and extend with command/diagnostic notifications |
| API-04 | Host can group related editor mutations into a transaction or equivalent batched operation surface | Add a lightweight mutation scope / transaction contract rather than a heavy bus |
| SERV-01 | Host can replace storage, clipboard, serialization, compatibility, localization, presentation, and diagnostics services through documented interfaces or options | Start with storage/clipboard/diagnostics and persistence-adjacent seams; keep existing mature seams compatible |
| SERV-02 | Default service implementations do not rely on demo-branded storage roots or host identity assumptions | Move current concrete services behind replaceable interfaces and eliminate demo defaults |
</phase_requirements>

## Summary

Phase 2 should be an extraction phase, not a rewrite phase. The codebase already has the right *shape* for a layered SDK, but most host-facing runtime behavior is still concentrated inside `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`. The right move is to introduce one explicit runtime session contract and a handful of narrower contracts around it, then adapt the existing `GraphEditorViewModel` facade to delegate into that runtime instead of continuing to grow.

The repo already has useful raw materials:
- typed event args under `src/AsterGraph.Editor/Events/`
- a host-facing factory/options entry point from Phase 1 under `src/AsterGraph.Editor/Hosting/`
- concrete service implementations for workspace, fragment workspace, template library, clipboard payloads, and history under `src/AsterGraph.Editor/Services/`

That means the Phase 2 plan should focus on extracting **public interfaces/contracts** around those concrete implementations, plus introducing a **host-facing runtime session surface** that owns commands, queries, events, and lightweight transactions. `GraphEditorViewModel` should then wrap or forward into that runtime, preserving compatibility for later phases.

**Primary recommendation:** Introduce a small runtime contract family rooted in a public `IGraphEditorSession`-style API, keep `GraphEditorViewModel` as the compatibility shell over it, extract persistence/clipboard/diagnostics services behind interfaces, and add a narrow batching scope that groups mutations and emits coherent notifications.

## Standard Stack

### Core
| Area | Current State | Recommendation | Why |
|------|---------------|----------------|-----|
| Runtime entry | `AsterGraphEditorFactory` creates `GraphEditorViewModel` | Keep factory as the host entry and have it compose the new runtime session internally | Preserves Phase 1 compatibility while enabling Phase 2 extraction |
| Commands | `RelayCommand` / `IAsyncRelayCommand` properties live on `GraphEditorViewModel` | Keep command objects for compatibility, but back them with a runtime command surface and reusable operations | Avoids breaking existing binding while enabling host-facing non-Avalonia execution |
| Events | `DocumentChanged`, `SelectionChanged`, `ViewportChanged`, `FragmentExported`, `FragmentImported` already exist | Preserve and regularize them, then add command/diagnostic events where needed | Existing event surface is a strong starting point |
| Services | Concrete classes only for workspace/fragment/template/history/clipboard | Introduce interfaces first, keep current implementations as defaults | Lowest-risk way to reach `SERV-01` / `SERV-02` |

### Supporting
| Area | Current State | Recommendation | Why |
|------|---------------|----------------|-----|
| Diagnostics | Mostly `StatusMessage` plus some events | Add a runtime-facing diagnostics seam in `AsterGraph.Editor` that can emit machine-readable notifications | Required for later Phase 5, but should start in runtime extraction |
| Transactions | No explicit public transaction API | Add a lightweight mutation scope | Needed for `API-04` without over-engineering |
| Serialization-adjacent seams | Serializer and payload services are concrete | Make persistence-related services replaceable before touching Avalonia surfaces | Hosts need these seams before UI decomposition |

## Architecture Patterns

### Recommended Project Structure

```text
src/AsterGraph.Editor/
├── Hosting/              # entry factories/options (already exists)
├── Contracts/            # new public runtime interfaces (Phase 2)
├── Events/               # typed runtime events (extend existing folder)
├── Services/             # default implementations behind interfaces
├── Session/ or Runtime/  # concrete runtime session/state coordinator
└── ViewModels/           # compatibility facade layer over runtime
```

### Pattern 1: Session-First Runtime Surface
**What:** Introduce a single public runtime session contract that exposes commands, queries, events, and batching in one host-facing root.

**Why:** Hosts need one stable runtime object to hold onto, rather than five unrelated services and a giant compatibility facade.

**Recommended shape:**
- `IGraphEditorSession`
- `IGraphEditorState` or a read-only session snapshot/query surface
- `IGraphEditorCommandService` or command methods/properties on the session
- runtime events for document/selection/viewport/command/diagnostic activity

**Consequence if skipped:** Phase 2 will only shuffle concrete services around without actually giving hosts a usable runtime contract.

### Pattern 2: Compatibility Facade Over Extracted Runtime
**What:** Keep `GraphEditorViewModel` public and let it delegate to the new runtime/session implementation.

**Why:** The repo just established a stable Phase 1 migration story around `GraphEditorViewModel`; ripping it out immediately would invalidate the work that just landed.

**Implication:** Plans should include tasks that move logic behind the facade while keeping existing members alive.

### Pattern 3: Interface-First Service Extraction
**What:** Start by defining interfaces for the concrete persistence/clipboard/template services, then update the factory/options path to accept those interfaces or bridge to the existing concrete implementations.

**Good Phase 2 targets:**
- workspace load/save
- fragment import/export/delete
- template library enumerate/load/save/delete
- system clipboard bridge / clipboard payload pipeline
- diagnostics sink or diagnostics publisher

**Do not over-scope:** localization/presentation/context-menu augmentor are already acceptable seams and can stay stable for now.

### Pattern 4: Lightweight Mutation Scope
**What:** Add a transaction or batching contract that groups multiple mutations and defers/coalesces notifications.

**Recommended shape:** a small, explicit scope such as:
- `BeginTransaction(...)` / `Complete()`
- or `RunBatch(string label, Action action)`
- or a disposable mutation scope with commit semantics

**Why:** This meets `API-04` without forcing a full architectural message bus into the repo.

## Existing Code Signals

### Events Already Worth Preserving

From `src/AsterGraph.Editor/Events/`:
- `GraphEditorDocumentChangedEventArgs`
- `GraphEditorSelectionChangedEventArgs`
- `GraphEditorViewportChangedEventArgs`
- `GraphEditorFragmentEventArgs`
- `GraphEditorDocumentChangeKind`

These provide strong raw material for the new public runtime event surface. The missing piece is not event typing but event *organization* and making command/diagnostic outcomes equally observable.

### Concrete Service Extractions Already Pointed At The Phase Goal

The following concrete classes are natural first extraction targets:
- `GraphWorkspaceService`
- `GraphFragmentWorkspaceService`
- `GraphFragmentLibraryService`
- `GraphSelectionClipboard`
- `GraphEditorHistoryService`
- `IGraphTextClipboardBridge` already exists as a useful bridge precedent

Notable constraint: `GraphWorkspaceService`, `GraphFragmentWorkspaceService`, and `GraphFragmentLibraryService` still default to `LocalApplicationData/AsterGraphDemo`, so Phase 2 must remove or encapsulate that behavior behind host-owned options/interfaces.

### `GraphEditorViewModel` Still Owns Too Much

`GraphEditorViewModel.cs` currently combines:
- command construction and permission gating
- event emission
- workspace/fragment/template IO
- clipboard integration
- history and dirty tracking
- selection and viewport updates
- context menu hosting
- document snapshot construction

This confirms the phase boundary is correct: the main problem is not a missing public type, but a missing *runtime contract boundary* around code that already exists.

## Recommended Contract Breakdown

This is the smallest useful contract family for Phase 2:

1. **Runtime session root**
   - `IGraphEditorSession`
   - host-facing root for commands, queries, events, and transaction entry

2. **Read/query surface**
   - selection ids
   - viewport state
   - document snapshot
   - capability/permission state

3. **Command/mutation surface**
   - node add/delete/duplicate
   - connection create/delete/disconnect
   - workspace/fragment/template commands
   - layout and viewport commands

4. **Transaction/batch surface**
   - mutation scope or grouped change execution

5. **Replaceable service seams**
   - workspace persistence
   - fragment persistence
   - template library
   - clipboard bridge/payload integration
   - diagnostics publishing

## Pitfalls

### Pitfall 1: Extracting Interfaces Without a Coherent Session Root
If the phase only adds interfaces like `IWorkspaceService` and `IClipboardService`, hosts will still depend on `GraphEditorViewModel` for everything that matters. That would satisfy `SERV-01` partially but fail `API-01` through `API-04`.

### Pitfall 2: Moving Avalonia Types Into Runtime Contracts
Phase 2 exists specifically to keep hosts out of Avalonia control internals. Any new runtime contract that leaks `Control`, `StyledProperty`, pointer events, or Avalonia view concerns misses the phase goal.

### Pitfall 3: Over-abstracting Too Early
A full command bus, event bus, mediator, or message pipeline would be high-risk and unnecessary for this phase. The repo needs stable host surfaces, not a framework experiment.

### Pitfall 4: Keeping Demo Storage Defaults in “Default” Runtime Services
If service interfaces are added but the default implementations still silently use `AsterGraphDemo` storage roots, the service seam will technically exist while still violating `SERV-02`.

### Pitfall 5: Ignoring Existing Event Contracts
The repo already has typed event args. Replacing them wholesale instead of evolving them would waste existing test and host-sample value and create unnecessary migration churn.

## Code Examples

### Existing Event Surface Worth Reusing
From `GraphEditorViewModel`:
- `DocumentChanged`
- `SelectionChanged`
- `ViewportChanged`
- `FragmentExported`
- `FragmentImported`

These should inform the runtime event contract instead of being discarded.

### Existing Concrete Workspace Service
`GraphWorkspaceService` shows the exact seam that should move behind an interface:
- default path selection
- save/load
- existence check

That makes it a strong target for `IWorkspaceStore`-style extraction.

### Existing Factory Entry Point
`AsterGraphEditorFactory.Create(AsterGraphEditorOptions options)` is now the right insertion point for runtime session composition. Phase 2 should build on that instead of inventing a parallel host entry path.

## Open Questions

1. **Should the public runtime query surface be split from the session root, or should the session expose both commands and read-only state directly?**
   - Recommendation: start with one root session contract and add a read-only view interface if needed for cleanliness. That keeps host ergonomics simple.

2. **How many service interfaces should Phase 2 extract in one pass?**
   - Recommendation: keep the first slice focused on `workspace / fragment / template / clipboard / diagnostics`; leave history internals and mature presentation seams as secondary unless required by the session design.

3. **Should transaction scope be disposable or callback-based?**
   - Recommendation: choose the smallest shape that can coalesce notifications and support host batch edits. Avoid multi-object orchestration APIs in this phase.

## Environment Availability

| Dependency | Required By | Available | Notes |
|------------|-------------|-----------|-------|
| Existing editor events | runtime event extraction | ✓ | Already in `src/AsterGraph.Editor/Events/` |
| Existing factory/options entry point | runtime composition | ✓ | Landed in Phase 1 |
| Existing workspace/fragment/template services | service seam extraction | ✓ | Concrete types already exist |
| Existing tests for editor/runtime behavior | regression validation | ✓ | Strong editor test project exists, though local workspace may contain extra noise files |

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.2 + existing `tests/AsterGraph.Editor.Tests` project |
| Config file | none — existing test project conventions |
| Quick run command | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession|FullyQualifiedName~GraphEditorService|FullyQualifiedName~GraphEditorTransaction" -v minimal` |
| Full suite command | `dotnet test avalonia-node-map.sln -v minimal` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| API-01 | Host can execute commands through runtime contracts | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession"` | ❌ Wave 0 |
| API-02 | Host can query selection/viewport/document/capabilities through runtime contracts | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSession"` | ❌ Wave 0 |
| API-03 | Host can subscribe to typed runtime events including command/diagnostic activity | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorSessionEvents"` | ❌ Wave 0 |
| API-04 | Host can batch related mutations through a lightweight transaction surface | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorTransaction"` | ❌ Wave 0 |
| SERV-01 | Host can replace core runtime services through public seams | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorServiceSeams"` | ❌ Wave 0 |
| SERV-02 | Default runtime services stop depending on demo-branded storage assumptions | unit | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorServiceSeams"` | ❌ Wave 0 |

### Wave 0 Gaps
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs` — command/query/runtime event surface
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorTransactionTests.cs` — lightweight batching or transaction behavior
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorServiceSeamsTests.cs` — default service seams, host replacement, and storage-root behavior

## Sources

### Primary (HIGH confidence)
- `.planning/ROADMAP.md`
- `.planning/REQUIREMENTS.md`
- `.planning/PROJECT.md`
- `.planning/STATE.md`
- `.planning/phases/02-runtime-contracts-service-seams/02-CONTEXT.md`
- `.planning/phases/01-consumption-compatibility-guardrails/01-CONTEXT.md`
- `.planning/phases/01-consumption-compatibility-guardrails/01-RESEARCH.md`
- `.planning/phases/01-consumption-compatibility-guardrails/01-VERIFICATION.md`
- `.planning/codebase/ARCHITECTURE.md`
- `.planning/codebase/CONCERNS.md`
- `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- `src/AsterGraph.Editor/Events/*.cs`
- `src/AsterGraph.Editor/Services/*.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorFactory.cs`
- `src/AsterGraph.Editor/Hosting/AsterGraphEditorOptions.cs`
- `tests/AsterGraph.Editor.Tests/*.cs`

### Secondary (MEDIUM confidence)
- `docs/host-integration.md`
- `docs/plans/2026-03-23-astergraph-editor-facade-refactor.md`
- `docs/plans/2026-03-23-astergraph-host-extensibility.md`

### Notes
- This Phase 2 research was produced locally as a fallback because the external research agent hit a transient `429 Too Many Requests` limit. Confidence remains medium-high because the phase is primarily about repo-internal contract extraction and existing code evidence.

## Metadata

**Confidence breakdown:**
- Runtime surface shape: HIGH
- Service seam priorities: HIGH
- Transaction/batching design: MEDIUM
- Validation architecture: MEDIUM-HIGH

**Research date:** 2026-03-26
**Valid until:** 2026-04-25
