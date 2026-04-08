# Phase 21: In-Context Proof And Narrative Polish - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in `21-CONTEXT.md` — this log preserves the alternatives considered.

**Date:** 2026-04-08
**Phase:** 21-in-context-proof-and-narrative-polish
**Areas discussed:** proof placement, ownership-cue style, demo-doc alignment

---

## Proof placement

| Option | Description | Selected |
|--------|-------------|----------|
| Intro strip + targeted drawer sections | Keep proof embedded near the graph and active menu group using compact badges plus dense drawer sections | X |
| Dedicated bottom proof rail | Add a new horizontal proof surface below the graph | |
| Modal walkthrough | Explain seam ownership through popups or guided overlays | |

**User's choice:** Auto-selected recommended option because the user asked to continue planning directly.
**Notes:** This preserves the graph-first shell and avoids rebuilding the layout again after Phases 19 and 20.

---

## Ownership-cue style

| Option | Description | Selected |
|--------|-------------|----------|
| Short labels + live values | Show host-owned seams and shared runtime state through compact labels, rows, and badges | X |
| Paragraph proof cards | Keep broad prose summaries inside bordered cards | |
| Visual-only icon cues | Rely mostly on icons and color without explicit ownership text | |

**User's choice:** Auto-selected recommended option because `PROOF-01` requires explicit seam ownership cues.
**Notes:** The phase should reduce prose, not hide meaning behind icons alone.

---

## Demo-documentation alignment scope

| Option | Description | Selected |
|--------|-------------|----------|
| README-first alignment | Refresh the main repository narrative so the demo story matches the shipped graph-first UI | X |
| Broad multi-doc sweep | Update README, quick start, host integration docs, and supporting guides together | |
| UI-only polish | Leave repo docs untouched and rely entirely on the demo window for the new story | |

**User's choice:** Auto-selected recommended option because the roadmap only requires demo-facing docs or proof surfaces to align, and README is the minimum high-signal surface.
**Notes:** A broader doc sweep can be deferred unless execution uncovers clear contradictions.

---

## the agent's Discretion

- Exact section names for proof, configuration, and runtime signal groups.
- Whether the active host group is shown as a chip, badge, or compact title row.
- Which final README subsection title best fits the existing document structure.

## Deferred Ideas

- Guided “capability tour” copy or narrative sequencing
- Screenshot-heavy documentation or media assets
- Additional docs beyond the README unless the execution phase finds a real mismatch
