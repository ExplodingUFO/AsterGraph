# Node Presentation Guidelines

This guide defines recommended host-side usage for `NodePresentationState`.

These guidelines are intentionally recommendation-level. They standardize editor-facing presentation patterns without forcing host business semantics into AsterGraph itself.

## Badge Count

Recommended top-right badge count:

- `0-3` visible badges per node
- if a node has more than `3` statuses, aggregate them before sending the snapshot to the editor

Why:

- more than `3` badges quickly reduces scanability
- dense badge stacks make node cards harder to compare at a glance

Recommended aggregation patterns:

- keep the highest-priority runtime state visible
- collapse secondary statuses into one summary badge such as `+2`
- move overflow detail into tooltip text

## Badge Text Length

Recommended badge text length:

- `2-8` visible characters
- prefer compact operational words such as `Ready`, `Dirty`, `Error`, `Host`

Avoid:

- sentence-like badges
- repeating long business nouns already visible elsewhere
- multiple badges that differ only by tiny wording changes

If a status needs a longer explanation, keep the badge short and put the detail in `ToolTip`.

## Status Bar Length

Recommended status-bar text length:

- about `8-24` visible characters
- enough for a short operational summary

Good examples:

- `Preview available`
- `Waiting for input`
- `Last run failed`

Avoid:

- paragraph-length explanations
- embedding multiple unrelated states into one bar
- using the status bar as a substitute for a property inspector

If the message becomes too long, shorten the visible text and move the rest into tooltip text.

## Tooltip Guidance

Tooltip text should add information, not restate the visible label.

Recommended tooltip structure:

1. what the state means
2. why it matters, if that is not obvious
3. what the user should do next, when useful

Good examples:

- `Host reports the node is ready for preview.`
- `Execution failed on the last run. Open details in the host inspector.`
- `Two additional host statuses were aggregated into this summary badge.`

Avoid:

- copying badge text verbatim without extra detail
- dumping raw logs or large error payloads into tooltips
- putting host-only internal jargon into shared visual surfaces unless the host UI already uses that vocabulary consistently

## Color And Emphasis

Use color to express priority, not decoration.

Recommended rules:

- keep one dominant emphasis color per node state snapshot
- use accent color consistently across related badge and status-bar meaning
- reserve high-saturation warning/error colors for true attention states

Avoid:

- giving every badge a different saturated color
- mixing multiple unrelated warning colors on the same node
- using color alone when short text can disambiguate the state cheaply

Hosts should keep their own semantic palette mapping stable, for example:

- ready / success
- running / active
- warning / stale
- error / blocked

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

This boundary is important:

- the host decides what a state means
- the editor only decides how the supplied snapshot is displayed

## Practical Defaults

If a host has no stronger design system yet, start with:

- `0-2` badges for most nodes
- one short status bar only when the node has an actionable runtime state
- tooltip text only when it adds meaning beyond the visible label
- one primary accent per snapshot

That default is usually enough to keep graphs readable while still exposing runtime context.
