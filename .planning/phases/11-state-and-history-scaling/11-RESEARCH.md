# Phase 11: State And History Scaling - Research

## Summary

The remaining scaling pressure is less about drawing and more about repeated read-model rebuilding:

- inspector text and topology summaries rescan connections
- computed property invalidation fans out broadly
- history and dirty tracking still lean on whole-document snapshots/signatures

The safest Phase 11 strategy is to cache or incrementally maintain derived state where it already has a clear owner, and to avoid broad invalidation when only one concern changed.
