# AsterGraph Serialization Versioning And Regression Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add explicit schema versioning to graph-document and clipboard serialization while introducing automated regression coverage for backward-compatible reads.

**Architecture:** Keep the change narrow and maintainable. `GraphDocumentSerializer` emits a versioned envelope but still reads the older unversioned shape. `GraphClipboardPayloadSerializer` gains a `SchemaVersion` field and still accepts the legacy payload. A focused test project validates both current and legacy paths without dragging the full UI into the test surface.

**Tech Stack:** .NET 8/9, System.Text.Json, xUnit, existing package smoke and host sample tools.

---

### Task 1: Version The Graph Document Contract

**Files:**
- Modify: `src/AsterGraph.Core/Serialization/GraphDocumentSerializer.cs`

**Step 1: Emit a schema version**

- Serialize graph documents through a versioned payload envelope.

**Step 2: Keep legacy reads working**

- Detect old unversioned JSON and deserialize it as a legacy document shape.
- Reject unknown future schema versions explicitly.

### Task 2: Version The Clipboard Payload Contract

**Files:**
- Modify: `src/AsterGraph.Editor/Services/GraphClipboardPayload.cs`
- Modify: `src/AsterGraph.Editor/Services/GraphClipboardPayloadSerializer.cs`

**Step 1: Emit a clipboard schema version**

- Add `SchemaVersion` to the serialized payload.

**Step 2: Keep legacy clipboard reads working**

- Accept the older payload shape that only had `Format`, `Origin`, `PrimaryNodeId`, `Nodes`, and `Connections`.
- Reject unknown future schema versions cleanly.

### Task 3: Add Focused Regression Tests

**Files:**
- Create: `src/AsterGraph.Editor/Properties/AssemblyInfo.cs`
- Create: `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`
- Create: `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`
- Modify: `avalonia-node-map.sln`

**Step 1: Expose required internals**

- Use `InternalsVisibleTo` only for the serialization test project.

**Step 2: Cover the high-value compatibility cases**

- Current graph-document payload contains schema version
- Legacy graph-document payload still reads
- Unknown graph-document schema version is rejected
- Current clipboard payload contains schema version
- Legacy clipboard payload still reads
- Unknown clipboard schema version is rejected

### Task 4: Update README

**Files:**
- Modify: `README.md`

**Step 1: Describe schema-version behavior**

- State that current document and clipboard payloads emit `SchemaVersion`.
- State that readers remain backward-compatible with the previous unversioned shapes.

### Task 5: Verify And Ship

**Files:**
- Modify: `Directory.Build.props`

**Step 1: Version bump**

- Bump package version for the serialization contract update.

**Step 2: Run verification**

- `dotnet build avalonia-node-map.sln`
- `dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`
- `dotnet pack src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Core/AsterGraph.Core.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Editor/AsterGraph.Editor.csproj -c Release -o artifacts/packages`
- `dotnet pack src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj -c Release -o artifacts/packages`
- `dotnet run --project tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj -p:UsePackedAsterGraphPackages=true`
- `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
