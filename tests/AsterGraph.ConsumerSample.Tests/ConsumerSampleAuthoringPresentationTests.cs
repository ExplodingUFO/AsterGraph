using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AsterGraph.ConsumerSample;
using Xunit;

namespace AsterGraph.ConsumerSample.Tests;

public sealed class ConsumerSampleAuthoringPresentationTests
{
    [AvaloniaFact]
    public void AuthoringSurfaceRecipe_CreatePresentationOptions_ProvidesCustomNodeAndEditorSeams()
    {
        var options = ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions();

        Assert.NotNull(options.NodeVisualPresenter);
        Assert.NotNull(options.NodeParameterEditorRegistry);
    }

    [AvaloniaFact]
    public void ConsumerSampleHost_ProjectsTemplateKeysAndMultipleHandles_ForAuthoringRecipe()
    {
        using var host = ConsumerSampleHost.Create();

        var reviewNodeId = host.GetFirstReviewNodeId();
        var reviewNode = host.Session.Queries.CreateDocumentSnapshot()
            .Nodes
            .Single(node => node.Id == reviewNodeId);

        Assert.True(reviewNode.Outputs.Count >= 2);
        Assert.True(reviewNode.Inputs.Count >= 2);

        var parameterSnapshots = host.GetSelectedParameterSnapshots();
        var statusSnapshot = parameterSnapshots.Single(snapshot => snapshot.Definition.Key == "status");
        var ownerSnapshot = parameterSnapshots.Single(snapshot => snapshot.Definition.Key == "owner");

        Assert.Equal(ConsumerSampleAuthoringSurfaceRecipe.StatusTemplateKey, statusSnapshot.Definition.TemplateKey);
        Assert.Equal(ConsumerSampleAuthoringSurfaceRecipe.OwnerTemplateKey, ownerSnapshot.Definition.TemplateKey);
    }

    [AvaloniaFact]
    public void ConsumerSampleWindow_RendersAuthoringRecipeChrome_AndToolbarResizeMutatesThroughSession()
    {
        using var host = ConsumerSampleHost.Create();
        var window = ConsumerSampleWindowFactory.Create(host);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

            var reviewNodeId = host.GetFirstReviewNodeId();
            var widenButton = FindNamed<Button>(window, $"PART_RecipeToolbarWiden_{reviewNodeId}");
            var edgeOverlay = FindNamed<Canvas>(window, "PART_AuthoringEdgeOverlay");
            var edgeBadge = window.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(control => control.Name?.StartsWith("PART_AuthoringEdgeBadge_", StringComparison.Ordinal) == true);
            var edgeText = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(control => control.Name?.StartsWith("PART_AuthoringEdgeText_", StringComparison.Ordinal) == true);

            Assert.NotNull(widenButton);
            Assert.NotNull(edgeOverlay);
            Assert.NotNull(edgeBadge);
            Assert.NotNull(edgeText);

            var initialWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .Size.Width;

            widenButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

            var widenedWidth = host.Session.Queries.GetNodeSurfaceSnapshots()
                .Single(snapshot => snapshot.NodeId == reviewNodeId)
                .Size.Width;

            Assert.True(widenedWidth > initialWidth);

            Assert.True(host.Session.Commands.TrySetConnectionLabel("consumer-sample-connection-001", "Resized Review Flow", updateStatus: false));
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

            edgeText = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(control => control.Name?.StartsWith("PART_AuthoringEdgeText_", StringComparison.Ordinal) == true);

            Assert.NotNull(edgeText);
            Assert.Contains("Resized Review Flow", edgeText!.Text, StringComparison.Ordinal);
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
