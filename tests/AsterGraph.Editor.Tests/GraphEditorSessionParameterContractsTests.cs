using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSessionParameterContractsTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.session.parameters");
    private const string FirstNodeId = "tests.session.parameters.node-001";
    private const string SecondNodeId = "tests.session.parameters.node-002";

    [Fact]
    public void RuntimeContracts_ExposeDefinitionAndParameterEditingSurface()
    {
        var queriesType = typeof(IGraphEditorQueries);
        var commandsType = typeof(IGraphEditorCommands);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetRegisteredNodeDefinitions));
        Assert.Equal(
            typeof(IReadOnlyList<INodeDefinition>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetRegisteredNodeDefinitions))!.ReturnType);
        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetStencilItemSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorStencilItemSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetStencilItemSnapshots))!.ReturnType);
        Assert.NotNull(typeof(GraphEditorStencilItemSnapshot).GetProperty(nameof(GraphEditorStencilItemSnapshot.DefinitionId)));
        Assert.NotNull(typeof(GraphEditorStencilItemSnapshot).GetProperty(nameof(GraphEditorStencilItemSnapshot.PortSummary)));

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSharedSelectionDefinition));
        Assert.Equal(
            typeof(INodeDefinition),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetSharedSelectionDefinition))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSelectedNodeParameterSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorNodeParameterSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetSelectedNodeParameterSnapshots))!.ReturnType);
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.IsValid)));
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.ValidationMessage)));
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.CanResetToDefault)));
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.IsUsingDefaultValue)));
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.ReadOnlyReason)));
        Assert.NotNull(typeof(GraphEditorNodeParameterSnapshot).GetProperty(nameof(GraphEditorNodeParameterSnapshot.HelpText)));

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValue), typeof(string), typeof(object));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValue), [typeof(string), typeof(object)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValues), typeof(IReadOnlyDictionary<string, object>));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValues), [typeof(IReadOnlyDictionary<string, object>)])!.ReturnType);
    }

    [Fact]
    public void SessionQueries_ExposeSharedDefinitionMetadata_AndParameterSnapshots()
    {
        var session = CreateSession();
        session.Commands.SetSelection([FirstNodeId, SecondNodeId], FirstNodeId, updateStatus: false);

        var registeredDefinitions = session.Queries.GetRegisteredNodeDefinitions();
        var stencilItems = session.Queries.GetStencilItemSnapshots();
        var sharedDefinition = session.Queries.GetSharedSelectionDefinition();
        var parameters = session.Queries.GetSelectedNodeParameterSnapshots();

        Assert.Contains(registeredDefinitions, definition => definition.Id == DefinitionId);
        var stencilItem = Assert.Single(stencilItems, item => item.DefinitionId == DefinitionId);
        Assert.Equal("Parameter Node", stencilItem.Title);
        Assert.Equal("Tests", stencilItem.Category);
        Assert.Equal("0 in  ·  0 out", stencilItem.PortSummary);
        Assert.NotNull(sharedDefinition);
        Assert.Equal(DefinitionId, sharedDefinition!.Id);
        Assert.Equal(2, sharedDefinition.Parameters.Count);

        var threshold = Assert.Single(parameters, parameter => parameter.Definition.Key == "threshold");
        Assert.True(threshold.HasMixedValues);
        Assert.Null(threshold.CurrentValue);
        Assert.True(threshold.CanEdit);

        var enabled = Assert.Single(parameters, parameter => parameter.Definition.Key == "enabled");
        Assert.False(enabled.HasMixedValues);
        Assert.Equal(true, enabled.CurrentValue);
    }

    [Fact]
    public void SessionCommands_TrySetSelectedNodeParameterValue_UpdatesSelectionAndPublishesCommand()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Commands.SetSelection([FirstNodeId, SecondNodeId], FirstNodeId, updateStatus: false);
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var edited = session.Commands.TrySetSelectedNodeParameterValue("threshold", "0.90");

        Assert.True(edited);
        Assert.Contains("nodes.parameters.set", commandIds);

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.All(
            document.Nodes.Where(node => node.Id is FirstNodeId or SecondNodeId),
            node => Assert.Equal(0.9d, Assert.Single(node.ParameterValues!, parameter => parameter.Key == "threshold").Value));

        var updated = Assert.Single(session.Queries.GetSelectedNodeParameterSnapshots(), parameter => parameter.Definition.Key == "threshold");
        Assert.False(updated.HasMixedValues);
        Assert.Equal(0.9d, updated.CurrentValue);
    }

    [Fact]
    public void SessionCommands_TrySetSelectedNodeParameterValue_RespectsNodeParameterPermissions()
    {
        var session = CreateSession(GraphEditorBehaviorOptions.Default with
        {
            Commands = GraphEditorCommandPermissions.Default with
            {
                Nodes = new NodeCommandPermissions
                {
                    AllowCreate = true,
                    AllowDelete = true,
                    AllowDuplicate = true,
                    AllowMove = true,
                    AllowEditParameters = false,
                },
            },
        });
        session.Commands.SetSelection([FirstNodeId], FirstNodeId, updateStatus: false);

        var edited = session.Commands.TrySetSelectedNodeParameterValue("threshold", "0.90");

        Assert.False(edited);
        var threshold = Assert.Single(session.Queries.GetSelectedNodeParameterSnapshots(), parameter => parameter.Definition.Key == "threshold");
        Assert.False(threshold.CanEdit);
        Assert.Equal(0.5d, threshold.CurrentValue);
    }

    [Fact]
    public void SessionCommands_TrySetSelectedNodeParameterValues_UpdatesMultipleParametersAndPublishesOneCommand()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Commands.SetSelection([FirstNodeId, SecondNodeId], FirstNodeId, updateStatus: false);
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var edited = session.Commands.TrySetSelectedNodeParameterValues(new Dictionary<string, object?>
        {
            ["threshold"] = "0.90",
            ["enabled"] = false,
        });

        Assert.True(edited);
        Assert.Contains("nodes.parameters.batch-set", commandIds);

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.All(
            document.Nodes.Where(node => node.Id is FirstNodeId or SecondNodeId),
            node =>
            {
                Assert.Equal(0.9d, Assert.Single(node.ParameterValues!, parameter => parameter.Key == "threshold").Value);
                Assert.False(Assert.IsType<bool>(Assert.Single(node.ParameterValues!, parameter => parameter.Key == "enabled").Value));
            });
    }

    [Fact]
    public void SessionQueries_GetSelectedNodeParameterSnapshots_SurfaceValidationStateForPersistedInvalidValues()
    {
        var session = CreateValidationSession();
        session.Commands.SetSelection(["tests.session.parameters.validation-node"], "tests.session.parameters.validation-node", updateStatus: false);

        var snapshots = session.Queries.GetSelectedNodeParameterSnapshots();

        var slug = Assert.Single(snapshots, snapshot => snapshot.Definition.Key == "slug");
        Assert.False(slug.IsValid);
        Assert.Equal("Slug must be at least 3 characters.", slug.ValidationMessage);
        Assert.Equal("Metadata", slug.Definition.GroupName);
        Assert.Equal("lowercase-id", slug.Definition.PlaceholderText);
    }

    [Fact]
    public void SessionQueries_GetSelectedNodeParameterSnapshots_ExposeResetReadOnlyAndExtendedDefinitionMetadata()
    {
        var session = CreateInspectorMetadataSession();
        session.Commands.SetSelection(["tests.session.parameters.inspector-node"], "tests.session.parameters.inspector-node", updateStatus: false);

        var snapshots = session.Queries.GetSelectedNodeParameterSnapshots();

        var threshold = Assert.Single(snapshots, snapshot => snapshot.Definition.Key == "threshold");
        Assert.True(threshold.CanResetToDefault);
        Assert.False(threshold.IsUsingDefaultValue);
        Assert.Null(threshold.ReadOnlyReason);
        Assert.Contains("Fine-tunes the visible threshold.", threshold.HelpText);
        Assert.Equal("ms", threshold.Definition.UnitSuffix);
        Assert.Equal(20, threshold.Definition.SortOrder);

        var systemKey = Assert.Single(snapshots, snapshot => snapshot.Definition.Key == "system-key");
        Assert.False(systemKey.CanResetToDefault);
        Assert.True(systemKey.IsUsingDefaultValue);
        Assert.Equal("参数定义将此字段标记为只读。", systemKey.ReadOnlyReason);
        Assert.True(systemKey.Definition.IsAdvanced);
        Assert.Equal(90, systemKey.Definition.SortOrder);
    }

    private static IGraphEditorSession CreateSession(GraphEditorBehaviorOptions? behaviorOptions = null)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Parameter Contract Graph",
                "Covers runtime parameter metadata and mutation contracts.",
                [
                    CreateNode(FirstNodeId, 0.5d, true, 120),
                    CreateNode(SecondNodeId, 0.8d, true, 360),
                ],
                []),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            BehaviorOptions = behaviorOptions,
        });

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefinitionId,
                "Parameter Node",
                "Tests",
                "Runtime parameter contract coverage.",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        defaultValue: 0.5d),
                    new NodeParameterDefinition(
                        "enabled",
                        "Enabled",
                        new PortTypeId("bool"),
                        ParameterEditorKind.Boolean,
                        defaultValue: true),
                ]));
        return catalog;
    }

    private static GraphNode CreateNode(string nodeId, double threshold, bool enabled, double x)
        => new(
            nodeId,
            "Parameter Node",
            "Tests",
            "Parameters",
            "Runtime parameter contract coverage node.",
            new GraphPoint(x, 160),
            new GraphSize(220, 140),
            [],
            [],
            "#6AD5C4",
            DefinitionId,
            [
                new GraphParameterValue("threshold", new PortTypeId("float"), threshold),
                new GraphParameterValue("enabled", new PortTypeId("bool"), enabled),
            ]);

    private static IGraphEditorSession CreateValidationSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Validation Graph",
                "Covers validation-aware parameter snapshots.",
                [
                    new GraphNode(
                        "tests.session.parameters.validation-node",
                        "Validation Node",
                        "Tests",
                        "Validation",
                        "Contains invalid persisted parameter data.",
                        new GraphPoint(120, 160),
                        new GraphSize(220, 140),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.session.parameters.validation"),
                        [
                            new GraphParameterValue("slug", new PortTypeId("string"), "ab"),
                        ]),
                ],
                []),
            NodeCatalog = CreateValidationCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static INodeCatalog CreateValidationCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.session.parameters.validation"),
                "Validation Node",
                "Tests",
                "Validation",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "slug",
                        "Slug",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        description: "Stable lowercase identifier.",
                        defaultValue: "node",
                        constraints: new ParameterConstraints(
                            MinimumLength: 3,
                            ValidationPattern: "^[a-z-]+$",
                            ValidationPatternDescription: "lowercase letters and dashes"),
                        groupName: "Metadata",
                        placeholderText: "lowercase-id"),
                ]));
        return catalog;
    }

    private static IGraphEditorSession CreateInspectorMetadataSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Inspector Metadata Graph",
                "Covers extended inspector-facing parameter metadata.",
                [
                    new GraphNode(
                        "tests.session.parameters.inspector-node",
                        "Inspector Metadata Node",
                        "Tests",
                        "Inspector",
                        "Contains parameter metadata used by the shipped inspector.",
                        new GraphPoint(120, 160),
                        new GraphSize(220, 140),
                        [],
                        [],
                        "#6AD5C4",
                        new NodeDefinitionId("tests.session.parameters.inspector"),
                        [
                            new GraphParameterValue("threshold", new PortTypeId("float"), 0.9d),
                            new GraphParameterValue("system-key", new PortTypeId("string"), "system-core"),
                        ]),
                ],
                []),
            NodeCatalog = CreateInspectorMetadataCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static INodeCatalog CreateInspectorMetadataCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                new NodeDefinitionId("tests.session.parameters.inspector"),
                "Inspector Metadata Node",
                "Tests",
                "Inspector",
                [],
                [],
                parameters:
                [
                    new NodeParameterDefinition(
                        "system-key",
                        "System Key",
                        new PortTypeId("string"),
                        ParameterEditorKind.Text,
                        defaultValue: "system-core",
                        constraints: new ParameterConstraints(IsReadOnly: true),
                        groupName: "Metadata",
                        sortOrder: 90,
                        isAdvanced: true),
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        defaultValue: 0.5d,
                        groupName: "Behavior",
                        helpText: "Fine-tunes the visible threshold.",
                        sortOrder: 20,
                        unitSuffix: "ms"),
                ]));
        return catalog;
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, parameterTypes);
        Assert.NotNull(method);
    }
}
