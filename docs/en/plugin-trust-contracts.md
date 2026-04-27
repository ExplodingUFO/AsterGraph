# Plugin Manifest and Trust Policy Contract v1

This page publishes the thin v1 contract for plugin manifest metadata, provenance and signature evidence, and host-owned trust policy.

## Manifest Fields

The host can treat these manifest fields as the public v1 surface:

- `Id`
- `DisplayName`
- `Description`
- `Version`
- `Compatibility`
- `CapabilitySummary`
- `Provenance`

`Provenance` carries the source and distribution clues the host can inspect before activation:

- `SourceKind`
- `Source`
- `Publisher`
- `PackageId`
- `PackageVersion`

`Compatibility` is host-facing metadata only. It describes the plugin's declared version and runtime fit; it does not grant trust.

## Provenance And Signature Evidence

Trust decisions can use provenance and signature evidence together:

- `GraphEditorPluginProvenanceEvidence.PackageIdentity`
- `GraphEditorPluginProvenanceEvidence.Signature`
- `GraphEditorPluginSignatureEvidence.Status`
- `GraphEditorPluginSignatureEvidence.Kind`
- `GraphEditorPluginSignatureEvidence.Signer`
- `GraphEditorPluginSignatureEvidence.TimestampUtc`
- `GraphEditorPluginSignatureEvidence.TimestampAuthority`
- `GraphEditorPluginSignatureEvidence.ReasonCode`
- `GraphEditorPluginSignatureEvidence.ReasonMessage`

The host should use this evidence as input to policy, not as an automatic allow signal.

## Trust Policy Ownership

Trust policy is host-owned.

- `IGraphEditorPluginTrustPolicy` is the host decision point
- `GraphEditorPluginTrustPolicyContext` exposes the manifest, provenance evidence, and package path
- the plugin itself cannot authorize loading
- plugin trust decisions are evaluated before any contribution code is allowed to execute

If the host does not configure a policy, the runtime uses `GraphEditorPluginTrustEvaluation.ImplicitAllow()`.

## Trust Review Short Path

Use this path when evaluating a trusted in-process plugin:

1. Author or generate the plugin with [Plugin And Custom Node Recipe](./plugin-recipe.md).
2. Validate the `.dll`, `.nupkg`, or plugin directory with `AsterGraph.PluginTool validate`.
3. Inspect structured local evidence with `AsterGraph.PluginTool inspect <path> --host-version <version> --json` when you need manifest, host compatibility, node definition, and parameter metadata details.
4. Generate a standalone SHA-256 evidence line with `AsterGraph.PluginTool hash <path>` for allowlist review.
5. Review the manifest, compatibility, provenance, signature evidence, node definitions, parameter metadata, and SHA-256 hash in the PluginTool output.
6. Apply a host-owned `IGraphEditorPluginTrustPolicy` before activation.
7. Use [Consumer Sample](./consumer-sample.md) as the defended hosted trust hop when validating a real host flow.

PluginTool validation is evidence for host policy. Treat `PLUGIN_COMPATIBILITY_OK`, `PLUGIN_MANIFEST_OK`, `PLUGIN_NODE_DEFINITIONS_OK`, `PLUGIN_PARAMETER_METADATA_OK`, and `PLUGIN_TRUST_EVIDENCE_OK` as local review markers. It is not a marketplace approval, a sandbox decision, or an automatic load authorization.

## Host Policy Examples

Use these patterns as host-owned policy examples, not runtime fallback modes:

| Pattern | Typical use | Policy input |
| --- | --- | --- |
| Allow all local dev | Inner-loop development on a known machine. | Fixed local plugin directory plus an explicit local-dev reason string. |
| Allow by hash | Small teams sharing known plugin binaries. | PluginTool SHA-256 hash must match the host allowlist. |
| Allow by manifest or publisher | Organization-published plugins. | Manifest id, package id/version, publisher metadata, and signature evidence must match host policy. |
| Block unknown source | Default prerelease or enterprise posture. | Block candidates without an allowlist, hash, or accepted signature match before activation. |
| Enterprise fixed plugin directory | Managed desktop deployments. | Discover only from an admin-controlled directory and keep allowlist import/export records for audit. |

## Implicit Allow Contract

The explicit implicit-allow contract is narrow:

- no host policy configured means the runtime may return an implicit allow result
- an implicit allow still depends on manifest and provenance visibility
- implicit allow is a host/runtime default, not a plugin capability
- a host policy can replace the default with explicit allow or block decisions

## Blocked Before Activation

Blocked plugins do not activate.

- trust refusal happens before plugin contribution code runs
- signature refusal also happens before activation
- package staging is a pre-activation gate, not a load guarantee
- `GraphEditorPluginStageOutcome.Refused` means the candidate was blocked before activation
- staged or cached package material can exist without the plugin being activated

## Non-Goals

This v1 contract does not include:

- a plugin marketplace
- remote install or update flows
- plugin unload lifecycle management
- sandboxing or isolation guarantees
- untrusted code execution support

## Related Docs

- [Host Integration](./host-integration.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Consumer Sample](./consumer-sample.md)
