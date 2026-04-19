using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.Integration;

internal static class DemoPluginShowcase
{
    public static readonly NodeDefinitionId ShowcaseNodeDefinitionId = new("aster.demo.plugin.showcase-node");
    public static readonly NodeDefinitionId BlockedShowcaseNodeDefinitionId = new("aster.demo.plugin.blocked-node");

    private static readonly GraphEditorPluginManifest AllowedManifest = new(
        "aster.demo.plugin.allowed",
        "Demo Trusted Plugin",
        new GraphEditorPluginManifestProvenance(
            GraphEditorPluginManifestSourceKind.Manifest,
            "demo.manifest.allowed",
            publisher: "AsterGraph Demo",
            packageId: "AsterGraph.Demo.TrustedPlugin",
            packageVersion: "0.2.0-alpha.3"),
        description: "Trusted showcase plugin for the public alpha demo.",
        version: "0.2.0-alpha.3",
        compatibility: new GraphEditorPluginCompatibilityManifest(
            minimumAsterGraphVersion: "0.2.0-alpha.3",
            targetFramework: "net9.0",
            runtimeSurface: "CreateSession/Create + hosted Avalonia"),
        capabilitySummary: "Adds node, menu, localization, and presentation contributions.");

    private static readonly GraphEditorPluginManifest BlockedManifest = new(
        "aster.demo.plugin.blocked",
        "Demo Blocked Plugin",
        new GraphEditorPluginManifestProvenance(
            GraphEditorPluginManifestSourceKind.Manifest,
            "demo.manifest.blocked",
            publisher: "AsterGraph Demo",
            packageId: "AsterGraph.Demo.BlockedPlugin",
            packageVersion: "0.2.0-alpha.3"),
        description: "Blocked showcase plugin used to prove trust-policy decisions.",
        version: "0.2.0-alpha.3",
        compatibility: new GraphEditorPluginCompatibilityManifest(
            minimumAsterGraphVersion: "0.2.0-alpha.3",
            targetFramework: "net9.0",
            runtimeSurface: "CreateSession/Create + hosted Avalonia"),
        capabilitySummary: "Intentionally blocked by the demo trust policy.");

    private static readonly GraphEditorPluginProvenanceEvidence AllowedProvenance = new(
        new GraphEditorPluginPackageIdentity("AsterGraph.Demo.TrustedPlugin", "0.2.0-alpha.3"),
        new GraphEditorPluginSignatureEvidence(
            GraphEditorPluginSignatureStatus.Valid,
            GraphEditorPluginSignatureKind.Repository,
            new GraphEditorPluginSignerIdentity("AsterGraph Demo", "DEMO-TRUST-001"),
            reasonCode: "demo.signature.valid",
            reasonMessage: "Repository signature validated for the demo showcase."));

    private static readonly GraphEditorPluginProvenanceEvidence BlockedProvenance = new(
        new GraphEditorPluginPackageIdentity("AsterGraph.Demo.BlockedPlugin", "0.2.0-alpha.3"),
        new GraphEditorPluginSignatureEvidence(
            GraphEditorPluginSignatureStatus.Unsigned,
            GraphEditorPluginSignatureKind.Unknown,
            reasonCode: "demo.signature.unsigned",
            reasonMessage: "Unsigned package used to prove a blocked trust decision."));

