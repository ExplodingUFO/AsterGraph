# Node Presentation Guidelines

This guide captures recommendation-level host guidance for `NodePresentationState`.

These rules standardize editor-facing presentation patterns without forcing host-specific business semantics into AsterGraph itself.

## Badge Count

Recommended visible badge count:

- keep most nodes at `0-3` badges
- if a node has more than `3` statuses, aggregate before sending the snapshot to the editor

Why this matters:

- dense badge stacks reduce scanability
- too many simultaneous badges make neighboring nodes harder to compare

Recommended aggregation patterns:

- keep the highest-priority runtime state visible
- collapse secondary statuses into a summary badge such as `+2`
- move overflow detail into tooltip text

## Badge Text Length

Recommended badge text length:

- about `2-8` visible characters
- prefer short operational words such as `Ready`, `Dirty`, `Error`, `Host`

Avoid:

- sentence-length badges
- long business nouns already visible elsewhere
- multiple badges that differ only by tiny wording changes

If a state needs a longer explanation, keep the badge short and move the detail into `ToolTip`.

## Status Bar Length

Recommended status-bar text length:

- about `8-24` visible characters
- long enough for a short operational summary

Good examples:

- `Preview available`
- `Waiting for input`
- `Last run failed`

Avoid:

- paragraph-length explanations
- stacking multiple unrelated states into one bar
- using the status bar as a substitute for an inspector or detail pane

## Tooltip Guidance

Tooltip text should add information rather than restate what is already visible.

Recommended structure:

1. what the state means
2. why it matters, if that is not obvious
3. what the user should do next, when useful

Good examples:

- `Host reports the node is ready for preview.`
- `Execution failed on the last run. Open details in the host inspector.`
- `Two additional host statuses were aggregated into this summary badge.`

Avoid:

- copying badge text verbatim
- dumping raw logs or large error payloads into tooltips
- leaking host-only internal jargon into shared UI unless that vocabulary is already user-facing

## Color And Emphasis

Use color to express priority, not decoration.

Recommended rules:

- keep one dominant emphasis color per node state snapshot
- map accent color consistently across related badge and status-bar meaning
- reserve high-saturation warning or error colors for true attention states

Avoid:

- giving every badge a different saturated color
- mixing multiple unrelated warning palettes on the same node
- relying on color alone when short text can disambiguate cheaply

The exact palette remains host-owned.

## Host And Editor Responsibilities

Hosts should own:

- business meaning of runtime states
- badge and status-bar text selection
- tooltip detail
- palette mapping from business state to accent color

AsterGraph should own:

- rendering the supplied snapshot
- layout behavior of badges and status bars
- keeping editor contracts framework-neutral

That boundary matters because the host decides what a state means while the editor only decides how the supplied snapshot is displayed.

## Practical Defaults

If a host has no stronger design system yet, start with:

- `0-2` badges for most nodes
- one short status bar only when the node has an actionable runtime state
- tooltip text only when it adds meaning beyond the visible label
- one primary accent per snapshot

That default is usually enough to keep graphs readable while still exposing runtime context.
