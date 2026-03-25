# Testing Patterns

**Analysis Date:** 2026-03-25

## Test Framework

**Runner:**
- `xUnit` `2.9.2` via `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` and `tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`.
- Config: there is no dedicated `xunit.runner.json`, `runsettings`, or coverage config file; test setup lives directly in the two test project files.
- UI-facing tests in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` use `Avalonia.Headless.XUnit` `11.3.10` with `[assembly: AvaloniaTestApplication(typeof(GraphEditorViewTestsAppBuilder))]`.

**Assertion Library:**
- Built-in `Xunit.Assert`; no `FluentAssertions`, `Shouldly`, or snapshot-test library was detected.

**Run Commands:**
```bash
dotnet test avalonia-node-map.sln
dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorViewTests
# Coverage: not configured in-repo; no coverlet package, collector, or runsettings file detected
```

## Test File Organization

**Location:**
- Tests live in dedicated projects under `tests/`, not next to production files.
- `tests/AsterGraph.Editor.Tests/` contains 15 test files covering editor, Avalonia, and host-integration seams.
- `tests/AsterGraph.Serialization.Tests/` contains serialization compatibility coverage in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

**Naming:**
- Test classes and files use `SubjectBehaviorTests` naming, for example `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs`, `tests/AsterGraph.Editor.Tests/NodeCanvasInteractionSessionTests.cs`, and `tests/AsterGraph.Editor.Tests/BrushFactorySafetyTests.cs`.

**Structure:**
```text
tests/
  AsterGraph.Editor.Tests/
    *.cs                        # editor logic, Avalonia shell, internal helpers, host seams
    AsterGraph.Editor.Tests.csproj
  AsterGraph.Serialization.Tests/
    SerializationCompatibilityTests.cs
    AsterGraph.Serialization.Tests.csproj
```

## Test Structure

**Suite Organization:**
```csharp
public sealed class NodeParameterValueAdapterTests
{
    [Fact]
    public void NormalizeValue_NumberAcceptsCommaAndDotFormats()
    {
        var definition = CreateNumberDefinition(isInt: false);
        var result = NodeParameterValueAdapter.NormalizeValue(definition, "1.5");

        Assert.True(result.IsValid);
        Assert.Equal(1.5d, Assert.IsType<double>(result.Value));
    }

    private static NodeParameterDefinition CreateNumberDefinition(...)
        => ...;
}
```

**Patterns:**
- Most tests are single-method `[Fact]` cases with explicit Arrange/Act/Assert blocks inside the method body, as seen across `tests/AsterGraph.Editor.Tests/NodeParameterValueAdapterTests.cs`, `tests/AsterGraph.Editor.Tests/GraphHostContextExtensionsTests.cs`, and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.
- Test data factories are local private static helpers inside the same file, such as `CreateEditor`, `CreateNode`, and `CreateDocument` in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`, `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`, and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.
- Cleanup and environment restoration are handled inline with `try/finally`, especially for culture changes and temp files in `tests/AsterGraph.Editor.Tests/NodeParameterValueAdapterTests.cs` and `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`.

## Mocking

**Framework:** No mocking framework is used.

**Patterns:**
```csharp
private sealed class TestClipboardBridge : IGraphTextClipboardBridge
{
    public string? Text { get; set; }

    public Task<string?> ReadTextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Text);

    public Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
    {
        Text = text;
        return Task.CompletedTask;
    }
}
```

**What to Mock:**
- Mock only host/platform seams with small handwritten doubles: `IGraphTextClipboardBridge` in `tests/AsterGraph.Editor.Tests/EditorClipboardAndFragmentCompatibilityTests.cs`, `IGraphLocalizationProvider` in `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs`, `IGraphHostContext` in `tests/AsterGraph.Editor.Tests/GraphHostContextExtensionsTests.cs`, and `IGraphContextMenuAugmentor` in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`.
- Use `InternalsVisibleTo` for internal implementation tests instead of changing visibility of production types; see `src/AsterGraph.Editor/Properties/AssemblyInfo.cs`, `src/AsterGraph.Avalonia/Properties/AssemblyInfo.cs`, and `src/AsterGraph.Core/Properties/AssemblyInfo.cs`.

**What NOT to Mock:**
- Do not mock core models, serializers, or the main editor view model. Tests instantiate real `GraphEditorViewModel`, `NodeCatalog`, `GraphDocument`, `GraphNode`, and `GraphConnection` objects in files such as `tests/AsterGraph.Editor.Tests/GraphEditorFacadeRefactorTests.cs`, `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`, and `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

## Fixtures and Factories

**Test Data:**
```csharp
private static GraphDocument CreateDocument()
    => new(
        "Serialization Test",
        "Exercise versioned graph document payloads.",
        [CreateNode()],
        [CreateConnection()]);
```

**Location:**
- Factories are local to each suite. There is no shared `TestHelpers/`, fixture project, or reusable builder library under `tests/`.
- Common examples: `CreateEditor` in `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs`, `CreateNode` in `tests/AsterGraph.Editor.Tests/NodeCanvasInteractionSessionTests.cs`, and `CreateFragment` in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

## Coverage

**Requirements:** None enforced. No minimum threshold, collector, or CI coverage settings were found in the repository.

**View Coverage:**
```bash
# Not configured in-repo
```

## Test Types

**Unit Tests:**
- Dominant test style. Most suites target pure or near-pure logic in editor services, parameter normalization, geometry helpers, context helpers, and presentation/state code under `src/AsterGraph.Editor/` and `src/AsterGraph.Avalonia/`.

**Integration Tests:**
- Lightweight integration tests exist for Avalonia shell behavior and serialization contracts.
- `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` boots a headless Avalonia application and verifies `GraphEditorView` chrome behavior and host-context propagation.
- `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs` verifies backward-compatible schema handling across `src/AsterGraph.Core/Serialization/` and `src/AsterGraph.Editor/Services/`.

**E2E Tests:**
- Not used. No browser automation, screenshot approval tests, or end-to-end host app test project was detected.

## Common Patterns

**Async Testing:**
```csharp
[Fact]
public async Task PasteSelectionAsync_ReadsLegacyClipboardPayload()
{
    var bridge = new TestClipboardBridge { Text = CreateLegacyClipboardJson() };
    var editor = CreateEditor(bridge, nodes: []);

    await editor.PasteSelectionAsync();

    Assert.Single(editor.Nodes);
}
```

**Error Testing:**
```csharp
[Fact]
public void GraphDocumentSerializer_RejectsUnknownSchemaVersion()
{
    var exception = Assert.Throws<InvalidOperationException>(
        () => GraphDocumentSerializer.Deserialize(json));

    Assert.Contains("Unsupported graph document schema version", exception.Message);
}
```

---

*Testing analysis: 2026-03-25*
