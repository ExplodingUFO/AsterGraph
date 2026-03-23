# AsterGraph Upgrade Path And Editor Regression Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make schema-version compatibility easier to maintain by extracting explicit upgrade helpers and add editor-level regression tests for clipboard and fragment workflows.

**Architecture:** Keep the current public API stable. Move serializer version branching into dedicated compatibility helpers so future schema changes are localized, and add a separate editor regression test project for `GraphEditorViewModel` behaviors around copy, paste, import, and export. Preserve existing packed-package smoke and sample-host validation as the outer regression ring.

**Tech Stack:** .NET 8/9, System.Text.Json, xUnit, existing AsterGraph projects and tooling.

---

### Task 1: Extract Compatibility Helpers

**Files:**
- Modify: `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphClipboardPayload.cs`

**Step 1: Add explicit compatibility helpers**

- Split schema-version branching out of the serializer mainline into dedicated helper methods or helper types.
- Keep the public serializer surface unchanged.

**Step 2: Preserve current behavior**

- Continue to emit `SchemaVersion`.
- Continue to read legacy unversioned payloads.
- Continue to reject unknown future schema versions.

### Task 2: Add Editor Regression Tests

**Files:**
- Create: `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`
- Create: `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`
- Modify: `avalonia-node-map.sln`

**Step 1: Build a focused editor test project**

- Reference:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
- Use xUnit.

**Step 2: Cover behavior that hosts depend on**

- `CopySelectionAsync` writes a versioned clipboard payload through the host bridge
- `PasteSelectionAsync` accepts a legacy clipboard payload
- `ExportSelectionFragmentTo(...)` writes a versioned fragment file
- `ImportFragmentFrom(...)` accepts a legacy fragment file payload

### Task 3: Update README

**Files:**
- Modify: `README.md`

**Step 1: Clarify the schema-upgrade boundary**

- Explain that schema branching now lives in dedicated compatibility helpers.
- Keep host-facing wording concise.

### Task 4: Verify And Ship

**Files:**
- Modify: `Directory.Build.props` if version bump is needed

**Step 1: Run verification**

- `dotnet build avalonia-node-map.sln`
- `dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`
- `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`
- `dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`

**Step 2: Commit**

- Commit only upgrade-helper, test-project, README, solution, and version changes.
