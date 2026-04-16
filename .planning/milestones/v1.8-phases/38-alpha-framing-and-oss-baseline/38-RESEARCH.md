---
phase: 38
title: Alpha Framing And OSS Baseline
status: completed
last_updated: 2026-04-16
---

# Phase 38 Research

## Goal

Unify the repo's public alpha framing and add the open-source collaboration baseline needed before public evaluation.

## Findings

1. Public versioning was still on `0.1.0-preview.8`, which no longer matched the repo's external-alpha posture.
2. Consumer-facing entry points were split between `README`, `docs`, and `.planning`, with no explicit alpha-status contract.
3. The repo was still missing public collaboration files:
   - `CONTRIBUTING.md`
   - `CODE_OF_CONDUCT.md`
   - `SECURITY.md`
   - issue templates
   - PR template
   - `global.json`
4. Existing CI and release lanes were already strong enough to validate the framing change; the main risk was narrative drift, not missing infrastructure.

## Execution Direction

- Move package metadata to an explicit alpha version.
- Publish one alpha-status/known-limitations doc and link to it from the primary docs surfaces.
- Keep `README` + `docs/` as the first consumer path and demote `.planning` to maintainer context.
- Add the missing OSS collaboration and SDK pinning files without inventing a new workflow.

## Verification Strategy

- Confirm the SDK resolves under `global.json`.
- Confirm the project version resolves to `0.2.0-alpha.1`.
- Run the existing repo gate: `eng/ci.ps1 -Lane all -Framework all -Configuration Release`.
