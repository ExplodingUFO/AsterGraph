# Phase 1: Consumption & Compatibility Guardrails - Research

**Researched:** 2026-03-25
**Domain:** .NET/Avalonia package consumption, public initialization seams, and staged compatibility for a host-facing component library
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Treat `AsterGraph.Editor` as a standard host-facing entry package for advanced or primary host integration scenarios, not only as an optional/internal-facing dependency. The package story should move beyond "Avalonia + Abstractions only" and explicitly document when `Editor` is part of the intended public consumption path.
- **D-02:** Keep `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia` as the publishable package set for this phase. `AsterGraph.Demo` remains non-consumable.
- **D-03:** Add formal public initialization entry points in Phase 1, such as `AddAsterGraphEditor(...)`, `AddAsterGraphAvalonia(...)`, options objects, factories, or equivalent public registration/construction helpers.
- **D-04:** Do not remove the current `new GraphEditorViewModel(...)` plus `GraphEditorView` path in Phase 1. Keep that path supported as a compatibility route while the new initialization surface is introduced and documented.
- **D-05:** `GraphEditorViewModel` and `GraphEditorView` remain the compatibility facade in this phase. Prefer additive public APIs, migration shims, and deprecation guidance over immediate hard breaks.
- **D-06:** Phase 1 must produce an explicit staged migration story for existing hosts. The goal is "planned migration with guardrails," not "one-shot rewrite."

### Claude's Discretion
- Exact naming and placement of the new public registration helpers between `AsterGraph.Editor` and `AsterGraph.Avalonia`
- Whether the public entry surface is pure DI registration, factory-based, or a hybrid
- Exact obsolete/deprecation mechanics and migration-note format, as long as the staged compatibility strategy is preserved

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| PKG-01 | Host can consume published AsterGraph packages on supported target frameworks with a documented package boundary and version/support story | Standard package set, support matrix, package-boundary documentation rules, package-smoke validation ring |
| PKG-02 | Host can initialize editor runtime and Avalonia components through documented registration or construction APIs instead of demo-only wiring patterns | Hybrid initialization recommendation: canonical factory/options API plus optional thin DI helpers, constructor path retained |
| PKG-03 | Existing hosts can migrate to the reorganized API surface through a staged compatibility path rather than a single breaking rewrite | Additive API-first rollout, `ObsoleteAttribute` guidance, migration docs, compatibility-smoke and facade regression tests |
</phase_requirements>

## Project Constraints (from CLAUDE.md)

- Keep the solution centered on .NET, C#, and Avalonia.
- Reorganize APIs in phased, deliberate steps rather than one uncontrolled break.
- Keep the result publishable as a general-purpose component library; public API quality matters.
- Preserve existing validated editing capabilities during the transition; this is refactor/SDK hardening, not a rewrite.
- Design package and API seams so hosts can replace or embed subcomponents independently over time.
- Treat debuggability as a product concern with explicit public seams.

## Summary

Phase 1 should not behave like an architectural rewrite. The repo already has the four-pack publishable boundary, multi-targeted `net8.0;net9.0` libraries, a host sample, a packed-package smoke project, and public constructor-based entry points through `GraphEditorViewModel` and `GraphEditorView`. The planning focus is therefore to formalize one supported consumption story, add public initialization helpers without invalidating existing hosts, and make the migration path mechanically testable.

The strongest implementation shape is a hybrid surface with one canonical composition source. Define host-facing option records plus a public factory/composer in `AsterGraph.Editor` and `AsterGraph.Avalonia`; then, if Phase 1 includes `AddAsterGraphEditor(...)` and `AddAsterGraphAvalonia(...)`, make them thin wrappers over that same factory instead of a second construction path. This satisfies D-03 and D-04 simultaneously: new helpers become the documented path, while direct construction remains supported and verifiable.

