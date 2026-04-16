---
phase: 37-maintainability-and-extension-contract-hardening
plan: 02
subsystem: docs
tags: [docs, extensions, precedence, plugins]
requires: []
provides:
  - published host-vs-plugin precedence rules
  - code-backed extension layering contract
affects: [extension-contracts, host-docs, editor-readme]
tech-stack:
  added: []
  patterns: [code-backed-precedence-rules]
key-files:
  created:
    - docs/extension-contracts.md
  modified:
    - docs/host-integration.md
    - src/AsterGraph.Editor/README.md
key-decisions:
  - "Write extension precedence from the implementation and tests, not from assumed product intent."
patterns-established:
  - "Host trust is pre-activation; host localization/presentation run last; retained menu augmentor owns the final override point after runtime/plugin composition."
requirements-completed: [EXT-02]
duration: working session
completed: 2026-04-16
---

# Phase 37 Plan 02: Extension Precedence Summary

**Published the actual host-vs-plugin precedence rules so extension behavior no longer has to be reconstructed from composition code.**

## Accomplishments

- Documented plugin trust as a host-owned pre-activation gate.
- Documented localization precedence as:
  - plugin providers first
  - host provider last
- Documented node-presentation precedence as:
  - plugin states merge first
  - host provider runs last for final override fields
  - badges continue to accumulate
- Documented context-menu layering as:
  - runtime session: stock descriptors then plugin augmentors
  - retained facade: host augmentor gets the final override point after runtime/plugin composition

## Task Commits

1. `4965bee` - `docs(37): publish extension and lane contracts`

## Self-Check: PASSED

- `rg -n "plugin trust is host-owned|host override fields|final override point" docs src/AsterGraph.Editor/README.md`

---
*Phase: 37-maintainability-and-extension-contract-hardening*
*Completed: 2026-04-16*
