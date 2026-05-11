using System.Reflection;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class CompatibilitySurfacePolicyTests
{
    [Fact]
    public void GraphDocument_RemovesRootOnlyConstructorAndDeconstructFromPublicSurface()
    {
        var rootOnlyConstructor = typeof(GraphDocument).GetConstructor(
        [
            typeof(string),
            typeof(string),
            typeof(IReadOnlyList<GraphNode>),
            typeof(IReadOnlyList<GraphConnection>),
            typeof(IReadOnlyList<GraphNodeGroup>),
        ]);

        Assert.Null(rootOnlyConstructor);
        Assert.DoesNotContain(
            typeof(GraphDocument).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == "Deconstruct" && method.GetParameters().Length == 5);
    }

    [Fact]
    public void GraphDocumentSerializer_UsesExplicitLegacyImportPathForLegacyPayloads()
    {
        const string legacyJson = """
        {
          "Title": "Legacy Graph",
          "Description": "Unversioned legacy payload.",
          "Nodes": [],
          "Connections": [],
          "Groups": []
        }
        """;

        var exception = Assert.Throws<InvalidOperationException>(() => GraphDocumentSerializer.Deserialize(legacyJson));
        Assert.Contains("ImportLegacy", exception.Message, StringComparison.Ordinal);

        var importLegacy = typeof(GraphDocumentSerializer).GetMethod("ImportLegacy", [typeof(string)]);
        Assert.NotNull(importLegacy);

        var document = Assert.IsType<GraphDocument>(importLegacy.Invoke(null, [legacyJson]));

        Assert.Equal("Legacy Graph", document.Title);
        Assert.Equal(GraphDocument.DefaultRootGraphId, document.RootGraphId);
        Assert.Single(document.GraphScopes);
    }

    [Fact]
    public void RuntimeCompatibilityShims_AreRemovedFromPrimaryPublicContracts()
    {
        Assert.Null(typeof(IGraphEditorCommands).GetMethod("BeginConnection", [typeof(string), typeof(string)]));
        Assert.Null(typeof(GraphEditorSession).GetMethod("BeginConnection", [typeof(string), typeof(string)]));
        Assert.Null(typeof(IGraphEditorQueries).GetMethod("GetCompatibleTargets", [typeof(string), typeof(string)]));
        Assert.Null(typeof(IGraphEditorSession).Assembly.GetType("AsterGraph.Editor.Menus.CompatiblePortTarget", throwOnError: false));
        Assert.NotNull(typeof(IGraphEditorQueries).GetMethod(nameof(IGraphEditorQueries.GetCompatiblePortTargets), [typeof(string), typeof(string)]));
    }

    [Fact]
    public void GraphEditorCapabilitySnapshot_RemovesObsoletePositionalCompatibilityMembers()
    {
        Assert.DoesNotContain(
            typeof(GraphEditorCapabilitySnapshot).GetConstructors(BindingFlags.Public | BindingFlags.Instance),
            constructor => constructor.GetParameters().Length == 14);
        Assert.DoesNotContain(
            typeof(GraphEditorCapabilitySnapshot).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == "Deconstruct" && method.GetParameters().Length == 14);
    }

    [Fact]
    public void PublicApiDocs_PublishV1CompatibilityRemovalPolicy()
    {
        var english = ReadRepoFile("docs/en/public-api-inventory.md");
        var chinese = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("V1 compatibility removal policy", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocument root-only constructor/deconstruct", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocumentSerializer.ImportLegacy(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorCommands.BeginConnection(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorQueries.GetCompatibleTargets(...)", contents, StringComparison.Ordinal);
            Assert.Contains("CompatiblePortTarget", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorCapabilitySnapshot obsolete constructor/deconstruct", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PublicApiDocs_PublishRetainedMigrationRemovalRoadmapGate()
    {
        var english = ReadRepoFile("docs/en/public-api-inventory.md");
        var chinese = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("Phase 492 retained migration removal roadmap", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorViewModel", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorView", contents, StringComparison.Ordinal);
            Assert.Contains("GraphEditorViewModel.Session", contents, StringComparison.Ordinal);
            Assert.Contains("GraphDocumentSerializer.ImportLegacy(...)", contents, StringComparison.Ordinal);
            Assert.Contains("inventory now, remove later", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Do not delete retained migration surfaces before", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("eng/public-api-baseline.txt", contents, StringComparison.Ordinal);
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../", relativePath));
        return File.ReadAllText(fullPath);
    }
}