Current validation is blocked before any Phase 1 change lands. On 2026-03-25, `dotnet test`, `dotnet run tools/AsterGraph.PackageSmoke`, and `dotnet run tools/AsterGraph.HostSample` all fail with `CS0579` duplicate assembly attributes because generated C# files under `src/AsterGraph.Abstractions/artifacts/audit/*_obj/**` are being compiled alongside `obj/**`. The planner should treat exclusion or cleanup of generated artifact trees as a Wave 0 prerequisite for any package-consumption verification loop.

**Primary recommendation:** Use a single AsterGraph-owned factory/options composition model as the canonical public entry API, keep `GraphEditorViewModel` plus `GraphEditorView` as the compatibility facade, and validate every migration step through package-smoke plus facade regression tests.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `AsterGraph.Abstractions` | `0.1.0-preview.7` | Stable contracts, identifiers, styling tokens | Locked publishable package; minimal-dependency host contract |
| `AsterGraph.Core` | `0.1.0-preview.7` | Graph models, serialization, compatibility | Locked publishable package for model/serialization consumers |
| `AsterGraph.Editor` | `0.1.0-preview.7` | Host-facing runtime, commands, events, factory/options home | D-01 makes this a standard host package for advanced integration |
| `AsterGraph.Avalonia` | `0.1.0-preview.7` | Default Avalonia view shell and adapters | Locked publishable UI package and current compatibility facade |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.5` stable on NuGet as of 2026-03-13 | Optional thin `IServiceCollection` adapters | Only if Phase 1 ships `AddAsterGraphEditor(...)` / `AddAsterGraphAvalonia(...)` |
| `Microsoft.Extensions.Options` | `10.0.5` stable on NuGet as of 2026-03-13 | Typed options registration via `Configure<TOptions>` | Only if DI helpers need option binding/configuration |
| `Avalonia` | repo pin `11.3.10` | Existing UI dependency in `AsterGraph.Avalonia` | Stay on repo pin in Phase 1 unless a blocker forces upgrade; latest stable verified is `11.3.12` from 2026-02-12 |
| `CommunityToolkit.Mvvm` | repo pin `8.2.1` | Existing editor/view-model foundation | Stay on repo pin in Phase 1 unless the new public API surface requires an upgrade; latest stable verified is `8.4.1` from 2026-03-19 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Canonical factory/options plus optional DI wrappers | DI-only registration surface | Conflicts with D-04 because non-DI hosts would have no first-class documented path |
| Keep current package IDs and assembly identities | Rename packages or introduce a new meta-package in Phase 1 | Creates avoidable binary/package migration pressure before the staged path exists |
| `ObsoleteAttribute` plus migration docs | README-only migration guidance | Compiler and IDE warnings are lost; migration becomes easy to miss |

**Installation:**
```bash
dotnet add src/AsterGraph.Editor/AsterGraph.Editor.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.5
dotnet add src/AsterGraph.Editor/AsterGraph.Editor.csproj package Microsoft.Extensions.Options --version 10.0.5
dotnet add src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.5
```

**Version verification:** Internal package version `0.1.0-preview.7` is defined centrally in `Directory.Build.props`. Current third-party versions and publish dates were verified against NuGet's official registration data on 2026-03-25: `Avalonia 11.3.12` (2026-02-12), `CommunityToolkit.Mvvm 8.4.1` (2026-03-19), `Microsoft.Extensions.DependencyInjection 10.0.5` (2026-03-13), `Microsoft.Extensions.Options 10.0.5` (2026-03-13), `Microsoft.NET.Test.Sdk 18.3.0` (2026-02-24), and `xunit 2.9.3` (2025-01-09). Recommendation: do not mix dependency upgrades into Phase 1 unless required, because this phase is about package/init guardrails, not stack churn.

## Architecture Patterns

### Recommended Project Structure
```text
src/
├── AsterGraph.Abstractions/   # stable contracts and tokens
├── AsterGraph.Core/           # immutable models and serialization
├── AsterGraph.Editor/         # canonical runtime composition, options, factory, compatibility facade
└── AsterGraph.Avalonia/       # default view shell and Avalonia-specific adapters
tests/
├── AsterGraph.Editor.Tests/   # facade, host, and new initialization regression tests
└── AsterGraph.Serialization.Tests/  # document/clipboard compatibility tests
tools/
├── AsterGraph.HostSample/     # real host composition sample
└── AsterGraph.PackageSmoke/   # package-consumption smoke ring
```

### Pattern 1: One Composition Root, Many Entry Shapes
**What:** Define one public factory/composer per runtime layer, and have every supported entry path call into it.
**When to use:** Always. This is the core guardrail that keeps constructor, factory, and optional DI registration behavior identical.
**Example:**
```csharp
// Source: repo pattern from tools/AsterGraph.HostSample/Program.cs and src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = new DefaultPortCompatibilityService(),
    StyleOptions = style,
    BehaviorOptions = behavior,
    ContextMenuAugmentor = menuAugmentor,
    NodePresentationProvider = presentationProvider,
    LocalizationProvider = localizationProvider,
});

