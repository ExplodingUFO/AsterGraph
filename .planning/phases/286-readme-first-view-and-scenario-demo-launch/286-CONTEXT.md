# Phase 286: README First View And Scenario Demo Launch - Context

**Gathered:** 2026-04-26
**Status:** Ready for planning
**Mode:** Autonomous context from roadmap and repository inspection

<domain>
## Phase Boundary

Phase 286 is limited to the public first impression and an explicit launch path for a prebuilt Demo scenario. It does not own the guided tour, broader scenario capability story, ConsumerSample onboarding path, or host builder facade work reserved for later phases.

</domain>

<decisions>
## Implementation Decisions

- Use `ai-pipeline` as the first prebuilt scenario because it gives the README a concrete SDK story without changing the runtime architecture.
- Keep the scenario entry inside `AsterGraph.Demo`; do not add a new public SDK compatibility layer or fallback route.
- Unknown scenario names should fail directly instead of silently opening the default graph.
- Use a repository-owned visual asset in `docs/assets` so the README first viewport is deterministic and testable without a GUI screenshot harness.

</decisions>

<code_context>
## Existing Code Insights

- `Program.Main` already has a `--proof` path and otherwise starts the Avalonia desktop lifetime.
- `App.OnFrameworkInitializationCompleted` creates `MainWindowViewModel` with `MainWindowShellOptions.CreatePersistentDefault()`.
- `MainWindowViewModel` creates the initial document with `DemoGraphFactory.CreateDefault(catalog)`.
- `MainWindowShellOptions` controls persistence and last-workspace restore, so a scenario launch can disable restore and force the prebuilt graph.
- Existing release-surface tests already assert README and demo proof contracts, making them a good place to defend the first-view launch path.

</code_context>

<specifics>
## Specific Ideas

- Add a small startup parser for `--scenario ai-pipeline` and `--scenario=ai-pipeline`.
- Add a scenario-aware graph factory method that creates an AI pipeline graph from demo node definitions.
- Add only the node definitions needed by the new scenario.
- Add README and README.zh-CN first-viewport content with the visual asset and launch command.
- Add tests for parser behavior, graph shape, and README proof markers.

</specifics>

<deferred>
## Deferred Ideas

- Guided tour panel and step progression.
- More scenario choices such as ETL, behavior trees, and material graphs.
- Screenshot/GIF generation automation.
- ConsumerSample scenario path.

</deferred>
