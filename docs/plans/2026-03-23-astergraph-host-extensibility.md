# AsterGraph Host Extensibility Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add host-oriented menu, presentation, localization, and host-context extension seams without baking business semantics into AsterGraph.

**Architecture:** Introduce a small host integration layer in `AsterGraph.Editor` for menu metadata, host context, node presentation, and localization. Keep `AsterGraph.Avalonia` responsible only for rendering those contracts and for injecting platform-specific host context. Preserve high cohesion by grouping host-facing concerns into dedicated namespaces instead of scattering fields across existing view models and controls.

**Tech Stack:** .NET 8/9, Avalonia 11, CommunityToolkit.Mvvm, xUnit

---

### Task 1: Add menu disabled reasons and localized stock menu text

**Files:**
- Modify: `src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs`
- Modify: `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs`
- Create: `src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphContextMenuBuilderTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void BuildCanvasMenu_DisabledCommandsExposeDisabledReason()
{
    var editor = HostExtensibilityTestFactory.CreateReadOnlyEditor();

    var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)));
    var saveItem = menu.Single(item => item.Id == "canvas-save");

    Assert.False(saveItem.IsEnabled);
    Assert.Equal("Snapshot saving is disabled by host permissions.", saveItem.DisabledReason);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphContextMenuBuilderTests`
Expected: FAIL because `DisabledReason` and localized stock text plumbing do not exist yet.

**Step 3: Write minimal implementation**