var view = new GraphEditorView
{
    Editor = editor,
};
```

### Pattern 2: Factory-First, DI-Second
**What:** The canonical public API is an AsterGraph-owned options/factory model. DI helpers are convenience wrappers, not the source of truth.
**When to use:** When satisfying PKG-02 without forcing every host into `IServiceCollection`.
**Example:**
```csharp
// Source: Microsoft Learn - OptionsServiceCollectionExtensions.Configure
services.Configure<AsterGraphEditorOptions>(options =>
{
    options.NodeCatalog = catalog;
    options.CompatibilityService = new DefaultPortCompatibilityService();
});
```

### Pattern 3: Staged Compatibility via Additive APIs
**What:** Keep `GraphEditorViewModel` and `GraphEditorView` working while introducing new helpers. Mark superseded setup entry points with migration guidance only after the replacement exists.
**When to use:** For PKG-03 and every public API move in this phase.
**Example:**
```csharp
// Source: Microsoft Learn - Breaking changes and .NET libraries
[Obsolete("Use AsterGraphEditorFactory.Create(...) or AddAsterGraphEditor(...) instead.", error: false)]
public static GraphEditorViewModel CreateLegacy(...)
{
    ...
}
```

### Pattern 4: Documentation, Sample, and Smoke Must Match Exactly
**What:** Keep `README.md`, package READMEs, `docs/host-integration.md`, `tools/AsterGraph.HostSample`, and `tools/AsterGraph.PackageSmoke` on the same package/init story.
**When to use:** For PKG-01 and PKG-02. If one artifact says `AsterGraph.Editor` is optional while another treats it as standard, the boundary is not actually clear.

### Anti-Patterns to Avoid
- **Dual construction logic:** Do not let constructor defaults, factory defaults, and DI registration defaults diverge.
- **DI-only public story:** This would turn the current supported constructor path into undocumented legacy overnight.
- **Assembly/package renames in Phase 1:** Microsoft library guidance warns that assembly identity changes are binary breaking; keep identities stable here.
- **Demo-led integration docs:** `AsterGraph.Demo` is explicitly non-consumable for this phase, so do not let its bootstrap become the normative host API.
- **Using the `Microsoft.Extensions.DependencyInjection` namespace for AsterGraph extensions:** Microsoft guidance for library authors says non-Microsoft packages should use their own namespace.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Service registration | Custom mini-DI container or bespoke service locator | `IServiceCollection` extensions if DI helpers are included | Standard .NET hosts already understand this model; custom containers create host friction |
| Options binding | Ad-hoc mutable dictionaries or string-key config maps | Typed option records plus `Configure<TOptions>` when DI is used | Keeps defaults versionable and migration-safe |
| Deprecation signaling | README-only migration notes or custom warning text in runtime status messages | `ObsoleteAttribute` on superseded APIs and NuGet package deprecation only for package-level changes | Compiler/IDE feedback is immediate; runtime/status-message warnings are too late |
| Package-consumption validation | Manual copy-paste setup in downstream hosts | `dotnet pack` + `tools/AsterGraph.PackageSmoke` + `tools/AsterGraph.HostSample` | Restore/build failures surface the real package boundary problems |
| Support matrix | Informal version promises | explicit `TargetFrameworks`, package README support notes, and Microsoft lifecycle dates | Prevents vague "works on modern .NET" claims |

**Key insight:** package-boundary work fails mostly at restore/build/upgrade time, not at runtime. The right tools are package metadata, compile-time warnings, and smoke consumers, not custom runtime indirection.

## Runtime State Inventory

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | None for Phase 1 package/init migration. Persisted graph and clipboard schema compatibility already lives in `AsterGraph.Core` and `AsterGraph.Editor` compatibility helpers. | Code edit only; no data migration |
| Live service config | None identified. No external UI-only or service-side configuration was found for package consumption in this repo. | None |
| OS-registered state | None identified. No systemd/launchd/Task Scheduler/pm2 style registrations are part of the Phase 1 consumption story. | None |
| Secrets/env vars | None identified in tracked files. Package consumption does not currently depend on host-specific secret names. | None |
| Build artifacts | `src/AsterGraph.Abstractions/artifacts/audit/*_obj/**` contains generated `.cs` files that are currently compiled by default and cause `CS0579` duplicate assembly attributes during `dotnet test` and `dotnet run`. | Code/build edit: exclude or clean generated artifact trees before validation |

## Common Pitfalls

### Pitfall 1: Diverging Initialization Paths
**What goes wrong:** Hosts created through direct construction, a new factory, and optional DI helpers do not get the same defaults or service seams.
**Why it happens:** Initialization logic gets copied into each entry point.
**How to avoid:** Route every entry path through one composition root with one defaulting strategy.
**Warning signs:** Host sample, README snippets, and DI tests each need slightly different setup code.

### Pitfall 2: Premature Breakage of the Compatibility Facade
**What goes wrong:** Existing hosts are forced to rewrite before the new entry surface is stable.
**Why it happens:** Constructors or existing public properties are removed or aggressively marked obsolete before replacements are documented and tested.
**How to avoid:** Introduce additive APIs first; keep `GraphEditorViewModel` plus `GraphEditorView` supported throughout Phase 1.
**Warning signs:** Existing sample code requires warning suppression or major edits just to stay compiling.

### Pitfall 3: Documentation Drift Across Package Story Artifacts
**What goes wrong:** `README.md`, package READMEs, host guide, host sample, and smoke project disagree on whether `AsterGraph.Editor` is standard or optional.
**Why it happens:** Package-boundary decisions are changed in one place only.
**How to avoid:** Treat docs plus sample plus smoke as one atomic update set.
**Warning signs:** The smoke project references packages that the README does not recommend.

### Pitfall 4: Build Artifact Leakage Into Default Compile Globs
**What goes wrong:** Generated files under `src/**/artifacts/**` are compiled with the source tree and produce duplicate assembly metadata.
**Why it happens:** SDK-style projects include `**/*.cs` by default unless excluded.
**How to avoid:** Exclude generated artifact directories or keep generated outputs outside the source tree.
**Warning signs:** `CS0579` errors mention both `obj/...` and `artifacts/audit/...` generated files.

### Pitfall 5: Assembly or Package Identity Changes Too Early
**What goes wrong:** Existing binaries or downstream package references break even if the source-level migration looks small.
**Why it happens:** Assemblies or package IDs are renamed while trying to "clean up" the public surface in the same phase.
**How to avoid:** Keep the four package IDs and assembly identities stable for Phase 1; add new entry points in place.
**Warning signs:** Planner tasks start talking about moving `GraphEditorViewModel` into another package or renaming assemblies.

### Pitfall 6: Over-scoping Dependency Upgrades
**What goes wrong:** A package-boundary phase turns into an Avalonia/MVVM upgrade phase, increasing migration noise and invalidating samples/tests for unrelated reasons.
**Why it happens:** Latest-package drift is treated as mandatory cleanup.
**How to avoid:** Keep repo-pinned third-party versions unless a concrete blocker forces a bump.
**Warning signs:** Phase tasks include broad dependency churn with no direct tie to PKG-01/02/03.

## Code Examples

Verified patterns from official sources and current repo practice:

### Typed Options Registration
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.optionsservicecollectionextensions.configure
services.Configure<AsterGraphEditorOptions>(options =>
{
    options.NodeCatalog = catalog;
    options.CompatibilityService = new DefaultPortCompatibilityService();
});
```

