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
        Assert.Contains("| #169 | `avalonia-node-map-bp0` | Phase 523: refresh React Flow parity issue wave after retained readiness audit", table, StringComparison.Ordinal);
        Assert.Contains("Phase 524: built-in component parity matrix for MiniMap, Controls, Background, Panel", table, StringComparison.Ordinal);
        Assert.Contains("Phase 525: MiniMap interaction and customization parity gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 526: Controls interactivity/custom-button parity gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 527: Background variant public surface gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 528: Panel versus viewport-attached overlay boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 529: whiteboard/lasso/eraser feasibility audit", table, StringComparison.Ordinal);
        Assert.Contains("P2", table, StringComparison.Ordinal);
        Assert.Contains("P3", table, StringComparison.Ordinal);
        Assert.Contains("P4", table, StringComparison.Ordinal);
        Assert.Contains("parity roadmap docs", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("focused docs tests", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("feature catalog", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GraphMiniMap", table, StringComparison.Ordinal);
        Assert.Contains("AsterGraphControls", table, StringComparison.Ordinal);
        Assert.Contains("background/grid", table, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraphPanel", table, StringComparison.Ordinal);
        Assert.True(
            table.Contains("Blocks the next implementation wave", StringComparison.OrdinalIgnoreCase)
            || table.Contains("阻塞下一批 implementation wave", StringComparison.OrdinalIgnoreCase));
        Assert.True(
            table.Contains("Can run in parallel", StringComparison.OrdinalIgnoreCase)
            || table.Contains("可与", StringComparison.OrdinalIgnoreCase));
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
