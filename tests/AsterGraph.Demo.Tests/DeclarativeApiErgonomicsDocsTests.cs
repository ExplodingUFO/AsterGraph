using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DeclarativeApiErgonomicsDocsTests
{
    [Fact]
    public void ParityDocs_RecordPhase503DeclarativeApiErgonomicsAuditInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 503", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #129", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-mzu", contents, StringComparison.Ordinal);
            Assert.Contains("declarative API ergonomics audit", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("DECLARATIVE_API_ERGONOMICS_AUDIT", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorSession", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphHostBuilder.Create(...).BuildAvaloniaView()", contents, StringComparison.Ordinal);
            Assert.Contains("templates/astergraph-avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("src/AsterGraph.Demo", contents, StringComparison.Ordinal);
            Assert.Contains("not equivalent to React Flow hooks/components", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 503: declarative API ergonomics audit", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 503 now owns the declarative API ergonomics audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 503 现在通过 GitHub #129 / `avalonia-node-map-mzu` 承接 declarative API ergonomics audit", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void HostDocs_DefineCurrentCopyableApiRoutesWithoutHookOrDslParityClaims()
    {
        var documents = new[]
        {
            ReadRepoFile("docs/en/quick-start.md"),
            ReadRepoFile("docs/zh-CN/quick-start.md"),
            ReadRepoFile("docs/en/host-integration.md"),
            ReadRepoFile("docs/zh-CN/host-integration.md"),
        };

        foreach (var contents in documents)
        {
            Assert.Contains("DECLARATIVE_API_ERGONOMICS_AUDIT", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorSession", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphHostBuilder.Create(...).BuildAvaloniaView()", contents, StringComparison.Ordinal);
            Assert.Contains("not a second runtime model", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no React hook parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no <ReactFlow>-equivalent declarative DSL", contents, StringComparison.OrdinalIgnoreCase);
            Assert.False(
                HasLineWithAll(contents, "AsterGraph", "has", "React hook parity"),
                "Docs must not claim React hook parity.");
            Assert.False(
                HasLineWithAll(contents, "AsterGraph", "has", "<ReactFlow>"),
                "Docs must not claim a <ReactFlow>-equivalent declarative DSL.");
        }
    }

    [Fact]
    public void Phase520Docs_DefineDeclarativeHostCompositionGateWithoutCurrentApiExpansion()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 520", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #162", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-vdc", contents, StringComparison.Ordinal);
            Assert.Contains("DECLARATIVE_HOST_COMPOSITION_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("future composition profile", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("AsterGraphHostBuilder.Create(...).BuildAvaloniaView()", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("not current public API", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no source generator", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no XAML extension", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no React hook parity", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no <ReactFlow>-equivalent declarative DSL", contents, StringComparison.OrdinalIgnoreCase);
        }

        var hostDocs = new[]
        {
            ReadRepoFile("docs/en/quick-start.md"),
            ReadRepoFile("docs/zh-CN/quick-start.md"),
            ReadRepoFile("docs/en/host-integration.md"),
            ReadRepoFile("docs/zh-CN/host-integration.md"),
        };

        foreach (var contents in hostDocs)
        {
            Assert.Contains("DECLARATIVE_HOST_COMPOSITION_GATE", contents, StringComparison.Ordinal);
            Assert.Contains("future composition profile", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not current public API", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("AsterGraphHostBuilder.Create(...).BuildAvaloniaView()", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphEditorFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", contents, StringComparison.Ordinal);
            Assert.False(
                HasLineWithAll(contents, "composition profile", "is available"),
                "Docs must not claim the future composition profile exists today.");
            Assert.False(
                HasLineWithAll(contents, "composition profile", "available today"),
                "Docs must keep the future composition profile outside the current public API.");
        }
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

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
        => contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
}
