using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Demo.ViewModels;

internal static class DemoHostMenuGroups
{
    public const string Showcase = "showcase";
    public const string View = "view";
    public const string Behavior = "behavior";
    public const string Runtime = "runtime";
    public const string Extensions = "extensions";
    public const string Automation = "automation";
    public const string Integration = "integration";
    public const string Proof = "proof";
}

public sealed record DemoLanguageOption(string Code, string DisplayName);

public partial class MainWindowViewModel
{
    private static readonly DemoLanguageOption ChineseLanguage = new("zh-CN", "中文");
    private static readonly DemoLanguageOption EnglishLanguage = new("en", "English");

    private static readonly IReadOnlyDictionary<string, string> ChineseUiText = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["menu.showcase"] = "展示",
        ["menu.view"] = "视图",
        ["menu.behavior"] = "行为",
        ["menu.runtime"] = "运行时",
        ["menu.extensions"] = "扩展",
        ["menu.automation"] = "自动化",
        ["menu.integration"] = "集成",
        ["menu.proof"] = "证明",
        ["menu.language"] = "语言",
        ["menu.showcase.open"] = "打开展示摘要",
        ["menu.pane.close"] = "收起面板",
        ["menu.view.header"] = "显示顶栏",
        ["menu.view.library"] = "显示节点库",
        ["menu.view.inspector"] = "显示检查器",
        ["menu.view.status"] = "显示状态栏",
        ["menu.view.minimap"] = "显示迷你地图",
        ["menu.view.open"] = "打开视图控制",
        ["menu.behavior.readOnly"] = "只读模式",
        ["menu.behavior.grid"] = "网格吸附",
        ["menu.behavior.guides"] = "对齐辅助线",
        ["menu.behavior.workspace"] = "工作区命令",
        ["menu.behavior.fragments"] = "片段命令",
        ["menu.behavior.hostExtensions"] = "宿主菜单扩展",
        ["menu.behavior.open"] = "打开行为控制",
        ["menu.runtime.state"] = "查看运行时状态",
        ["menu.runtime.diagnostics"] = "查看运行时诊断",
        ["menu.runtime.saveAs"] = "另存工作区",
        ["menu.runtime.reopenLast"] = "重新打开上次工作区",
        ["menu.runtime.restoreDraft"] = "恢复自动保存草稿",
        ["menu.extensions.candidates"] = "查看插件候选项",
        ["menu.extensions.loading"] = "查看加载与信任",
        ["menu.extensions.exportAllowlist"] = "导出 allowlist",
        ["menu.extensions.importAllowlist"] = "导入 allowlist",
        ["menu.automation.runSelection"] = "运行焦点自动化",
        ["menu.automation.runPlugin"] = "运行插件自动化",
        ["menu.automation.runWorkspace"] = "运行工作区自动化",
        ["menu.automation.open"] = "打开自动化面板",
        ["menu.integration.consumerPath"] = "查看 consumer path",
        ["menu.integration.standalone"] = "查看独立表面",
        ["menu.proof.boundary"] = "查看宿主边界",
        ["badge.language"] = "语言 · {0}",
        ["drawer.caption"] = "宿主控制抽屉",
        ["intro.kicker"] = "宿主级展示台",
        ["intro.title"] = "实时 SDK 会话",
        ["badge.hostOwnership"] = "宿主控制",
        ["badge.runtimeOwnership"] = "共享运行时",
        ["badge.activeGroup"] = "当前分组 · {0}",
        ["showcase.heading"] = "能力展示",
        ["runtime.config.heading"] = "宿主开关快照",
        ["runtime.signals.heading"] = "共享运行时状态",
        ["runtime.diagnostics.heading"] = "最近诊断",
        ["runtime.shell.heading"] = "原生壳层流程",
        ["extensions.candidates.heading"] = "插件候选项",
        ["extensions.allowlist.heading"] = "Allowlist 决策",
        ["extensions.loads.heading"] = "插件加载快照",
        ["automation.request.heading"] = "自动化请求",
        ["automation.progress.heading"] = "自动化进度",
        ["automation.result.heading"] = "自动化结果",
        ["integration.consumer.heading"] = "Consumer Path",
        ["integration.standalone.heading"] = "独立表面",
        ["integration.presentation.heading"] = "替换呈现",
        ["integration.localization.heading"] = "本地化证明",
        ["proof.config.heading"] = "宿主控制快照",
        ["proof.ownership.heading"] = "边界证据",
        ["proof.runtime.heading"] = "共享运行时证据",
        ["proof.shell.heading"] = "宿主壳层证据",
    };

    private static readonly IReadOnlyDictionary<string, string> EnglishUiText = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["menu.showcase"] = "Showcase",
        ["menu.view"] = "View",
        ["menu.behavior"] = "Behavior",
        ["menu.runtime"] = "Runtime",
        ["menu.extensions"] = "Extensions",
        ["menu.automation"] = "Automation",
        ["menu.integration"] = "Integration",
        ["menu.proof"] = "Proof",
        ["menu.language"] = "Language",
        ["menu.showcase.open"] = "Open showcase summary",
        ["menu.pane.close"] = "Close drawer",
        ["menu.view.header"] = "Show header",
        ["menu.view.library"] = "Show library",
        ["menu.view.inspector"] = "Show inspector",
        ["menu.view.status"] = "Show status bar",
        ["menu.view.minimap"] = "Show mini map",
        ["menu.view.open"] = "Open view controls",
        ["menu.behavior.readOnly"] = "Read-only mode",
        ["menu.behavior.grid"] = "Grid snapping",
        ["menu.behavior.guides"] = "Alignment guides",
        ["menu.behavior.workspace"] = "Workspace commands",
        ["menu.behavior.fragments"] = "Fragment commands",
        ["menu.behavior.hostExtensions"] = "Host menu extensions",
        ["menu.behavior.open"] = "Open behavior controls",
        ["menu.runtime.state"] = "View runtime state",
        ["menu.runtime.diagnostics"] = "View runtime diagnostics",
        ["menu.runtime.saveAs"] = "Save workspace as",
        ["menu.runtime.reopenLast"] = "Reopen last workspace",
        ["menu.runtime.restoreDraft"] = "Restore autosave draft",
        ["menu.extensions.candidates"] = "View plugin candidates",
        ["menu.extensions.loading"] = "View load and trust",
        ["menu.extensions.exportAllowlist"] = "Export allowlist",
        ["menu.extensions.importAllowlist"] = "Import allowlist",
        ["menu.automation.runSelection"] = "Run focus automation",
        ["menu.automation.runPlugin"] = "Run plugin automation",
        ["menu.automation.runWorkspace"] = "Run workspace automation",
        ["menu.automation.open"] = "Open automation panel",
        ["menu.integration.consumerPath"] = "View consumer path",
        ["menu.integration.standalone"] = "View standalone surfaces",
        ["menu.proof.boundary"] = "View host boundary",
        ["badge.language"] = "Language · {0}",
        ["drawer.caption"] = "Host Controls Drawer",
        ["intro.kicker"] = "Host-Owned Showcase",
        ["intro.title"] = "Host-Owned Showcase",
        ["badge.hostOwnership"] = "Host Controls",
        ["badge.runtimeOwnership"] = "Shared Runtime",
        ["badge.activeGroup"] = "Active Group · {0}",
        ["showcase.heading"] = "Capability Showcase",
        ["runtime.config.heading"] = "Host Control Snapshot",
        ["runtime.signals.heading"] = "Shared Runtime State",
        ["runtime.diagnostics.heading"] = "Recent Diagnostics",
        ["runtime.shell.heading"] = "Native Shell Workflow",
        ["extensions.candidates.heading"] = "Plugin Candidates",
        ["extensions.allowlist.heading"] = "Allowlist Decisions",
        ["extensions.loads.heading"] = "Plugin Load Snapshots",
        ["automation.request.heading"] = "Automation Request",
        ["automation.progress.heading"] = "Automation Progress",
        ["automation.result.heading"] = "Automation Result",
        ["integration.consumer.heading"] = "Consumer Path",
        ["integration.standalone.heading"] = "Standalone Surfaces",
        ["integration.presentation.heading"] = "Presenter Replacement",
        ["integration.localization.heading"] = "Localization Proof",
        ["proof.config.heading"] = "Host Control Snapshot",
        ["proof.ownership.heading"] = "Boundary Proof",
        ["proof.runtime.heading"] = "Shared Runtime Proof",
        ["proof.shell.heading"] = "Host Shell Proof",
    };

    public IReadOnlyList<DemoLanguageOption> AvailableLanguages { get; } =
    [
        ChineseLanguage,
        EnglishLanguage,
    ];

    [ObservableProperty]
    private DemoLanguageOption selectedLanguage = ChineseLanguage;

    [ObservableProperty]
    private string selectedHostMenuGroup = DemoHostMenuGroups.Showcase;

    public IReadOnlyDictionary<string, string> UiText
        => IsEnglish ? EnglishUiText : ChineseUiText;

    public bool IsEnglish => string.Equals(SelectedLanguage.Code, EnglishLanguage.Code, StringComparison.OrdinalIgnoreCase);

    public string CurrentLanguageBadgeText
        => string.Format(CultureInfo.InvariantCulture, Text("badge.language"), SelectedLanguage.DisplayName);

    public IReadOnlyList<string> LocalizationProofLines
    {
        get
        {
            var addNode = BuildLocalizedAddNodeCaption();

            return
            [
                T("当前语言：", "Current language: ") + SelectedLanguage.DisplayName,
                T("运行时菜单文案：", "Runtime menu caption: ") + addNode,
                T("检查器空状态：", "Inspector empty-state: ") + Editor.InspectorTitle,
                T("宿主覆盖优先于 plugin localization。", "Host override wins over plugin localization."),
            ];
        }
    }

    [RelayCommand]
    public void SelectLanguage(string languageCode)
        => SelectedLanguage = ResolveLanguage(languageCode);

    partial void OnSelectedLanguageChanged(DemoLanguageOption value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Editor.SetLocalizationProvider(CreateGraphLocalizationProvider(value.Code));
        UpdateCapabilities();
        OnPropertyChanged(nameof(UiText));
        OnPropertyChanged(nameof(CurrentLanguageBadgeText));
        RefreshRuntimeProjection();
    }

    partial void OnSelectedHostMenuGroupChanged(string value)
        => RefreshRuntimeProjection();

    public string SelectedHostMenuGroupTitle
        => GetHostMenuGroupTitle(SelectedHostMenuGroup);

    private static DemoLanguageOption ResolveLanguage(string? languageCode)
        => string.Equals(languageCode, EnglishLanguage.Code, StringComparison.OrdinalIgnoreCase)
            ? EnglishLanguage
            : ChineseLanguage;

    private static IGraphLocalizationProvider CreateGraphLocalizationProvider(string languageCode)
        => new DemoGraphLocalizationProvider(languageCode);

    private string GetHostMenuGroupTitle(string groupKey)
        => groupKey switch
        {
            DemoHostMenuGroups.Showcase => Text("menu.showcase"),
            DemoHostMenuGroups.View => Text("menu.view"),
            DemoHostMenuGroups.Behavior => Text("menu.behavior"),
            DemoHostMenuGroups.Runtime => Text("menu.runtime"),
            DemoHostMenuGroups.Extensions => Text("menu.extensions"),
            DemoHostMenuGroups.Automation => Text("menu.automation"),
            DemoHostMenuGroups.Integration => Text("menu.integration"),
            DemoHostMenuGroups.Proof => Text("menu.proof"),
            _ => Text("menu.showcase"),
        };

    private string NormalizeHostMenuGroup(string? groupTitleOrKey)
    {
        if (string.IsNullOrWhiteSpace(groupTitleOrKey))
        {
            return DemoHostMenuGroups.Showcase;
        }

        var candidate = groupTitleOrKey.Trim();
        return candidate switch
        {
            DemoHostMenuGroups.Showcase or "展示" or "Showcase" => DemoHostMenuGroups.Showcase,
            DemoHostMenuGroups.View or "视图" or "View" => DemoHostMenuGroups.View,
            DemoHostMenuGroups.Behavior or "行为" or "Behavior" => DemoHostMenuGroups.Behavior,
            DemoHostMenuGroups.Runtime or "运行时" or "Runtime" => DemoHostMenuGroups.Runtime,
            DemoHostMenuGroups.Extensions or "扩展" or "Extensions" => DemoHostMenuGroups.Extensions,
            DemoHostMenuGroups.Automation or "自动化" or "Automation" => DemoHostMenuGroups.Automation,
            DemoHostMenuGroups.Integration or "集成" or "Integration" => DemoHostMenuGroups.Integration,
            DemoHostMenuGroups.Proof or "证明" or "Proof" => DemoHostMenuGroups.Proof,
            _ => DemoHostMenuGroups.Showcase,
        };
    }

    private string T(string zhCn, string en)
        => IsEnglish ? en : zhCn;

    private string Text(string key)
        => UiText.TryGetValue(key, out var value) ? value : key;

    private string BuildLocalizedAddNodeCaption()
        => Editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(0, 0)))
            .Single(item => item.Id == "canvas-add-node")
            .Header;

    private sealed class DemoGraphLocalizationProvider(string languageCode) : IGraphLocalizationProvider
    {
        private static readonly IReadOnlyDictionary<string, string> ChineseValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.menu.canvas.addNode"] = "添加节点",
            ["editor.inspector.title.none"] = "请选择一个节点",
            ["editor.inspector.chrome.title"] = "检查器",
            ["editor.inspector.section.connections"] = "连接关系",
            ["editor.inspector.section.inputs"] = "输入端口",
            ["editor.inspector.section.outputs"] = "输出端口",
            ["editor.inspector.section.upstream"] = "上游依赖",
            ["editor.inspector.section.downstream"] = "下游依赖",
            ["editor.inspector.section.parameters"] = "参数编辑",
            ["editor.inspector.section.parameters.intro"] = "以下控件直接编辑当前节点参数。",
            ["editor.inspector.section.parameters.surface"] = "详细参数编辑固定在检查器中，避免遮挡节点端口和连线。",
            ["editor.inspector.section.parameters.guidance"] = "使用搜索过滤参数、折叠分组，并在需要时恢复默认值。",
            ["editor.inspector.section.parameters.search"] = "搜索参数、分组或提示信息",
        };

        private static readonly IReadOnlyDictionary<string, string> EnglishValues = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.menu.canvas.addNode"] = "Add Node",
            ["editor.inspector.title.none"] = "Select a node",
            ["editor.inspector.chrome.title"] = "Inspector",
            ["editor.inspector.section.connections"] = "Connections",
            ["editor.inspector.section.inputs"] = "Inputs",
            ["editor.inspector.section.outputs"] = "Outputs",
            ["editor.inspector.section.upstream"] = "Upstream",
            ["editor.inspector.section.downstream"] = "Downstream",
            ["editor.inspector.section.parameters"] = "Parameter Editing",
            ["editor.inspector.section.parameters.intro"] = "Use these controls to edit the current node parameters directly.",
            ["editor.inspector.section.parameters.surface"] = "Detailed parameter editing stays in the inspector so ports and edges remain visible.",
            ["editor.inspector.section.parameters.guidance"] = "Use search to filter parameters, collapse groups, and reset defaults when needed.",
            ["editor.inspector.section.parameters.search"] = "Search parameters, groups, or help",
        };

        public string GetString(string key, string fallback)
        {
            var values = string.Equals(languageCode, EnglishLanguage.Code, StringComparison.OrdinalIgnoreCase)
                ? EnglishValues
                : ChineseValues;

            return values.TryGetValue(key, out var localized) ? localized : fallback;
        }
    }
}
