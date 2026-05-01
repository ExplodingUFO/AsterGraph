# Phase 471 Summary: v0.78 Release Proof And API Governance

## Outcome

Phase 471 closed v0.78 release proof and API governance. The release-facing public API baseline is aligned, documentation/examples were scanned for unsupported claims, and the .NET 8/9/10 CI-sensitive release path was verified with special focus on .NET 10.

## Beads

- Parent: `avalonia-node-map-v78.6`
- Completed children:
  - `avalonia-node-map-v78.6.1` — public API inventory and baseline classification
  - `avalonia-node-map-v78.6.2` — docs and examples release gate
  - `avalonia-node-map-v78.6.3` — CI-sensitive .NET 8/9/10 release proof
  - `avalonia-node-map-v78.6.4` — milestone synthesis and handoff

## Proof Tracks

### Public API

- Artifact: `471.1-API-INVENTORY.md`
- Baseline: `eng/public-api-baseline.txt`
- Public docs: `docs/en/public-api-inventory.md`, `docs/zh-CN/public-api-inventory.md`
- Classified drift: `ViewportVisibleSceneProjection.ToInvalidationBudgetMarker(...)`
- Verification: `.\eng\validate-public-api-surface.ps1 -Configuration Release -Framework net9.0` passed with `PUBLIC_API_SURFACE_OK:4122:net9.0`

### Docs And Examples

- Artifact: `471.2-DOCS-EXAMPLES-GATE.md`
- Scope: release-facing docs, examples, templates, cookbook text, and source-backed support boundaries
- Verification: external inspiration-name scan is clean except the existing guard test; unsupported positive generated-runnable-Demo-code/adapter/runtime claim scan passed

### CI And Framework Proof

- Artifact: `471.3-CI-NET8-NET9-NET10-PROOF.md`
- CI inventory: workflows cover `net8.0`, `net9.0`, and `net10.0`
- Local verification:
  - `dotnet build src\AsterGraph.Avalonia\AsterGraph.Avalonia.csproj -c Release -m:1 --nologo` passed for net8.0, net9.0, and net10.0
  - `dotnet test tests\AsterGraph.Demo.Tests\AsterGraph.Demo.Tests.csproj -c Release --filter FullyQualifiedName~CrossPlatformHostPackagingCiTests -m:1 --logger "console;verbosity=minimal" --nologo` passed
  - `.\eng\ci.ps1 -Lane all -Framework net10.0 -Configuration Release` passed on master

## Release Boundary

Phase 471 did not add fallback layers, compatibility shims, generated runnable Demo code, a second runtime model, marketplace behavior, sandboxing, external adapter expansion, or public docs that name external inspiration projects. The Demo remains a sample/proof surface; supported contracts remain in package APIs and source-backed docs.

## Result

`REL-02` is satisfied. v0.78 can be closed with public API inventory, docs/examples release proof, .NET 8/9/10 framework proof, and clean beads/Dolt/Git handoff.
