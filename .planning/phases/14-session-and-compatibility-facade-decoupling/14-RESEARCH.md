# Phase 14: Session And Compatibility Facade Decoupling - Research

## Summary

Phase 13 solved composition, not convergence.

The canonical `CreateSession(...)` path is now kernel-first, but the compatibility path still keeps a second runtime implementation alive inside `GraphEditorViewModel`. That means the product still pays dual-path drift costs in three places:

- state ownership: kernel path vs VM-owned mutable graph state
- session hosting: `GraphEditorSession` can speak to either a real kernel or a VM-shaped compatibility host
- host-facing reads: DTO snapshots are canonical for the new runtime, but compatibility APIs still leak MVVM-shaped runtime objects

The safest Phase 14 strategy is therefore adapter-before-normalization:

1. make `GraphEditorViewModel` consume a kernel as its canonical state source
2. turn the compatibility façade into projection + UI/service adapter logic on top of that kernel
3. close the remaining read-only/snapshot gaps that still let host code depend on live mutable runtime state

## Why this split

- It satisfies `KERN-03` directly without dragging full capability/menu descriptor redesign into the same phase.
- It keeps the migration story intact: `GraphEditorViewModel` survives, but its role changes from owner to adapter.
- It makes `CAP-03` tractable because there is one canonical state graph to snapshot from instead of two drifting implementations.

## Primary technical risks

- projection drift if VM collections and inspector state are not synchronized from kernel events consistently
- command regressions if kernel-routed mutations change status text, history baselines, or event ordering seen by existing hosts
- partial decoupling that removes direct VM ownership but still leaves VM-only query types as the effective public contract
- over-scoping into Phase 15 by attempting full command/menu/capability descriptor redesign too early

## Recommended planning shape

- `14-01`: make `GraphEditorViewModel` kernel-backed and move projection refresh into an adapter sync layer
- `14-02`: route compatibility/session behavior through the shared kernel-backed path and close the main read-only snapshot leaks
- `14-03`: prove parity across legacy/factory/session paths and update sample/smoke guidance so the kernel-first route is clearly canonical