### Constructor Compatibility Path That Must Stay Supported
```csharp
// Source: tools/AsterGraph.HostSample/Program.cs
var editor = new GraphEditorViewModel(
    document,
    catalog,
    new DefaultPortCompatibilityService(),
    styleOptions: style,
    behaviorOptions: behavior,
    contextMenuAugmentor: new HostSampleAugmentor(),
    nodePresentationProvider: new HostSamplePresentationProvider(),
    localizationProvider: new HostSampleLocalizationProvider());

var view = new GraphEditorView
{
    Editor = editor,
};
```

### Multi-targeted Library Project
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/standard/frameworks -->
<PropertyGroup>
  <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
</PropertyGroup>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Constructor-only host composition is the only real entry story | Public constructor path stays valid, but a canonical factory/options entry surface becomes the documented default | Recommended for Phase 1 planning on 2026-03-25 | Hosts gain a supported initialization seam without forcing immediate rewrites |
| Package boundary is described as "Avalonia + Abstractions" by default, with `Editor` framed as optional/deeper | `AsterGraph.Editor` is explicitly a standard host-facing package for advanced or primary integration scenarios | Locked by D-01 on 2026-03-25 | README, host guide, smoke project, and samples must be updated in lockstep |
| Migration guidance lives mostly in prose and historical plans | Compiler-visible obsolescence plus facade regression tests plus smoke consumers | Longstanding .NET library guidance; recommended for this phase | Upgrade path becomes testable instead of purely documentary |
| Validation assumes source tree is clean | Validation must account for generated artifacts leaking into compile globs | Observed on 2026-03-25 | Planner needs a Wave 0 unblock before any package-smoke claims are trustworthy |

