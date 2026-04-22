# Serialization Contracts

This is the pre-1.0 serialization contract. It is explicit and intentionally narrow.

## Workspace document persistence

- Canonical save/load flow uses `IGraphWorkspaceService` and `GraphDocumentSerializer`.
- Canonical write envelope for workspace documents is:
  - `SchemaVersion`
  - `Title`
  - `Description`
  - `RootGraphId`
  - `GraphScopes`
- Current workspace/document schema version is `5`.
- Read behavior accepts:
  - unversioned legacy payloads (best-effort compatibility path), and
  - `SchemaVersion` values from `1` through `5`.
- Read behavior rejects unknown/future schema versions.

## Fragment and clipboard payloads

- Clipboard and fragment payload versioning is separate from workspace-document versioning.
- Clipboard/payload compatibility rules are defined by their respective fragment/clipboard contracts and are not tied to the workspace `SchemaVersion`.

