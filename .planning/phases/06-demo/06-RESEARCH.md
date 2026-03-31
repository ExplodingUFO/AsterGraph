# Phase 6: Demo 程序排版修复、全面中文化、并用演示方式覆盖现有架构能力 - Research

**Researched:** 2026-03-27
**Domain:** Avalonia demo-shell redesign, localization coverage, and architecture-showcase composition for AsterGraph
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
### 页面结构
- **D-01:** 新版 Demo 采用“三栏总览”主布局：左侧用于能力导航与切换，中间为主演示区，右侧为说明、状态与架构信息区。
- **D-02:** 能力呈现采用“总览 + 分区”的单页结构，不做按模式跳转的多页体验，避免用户来回切换后失去整体架构视图。
- **D-03:** 中心内容以“单主编辑器”为核心，保留一个可交互的主编辑器实例作为体验中心，其余能力通过分区说明、状态卡片、辅助面板或受控预览表达，不在首页堆叠多个同级实时编辑器视图。

### 能力展示范围
- **D-04:** 首页必须明确覆盖四类现有架构能力：完整壳层、独立表面、可替换呈现、运行时与诊断。
- **D-05:** “完整壳层”能力应直接体现 `GraphEditorView` 的默认集成价值，而不是只做文字介绍。
- **D-06:** “独立表面”能力应明确说明并可见地展示 standalone canvas、inspector、mini map 的可嵌入边界。
- **D-07:** “可替换呈现”能力应体现 stock 与 custom presenter 的替换路径，以及宿主可插拔的视觉扩展点。
- **D-08:** “运行时与诊断”能力应把会话、查询、能力快照、诊断/检查能力显式展示出来，而不是隐藏在代码或控制台里。

### 中文化策略
- **D-09:** Demo 采用“全面中文”策略：Demo 外壳、自定义说明区、默认工具栏、侧栏说明、状态文案都尽量中文化。
- **D-10:** API 名称、类型名、接口名等技术标识可以保留英文，不强行把源码级术语翻译进 UI。
- **D-11:** 中文化不应只停留在 `MainWindow` 外壳；默认 `GraphEditorView` 内当前英文文案也属于本阶段关注范围。

### 演示表达风格
- **D-12:** 整体气质采用“架构导向”的 SDK 展示台风格，而不是纯产品营销风格。
- **D-13:** 每个展示区块都应帮助用户理解“这一层属于哪一层架构、能替换什么、宿主如何接入”，而不只是展示视觉效果。

### Claude's Discretion
- 在“三栏总览”内每栏的具体比例、卡片密度、层级分组与视觉样式
- 如何在不压垮主编辑体验的前提下展示 standalone surfaces 与 diagnostics 信息
- 中文术语的最终措辞，只要保持技术语义稳定、读起来像中文软件界面

### Deferred Ideas (OUT OF SCOPE)
- 新增超出当前架构范围的图编辑器终端功能
- 为 diagnostics 单独再做一套完整工作台产品界面
- 引入 Avalonia 之外的第二套前端/展示技术栈
</user_constraints>

## Project Constraints (from CLAUDE.md)

- Keep the solution centered on .NET, C#, and Avalonia.
- Treat API reorganization as phased migration, not an uncontrolled break.
- Keep the result publishable as a general-purpose component library; public API quality matters.
- Preserve existing validated editing capabilities during the transition; this is refactor/SDK hardening, not rewrite.
- Hosts must be able to replace or embed subcomponents independently.
- Debuggability is a product requirement; diagnostics need explicit public seams.
- Keep visual-only shell toggles in `AsterGraph.Avalonia/Controls/GraphEditorViewChromeMode.cs`; do not move them into editor behavior options.
- Route editor behavior through `AsterGraph.Editor/ViewModels/GraphEditorViewModel.cs`; Avalonia should render/bind that state rather than inventing a second runtime layer.
- Use `IGraphLocalizationProvider` for built-in editor text localization rather than hard-coding alternate runtime paths.
- Follow existing C#/Avalonia naming and layering conventions; add new types to owning project namespaces rather than creating export barrels.
- Validation is compiler + xUnit/Avalonia headless tests; plan around the existing focused-test workflow.
- Do not recommend approaches that move diagnostics ownership into `AsterGraph.Avalonia` or add a separate workbench product in this phase.

## Summary

