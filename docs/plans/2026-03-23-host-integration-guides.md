# Host Integration Guides Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a reusable host integration sample, typed host-context helpers, and node presentation guidance for AsterGraph hosts.

**Architecture:** Keep the editor contract framework-neutral by adding helper extensions instead of UI-specific properties. Expand the runnable host sample so it demonstrates the four main extension seams together, then document both the integration flow and presentation recommendations from the same source of truth.

**Tech Stack:** .NET 8, Avalonia, CommunityToolkit.Mvvm, xUnit, markdown docs

---

### Task 1: Add Host Context Helper Tests

**Files:**
- Create: `tests/AsterGraph.Editor.Tests/GraphHostContextExtensionsTests.cs`
- Modify: `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`

**Step 1: Write the failing test**

Add tests that assert:

- `TryGetOwner<T>` succeeds when `Owner` matches `T`
- `TryGetOwner<T>` returns `false` and `null` when types do not match
- `TryGetTopLevel<T>` succeeds when `TopLevel` matches `T`
- `ContextMenuContext` extension overloads delegate through `HostContext`
- null host/context paths fail safely

**Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphHostContextExtensionsTests
```

Expected:

- build/test failure because the helper API does not exist yet

**Step 3: Write minimal implementation**

Do not touch Avalonia types. Add only the extension surface required by the tests.

**Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphHostContextExtensionsTests
```

Expected:

- PASS

**Step 5: Commit**

```powershell
git add tests/AsterGraph.Editor.Tests/GraphHostContextExtensionsTests.cs tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj src/AsterGraph.Editor/Hosting/GraphHostContextExtensions.cs
git commit -m "feat: add host context helper extensions"
```

### Task 2: Expand Host Sample To Cover All Integration Seams

**Files:**
- Modify: `tools/AsterGraph.HostSample/Program.cs`

**Step 1: Write the failing host sample assertions**

Update the host sample output contract so it must prove:

- localization provider changed at least one stock string
- node presentation provider changed at least one node presentation field
- typed host context retrieval works from menu context
- style options are applied into the editor/view model composition

**Step 2: Run sample to verify it fails**

Run:

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj
```

Expected:

- output missing one or more new proof lines

**Step 3: Write minimal implementation**

Add sample-only helper classes in the same file if possible:

- a sample localization provider
- a sample presentation provider
- a sample host context with typed owner/top-level objects
- a sample style options value

Keep the sample lightweight and deterministic.

**Step 4: Run sample to verify it passes**

Run:

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj
```

Expected:

- output includes explicit proof lines for localization, presentation, style, and typed host context

**Step 5: Commit**

```powershell
git add tools/AsterGraph.HostSample/Program.cs
git commit -m "feat: expand host integration sample"
```

### Task 3: Add Host Integration Documentation

**Files:**
- Create: `docs/host-integration.md`
- Modify: `README.md`
- Modify: `src/AsterGraph.Editor/README.md`
- Modify: `src/AsterGraph.Avalonia/README.md`

**Step 1: Write the docs before editing references**

Document:

- when a host only needs `AsterGraph.Avalonia`
- when a host should also reference `AsterGraph.Editor`
- how to wire localization, presentation, style, and menu context together
- how to use the new helper API safely
- where the runnable sample lives

**Step 2: Update README links**

Add short entry-point references from the existing READMEs instead of duplicating long content.

**Step 3: Verify docs are linked and discoverable**

Run:

```powershell
rg -n "host-integration|HostSample|node-presentation-guidelines" README.md src/AsterGraph.Editor/README.md src/AsterGraph.Avalonia/README.md docs/host-integration.md
```

Expected:

- every new doc is reachable from at least one existing README

**Step 4: Commit**

```powershell
git add docs/host-integration.md README.md src/AsterGraph.Editor/README.md src/AsterGraph.Avalonia/README.md
git commit -m "docs: add host integration guide"
```

### Task 4: Add Node Presentation Guidelines

**Files:**
- Create: `docs/node-presentation-guidelines.md`

**Step 1: Write the guide**

Cover:

- recommended badge count
- recommended badge text length
- recommended status bar text length
- tooltip guidance
- color and emphasis guidance
- host responsibilities vs editor responsibilities

**Step 2: Verify the guide is linked**

Run:

```powershell
rg -n "node-presentation-guidelines" README.md docs/host-integration.md docs/node-presentation-guidelines.md
```

Expected:

- guide appears in both the guide file and at least one entry-point doc

**Step 3: Commit**

```powershell
git add docs/node-presentation-guidelines.md docs/host-integration.md README.md
git commit -m "docs: add node presentation guidance"
```

### Task 5: Final Verification And Packaging Smoke

**Files:**
- No code changes unless verification reveals issues

**Step 1: Run focused editor tests**

```powershell
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj
```

Expected:

- PASS

**Step 2: Run serialization regression tests**

```powershell
dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj
```

Expected:

- PASS

**Step 3: Run host sample**

```powershell
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj
```

Expected:

- output proves localization, presentation, host context, and style composition

**Step 4: Run package smoke**

```powershell
dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj
```

Expected:

- PASS

**Step 5: Commit any verification-driven fix**

Only if needed:

```powershell
git add <files>
git commit -m "fix: polish host integration guidance"
```
