using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;

namespace AsterGraph.ConsumerSample;

public sealed record ConsumerSampleProofResult(
    bool HostMenuActionOk,
    bool PluginContributionOk,
    bool ParameterEditingOk,
    bool WindowCompositionOk)
{
    public bool IsOk => HostMenuActionOk && PluginContributionOk && ParameterEditingOk && WindowCompositionOk;
}

public static class ConsumerSampleProof
{
    public static ConsumerSampleProofResult Run()
    {
        ConsumerSampleHeadlessEnvironment.EnsureInitialized();

        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        window.Show();

        var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var hostMenuActionOk = host.AddHostReviewNode()
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == initialSnapshot.Nodes.Count + 1;

        var afterHostSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        var pluginContributionOk = host.HasPluginNodeDefinition()
            && host.HasPluginMenuContribution()
            && host.AddPluginAuditNode()
            && host.PluginLoadSnapshots.Any(snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin")
            && host.Session.Queries.CreateDocumentSnapshot().Nodes.Count == afterHostSnapshot.Nodes.Count + 1;

        host.SelectNode(host.GetFirstReviewNodeId());
        var parameterEditingOk = host.TrySetSelectedOwner("release-owner")
            && host.ApproveSelection()
            && host.GetSelectedParameterSnapshots().Any(snapshot =>
                snapshot.Definition.Key == "status"
                && string.Equals(snapshot.CurrentValue?.ToString(), "approved", StringComparison.Ordinal))
            && host.GetSelectedParameterSnapshots().Any(snapshot =>
                snapshot.Definition.Key == "owner"
                && string.Equals(snapshot.CurrentValue?.ToString(), "release-owner", StringComparison.Ordinal));

        var windowCompositionOk =
            FindNamed<Menu>(window, "PART_MainMenu") is not null
            && FindNamed<Button>(window, "PART_AddReviewNodeButton") is not null
            && FindNamed<Button>(window, "PART_AddPluginNodeButton") is not null
            && FindNamed<Button>(window, "PART_ApproveSelectionButton") is not null
            && FindNamed<TextBlock>(window, "PART_TrustBoundaryText") is not null;

        window.Close();

        return new ConsumerSampleProofResult(
            HostMenuActionOk: hostMenuActionOk,
            PluginContributionOk: pluginContributionOk,
            ParameterEditingOk: parameterEditingOk,
            WindowCompositionOk: windowCompositionOk);
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
}

internal static class ConsumerSampleHeadlessEnvironment
{
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        AppBuilder.Configure<ConsumerSampleProofApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();
        _initialized = true;
    }
}

internal sealed class ConsumerSampleProofApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}