Phase 6 is not a feature phase. It is a demo-host composition and communication phase that should make already-built architecture visible, understandable, and Chinese-first without undermining the current package/runtime boundaries. The strongest implementation path is to keep one real `GraphEditorView`/editor session as the center, then use the left and right columns to control, explain, and inspect that same session plus adjacent standalone/custom presentation proof blocks. This matches the locked “single main editor” decision and the existing architecture where `AsterGraph.Editor` owns state while `AsterGraph.Avalonia` renders shells and standalone surfaces.

The codebase already contains nearly all proof points needed. `GraphEditorView` already exposes full-shell chrome regions and has many English literals in XAML; `GraphEditorViewModel` already localizes a large portion of captions through `IGraphLocalizationProvider`; `GraphEditorViewChromeMode` already proves shell-vs-canvas toggling; the standalone view factories already prove embeddable canvas/inspector/minimap; and the Phase 5 diagnostics/session APIs already expose inspection snapshots and recent diagnostics from `Editor.Session.Diagnostics`. The main planning challenge is therefore composition and information architecture, not deep new infrastructure.

The main technical risk is letting the demo drift into a second ad-hoc UI model: duplicating editor state in Demo-only view models, inventing separate fake diagnostics data, or localizing only the outer shell while leaving the default `GraphEditorView` in English. Avoid that. The demo should consume the real public seams the same way a host would, so the UI itself becomes the proof ring.

**Primary recommendation:** Build the new Demo as a single-page three-column “SDK architecture showcase” over one real editor/session, localize `GraphEditorView` + demo shell through existing localization seams and targeted XAML cleanup, and surface standalone/presenter/diagnostics capabilities as live proof panels tied to that same runtime.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET SDK | 10.0.201 installed | Build/run/test the solution across `net8.0`/`net9.0` projects | Already the local verified runtime and supports the repo target mix |
| Avalonia | 11.3.10 | Demo shell, full editor view, standalone surfaces | Current UI framework of record across `AsterGraph.Avalonia` and `AsterGraph.Demo` |
| Avalonia.Desktop | 11.3.10 | Desktop demo host bootstrap | Required by `src/AsterGraph.Demo/AsterGraph.Demo.csproj` |
| Avalonia.Themes.Fluent | 11.3.10 | Shared theme resources for demo and UI tests | Already used by Demo and headless tests |
| CommunityToolkit.Mvvm | 8.2.1 | Demo shell view model state and bindings | Existing MVVM pattern in `MainWindowViewModel` and the broader repo |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Avalonia.Headless.XUnit | 11.3.10 | UI layout/visibility regression tests | For focused Demo/GraphEditorView assertions without interactive desktop runs |
| xUnit | 2.9.2 | Unit and focused integration tests | For demo/view model and architecture proof assertions |
| Microsoft.NET.Test.Sdk | 17.11.1 | Test runner integration | Required for all repo test execution |
| AsterGraph editor/runtime APIs | workspace source | Session queries, diagnostics, command/capability surfaces | Use these directly for runtime/diagnostic showcase panels instead of fake data |
| AsterGraph Avalonia view factories | workspace source | Full shell and standalone surface composition | Use for live embeddable-surface proof blocks |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Reusing `GraphEditorView` + factories | Building a demo-only shell from raw internals | Would weaken the host-story proof and duplicate behavior/layout logic |
| `IGraphLocalizationProvider` + targeted XAML translation | Separate resource system only for Demo | Adds parallel localization path and leaves stock shell untranslated |
| Live session diagnostics panels | Static markdown/text explanation only | Easier, but fails D-08 because runtime/diagnostics stay non-obvious |
| One main editor plus proof panels | Multiple live editors on the home page | Violates D-03 and makes layout/readability worse |

**Installation:**
```bash
# Existing repo flow
# No new external stack needed beyond dotnet restore/build/test/run
```

**Version verification:** Verified from project files on 2026-03-27.
- `src/AsterGraph.Demo/AsterGraph.Demo.csproj` pins Avalonia/Avalonia.Desktop/Avalonia.Themes.Fluent/Avalonia.Fonts.Inter `11.3.10` and CommunityToolkit.Mvvm `8.2.1`.
- `tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj` pins Avalonia.Headless.XUnit `11.3.10`, Microsoft.NET.Test.Sdk `17.11.1`, xUnit `2.9.2`.

## Architecture Patterns