Add `DisabledReason` to `MenuItemDescriptor`, wire stock builder reasons through helper methods, add `IGraphLocalizationProvider`, and have `GraphEditorViewModel` expose a localized string lookup used by the builder.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphContextMenuBuilderTests`
Expected: PASS

**Step 5: Commit**

```bash
git add src/AsterGraph.Editor/Menus/MenuItemDescriptor.cs src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs src/AsterGraph.Editor/Localization/IGraphLocalizationProvider.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Avalonia/Menus/GraphContextMenuPresenter.cs tests/AsterGraph.Editor.Tests/GraphContextMenuBuilderTests.cs
git commit -m "feat: add menu disabled reasons and localization seam"
```

### Task 2: Add host context injection to context menu requests

**Files:**
- Create: `src/AsterGraph.Editor/Hosting/IGraphHostContext.cs`
- Modify: `src/AsterGraph.Editor/Menus/ContextMenuContext.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasContextMenuContextFactory.cs`
- Test: `tests/AsterGraph.Editor.Tests/NodeCanvasContextMenuContextFactoryTests.cs`
- Test: `tools/AsterGraph.HostSample/Program.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void CreateNodeContext_EmbedsHostContext()
{
    var snapshot = HostExtensibilityTestFactory.CreateContextMenuSnapshot();
    var hostContext = new TestHostContext(owner: new object(), topLevel: new object());

    var context = NodeCanvasContextMenuContextFactory.CreateNodeContext(
        snapshot,
        new GraphPoint(10, 20),
        "node-001",
        useSelectionTools: false,
        hostContext);

    Assert.Same(hostContext, context.HostContext);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter NodeCanvasContextMenuContextFactoryTests`
Expected: FAIL because `HostContext` does not exist yet.

**Step 3: Write minimal implementation**

Add `IGraphHostContext`, carry it through `ContextMenuContext`, let `GraphEditorView` inject an Avalonia-backed host context, and pass it through the node canvas factory.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter NodeCanvasContextMenuContextFactoryTests`
Expected: PASS

**Step 5: Commit**

```bash
git add src/AsterGraph.Editor/Hosting/IGraphHostContext.cs src/AsterGraph.Editor/Menus/ContextMenuContext.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs src/AsterGraph.Avalonia/Controls/Internal/NodeCanvasContextMenuContextFactory.cs tests/AsterGraph.Editor.Tests/NodeCanvasContextMenuContextFactoryTests.cs tools/AsterGraph.HostSample/Program.cs
git commit -m "feat: add host context to context menu requests"
```

### Task 3: Add node presentation state and refresh API

**Files:**
- Create: `src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs`
- Create: `src/AsterGraph.Editor/Presentation/NodePresentationState.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Editor/ViewModels/NodeViewModel.cs`
- Modify: `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorPresentationTests.cs`

**Step 1: Write the failing test**

```csharp
[Fact]
public void RefreshNodePresentation_UpdatesSubtitleBadgeAndStatusBar()
{
    var provider = new TestNodePresentationProvider();
    var editor = HostExtensibilityTestFactory.CreateEditor(presentationProvider: provider);
    var node = editor.Nodes[0];

    provider.SetState(node.Id, new NodePresentationState(
        SubtitleOverride: "已加载",
        DescriptionOverride: "离线数据已同步",
        TopRightBadges: [new NodeAdornmentDescriptor("成功", "#3FB950", "最近一次运行成功")],
        StatusBar: new NodeStatusBarDescriptor("可预览", "#2F81F7")));

    editor.RefreshNodePresentation(node.Id);

    Assert.Equal("已加载", node.Subtitle);
    Assert.Single(node.Presentation.TopRightBadges);
    Assert.Equal("可预览", node.Presentation.StatusBar!.Text);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorPresentationTests`
Expected: FAIL because presentation provider/state and refresh API do not exist yet.

**Step 3: Write minimal implementation**

Add host-facing presentation contracts, let `GraphEditorViewModel` own refresh methods, keep `NodeViewModel` carrying current presentation snapshot, and render badges/status bar/override text in `NodeCanvas`.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorPresentationTests`
Expected: PASS

**Step 5: Commit**

```bash
git add src/AsterGraph.Editor/Presentation/INodePresentationProvider.cs src/AsterGraph.Editor/Presentation/NodePresentationState.cs src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Editor/ViewModels/NodeViewModel.cs src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs tests/AsterGraph.Editor.Tests/GraphEditorPresentationTests.cs
git commit -m "feat: add node presentation state refresh"
```

### Task 4: Apply localization provider to stock inspector/library/status text

**Files:**
- Modify: `src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`
- Modify: `src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs`
- Modify: `src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs`
- Test: `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs`
- Modify: `README.md`

**Step 1: Write the failing test**

```csharp
[Fact]
public void InspectorStrings_UseLocalizationProviderFallbackKeys()
{
    var editor = HostExtensibilityTestFactory.CreateEditor(localizationProvider: new TestLocalizationProvider());

    Assert.Equal("未选择节点", editor.InspectorTitle);
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorLocalizationTests`
Expected: FAIL because inspector and status text still use hard-coded strings.

**Step 3: Write minimal implementation**

Move stock text access behind localization keys and keep fallback English/Chinese defaults inside one place.

**Step 4: Run test to verify it passes**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj --filter GraphEditorLocalizationTests`
Expected: PASS

**Step 5: Commit**

```bash
git add src/AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs src/AsterGraph.Editor/Menus/GraphContextMenuBuilder.cs src/AsterGraph.Demo/ViewModels/MainWindowViewModel.cs tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs README.md
git commit -m "feat: localize stock editor strings"
```

### Task 5: Regression and host smoke

**Files:**
- Modify: `tools/AsterGraph.HostSample/Program.cs`
- Modify: `tools/AsterGraph.PackageSmoke/Program.cs`

**Step 1: Run editor tests**

Run: `dotnet test tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj`
Expected: PASS

**Step 2: Run serialization tests**

Run: `dotnet test tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj`
Expected: PASS

**Step 3: Run build**

Run: `dotnet build avalonia-node-map.sln`
Expected: PASS

**Step 4: Run host sample**

Run: `dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj`
Expected: PASS with output proving host context, menu reasons, and presentation hooks are wired.

**Step 5: Commit**

```bash
git add tools/AsterGraph.HostSample/Program.cs tools/AsterGraph.PackageSmoke/Program.cs
git commit -m "test: add host extensibility smoke coverage"
```
