using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls;
using AsterGraph.ConsumerSample;
using Xunit;

namespace AsterGraph.ConsumerSample.Tests;

public sealed class ConsumerSampleProofTests
{
    [Fact]
    public void ConsumerSampleHost_CombinesHostNodesPluginContributionsAndParameterEditing()
    {
        using var host = ConsumerSampleHost.Create();

        var initialSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.True(host.PluginLoadSnapshots.Count > 0);
        Assert.Contains(host.PluginLoadSnapshots, snapshot => snapshot.Descriptor?.Id == "consumer.sample.audit-plugin");

        host.AddHostReviewNode();
        host.AddPluginAuditNode();

        var updatedSnapshot = host.Session.Queries.CreateDocumentSnapshot();
        Assert.Equal(initialSnapshot.Nodes.Count + 2, updatedSnapshot.Nodes.Count);

        var reviewNodeId = host.GetFirstReviewNodeId();
        host.Session.Commands.SetSelection([reviewNodeId], reviewNodeId, updateStatus: false);

        var parameterSnapshots = host.Session.Queries.GetSelectedNodeParameterSnapshots();
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "status");
        Assert.Contains(parameterSnapshots, snapshot => snapshot.Definition.Key == "owner");

        Assert.True(host.ApproveSelection());

        var approvedStatus = host.Session.Queries.GetSelectedNodeParameterSnapshots()
            .Single(snapshot => snapshot.Definition.Key == "status");
        Assert.Equal("approved", approvedStatus.CurrentValue);
    }

    [Fact]
    public void ConsumerSampleProof_Run_EmitsGreenMarkers()
    {
        var result = ConsumerSampleProof.Run();

        Assert.True(result.IsOk);
        Assert.True(result.HostMenuActionOk);
        Assert.True(result.PluginContributionOk);
        Assert.True(result.ParameterEditingOk);
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_RendersHostedEditorAndIntegrationPanels()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();

            Assert.NotNull(FindNamed<Menu>(window, "PART_MainMenu"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddReviewNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_AddPluginNodeButton"));
            Assert.NotNull(FindNamed<Button>(window, "PART_ApproveSelectionButton"));
            Assert.NotNull(FindNamed<GraphEditorView>(window, "PART_EditorView"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_ParameterItems"));
            Assert.NotNull(FindNamed<ItemsControl>(window, "PART_PluginSnapshotItems"));
            Assert.NotNull(FindNamed<TextBlock>(window, "PART_TrustBoundaryText"));
        }
        finally
        {
            window.Close();
        }
    }

    private static T? FindNamed<T>(Window window, string name)
        where T : Control
        => window.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
}