### Recommended Project Structure
```text
src/
├── AsterGraph.Demo/
│   ├── Views/MainWindow.axaml          # Three-column showcase shell
│   ├── ViewModels/MainWindowViewModel.cs # Demo-only composition, toggles, diagnostics mapping
│   └── Definitions/                    # Existing demo content providers
├── AsterGraph.Avalonia/
│   └── Controls/GraphEditorView.axaml  # Stock shell text/layout needing Chinese cleanup
└── AsterGraph.Editor/
    └── ViewModels/GraphEditorViewModel.cs # Canonical captions, status, queries, localization keys
```

### Pattern 1: Single canonical runtime, multiple visual proof panes
**What:** Keep exactly one primary `GraphEditorViewModel` / session as the live source of truth, and let the demo shell read from that runtime to populate architecture explanation, capability badges, session snapshots, and diagnostics cards.
**When to use:** For the central editor, runtime/diagnostics panel, and any “what the host can access” showcase.
**Example:**
```csharp
var retainedSession = editor.Session;
var retainedInspection = retainedSession.Diagnostics.CaptureInspectionSnapshot();
var retainedRecentDiagnostics = retainedSession.Diagnostics.GetRecentDiagnostics(10);
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs`

### Pattern 2: Showcase standalone surfaces through canonical factories
**What:** Demonstrate embeddable canvas/inspector/minimap using the public factory entry points and the same editor instance, rather than creating a parallel demo-only composition model.
**When to use:** For the “独立表面” section in the new left/right proof areas.
**Example:**
```csharp
var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
{
    Editor = editor,
});
var standaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
{
    Editor = editor,
});
var standaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
{
    Editor = editor,
});
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs`

### Pattern 3: Use chrome mode to explain full shell vs canvas-only boundaries
**What:** `GraphEditorViewChromeMode` is the existing shell-toggle seam. It is the cleanest visual proof of “full shell” vs “standalone canvas” without rebuilding editor state.
**When to use:** For a side-by-side explanation card or compact preview proving shell omission.
**Example:**
```csharp
view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Avalonia\Controls\GraphEditorView.axaml.cs`

### Pattern 4: Localize built-in editor text through `IGraphLocalizationProvider`
**What:** `GraphEditorViewModel` already routes many built-in captions through `LocalizeText` / `LocalizeFormat`, which call `_localizationProvider.GetString(key, fallback)`.
**When to use:** For inspector text, status/help captions, workspace/fragment summaries, and other runtime-owned strings.
**Example:**
```csharp
public string StatsCaption => LocalizeFormat(
    "editor.stats.caption",
    "{0} nodes  ·  {1} links  ·  {2:0}% zoom",
    Nodes.Count,
    Connections.Count,
    Zoom * 100);
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\ViewModels\GraphEditorViewModel.cs:562`

### Pattern 5: Keep presenter replacement proof visual-only
**What:** Custom presenter proof should remain an Avalonia presentation swap over the existing editor/session, not a second behavior implementation.
**When to use:** For the “可替换呈现” section.
**Example:**
```csharp
var customPresentation = new AsterGraphPresentationOptions
{
    NodeVisualPresenter = customNodePresenter,
    ContextMenuPresenter = customMenuPresenter,
    InspectorPresenter = customInspectorPresenter,
    MiniMapPresenter = customMiniMapPresenter,
};
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs`

### Anti-Patterns to Avoid
- **Demo-only fake architecture data:** Don’t hard-code counts/capabilities/diagnostics that the session can already provide.
- **Two runtime layers:** Don’t mirror editor state into a second “demo state model” just to render cards.
- **Localized shell, untranslated stock control:** Don’t stop at `MainWindow.axaml`; Phase 6 explicitly includes `GraphEditorView.axaml` text.
- **Multiple equal-weight live editors:** Violates D-03 and makes the architecture story harder to read.
- **Diagnostics in console only:** Host sample proves the APIs, but Demo must surface them in UI.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Full-shell vs canvas-only comparison | New shell-visibility subsystem in Demo | `GraphEditorViewChromeMode` | The seam already exists and is tested |
| Standalone surface showcase | Ad-hoc mock controls | `AsterGraphCanvasViewFactory`, `AsterGraphInspectorViewFactory`, `AsterGraphMiniMapViewFactory` | These are the public host composition APIs the demo should prove |
| Diagnostics/inspection view data | Custom snapshot classes | `Editor.Session.Diagnostics.CaptureInspectionSnapshot()` and `GetRecentDiagnostics(...)` | Phase 5 already defines the canonical debug surface |
| Localization of runtime-owned captions | Demo-specific string replacement layer | `IGraphLocalizationProvider` and existing `LocalizeText`/`LocalizeFormat` keys | Keeps text controlled at the runtime seam |
| Presenter replacement proof | New behavior fork | `AsterGraphPresentationOptions` presenters | Keeps replacement opt-in and visual-only |

