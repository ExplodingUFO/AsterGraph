using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginCommandContractTests
{
    private static readonly NodeDefinitionId HostDefinitionId = new("tests.plugin-command.host");
    private static readonly NodeDefinitionId PluginDefinitionId = new("tests.plugin-command.plugin");
    private const string PluginCommandId = "tests.plugin.command.add-node";
    private const string PluginCommandGroup = "plugin";

    [Fact]
    public void CreateSession_WithPluginCommandContribution_ExposesPluginCommandDescriptor()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromPlugin(new PluginCommandPlugin())));

        var descriptor = Assert.Single(
            session.Queries.GetCommandDescriptors(),
            candidate => string.Equals(candidate.Id, PluginCommandId, StringComparison.Ordinal));

        Assert.Equal("Add Plugin Command Node", descriptor.Title);
        Assert.Equal(PluginCommandGroup, descriptor.Group);
        Assert.Equal("plugin", descriptor.IconKey);
        Assert.Equal("Ctrl+Shift+P", descriptor.DefaultShortcut);
        Assert.True(descriptor.CanExecute);
        Assert.Equal(GraphEditorCommandSourceKind.Plugin, descriptor.Source);
    }

    [Fact]
    public void CreateSession_WithPluginCommandContribution_ExecutesThroughCanonicalCommandRoute()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromPlugin(new PluginCommandPlugin())));
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);

        var executed = session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(PluginCommandId));
        var document = session.Queries.CreateDocumentSnapshot();

        Assert.True(executed);
        Assert.Contains(executedCommandIds, commandId => string.Equals(commandId, PluginCommandId, StringComparison.Ordinal));
        Assert.Contains(document.Nodes, node => node.DefinitionId == PluginDefinitionId);
    }

    [Fact]
    public void CreateSession_WithPluginCommandIdCollision_DoesNotOverrideHostCommandDescriptor()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromPlugin(new ConflictingPluginCommandPlugin())));

        var descriptor = Assert.Single(
            session.Queries.GetCommandDescriptors(),
            candidate => string.Equals(candidate.Id, "history.undo", StringComparison.Ordinal));

        Assert.Equal(GraphEditorCommandSourceKind.Kernel, descriptor.Source);
        Assert.Equal("Undo", descriptor.Title);
    }

    [Fact]
    public void HostedActionFactory_CreateHostAction_UsesSharedCommandDescriptorMetadata()
    {
        var descriptor = new GraphEditorCommandDescriptorSnapshot(
            "host.review.add",
            "Add Review Node",
            "host",
            "add",
            "Ctrl+Shift+R",
            GraphEditorCommandSourceKind.Host,
            isEnabled: true);

        var invoked = 0;
        var action = AsterGraphHostedActionFactory.CreateHostAction(
            descriptor,
            () =>
            {
                invoked++;
                return true;
            });

        Assert.Equal(descriptor, action.Descriptor);
        Assert.Equal(descriptor.Id, action.Id);
        Assert.Equal(descriptor.Title, action.Title);
        Assert.Equal(descriptor.Group, action.Group);
        Assert.Equal(descriptor.Source, action.CommandSource);
        Assert.True(action.TryExecute());
        Assert.Equal(1, invoked);
    }

    [Fact]
    public void HostedActionFactory_CreateProjection_SelectsOrderedActionsAndShortcutSubset()
    {
        var addReview = AsterGraphHostedActionFactory.CreateHostAction(
            new GraphEditorCommandDescriptorSnapshot(
                "host.review.add",
                "Add Review Node",
                "host",
                "add",
                "Ctrl+Shift+R",
                GraphEditorCommandSourceKind.Host,
                isEnabled: true),
            () => true);
        var approve = AsterGraphHostedActionFactory.CreateHostAction(
            new GraphEditorCommandDescriptorSnapshot(
                "host.review.approve",
                "Approve Review Node",
                "host",
                "approve",
                null,
                GraphEditorCommandSourceKind.Host,
                isEnabled: false,
                disabledReason: "Select one review node before approving."),
            () => false);
        var projection = AsterGraphHostedActionFactory.CreateProjection(
            [
                approve,
                addReview,
            ]);

        var selected = projection.Select(["host.review.add", "host.review.approve"]);
        var shortcuts = projection.WithShortcuts();

        Assert.Equal(["host.review.add", "host.review.approve"], selected.Select(action => action.Id));
        Assert.Equal("Select one review node before approving.", selected[1].DisabledReason);
        Assert.Equal(["host.review.add"], shortcuts.Select(action => action.Id));
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithDefaultShortcuts_ReportsDuplicateEffectiveShortcuts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", "Ctrl+Shift+R"));

        var conflicts = AsterGraphHostedActionShortcutConflictDetector.FindConflicts(actions);
        var conflict = Assert.Single(conflicts);

        Assert.Equal("Ctrl+Shift+R", conflict.Shortcut);
        Assert.Equal(["host.review.add", "plugin.review.add"], conflict.ActionIds);
        Assert.Equal(["commands.addReview", "plugin.addReview"], conflict.CommandIds);
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithPolicyOverrides_ReportsEffectiveShortcutConflicts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", "Ctrl+Alt+R"));
        var effectiveActions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
            actions,
            new AsterGraphCommandShortcutPolicy
            {
                ShortcutOverrides = new Dictionary<string, string?>
                {
                    ["plugin.review.add"] = "Ctrl+Shift+R",
                },
            });

        var conflict = Assert.Single(AsterGraphHostedActionShortcutConflictDetector.FindConflicts(effectiveActions));

        Assert.Equal("Ctrl+Shift+R", conflict.Shortcut);
        Assert.Equal(["host.review.add", "plugin.review.add"], conflict.ActionIds);
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithDisabledShortcutPolicy_IgnoresDisabledShortcuts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", "Ctrl+Shift+R"));
        var effectiveActions = AsterGraphHostedActionFactory.ApplyCommandShortcutPolicy(
            actions,
            AsterGraphCommandShortcutPolicy.Disabled);

        var conflicts = AsterGraphHostedActionShortcutConflictDetector.FindConflicts(effectiveActions);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithBlankShortcuts_IgnoresBlankShortcuts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", " "),
            ("plugin.review.approve", "plugin.approveReview", null));

        var conflicts = AsterGraphHostedActionShortcutConflictDetector.FindConflicts(actions);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithExcludedActionIds_IgnoresExcludedShortcuts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", "Ctrl+Shift+R"));

        var conflicts = AsterGraphHostedActionShortcutConflictDetector.FindConflicts(
            actions,
            ["plugin.review.add"]);

        Assert.Empty(conflicts);
    }

    [Fact]
    public void HostedActionShortcutConflictDetector_WithDistinctShortcuts_ReturnsNoConflicts()
    {
        var actions = CreateShortcutActions(
            ("host.review.add", "commands.addReview", "Ctrl+Shift+R"),
            ("plugin.review.add", "plugin.addReview", "Ctrl+Alt+R"));

        var conflicts = AsterGraphHostedActionShortcutConflictDetector.FindConflicts(actions);

        Assert.Empty(conflicts);
    }

    private static AsterGraphEditorOptions CreateOptions(params GraphEditorPluginRegistration[] pluginRegistrations)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations = pluginRegistrations,
        };

    private static GraphDocument CreateDocument()
        => new(
            "Plugin Command Graph",
            "Plugin command route coverage.",
            [
                new GraphNode(
                    "tests.plugin-command.host-node",
                    "Host Node",
                    "Tests",
                    "Plugin Commands",
                    "Host-owned node used for runtime command coverage.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [],
                    "#6AD5C4",
                    HostDefinitionId),
            ],
            []);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(HostDefinitionId, "Host Node", "Tests", "Plugin Commands", [], []));
        catalog.RegisterDefinition(new NodeDefinition(PluginDefinitionId, "Plugin Node", "Tests", "Plugin Commands", [], []));
        return catalog;
    }

    private static IReadOnlyList<AsterGraphHostedActionDescriptor> CreateShortcutActions(
        params (string ActionId, string CommandId, string? Shortcut)[] actions)
        => actions
            .Select(action => new AsterGraphHostedActionDescriptor(
                new GraphEditorCommandDescriptorSnapshot(
                    action.ActionId,
                    action.ActionId,
                    "tests",
                    null,
                    action.Shortcut,
                    GraphEditorCommandSourceKind.Host,
                    isEnabled: true),
                () => true,
                action.CommandId))
            .ToList();

    private sealed class PluginCommandPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.plugin-command", "Plugin Command");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddCommandContributor(new AddNodeCommandContributor());
        }
    }

    private sealed class ConflictingPluginCommandPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.plugin-command.conflict", "Plugin Command Conflict");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddCommandContributor(new ConflictingCommandContributor());
        }
    }

    private sealed class AddNodeCommandContributor : IGraphEditorPluginCommandContributor
    {
        public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return
            [
                new GraphEditorCommandDescriptorSnapshot(
                    PluginCommandId,
                    "Add Plugin Command Node",
                    PluginCommandGroup,
                    "plugin",
                    "Ctrl+Shift+P",
                    GraphEditorCommandSourceKind.Plugin,
                    isEnabled: true),
            ];
        }

        public bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            context.Session.Commands.AddNode(PluginDefinitionId, new GraphPoint(420, 240));
            var nodes = context.Session.Queries.CreateDocumentSnapshot().Nodes;
            return nodes.Any(node => node.DefinitionId == PluginDefinitionId);
        }
    }

    private sealed class ConflictingCommandContributor : IGraphEditorPluginCommandContributor
    {
        public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return
            [
                new GraphEditorCommandDescriptorSnapshot(
                    "history.undo",
                    "Plugin Undo Override",
                    PluginCommandGroup,
                    "plugin",
                    null,
                    GraphEditorCommandSourceKind.Plugin,
                    isEnabled: true),
            ];
        }

        public bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return true;
        }
    }
}
