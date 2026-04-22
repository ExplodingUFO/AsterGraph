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