    public static DemoPluginShowcaseConfiguration Create(string storageRootPath)
    {
        var assemblyPath = typeof(DemoAllowedPlugin).Assembly.Location;
        var allowlistStore = new DemoPluginAllowlistStore(storageRootPath);
        var trustPolicy = new DemoPluginAllowlistTrustPolicy(
            allowlistStore,
            [
                DemoPluginAllowlistTrustPolicy.CreateEntry(AllowedManifest, AllowedProvenance),
            ]);

        return new DemoPluginShowcaseConfiguration(
            [
                GraphEditorPluginRegistration.FromPlugin(new DemoAllowedPlugin(), AllowedManifest, AllowedProvenance),
                GraphEditorPluginRegistration.FromPlugin(new DemoBlockedPlugin(), BlockedManifest, BlockedProvenance),
            ],
            trustPolicy,
            new GraphEditorPluginDiscoveryOptions
            {
                ManifestSources =
                [
                    new DemoManifestSource(
                    [
                        new GraphEditorPluginManifestSourceCandidate(
                            "demo.manifest.allowed",
                            assemblyPath,
                            AllowedManifest,
                            typeof(DemoAllowedPlugin).FullName,
                            AllowedProvenance),
                        new GraphEditorPluginManifestSourceCandidate(
                            "demo.manifest.blocked",
                            assemblyPath,
                            BlockedManifest,
                            typeof(DemoBlockedPlugin).FullName,
                            BlockedProvenance),
                    ]),
                ],
                TrustPolicy = trustPolicy,
            });
    }

    internal sealed record DemoPluginShowcaseConfiguration(
        IReadOnlyList<GraphEditorPluginRegistration> Registrations,
        DemoPluginAllowlistTrustPolicy TrustPolicy,
        GraphEditorPluginDiscoveryOptions DiscoveryOptions);

    private sealed class DemoManifestSource(IReadOnlyList<GraphEditorPluginManifestSourceCandidate> candidates) : IGraphEditorPluginManifestSource
    {
        public IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates()
            => candidates;
    }

    private sealed class DemoAllowedPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            "aster.demo.plugin.allowed",
            "Demo Trusted Plugin",
            "Public alpha showcase plugin.",
            "0.2.0-alpha.3");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddNodeDefinitionProvider(new DemoPluginNodeDefinitionProvider(
                ShowcaseNodeDefinitionId,
                "Plugin Relay",
                "Trusted plugin node surfaced by the demo trust showcase.",
                "#6AD5C4"));
            builder.AddContextMenuAugmentor(new DemoPluginContextMenuAugmentor());
            builder.AddNodePresentationProvider(new DemoPluginNodePresentationProvider());
            builder.AddLocalizationProvider(new DemoPluginLocalizationProvider());
        }
    }

    private sealed class DemoBlockedPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            "aster.demo.plugin.blocked",
            "Demo Blocked Plugin",
            "Blocked by the demo trust policy.",
            "0.2.0-alpha.3");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new DemoPluginNodeDefinitionProvider(
                BlockedShowcaseNodeDefinitionId,
                "Blocked Relay",
                "Blocked plugin node that becomes available only after allowlist approval.",
                "#FF8A5B"));
        }
    }

    private sealed class DemoPluginNodeDefinitionProvider(
        NodeDefinitionId definitionId,
        string displayName,
        string description,
        string accentHex) : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            =>
            [
                new NodeDefinition(
                    definitionId,
                    displayName,
                    "Demo Plugins",
                    "Trusted",
                    [new PortDefinition("input", "Input", new PortTypeId("float"), "#F3B36B")],
                    [new PortDefinition("output", "Output", new PortTypeId("float"), "#6AD5C4")],
                    description: description,
                    accentHex: accentHex),
            ];
    }

    private sealed class DemoPluginContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return
            [
                .. context.StockItems,
                new GraphEditorMenuItemDescriptorSnapshot(
                    "demo.plugin.menu",
                    "Trusted Plugin Contribution",
                    iconKey: "plugin",
                    isEnabled: false),
            ];
        }
    }

    private sealed class DemoPluginNodePresentationProvider : IGraphEditorPluginNodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Node.DefinitionId == ShowcaseNodeDefinitionId
                ? new NodePresentationState(
                    SubtitleOverride: "Plugin Surface",
                    TopRightBadges:
                    [
                        new NodeAdornmentDescriptor("Trusted", "#6AD5C4"),
                    ])
                : NodePresentationState.Empty;
        }
    }

    private sealed class DemoPluginLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.menu.canvas.addNode"
                ? "Plugin Add Node"
                : fallback;
    }
}