**Key insight:** Phase 6 should mostly compose and expose existing seams. If the plan starts inventing new infrastructure for shell toggles, diagnostics snapshots, standalone previews, or localization routing, it is probably fighting the architecture instead of showcasing it.

## Common Pitfalls

### Pitfall 1: Over-localizing API identity
**What goes wrong:** UI translations rename APIs/types/interfaces in ways that obscure the actual extension seams.
**Why it happens:** A “fully Chinese” goal gets interpreted as “translate everything.”
**How to avoid:** Translate UX nouns and explanations, but keep code identifiers such as `GraphEditorView`, `IGraphEditorSession`, `AsterGraphCanvasViewFactory` in English where they are acting as technical anchors.
**Warning signs:** Cards or captions no longer tell users what public type/entry point they would actually use.

### Pitfall 2: Demo proves architecture with screenshots, not live seams
**What goes wrong:** The UI describes standalone surfaces/presenters/diagnostics but does not actually instantiate them from public APIs.
**Why it happens:** It is easier to author copy than to wire live proof widgets.
**How to avoid:** Every architecture section should bind to a real object or output from the existing public seam.
**Warning signs:** Showcase content can drift out of sync with HostSample or current runtime behavior.

### Pitfall 3: One-column information overload inside the main shell
**What goes wrong:** All architecture explanation gets stuffed into `GraphEditorView` or stacked above it, making the editing experience cramped.
**Why it happens:** Reusing the old top-toolbar-only demo layout.
**How to avoid:** Respect D-01/D-03. Keep the center column primarily for the live editor, and offload explanation/state cards to left/right columns.
**Warning signs:** The main editor loses usable canvas area or the user must scroll constantly to see core proof sections.

### Pitfall 4: Translating only outer XAML literals
**What goes wrong:** `MainWindow.axaml` becomes Chinese, but inspector/status/workspace/fragment/runtime text inside the stock shell remains English.
**Why it happens:** Outer-window literals are obvious, `GraphEditorViewModel` fallbacks and `GraphEditorView.axaml` literals are less obvious.
**How to avoid:** Audit both XAML literals and localization keys/fallbacks in `GraphEditorViewModel`.
**Warning signs:** Mixed Chinese/English shell after selection changes or command execution.

### Pitfall 5: Diagnostics panel shows `StatusMessage` only
**What goes wrong:** The demo claims to showcase diagnostics but only mirrors the human-readable status bar.
**Why it happens:** `StatusMessage` is easy to bind.
**How to avoid:** Treat `StatusMessage` as compatibility UX only; diagnostics cards should come from `Editor.Session.Diagnostics` snapshots/history.
**Warning signs:** No diagnostic code/severity/history appears anywhere in the UI.

### Pitfall 6: Validation blocked by existing workspace noise
**What goes wrong:** Phase validation assumes the current test projects are green, but unrelated local breakage masks Demo regressions.
**Why it happens:** Current workspace already has failing tests/build noise outside Phase 6.
**How to avoid:** Use focused Demo/GraphEditorView verification and document current blockers explicitly.
**Warning signs:** `dotnet test` failures reference unrelated compile issues in test files already noted in state.

## Code Examples

Verified patterns from current code:

### Localize built-in runtime captions
```csharp
private sealed class DemoGraphLocalizationProvider : IGraphLocalizationProvider
{
    private static readonly IReadOnlyDictionary<string, string> Values = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["editor.menu.canvas.addNode"] = "添加节点",
        ["editor.inspector.title.none"] = "请选择一个节点",
    };

    public string GetString(string key, string fallback)
        => Values.TryGetValue(key, out var localized) ? localized : fallback;
}
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Demo\ViewModels\MainWindowViewModel.cs:191`

### Pull diagnostics and inspection from the retained session
```csharp
var retainedInspection = retainedSession.Diagnostics.CaptureInspectionSnapshot();
var retainedRecentDiagnostics = retainedSession.Diagnostics.GetRecentDiagnostics(10);
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs:163`