**Deprecated/outdated:**
- Demo-only or constructor-only wiring as the sole "official" consumption story.
- Treating `AsterGraph.Editor` as effectively internal when current samples, tests, and host seams already expose it publicly.
- Using NuGet custom deprecation messages as the main migration channel; Microsoft documents that `dotnet.exe` does not display those custom messages.

## Open Questions

1. **Should Phase 1 include DI helpers, or should it ship only a factory/options API?**
   - What we know: D-03 allows either `AddAsterGraph...` helpers or equivalent public construction helpers; the repo currently has no `Microsoft.Extensions.*` dependency.
   - What's unclear: whether adding DI abstractions/options dependencies to `AsterGraph.Editor` and `AsterGraph.Avalonia` is acceptable in this milestone.
   - Recommendation: plan around a factory/options API as required work and treat DI wrappers as a thin, optional layer on top.

2. **Is the duplicate-assembly build failure a local residue problem or a repository-wide validation blocker?**
   - What we know: all attempted validation commands on 2026-03-25 failed because generated files in `src/AsterGraph.Abstractions/artifacts/audit/*_obj/**` are being compiled.
   - What's unclear: whether every contributor sees the same residue or whether it is machine-specific.
   - Recommendation: make exclusion/cleanup of generated artifact trees the first verification task in the phase plan so the answer becomes deterministic.

3. **How strongly should the support story prefer `net8.0` versus `net9.0`?**
   - What we know: the packages already multi-target both; Microsoft lists both .NET 8 and .NET 9 as in support on 2026-03-25, with published end dates of 2026-11-10.
   - What's unclear: whether the docs should market `net8.0` as the primary host baseline or treat both equally.
   - Recommendation: document both as supported, but frame `net8.0` as the safest baseline for downstream hosts because it is the repo's test target and common LTS choice.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| `.NET SDK` | build, pack, test, smoke, package-consumption verification | ✓ | `10.0.201` | — |
