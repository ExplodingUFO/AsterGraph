using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ReactFlowParityRoadmapDocsTests
{
    [Fact]
    public void ParityRoadmapDocs_RecordPhase490RepairInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 490", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #103", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-3x0", contents, StringComparison.Ordinal);
            Assert.Contains("stale-roadmap repair", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/tests only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("No Core/Editor/Avalonia runtime or public API changes", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_RecordClosedPhase485Through489Trackers()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("#93", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-3hc", contents, StringComparison.Ordinal);
            Assert.Contains("PR #94", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 485", contents, StringComparison.Ordinal);

            Assert.Contains("#95", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-0xr", contents, StringComparison.Ordinal);
            Assert.Contains("PR #96", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 486", contents, StringComparison.Ordinal);

            Assert.Contains("#97", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-i8s", contents, StringComparison.Ordinal);
            Assert.Contains("PR #98", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 487", contents, StringComparison.Ordinal);

            Assert.Contains("#99", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-ce1", contents, StringComparison.Ordinal);
            Assert.Contains("PR #100", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 488", contents, StringComparison.Ordinal);

            Assert.Contains("#101", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-6sc", contents, StringComparison.Ordinal);
            Assert.Contains("PR #102", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 489", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_RemoveStaleActiveAndCandidateCompletedWork()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.DoesNotContain("Phase 489 is the current active P2 item", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("TBD | TBD | Cookbook example architecture refresh", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("Current active branch", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("当前 active branch", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("docs/cookbook-example-architecture", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("perf/renderer-virtualization-spike`: active", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_SelectAccessibilityBreadthAsNextOpenGap()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("accessibility breadth audit", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 491", contents, StringComparison.Ordinal);
            Assert.Contains("#105", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 491 closed the accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 491 已关闭 accessibility breadth audit", chineseParity, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase491AccessibilityBreadthAuditInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 491", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #105", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-44i", contents, StringComparison.Ordinal);
            Assert.Contains("source-backed contract", contents, StringComparison.Ordinal);
            Assert.Contains("dynamic screen-reader announcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 491 closed the accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 491 已关闭 accessibility breadth audit", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase492RetainedMigrationRoadmapInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 492", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #107", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-j8v", contents, StringComparison.Ordinal);
            Assert.Contains("Retained migration removal roadmap", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("inventory now, remove later", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Retained migration removal roadmap", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 492 now owns the retained migration removal roadmap", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 492 现在负责 retained migration removal roadmap", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase493CustomNodePresenterProofInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 493", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #109", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8qv", contents, StringComparison.Ordinal);
            Assert.Contains("custom node presenter cookbook parity proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("NodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("NodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("CUSTOM_EXTENSION_SURFACE_OK", contents, StringComparison.Ordinal);
            Assert.Contains("runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Keep improving docs and samples with concrete host-owned visual presenter examples", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("用具体 host-owned visual presenter 示例继续补文档", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 493 closed the custom node presenter cookbook parity proof", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 493 已关闭 custom node presenter cookbook parity proof", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase494LocalizedShellVisualGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 494", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #111", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-p5z", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-state language/theme metadata", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("manifest-driven full-window shell", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("runtime UI changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 494 closed localized full-window shell visual gate coverage", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 494 已关闭 localized full-window shell visual gate coverage", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase495PostShellGateIssueWaveRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 495", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #113", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-wzt", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-494 roadmap refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Cookbook example architecture contract", contents, StringComparison.Ordinal);
            Assert.Contains("shell visual gate breadth", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained migration removal execution gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Core/Editor/Avalonia runtime changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/phase-495-roadmap-refresh", contents, StringComparison.Ordinal);

            Assert.DoesNotContain("| #105 | `avalonia-node-map-44i` | Phase 491", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #107 | `avalonia-node-map-j8v` | Phase 492", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #109 | `avalonia-node-map-8qv` | Phase 493", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #111 | `avalonia-node-map-p5z` | Phase 494", contents, StringComparison.Ordinal);
        }

        Assert.Contains("at least three concrete follow-up candidates", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("至少三个具体 follow-up candidates", chineseParity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase497ClosedDrawerShellVisualGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 497", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #117", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-5nl", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed", contents, StringComparison.Ordinal);
            Assert.Contains("expectedPaneOpen: false", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/phase-497-shell-closed-drawer-gate", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 497 closed the closed-drawer shell visual gate breadth slice", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 497 已通过 GitHub #117 / `avalonia-node-map-5nl` 关闭 closed-drawer shell visual gate breadth slice", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase498RetainedRemovalGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 498", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #119", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-3um", contents, StringComparison.Ordinal);
            Assert.Contains("retained migration removal execution gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("exact symbols", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("blocker tests", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("support-window", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("migration evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eng/public-api-baseline.txt", contents, StringComparison.Ordinal);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API baseline change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/phase-498-retained-removal-gate", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| TBD | TBD | Phase 498", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 498 closed the retained migration removal execution gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 498 已通过 GitHub #119 / `avalonia-node-map-3um` 关闭 retained migration removal execution gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase500SelectedRuntimeShellStateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 500", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #123", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-66t", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-closed", contents, StringComparison.Ordinal);
            Assert.Contains("expectedPaneOpen: false", contents, StringComparison.Ordinal);
            Assert.Contains("manifest/docs/tests only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("five manifest-driven full-window shell captures", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 500", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 500 closed the selected runtime shell visual gate state", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 500 已通过 GitHub #123 / `avalonia-node-map-66t` 关闭 selected runtime shell visual gate state", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase508ShellFlyoutVisualCaptureInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 508", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #139", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-2nu", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-view-menu-flyout", contents, StringComparison.Ordinal);
            Assert.Contains("PART_ViewMenu", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-flyout-state", contents, StringComparison.Ordinal);
            Assert.Contains("View menu", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("popup", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("context-menu", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 508", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 508 now owns the shell flyout visual capture", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 508 现在通过 GitHub #139 / `avalonia-node-map-2nu` 承接 shell flyout visual capture", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase509PopupVisualCaptureInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 509", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #140", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-0ff", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-host-command-tooltip-popup", contents, StringComparison.Ordinal);
            Assert.Contains("PART_HostCommand_history.undo", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-popup-state", contents, StringComparison.Ordinal);
            Assert.Contains("Nothing to undo yet.", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("context-menu", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 509", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 509 now owns the popup visual capture", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 509 现在通过 GitHub #140 / `avalonia-node-map-0ff` 承接 popup visual capture", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase510ContextMenuVisualCaptureInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 510", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #141", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8lu", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-canvas-context-menu", contents, StringComparison.Ordinal);
            Assert.Contains("PART_NodeCanvas", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-context-menu-state", contents, StringComparison.Ordinal);
            Assert.Contains("Add Node", contents, StringComparison.Ordinal);
            Assert.Contains("Fit View", contents, StringComparison.Ordinal);
            Assert.Contains("Reset View", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad context-menu coverage", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 510", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 510 now owns the context-menu visual capture", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 510 现在通过 GitHub #141 / `avalonia-node-map-8lu` 承接 context-menu visual capture", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase511BoundedLanguageThemeShellVariantsInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 511", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #142", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-9rq", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("zh-CN", contents, StringComparison.Ordinal);
            Assert.Contains("canonical-dark", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad visual/language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 511", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 511 now owns the additional language/theme shell variants", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 511 现在通过 GitHub #142 / `avalonia-node-map-9rq` 承接 additional language/theme shell variants", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase512PixelBaselineDriftMeasurementInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 512", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #143", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-1j4", contents, StringComparison.Ordinal);
            Assert.Contains("pixel-baseline drift measurement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("record-only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drift-evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("HostRuntimeDescription", contents, StringComparison.Ordinal);
            Assert.Contains("OsDescription", contents, StringComparison.Ordinal);
            Assert.Contains("ProcessArchitecture", contents, StringComparison.Ordinal);
            Assert.Contains("PngSha256", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no visual redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 512", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("Phase 512: pixel-baseline drift measurement | P3 | drift measurement docs/tests/artifact metadata | Current owned slice", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 512 closed pixel-baseline drift measurement", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 512 已通过 GitHub #143 / `avalonia-node-map-1j4` 关闭 pixel-baseline drift measurement", chineseParity, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 512 now owns pixel-baseline drift measurement", englishParity, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 512 现在通过 GitHub #143 / `avalonia-node-map-1j4` 承接 pixel-baseline drift measurement", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase513PostPhase512QueueRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 513", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #149", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-d8q", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-512 roadmap refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 506 visual queue", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phases 508-512", contents, StringComparison.Ordinal);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement | P3 | drift measurement docs/tests/artifact metadata | Current owned slice", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 513 refreshes the post-Phase-512 queue", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 513 刷新 post-Phase-512 queue", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_SelectConcretePostPhase512FollowUps()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        AssertPostPhase518Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase518Queue(ExtractIssueWaveTable(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase519PostDynamicAnnouncementRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 519", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #161", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-rs5", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-518 roadmap refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ACCESSIBILITY_DYNAMIC_ANNOUNCEMENT_CONTRACT", contents, StringComparison.Ordinal);
            Assert.Contains(".planning/", contents, StringComparison.Ordinal);
            Assert.Contains("local/ignored", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad parity or screen-reader certification claim", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 519 refreshes the post-Phase-518 queue", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 519 刷新 post-Phase-518 queue", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase523PostRetainedReadinessRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 523", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #169", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-bp0", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-522 parity issue-wave refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("built-in component parity matrix", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("MiniMap interaction and customization parity gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Controls interactivity/custom-button parity gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Background variant public surface gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Panel versus viewport-attached overlay boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard/lasso/eraser feasibility audit", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 523 refreshes the post-Phase-522 parity issue wave", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 523 刷新 post-Phase-522 parity issue wave", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase524BuiltInComponentParityMatrixInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 524", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #171", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-0k0", contents, StringComparison.Ordinal);
            Assert.Contains("built-in component parity matrix", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("MiniMap", contents, StringComparison.Ordinal);
            Assert.Contains("Controls", contents, StringComparison.Ordinal);
            Assert.Contains("Background/Grid", contents, StringComparison.Ordinal);
            Assert.Contains("Panel", contents, StringComparison.Ordinal);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertBuiltInComponentMatrix(ExtractBuiltInComponentMatrix(englishParity));
        AssertBuiltInComponentMatrix(ExtractBuiltInComponentMatrix(chineseParity));

        Assert.Contains("Phase 524 records the built-in component parity matrix", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 524 记录 built-in component parity matrix", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase525MiniMapInteractionCustomizationGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 525", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #173", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-ba7", contents, StringComparison.Ordinal);
            Assert.Contains("MiniMap interaction and customization parity gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport recentering", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport/session sync", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("custom presenter", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("factory/options customization", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("accessibility/focus boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("partial / guarded", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow MiniMap parity claim", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertMiniMapInteractionCustomizationGate(ExtractMiniMapInteractionCustomizationGate(englishParity));
        AssertMiniMapInteractionCustomizationGate(ExtractMiniMapInteractionCustomizationGate(chineseParity));

        Assert.Contains("Phase 525 records the MiniMap interaction and customization parity gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 525 记录 MiniMap interaction and customization parity gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase526ControlsInteractivityCustomButtonGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 526", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #175", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-clw", contents, StringComparison.Ordinal);
            Assert.Contains("Controls interactivity/custom-button parity gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("canonical viewport commands", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("hosted action descriptor projection", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("disabled-command recovery", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("stable button/focus boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("built-in catalog/Cookbook route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("custom button/action injection gaps", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("partial / guarded", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport.zoom-in", contents, StringComparison.Ordinal);
            Assert.Contains("viewport.zoom-out", contents, StringComparison.Ordinal);
            Assert.Contains("viewport.fit", contents, StringComparison.Ordinal);
            Assert.Contains("viewport.reset", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphHostedActionFactory.CreateCommandActions", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphControls_RendersViewportActionsAndExecutesCanonicalCommands", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphControls_DisabledCommandDescriptorsExposeTooltipRecoveryText", contents, StringComparison.Ordinal);
            Assert.Contains("PART_AsterGraphControlsZoomInButton", contents, StringComparison.Ordinal);
            Assert.Contains("PART_AsterGraphControlsZoomOutButton", contents, StringComparison.Ordinal);
            Assert.Contains("PART_AsterGraphControlsFitViewButton", contents, StringComparison.Ordinal);
            Assert.Contains("PART_AsterGraphControlsResetViewButton", contents, StringComparison.Ordinal);
            Assert.Contains("BUILTIN_STANDALONE_CONTROLS_OK", contents, StringComparison.Ordinal);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow Controls parity claim", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertControlsInteractivityCustomButtonGate(ExtractControlsInteractivityCustomButtonGate(englishParity));
        AssertControlsInteractivityCustomButtonGate(ExtractControlsInteractivityCustomButtonGate(chineseParity));

        Assert.Contains("Phase 526 records the Controls interactivity/custom-button parity gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 526 记录 Controls interactivity/custom-button parity gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase527BackgroundVariantPublicSurfaceGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 527", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #177", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-dim", contents, StringComparison.Ordinal);
            Assert.Contains("Background variant public surface gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GridBackground line-grid renderer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("CanvasStyleOptions grid tokens", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("bounded line-density behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("snap-to-grid/session command evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("built-in catalog/Cookbook route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screenshot-gate coverage", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("partial / guarded", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GridBackground.CalculateVisibleLineMetrics", contents, StringComparison.Ordinal);
            Assert.Contains("CanvasStyleOptions.GridBackgroundHex", contents, StringComparison.Ordinal);
            Assert.Contains("PrimaryGridSpacing", contents, StringComparison.Ordinal);
            Assert.Contains("SecondaryGridSpacing", contents, StringComparison.Ordinal);
            Assert.Contains("TrySnapSelectedNodesToGrid", contents, StringComparison.Ordinal);
            Assert.Contains("CalculateVisibleLineMetrics_WithExtremeZoomSpacing_KeepsLineDensityBounded", contents, StringComparison.Ordinal);
            Assert.Contains("AuthoringToolsChrome_ProjectsStockSelectionLayoutActions", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-background-grid-route", contents, StringComparison.Ordinal);
            Assert.Contains("background-grid-density", contents, StringComparison.Ordinal);
            Assert.Contains("GRID_BACKGROUND_DENSITY_OK", contents, StringComparison.Ordinal);
            Assert.Contains("cookbook-builtin-background-grid", contents, StringComparison.Ordinal);
            Assert.Contains("dots/lines/cross variants", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("gap/size public API policy", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("background graph indexing", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("second renderer/new layout runtime", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow Background parity claim", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertBackgroundVariantPublicSurfaceGate(ExtractBackgroundVariantPublicSurfaceGate(englishParity));
        AssertBackgroundVariantPublicSurfaceGate(ExtractBackgroundVariantPublicSurfaceGate(chineseParity));

        Assert.Contains("Phase 527 records the Background variant public surface gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 527 记录 Background variant public surface gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase528PanelViewportAttachedOverlayBoundaryInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 528", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #179", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-9ow", contents, StringComparison.Ordinal);
            Assert.Contains("Panel versus viewport-attached overlay boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("standalone `AsterGraphPanel`", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPanelPosition", contents, StringComparison.Ordinal);
            Assert.Contains("TopLeft", contents, StringComparison.Ordinal);
            Assert.Contains("TopCenter", contents, StringComparison.Ordinal);
            Assert.Contains("TopRight", contents, StringComparison.Ordinal);
            Assert.Contains("CenterLeft", contents, StringComparison.Ordinal);
            Assert.Contains("Center", contents, StringComparison.Ordinal);
            Assert.Contains("CenterRight", contents, StringComparison.Ordinal);
            Assert.Contains("BottomLeft", contents, StringComparison.Ordinal);
            Assert.Contains("BottomCenter", contents, StringComparison.Ordinal);
            Assert.Contains("BottomRight", contents, StringComparison.Ordinal);
            Assert.Contains("Offset", contents, StringComparison.Ordinal);
            Assert.Contains("Padding", contents, StringComparison.Ordinal);
            Assert.Contains("CornerRadius", contents, StringComparison.Ordinal);
            Assert.Contains("host-owned content composition", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("focus boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("AsterGraphPanel_ArrangesContentAtRequestedOverlayPosition", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPanel_DefaultsToNonFocusableOverlayContainerAndPreservesFocusableContent", contents, StringComparison.Ordinal);
            Assert.Contains("builtin-standalone-panel-route", contents, StringComparison.Ordinal);
            Assert.Contains("standalone-panel", contents, StringComparison.Ordinal);
            Assert.Contains("cookbook-builtin-standalone-panel", contents, StringComparison.Ordinal);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow Panel parity claim", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no new viewport runtime", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no panel persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no remote sync", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no shell dependency", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no workflow engine", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 528: Panel versus viewport-attached overlay boundary", contents, StringComparison.Ordinal);
        }

        AssertPanelViewportAttachedOverlayBoundary(ExtractPanelViewportAttachedOverlayBoundary(englishParity));
        AssertPanelViewportAttachedOverlayBoundary(ExtractPanelViewportAttachedOverlayBoundary(chineseParity));

        Assert.Contains("Phase 528 records the Panel versus viewport-attached overlay boundary", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 528 记录 Panel versus viewport-attached overlay boundary", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase529WhiteboardLassoEraserFeasibilityAuditInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 529", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #181", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-jrm", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard/lasso/eraser feasibility audit", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("React Flow whiteboard", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Freehand draw", contents, StringComparison.Ordinal);
            Assert.Contains("Lasso selection", contents, StringComparison.Ordinal);
            Assert.Contains("Eraser", contents, StringComparison.Ordinal);
            Assert.Contains("Rectangle draw", contents, StringComparison.Ordinal);
            Assert.Contains("GetSelectionRectangleSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("UpdateMarqueeSelection", contents, StringComparison.Ordinal);
            Assert.Contains("selection-marquee-workbench", contents, StringComparison.Ordinal);
            Assert.Contains("interaction-selection-marquee-route", contents, StringComparison.Ordinal);
            Assert.Contains("v079-selection-rectangle-route", contents, StringComparison.Ordinal);
            Assert.Contains("selection.select-all", contents, StringComparison.Ordinal);
            Assert.Contains("selection.select-none", contents, StringComparison.Ordinal);
            Assert.Contains("selection.invert", contents, StringComparison.Ordinal);
            Assert.Contains("selection.delete", contents, StringComparison.Ordinal);
            Assert.Contains("selection.transform.move", contents, StringComparison.Ordinal);
            Assert.Contains("lasso/freehand selection", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser tool", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("rectangle/freehand drawing", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pointer-mode state machine", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drawing persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer layer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("hit-testing", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime UI behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no whiteboard implementation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 529: whiteboard/lasso/eraser feasibility audit", contents, StringComparison.Ordinal);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));

        Assert.Contains("Phase 529 records the whiteboard/lasso/eraser feasibility audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 529 记录 whiteboard/lasso/eraser feasibility audit", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase530LassoSelectionQueryContractInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 530", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #183", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8um", contents, StringComparison.Ordinal);
            Assert.Contains("lasso/freehand selection query contract", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GetSelectionLassoSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorSelectionLassoSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("center-point", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("endpoint", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pointer-mode state", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("gesture capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict visual-baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase531AvaloniaLassoSelectionBridgeInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 531", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #185", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-084", contents, StringComparison.Ordinal);
            Assert.Contains("Avalonia lasso selection bridge", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screen-space lasso points", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("world-space points", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GetSelectionLassoSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("Shift union", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Ctrl toggle", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("UpdateLassoSelection_WithFinalizeTrue_UsesBackendSelectionLassoQuery", contents, StringComparison.Ordinal);
            Assert.Contains("LassoSelection_RoutesThroughCanvasBridge_AndSelectsContainedNodes", contents, StringComparison.Ordinal);
            Assert.Contains("public pointer-mode UI", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("visual gesture capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso screenshot route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase532InternalLassoGestureCaptureInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 532", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #187", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-mka", contents, StringComparison.Ordinal);
            Assert.Contains("internal lasso gesture capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("NodeCanvasSelectionGestureKind.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("TryBeginLassoSelection_WithLassoGestureKind_RecordsStartAndMoveAfterThreshold", contents, StringComparison.Ordinal);
            Assert.Contains("HandleMoved_WhenCanvasSelectionUsesLassoGesture_RecordsLassoPointAndSkipsMarqueeUpdate", contents, StringComparison.Ordinal);
            Assert.Contains("HandleReleased_AfterLassoSelection_FinalizesLassoSelectionAndResetsSession", contents, StringComparison.Ordinal);
            Assert.Contains("UpdateLassoSelection", contents, StringComparison.Ordinal);
            Assert.Contains("marquee remains the default", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("public pointer-mode UI", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("visual gesture capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso screenshot route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase533PublicLassoPointerModeActivationInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 533", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #189", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-cxe", contents, StringComparison.Ordinal);
            Assert.Contains("public lasso pointer-mode activation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("NodeCanvasSelectionMode.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("LassoSelectionMode_RoutesThroughCanvasPointerHandlers_AndSelectsContainedNodes", contents, StringComparison.Ordinal);
            Assert.Contains("marquee remains the default", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("visual gesture capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso screenshot route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase534LassoVisualGestureFeedbackRouteInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 534", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #191", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-lzy", contents, StringComparison.Ordinal);
            Assert.Contains("lasso visual gesture feedback route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("transient lasso path", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("UpdateLassoFeedback", contents, StringComparison.Ordinal);
            Assert.Contains("ClearLassoFeedback", contents, StringComparison.Ordinal);
            Assert.Contains("LassoSelectionMode_RendersTransientFeedbackPathOnlyDuringDrag", contents, StringComparison.Ordinal);
            Assert.Contains("toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso screenshot route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase535PostLassoQueueRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 535", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #193", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8l6", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-534 parity queue refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso screenshot proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("lasso toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser behavior/API feasibility", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("rectangle/freehand drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard persistence/render-layer readiness", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no whiteboard implementation", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertPostPhase534Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase534Queue(ExtractIssueWaveTable(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase536LassoScreenshotProofBoundaryInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 536", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #195", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-uvd", contents, StringComparison.Ordinal);
            Assert.Contains("lasso screenshot route and Cookbook proof boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("cookbook-interaction-lasso-screenshot-proof", contents, StringComparison.Ordinal);
            Assert.Contains("interaction-lasso-screenshot-proof-route", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-lasso-screenshot-proof", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-lasso-state", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasSelectionMode.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("LassoSelectionMode_RendersTransientFeedbackPathOnlyDuringDrag", contents, StringComparison.Ordinal);
            Assert.Contains("LASSO_SCREENSHOT_PROOF_BOUNDARY_OK", contents, StringComparison.Ordinal);
            Assert.Contains("toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer rewrite", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baseline", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("full whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertPostPhase534Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase534Queue(ExtractIssueWaveTable(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase537LassoToolbarErgonomicsInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 537", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #197", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-w9h", contents, StringComparison.Ordinal);
            Assert.Contains("lasso toolbar UX and public activation ergonomics boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("CreatePointerSelectionModeActions", contents, StringComparison.Ordinal);
            Assert.Contains("pointer-mode.lasso-selection", contents, StringComparison.Ordinal);
            Assert.Contains("PART_PointerModeLassoButton", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvas.SelectionMode", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasSelectionMode.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("HostedActions_PointerModeActionsSwitchNodeCanvasSelectionModeWithoutRuntimeCommandRoute", contents, StringComparison.Ordinal);
            Assert.Contains("AuthoringToolsChrome_ProjectsPointerModeActionsThroughNodeCanvasSelectionMode", contents, StringComparison.Ordinal);
            Assert.Contains("no eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer rewrite", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertPostPhase537Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase537Queue(ExtractIssueWaveTable(chineseParity));
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase538EraserBehaviorApiFeasibilityGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 538", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #199", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-a3w", contents, StringComparison.Ordinal);
            Assert.Contains("eraser behavior/API feasibility gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("selection.delete", contents, StringComparison.Ordinal);
            Assert.Contains("Delete Selection", contents, StringComparison.Ordinal);
            Assert.Contains("selection-delete", contents, StringComparison.Ordinal);
            Assert.Contains("graph-selection deletion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not an eraser cursor", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no eraser cursor", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no collision trail", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no eraser-specific hit-testing", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer rewrite", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baseline", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        AssertPostPhase538Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase538Queue(ExtractIssueWaveTable(chineseParity));
        AssertPhase538WorktreePlan(ExtractRecommendedWorktreePlan(englishParity));
        AssertPhase538WorktreePlan(ExtractRecommendedWorktreePlan(chineseParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(englishParity));
        AssertWhiteboardLassoEraserFeasibilityAudit(ExtractWhiteboardLassoEraserFeasibilityAudit(chineseParity));
    }

    [Fact]
    public void AuthoringSurfaceRecipeDocs_SurfacePhase537PointerModeToolbarRoute()
    {
        var englishRecipe = ReadRepoFile("docs/en/authoring-surface-recipe.md");
        var chineseRecipe = ReadRepoFile("docs/zh-CN/authoring-surface-recipe.md");

        foreach (var contents in new[] { englishRecipe, chineseRecipe })
        {
            Assert.Contains("CreatePointerSelectionModeActions", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvas.SelectionMode", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasSelectionMode.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("selection model", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase539DrawingPrimitiveModelGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 539", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #201", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-rwr", contents, StringComparison.Ordinal);
            Assert.Contains("rectangle/freehand drawing primitive model gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GraphDocument", contents, StringComparison.Ordinal);
            Assert.Contains("GraphNode", contents, StringComparison.Ordinal);
            Assert.Contains("GraphConnection", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorSelectionRectangleSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorSelectionLassoSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("model identity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("geometry", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("style/brush state", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("hit-testing/edit lifecycle", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer projection", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Cookbook/screenshot evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no Avalonia pointer coordinator edits", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer-layer changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence/schema changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("rectangle/freehand drawing parity is supported", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Rectangle draw parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Freehand draw parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 539: rectangle/freehand drawing primitive model gate", contents, StringComparison.Ordinal);
        }

        AssertPostPhase539Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase539Queue(ExtractIssueWaveTable(chineseParity));

        Assert.Contains("Phase 539 records the rectangle/freehand drawing primitive model gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 539 记录 rectangle/freehand drawing primitive model gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase540WhiteboardPersistenceRenderGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 540", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #203", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-91b", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard persistence and render-layer readiness gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GraphDocumentSerializer", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocumentCompatibility", contents, StringComparison.Ordinal);
            Assert.Contains("CurrentSchemaVersion", contents, StringComparison.Ordinal);
            Assert.Contains("GraphWorkspaceService", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorSceneSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasConnectionSceneRenderer", contents, StringComparison.Ordinal);
            Assert.Contains("CookbookScreenshotGateRoutes.json", contents, StringComparison.Ordinal);
            Assert.Contains("CookbookShellVisualGateStates.json", contents, StringComparison.Ordinal);
            Assert.Contains("persistence/schema contract", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer projection contract", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("hit-testing/edit lifecycle", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screenshot/Cookbook evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("migration policy", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard annotation persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard annotation rendering", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence/schema changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer-layer changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("whiteboard persistence parity is supported", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("render-layer parity is supported", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("whiteboard annotation persistence is implemented", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 540: whiteboard persistence and render-layer readiness gate", contents, StringComparison.Ordinal);
        }

        AssertPostPhase540Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase540Queue(ExtractIssueWaveTable(chineseParity));

        Assert.Contains("Phase 540 records the whiteboard persistence and render-layer readiness gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 540 记录 whiteboard persistence and render-layer readiness gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase541PostPhase540WhiteboardWaveSplitInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 541", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #205", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-2z1", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-540 whiteboard implementation wave split", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 542", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #206", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-b31", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard primitive core model contract gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 543", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #207", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-aj8", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard renderer projection and hit-testing proof gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 544", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #208", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-32n", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard primitive persistence schema policy gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 545", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #209", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-7ns", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard Cookbook and screenshot proof route gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("model identity/geometry/style contract", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer projection and hit-testing proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence/schema policy", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Cookbook/screenshot proof route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no model/schema changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer-layer changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 542", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| TBD | TBD | Phase 543", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| TBD | TBD | Phase 544", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| TBD | TBD | Phase 545", contents, StringComparison.Ordinal);
        }

        AssertPostPhase534Queue(ExtractIssueWaveTable(englishParity));
        AssertPostPhase534Queue(ExtractIssueWaveTable(chineseParity));

        Assert.Contains("Phase 541 records the post-Phase-540 whiteboard implementation wave split", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 541 记录 post-Phase-540 whiteboard implementation wave split", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase542WhiteboardPrimitiveCoreModelContractGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 542", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #206", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-b31", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard primitive core model contract gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("WHITEBOARD_PRIMITIVE_CORE_MODEL_CONTRACT_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocument, GraphNode, GraphConnection, and GraphNodeGroup remain graph-scene models", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("whiteboard primitives remain separate annotation concepts", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("primitive identity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("primitive kind", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("geometry envelope", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("style/brush state", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("edit lifecycle metadata", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer-neutral", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("persistence-neutral", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pointer-neutral", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no production model type", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime UI behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer work", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence/schema migration", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 542 records the whiteboard primitive core model contract gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 542 记录 whiteboard primitive core model contract gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase543WhiteboardRendererHitTestingProofGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 543", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #207", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-aj8", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard renderer projection and hit-testing proof gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("WHITEBOARD_RENDERER_HIT_TEST_PROOF_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorSceneSnapshot", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasConnectionSceneRenderer", contents, StringComparison.Ordinal);
            Assert.Contains("adapter-neutral scene snapshot", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Avalonia renderer seam", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("primitive projection proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("hit-testing proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("edit lifecycle proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("without a renderer rewrite", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer rewrite", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no new persistence/schema behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public drawing API", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no toolbar/tool activation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no eraser behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 543 records the whiteboard renderer projection and hit-testing proof gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 543 记录 whiteboard renderer projection and hit-testing proof gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase544WhiteboardPrimitivePersistenceSchemaPolicyGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 544", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #208", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-32n", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard primitive persistence schema policy gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("WHITEBOARD_PRIMITIVE_PERSISTENCE_SCHEMA_POLICY_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocumentSerializer", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocumentCompatibility", contents, StringComparison.Ordinal);
            Assert.Contains("CurrentSchemaVersion", contents, StringComparison.Ordinal);
            Assert.Contains("`GraphDocument` schema", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("separate annotation surface", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("workspace persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("clipboard fragments", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screenshot artifacts", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("schema version", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("migration policy", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("compatibility test", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no schema version changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime UI behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer work", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public drawing API", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no toolbar/tool activation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no eraser behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("whiteboard primitive persistence is implemented", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GraphDocument schema now stores whiteboard primitives", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 544 records the whiteboard primitive persistence schema policy gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 544 记录 whiteboard primitive persistence schema policy gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase545WhiteboardCookbookScreenshotProofRouteGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 545", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #209", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-7ns", contents, StringComparison.Ordinal);
            Assert.Contains("whiteboard Cookbook and screenshot proof route gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("WHITEBOARD_COOKBOOK_SCREENSHOT_PROOF_ROUTE_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("Cookbook route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("shell state", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("screenshot metadata", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("before/after visual evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("non-overlap requirements", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("model, renderer, and persistence gates", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no screenshot manifest expansion", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime UI behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no drawing tool implementation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no renderer changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence/schema changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no pointer coordinator edits", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full React Flow whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("whiteboard screenshot proof route is implemented", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Cookbook screenshot manifest now includes whiteboard primitives", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 545 records the whiteboard Cookbook and screenshot proof route gate", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 545 记录 whiteboard Cookbook and screenshot proof route gate", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase501PostPhase500QueueRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 501", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #125", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-38n", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-500 parity follow-up queue", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("renderer virtualization execution proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("declarative API ergonomics", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("layout provider evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("manual assistive-technology validation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 501: choose next bounded parity follow-up", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 501 closed the post-Phase-500 parity follow-up queue refresh", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 501 已通过 GitHub #125 / `avalonia-node-map-38n` 关闭 post-Phase-500 parity follow-up queue refresh", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase502RendererVirtualizationExecutionProofInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 502", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #127", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-mai", contents, StringComparison.Ordinal);
            Assert.Contains("renderer virtualization execution proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("focused renderer tests", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("scale docs", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("proof command", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("artifact metadata", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport-budgeted scene projection/rendering", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("true renderer virtualization", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("non-informational renderer thresholds", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 502: renderer virtualization execution proof", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 502 closed the renderer virtualization execution proof contract", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 502 已通过 GitHub #127 / `avalonia-node-map-mai` 关闭 renderer virtualization execution proof contract", chineseParity, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 502 now owns the renderer virtualization execution proof", englishParity, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 502 现在通过 GitHub #127 / `avalonia-node-map-mai` 承接 renderer virtualization execution proof", chineseParity, StringComparison.Ordinal);
    }

    private static void AssertPostPhase518Queue(string table)
    {
        AssertPostPhase534Queue(table);
        Assert.Contains("P2", table, StringComparison.Ordinal);
        Assert.Contains("P3", table, StringComparison.Ordinal);
        Assert.Contains("P4", table, StringComparison.Ordinal);
        Assert.Contains("parity roadmap docs", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("focused docs tests", table, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            table.Contains("Blocks the next implementation wave", StringComparison.OrdinalIgnoreCase)
            || table.Contains("阻塞下一批 implementation wave", StringComparison.OrdinalIgnoreCase));
        Assert.True(
            table.Contains("in parallel", StringComparison.OrdinalIgnoreCase)
            || table.Contains("可与", StringComparison.OrdinalIgnoreCase)
            || table.Contains("Current stacked drawing model gate", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("Current docs/API-policy slice", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ready after Phase 519", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| #149 | `avalonia-node-map-d8q` | Phase 513: post-Phase-512 roadmap refresh", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #150 | `avalonia-node-map-ien` | Phase 514: execute renderer virtualization proof harness", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #151 | `avalonia-node-map-t44` | Phase 515: decide strict pixel baseline policy from drift evidence", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #152 | `avalonia-node-map-821` | Phase 516: record manual assistive-technology validation evidence", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #161 | `avalonia-node-map-rs5` | Phase 519: refresh parity roadmap after dynamic announcement proof", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #162 | `avalonia-node-map-vdc` | Phase 520: define declarative host composition API gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #163 | `avalonia-node-map-ayx` | Phase 521: define strict pixel-baseline comparator readiness gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #164 | `avalonia-node-map-ecx` | Phase 522: audit retained migration removal readiness", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement | P3 | drift measurement docs/tests/artifact metadata | Current owned slice", table, StringComparison.Ordinal);
    }

    private static void AssertPostPhase534Queue(string table)
    {
        Assert.Contains("| #193 | `avalonia-node-map-8l6` | Phase 535: refresh post-lasso visual feedback parity queue", table, StringComparison.Ordinal);
        Assert.Contains("| #195 | `avalonia-node-map-uvd` | Phase 536: lasso screenshot route and Cookbook proof boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.True(
            table.Contains("TBD", StringComparison.Ordinal)
            || table.Contains("| #203 | `avalonia-node-map-91b` | Phase 540: whiteboard persistence and render-layer readiness gate", StringComparison.Ordinal));
        Assert.Contains("Phase 541: post-Phase-540 whiteboard implementation wave split", table, StringComparison.Ordinal);
        Assert.Contains("Phase 542: whiteboard primitive core model contract gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 543: whiteboard renderer projection and hit-testing proof gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 544: whiteboard primitive persistence schema policy gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 545: whiteboard Cookbook and screenshot proof route gate", table, StringComparison.Ordinal);
        Assert.Contains("Cookbook screenshot manifest", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hosted authoring tools", table, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            table.Contains("editor selection/delete commands", StringComparison.OrdinalIgnoreCase)
            || table.Contains("graph-selection deletion evidence", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Core/Editor model contract", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence/schema planning", table, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            table.Contains("Blocks the next implementation wave", StringComparison.OrdinalIgnoreCase)
            || table.Contains("阻塞下一批 implementation wave", StringComparison.OrdinalIgnoreCase));
        Assert.True(
            table.Contains("Can run after Phase 535 in parallel", StringComparison.OrdinalIgnoreCase)
            || table.Contains("Phase 535 后可与", StringComparison.OrdinalIgnoreCase)
            || table.Contains("Blocks lasso toolbar work", StringComparison.OrdinalIgnoreCase)
            || table.Contains("进入 lasso toolbar work", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("| #191 | `avalonia-node-map-lzy` | Phase 534: lasso visual gesture feedback route", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Phase 536: lasso screenshot route and Cookbook proof boundary", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Current Avalonia visual feedback slice", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("当前 Avalonia overlay feedback slice", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertPostPhase537Queue(string table)
    {
        Assert.Contains("| #197 | `avalonia-node-map-w9h` | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.True(
            table.Contains("TBD", StringComparison.Ordinal)
            || table.Contains("| #203 | `avalonia-node-map-91b` | Phase 540: whiteboard persistence and render-layer readiness gate", StringComparison.Ordinal));
        Assert.Contains("hosted authoring tools", table, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            table.Contains("editor selection/delete commands", StringComparison.OrdinalIgnoreCase)
            || table.Contains("graph-selection deletion evidence", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Core/Editor model contract", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence/schema planning", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| TBD | TBD | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
    }

    private static void AssertPostPhase538Queue(string table)
    {
        Assert.Contains("| #197 | `avalonia-node-map-w9h` | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.Contains("| #199 | `avalonia-node-map-a3w` | Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.Contains("| #201 | `avalonia-node-map-rwr` | Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.True(
            table.Contains("TBD", StringComparison.Ordinal)
            || table.Contains("| #203 | `avalonia-node-map-91b` | Phase 540: whiteboard persistence and render-layer readiness gate", StringComparison.Ordinal));
        Assert.Contains("graph-selection deletion evidence", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Core/Editor model contract", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence/schema planning", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| TBD | TBD | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Current toolbar ergonomics slice", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertPostPhase539Queue(string table)
    {
        Assert.Contains("| #197 | `avalonia-node-map-w9h` | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.Contains("| #199 | `avalonia-node-map-a3w` | Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.Contains("| #201 | `avalonia-node-map-rwr` | Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.Contains("Current stacked drawing model gate", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("graph-selection deletion evidence", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Core/Editor model contract", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("persistence/schema planning", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| TBD | TBD | Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
    }

    private static void AssertPostPhase540Queue(string table)
    {
        AssertPostPhase539Queue(table);
        Assert.Contains("| #203 | `avalonia-node-map-91b` | Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.Contains("Stacked after Phase 539", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("readiness criteria", table, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("| TBD | TBD | Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
    }

    private static void AssertPhase538WorktreePlan(string plan)
    {
        Assert.Contains("feature/phase-537-lasso-toolbar-ergonomics", plan, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-w9h", plan, StringComparison.Ordinal);
        Assert.True(
            plan.Contains("merged worktree", StringComparison.OrdinalIgnoreCase)
            || plan.Contains("已合并", StringComparison.Ordinal),
            "Expected Phase 537 worktree plan entry to be marked merged.");
        Assert.Contains("feature/phase-538-eraser-feasibility", plan, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-a3w", plan, StringComparison.Ordinal);
        Assert.True(
            plan.Contains("current worktree", StringComparison.OrdinalIgnoreCase)
            || plan.Contains("当前 worktree", StringComparison.Ordinal),
            "Expected Phase 538 worktree plan entry to be marked current.");
        Assert.Contains("graph-selection deletion evidence", plan, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("feature/phase-538-eraser-feasibility`: future candidate", plan, StringComparison.Ordinal);
    }

    private static void AssertBuiltInComponentMatrix(string table)
    {
        Assert.Contains("| Built-in |", table, StringComparison.Ordinal);
        Assert.Contains("| MiniMap | Partial / guarded | `GraphMiniMap` + `AsterGraphMiniMapViewFactory.Create(...)` |", table, StringComparison.Ordinal);
        Assert.Contains("| Controls | Partial / guarded | `AsterGraphControls` |", table, StringComparison.Ordinal);
        Assert.Contains("| Background/Grid | Partial / guarded | `GridBackground` + style grid options |", table, StringComparison.Ordinal);
        Assert.Contains("| Panel | Partial / guarded | `AsterGraphPanel` + `AsterGraphPanelPosition` |", table, StringComparison.Ordinal);
        Assert.Contains("GraphMiniMapStandaloneTests", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphBuiltInControlsTests", table, StringComparison.Ordinal);
        Assert.Contains("GridBackgroundTests", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphBuiltInPanelTests", table, StringComparison.Ordinal);
        Assert.Contains("minimap-workbench", table, StringComparison.Ordinal);
        Assert.Contains("hosted-controls-panel", table, StringComparison.Ordinal);
        Assert.Contains("background-grid-density", table, StringComparison.Ordinal);
        Assert.Contains("standalone-panel", table, StringComparison.Ordinal);
        Assert.Contains("Phase 525", table, StringComparison.Ordinal);
        Assert.Contains("Phase 526", table, StringComparison.Ordinal);
        Assert.Contains("Phase 527", table, StringComparison.Ordinal);
        Assert.Contains("Phase 528", table, StringComparison.Ordinal);
        Assert.Contains("not full React Flow built-in parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertMiniMapInteractionCustomizationGate(string table)
    {
        Assert.Contains("| MiniMap gate |", table, StringComparison.Ordinal);
        Assert.Contains("| Viewport recentering | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Viewport/session sync | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Custom presenter/render behavior | Supported seam / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Factory/options customization | Supported seam / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Accessibility/focus boundary | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("StandaloneMiniMap_RecenterViewport_ForDifferentMiniMapPoints", table, StringComparison.Ordinal);
        Assert.Contains("HostedMiniMap_BalancedModeInvalidatesOnViewportChange", table, StringComparison.Ordinal);
        Assert.Contains("StandaloneMiniMap_CustomPresenter_RecenterViewportThroughEditorApi", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphMiniMapViewOptions", table, StringComparison.Ordinal);
        Assert.Contains("StandaloneMiniMap_StockSurfaceStaysOutOfKeyboardFocusPath", table, StringComparison.Ordinal);
        Assert.Contains("not full React Flow MiniMap parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertControlsInteractivityCustomButtonGate(string table)
    {
        Assert.Contains("| Controls gate |", table, StringComparison.Ordinal);
        Assert.Contains("| Canonical viewport commands | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Hosted action descriptor projection | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Disabled-command recovery | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Stable button/focus boundary | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Built-in catalog/Cookbook route | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Custom button/action injection gaps | Gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("viewport.zoom-in", table, StringComparison.Ordinal);
        Assert.Contains("viewport.zoom-out", table, StringComparison.Ordinal);
        Assert.Contains("viewport.fit", table, StringComparison.Ordinal);
        Assert.Contains("viewport.reset", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphHostedActionFactory.CreateCommandActions", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphControls_RendersViewportActionsAndExecutesCanonicalCommands", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphControls_DisabledCommandDescriptorsExposeTooltipRecoveryText", table, StringComparison.Ordinal);
        Assert.Contains("PART_AsterGraphControlsZoomInButton", table, StringComparison.Ordinal);
        Assert.Contains("PART_AsterGraphControlsZoomOutButton", table, StringComparison.Ordinal);
        Assert.Contains("PART_AsterGraphControlsFitViewButton", table, StringComparison.Ordinal);
        Assert.Contains("PART_AsterGraphControlsResetViewButton", table, StringComparison.Ordinal);
        Assert.Contains("BUILTIN_STANDALONE_CONTROLS_OK", table, StringComparison.Ordinal);
        Assert.Contains("not full React Flow Controls parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertBackgroundVariantPublicSurfaceGate(string table)
    {
        Assert.Contains("| Background gate |", table, StringComparison.Ordinal);
        Assert.Contains("| GridBackground line-grid renderer | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| CanvasStyleOptions grid tokens | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Bounded line-density behavior | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Snap-to-grid/session command evidence | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Built-in catalog/Cookbook route | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Screenshot-gate coverage | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Dots/lines/cross variants and gap/size API | Gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("GridBackground.CalculateVisibleLineMetrics", table, StringComparison.Ordinal);
        Assert.Contains("CanvasStyleOptions.GridBackgroundHex", table, StringComparison.Ordinal);
        Assert.Contains("PrimaryGridSpacing", table, StringComparison.Ordinal);
        Assert.Contains("SecondaryGridSpacing", table, StringComparison.Ordinal);
        Assert.Contains("TrySnapSelectedNodesToGrid", table, StringComparison.Ordinal);
        Assert.Contains("CalculateVisibleLineMetrics_WithExtremeZoomSpacing_KeepsLineDensityBounded", table, StringComparison.Ordinal);
        Assert.Contains("AuthoringToolsChrome_ProjectsStockSelectionLayoutActions", table, StringComparison.Ordinal);
        Assert.Contains("builtin-background-grid-route", table, StringComparison.Ordinal);
        Assert.Contains("background-grid-density", table, StringComparison.Ordinal);
        Assert.Contains("GRID_BACKGROUND_DENSITY_OK", table, StringComparison.Ordinal);
        Assert.Contains("cookbook-builtin-background-grid", table, StringComparison.Ordinal);
        Assert.Contains("not full React Flow Background parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertPanelViewportAttachedOverlayBoundary(string table)
    {
        Assert.Contains("| Panel gate |", table, StringComparison.Ordinal);
        Assert.Contains("| Public standalone overlay primitive | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Placement enum and properties | Supported / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Host-owned content composition | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Focus boundary | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Built-in catalog/Cookbook route | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Screenshot-gate coverage | Guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Viewport-attached overlay parity | Gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphPanel", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphPanelPosition", table, StringComparison.Ordinal);
        Assert.Contains("TopLeft", table, StringComparison.Ordinal);
        Assert.Contains("TopCenter", table, StringComparison.Ordinal);
        Assert.Contains("TopRight", table, StringComparison.Ordinal);
        Assert.Contains("CenterLeft", table, StringComparison.Ordinal);
        Assert.Contains("Center", table, StringComparison.Ordinal);
        Assert.Contains("CenterRight", table, StringComparison.Ordinal);
        Assert.Contains("BottomLeft", table, StringComparison.Ordinal);
        Assert.Contains("BottomCenter", table, StringComparison.Ordinal);
        Assert.Contains("BottomRight", table, StringComparison.Ordinal);
        Assert.Contains("Position", table, StringComparison.Ordinal);
        Assert.Contains("Offset", table, StringComparison.Ordinal);
        Assert.Contains("Padding", table, StringComparison.Ordinal);
        Assert.Contains("CornerRadius", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphPanel_ArrangesContentAtRequestedOverlayPosition", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphPanel_DefaultsToNonFocusableOverlayContainerAndPreservesFocusableContent", table, StringComparison.Ordinal);
        Assert.Contains("builtin-standalone-panel-route", table, StringComparison.Ordinal);
        Assert.Contains("standalone-panel", table, StringComparison.Ordinal);
        Assert.Contains("cookbook-builtin-standalone-panel", table, StringComparison.Ordinal);
        Assert.Contains("panel persistence", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remote sync", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shell dependency", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("workflow engine", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not full React Flow Panel parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertWhiteboardLassoEraserFeasibilityAudit(string table)
    {
        Assert.Contains("| Whiteboard gate |", table, StringComparison.Ordinal);
        Assert.Contains("| React Flow whiteboard reference | Gap-scoping reference |", table, StringComparison.Ordinal);
        Assert.Contains("| Rectangle marquee selection | Present / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Multi-select and command projection | Present / guarded |", table, StringComparison.Ordinal);
        Assert.Contains("| Lasso/freehand selection | Public Avalonia lasso selection mode with hosted toolbar activation and transient visual feedback / whiteboard gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("| Eraser tool | Feasibility gate recorded / gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("| Rectangle/freehand drawing | Model gate recorded / gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("| Whiteboard persistence/render layer | Readiness gate recorded / gap retained |", table, StringComparison.Ordinal);
        Assert.Contains("https://reactflow.dev/learn/advanced-use/whiteboard", table, StringComparison.Ordinal);
        Assert.Contains("Freehand draw", table, StringComparison.Ordinal);
        Assert.Contains("Lasso selection", table, StringComparison.Ordinal);
        Assert.Contains("Eraser", table, StringComparison.Ordinal);
        Assert.Contains("Rectangle draw", table, StringComparison.Ordinal);
        Assert.Contains("GetSelectionRectangleSnapshot", table, StringComparison.Ordinal);
        Assert.Contains("GetSelectionLassoSnapshot", table, StringComparison.Ordinal);
        Assert.Contains("GraphEditorSelectionLassoSnapshot", table, StringComparison.Ordinal);
        Assert.Contains("UpdateMarqueeSelection", table, StringComparison.Ordinal);
        Assert.Contains("Queries_GetSelectionRectangleSnapshot_ReturnsNodesAndConnectionsInRectangle", table, StringComparison.Ordinal);
        Assert.Contains("Queries_GetSelectionLassoSnapshot_ReturnsCenterContainedNodesAndConnections", table, StringComparison.Ordinal);
        Assert.Contains("Queries_GetSelectionLassoSnapshot_UsesNodeCenterInsteadOfBoundsIntersection", table, StringComparison.Ordinal);
        Assert.Contains("Queries_GetSelectionLassoSnapshot_WithOpenOrDegeneratePath_ReturnsDeterministicResults", table, StringComparison.Ordinal);
        Assert.Contains("UpdateMarqueeSelection_WithFinalizeTrue_UsesBackendSelectionRectangleQuery", table, StringComparison.Ordinal);
        Assert.Contains("UpdateLassoSelection_WithFinalizeTrue_UsesBackendSelectionLassoQuery", table, StringComparison.Ordinal);
        Assert.Contains("LassoSelection_RoutesThroughCanvasBridge_AndSelectsContainedNodes", table, StringComparison.Ordinal);
        Assert.Contains("NodeCanvasSelectionGestureKind.Lasso", table, StringComparison.Ordinal);
        Assert.Contains("TryBeginLassoSelection_WithLassoGestureKind_RecordsStartAndMoveAfterThreshold", table, StringComparison.Ordinal);
        Assert.Contains("HandleMoved_WhenCanvasSelectionUsesLassoGesture_RecordsLassoPointAndSkipsMarqueeUpdate", table, StringComparison.Ordinal);
        Assert.Contains("HandleReleased_AfterLassoSelection_FinalizesLassoSelectionAndResetsSession", table, StringComparison.Ordinal);
        Assert.Contains("NodeCanvasSelectionMode.Lasso", table, StringComparison.Ordinal);
        Assert.Contains("LassoSelectionMode_RoutesThroughCanvasPointerHandlers_AndSelectsContainedNodes", table, StringComparison.Ordinal);
        Assert.Contains("UpdateLassoFeedback", table, StringComparison.Ordinal);
        Assert.Contains("ClearLassoFeedback", table, StringComparison.Ordinal);
        Assert.Contains("LassoSelectionMode_RendersTransientFeedbackPathOnlyDuringDrag", table, StringComparison.Ordinal);
        Assert.Contains("CreatePointerSelectionModeActions", table, StringComparison.Ordinal);
        Assert.Contains("pointer-mode.lasso-selection", table, StringComparison.Ordinal);
        Assert.Contains("PART_PointerModeLassoButton", table, StringComparison.Ordinal);
        Assert.Contains("HostedActions_PointerModeActionsSwitchNodeCanvasSelectionModeWithoutRuntimeCommandRoute", table, StringComparison.Ordinal);
        Assert.Contains("AuthoringToolsChrome_ProjectsPointerModeActionsThroughNodeCanvasSelectionMode", table, StringComparison.Ordinal);
        Assert.Contains("interaction-selection-marquee-route", table, StringComparison.Ordinal);
        Assert.Contains("selection-marquee-workbench", table, StringComparison.Ordinal);
        Assert.Contains("v079-selection-rectangle-route", table, StringComparison.Ordinal);
        Assert.Contains("selection.select-all", table, StringComparison.Ordinal);
        Assert.Contains("selection.select-none", table, StringComparison.Ordinal);
        Assert.Contains("selection.invert", table, StringComparison.Ordinal);
        Assert.Contains("selection.delete", table, StringComparison.Ordinal);
        Assert.Contains("Delete Selection", table, StringComparison.Ordinal);
        Assert.Contains("selection-delete", table, StringComparison.Ordinal);
        Assert.Contains("selection.transform.move", table, StringComparison.Ordinal);
        Assert.Contains("graph-selection deletion", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not an eraser cursor", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no eraser cursor", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no collision trail", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no eraser-specific hit-testing", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("public pointer-mode activation route", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("transient visual feedback", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hosted toolbar", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("drawing persistence", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("renderer layer", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("screenshot manifest expansion", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not full React Flow whiteboard parity", table, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractMiniMapInteractionCustomizationGate(string contents)
    {
        var headingStart = contents.IndexOf("\n## MiniMap Interaction And Customization Gate", StringComparison.Ordinal);
        Assert.True(headingStart >= 0, "Expected MiniMap interaction and customization gate heading.");

        var tableStart = contents.IndexOf("| MiniMap gate |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected MiniMap interaction and customization gate table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after MiniMap interaction and customization gate table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractControlsInteractivityCustomButtonGate(string contents)
    {
        var headingStart = contents.IndexOf("\n## Controls Interactivity And Custom-Button Gate", StringComparison.Ordinal);
        Assert.True(headingStart >= 0, "Expected Controls interactivity and custom-button gate heading.");

        var tableStart = contents.IndexOf("| Controls gate |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected Controls interactivity and custom-button gate table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after Controls interactivity and custom-button gate table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractBackgroundVariantPublicSurfaceGate(string contents)
    {
        var headingStart = contents.IndexOf("\n## Background Variant Public Surface Gate", StringComparison.Ordinal);
        Assert.True(headingStart >= 0, "Expected Background variant public surface gate heading.");

        var tableStart = contents.IndexOf("| Background gate |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected Background variant public surface gate table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after Background variant public surface gate table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractPanelViewportAttachedOverlayBoundary(string contents)
    {
        var headingStart = contents.IndexOf("\n## Panel Versus Viewport-Attached Overlay Boundary", StringComparison.Ordinal);
        Assert.True(headingStart >= 0, "Expected Panel versus viewport-attached overlay boundary heading.");

        var tableStart = contents.IndexOf("| Panel gate |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected Panel versus viewport-attached overlay boundary table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after Panel versus viewport-attached overlay boundary table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractWhiteboardLassoEraserFeasibilityAudit(string contents)
    {
        var headingStart = contents.IndexOf("\n## Whiteboard Lasso Eraser Feasibility Audit", StringComparison.Ordinal);
        if (headingStart < 0)
        {
            headingStart = contents.IndexOf("\n## Whiteboard/Lasso/Eraser 可行性审计", StringComparison.Ordinal);
        }

        Assert.True(headingStart >= 0, "Expected whiteboard/lasso/eraser feasibility audit heading.");

        var tableStart = contents.IndexOf("| Whiteboard gate |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected whiteboard/lasso/eraser feasibility audit table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after whiteboard/lasso/eraser feasibility audit table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractBuiltInComponentMatrix(string contents)
    {
        var headingStart = contents.IndexOf("\n## Built-in Component Parity Matrix", StringComparison.Ordinal);
        if (headingStart < 0)
        {
            headingStart = contents.IndexOf("\n## Built-in Component 对齐矩阵", StringComparison.Ordinal);
        }

        Assert.True(headingStart >= 0, "Expected built-in component parity matrix heading.");

        var tableStart = contents.IndexOf("| Built-in |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected built-in component parity matrix table header.");

        var nextHeading = contents.IndexOf("\n## ", tableStart, StringComparison.Ordinal);
        Assert.True(nextHeading > tableStart, "Expected heading after built-in component parity matrix table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractIssueWaveTable(string contents)
    {
        var headingStart = contents.IndexOf("\n## Next Issue Wave", StringComparison.Ordinal);
        if (headingStart < 0)
        {
            headingStart = contents.IndexOf("\n## 下一轮 Issue Wave", StringComparison.Ordinal);
        }

        Assert.True(headingStart >= 0, "Expected Next Issue Wave heading.");

        var tableStart = contents.IndexOf("| GitHub |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected Next Issue Wave table header.");
        var nextHeading = contents.IndexOf("\n## Recommended Parallel Worktree Plan", tableStart, StringComparison.Ordinal);
        if (nextHeading < 0)
        {
            nextHeading = contents.IndexOf("\n## 推荐并行 Worktree 计划", tableStart, StringComparison.Ordinal);
        }

        Assert.True(nextHeading > tableStart, "Expected worktree plan heading after Next Issue Wave table.");
        return contents[tableStart..nextHeading];
    }

    private static string ExtractRecommendedWorktreePlan(string contents)
    {
        var headingStart = contents.IndexOf("\n## Recommended Parallel Worktree Plan", StringComparison.Ordinal);
        if (headingStart < 0)
        {
            headingStart = contents.IndexOf("\n## 推荐并行 Worktree 计划", StringComparison.Ordinal);
        }

        Assert.True(headingStart >= 0, "Expected recommended worktree plan heading.");

        var nextHeading = contents.IndexOf("\n## UI Verification Policy", headingStart, StringComparison.Ordinal);
        if (nextHeading < 0)
        {
            nextHeading = contents.IndexOf("\n## UI 验证策略", headingStart, StringComparison.Ordinal);
        }

        Assert.True(nextHeading > headingStart, "Expected UI verification heading after recommended worktree plan.");
        return contents[headingStart..nextHeading];
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }
}