### Prove shell composition over standalone surfaces
```csharp
Console.WriteLine($"Full shell embeds standalone surfaces: inspector={ReferenceEquals(editor, shellInspectorSurface.Editor)}, minimap={ReferenceEquals(editor, shellMiniMapSurface.ViewModel)}");
Console.WriteLine($"Standalone surfaces share editor: canvas={ReferenceEquals(editor, standaloneCanvas.ViewModel)}, inspector={ReferenceEquals(editor, standaloneInspector.Editor)}, minimap={ReferenceEquals(editor, standaloneMiniMap.ViewModel)}");
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs:265`

### Verify chrome-only shell changes do not rebuild editor state
```csharp
view.ChromeMode = GraphEditorViewChromeMode.CanvasOnly;
Assert.Same(editor, view.Editor);
Assert.Same(editor.Nodes[0], editor.SelectedNode);
```
Source: `F:\CodeProjects\DotnetCore\avalonia-node-map\tests\AsterGraph.Editor.Tests\GraphEditorViewTests.cs:74`

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Monolithic full-shell demo as the only story | Full shell plus standalone canvas/inspector/minimap factories | Phase 3 | Demo can now visibly prove embeddable surface boundaries |
| Stock presentation only | Optional custom node/menu/inspector/minimap presenters | Phase 4 | Demo can now show replaceable visuals without forking behavior |
| Status text as the main observability story | Session diagnostics, inspection snapshots, recent history, logger/activity integration | Phase 5 | Demo can now expose host-grade runtime observability |
| English-first shell copy | Localization-provider backed editor captions + Chinese docs/comments patterns | existing + Phase 6 target | Demo can become Chinese-first without changing API identifiers |

**Deprecated/outdated:**
- Treating `AsterGraph.Demo` as a simple playground window is outdated for this phase; it now needs to function as an SDK architecture showcase.
- Treating `StatusMessage` as the diagnostics story is outdated after Phase 5.
- Treating `GraphEditorView` as the only relevant host surface is outdated after Phase 3.

## Open Questions

1. **How dense should the live standalone/presenter previews be inside the three-column layout?**
   - What we know: The user wants visible proof, but D-03 forbids stacking multiple equal-weight live editors.
   - What's unclear: Whether previews should be miniature live controls, static diagrams plus live metadata, or collapsible sections.
   - Recommendation: Plan small live preview cards for standalone inspector/minimap and one controlled shell/canvas comparison, not full-size duplicate editors.

2. **How much of `GraphEditorView.axaml` should be text-translated vs structurally reorganized?**
   - What we know: It contains many English literals and shell sections already useful to the Demo.
   - What's unclear: Whether Phase 6 should only translate text or also simplify/relabel section groupings to better support the new architecture-story shell.
   - Recommendation: Plan a copy audit first, then only make structural changes that directly improve the architecture showcase; avoid gratuitous shell rewrites.

3. **Should Demo-specific architecture cards read raw session types directly or through mapped DTOs in `MainWindowViewModel`?**
   - What we know: The runtime APIs already provide immutable snapshots suitable for UI consumption.
   - What's unclear: Desired balance between direct binding simplicity and view-model formatting helpers.
   - Recommendation: Use thin formatting properties in `MainWindowViewModel`, but do not create parallel domain models.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| `dotnet` SDK | Build/run/test/demo execution | ✓ | 10.0.201 | — |
| Avalonia desktop packages | Demo UI runtime | ✓ | 11.3.10 in project refs | — |
| Avalonia headless test packages | UI regression tests | ✓ | 11.3.10 in test refs | — |
| `AsterGraph.HostSample` | Architecture proof capture | ✓ | workspace project | — |

**Missing dependencies with no fallback:**
- None found for planning. Phase 6 is code/UI composition work on the existing .NET/Avalonia stack.

**Missing dependencies with fallback:**
- None.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.2 + Avalonia.Headless.XUnit 11.3.10 |
| Config file | none — convention via test projects |
| Quick run command | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter GraphEditorViewTests` |
| Full suite command | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/avalonia-node-map.sln"` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| DEMO-LAYOUT-01 | Three-column demo shell keeps center editor usable and side panels visible | headless UI | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter Demo` | ❌ Wave 0 |
| DEMO-I18N-01 | Demo shell + stock shell Chinese copy appears through bindings/localization keys | headless UI / view-model | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter Localization` | ❌ Wave 0 |
| DEMO-SURFACES-01 | Full shell, standalone surfaces, and chrome-mode proof remain wired | headless UI | `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj" --filter GraphEditorViewTests` | ✅ |
| DEMO-PRESENTATION-01 | Custom presenter showcase still resolves through public presentation options | host sample / focused test | `dotnet run --project "F:/CodeProjects/DotnetCore/avalonia-node-map/tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj"` | ✅ host sample |
| DEMO-DIAG-01 | Demo surfaces inspection snapshot and recent diagnostics from session APIs | focused VM test / host sample | `dotnet run --project "F:/CodeProjects/DotnetCore/avalonia-node-map/tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj"` | ✅ host sample |