| `nuget.org` package source | restore and package metadata verification | ✓ | `https://api.nuget.org/v3/index.json` configured | local feed in `NuGet.config.sample` once packages are packed |
| Local package feed `artifacts/packages` | packed-package smoke path | ✗ | — | use project-reference mode in `tools/AsterGraph.PackageSmoke` until pack step |

**Missing dependencies with no fallback:**
- None.

**Missing dependencies with fallback:**
- `artifacts/packages` is not populated yet. `tools/AsterGraph.PackageSmoke` already supports project-reference fallback until the pack step runs.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | `xUnit 2.9.2` plus `Avalonia.Headless.XUnit 11.3.10` |
| Config file | none — test behavior is driven by project files and `[assembly: AvaloniaTestApplication(...)]` in `GraphEditorViewTests.cs` |
| Quick run command | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorViewTests|FullyQualifiedName~GraphEditorFacadeRefactorTests|FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphHostContextExtensionsTests" -v minimal` |
| Full suite command | `dotnet test avalonia-node-map.sln -v minimal` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| PKG-01 | Published packages restore/build through the documented package boundary on supported TFMs | smoke | `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true` | ✅ |
| PKG-02 | Hosts can initialize through documented public construction/registration helpers, not demo-only bootstrap code | unit + smoke | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~GraphEditorInitialization"` | ❌ Wave 0 |
| PKG-03 | Existing hosts can stay on the compatibility facade while adopting the new entry surface gradually | unit + smoke | `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter "FullyQualifiedName~EditorClipboardAndFragmentCompatibilityTests|FullyQualifiedName~GraphEditorMigrationCompatibility"` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** run the relevant targeted editor test command plus the smallest smoke command affected by the task
- **Per wave merge:** `dotnet test avalonia-node-map.sln -v minimal` and `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj`
- **Phase gate:** full suite green plus packed-package smoke green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] Exclude or clean `src/**/artifacts/**` generated `.cs` outputs from default compile items; current validation commands fail with `CS0579` before tests execute.
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorInitializationTests.cs` — cover factory/options defaults and any new `AddAsterGraph...` helpers.
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorMigrationCompatibilityTests.cs` — cover obsolete/migration shims and parity between old and new entry paths.
- [ ] `tools/AsterGraph.PackageSmoke` update — verify the final documented package boundary once `AsterGraph.Editor` is formalized as host-facing for advanced integration.

## Sources

### Primary (HIGH confidence)
- Repo canonical refs: `README.md`, `docs/host-integration.md`, `tools/AsterGraph.HostSample/Program.cs`, `tools/AsterGraph.PackageSmoke/Program.cs`, `Directory.Build.props`, package `*.csproj` files, and current tests under `tests/`
- Microsoft lifecycle: https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core
- Target frameworks in SDK-style projects: https://learn.microsoft.com/en-us/dotnet/standard/frameworks
- Breaking changes and .NET libraries: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes
- Options `Configure<TOptions>` API: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.optionsservicecollectionextensions.configure
- Options pattern guidance for .NET library authors: https://learn.microsoft.com/en-us/dotnet/core/extensions/options-library-authors
- NuGet package deprecation guidance: https://learn.microsoft.com/en-us/nuget/nuget-org/deprecate-packages
- NuGet official registration data queried on 2026-03-25 for `Avalonia`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Options`, `Microsoft.NET.Test.Sdk`, `xunit`, and `xunit.runner.visualstudio`

### Secondary (MEDIUM confidence)
- None.

### Tertiary (LOW confidence)
- None.

## Metadata

**Confidence breakdown:**
- Standard stack: MEDIUM - package boundary and support matrix are well-supported, but the exact DI-helper choice remains discretionary
- Architecture: MEDIUM - the hybrid/factory-first recommendation is a strong inference from locked decisions plus current code, not an implemented fact
- Pitfalls: HIGH - grounded in current repo failures and official .NET library guidance

**Research date:** 2026-03-25
**Valid until:** 2026-04-24
