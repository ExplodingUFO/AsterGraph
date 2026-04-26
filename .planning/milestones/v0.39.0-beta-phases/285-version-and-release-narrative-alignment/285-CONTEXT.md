# Phase 285: Version And Release Narrative Alignment - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning

<domain>
## Phase Boundary

This phase removes public version ambiguity across README, versioning docs, release workflow output, and issue/release links while preserving local planning labels as private bookkeeping. It must not change package versions, runtime APIs, adapter support, performance budgets, or plugin trust guarantees.

</domain>

<decisions>
## Implementation Decisions

### Version Source Of Truth
- Use `Directory.Build.props` as the only machine-readable package version source.
- Public tag expectations derive mechanically as `v{Version}` unless a release workflow supplies an explicit tag.
- README and versioning docs must carry the same package version/tag pair, not a local milestone label.
- GitHub Releases/Prereleases are public packaging artifacts; `.planning` milestone labels are maintainer scheduling artifacts.

### Release Gate
- Add a small validation script instead of embedding more ad hoc PowerShell into workflow YAML.
- Wire the script into the release lane and GitHub prerelease workflow so public docs fail release validation when version/tag wording drifts.
- Keep validation direct: check required files and required exact strings; do not add a fallback compatibility mode.

### Scope Guardrails
- Keep 1000-node as defended and 5000-node stress as informational-only.
- Keep plugin loading described as trusted in-process extension; do not imply sandboxing or untrusted-code isolation.
- Leave public API analyzers, templates, and performance-budget promotion to later phases.

### the agent's Discretion
- The exact script output marker and test method names are implementation details as long as failures are clear and tests are focused.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `eng/write-prerelease-notes.ps1` already reads `Directory.Build.props` and generates release-note headers.
- `eng/ci.ps1` already validates prerelease notes and release proof markers in the release lane.
- `tests/AsterGraph.Demo.Tests/ReleaseClosureContractTests.cs` already has PowerShell script execution helpers.
- `tests/AsterGraph.Demo.Tests/DemoProofReleaseSurfaceTests.cs` already defends public versioning docs and README vocabulary.

### Established Patterns
- Release/documentation proof tests use exact string assertions against repo files.
- PowerShell validation scripts fail by throwing and are called from `eng/ci.ps1`.
- Public docs are kept bilingual for README and versioning pages.

### Integration Points
- `README.md`, `README.zh-CN.md`, `docs/en/versioning.md`, and `docs/zh-CN/versioning.md`.
- `.github/workflows/release.yml`, `.github/ISSUE_TEMPLATE/config.yml`, and public launch checklists.
- `eng/ci.ps1` release lane and `tests/AsterGraph.Demo.Tests`.

</code_context>

<specifics>
## Specific Ideas

Keep the implementation surgical: script, workflow hook, small docs wording, and focused tests.

</specifics>

<deferred>
## Deferred Ideas

- Public API analyzer and API baseline/diff gate.
- `dotnet new` templates.
- 5000-node defended performance budget promotion.
- Plugin template, manifest validator CLI, marketplace, unload lifecycle, or sandboxing.

</deferred>