### Sampling Rate
- **Per task commit:** focused headless/UI command for the edited surface plus Demo build/run if needed
- **Per wave merge:** `dotnet test "F:/CodeProjects/DotnetCore/avalonia-node-map/tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj"`
- **Phase gate:** full relevant suite green plus host sample output still proving shell/surface/presenter/diagnostics paths

### Wave 0 Gaps
- [ ] `tests/AsterGraph.Editor.Tests/DemoMainWindowTests.cs` — verify three-column layout regions, key Chinese labels, and central editor presence
- [ ] `tests/AsterGraph.Editor.Tests/GraphEditorLocalizationTests.cs` — verify localized `GraphEditorViewModel` captions and selected `GraphEditorView` literals after Phase 6 translation
- [ ] `tests/AsterGraph.Editor.Tests/DemoDiagnosticsProjectionTests.cs` — verify `MainWindowViewModel` maps session inspection/diagnostic data into showcase cards without fake data
- [ ] Fix or isolate current test-project noise before treating `dotnet test` failures as Phase 6 regressions. Current observed blockers: `GraphEditorViewTestsAppBuilder` resolution failure in `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` and static/instance serializer usage errors in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`.

## Sources

### Primary (HIGH confidence)
- `F:\CodeProjects\DotnetCore\avalonia-node-map\CLAUDE.md` - project constraints, architecture rules, workflow requirements
- `F:\CodeProjects\DotnetCore\avalonia-node-map\.planning\phases\06-demo\06-CONTEXT.md` - locked phase decisions and scope
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Demo\Views\MainWindow.axaml` - current demo layout and English UI copy
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Demo\ViewModels\MainWindowViewModel.cs` - current demo toggles and localization provider usage
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Avalonia\Controls\GraphEditorView.axaml` - stock shell structure and English literals
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Avalonia\Controls\GraphEditorView.axaml.cs` - chrome mode and presentation application seam
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\ViewModels\GraphEditorViewModel.cs` - localizable captions, workspace/fragment/mode/inspector strings
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\Localization\IGraphLocalizationProvider.cs` - canonical runtime localization seam
- `F:\CodeProjects\DotnetCore\avalonia-node-map\src\AsterGraph.Editor\Diagnostics\IGraphEditorDiagnostics.cs` - canonical diagnostics API
- `F:\CodeProjects\DotnetCore\avalonia-node-map\tools\AsterGraph.HostSample\Program.cs` - verified proof ring for shell, standalone surfaces, presenters, diagnostics, inspection
- `F:\CodeProjects\DotnetCore\avalonia-node-map\tests\AsterGraph.Editor.Tests\GraphEditorViewTests.cs` - current headless verification approach for shell chrome behavior
- `F:\CodeProjects\DotnetCore\avalonia-node-map\.planning\phases\03-embeddable-avalonia-surfaces\03-CONTEXT.md` - standalone surface decisions
- `F:\CodeProjects\DotnetCore\avalonia-node-map\.planning\phases\04-replaceable-presentation-kit\04-CONTEXT.md` - presenter replacement decisions
- `F:\CodeProjects\DotnetCore\avalonia-node-map\.planning\phases\05-diagnostics-integration-inspection\05-CONTEXT.md` - diagnostics/inspection decisions

### Secondary (MEDIUM confidence)
- `dotnet --version` output on 2026-03-27 - verified installed SDK `10.0.201`
- `dotnet run --project "F:/CodeProjects/DotnetCore/avalonia-node-map/tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj"` output on 2026-03-27 - verified current runtime/surface/presenter/diagnostics proof markers

### Tertiary (LOW confidence)
- None.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - directly verified from repo project files and local `dotnet` runtime
- Architecture: HIGH - based on locked phase context plus current source/proof code paths
- Pitfalls: HIGH - derived from concrete existing seams, current English literals, and observed validation blockers

**Research date:** 2026-03-27
**Valid until:** 2026-04-26
