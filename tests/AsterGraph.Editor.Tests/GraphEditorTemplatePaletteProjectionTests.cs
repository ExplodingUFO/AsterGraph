using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorTemplatePaletteProjectionTests
{
    private static readonly NodeDefinitionId AlphaDefinitionId = new("tests.palette.alpha");
    private static readonly NodeDefinitionId BetaDefinitionId = new("tests.palette.beta");
    private const string AlphaNodeId = "tests.palette.alpha-node";
    private const string BetaNodeId = "tests.palette.beta-node";

    [Fact]
    public void RuntimeContracts_ExposeSearchableTemplatePaletteProjection()
    {
        var queriesType = typeof(IGraphEditorQueries);

        var method = queriesType.GetMethod(nameof(IGraphEditorQueries.GetTemplatePaletteSnapshot), [typeof(GraphEditorTemplatePaletteQuery)]);

        Assert.NotNull(method);
        Assert.Equal(typeof(GraphEditorTemplatePaletteSnapshot), method!.ReturnType);
        Assert.NotNull(typeof(GraphEditorTemplatePaletteSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteSnapshot.Items)));
        Assert.NotNull(typeof(GraphEditorTemplatePaletteItemSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteItemSnapshot.Id)));
        Assert.NotNull(typeof(GraphEditorTemplatePaletteItemSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteItemSnapshot.Kind)));
        Assert.NotNull(typeof(GraphEditorTemplatePaletteItemSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteItemSnapshot.SourceKind)));
        Assert.NotNull(typeof(GraphEditorTemplatePaletteItemSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteItemSnapshot.SourceId)));
        Assert.NotNull(typeof(GraphEditorTemplatePaletteItemSnapshot).GetProperty(nameof(GraphEditorTemplatePaletteItemSnapshot.TemplateKey)));
        Assert.NotNull(typeof(GraphEditorFragmentTemplateSnapshot).GetProperty(nameof(GraphEditorFragmentTemplateSnapshot.GroupCount)));
    }

    [Fact]
    public void SessionQueries_ProjectNodeAndFragmentTemplates_InDeterministicPaletteOrder()
    {
        var session = CreateSession();
        session.Commands.SetSelection([AlphaNodeId, BetaNodeId], AlphaNodeId, updateStatus: false);

        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Palette Group");
        Assert.NotEmpty(groupId);
        var templatePath = session.Commands.TryExportSelectionAsTemplate("Grouped Fragment");

        var palette = session.Queries.GetTemplatePaletteSnapshot();

        Assert.Equal(3, palette.Count);
        Assert.Equal(
            [
                "node:tests.palette.beta",
                "node:tests.palette.alpha",
                $"fragment:{templatePath}",
            ],
            palette.Items.Select(item => item.Id).ToArray());

        var nodeItem = Assert.Single(palette.Items, item => item.Id == "node:tests.palette.alpha");
        Assert.Equal(GraphEditorTemplatePaletteItemKind.NodeTemplate, nodeItem.Kind);
        Assert.Equal(GraphEditorTemplatePaletteSourceKind.NodeCatalog, nodeItem.SourceKind);
        Assert.Equal("Processing", nodeItem.Category);
        Assert.Equal("1 in  ·  0 out", nodeItem.Summary);
        Assert.Equal(1, nodeItem.NodeCount);

        var fragmentItem = Assert.Single(palette.Items, item => item.Id == $"fragment:{templatePath}");
        Assert.Equal(GraphEditorTemplatePaletteItemKind.FragmentTemplate, fragmentItem.Kind);
        Assert.Equal(GraphEditorTemplatePaletteSourceKind.FragmentLibrary, fragmentItem.SourceKind);
        Assert.Equal("Fragment Groups", fragmentItem.Category);
        Assert.Equal(2, fragmentItem.NodeCount);
        Assert.Equal(1, fragmentItem.GroupCount);
        Assert.True(fragmentItem.HasGroups);
        Assert.Contains("groups", fragmentItem.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void SessionQueries_FilterAndSearchTemplatePalette_WithoutUiSearchState()
    {
        var session = CreateSession();
        session.Commands.SetSelection([AlphaNodeId, BetaNodeId], AlphaNodeId, updateStatus: false);
        session.Commands.TryCreateNodeGroupFromSelection("Palette Group");
        session.Commands.TryExportSelectionAsTemplate("Grouped Fragment");

        var searched = session.Queries.GetTemplatePaletteSnapshot(new GraphEditorTemplatePaletteQuery("GROUP"));
        var sourceFiltered = session.Queries.GetTemplatePaletteSnapshot(new GraphEditorTemplatePaletteQuery(
            SourceKind: GraphEditorTemplatePaletteSourceKind.NodeCatalog));
        var kindFiltered = session.Queries.GetTemplatePaletteSnapshot(new GraphEditorTemplatePaletteQuery(
            Kind: GraphEditorTemplatePaletteItemKind.FragmentTemplate));
        var categoryFiltered = session.Queries.GetTemplatePaletteSnapshot(new GraphEditorTemplatePaletteQuery(
            Category: "Processing"));

        var searchedItem = Assert.Single(searched.Items);
        Assert.Equal(GraphEditorTemplatePaletteItemKind.FragmentTemplate, searchedItem.Kind);
        Assert.Equal("GROUP", searched.SearchText);

        Assert.Equal(2, sourceFiltered.Count);
        Assert.All(sourceFiltered.Items, item => Assert.Equal(GraphEditorTemplatePaletteSourceKind.NodeCatalog, item.SourceKind));

        var fragment = Assert.Single(kindFiltered.Items);
        Assert.Equal("Fragment Groups", fragment.Category);

        var processingNode = Assert.Single(categoryFiltered.Items);
        Assert.Equal("Alpha Processor", processingNode.Title);
    }

    private static IGraphEditorSession CreateSession()
    {
        var storageRootPath = Path.Combine(
            Path.GetTempPath(),
            "astergraph-template-palette-projection",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(storageRootPath);

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Palette Projection Graph",
                "Covers searchable template palette projection.",
                [
                    new GraphNode(
                        AlphaNodeId,
                        "Alpha Processor",
                        "Processing",
                        "Transforms data.",
                        "Runtime node used by palette projection tests.",
                        new GraphPoint(120, 160),
                        new GraphSize(220, 140),
                        [new GraphPort("input", "Input", PortDirection.Input, "Data", "#6AD5C4", new PortTypeId("data"))],
                        [],
                        "#6AD5C4",
                        AlphaDefinitionId),
                    new GraphNode(
                        BetaNodeId,
                        "Beta Loader",
                        "Inputs",
                        "Loads data.",
                        "Runtime node used by palette projection tests.",
                        new GraphPoint(420, 160),
                        new GraphSize(220, 140),
                        [],
                        [new GraphPort("output", "Output", PortDirection.Output, "Data", "#B88CFF", new PortTypeId("data"))],
                        "#B88CFF",
                        BetaDefinitionId),
                ],
                []),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRootPath,
        });
    }

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                AlphaDefinitionId,
                "Alpha Processor",
                "Processing",
                "Transforms data.",
                [new PortDefinition("input", "Input", new PortTypeId("data"))],
                []));
        catalog.RegisterDefinition(
            new NodeDefinition(
                BetaDefinitionId,
                "Beta Loader",
                "Inputs",
                "Loads data.",
                [],
                [new PortDefinition("output", "Output", new PortTypeId("data"))]));
        return catalog;
    }
}
